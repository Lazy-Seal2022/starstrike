# Sprite Specification: Viper Interceptor (`ship_player_interceptor`)

> **Category:** Player Vessels — Tier 3 Hyper-Speed Interceptor  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** Extreme velocity hit-and-run dogfighter with razor turning  

---

## 1. Concept & Visual Identity

The **Viper Interceptor** is evolved from the Fighter line into a hyper-kinetic racing dart. Every non-essential plate of armor has been stripped away to house an oversized fusion booster engine.

* **Personality:** Supersonic, needle-sharp, electric, lethal.
* **Canvas Size:** **64 x 64 pixels** (2.0 x 2.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
                     /|  (Knife-Edge Port Fin)
                    / |
  =================>  |=== [ COCKPIT ] === [ TRI-AFTERBURNER ENGINE BLOCK ]
  (Needle Prow)     \ |                    [ === MAIN PLUME === ]
                     \|  (Knife-Edge Starboard Fin)
```

### Exterior Details
* **Chassis Armor:** Obsidian composite carbon plating (`#020617`) with brilliant cyan edge striping (`#06B6D4`).
* **Prow:** Extended, needle-sharp penetrator nose that slices through space debris.
* **Wings:** Ultra-thin double-delta wings with razor-sharp trailing ailerons.
* **Cockpit:** Low-profile slit canopy with internal HUD projection glow.

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Port Wing-Root Hardpoint** | `(X: +0.28, Y: +0.26)` | High-velocity twin pulse cannon. |
| **Starboard Wing-Root Hardpoint**| `(X: +0.28, Y: -0.26)` | High-velocity twin pulse cannon. |
| **Main Center Thruster** | `(X: -0.68, Y: 0.0)` | High-output afterburner flame socket. |
| **Micro Vectoring Thrusters**| `(X: -0.62, Y: ±0.22)`| Twin lateral maneuvering nozzles. |

---

## 4. Required Visual States & Deliverables

1. `ship_player_interceptor_diffuse.png` (64x64 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_interceptor_emissive.png` (64x64 px, 24-bit RGB) — Slit cockpit glow, wingtip lights, triple thruster cores.
3. `ship_player_interceptor_damaged.png` (64x64 px, 32-bit RGBA) — Severed starboard winglet, trail of electrical sparks.
4. `ship_player_interceptor_bank.png` (320x64 px spritesheet) — Fast, sharp 5-frame banking roll.
5. `ship_player_interceptor_icon.png` (128x128 px, 32-bit RGBA) — High-res winged dagger badge for Hangar UI.
