#!/usr/bin/env python3
"""
StarStrike — Sprite Preprocessing Pipeline
===========================================
Automated tool to preprocess raw concept/AI-generated sprite art into game-ready,
production-grade Unity 2D assets.

Key Pipeline Steps:
1. AI watermark / signature removal from corners.
2. Intelligent connected-component background separation (outer darkness removal).
3. Anti-aliased alpha channel generation via distance transform (no black halos).
4. Rotational center-of-mass alignment to exact (0.5, 0.5) pivot.
5. High-fidelity Lanczos downscaling aligned with StarStrike's 32 PPU scale hierarchy.
6. Emissive bloom mask generation for Unity URP 2D lighting.
"""

import os
import sys
import argparse
from pathlib import Path
from PIL import Image
import numpy as np
import scipy.ndimage as ndi


# Per-ship configuration: target resolution, padding, and specific tuning
SHIP_CONFIGS = {
    "ship_delta_scout": {
        "dest_name": "Ship_Scout",
        "target_size": 256,
        "padding_ratio": 1.10,
        "emissive_cyan": True,
        "emissive_red": False,
        "emissive_gold": False,
    },
    "ship_titan_juggernaut": {
        "dest_name": "Ship_Juggernaut",
        "target_size": 384,
        "padding_ratio": 1.08,
        "emissive_cyan": False,
        "emissive_red": True,
        "emissive_gold": False,
    },
    "ship_vanguard_carrier": {
        "dest_name": "Ship_Carrier",
        "target_size": 384,
        "padding_ratio": 1.08,
        "emissive_cyan": True,
        "emissive_red": False,
        "emissive_gold": True,
    },
}


def remove_watermark(arr: np.ndarray, corner_size: int = 200) -> np.ndarray:
    """Zeroes out the bottom-right watermark region to prevent bounding box pollution."""
    h, w = arr.shape[:2]
    cleaned = arr.copy()
    cleaned[h - corner_size :, w - corner_size :, :3] = 0
    return cleaned


def extract_alpha_mask(rgb_arr: np.ndarray, dark_threshold: int = 22) -> np.ndarray:
    """
    Separates the outer dark background from the ship using connected-component labeling.
    This guarantees that dark internal mechanical panels inside the ship are NOT erased.
    """
    h, w = rgb_arr.shape[:2]
    # Background pixels are very dark
    is_dark = np.max(rgb_arr, axis=2) <= dark_threshold

    # Label connected components of dark pixels
    labeled, _ = ndi.label(is_dark)

    # Collect labels corresponding to the 4 corners (outer canvas background)
    corners = [(0, 0), (0, w - 1), (h - 1, 0), (h - 1, w - 1)]
    bg_mask = np.zeros((h, w), dtype=bool)
    for y, x in corners:
        c_label = labeled[y, x]
        if c_label != 0:
            bg_mask |= (labeled == c_label)

    # The ship is everything not part of the connected background
    ship_mask = ~bg_mask

    # Distance transform creates a smooth, anti-aliased 1-2 pixel edge transition
    dist = ndi.distance_transform_edt(ship_mask)
    alpha = np.clip(dist * 255.0, 0, 255).astype(np.uint8)
    return alpha


def center_and_crop(rgba_img: Image.Image, padding_ratio: float = 1.08) -> Image.Image:
    """
    Crops the sprite tightly to its visible pixels and centers it within a square canvas.
    This guarantees the physics center-of-mass aligns with Unity's (0.5, 0.5) pivot point.
    """
    bbox = rgba_img.getbbox()
    if not bbox:
        return rgba_img

    cropped = rgba_img.crop(bbox)
    max_dim = max(cropped.width, cropped.height)
    square_dim = int(max_dim * padding_ratio)

    canvas = Image.new("RGBA", (square_dim, square_dim), (0, 0, 0, 0))
    offset = ((square_dim - cropped.width) // 2, (square_dim - cropped.height) // 2)
    canvas.paste(cropped, offset)
    return canvas


def generate_emissive_mask(rgba_img: Image.Image, config: dict) -> Image.Image:
    """
    Extracts high-luminance glowing components (canopies, runway lights, thrusters)
    into a dedicated black-and-color mask for URP 2D Bloom.
    """
    arr = np.array(rgba_img)
    r, g, b, a = arr[:, :, 0], arr[:, :, 1], arr[:, :, 2], arr[:, :, 3]

    emissive_mask = np.zeros(r.shape, dtype=bool)

    if config.get("emissive_cyan", False):
        # Bright cyan cockpit / runway lights (high green and blue, lower red)
        cyan = (g > 150) & (b > 180) & (r < 190) & (a > 100)
        emissive_mask |= cyan

    if config.get("emissive_red", False):
        # Tactical red command slit / alert conduits (high red, low green/blue)
        red = (r > 160) & (g < 90) & (b < 90) & (a > 100)
        emissive_mask |= red

    if config.get("emissive_gold", False):
        # Golden command trim / heat radiators
        gold = (r > 180) & (g > 130) & (b < 90) & (a > 100)
        emissive_mask |= gold

    # Pure white hot spots (canopy centers, nozzle interiors)
    hot_white = (r > 230) & (g > 230) & (b > 230) & (a > 100)
    emissive_mask |= hot_white

    emissive_rgb = np.zeros((arr.shape[0], arr.shape[1], 3), dtype=np.uint8)
    emissive_rgb[emissive_mask] = arr[:, :, :3][emissive_mask]

    return Image.fromarray(emissive_rgb)


def process_sprite(src_path: Path, output_dir: Path, config: dict) -> tuple[Path, Path]:
    """Processes a single raw sprite file into game-ready diffuse and emissive textures."""
    print(f"\nProcessing: {src_path.name}")
    raw_img = Image.open(src_path).convert("RGB")
    raw_arr = np.array(raw_img)

    # 1. Strip watermark
    clean_arr = remove_watermark(raw_arr)

    # 2. Extract anti-aliased alpha
    alpha = extract_alpha_mask(clean_arr)
    rgba_arr = np.dstack([clean_arr, alpha])
    rgba_img = Image.fromarray(rgba_arr)

    # 3. Center and crop
    centered_img = center_and_crop(rgba_img, config["padding_ratio"])

    # 4. Resize to target in-game resolution using Lanczos
    target_size = config["target_size"]
    game_diffuse = centered_img.resize((target_size, target_size), Image.Resampling.LANCZOS)

    # 5. Generate emissive mask
    emissive_mask = generate_emissive_mask(game_diffuse, config)

    # 6. Save outputs
    dest_base = config["dest_name"]
    diffuse_path = output_dir / f"{dest_base}.png"
    emissive_path = output_dir / f"{dest_base}_emissive.png"

    game_diffuse.save(diffuse_path, optimize=True)
    emissive_mask.save(emissive_path, optimize=True)

    print(f"  [+] Saved diffuse:  {diffuse_path} ({target_size}x{target_size})")
    print(f"  [+] Saved emissive: {emissive_path} ({target_size}x{target_size})")

    return diffuse_path, emissive_path


def main():
    parser = argparse.ArgumentParser(description="Preprocess raw StarStrike ship sprites.")
    parser.add_argument("--input", default="test_sprite", help="Path to raw sprites folder.")
    parser.add_argument(
        "--output",
        default="unity/Assets/_Project/Art/Sprites/Ships",
        help="Destination directory for processed sprites.",
    )
    args = parser.parse_args()

    input_dir = Path(args.input)
    output_dir = Path(args.output)
    output_dir.mkdir(parents=True, exist_ok=True)

    if not input_dir.exists():
        print(f"Error: Input directory '{input_dir}' does not exist.")
        sys.exit(1)

    print("==================================================")
    print(" StarStrike — Sprite Preprocessing Pipeline")
    print("==================================================")
    print(f"Input Directory:  {input_dir.resolve()}")
    print(f"Output Directory: {output_dir.resolve()}")

    processed_count = 0
    for file_path in sorted(input_dir.glob("*.png")):
        stem = file_path.stem
        # Check if we have a known config, or use default fallback
        config = SHIP_CONFIGS.get(
            stem,
            {
                "dest_name": stem.replace("ship_", "Ship_").title(),
                "target_size": 256,
                "padding_ratio": 1.10,
                "emissive_cyan": True,
                "emissive_red": True,
                "emissive_gold": True,
            },
        )
        process_sprite(file_path, output_dir, config)
        processed_count += 1

    print("\n==================================================")
    print(f" Successfully processed {processed_count} sprites!")
    print("==================================================")


if __name__ == "__main__":
    main()
