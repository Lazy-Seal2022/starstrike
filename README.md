# 🚀 StarStrike

> **Modular 2D Multiplatform Spaceship Shooter inspired by Starblast.io**  
> Built with **Phaser 3 + TypeScript + Vite** using **ECS (Entity-Component-System)** architecture.

---

## 🎮 Features (v0.1.0 MVP)

- **Newtonian Space Physics**: Authentic zero-gravity flight feel with forward thrust, reverse braking, space drag, and rotational inertia.
- **Procedural Graphics Generator**: All ship sprites, asteroids, crystals, lasers, and particles are generated dynamically on launch using vector graphics — zero missing image assets.
- **Mining & Asteroid Field**: 80+ asteroids of varying sizes (Large, Medium, Small). Destroying asteroids causes them to fracture into smaller pieces and drop precious glowing crystals.
- **Crystal Magnetism**: Crystals float in space and magnetically pull towards the player's ship when within proximity.
- **Laser Weapon Systems**: Pulse blasters with energy consumption, recoil, and particle spark impacts. Tier 2 ships feature dual weapon barrels.
- **8-Category Upgrade Ladder**:
  1. Shield Capacity
  2. Shield Regeneration Rate
  3. Energy Capacity
  4. Energy Regeneration Rate
  5. Pulse Laser Damage
  6. Pulse Laser Speed
  7. Engine Top Speed
  8. Ship Agility / Turn Rate
- **Ship Evolution System**: Max out your Scout's stats to unlock Tier 2 evolution choices:
  - ⚔️ **Ares Fighter**: High-speed interceptor equipped with dual blaster cannons.
  - ⛏️ **Goliath Miner**: Heavily armored vessel with high shield capacity and mining power.
- **Multiplatform Input Ready**:
  - **Desktop**: Mouse aim + WASD/Arrows thrust + Left Click/Space to fire + Shift/Right Click to boost.
  - **Mobile / Tablet**: Responsive viewport with on-screen virtual touch controls (Fire & Thrust buttons).
- **Decoupled ECS + EventBus**: Clean, extensible architecture designed for easy expansion into multiplayer and additional ship classes.

---

## 🕹️ Controls

| Action | Desktop Controls | Mobile / Touch |
| :--- | :--- | :--- |
| **Aim / Steer** | Move Mouse Cursor | Drag on Left Screen |
| **Forward Thrust** | `W` or `Up Arrow` | `THRUST` Button |
| **Reverse / Brake**| `S` or `Down Arrow`| — |
| **Fire Blasters** | `Left Click` or `Space` | `FIRE` Button |
| **Afterburner Boost** | `Shift` or `Right Click` | Consumes Energy |
| **Upgrades Menu** | `U` Key or Click `[U] UPGRADES` | Tap `[U] UPGRADES` Button |

---

## 🛠️ Technology Stack

- **Game Engine**: [Phaser 3](https://phaser.io/)
- **Language**: [TypeScript](https://www.typescriptlang.org/)
- **Bundler & Dev Server**: [Vite](https://vitejs.dev/)
- **Architecture**: Entity-Component-System (ECS) + EventBus Singleton

---

## 🚀 Getting Started

### Prerequisites
- Node.js (v18+)
- npm

### Installation
```bash
# Clone the repository
git clone https://github.com/Lazy-Seal2022/starstrike.git
cd starstrike

# Install dependencies
npm install

# Start development server with instant Hot Module Replacement (HMR)
npm run dev
```

Open your browser at `http://localhost:3000` to play immediately!

### Production Build
```bash
npm run build
npm run preview
```

---

## 🗺️ Modular Directory Layout

```
starstrike/
├── src/
│   ├── main.ts                # Entry point: Phaser game config
│   ├── core/                  # Engine-agnostic. NO Phaser / DOM imports (reusable on a future server)
│   │   ├── ecs/               # Entity, Component, System, World
│   │   ├── EventBus.ts        # Pub/sub between systems, scenes and UI
│   │   └── types.ts           # Shared TypeScript interfaces
│   ├── config/                # Pure data / tuning
│   │   ├── game.ts            # Physics, world bounds, mining, upgrade costs
│   │   └── ships.ts           # Ship registry (add new ships here)
│   ├── components/            # ECS component data (Transform, Physics, Health, Weapon, ...)
│   ├── features/              # One folder per gameplay feature: its systems + managers
│   │   ├── ai/                # AISystem (enemy drones)
│   │   ├── combat/            # CombatSystem (lasers, damage, death)
│   │   ├── mining/            # MiningSystem (gem magnetism & pickup)
│   │   ├── movement/          # MovementSystem (Newtonian physics)
│   │   ├── regeneration/      # RegenerationSystem (shield & energy regen)
│   │   ├── progression/       # UpgradeManager, ShipEvolution
│   │   └── world/             # WorldManager (starfield, bounds, asteroid spawning)
│   ├── services/              # App-wide singletons: SoundManager, AssetGenerator
│   ├── scenes/                # Phaser scenes: Boot, Game, HUD
│   └── ui/                    # Reusable HUD widgets (Minimap)
├── index.html
├── package.json
├── tsconfig.json              # `@/` alias -> src/
└── vite.config.ts
```

**Rules of thumb**
- New gameplay feature → new folder in `src/features/`, register its system in `GameScene`.
- `core/` and `config/` must never import `phaser` or touch `window`/`document`.
- Import with the `@/` alias (`import { eventBus } from "@/core/EventBus"`), never long `../../` chains.

### Cross-platform targets (planned)
The game is one web build (`dist/`), wrapped per platform. Native folders are generated by their CLIs and live at the repo root:

| Platform | Wrapper | Generated folder |
| :--- | :--- | :--- |
| Web | Vite build, any static host | `dist/` |
| Android | Capacitor | `android/` |
| iOS | Capacitor (needs macOS/Xcode or macOS CI) | `ios/` |
| Windows | Tauri | `src-tauri/` |


---

## 📜 License

MIT License © 2026 Lazy-Seal2022
