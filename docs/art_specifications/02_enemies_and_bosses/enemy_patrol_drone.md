# Sprite Specification: Swarm Drone (`enemy_drone`)

> **Category:** Hostile Armada — Light Swarm Combatant  
> **Target Audience:** 2D Creature/Vehicle Artist & Sprite Animator  
> **In-Game Role:** High-speed patrol and swarming harassment fighter  

---

## 1. Concept & Visual Identity

The **Swarm Drone** is an autonomous, mass-produced robotic strike fighter. It possesses no cockpit, no life-support, and no crew comforts — only thrusters, targeting optics, and forward laser barrels.

* **Personality:** Swarming, insectoid, mechanical, expendable.
* **Canvas Size:** **48 x 48 pixels** (1.5 x 1.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
            /|  (Upper Mandible Wing)
           / |
  =======>O  | === [ COMPACT CORE ] === ==== (Crimson Thruster)
  (Optic)  \ |
            \|  (Lower Mandible Wing)
```

### Exterior Details
* **Chassis Armor:** Dark charcoal matte plating (`#0F172A`) with blood-red hazard chevrons (`#FF0055`).
* **Sensors:** A single piercing cyclops optic sensor centered at the prow, glowing pure crimson with high emissive bloom.
* **Wings:** Aggressive forward-raked wasp mandibles.

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Laser Cannon Muzzle** | `(X: +0.55, Y: 0.0)` | Spawns red enemy laser projectiles. |
| **Micro Thruster Bell** | `(X: -0.55, Y: 0.0)` | Emits continuous red propulsion flame. |

---

## 4. Required Visual States & Deliverables

1. `enemy_drone_diffuse.png` (48x48 px, 32-bit RGBA) — Neutral flight state.
2. `enemy_drone_emissive.png` (48x48 px, 24-bit RGB) — Crimson cyclops optic lens and red thruster flame core.
3. `enemy_drone_stunned.png` (48x48 px, 32-bit RGBA) — Stunned state: optic unlit, crackling cyan electric arcs across the hull.
4. `enemy_drone_debris.png` (96x48 px spritesheet) — 3 distinct shattered hull shards (left wing, right wing, shattered sensor core) for explosion physics.
