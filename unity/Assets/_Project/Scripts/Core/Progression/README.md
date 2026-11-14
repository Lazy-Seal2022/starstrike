# Progression

## Overview
Comprehensive progression system containing upgrade definitions, scaling math formulas, and player upgrade state tracking.

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
- Cost calculation enforces integer values and exponential scaling without floating-point runaway.
- MonoBehaviour lifecycle contracts strictly preserved.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
