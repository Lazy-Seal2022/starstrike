# Sprite Specification: Titan Juggernaut (`ship_player_juggernaut`)

> **Category:** Player Vessels — Tier 3 Heavy Fortress Dreadnought  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** Unstoppable armored flying fortress with 3 battery cannons  

---

## 1. Concept & Visual Identity

The **Titan Juggernaut** is evolved from the Fighter line into a massive flying bastion. It bristles with heavy artillery batteries and multi-layered reactive armor, designed to plow directly through asteroid clusters and enemy formations.

* **Personality:** Monolithic, terrifying, fortified, overwhelming firepower.
* **Canvas Size:** **96 x 96 pixels** (3.0 x 3.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
           /============================== (Heavy Port Armor Sponson)
          /    [PORT BATTERY 1]
         /
  ======>      [MAIN SPINAL BATTERY 2]   [ RECESSED BRIDGE ]   ==== (QUAD PROPULSION)
  (Heavy \                                                     ==== (WALL OF FLAME)
   Prow)  \    [STARBOARD BATTERY 3]
           \============================== (Heavy Starboard Armor Sponson)
```

### Exterior Details
* **Chassis Armor:** Massive overlapping composite armor slabs in dark battleship slate (`#1E293B`) with reinforced titanium rivets.
* **Weapons:** 3 Heavy Battery Hardpoints that fire simultaneous 3-stream bullet waves.
* **Bridge:** Deeply recessed armored slit bridge to protect the crew from direct impacts.
* **Engine Wall:** A continuous stern bulkhead hosting 4 heavy plasma thruster tubes.

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Port Battery Hardpoint** | `(X: +0.32, Y: +0.45)` | Heavy broadside cannon. |
| **Center Spinal Battery** | `(X: +0.65, Y: 0.0)` | Prow heavy rail-cannon. |
| **Starboard Battery Hardpoint**| `(X: +0.32, Y: -0.45)` | Heavy broadside cannon. |
| **Quad Thruster Cluster** | `(X: -0.68, Y: ±0.15, ±0.40)` | 4 heavy engine sockets. |

---

## 4. Required Visual States & Deliverables

1. `ship_player_juggernaut_diffuse.png` (96x96 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_juggernaut_emissive.png` (96x96 px, 24-bit RGB) — Red tactical bridge lights, weapon cooling jackets, quad orange exhaust bells.
3. `ship_player_juggernaut_damaged.png` (96x96 px, 32-bit RGBA) — Heavy armor plating sheared off, exposing smoking internal structural bulkheads.
4. `ship_player_juggernaut_bank.png` (480x96 px spritesheet) — Slow, weighty 5-frame banking rotation.
5. `ship_player_juggernaut_icon.png` (128x128 px, 32-bit RGBA) — Heavy fortress shield badge for Hangar UI.
