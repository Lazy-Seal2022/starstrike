# Sprite Specification: Pulse Laser Projectile (`proj_laser`)

> **Category:** Weapons & Arsenal — Weapon 1 Primary Laser Bolt  
> **Target Audience:** 2D FX Artist & Weapon Illustrator  
> **In-Game Role:** High-velocity energy bolts (Player Cyan vs. Hostile Crimson)  

---

## 1. Concept & Visual Identity

The **Pulse Laser** is the primary dogfighting weapon. It fires concentrated plasma bolts traveling at high velocity. The bolt is aerodynamic and luminous, leaving a brief retinal after-burn trail across space.

* **Canvas Size:** **16 x 32 pixels** (0.5 x 1.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Orientation:** Oriented lengthwise, traveling along the **+X axis** (or +Y if vertical bolt).

---

## 2. Visual Blueprint & Color Variations

```
  [ TAPERED GLOWING TAIL ]  ===========> (PURE WHITE SUPERHEATED HEAD)
```

### Color Matrix: Player vs. Enemy
| Component | Player Laser (Friend) | Enemy Laser (Hostile) |
| :--- | :--- | :--- |
| **Hot Plasma Core** | Pure White (`#FFFFFF`) | Hot Coral White (`#FFE4E8`) |
| **Outer Energy Halo** | Electric Cyan (`#00F0FF`) | Neon Crimson (`#FF0055`) |
| **Fading Tail Ribbon** | Deep Azure (`#0284C7`) | Deep Blood Red (`#990022`) |

---

## 3. Accompanying Particle FX Requirements

1. **Muzzle Flash (32x32 px):** 2-frame circular lens flare erupting at the ship's weapon hardpoints upon trigger pull.
2. **Impact Spark Burst:** 6–8 tiny radiant diamond particles bouncing outward upon impacting shields or armor.

---

## 4. Required Visual Deliverables

1. `proj_laser_player.png` (16x32 px, 32-bit RGBA) — Player cyan bolt.
2. `proj_laser_enemy.png` (16x32 px, 32-bit RGBA) — Enemy crimson bolt.
3. `vfx_muzzle_flash_player.png` (64x32 px, 2-frame flipbook) — Cyan muzzle blast.
4. `vfx_muzzle_flash_enemy.png` (64x32 px, 2-frame flipbook) — Red muzzle blast.
