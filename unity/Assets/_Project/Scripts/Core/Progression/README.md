# Progression

## Overview
Mathematical formulas for upgrade curves, player progression state calculations, and stat evaluation.

## Interface
- `class ProgressionMath` [ProgressionMath.cs]
  - `CalculateStat(float baseValue, float multiplier, int level) -> float`
  - `CalculateCost(int baseCost, float costMultiplier, int level) -> int`
  - `GetCurrentStats(ShipDefinition ship, UpgradeDefinition upgradeDef, UpgradeState state) -> StatValues`
- `class UpgradeState` [UpgradeState.cs]
  - `GetLevel(string statName) -> int`
  - `Reset() -> void`
  - `AreAllStatsMaxed(int maxLevel) -> bool`

## Invariants
- Progression calculations are functional, deterministic, and free of floating-point runaway.
- State mutation preserves lifecycle invariants.

## Side Effects
- In-memory progression state tracking.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
