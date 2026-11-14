# Progression

## Overview
Mathematical formulas for upgrade curves, player progression state, and upgrade tier metadata definitions.

## Interface
- `class UpgradeDefinition` [UpgradeDefinition.cs]
  - `id: string`
  - `upgradeName: string`
  - `description: string`
  - `baseCost: int`
  - `costMultiplier: float`
  - `maxLevel: int`
  - `statMultiplierPerLevel: float`
- `class ProgressionMath` [ProgressionMath.cs]
  - `CalculateStat(float baseValue, float multiplier, int level) -> float`
  - `CalculateCost(int baseCost, float costMultiplier, int level) -> int`
  - `GetCurrentStats(ShipDefinition ship, UpgradeDefinition upgradeDef, UpgradeState state) -> StatValues`
- `class UpgradeState` [UpgradeState.cs]
  - `GetLevel(string statName) -> int`
  - `Reset() -> void`
  - `AreAllStatsMaxed(int maxLevel) -> bool`

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved.
- Upgrade costs and stat bonuses scale deterministically without floating-point runaway.

## Side Effects
- In-memory progression state tracking.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
