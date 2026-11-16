# Sprite Specification: Scattergun Pellets (`proj_scattergun`)

> **Category:** Weapons & Arsenal — Weapon 2 Close-Quarters Kinetic Shotgun  
> **Target Audience:** 2D FX Artist & Weapon Illustrator  
> **In-Game Role:** Conical spread of 6 molten tungsten kinetic slugs  

---

## 1. Concept & Visual Identity

The **Scattergun** is an industrial close-range weapon that fires a shotgun spread of 6 dense kinetic penetrators. Unlike laser bolts, these are physical incandescent metal slugs propelled by explosive charges.

* **Canvas Size:** **12 x 12 pixels per pellet** (0.375 x 0.375 Unity Units at 32 PPU).
* **Pivot Point:** `(0.5, 0.5)` Center of mass.
* **Fire Pattern:** Volley of 6 simultaneous pellets dispersed in a 35-degree forward cone.

---

## 2. Visual Blueprint & Colors

```
     .----.
   (  (::) )  ===> (MOLTEN TUNGSTEN CORE WITH INCANDESCENT ORANGE HEAT)
     '----'
```

* **Core Pellet:** Blazing molten tungsten core (`#FFEDD5` to `#F97316`).
* **Tracer Streak:** Short, incandescent orange-yellow motion trail (`#FDE047`).
* **Muzzle Blast:** Dramatic conical fiery burst from the prow accompanied by expanding propellant smoke wisps.
* **Surface Impact:** Heavy kinetic sparks that chip small craters into asteroid surfaces.

---

## 3. Required Visual Deliverables

1. `proj_pellet_scattergun.png` (12x12 px, 32-bit RGBA) — Molten slug pellet with motion streak.
2. `vfx_scattergun_muzzle.png` (96x64 px, 3-frame flipbook) — Fiery conical propellant burst.
3. `vfx_kinetic_impact_sparks.png` (48x48 px, 3-frame flipbook) — Violent orange/yellow spark splatter on impact.
