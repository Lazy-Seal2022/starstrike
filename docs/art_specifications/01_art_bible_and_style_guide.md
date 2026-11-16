# 01 — Art Bible & Style Guide

> **Target:** All 2D Artists, Concept Illustrators & Technical Artists  
> **Aesthetic Theme:** Neo-Retro High-Contrast Space Combat / Clean Industrial Cyberpunk  

---

## 1. Aesthetic Identity & Style Pillars

StarStrike avoids muddy, hyper-textured realism in favor of **bold, sharp, and readable graphic illustration**. Every visual element must look like a deliberate piece of aerospace industrial equipment brought to life with vibrant energy glow.

```
       [ HARDWARE / CHASSIS ]                [ ENERGY / EMISSIVE ]
  Dark titanium, matte slate, ceramic      Saturated neon plasma, cyan shields,
  armor plates with panel seam lines.     crimson laser bolts, bright thrusters.
```

### Shape Language & Visual Psychology
* **Player Vessels (Precision & Command):** Clean geometric wedges, swept delta wings, balanced triangular arrows pointing forward. Communicates speed, authority, and aerodynamic control.
* **Hostile / Enemy Armada (Aggression & Menace):** Jagged angles, claw-like silhouettes, asymmetrical weapon outriggers, menacing prow mandibles. Communicates danger, instability, and predatory intent.
* **Celestial Bodies / Asteroids (Brutal Geology):** Faceted polygonal silhouettes with sharp fractures, crater notches, and exposed glowing mineral veins.
* **Technology & UI (Aerospace Telemetry):** Crisp vector borders, 45-degree angled chamfers, digital tick marks, clean monospace typography.

---

## 2. Color System & Material Palettes

### Primary Chassis Tones (Chassis & Armor Plating)
| Tone Name | Hex Code | Swatch Preview | Intended Usage |
| :--- | :--- | :--- | :--- |
| **Deep Void Black** | `#0B0F19` | Dark Background | Main hull undertones, shadow cavities |
| **Gunmetal Armor** | `#1E293B` | Dark Slate Blue | Primary ship armor plating |
| **Titanium Gray** | `#334155` | Medium Slate | Wing bevels, structural struts, hinges |
| **Ceramic Highlight** | `#64748B` | Light Slate | Edge highlights, cockpit rims, armor seam lines |
| **Pure Hull White** | `#F8FAFC` | Crisp White | Player markings, stripe accents, identification numbers |

### Emissive & Energy Palettes
| Energy Type | Primary Hex | Hot Core Hex | Intended Usage |
| :--- | :--- | :--- | :--- |
| **Player Plasma / Tech** | `#00F0FF` | `#FFFFFF` | Player lasers, thruster flames, radar HUD, shields |
| **Hostile Red / Danger** | `#FF0055` | `#FFE4E8` | Enemy lasers, kamikaze engines, hazard warning indicators |
| **Deflector Barrier** | `#3B82F6` | `#DBEAFE` | Passive reactive shields, barrier bubbles |
| **Capacitor Overcharge** | `#FACC15` | `#FEF08A` | Energy meters, overdrive power-up, gem sparkles |
| **Tachyon / Micro-Warp** | `#A855F7` | `#F3E8FF` | Blink teleport trails, sniper targeting lasers, phase rifts |
| **Mineral Crystals** | `#10B981` | `#A7F3D0` | Mined gem drops, economic reward pickups |

---

## 3. Scale, Resolution & Unit Standards

StarStrike runs on a strict **32 Pixels Per Unit (PPU)** standard. This ensures pixel grids align with physics colliders and particle simulations.

```
+---------------------------------------------------------------+
|  1.0 Unity Unit = 32 x 32 Pixels                              |
|  2.0 Unity Units (Standard Ship) = 64 x 64 Pixels             |
|  4.0 Unity Units (Heavy Cruiser/Boss) = 128 x 128 Pixels       |
|  8.0 Unity Units (Flagship Dreadnought) = 256 x 256 Pixels    |
+---------------------------------------------------------------+
```

### Canvas & Dimension Matrix
| Asset Category | In-Game Canvas Size | World Unit Size | Physics Collider |
| :--- | :--- | :--- | :--- |
| **Light Ships (Scout/Interceptor)** | 64 x 64 px | 2.0 x 2.0 units | Circle / Polygon (Radius ~0.8) |
| **Medium Ships (Fighter/Miner)** | 80 x 80 px | 2.5 x 2.5 units | Circle / Polygon (Radius ~1.0) |
| **Heavy Ships (Juggernaut/Carrier)** | 96 x 96 px | 3.0 x 3.0 units | Polygon (Radius ~1.4) |
| **Dreadnought Capital Boss** | 256 x 256 px | 8.0 x 8.0 units | Composite Multi-Colliders |
| **Small Asteroids / Gems** | 32 x 32 px | 1.0 x 1.0 units | Circle (Radius ~0.4) |
| **Medium Asteroids** | 64 x 64 px | 2.0 x 2.0 units | Circle / Polygon (Radius ~0.9) |
| **Large Asteroids** | 128 x 128 px | 4.0 x 4.0 units | Polygon (Radius ~1.8) |
| **Standard Projectiles** | 32 x 32 px (or 16x32) | 0.5 x 1.0 units | Capsule / Box Collider |

---

## 4. Layer Sorting Order & Render Hierarchy

To prevent visual z-fighting and ensure gameplay clarity at all camera zoom levels:

| Sorting Layer | Order in Layer | Target GameObjects |
| :--- | :---: | :--- |
| **Background** | 0 | Deep void cosmos gradient, space nebulae |
| **BackgroundParallax** | 10 | Distant parallax starfield, dust motes |
| **EnvironmentUnder** | 20 | Asteroid shadows, solar flare occluder silhouettes |
| **Environment** | 30 | Asteroids, planetary debris, space stations |
| **Pickups** | 40 | Gem crystals, floating weapon power-up items |
| **Enemies** | 50 | Hostile drones, snipers, rammers, enemy bosses |
| **Player** | 60 | Player ship hulls, cockpit glass, thruster plumes |
| **Projectiles** | 70 | Player & enemy laser bolts, missiles, shotgun pellets |
| **Effects (VFX)** | 80 | Shockwave rings, explosion fireballs, sparks, EMP wave |
| **HUD / UI** | 100+ | Reticle, health arcs, floating damage numbers, radar |

---

## 5. Lighting & Post-Processing Response (URP 2D)

StarStrike utilizes Unity Universal Render Pipeline 2D Lighting:
1. **Unlit Emissive Masks:** Any component with glowing energy (cockpit canopy, wingtip navigation beacons, laser muzzle ports, engine thruster exhaust) must be pure white `#FFFFFF` in the emissive channel. This triggers the **URP Bloom Volume** with a vibrant, neon halo.
2. **2D Sprite Diffuse Lighting:** Hull armor plating responds subtly to directional sector light sources (e.g. ambient solar flare light), casting soft directional shadows across the asteroid field.

---

## 6. File Naming Conventions

All exported art assets must adhere to this standardized naming convention:

```
[category]_[subcategory]_[name]_[state/variant]_[index].[ext]

Examples:
- ship_player_fighter_default.png
- ship_player_fighter_shielded.png
- ship_enemy_sniper_charging_01.png
- proj_laser_player_dual.png
- vfx_shockwave_emp_ring.png
- ui_icon_skill_emp_nova.png
- env_asteroid_large_rocky_01.png
```
