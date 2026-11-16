# Sprite Specification: Mining Gem Crystals (`pickup_gem`)

> **Category:** Economy & Pickups — Core Mining Resource  
> **Target Audience:** 2D FX Artist & Prop Illustrator  
> **In-Game Role:** Extracted minerals used for ship evolution & upgrades  

---

## 1. Concept & Visual Identity

**Gem Crystals** are high-density crystalline energy nodules dislodged from shattered asteroids and destroyed elite ships. They drift through space with faint magnetic attraction toward the player's vessel.

* **Canvas Size:** **16 x 16 pixels** (0.5 x 0.5 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center.
* **Silhouette:** Faceted double-pyramid or bipyramidal mineral crystal.
* **Color Palette:** Radiant Emerald Green (`#10B981`) with pure white specular glints (`#FFFFFF`).

---

## 2. Visual Behavior & Animation Loop

1. **Floating Idle Loop:** 360-degree slow tumble rotation in space.
2. **Emissive Halo:** Gentle breathing pulse (1 Hz) in the emissive channel.
3. **Magnetic Pull FX:** When drawn toward the player, emits a sparkling green particle trail behind it.
4. **Collection Flash:** Disintegrates into a brief burst of 4 green spark chevrons and increments HUD counter.

---

## 3. Required Visual Deliverables

1. `pickup_gem_emerald_spin.png` (96x16 px, 6-frame flipbook) — 6-frame smooth 360-degree tumble loop.
2. `pickup_gem_emissive.png` (96x16 px, 24-bit RGB) — Pulsing green glow mask.
3. `icon_currency_gem.png` (64x64 px, 32-bit RGBA) — High-res vector crystal icon for UI and HUD counters.
