# Sprite Specification: Sector Biomes & Background Art (`env_biomes`)

> **Category:** Environment & World Objects — Sector Biomes  
> **Target Audience:** Digital Matte Painter & Parallax Artist  
> **In-Game Role:** Background cosmic expanses, parallax starfields & sector hazards  

---

## 1. Concept & Visual Identity

Each sector in StarStrike introduces a distinct environmental backdrop that influences ambient lighting, space dust, and gameplay hazards:

1. **Sector 1: Deep Void** (Dark navy cosmic expanse with 3-layer parallax starfield).
2. **Sector 2: Crystal Belt** (Emerald/turquoise dust field with dense reflective asteroids).
3. **Sector 3: Solar Flare Zone** (Blistering amber heat waves requiring players to shelter in asteroid shadow cones).
4. **Sector 4: Nebular Ion Storm** (Violent magenta gas clouds with atmospheric lightning and radar interference).

---

## 2. Multi-Layer Parallax Architecture

To create immense cosmic depth, the camera renders 4 independent background layers:

```
[ LAYER 0: SKYBOX NEBULA ]      Distant, seamless space nebula texture (Static or slow drift).
[ LAYER 1: DEEP STARS ]         Dense field of tiny, pin-prick stars (Parallax Factor: 0.1).
[ LAYER 2: MID-GROUND DUST ]    Drifting cosmic dust motes and colored gas clouds (Parallax: 0.3).
[ LAYER 3: DERELICT SHADOWS ]   Distant silhouetted space stations in background blur (Parallax: 0.5).
[ LAYER 4: GAMEPLAY ARENA ]     Ships, asteroids, and combat action (Parallax Factor: 1.0).
```

---

## 3. Sector 3 Special Hazard: Solar Flare & Asteroid Shadows

In the **Solar Flare Zone**, a wall of intense radiation sweeps across the sector periodically:
* **The Hazard Wall:** A bright glowing amber/orange radiation wave (`#F59E0B`) washing over the screen.
* **Asteroid Shadow Cones:** Large asteroids cast soft **directional shadow zones** behind them opposite the solar star. The artist must provide a soft dark shadow sprite that projects behind large asteroids to visually signal safe shelters to the player.

---

## 4. Required Visual Deliverables

1. `bg_skybox_deep_void.png` (1024x1024 px, seamless tiling) — Navy/black cosmic void.
2. `bg_skybox_crystal_belt.png` (1024x1024 px, seamless tiling) — Turquoise/emerald dust field.
3. `bg_skybox_solar_flare.png` (1024x1024 px, seamless tiling) — Fiery amber solar nebula.
4. `bg_skybox_ion_storm.png` (1024x1024 px, seamless tiling) — Magenta ion gas clouds.
5. `bg_stars_far.png` & `bg_stars_mid.png` (512x512 px with transparency) — Parallax star layers.
6. `env_solar_shadow_cone.png` (256x128 px, soft gradient alpha) — Shadow umbrella shelter sprite.
