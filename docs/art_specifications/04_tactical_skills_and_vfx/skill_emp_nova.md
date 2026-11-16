# Sprite Specification: EMP Nova (`skill_emp_nova`)

> **Category:** Tactical Skills — Active Skill 3 Electromagnetic Blast  
> **Target Audience:** 2D FX Artist & Vector Graphic Specialist  
> **In-Game Role:** 360-degree expanding shockwave ring stunning & disarming enemies  

---

## 1. Concept & Visual Identity

The **EMP Nova** releases an omnidirectional electromagnetic shockwave covering a **9.5-unit radius** centered on the player's ship. It deals direct damage, disarms enemy weapons, and freezes hostile thrusters for 3.0 seconds.

* **Color Palette:** Pure Neon Cyan (`#00F0FF`) with White Core (`#FFFFFF`) and crackling electrical blue arcs.
* **Effective Radius:** Expands from 0.3 units to **9.5 units** in 0.45 seconds.

---

## 2. Visual Layer Structure (`EMPShockwaveVisual`)

```
               /===============================\  (Outer Expanding Vector Ring)
              /      [ INNER RADIANT PULSE ]    \
             |       (TRANSLUCENT CYAN GLOW)     |
             |                                   |
              \      [ DISARMING LIGHTNING ]    /
               \===============================/
```

1. **Expanding Perimeter Ring:** Vector-smooth glowing neon cyan ring rendered via `LineRenderer` (48 circular segments) with fading line width and alpha.
2. **Inner Radiant Energy Disk:** Expanding soft circular glow disk (`ProceduralSpriteHelper.GetCircleSprite(128)`) filling the shockwave interior.
3. **Target Stun Arcs:** Hit enemies flash bright cyan on impact and display erratic electrical static arcs jumping across their hulls for 3 seconds.
4. **Ship Trigger Flash:** The player's ship flashes intense electric cyan on release.

---

## 3. Required Visual Deliverables

1. `vfx_emp_inner_disk.png` (128x128 px, 32-bit RGBA) — Radial glowing energy gradient.
2. `vfx_emp_lightning_arc.png` (48x24 px, 4-frame flipbook) — Crackling electrical lightning sprite attached to stunned ships.
3. `icon_skill_emp.png` (64x64 px, 32-bit RGBA) — Pulsing electromagnetic sphere vector icon for HUD.
