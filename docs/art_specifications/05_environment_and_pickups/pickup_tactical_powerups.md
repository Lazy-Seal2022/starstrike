# Sprite Specification: Tactical Power-Up Items (`pickup_powerup`)

> **Category:** Economy & Pickups — In-Run Combat Power-Ups  
> **Target Audience:** 2D FX Artist & UI Illustrator  
> **In-Game Role:** Temporary tactical buffs (Weapon Overdrive, Shield Restore, Speed Boost)  

---

## 1. Concept & Visual Identity

**Power-Up Items** are encased in high-energy containment spheres floating in space. Each power-up clearly displays an internal holographic glyph that communicates its exact combat benefit.

* **Canvas Size:** **32 x 32 pixels** (1.0 x 1.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center.

---

## 2. Power-Up Types & Visual Anatomy

```
   [ 1. WEAPON OVERDRIVE ]         [ 2. SHIELD RESTORE ]         [ 3. SPEED INJECTOR ]
     Red Diamond Prism           Cobalt Blue Energy Sphere      Green Double Chevrons
    +1 Hardpoint Slot / ROF       Instant Shield Recovery        Max Speed & Acceleration
```

1. **Weapon Overdrive:**
   - Sharp red/crimson diamond prism (`#FF0055`) with high-voltage core.
   - Grants +1 extra parallel weapon hardpoint (up to 3 cannons) and fire rate boost.
2. **Shield Restore Capsule:**
   - Shimmering royal azure energy orb (`#3B82F6`) with an internal medical cross / shield crest.
3. **Speed Injector:**
   - Luminous emerald double chevrons (`#10B981`) surrounded by an aerodynamic trailing ring.

---

## 3. Required Visual Deliverables

1. `pickup_powerup_overdrive.png` (32x32 px, 32-bit RGBA) — Red diamond power-up sprite.
2. `pickup_powerup_shield.png` (32x32 px, 32-bit RGBA) — Blue shield sphere power-up sprite.
3. `pickup_powerup_speed.png` (32x32 px, 32-bit RGBA) — Green chevron boost power-up sprite.
4. `vfx_powerup_ring_loop.png` (128x32 px, 4-frame flipbook) — Translucent spinning containment ring overlay.
