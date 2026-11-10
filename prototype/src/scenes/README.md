# scenes

## Overview
Phaser 3 stateful scenes (BootScene asset preload, GameScene world, HUDScene overlays).

## Interface
- `class BootScene` [BootScene.ts]
- `class GameScene` [GameScene.ts]
- `class HUDScene` [HUDScene.ts]

## Invariants
- Type contracts strictly verified at compile-time via TypeScript [BootScene.ts]
- Type contracts strictly verified at compile-time via TypeScript [GameScene.ts]
- Type contracts strictly verified at compile-time via TypeScript [HUDScene.ts]

## Side Effects
- In-memory data structure mutation [BootScene.ts]
- Phaser rendering pipeline and scene object mutation [GameScene.ts]
- Publish-subscribe messaging via EventBus [GameScene.ts]
- Publish-subscribe messaging via EventBus [HUDScene.ts]

## Verification
- Run: `npm run build (in prototype/)`

