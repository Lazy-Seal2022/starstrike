# services

## Overview
Procedural vector asset generator and sound playback management.

## Interface
- `class AssetGenerator` [AssetGenerator.ts]
- `class SoundManager` [SoundManager.ts]
- `const soundManager` [SoundManager.ts]

## Invariants
- Singleton instance access control [SoundManager.ts]
- Type contracts strictly verified at compile-time via TypeScript [AssetGenerator.ts]

## Side Effects
- In-memory data structure mutation [AssetGenerator.ts]
- Publish-subscribe messaging via EventBus [SoundManager.ts]

## Verification
- Run: `npm run build (in prototype/)`

