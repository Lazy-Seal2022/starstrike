# Sprite Specification: Tachyon Continuous Beam (`proj_beam`)

> **Category:** Weapons & Arsenal — Weapon 4 Sustained Laser  
> **Target Audience:** 2D FX Artist & Shader Specialist  
> **In-Game Role:** Non-stop coherent laser cutting through armor and asteroids  

---

## 1. Concept & Visual Identity

The **Tachyon Beam** is a continuous, focused stream of high-energy coherent radiation. Rather than discrete projectiles, it manifests as a sustained laser line connecting the ship's prow to the impact target.

* **Color Palette:** Searing Ultra-Violet / Electric Violet (`#8B5CF6`) with a blazing white core (`#FFFFFF`).
* **Visual Profile:** A 3-piece modular texture system seamlessly stretched by Unity's `LineRenderer`.

---

## 2. 3-Piece Modular Texture Architecture

```
[ 1. EMITTER ORIGIN ]       [ 2. SEAMLESS TILING BODY ]       [ 3. CONTACT IMPACT CORE ]
    (Lens Flare)        =====================================       (Radial Plasma Splash)
 (Ship Nose Anchor)       (Undulating Electrical Edges)          (Violent Spark Shower)
```

1. **Beam Emitter Flare (32x32 px):** Blinding spherical lens flare with horizontal anamorphic streak anchored to the ship's nose.
2. **Beam Body (32x64 px Seamless Tiling):** Coherent energy cylinder with undulating electric arcs along the edges.
3. **Impact Contact Core (48x48 px):** Expanding contact point with violent lateral spark splashes and surface scorching.

---

## 3. Required Visual Deliverables

1. `vfx_beam_emitter_flare.png` (32x32 px, 32-bit RGBA) — Violet lens flare at ship origin.
2. `vfx_beam_body_tile.png` (32x64 px, 32-bit RGBA) — Seamlessly vertically tiling beam core.
3. `vfx_beam_impact_splash.png` (144x48 px, 3-frame flipbook) — Crackling plasma impact contact point.
