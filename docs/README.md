# StarStrike Documentation Hub

Welcome to the central documentation hub for **StarStrike**, a high-octane, top-down 2D space arena combat and mining survival game built with Unity 6 (URP 2D).

This documentation suite provides a complete breakdown of current game features, flight and combat manuals, technical architecture, and a comprehensive gap analysis detailing what the game is currently missing and where its roadmap leads.

---

## Documentation Sitemap

```
docs/
├── README.md                                 # Documentation Hub & Index
├── overview/
│   └── features.md                           # Exhaustive Current Feature Matrix & Subsystem Audit
├── gameplay/
│   ├── how_to_play.md                        # Official Player Manual, Flight Mechanics & Controls Guide
│   ├── ships_and_progression.md              # Ship Classes, Evolution Tree & Upgrade Economics
│   └── combat_and_arsenal.md                 # Weapons, Skills, Enemy AI & Combat Director
├── technical/
│   └── architecture_and_layers.md            # Enterprise ASMDEF Architecture & Systems Design
├── art_specifications/                       # Visual Design Briefs & Sprite Design Suite
│   ├── README.md                             # Master Art Hub & Artist Onboarding Guide
│   ├── 00_style_guide/                       # Universal Aesthetics, Palettes, Lighting & PPU
│   ├── 01_player_ships/                      # 1 File Per Player Hull (Scout, Fighter, Miner, etc.)
│   ├── 02_enemies_and_bosses/                # 1 File Per Enemy Craft & Flagship Dreadnought Boss
│   ├── 03_weapons_and_projectiles/           # 1 File Per Weapon System (Laser, Scatter, Missiles, Beam)
│   ├── 04_tactical_skills_and_vfx/           # 1 File Per Ability FX (Shockwave, Blink, EMP, Barrier)
│   ├── 05_environment_and_pickups/           # 1 File Per Asteroid, Gem, Power-up & Sector Biome
│   └── 06_ui_ux_widgets/                     # 1 File Per HUD Gauge, Minimap, Cards, Screens & Icons
└── roadmap/
    ├── gap_analysis_and_missing_features.md  # Detailed Missing Features & Technical Debt Audit
    └── future_roadmap.md                     # Milestone Roadmap (Alpha -> Beta -> V1.0 -> LiveOps)
```

---

## Quick Navigation

### 1. Game Overview & Features
- [Current Features Matrix](file:///home/quan/projects/starstrike/docs/overview/features.md): Discover every system currently implemented in StarStrike, from Newtonian inertia and multi-slot weapon systems to wave generation, procedural rendering, and LAN dogfighting.

### 2. Player Manuals & Guides
- [How to Play Guide](file:///home/quan/projects/starstrike/docs/gameplay/how_to_play.md): Step-by-step instructions on piloting your ship, reading the HUD, mining asteroids, surviving drone swarms, and dominating boss encounters.
- [Ships & Evolution Guide](file:///home/quan/projects/starstrike/docs/gameplay/ships_and_progression.md): In-depth breakdown of the 5 playable ship classes (Delta Scout, Ares Fighter, Goliath Miner, Viper Interceptor, Titan Juggernaut), stat values, in-run stat leveling, and permanent meta-progression.
- [Combat & Arsenal Guide](file:///home/quan/projects/starstrike/docs/gameplay/combat_and_arsenal.md): Complete analysis of primary pulse cannons, scatterguns, homing missiles, continuous beam lasers, tactical skills, and enemy behavioral patterns.

### 3. Architecture & Engineering
- [Architecture & Layers](file:///home/quan/projects/starstrike/docs/technical/architecture_and_layers.md): Technical deep-dive into Unity's Assembly Definition decoupling (`Core` -> `Platform` -> `Gameplay` -> `UI` / `Network`), ScriptableObject configurations, event-driven communication, and object pooling.

### 4. Art Bible & Sprite Design Specifications
- [Master Art Production Hub](file:///home/quan/projects/starstrike/docs/art_specifications/README.md): Master index and artist onboarding guide explaining the modular 1-to-1 sprite documentation.
- [00. Style Guides](file:///home/quan/projects/starstrike/docs/art_specifications/00_style_guide/style_art_direction_and_pillars.md): Overarching game aesthetics, functional color palettes, emissive masks, and 32 PPU technical constraints.
- [01. Player Ships](file:///home/quan/projects/starstrike/docs/art_specifications/01_player_ships/ship_delta_scout.md): 7 dedicated specification files for each playable hull (Scout, Fighter, Miner, Interceptor, Juggernaut, Carrier, Phantom).
- [02. Hostile Armada & Bosses](file:///home/quan/projects/starstrike/docs/art_specifications/02_enemies_and_bosses/enemy_patrol_drone.md): 5 dedicated specification files for hostile drones, kamikaze rammers, snipers, shield cruisers, and the multi-phase Dreadnought Boss.
- [03. Weapons & Projectiles](file:///home/quan/projects/starstrike/docs/art_specifications/03_weapons_and_projectiles/proj_pulse_laser.md): 4 dedicated specification files for Pulse Laser, Scattergun, Swarm Missiles, and Tachyon Beam.
- [04. Tactical Skills & Abilities](file:///home/quan/projects/starstrike/docs/art_specifications/04_tactical_skills_and_vfx/skill_forward_shockwave.md): 4 dedicated specification files for Forward Shockwave, Blink Warp, EMP Nova, and Reactive Barrier.
- [05. Environment & Pickups](file:///home/quan/projects/starstrike/docs/art_specifications/05_environment_and_pickups/env_asteroids_system.md): 4 dedicated specification files for Asteroids, Gem Minerals, Power-Ups, and Sector Biomes.
- [06. UI/UX & HUD Widgets](file:///home/quan/projects/starstrike/docs/art_specifications/06_ui_ux_widgets/ui_hud_vital_arcs.md): 5 dedicated specification files for Vital Arcs, Minimap Radar, Evolution Cards, Hangar Showcase, and Master Iconography.

### 5. Roadmap & Future Evolution
- [Missing Features & Gap Analysis](file:///home/quan/projects/starstrike/docs/roadmap/gap_analysis_and_missing_features.md): An honest, rigorous evaluation of what StarStrike currently lacks—audio dynamism, elite enemies, endless mode modifiers, full UGS online matchmaking, and UI polish.
- [Future Development Roadmap](file:///home/quan/projects/starstrike/docs/roadmap/future_roadmap.md): The phased delivery plan from current state through Beta stabilization, Steam/Mobile launch, and post-launch live expansions.
