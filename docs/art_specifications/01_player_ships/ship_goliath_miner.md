# Sprite Specification: Goliath Miner (`ship_player_miner`)

> **Category:** Player Vessels — Tier 2 Industrial Heavy Miner  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** Heavy armored asteroid harvester & gunboat  

---

## 1. Concept & Visual Identity

The **Goliath Miner** is the Tier 2 evolution for players focusing on asteroid mining, shield endurance, and resource gathering. It trades aerodynamic sleekness for brutal, utilitarian industrial machinery.

* **Personality:** Bulky, rugged, unyielding, industrial.
* **Canvas Size:** **80 x 80 pixels** (2.5 x 2.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
       /======================= (Reinforced Upper Prow Mandible)
      /    [MINING LASER 1]
     |
  ===|     [ INDUSTRIAL CABIN ]    [ ORE CARGO BAYS ]   ==== (Heavy Engine Block)
     |
      \    [MINING LASER 2]
       \======================= (Reinforced Lower Prow Mandible)
```

### Exterior Details
* **Chassis Armor:** Heavy cast iron and slate panels (`#334155`) with high-visibility hazard stripes (`#FACC15` & `#0F172A`).
* **Cockpit / Cabin:** High-mounted reinforced glass cupola with security roll-cage bars.
* **Prow Mandibles:** Dual hydraulic rock-crushing jaws protruding from the prow.
* **Mining Sensors:** Glowing emerald-green mineral scanner array (`#10B981`) mounted along the forward frame.

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Upper Mining Hardpoint** | `(X: +0.42, Y: +0.36)` | Heavy mining laser / cannon emitter. |
| **Lower Mining Hardpoint** | `(X: +0.42, Y: -0.36)` | Heavy mining laser / cannon emitter. |
| **Port Heavy Thruster** | `(X: -0.65, Y: +0.32)` | Cylindrical heavy propulsion bell. |
| **Starboard Heavy Thruster**| `(X: -0.65, Y: -0.32)` | Cylindrical heavy propulsion bell. |

---

## 4. Required Visual States & Deliverables

1. `ship_player_miner_diffuse.png` (80x80 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_miner_emissive.png` (80x80 px, 24-bit RGB) — Green mineral sensor lights, yellow warning beacons, and glowing orange thruster nozzles.
3. `ship_player_miner_damaged.png` (80x80 px, 32-bit RGBA) — Dented prow armor, missing hazard stripe plate, exposed hydraulic fluid stains.
4. `ship_player_miner_turn.png` (400x80 px spritesheet) — 5-frame heavy turning animation (ponderous roll).
5. `ship_player_miner_icon.png` (128x128 px, 32-bit RGBA) — High-res industrial badge with crossed pickaxes/gears for Hangar UI.
