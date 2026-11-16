# Sprite Specification: Ares Fighter (`ship_player_fighter`)

> **Category:** Player Vessels — Tier 2 Combat Strike Fighter  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** Space superiority attack craft with twin wing cannons  

---

## 1. Concept & Visual Identity

The **Ares Fighter** is evolved from the Scout as a dedicated assault dogfighter. It features aggressive forward-swept wings, visible missile rail pylons, and a reinforced dual-engine block.

* **Personality:** Aggressive, lethal, predatory, heavily armed.
* **Canvas Size:** **80 x 80 pixels** (2.5 x 2.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
           /==================== (Port Forward-Swept Wing with Cannon)
          /   [HARDPOINT 1]
         /
  ======>     [ COCKPIT ]       (Dual Engine Nacelles)
  (Nose) \                      ======== (Port Thruster)
          \   [HARDPOINT 2]     ======== (Starboard Thruster)
           \==================== (Starboard Forward-Swept Wing with Cannon)
```

### Exterior Details
* **Chassis Armor:** Heavy navy/cobalt plating (`#0F172A`) with ceramic white bevel highlights (`#64748B`).
* **Cockpit Canopy:** Angular diamond-faceted glass glowing cyan (`#00F0FF`).
* **Wing Accents:** Forward-swept wings with neon sky-blue chevron wing stripes (`#38BDF8`).
* **Radiators:** Subtle copper/orange heat dissipation vents behind the cockpit (`#EA580C`).

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Port Weapon Hardpoint** | `(X: +0.35, Y: +0.38)` | Wingtip cannon. Fires left parallel laser bolt. |
| **Starboard Weapon Hardpoint** | `(X: +0.35, Y: -0.38)` | Wingtip cannon. Fires right parallel laser bolt. |
| **Port Thruster Bell** | `(X: -0.68, Y: +0.25)` | Left engine exhaust nozzle. |
| **Starboard Thruster Bell** | `(X: -0.68, Y: -0.25)` | Right engine exhaust nozzle. |

---

## 4. Required Visual States & Deliverables

1. `ship_player_fighter_diffuse.png` (80x80 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_fighter_emissive.png` (80x80 px, 24-bit RGB) — Cyan cockpit canopy, twin thruster nozzle cores, and weapon muzzle standby lights.
3. `ship_player_fighter_damaged.png` (80x80 px, 32-bit RGBA) — Starboard wing cracked and scorched, smoke wisps, cockpit glass cracked.
4. `ship_player_fighter_bank.png` (400x80 px spritesheet) — 5-frame bank animation (Hard Left to Hard Right).
5. `ship_player_fighter_icon.png` (128x128 px, 32-bit RGBA) — High-res orthographic badge with combat crosshair badge for Hangar UI.
