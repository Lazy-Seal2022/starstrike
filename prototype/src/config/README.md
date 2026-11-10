# config

## Overview
Static balance configurations, weapon parameters, and ship evolution progression data.

## Interface
- `const GAME_CONFIG` [game.ts]
- `const SHIPS_REGISTRY` [ships.ts]

## Invariants
- Type contracts strictly verified at compile-time via TypeScript [game.ts]
- Type contracts strictly verified at compile-time via TypeScript [ships.ts]

## Side Effects
- In-memory data structure mutation [game.ts]
- In-memory data structure mutation [ships.ts]

## Verification
- Run: `npm run build (in prototype/)`

