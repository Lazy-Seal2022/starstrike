# Sprite Specification: Kamikaze Rammer (`enemy_kamikaze`)

> **Category:** Hostile Armada — Suicide Ramming Vessel  
> **Target Audience:** 2D Creature/Vehicle Artist & Sprite Animator  
> **In-Game Role:** High-velocity rammer detonating in a violent 4.5m blast  

---

## 1. Concept & Visual Identity

The **Kamikaze Rammer** is an unstable, rocket-propelled warhead encased in a skeletal flight frame. It exists solely to accelerate to maximum speed and detonate on impact or destruction.

* **Personality:** Volatile, unstable, terrifying, frantic.
* **Canvas Size:** **56 x 56 pixels** (1.75 x 1.75 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
            /|
           / |  [ EXPOSED WARHEAD ]     [ OVERCHARGED FUSION BOOSTER ]
  ========( O )======================== [ ======== WALL OF FLAME ===== ]
  (Spike)  \ |  [ CONTAINMENT CAGE ]
            \|
```

### Exterior Details
* **Forward Warhead:** Massive spherical plasma warhead glowing molten orange-red (`#FF3B00`), held by hydraulic containment claws.
* **Armor:** Stripped-down blackened frame with industrial yellow/black caution stripes.
* **Prow Penetrator:** Heavy titanium ramming spike protruding past the warhead.
* **Booster Engine:** Oversized cylindrical rocket engine occupying 60% of the ship's length.

---

## 3. Visual States & Dive Telegraph

1. **Cruising State:** Warhead pulses at a calm 1 Hz glow. Normal orange thruster flame.
2. **Dive Lock-On State (High Threat):**
   - The thruster flame triples in length and turns intense white-hot yellow.
   - The warhead core strobes frantically at 8 Hz with sparks venting from the containment cage.
3. **Detonation FX:** 4.5-unit radial fireball with expanding shockwave and molten metal droplets.

---

## 4. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Warhead Detonation Center** | `(X: +0.25, Y: 0.0)` | Origin point of explosion shockwave. |
| **Oversized Booster Nozzle** | `(X: -0.62, Y: 0.0)` | Anchor for violent afterburner plume. |

---

## 5. Required Visual Deliverables

1. `enemy_kamikaze_diffuse.png` (56x56 px, 32-bit RGBA) — Neutral flight state.
2. `enemy_kamikaze_emissive.png` (56x56 px, 24-bit RGB) — Molten orange-red warhead core, engine nozzle, and warning beacons.
3. `enemy_kamikaze_dive_loop.png` (224x56 px spritesheet) — 4-frame animated strobe cycle for dive attack.
4. `enemy_kamikaze_stunned.png` (56x56 px, 32-bit RGBA) — Core dimmed to dark amber with blue electrical shorts.
