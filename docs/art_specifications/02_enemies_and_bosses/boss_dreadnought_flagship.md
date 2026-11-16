# Sprite Specification: Dreadnought Flagship (`boss_dreadnought`)

> **Category:** Hostile Capital Boss — Sector Climax Encounter  
> **Target Audience:** Lead 2D Vehicle Artist & Boss Specialist  
> **In-Game Role:** Screen-filling multi-phase capital warship  

---

## 1. Concept & Visual Identity

The **Dreadnought Flagship** is a screen-commanding titan of war. Designed as a mobile fortress, it commands respect through sheer scale, bristling multi-directional weapon batteries, layered armor decking, and an exposed fusion core in its enraged state.

* **Personality:** Colossal, terrifying, overwhelming, brutalist warship.
* **Canvas Size:** **256 x 256 pixels** (8.0 x 8.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Multi-Phase Transformation Blueprint

```
Phase 1: Armored Fortress (100% - 50% HP)
                  /====================================\
                 /   [ PORT FLAK TURRET ]               \
  ==============>     [ RECESSED COMMAND BRIDGE ]        ==== (QUAD FUSION NOZZLES)
  (HEAVY PROW)   \   [ STARBOARD FLAK TURRET ]          /
                  \====================================/

Phase 2: Overheated Berserk Rage (< 50% HP)
                  /=== [ TORN ARMOR HULL / SMOKE ] ====\
                 /                                      \
  ==============>     [ MOLTEN EXPOSED FUSION CORE ]     ==== (BURNING EXHAUST)
  (SMOKING NOSE) \    [ (PULSING ORANGE/WHITE HEAT) ]   /
                  \=== [ ELECTRICAL SPARKS & DEBRIS ] ==/
```

---

## 3. Visual States & Attacks

### Phase 1: Heavy Shielded Siege (100% – 50% HP)
* **Chassis Armor:** Pristine blackened dread-hull steel (`#0B0F19`) with reinforced titanium ribbing.
* **Forward Deflector Bubble:** Massive translucent blue shield arc spanning the forward 180° quadrant.
* **Independent Rotating Turrets:** Port and Starboard turrets track the player independently, firing alternating flak salvos.

### Phase 2: Overheated Berserk Rage (< 50% HP)
* **Visual Damage:** Outer composite armor plates have sheared off; exposed conduits spew thick black smoke and orange sparks.
* **Fusion Meltdown Core:** The central dorsal hull splits open, exposing the pulsating molten orange-yellow core (`#FF5500` to `#FFFFFF`).
* **Radial Flak Barrage:** Fires spiraling 360-degree waves of fiery red ordnance.
* **Titan Beam Sweep:** The forward spinal battery charges a screen-clearing laser beam that sweeps across a 90° arc.

---

## 4. Hardware Hardpoints & Sockets

| Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Prow Spinal Laser Core** | `(X: +0.82, Y: 0.0)` | Origin for forward mega-beam. |
| **Port Flak Turret Mount** | `(X: +0.25, Y: +0.55)` | Socket for rotating port turret sprite. |
| **Starboard Turret Mount** | `(X: +0.25, Y: -0.55)` | Socket for rotating starboard turret sprite. |
| **Exposed Fusion Core** | `(X: 0.0, Y: 0.0)` | Center of the ship, revealed in Phase 2. |
| **Quad Warp Drives** | `(X: -0.80, Y: ±0.25, ±0.55)` | 4 massive capital-class engine nozzles. |

---

## 5. Required Visual Deliverables

1. `boss_dreadnought_phase1_diffuse.png` (256x256 px, 32-bit RGBA) — Pristine capital hull.
2. `boss_dreadnought_phase1_emissive.png` (256x256 px, 24-bit RGB) — Command bridge, engine nozzles, and shield emitter conduits.
3. `boss_dreadnought_phase2_diffuse.png` (256x256 px, 32-bit RGBA) — Battle-damaged hull with exposed internal circuitry.
4. `boss_dreadnought_phase2_core_loop.png` (256x256 px, 4-frame flipbook) — Pulsing molten core animation.
5. `boss_turret_sprite.png` (48x48 px, 32-bit RGBA) — Detached rotating twin-barrel flak turret.
6. `boss_dreadnought_icon.png` (128x128 px) — Boss health bar banner portrait icon.
