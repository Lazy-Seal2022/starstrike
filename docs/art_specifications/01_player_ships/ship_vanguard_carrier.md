# Sprite Specification: Vanguard Carrier (`ship_player_carrier`)

> **Category:** Player Vessels — Tier 3 Fleet Command Carrier  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** High-shield command vessel with 3 battery hardpoints and drone bays  

---

## 1. Concept & Visual Identity

The **Vanguard Carrier** evolves from the industrial Miner line into a naval command flagship. It features a dual-hull catamaran chassis with a central runway/launch bay for autonomous micro-drones.

* **Personality:** Authoritative, strategic, technological, command cruiser.
* **Canvas Size:** **96 x 96 pixels** (3.0 x 3.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
       /============================== (Port Catamaran Hull - Battery 1)
      /    [CENTRAL FLIGHT DECK]
     |     [DRONE HATCH / ANTENNAS]   (Spinal Battery 2)  ==== (Port Fusion Nacelle)
     |
      \    [CENTRAL FLIGHT DECK]                          ==== (Starboard Fusion Nacelle)
       \============================== (Starboard Catamaran Hull - Battery 3)
```

### Exterior Details
* **Chassis Armor:** Deep imperial navy blue (`#1E3A8A`) with formal gold command trim (`#F59E0B`).
* **Catamaran Architecture:** Twin side hulls connected by a central communications and flight-deck bridge.
* **Flight Deck:** Illuminated runway strip with animated cyan guide lights.
* **Communications:** Dorsal sensor spire with rotating radar sweep dome.

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Port Outrigger Battery** | `(X: +0.38, Y: +0.42)` | Left carrier sponson turret. |
| **Center Dorsal Battery** | `(X: +0.60, Y: 0.0)` | Prow command laser battery. |
| **Starboard Outrigger Battery**| `(X: +0.38, Y: -0.42)` | Right carrier sponson turret. |
| **Port Outboard Nacelle** | `(X: -0.65, Y: +0.38)` | Left main fusion drive. |
| **Starboard Outboard Nacelle**| `(X: -0.65, Y: -0.38)` | Right main fusion drive. |

---

## 4. Required Visual States & Deliverables

1. `ship_player_carrier_diffuse.png` (96x96 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_carrier_emissive.png` (96x96 px, 24-bit RGB) — Runway landing strips, bridge viewport array, fusion engine exhausts.
3. `ship_player_carrier_damaged.png` (96x96 px, 32-bit RGBA) — Breached runway deck, blackened drone launch bays, flickering antenna array.
4. `ship_player_carrier_turn.png` (480x96 px spritesheet) — Ponderous naval turning frames.
5. `ship_player_carrier_icon.png` (128x128 px, 32-bit RGBA) — Golden fleet command eagle badge for Hangar UI.
