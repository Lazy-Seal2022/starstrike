# Style Guide — Technical Constraints & Export Standards

> **Document Type:** Overarching Game Style Specification  
> **Audience:** Technical Artists, Sprite Packers, Animators  
> **Target Engine:** Unity 6 LTS (URP 2D)  

---

## 1. Grid, Scale & Pixels Per Unit (PPU)

StarStrike enforces a single unified coordinate scale across all world-space game objects:

$$\mathbf{1.0\ Unity\ World\ Unit} = \mathbf{32\ Pixels}$$

```
+--------------------------------------------------------------------------+
|  CANVAS SIZE   |  WORLD SIZE  |  TYPICAL ASSET USAGE                     |
|  16 x 16 px    |  0.5 x 0.5 u |  Gem crystals, debris motes, particles    |
|  32 x 32 px    |  1.0 x 1.0 u |  Small asteroids, laser bolts, power-ups |
|  64 x 64 px    |  2.0 x 2.0 u |  Tier 1 Scout, Interceptor, Medium rocks |
|  80 x 80 px    |  2.5 x 2.5 u |  Tier 2 Fighter, Miner, Phantom          |
|  96 x 96 px    |  3.0 x 3.0 u |  Tier 3 Juggernaut, Carrier              |
|  128 x 128 px  |  4.0 x 4.0 u |  Large Asteroids, Elite Cruisers         |
|  256 x 256 px  |  8.0 x 8.0 u |  Dreadnought Flagship Capital Boss       |
+--------------------------------------------------------------------------+
```

---

## 2. Pivot & Orientation Standards

All game physics and rotation routines assume consistent pivot and facing orientations:

* **Sprite Facing Angle:** All ships, projectiles, and directional effects must be drawn facing **Right (+X axis)** in the source PNG file. (In Unity 2D, `transform.right` is standard forward heading).
* **Pivot Placement:** **Exact Center `(0.5, 0.5)`**. The physical center of mass MUST align with the center of the canvas so spinning asteroids and banking ships do not wobble artificially.

---

## 3. Render Layers & Sorting Order

To prevent visual artifacts, assets must be assigned to these Unity Sorting Layers:

| Sorting Layer | Order | Elements Contained |
| :--- | :---: | :--- |
| **Background** | 0 | Cosmic skybox, deep nebulas |
| **ParallaxStars** | 10 | 3-layer parallax starfield dust |
| **EnvironmentUnder** | 20 | Asteroid drop shadows, solar flare occlusions |
| **Environment** | 30 | Asteroids, planetary debris, mining rocks |
| **Pickups** | 40 | Mined crystals, power-up items |
| **Enemies** | 50 | Hostile drones, snipers, rammers, bosses |
| **Player** | 60 | Player ship hulls, cockpits, thruster fires |
| **Projectiles** | 70 | Laser bolts, missiles, tachyon beams |
| **VFX / Explosions** | 80 | Shockwaves, fireball flips, EMP rings |
| **HUD / UI** | 100+ | Vital arcs, minimap radar, floating combat text |

---

## 4. Export & File Formats

* **Image Format:** 32-bit PNG (RGB + 8-bit Alpha).
* **Alpha Integrity:** No blurred halo fringes around silhouettes. Use clean 1-pixel alpha transitions or matte premultiplication.
* **Atlas Compatibility:** All static and flipbook sprites will be packed into Unity SpriteAtlas (`StarStrikeMasterAtlas.spriteatlasv2`). Power-of-two texture dimensions (POT) are required.
