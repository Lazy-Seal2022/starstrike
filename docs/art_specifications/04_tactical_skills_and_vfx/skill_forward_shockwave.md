# Sprite Specification: Forward Shockwave (`skill_shockwave`)

> **Category:** Tactical Skills — Active Skill 1 Displacement Wave  
> **Target Audience:** 2D FX Artist & Particle Specialist  
> **In-Game Role:** Kinetic crescent wave repelling asteroids and pushing the ship back  

---

## 1. Concept & Visual Identity

The **Forward Shockwave** is an expanding aerodynamic sonic boom fired directly forward. It repels physical objects in its path while imparting a Newtonian recoil kick to the player ship.

* **Canvas Size:** **64 x 32 pixels** (Scales up to 2.5 x 1.2 Unity Units).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Heading:** Propagates forward along the **+X axis**.

---

## 2. Visual Blueprint & Colors

```
      .---.
    /       \  ===> (HIGH-PRESSURE TRANSLUCENT LEADING EDGE)
  (           )
    \       /  ===> (VACUUM WAKE / ELECTRIC CYAN GLOW)
      '---'
```

* **Leading Edge:** High-pressure curved crescent arc glowing bright cyan (`#38BDF8`) with intense white bloom.
* **Trailing Wake:** Fading translucent blue refractive trail that parts space dust motes.
* **Dynamic Expansion:** Expands laterally as it travels across its 1.5-second lifespan before dissipating.

---

## 3. Required Visual Deliverables

1. `vfx_shockwave_crescent.png` (64x32 px, 32-bit RGBA) — High-contrast crescent compression wave.
2. `vfx_shockwave_recoil_burst.png` (48x48 px, 3-frame flipbook) — Gas exhaust burst at ship nose on discharge.
3. `icon_skill_shockwave.png` (64x64 px, 32-bit RGBA) — Vector skill icon for HUD cooldown widget.
