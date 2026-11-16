# Sprite Specification: Shadow Phantom (`ship_player_phantom`)

> **Category:** Player Vessels — Tier 3 Stealth Recon Phantom  
> **Target Audience:** 2D Vehicle Artist & Sprite Animator  
> **In-Game Role:** Electronic warfare and cloaking stealth strike craft  

---

## 1. Concept & Visual Identity

The **Shadow Phantom** is an evolution from the Miner/Scout line into a classified electronic warfare craft. Inspired by stealth aircraft (F-117 and B-2 Spirit), it features razor-faceted diamond geometry, radar-absorbent materials, and completely recessed internal weapons.

* **Personality:** Ghostly, mysterious, technological assassin, evasive.
* **Canvas Size:** **80 x 80 pixels** (2.5 x 2.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Default Heading:** Facing **Right (+X)** in source file.

---

## 2. Visual Anatomy & Blueprint

```
            /|  (Port Diamond Wing Facet)
           / |
  =======>/  |  [ FLUSH RECESSED CANOPY ]   === (INTERNAL AEROSPIKE SLOT EXHAUST)
  (Faceted\  |                              === (VIOLET SUB-SURFACE GLOW)
   Prow)   \ |
            \|  (Starboard Diamond Wing Facet)
```

### Exterior Details
* **Chassis Armor:** Matte carbon-fiber radar-absorbent coating (`#020617` to `#0B0F19`) with sharp specular facet highlights.
* **Faceted Geometry:** Zero 90-degree angles; all plates join at sharp 45-degree and 60-degree stealth angles.
* **Energy Traces:** Sub-surface glowing ultraviolet/violet veins (`#8B5CF6`) that pulse only when maneuvering.
* **Recessed Engines:** Internal aerospike slot exhaust shielded from thermal and visual detection.

---

## 3. Hardware Hardpoints & Socket Coordinates

| Hardware Component | Local Coordinates (X, Y) | Visual Function |
| :--- | :--- | :--- |
| **Port Internal Weapon Bay** | `(X: +0.32, Y: +0.35)` | Recessed weapon slot (opens doors upon firing). |
| **Starboard Internal Bay** | `(X: +0.32, Y: -0.35)` | Recessed weapon slot (opens doors upon firing). |
| **Linear Slot Exhaust** | `(X: -0.62, Y: 0.0)` | Linear ultraviolet heat dissipation slot. |

---

## 4. Required Visual States & Deliverables

1. `ship_player_phantom_diffuse.png` (80x80 px, 32-bit RGBA) — Neutral flight state.
2. `ship_player_phantom_emissive.png` (80x80 px, 24-bit RGB) — Faint violet internal grid lines and linear exhaust slot.
3. `ship_player_phantom_damaged.png` (80x80 px, 32-bit RGBA) — Stealth coating peeling off, exposing glowing raw electronics beneath.
4. `ship_player_phantom_cloak.png` (80x80 px, 32-bit RGBA) — 4-frame holographic glitch transition sprite for cloaking/decloaking.
5. `ship_player_phantom_icon.png` (128x128 px, 32-bit RGBA) — Stealth diamond chevron badge for Hangar UI.
