# progression

## Overview
Ship stat tier upgrades and multi-tier ship evolution system.

## Interface
- `class ShipEvolution` [ShipEvolution.ts]
- `class UpgradeManager` [UpgradeManager.ts]

## Invariants
- Type contracts strictly verified at compile-time via TypeScript [ShipEvolution.ts]
- Type contracts strictly verified at compile-time via TypeScript [UpgradeManager.ts]

## Side Effects
- Publish-subscribe messaging via EventBus [ShipEvolution.ts]
- Publish-subscribe messaging via EventBus [UpgradeManager.ts]

## Verification
- Run: `npm run build (in prototype/)`

