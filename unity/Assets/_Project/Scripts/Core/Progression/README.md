# Progression

## Overview
Mathematical formulas for upgrade curves and player progression state.

## Interface
- `class ProgressionMath` [ProgressionMath.cs]
- `CalculateStat(float baseValue, float multiplier, int level) -> float` [ProgressionMath.cs]
- `CalculateCost(int baseCost, float costMultiplier, int level) -> int` [ProgressionMath.cs]
- `GetCurrentStats(ShipDefinition ship, UpgradeDefinition upgradeDef, UpgradeState state) -> StatValues` [ProgressionMath.cs]
- `class UpgradeState` [UpgradeState.cs]
- `GetLevel(string statName) -> int` [UpgradeState.cs]
- `Reset() -> void` [UpgradeState.cs]
- `AreAllStatsMaxed(int maxLevel) -> bool` [UpgradeState.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [ProgressionMath.cs]
- MonoBehaviour lifecycle contracts strictly preserved [UpgradeState.cs]

## Side Effects
- In-memory state manipulation [ProgressionMath.cs]
- In-memory state manipulation [UpgradeState.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

