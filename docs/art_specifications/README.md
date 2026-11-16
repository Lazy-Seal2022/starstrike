# StarStrike — Game Art Production Bible & Asset Specification

> **Version:** 1.0.0 (Production Master)  
> **Target Engine:** Unity 6 LTS (Universal Render Pipeline - 2D Renderer)  
> **Intended Audience:** Concept Artists, 2D Sprite/Vector Illustrators, UI/UX Designers, VFX/Technical Artists

---

## 1. Project Visual Overview & Vision

**StarStrike** is a top-down, high-tempo retro-modern space combat roguelite. The visual target combines the razor-sharp clarity of classic vector space shooters (*Asteroids*, *Geometry Wars*) with the tactile weight, industrial grit, and neon-lit atmosphere of modern sci-fi (*FTL*, *Synthetik*, *The Expanse*).

### Visual Pillars
1. **High-Contrast Readability at 60+ FPS**: Space is vast, dark, and filled with chaos. The player must instantaneously identify their ship, bullet streams, enemy threats, and asteroid hazards through distinct silhouette language and dedicated emissive color hierarchy.
2. **Industrial Tactility Meets Neon Energy**: Ships are rugged, functional mechanical craft (exposed piping, plating, visible thruster bells, hardpoint mounts) contrasted with intensely glowing plasma, shields, and laser effects that pop against cold space.
3. **Visceral Juice & Physical Feedback**: Every action has a visual reaction. Guns recoil and emit hot exhaust, thrusters flare when boosting, shields ripple with hexagonal energy grids on impact, and ships crack and shed debris before exploding into brilliant shrapnel.

---

## 2. Technical Asset Specifications & Art Constraints

Artists must follow these strict technical standards to ensure all assets integrate directly into the Unity 2D pipeline:

| Parameter | Specification | Notes |
| :--- | :--- | :--- |
| **Render Pipeline** | Universal Render Pipeline (URP 2D) | Supports 2D Lit/Unlit materials, Normal Maps, and Post-Processing Bloom. |
| **Pixels Per Unit (PPU)** | **32 PPU** (Standard Game World) | 1 Unity World Unit = 32 Pixels (e.g., a 64x64px ship spans 2.0 Unity units). |
| **Source Texture Format** | 32-bit PNG with RGBA (Alpha channel) | Crisp transparency with clean edges; zero accidental transparent halos. |
| **Color Space** | Linear / sRGB (Gamma 2.2) | Optimized for URP Bloom and Color Adjustments. |
| **Texture Atlas Packing** | Power-of-Two (POT) Spritesheets | Packed via Unity SpriteAtlas (`StarStrikeMasterAtlas.spriteatlasv2`). |
| **Pivot Standards** | **Center (0.5, 0.5)** | All rotating ships, projectiles, and asteroids must pivot at their physics center of mass. |
| **Default Facing Angle** | **Facing UP / North (+Y axis) or RIGHT (+X)** | Ships and projectiles are rendered facing **Right (+X)** in source files (or Up with documented sprite rotation). |
| **Lighting Response** | Diffuse + Emissive Mask | Ship bases use standard diffuse; weapon hardpoints and thruster nozzles use pure white emissive masks. |

---

## 3. Universal Color Palette & Emission Hierarchy

To maintain instantaneous gameplay readability, colors are strictly bound to gameplay functions:

```
[ PLAYER / ALLY ]      Cyan / Electric Blue   (#00F0FF, #00A2FF)
[ ENEMY / HOSTILE ]    Crimson / Neon Red     (#FF2244, #FF0055)
[ SHIELD / DEFENSE ]   Vibrant Cobalt / Azure (#3B82F6, #60A5FA)
[ ENERGY / CHARGE ]    High-Volt Yellow       (#FACC15, #FFE600)
[ CRITICAL / DANGER ]  Hazard Orange          (#F97316, #FF5500)
[ MINING / VALUABLES ] Emerald / Jade Green   (#10B981, #34D399)
[ SPECIAL / WARP ]     Tachyon Violet         (#A855F7, #C084FC)
[ ENVIRONMENT DEBRIS ] Slate / Gunmetal Gray  (#334155, #64748B)
```

---

## 4. Production Document Index

This documentation suite is broken down into 6 focused modular specifications. Each document contains exact dimensions, animation states, hardpoint coordinates, and visual delivery checklists:

* **[01. Art Bible & Style Guide](file:///home/quan/projects/starstrike/docs/art_specifications/01_art_bible_and_style_guide.md)**  
  *Silhouette philosophy, lighting guidelines, layer sorting order, file naming conventions, and export presets.*
* **[02. Player Ships & Hangar Customization](file:///home/quan/projects/starstrike/docs/art_specifications/02_player_ships_and_hangar.md)**  
  *Detailed visual briefs for all 7 playable hulls across Tiers 1–3 (Scout, Fighter, Miner, Interceptor, Juggernaut, Carrier, Phantom), weapon hardpoints, and thrusters.*
* **[03. Enemy Armada & Flagship Bosses](file:///home/quan/projects/starstrike/docs/art_specifications/03_enemy_armada_and_bosses.md)**  
  *Combat archetypes (Swarm Drone, Kamikaze Rammer, Railgun Sniper, Support Shield Cruiser) and the multi-phase Dreadnought Boss.*
* **[04. Arsenal, Projectiles & Tactical Skills](file:///home/quan/projects/starstrike/docs/art_specifications/04_arsenal_projectiles_and_vfx.md)**  
  *Visual specs for the 4 primary weapon systems (Pulse Laser, Scattergun, Homing Missiles, Tachyon Beam) and 4 tactical skills (Shockwave, Blink, EMP Nova, Barrier).*
* **[05. Environment, Asteroids & Sector Biomes](file:///home/quan/projects/starstrike/docs/art_specifications/05_environment_biomes_and_hazards.md)**  
  *Asteroid size classes with fracture states, energy crystals, power-up icons, and 4 background sector biomes (Deep Void, Crystal Belt, Solar Flare, Nebular Storm).*
* **[06. UI, UX & HUD Design System](file:///home/quan/projects/starstrike/docs/art_specifications/06_ui_ux_and_hud_system.md)**  
  *Complete in-game HUD layout, dynamic thruster gauge, health/shield arcs, minimap radar widgets, branch evolution cards, and menu wireframes.*
