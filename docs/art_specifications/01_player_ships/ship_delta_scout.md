# Sprite Specification: Delta Scout (`ship_player_scout`)

> **Category:** Player Vessels — Tier 1 Starter Hull  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** Light, highly agile reconnaissance starter craft  

---

## 1. Concept & Visual Identity

The **Delta Scout** is the iconic starter vessel in StarStrike. It communicates nimbleness, precision, and reliable aerospace craftsmanship. Its silhouette is a sleek, razor-sharp equilateral delta wing with a forward-pointing dart cockpit.

* **Personality:** Nimble, clean, responsive, agile.
* **Canvas Size:** **64 x 64 pixels** (2.0 x 2.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
                     /|  (Port Wingtip Fin)
                    / |
   (Nose Muzzle)   /  |=================== (Cyan Racing Stripe)
  ================>   |  [ COCKPIT GLASS ]  |===== (Single Stern Thruster)
                   \  |=================== (Cyan Racing Stripe)
                    \ |
                     \|  (Starboard Wingtip Fin)
```

### Exterior Details
* **Chassis Armor:** Slate gunmetal plating (`#1E293B`) with beveled titanium leading edges (`#334155`).
* **Cockpit Canopy:** Sleek elongated teardrop glass glowing radiant cyan (`#00F0FF`).
* **Insignia / Stripes:** Two crisp cyan racing stripes flanking the cockpit along the dorsal ridge.
* **Wingtip Stabilizers:** Micro vertical winglets with 1px white navigation strobe lights.

---

## 3. Hardware Hardpoints & Socket Coordinates

These coordinates represent local offsets from sprite center `(0, 0)`:

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Primary Weapon Hardpoint** | `(X: +0.65, Y: 0.0)` | **1 Central Nose Hardpoint**. Spawns single laser bolt. |
| **Engine Thruster Socket** | `(X: -0.65, Y: 0.0)` | **1 Central Stern Nozzle**. Anchor for thruster flame particle FX. |
| **Cockpit Center** | `(X: +0.10, Y: 0.0)` | Glass reflection / pilot helmet silhouette. |

---

## 4. Required Visual States & Deliverables

The artist must export the following PNG assets:

1. `ship_player_scout_diffuse.png` (64x64 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_scout_emissive.png` (64x64 px, 24-bit RGB) — Black mask with glowing cyan cockpit, thruster nozzle ring, and wingtip strobes.
3. `ship_player_scout_damaged.png` (64x64 px, 32-bit RGBA) — 50% health variant showing armor scorched by laser fire, cracked canopy glass, and exposed wiring at the port wing.
4. `ship_player_scout_bank.png` (320x64 px spritesheet) — 5-frame banking animation:
   - Frame 1: Hard Port Bank (-30° tilt)
   - Frame 2: Soft Port Bank (-15° tilt)
   - Frame 3: Neutral Flat Flight (0°)
   - Frame 4: Soft Starboard Bank (+15° tilt)
   - Frame 5: Hard Starboard Bank (+30° tilt)
5. `ship_player_scout_icon.png` (128x128 px, 32-bit RGBA) — High-res orthographic badge with hexagonal frame for Hangar UI.
