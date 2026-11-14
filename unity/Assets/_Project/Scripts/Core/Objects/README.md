# Objects

## Overview
Configurations and definitions for world objects, pickups, collectable resource gems, and power-up modifiers.

## Interface
- `class LootConfig` [LootConfig.cs]
  - `defaultGemValue: int`
  - `magnetRadius: float`
  - `magnetSpeed: float`
  - `minEjectionSpeed: float`
  - `maxEjectionSpeed: float`
  - `gemColliderRadius: float`
  - `shieldOverchargeBonus: float`
  - `weaponOverdriveDamageMultiplier: float`
  - `weaponOverdriveFireRateMultiplier: float`
  - `minFireRate: float`

## Invariants
- `magnetRadius >= 0` and `magnetSpeed >= 0`.
- `weaponOverdriveFireRateMultiplier` balanced to preserve minimum rate clamp.

## Side Effects
- Read-only loot parameters referenced by gem collectors, power-up items, and asteroid drop systems.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
