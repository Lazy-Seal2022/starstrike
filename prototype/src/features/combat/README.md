# combat

## Overview
Weapon firing, projectile trajectory simulation, and damage calculation.

## Interface
- `class CombatSystem` [CombatSystem.ts]

## Invariants
- Type contracts strictly verified at compile-time via TypeScript [CombatSystem.ts]

## Side Effects
- Phaser rendering pipeline and scene object mutation [CombatSystem.ts]
- Publish-subscribe messaging via EventBus [CombatSystem.ts]

## Verification
- Run: `npm run build (in prototype/)`

