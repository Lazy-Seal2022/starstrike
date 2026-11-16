# Sprite Specification: Reactive Barrier (`skill_barrier`)

> **Category:** Tactical Skills — Passive Deflector Shield  
> **Target Audience:** 2D FX Artist & Shader Specialist  
> **In-Game Role:** Hexagonal deflector bubble popping up on heavy shield damage  

---

## 1. Concept & Visual Identity

The **Reactive Barrier (Deflector Surge)** is an autonomous defensive shield that deploys when the ship takes heavy damage. It manifests as a translucent hexagonal honeycomb energy sphere enclosing the vessel.

* **Radius:** **1.6 Unity Units** (enclosing all light and medium ship hulls).
* **Color Palette:** Saturated Cobalt / Royal Azure (`#3B82F6`) with glowing white-blue cell edges (`#93C5FD`).

---

## 2. Visual Blueprint & Shader Interaction

```
          /\_/\_/\_/\
         /           \  (HEXAGONAL HONEYCOMB WIREFRAME CELL MESH)
        |  [ SHIP ]   |
         \           /  (CIRCULAR BULLET IMPACT RIPPLES)
          \_/\_/\_/\_/
```

* **Honeycomb Mesh:** Semi-transparent interior cells with illuminated wireframe hex borders.
* **Impact Response:** When incoming enemy projectiles strike the barrier, localized circular shockwave ripples propagate outward across the hexagonal grid.
* **Collapse Flash:** When the barrier expires or absorbs its damage quota, it shatters outward into fading triangular crystal shards.

---

## 3. Required Visual Deliverables

1. `vfx_barrier_hex_mesh.png` (128x128 px, 32-bit RGBA) — Seamlessly looping hexagonal sphere projection texture.
2. `vfx_barrier_shatter_sheet.png` (256x128 px, 4-frame flipbook) — Geometric shield collapse animation.
3. `icon_skill_barrier.png` (64x64 px, 32-bit RGBA) — Crest shield with hexagonal energy pattern for HUD.
