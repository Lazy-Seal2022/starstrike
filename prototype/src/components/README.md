# components

## Overview
ECS Component definitions for ships, physics, weapons, health, and collectables.

## Interface
- `class TransformComponent` [index.ts]
- `class PhysicsComponent` [index.ts]
- `class SpriteComponent` [index.ts]
- `class HealthComponent` [index.ts]
- `class EnergyComponent` [index.ts]
- `class WeaponComponent` [index.ts]
- `class AsteroidComponent` [index.ts]
- `class GemComponent` [index.ts]
- `class LaserComponent` [index.ts]
- `class AIComponent` [index.ts]
- `class PlayerComponent` [index.ts]

## Invariants
- Immutable configuration schemas enforced via `readonly`/`as const` [index.ts]

## Side Effects
- In-memory data structure mutation [index.ts]

## Verification
- Run: `npm run build (in prototype/)`

