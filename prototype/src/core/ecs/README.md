# ecs

## Overview
Lightweight Entity-Component-System architectural core primitives (Entity, Component, System).

## Interface
- `interface Component` [Component.ts]
- `class Entity` [Entity.ts]
- `interface System` [System.ts]
- `class World` [World.ts]

## Invariants
- Immutable configuration schemas enforced via `readonly`/`as const` [Component.ts]
- Immutable configuration schemas enforced via `readonly`/`as const` [Entity.ts]
- Type contracts strictly verified at compile-time via TypeScript [System.ts]
- Type contracts strictly verified at compile-time via TypeScript [World.ts]

## Side Effects
- In-memory data structure mutation [Component.ts]
- In-memory data structure mutation [Entity.ts]
- In-memory data structure mutation [System.ts]
- In-memory data structure mutation [World.ts]

## Verification
- Run: `npm run build (in prototype/)`

