# ui

## Overview
Tactical radar minimap and in-game HUD rendering.

## Interface
- `class Minimap` [Minimap.ts]

## Invariants
- Immutable configuration schemas enforced via `readonly`/`as const` [Minimap.ts]

## Side Effects
- Phaser rendering pipeline and scene object mutation [Minimap.ts]

## Verification
- Run: `npm run build (in prototype/)`

