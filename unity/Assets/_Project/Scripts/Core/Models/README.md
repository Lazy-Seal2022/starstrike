# Models

## Overview
Data definitions, ScriptableObject configurations, and gameplay stats.

## Interface
- `enum AsteroidTier` [AsteroidConfig.cs]
- `class AsteroidTierData` [AsteroidConfig.cs]
- `class AsteroidConfig` [AsteroidConfig.cs]
- `GetTierData(AsteroidTier tier) -> AsteroidTierData` [AsteroidConfig.cs]
- `class BossConfig` [BossConfig.cs]
- `class CombatConfig` [CombatConfig.cs]
- `class EnemyDefinition` [EnemyDefinition.cs]
- `class FlightConfig` [FlightConfig.cs]
- `class GameConfig` [GameConfig.cs]
- `enum GameMode` [GameMode.cs]
- `class LootConfig` [LootConfig.cs]
- `struct RunSummary` [RunSummary.cs]
- `CalculateScore(int gems, int asteroids, int enemies, bool bossKilled, float time) -> int` [RunSummary.cs]
- `Max(int a, int b) -> int` [RunSummary.cs]
- `class ShipDefinition` [ShipDefinition.cs]
- `class ShipStats` [ShipStats.cs]
- `CreateScout() -> ShipStats` [ShipStats.cs]
- `CreateFighter() -> ShipStats` [ShipStats.cs]
- `CreateMiner() -> ShipStats` [ShipStats.cs]
- `struct StatValues` [StatValues.cs]
- `enum SkillSlotType` [SkillSlotType.cs]
- `class SkillDefinition` [SkillDefinition.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [AsteroidConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [BossConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [CombatConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [EnemyDefinition.cs]
- MonoBehaviour lifecycle contracts strictly preserved [FlightConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [GameConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [GameMode.cs]
- MonoBehaviour lifecycle contracts strictly preserved [LootConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [RunSummary.cs]
- MonoBehaviour lifecycle contracts strictly preserved [ShipDefinition.cs]
- MonoBehaviour lifecycle contracts strictly preserved [ShipStats.cs]
- MonoBehaviour lifecycle contracts strictly preserved [StatValues.cs]
- MonoBehaviour lifecycle contracts strictly preserved [UIThemeConfig.cs]
- MonoBehaviour lifecycle contracts strictly preserved [UpgradeDefinition.cs]

## Side Effects
- GameObject hierarchy mutation via Instantiate/Destroy [RunSummary.cs]
- In-memory state manipulation [AsteroidConfig.cs]
- In-memory state manipulation [BossConfig.cs]
- In-memory state manipulation [CombatConfig.cs]
- In-memory state manipulation [EnemyDefinition.cs]
- In-memory state manipulation [FlightConfig.cs]
- In-memory state manipulation [GameConfig.cs]
- In-memory state manipulation [GameMode.cs]
- In-memory state manipulation [LootConfig.cs]
- In-memory state manipulation [ShipDefinition.cs]
- In-memory state manipulation [ShipStats.cs]
- In-memory state manipulation [StatValues.cs]
- In-memory state manipulation [UIThemeConfig.cs]
- In-memory state manipulation [UpgradeDefinition.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

