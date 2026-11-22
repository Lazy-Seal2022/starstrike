import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter
from scipy.ndimage import gaussian_filter

def create_radial_gradient(size, inner_radius, outer_radius):
    y, x = np.ogrid[:size, :size]
    center = size / 2.0
    dist = np.sqrt((x - center)**2 + (y - center)**2)
    t = np.clip((dist - inner_radius) / max(1e-5, (outer_radius - inner_radius)), 0.0, 1.0)
    return dist, t

def draw_hexagon(draw, center, radius, outline, width=1, fill=None):
    cx, cy = center
    points = []
    for i in range(6):
        angle = math.pi / 3.0 * i - math.pi / 6.0
        px = cx + radius * math.cos(angle)
        py = cy + radius * math.sin(angle)
        points.append((px, py))
    if fill:
        draw.polygon(points, fill=fill)
    if outline:
        draw.line(points + [points[0]], fill=outline, width=width)

# ==============================================================================
# 1. ARCADE EXPLOSION (8 FRAMES, 256x256)
# ==============================================================================
def generate_explosion_frames(output_dir):
    os.makedirs(output_dir, exist_ok=True)
    size = 256
    center = size / 2.0
    frames = []

    rng = np.random.RandomState(1337)
    
    # 8 distinct animation stages
    # progress from 0.0 to 1.0
    progress_steps = [0.06, 0.16, 0.30, 0.46, 0.62, 0.76, 0.88, 0.98]
    
    # Pre-generate persistent seed directions for flame lobes
    num_lobes = 18
    lobe_angles = np.linspace(0, 2 * math.pi, num_lobes, endpoint=False) + rng.uniform(-0.15, 0.15, num_lobes)
    lobe_speed = rng.uniform(0.7, 1.2, num_lobes)
    lobe_sizes = rng.uniform(0.6, 1.1, num_lobes)

    for f_idx, progress in enumerate(progress_steps):
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        
        # 1. Shockwave Ring (expands fastest, high speed)
        if 0.05 < progress < 0.75:
            ring_p = (progress - 0.05) / 0.70
            ring_rad = ring_p * (size * 0.46)
            ring_alpha = int(255 * (1.0 - ring_p)**1.4 * 0.9)
            if ring_alpha > 0 and ring_rad > 8:
                ring_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
                rdraw = ImageDraw.Draw(ring_img)
                # Outer electric orange ring
                rdraw.ellipse([center - ring_rad, center - ring_rad, center + ring_rad, center + ring_rad],
                              outline=(255, 120, 20, ring_alpha), width=max(2, int(6 * (1.0 - ring_p))))
                # Inner hot white-yellow rim
                if ring_p < 0.4:
                    rdraw.ellipse([center - ring_rad*0.95, center - ring_rad*0.95, center + ring_rad*0.95, center + ring_rad*0.95],
                                  outline=(255, 240, 150, int(ring_alpha * 0.7)), width=2)
                ring_img = ring_img.filter(ImageFilter.GaussianBlur(radius=1.8))
                img = Image.alpha_composite(img, ring_img)

        # 2. Outer Deep Red/Crimson & Smoke Lobes
        outer_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        odraw = ImageDraw.Draw(outer_img)
        
        # Expansion curve: fast pop out, then slower billow
        expansion = math.sin(math.pow(progress, 0.5) * math.pi * 0.5)
        current_rad = expansion * (size * 0.36)
        
        outer_alpha = int(255 * max(0.0, min(1.0, (1.0 - progress) * 1.35)))
        smoke_tint = np.clip(progress * 1.8, 0.0, 1.0)
        
        # Outer color shifts from deep crimson red (230, 20, 5) to dark smoke (50, 15, 12)
        cr = int((1.0 - smoke_tint) * 230 + smoke_tint * 55)
        cg = int((1.0 - smoke_tint) * 35 + smoke_tint * 20)
        cb = int((1.0 - smoke_tint) * 10 + smoke_tint * 18)
        
        for i in range(num_lobes):
            dist = current_rad * lobe_speed[i] * 0.88
            lx = center + math.cos(lobe_angles[i]) * dist
            ly = center + math.sin(lobe_angles[i]) * dist
            lr = (size * 0.12) * lobe_sizes[i] * (0.8 + 0.6 * expansion)
            odraw.ellipse([lx - lr, ly - lr, lx + lr, ly + lr], fill=(cr, cg, cb, outer_alpha))
            
        outer_img = outer_img.filter(ImageFilter.GaussianBlur(radius=4.5))
        img = Image.alpha_composite(img, outer_img)

        # 3. Mid Blazing Arcade Orange/Gold Layer
        if progress < 0.85:
            mid_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
            mdraw = ImageDraw.Draw(mid_img)
            mid_alpha = int(255 * (1.0 - progress * 1.1))
            mid_rad = current_rad * 0.68
            for i in range(0, num_lobes, 1):
                dist = mid_rad * lobe_speed[i] * 0.75
                lx = center + math.cos(lobe_angles[i]) * dist
                ly = center + math.sin(lobe_angles[i]) * dist
                lr = (size * 0.09) * lobe_sizes[i] * (0.8 + 0.4 * expansion)
                mdraw.ellipse([lx - lr, ly - lr, lx + lr, ly + lr], fill=(255, 95, 0, mid_alpha))
            mid_img = mid_img.filter(ImageFilter.GaussianBlur(radius=3.5))
            img = Image.alpha_composite(img, mid_img)

        # 4. Inner Intense Lemon Yellow Plasma Core
        if progress < 0.65:
            yel_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
            ydraw = ImageDraw.Draw(yel_img)
            yel_alpha = int(255 * (1.0 - progress / 0.65))
            yel_rad = current_rad * 0.40
            for i in range(0, num_lobes, 2):
                dist = yel_rad * lobe_speed[i] * 0.6
                lx = center + math.cos(lobe_angles[i]) * dist
                ly = center + math.sin(lobe_angles[i]) * dist
                lr = (size * 0.065) * (0.6 + 0.4 * expansion)
                ydraw.ellipse([lx - lr, ly - lr, lx + lr, ly + lr], fill=(255, 230, 20, yel_alpha))
            yel_img = yel_img.filter(ImageFilter.GaussianBlur(radius=2.5))
            img = Image.alpha_composite(img, yel_img)

        # 5. Central Searing White-Hot Star Flash
        if progress < 0.40:
            core_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
            cdraw = ImageDraw.Draw(core_img)
            core_alpha = int(255 * (1.0 - progress / 0.40)**1.2)
            core_r = max(4.0, (size * 0.16) * (1.0 - progress * 2.2))
            cdraw.ellipse([center - core_r, center - core_r, center + core_r, center + core_r],
                          fill=(255, 255, 255, core_alpha))
            # 4-point diffraction flash in first 2 frames
            if progress < 0.22:
                flare_len = (size * 0.42) * (1.0 - progress * 3.5)
                if flare_len > 10:
                    cdraw.line([center - flare_len, center, center + flare_len, center], fill=(255, 255, 255, core_alpha), width=3)
                    cdraw.line([center, center - flare_len, center, center + flare_len], fill=(255, 255, 255, core_alpha), width=3)
            core_img = core_img.filter(ImageFilter.GaussianBlur(radius=1.8))
            img = Image.alpha_composite(img, core_img)

        # 6. High-velocity incendiary sparks / flying embers
        if progress > 0.08:
            spark_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
            sdraw = ImageDraw.Draw(spark_img)
            num_sparks = 24
            spark_rng = np.random.RandomState(42)
            s_angles = spark_rng.uniform(0, math.pi * 2, num_sparks)
            s_vels = spark_rng.uniform(0.8, 1.4, num_sparks)
            s_sizes = spark_rng.uniform(1.5, 3.5, num_sparks)
            
            spark_decay = max(0.0, 1.0 - progress * 1.05)
            s_alpha = int(255 * spark_decay)
            
            for s in range(num_sparks):
                s_dist = progress * (size * 0.44) * s_vels[s]
                sx = center + math.cos(s_angles[s]) * s_dist
                sy = center + math.sin(s_angles[s]) * s_dist
                sr = max(1.0, s_sizes[s] * spark_decay)
                # Searing yellow-white sparks
                sdraw.ellipse([sx - sr, sy - sr, sx + sr, sy + sr], fill=(255, 240, 120, s_alpha))
                # Spark streak trail
                if progress < 0.6:
                    tail_x = sx - math.cos(s_angles[s]) * 6.0
                    tail_y = sy - math.sin(s_angles[s]) * 6.0
                    sdraw.line([tail_x, tail_y, sx, sy], fill=(255, 140, 30, int(s_alpha * 0.6)), width=1)
                    
            spark_img = spark_img.filter(ImageFilter.GaussianBlur(radius=0.8))
            img = Image.alpha_composite(img, spark_img)

        frame_path = os.path.join(output_dir, f"Explosion_frame_{f_idx}.png")
        img.save(frame_path)
        frames.append(img)
        print(f"Generated {frame_path}")

    # Build horizontal strip (2048 x 256)
    strip = Image.new('RGBA', (size * len(frames), size), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        strip.paste(frame, (i * size, 0))
    strip_path = os.path.join(output_dir, "Explosion_strip8.png")
    strip.save(strip_path)
    print(f"Saved {strip_path} ({strip.size})")
    return frames

# ==============================================================================
# 2. THRUSTER PLUMES (CRUISE & BOOST, 6 FRAMES EACH, 128x64)
# ==============================================================================
def generate_thruster_plumes(output_dir):
    os.makedirs(output_dir, exist_ok=True)
    w, h = 128, 64
    cy = h / 2.0
    # Nozzle is at left (X=8), flame shoots towards right (X=120)
    # When attached in Unity with rotation=180, it shoots backwards towards -X!

    for mode in ["Cruise", "Boost"]:
        frames = []
        is_boost = (mode == "Boost")
        
        # Colors:
        # Cruise: Electric Cyan (#00FFFF), Sky Blue (#00A2FF), Deep Neon Blue (#0040FF)
        # Boost: Searing White-Hot (#FFFFFF), Electric Gold (#FFD200), Blaze Orange (#FF3D00), Crimson (#D91000)
        
        flame_len = 105.0 if is_boost else 72.0
        max_thickness = 22.0 if is_boost else 14.0
        
        for f_idx in range(6):
            phase = (f_idx / 6.0) * math.pi * 2.0
            img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            draw = ImageDraw.Draw(img)
            
            # Layer 1: Soft Outer Corona Glow
            corona_img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            cdraw = ImageDraw.Draw(corona_img)
            c_color = (255, 60, 0, 90) if is_boost else (0, 100, 255, 90)
            
            c_pts = []
            x_steps = 24
            for s in range(x_steps + 1):
                x = 8.0 + (s / x_steps) * (flame_len * 1.15)
                nx = (x - 8.0) / (flame_len * 1.15)
                # Taper profile: starts at nozzle, peaks at 25%, tapers to needle point
                profile = math.sin(nx * math.pi) if nx < 1.0 else 0.0
                wave = math.sin(phase * 2.0 + nx * 6.0) * (2.5 if is_boost else 1.2)
                ty = (max_thickness * 1.4) * profile + wave
                c_pts.append((x, cy - ty))
            for s in range(x_steps, -1, -1):
                x = 8.0 + (s / x_steps) * (flame_len * 1.15)
                nx = (x - 8.0) / (flame_len * 1.15)
                profile = math.sin(nx * math.pi) if nx < 1.0 else 0.0
                wave = math.sin(phase * 2.0 + nx * 6.0) * (2.5 if is_boost else 1.2)
                ty = (max_thickness * 1.4) * profile + wave
                c_pts.append((x, cy + ty))
                
            cdraw.polygon(c_pts, fill=c_color)
            corona_img = corona_img.filter(ImageFilter.GaussianBlur(radius=5.0))
            img = Image.alpha_composite(img, corona_img)

            # Layer 2: Main Vibrant Flame Envelope
            main_img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            mdraw = ImageDraw.Draw(main_img)
            m_color = (255, 140, 10, 220) if is_boost else (0, 220, 255, 220)
            
            m_pts = []
            for s in range(x_steps + 1):
                x = 8.0 + (s / x_steps) * flame_len
                nx = (x - 8.0) / flame_len
                profile = math.sin(nx * math.pi * 0.95)**0.8 if nx < 1.0 else 0.0
                wave = math.sin(phase * 3.0 + nx * 8.0) * (2.0 if is_boost else 1.0)
                ty = max_thickness * profile + wave
                m_pts.append((x, cy - ty))
            for s in range(x_steps, -1, -1):
                x = 8.0 + (s / x_steps) * flame_len
                nx = (x - 8.0) / flame_len
                profile = math.sin(nx * math.pi * 0.95)**0.8 if nx < 1.0 else 0.0
                wave = math.sin(phase * 3.0 + nx * 8.0) * (2.0 if is_boost else 1.0)
                ty = max_thickness * profile + wave
                m_pts.append((x, cy + ty))
                
            mdraw.polygon(m_pts, fill=m_color)
            main_img = main_img.filter(ImageFilter.GaussianBlur(radius=2.2))
            img = Image.alpha_composite(img, main_img)

            # Layer 3: Supersonic Shock Diamonds (Diamonds inside flame jet)
            diamond_img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            ddraw = ImageDraw.Draw(diamond_img)
            
            diamond_positions = [22.0, 42.0, 64.0, 86.0] if is_boost else [18.0, 34.0, 50.0]
            for d_idx, dx in enumerate(diamond_positions):
                d_scale = 1.0 - (d_idx / len(diamond_positions)) * 0.5
                dw = (10.0 if is_boost else 7.0) * d_scale
                dh = (9.0 if is_boost else 6.0) * d_scale
                d_col = (255, 255, 230, 240) if is_boost else (200, 255, 255, 240)
                d_poly = [
                    (dx - dw, cy),
                    (dx, cy - dh),
                    (dx + dw, cy),
                    (dx, cy + dh)
                ]
                ddraw.polygon(d_poly, fill=d_col)
            diamond_img = diamond_img.filter(ImageFilter.GaussianBlur(radius=1.5))
            img = Image.alpha_composite(img, diamond_img)

            # Layer 4: Searing White-Hot Center Jet
            core_img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            cdraw = ImageDraw.Draw(core_img)
            core_len = flame_len * 0.65
            core_pts = [
                (8.0, cy - max_thickness * 0.45),
                (8.0 + core_len * 0.5, cy - max_thickness * 0.3),
                (8.0 + core_len, cy),
                (8.0 + core_len * 0.5, cy + max_thickness * 0.3),
                (8.0, cy + max_thickness * 0.45)
            ]
            cdraw.polygon(core_pts, fill=(255, 255, 255, 255))
            core_img = core_img.filter(ImageFilter.GaussianBlur(radius=1.2))
            img = Image.alpha_composite(img, core_img)

            # Layer 5: High-energy turbulent sparks trailing off
            spark_img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            sdraw = ImageDraw.Draw(spark_img)
            spk_rng = np.random.RandomState(f_idx * 17 + (100 if is_boost else 0))
            num_sparks = 10 if is_boost else 5
            for _ in range(num_sparks):
                sx = spk_rng.uniform(flame_len * 0.5, flame_len * 1.15)
                sy = cy + spk_rng.uniform(-max_thickness * 0.8, max_thickness * 0.8)
                sr = spk_rng.uniform(1.0, 2.2 if is_boost else 1.5)
                s_col = (255, 240, 160, 220) if is_boost else (180, 245, 255, 220)
                sdraw.ellipse([sx - sr, sy - sr, sx + sr, sy + sr], fill=s_col)
            spark_img = spark_img.filter(ImageFilter.GaussianBlur(radius=0.8))
            img = Image.alpha_composite(img, spark_img)

            fname = f"Thruster{mode}_frame_{f_idx}.png"
            fpath = os.path.join(output_dir, fname)
            img.save(fpath)
            frames.append(img)
            print(f"Generated {fpath}")

        # Horizontal strip (768 x 64)
        strip = Image.new('RGBA', (w * 6, h), (0, 0, 0, 0))
        for i, frame in enumerate(frames):
            strip.paste(frame, (i * w, 0))
        strip_path = os.path.join(output_dir, f"Thruster{mode}_strip6.png")
        strip.save(strip_path)
        print(f"Saved {strip_path} ({strip.size})")

# ==============================================================================
# 3. SHIELD IMPACT RIPPLE (6 FRAMES, 256x256)
# ==============================================================================
def generate_shield_ripples(output_dir):
    os.makedirs(output_dir, exist_ok=True)
    size = 256
    center = size / 2.0
    frames = []

    progress_steps = [0.08, 0.22, 0.40, 0.60, 0.78, 0.95]
    
    for f_idx, progress in enumerate(progress_steps):
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        
        # Cubic ease-out expansion
        eased = 1.0 - math.pow(1.0 - progress, 3.0)
        current_rad = 18.0 + eased * (size * 0.40)
        alpha = math.pow(1.0 - progress, 1.8)
        
        # 1. Concentric Shockwave Rings (Outer glow)
        glow_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        gdraw = ImageDraw.Draw(glow_img)
        glow_alpha = int(255 * alpha * 0.7)
        if glow_alpha > 0:
            gdraw.ellipse([center - current_rad, center - current_rad, center + current_rad, center + current_rad],
                          outline=(0, 160, 255, glow_alpha), width=int(14 * (1.0 - progress * 0.5)))
            glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=5.0))
            img = Image.alpha_composite(img, glow_img)

        # 2. Cybernetic Hexagonal Matrix Lattice
        hex_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        hdraw = ImageDraw.Draw(hex_img)
        
        # Draw hexagonal honeycomb cells within radius
        cell_r = 22.0
        h_spacing = cell_r * math.sqrt(3)
        v_spacing = cell_r * 1.5
        
        hex_alpha = int(255 * alpha * 0.9)
        if hex_alpha > 0:
            grid_lim = int(current_rad / cell_r) + 2
            for row in range(-grid_lim, grid_lim + 1):
                for col in range(-grid_lim, grid_lim + 1):
                    hx = center + col * h_spacing + (cell_r * math.sqrt(3) / 2.0 if row % 2 != 0 else 0.0)
                    hy = center + row * v_spacing
                    h_dist = math.sqrt((hx - center)**2 + (hy - center)**2)
                    
                    if h_dist <= current_rad + 8.0:
                        cell_p = 1.0 - (h_dist / (current_rad + 12.0))
                        c_alpha = int(hex_alpha * math.pow(max(0.0, cell_p), 0.7))
                        if c_alpha > 5:
                            # Hex perimeter line: electric cyan (#00FFFF)
                            draw_hexagon(hdraw, (hx, hy), cell_r * 0.94, outline=(0, 255, 255, c_alpha), width=2)
                            # Center node dot
                            hdraw.ellipse([hx - 2.5, hy - 2.5, hx + 2.5, hy + 2.5], fill=(200, 255, 255, c_alpha))
                            
            hex_img = hex_img.filter(ImageFilter.GaussianBlur(radius=1.2))
            img = Image.alpha_composite(img, hex_img)

        # 3. Micro-lightning / Arc tendrils radiating
        if 0.1 < progress < 0.7:
            arc_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
            adraw = ImageDraw.Draw(arc_img)
            arc_rng = np.random.RandomState(f_idx * 33 + 7)
            num_arcs = 12
            for a in range(num_arcs):
                base_angle = (a / num_arcs) * math.pi * 2.0 + arc_rng.uniform(-0.2, 0.2)
                pts = [(center, center)]
                steps = 6
                cur_dist = 0.0
                cur_x, cur_y = center, center
                for s in range(1, steps + 1):
                    cur_dist = (s / steps) * current_rad * arc_rng.uniform(0.7, 1.0)
                    cur_angle = base_angle + arc_rng.uniform(-0.35, 0.35)
                    cur_x = center + math.cos(cur_angle) * cur_dist
                    cur_y = center + math.sin(cur_angle) * cur_dist
                    pts.append((cur_x, cur_y))
                adraw.line(pts, fill=(220, 255, 255, int(200 * alpha)), width=2)
            arc_img = arc_img.filter(ImageFilter.GaussianBlur(radius=1.0))
            img = Image.alpha_composite(img, arc_img)

        # 4. Central Kinetic Strike Flash
        if progress < 0.5:
            core_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
            cdraw = ImageDraw.Draw(core_img)
            core_alpha = int(255 * (1.0 - progress / 0.5)**1.5)
            core_r = max(5.0, (size * 0.12) * (1.0 - progress * 1.8))
            cdraw.ellipse([center - core_r, center - core_r, center + core_r, center + core_r],
                          fill=(255, 255, 255, core_alpha))
            core_img = core_img.filter(ImageFilter.GaussianBlur(radius=2.0))
            img = Image.alpha_composite(img, core_img)

        fpath = os.path.join(output_dir, f"ShieldRipple_frame_{f_idx}.png")
        img.save(fpath)
        frames.append(img)
        print(f"Generated {fpath}")

    # Horizontal strip (1536 x 256)
    strip = Image.new('RGBA', (size * 6, size), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        strip.paste(frame, (i * size, 0))
    strip_path = os.path.join(output_dir, "ShieldRipple_strip6.png")
    strip.save(strip_path)
    print(f"Saved {strip_path} ({strip.size})")

# ==============================================================================
# 4. MUZZLE FLASH (4 FRAMES, 128x128)
# ==============================================================================
def generate_muzzle_flashes(output_dir):
    os.makedirs(output_dir, exist_ok=True)
    size = 128
    center = size / 2.0
    frames = []

    progress_steps = [0.10, 0.35, 0.65, 0.95]

    for f_idx, progress in enumerate(progress_steps):
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        alpha = math.pow(1.0 - progress, 1.4)
        
        # 1. Broad Anamorphic Corona Glow
        glow_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        gdraw = ImageDraw.Draw(glow_img)
        g_rad = (size * 0.38) * (1.0 - progress * 0.4)
        g_alpha = int(255 * alpha * 0.8)
        if g_alpha > 0:
            gdraw.ellipse([center - g_rad, center - g_rad, center + g_rad, center + g_rad],
                          fill=(0, 200, 255, g_alpha))
            glow_img = glow_img.filter(ImageFilter.GaussianBlur(radius=6.0))
            img = Image.alpha_composite(img, glow_img)

        # 2. 8-Point Anamorphic Plasma Starburst
        star_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        sdraw = ImageDraw.Draw(star_img)
        s_alpha = int(255 * alpha)
        
        # Horizontal primary flare streak (forward firing axis)
        h_len = (size * 0.46) * (1.0 - progress * 0.7)
        v_len = (size * 0.30) * (1.0 - progress * 0.8)
        diag_len = (size * 0.22) * (1.0 - progress * 0.9)
        
        # Electric cyan outer spikes
        sdraw.line([center - h_len, center, center + h_len, center], fill=(0, 255, 255, s_alpha), width=4)
        sdraw.line([center, center - v_len, center, center + v_len], fill=(0, 255, 255, s_alpha), width=4)
        sdraw.line([center - diag_len, center - diag_len, center + diag_len, center + diag_len], fill=(0, 220, 255, int(s_alpha*0.8)), width=2)
        sdraw.line([center - diag_len, center + diag_len, center + diag_len, center - diag_len], fill=(0, 220, 255, int(s_alpha*0.8)), width=2)
        
        # Searing white core spikes
        sdraw.line([center - h_len*0.65, center, center + h_len*0.65, center], fill=(255, 255, 255, s_alpha), width=2)
        sdraw.line([center, center - v_len*0.65, center, center + v_len*0.65], fill=(255, 255, 255, s_alpha), width=2)

        star_img = star_img.filter(ImageFilter.GaussianBlur(radius=1.5))
        img = Image.alpha_composite(img, star_img)

        # 3. Blinding White-Hot Core
        core_img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        cdraw = ImageDraw.Draw(core_img)
        core_r = max(2.5, (size * 0.12) * (1.0 - progress * 1.5))
        cdraw.ellipse([center - core_r, center - core_r, center + core_r, center + core_r],
                      fill=(255, 255, 255, int(255 * alpha)))
        core_img = core_img.filter(ImageFilter.GaussianBlur(radius=1.2))
        img = Image.alpha_composite(img, core_img)

        fpath = os.path.join(output_dir, f"MuzzleFlash_frame_{f_idx}.png")
        img.save(fpath)
        frames.append(img)
        print(f"Generated {fpath}")

    # Horizontal strip (512 x 128)
    strip = Image.new('RGBA', (size * 4, size), (0, 0, 0, 0))
    for i, frame in enumerate(frames):
        strip.paste(frame, (i * size, 0))
    strip_path = os.path.join(output_dir, "MuzzleFlash_strip4.png")
    strip.save(strip_path)
    print(f"Saved {strip_path} ({strip.size})")

if __name__ == "__main__":
    vfx_root = "unity/Assets/_Project/Art/Sprites/VFX"
    print("=== Generating High-Juice Visual FX Sprites ===")
    generate_explosion_frames(os.path.join(vfx_root, "Explosion"))
    generate_thruster_plumes(os.path.join(vfx_root, "ThrusterPlume"))
    generate_shield_ripples(os.path.join(vfx_root, "ShieldRipple"))
    generate_muzzle_flashes(os.path.join(vfx_root, "MuzzleFlash"))
    print("=== All High-Juice Sprites Generated! ===")
