# src

## Overview
Web prototype application bootstrap and Phaser game instance initialization.

## Interface
- Direct modular code package symbols.

### Subdirectories
- **[components](./components/README.md)**: ECS Component definitions for ships, physics, weapons, health, and collectables.
- **[config](./config/README.md)**: Static balance configurations, weapon parameters, and ship evolution progression data.
- **[core](./core/README.md)**: Decoupled event messaging (EventBus) and core TypeScript domain types.
- **[features](./features/README.md)**: Gameplay feature systems rollup for the web prototype.
- **[scenes](./scenes/README.md)**: Phaser 3 stateful scenes (BootScene asset preload, GameScene world, HUDScene overlays).
- **[services](./services/README.md)**: Procedural vector asset generator and sound playback management.
- **[ui](./ui/README.md)**: Tactical radar minimap and in-game HUD rendering.

## Invariants
- Type contracts strictly verified at compile-time via TypeScript [main.ts]

## Side Effects
- In-memory data structure mutation [main.ts]

## Verification
- Run: `npm run build (in prototype/)`

