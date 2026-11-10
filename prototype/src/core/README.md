# core

## Overview
Decoupled event messaging (EventBus) and core TypeScript domain types.

## Interface
- `class EventBus` [EventBus.ts]
- `const eventBus` [EventBus.ts]
- `type ShipTier` [types.ts]
- `type UpgradeKey` [types.ts]
- `interface ShipStats` [types.ts]
- `interface UpgradeLevels` [types.ts]
- `interface ShipDefinition` [types.ts]
- `interface GameEvents` [types.ts]

### Subdirectories
- **[ecs](./ecs/README.md)**: Lightweight Entity-Component-System architectural core primitives (Entity, Component, System).

## Invariants
- Singleton instance access control [EventBus.ts]
- Type contracts strictly verified at compile-time via TypeScript [types.ts]

## Side Effects
- In-memory data structure mutation [types.ts]
- Publish-subscribe messaging via EventBus [EventBus.ts]

## Verification
- Run: `npm run build (in prototype/)`

