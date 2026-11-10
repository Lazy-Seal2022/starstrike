# Progression

## Overview
Player XP level ladder, ship evolution unlocking, and achievement milestones.

## Interface
- `class AchievementManager` [AchievementManager.cs]
- `class PlayerProgression` [PlayerProgression.cs]
- `Initialize() -> void` [PlayerProgression.cs]
- `ResetForNewRun() -> void` [PlayerProgression.cs]
- `CreateDefaultShipTree() -> ShipDefinition` [PlayerProgression.cs]
- `AddGems(int amount) -> void` [PlayerProgression.cs]
- `UpgradeStat(string statName) -> bool` [PlayerProgression.cs]
- `GetUpgradeCost(string statName) -> int` [PlayerProgression.cs]
- `GetMaxLevel() -> int` [PlayerProgression.cs]
- `CanEvolve() -> bool` [PlayerProgression.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [AchievementManager.cs]
- Singleton instance exclusivity maintained via `Awake` guard [PlayerProgression.cs]

## Side Effects
- Event broadcast across decoupled game listeners [AchievementManager.cs]
- Event broadcast across decoupled game listeners [PlayerProgression.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [AchievementManager.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [PlayerProgression.cs]
- Persistent disk read/write access via StorageService [AchievementManager.cs]
- Persistent disk read/write access via StorageService [PlayerProgression.cs]
- Synchronous asset loading from Resources directory [PlayerProgression.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

