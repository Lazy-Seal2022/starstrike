# 04 — Arsenal, Projectiles & Tactical Skills

> **Target:** VFX Artists, 2D Weapon Illustrators, Particle Specialists  
> **Topic:** The 4 Primary Weapon Systems, 4 Tactical Skills & Combat Juice VFX  

---

## 1. Visual Arsenal Philosophy: Weapon Identity

Every weapon in StarStrike must feel distinct in **silhouette, velocity, trajectory, and acoustic-visual impact**. Players switch weapons dynamically (Number keys 1–4) depending on the tactical situation:

```
[ 1: PULSE LASER ]    Fast, accurate, cyan energy bolts for medium/long-range dogfighting.
[ 2: SCATTERGUN ]     Heavy kinetic pellet cone for point-blank asteroid and swarm clearing.
[ 3: SWARM MISSILES ] Agile homing micro-rockets with twisting smoke trails.
[ 4: TACHYON BEAM ]   Continuous, searing violet laser cutting through armor.
```

---

## 2. Primary Weapon Systems & Projectiles

### 2.1. Weapon 1: Pulse Laser (`proj_laser_player`)
* **Weapon Concept:** Rapid-fire directed energy bolts.
* **Canvas Size:** 16 x 32 px (Vertical bolt, Length: 1.0 unit, Width: 0.25 unit).
* **Color Palette:** Saturated Cyan (`#00F0FF`) with pure White hot plasma core (`#FFFFFF`).
* **Visual Profile:**
  - Sharp, aerodynamic capsule shape with a tapered glowing tail.
  - Leaves a faint 0.1s fading trail behind it.
* **Muzzle Flash:** 2-frame 32x32px circular plasma burst at weapon hardpoints upon firing.
* **Impact FX:** Circular spark burst (6–8 micro-particles bouncing off armor).

---

### 2.2. Weapon 2: Scattergun (`proj_pellet_scatter`)
* **Weapon Concept:** Multi-projectile kinetic slug blast with devastating close-range spread.
* **Canvas Size:** 12 x 12 px per pellet (6 pellets fired in a 35° cone).
* **Color Palette:** Molten Tungsten Orange (`#F97316`) fading into Yellow sparks (`#FDE047`).
* **Visual Profile:**
  - Round glowing incendiary flechettes with short kinetic streaks.
* **Muzzle Flash:** Broad conical flame erupting from the ship's prow with a faint puff of propellant smoke.
* **Impact FX:** Heavy metal-on-metal spark shower with small kinetic impact craters on asteroids.

---

### 2.3. Weapon 3: Swarm Missiles (`proj_missile_homing`)
* **Weapon Concept:** Autonomous seeker missiles that track enemy vessels and asteroids.
* **Canvas Size:** 16 x 32 px.
* **Silhouette:** Sleek rocket casing with 4 stabilizing fins and a glowing guidance optic nose.
* **Color Palette:** Matte Olive/Gray casing (`#475569`) with Hazard Amber nosecone (`#F59E0B`).
* **Particle Trails:**
  - **Engine Plume:** Intense needle-like flame flare.
  - **Smoke Trail:** Dense particle ribbon trailing behind each missile with corkscrewing turbulence.
* **Lock-On Reticle UI:** Rotating square or diamond HUD brackets locking onto the targeted enemy.
* **Detonation FX:** Volumetric fiery blast with expanding smoke ring.

---

### 2.4. Weapon 4: Continuous Tachyon Beam (`proj_beam_tachyon`)
* **Weapon Concept:** Non-stop coherent laser beam that concentrates sustained tick damage.
* **Sprite Construction (3-Piece Modular Texture):**
  1. **Beam Origin / Emitter (32x32 px):** Blinding lens flare anchored to the ship's nose.
  2. **Beam Body (32x64 px Seamless Tiling):** Coherent energy cylinder with undulating electric arcs along the edges.
  3. **Beam Impact Core (48x48 px):** Expanding contact point with violent lateral spark splashes.
* **Color Palette:** Ultra-violet / Deep Electric Violet (`#8B5CF6`) with a blazing white laser core.
* **Dynamic Behavior:** Beam width pulses subtly (thickening during sustained fire) to communicate rising energy draw.

---

## 3. Tactical Skills & Ultimate Abilities

### 3.1. Active 1: Forward Shockwave (`skill_shockwave`)
* **Concept:** High-energy kinetic compression wave fired forward, bowling over enemies and pushing the ship backward via Newtonian recoil.
* **Sprite Canvas:** 64 x 32 px (Expands to 2.5 x 1.2 Unity Units).
* **Visual Profile:**
  - Crescent-shaped aerodynamic sonic boom or distortion wave.
  - Translucent electric blue ripple with bright leading edge (`#38BDF8`).
  - Leaves a vacuum wake line that briefly parts space dust motes.

---

### 3.2. Active 2: Micro-Warp Blink (`skill_blink`)
* **Concept:** Instant forward subspace teleport covering 8.5 units with a brief invulnerability window.
* **Visual Sequence:**
  1. **Origin Dematerialization:** Ship collapses into a sharp horizontal cyan light streak (`0.05s`).
  2. **Afterimage Ghost Trail:** Spawns 3 translucent afterimage silhouette sprites of the player ship along the warp path, fading from 60% to 0% alpha over 0.3s.
  3. **Arrival Compression:** Sharp chromatic aberration burst at the destination coordinate as the ship snaps back into physical space.

---

### 3.3. Active 3: EMP Nova (`skill_emp_nova`)
* **Concept:** Omnidirectional 360-degree electromagnetic blast radiating across a **9.5-unit radius**.
* **Visual Structure:**
  1. **Shockwave Perimeter Ring (`EMPShockwaveVisual`):** Vector-smooth glowing neon cyan ring that rapidly bursts outward from radius 0.3 to 9.5 units over 0.45 seconds.
  2. **Inner Radiant Energy Disk:** Expanding translucent cyan pulse inside the ring that washes over the screen.
  3. **Stun Arcs on Targets:** Every enemy caught in the blast lights up with crackling electric blue lightning arcs running along their hull panels for the full 3.0-second duration.
  4. **Player Flash:** Player ship flashes intense electric cyan on release.

---

### 3.4. Passive: Reactive Barrier / Deflector Surge (`skill_barrier`)
* **Concept:** Autonomous defensive deflector grid that pops up upon taking heavy shield damage.
* **Visual Profile:**
  - Translucent spherical shield bubble surrounding the ship (`Radius: 1.6 units`).
  - **Hexagonal Honeycomb Mesh Texture:** Transparent in the center, glowing bright cobalt (`#3B82F6`) along hexagonal wireframe cell edges.
  - **Impact Ripple Shader:** When incoming enemy lasers hit the barrier, localized circular ripples propagate outward across the hexagonal grid.

---

## 4. Combat Juice & Feedback FX Asset Matrix

| Effect Name | Asset Type | Visual Description |
| :--- | :--- | :--- |
| `fx_explosion_small.png` | 6-frame flipbook (64x64) | Quick fiery burst for basic drones and missiles (0.25s). |
| `fx_explosion_large.png` | 8-frame flipbook (128x128) | Heavy multi-stage fireball with black smoke puffs and shrapnel for cruisers/bosses. |
| `fx_shield_impact.png` | 4-frame flipbook (48x48) | Circular blue energy ripples with spark splash. |
| `fx_hull_spark.png` | Particle Sprite (8x8) | Tiny glowing embers for bullet bounces and thruster debris. |
| `fx_laser_charge.png` | 4-frame flipbook (32x32) | Concentrated swirling light orbs for sniper and boss attack telegraphs. |
| `fx_warp_distortion.png` | Normal Map Texture (128x128) | Radial ripple normal map used with URP screen distortion shaders for EMP and Blink. |
