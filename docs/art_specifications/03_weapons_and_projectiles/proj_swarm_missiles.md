# Sprite Specification: Swarm Missiles (`proj_missile`)

> **Category:** Weapons & Arsenal — Weapon 3 Autonomous Seeker Missiles  
> **Target Audience:** 2D Weapon Artist & Particle Specialist  
> **In-Game Role:** Agile homing micro-rockets tracking hostile targets  

---

## 1. Concept & Visual Identity

**Swarm Missiles** are agile micro-ordinance launched in pairs. They ignite their rocket motors half a second after release and violently bank toward their targets, leaving dramatic corkscrewing smoke ribbons behind them.

* **Canvas Size:** **16 x 32 pixels** (0.5 x 1.0 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Orientation:** Traveling along **+X axis** (or +Y if vertical).

---

## 2. Visual Blueprint & Anatomy

```
               /|  (Stabilizer Fin)
  [ ROCKET ]  / |
  [ MOTOR  ] (  |================= (HAZARD OLIVE BODY) ====> [ AMBER SEEKER NOSE ]
  [ NOZZLE ]  \ |
               \|  (Stabilizer Fin)
```

* **Missile Body:** Matte olive drab / gunmetal composite casing (`#475569`).
* **Guidance Seeker Optic:** Glowing hazard amber nosecone (`#F59E0B`).
* **Fins:** 4 cruciform titanium stabilizing fins.

---

## 3. Accompanying Particle & UI FX

1. **Rocket Flame Plume:** Needle-sharp, intense white-orange propulsion plume anchored to the tail nozzle.
2. **Turbulent Smoke Ribbon:** Volumetric white/gray particle trail twisting behind the missile as it tracks targets.
3. **Target Lock Reticle (UI):** Rotating square or diamond HUD brackets locking onto the targeted enemy ship.
4. **Impact Blast:** Volumetric fiery detonation with expanding shockwave ring.

---

## 4. Required Visual Deliverables

1. `proj_missile_homing.png` (16x32 px, 32-bit RGBA) — Seeker rocket casing.
2. `proj_missile_emissive.png` (16x32 px, 24-bit RGB) — Glowing amber seeker optic and rocket exhaust core.
3. `ui_hud_missile_lock.png` (32x32 px, 32-bit RGBA) — Target acquisition diamond reticle.
4. `vfx_missile_explosion.png` (128x128 px, 6-frame flipbook) — Fiery ordnance detonation.
