# Definitions

## Overview
Pure C# data models, enumerations, stat value containers, and ScriptableObject schemas for weapons, ships, enemies, asteroids, loot, and game modes.

## Subdirectories
- **Combat/**: `CombatConfig.cs`, `EnemyDefinition.cs`, `BossConfig.cs`, `WaveDefinition.cs`
- **Ships/**: `ShipDefinition.cs`, `StatValues.cs`, `FlightConfig.cs`, `ShipArchetypeFactory.cs`, `ShipClass.cs`, `EquipmentModule.cs`, `Classes/` (modular class builders)
- **Skills/**: `SkillDefinition.cs`, `SkillSlotType.cs`
- **Environment/**: `AsteroidConfig.cs`, `LootConfig.cs`
- **Game/**: `GameConfig.cs`, `GameMode.cs`, `RunSummary.cs`, `UIThemeConfig.cs`, `AudioConfig.cs`

## Invariants
- All classes remain data-pure with zero dependencies on scene GameObjects or MonoBehaviours.
- ScriptableObject asset instances are treated as read-only at runtime.
- Preserves `namespace StarStrike.Core` for complete backwards compatibility.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
