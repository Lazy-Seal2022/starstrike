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
├── art_specifications/                       # Visual Design Briefs & Art Production Bible
│   ├── README.md                             # Art Hub, High-Level Pillars & Technical Asset Specs
│   ├── 01_art_bible_and_style_guide.md       # Aesthetic Theme, Palette Codes, PPU & Render Sorting
│   ├── 02_player_ships_and_hangar.md         # 7 Playable Hulls, Hardpoints & Thruster Sockets
│   ├── 03_enemy_armada_and_bosses.md         # Hostile Archetypes, Telegraphs & Dreadnought Boss
│   ├── 04_arsenal_projectiles_and_vfx.md     # 4 Primary Weapons, 4 Skills & Combat VFX Matrix
│   ├── 05_environment_biomes_and_hazards.md  # Asteroids, Gem Minerals & 4 Sector Biomes
│   └── 06_ui_ux_and_hud_system.md            # HUD Wireframe, Evolution Cards, Screens & Icons
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

### 4. Art Bible & Visual Asset Specifications
- [Master Art Production Guide](file:///home/quan/projects/starstrike/docs/art_specifications/README.md): High-level visual pillars, technical constraints, PPU rules, and asset delivery standards.
- [01. Art Bible & Style Guide](file:///home/quan/projects/starstrike/docs/art_specifications/01_art_bible_and_style_guide.md): Color palettes, silhouette philosophy, lighting, and naming conventions.
- [02. Player Ships & Hangar](file:///home/quan/projects/starstrike/docs/art_specifications/02_player_ships_and_hangar.md): Specifications for all 7 player vessels across Tiers 1–3, weapon hardpoint mounts, and thrusters.
- [03. Enemy Armada & Bosses](file:///home/quan/projects/starstrike/docs/art_specifications/03_enemy_armada_and_bosses.md): Drone swarms, kamikaze rammers, snipers, shield cruisers, and the multi-phase Dreadnought Boss.
- [04. Arsenal, Projectiles & VFX](file:///home/quan/projects/starstrike/docs/art_specifications/04_arsenal_projectiles_and_vfx.md): 4 primary weapon systems, 4 tactical skills, and combat particle matrices.
- [05. Environment & Biomes](file:///home/quan/projects/starstrike/docs/art_specifications/05_environment_biomes_and_hazards.md): Asteroid fracture trees, mining crystals, power-up orbs, and 4 sector biomes.
- [06. UI/UX & HUD System](file:///home/quan/projects/starstrike/docs/art_specifications/06_ui_ux_and_hud_system.md): In-game flight telemetry HUD wireframes, branch evolution cards, and complete icon lists.

### 5. Roadmap & Future Evolution
- [Missing Features & Gap Analysis](file:///home/quan/projects/starstrike/docs/roadmap/gap_analysis_and_missing_features.md): An honest, rigorous evaluation of what StarStrike currently lacks—audio dynamism, elite enemies, endless mode modifiers, full UGS online matchmaking, and UI polish.
- [Future Development Roadmap](file:///home/quan/projects/starstrike/docs/roadmap/future_roadmap.md): The phased delivery plan from current state through Beta stabilization, Steam/Mobile launch, and post-launch live expansions.
