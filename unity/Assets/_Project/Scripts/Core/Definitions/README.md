# Definitions

## Overview
Pure C# data models, enumerations, stat value containers, and ScriptableObject schemas for weapons, ships, enemies, asteroids, loot, and game modes.

## Subdirectories
- **Combat/**: `CombatConfig.cs`, `EnemyDefinition.cs`, `BossConfig.cs`
- **Ships/**: `ShipDefinition.cs`, `ShipStats.cs`, `StatValues.cs`, `FlightConfig.cs`
- **Skills/**: `SkillDefinition.cs`, `SkillSlotType.cs`
- **Asteroids/**: `AsteroidConfig.cs`
- **Loot/**: `LootConfig.cs`
- **Game/**: `GameConfig.cs`, `GameMode.cs`, `RunSummary.cs`, `UIThemeConfig.cs`
- **Audio/**: `AudioConfig.cs`
- **Waves/**: `WaveDefinition.cs`, `WaveConfig.cs`, `SpawnEntry.cs`

## Invariants
- All classes remain data-pure with zero dependencies on scene GameObjects or MonoBehaviours.
- ScriptableObject asset instances are treated as read-only at runtime.
- Preserves `namespace StarStrike.Core` for complete backwards compatibility.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
