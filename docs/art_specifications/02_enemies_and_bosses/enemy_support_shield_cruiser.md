# Sprite Specification: Support Shield Cruiser (`enemy_support`)

> **Category:** Hostile Armada — Fleet Defense & Shield Protector  
> **Target Audience:** 2D Creature/Vehicle Artist & Sprite Animator  
> **In-Game Role:** Projects invulnerability bubbles & tether beams to allies  

---

## 1. Concept & Visual Identity

The **Support Shield Cruiser** is an armored utility platform acting as a mobile shield generator for drone swarms. It lacks heavy offensive weaponry but vastly multiplies the threat of surrounding hostiles by granting them impenetrable energy bubbles.

* **Personality:** Guardian, defensive anchor, technological nexus.
* **Canvas Size:** **80 x 80 pixels** (2.5 x 2.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
                 (Orbital Coil 1 - Rotates)
                           ( o )
                             |
       /=====================+=====================\
      /         [ HEAVY BULKHEAD ARMOR ]            \
     |  (o)     [ CENTRAL SHIELD CORE ]       (o)   |
      \ (Coil 2) [ COBALT ENERGY GRID ]     (Coil 3)/
       \===========================================/
```

### Exterior Details
* **Chassis Armor:** Heavy hexagonal station chassis in dark naval slate (`#1E293B`) with radiant azure conduits (`#2563EB`).
* **Rotating Projector Coils:** 3 detached magnetic induction coils floating and orbiting continuously around the perimeter.
* **Central Shield Dome:** Pulsing blue energy core at the center of the vessel.

---

## 3. Visual FX & Tether Mechanics

* **Tether Laser Beam:** Renders an animated electric blue tether beam connecting the cruiser's core to all shielded allies within a 12-unit radius.
* **Shield Bubble:** Protected allies are encircled by a shimmering hexagonal azure bubble (`Radius: 1.5 units`).
* **Overload / EMP Collapse:** When the cruiser is EMP-stunned or destroyed, the orbiting coils violently lose power, fall into chaotic spin, and the tether beams shatter into sparkling blue dust.

---

## 4. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Central Projector Core** | `(X: 0.0, Y: 0.0)` | Origin point for all shield tether laser beams. |
| **Orbital Path Radius** | `Radius: 0.55 units` | Orbit track for the 3 detached projector coils. |
| **Defensive Defense Turret** | `(X: +0.45, Y: 0.0)` | Light point-defense laser mount. |

---

## 5. Required Visual Deliverables

1. `enemy_support_base_diffuse.png` (80x80 px, 32-bit RGBA) — Central platform chassis without coils.
2. `enemy_support_base_emissive.png` (80x80 px, 24-bit RGB) — Azure conduit lines and central glowing core.
3. `enemy_support_coil.png` (24x24 px, 32-bit RGBA) — Detached orbital shield projector coil.
4. `enemy_support_coil_emissive.png` (24x24 px, 24-bit RGB) — Glowing magnetic induction tip of the coil.
5. `enemy_support_stunned.png` (80x80 px, 32-bit RGBA) — Platform with dead, flickering conduits and short-circuit sparks.
