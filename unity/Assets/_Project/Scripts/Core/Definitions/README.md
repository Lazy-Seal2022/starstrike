# Definitions

## Overview
Pure C# data models, enumerations, stat value containers, and ScriptableObject schemas for weapons, ships, enemies, asteroids, loot, and game modes.

## Interface
### Combat & Enemies
- `class CombatConfig` [CombatConfig.cs]: Global weapon balancing, pulse projectile parameters, recoil force.
- `class EnemyDefinition` [EnemyDefinition.cs]: Archetype stats, attack radius, and AI profiles for enemy drones.
- `class BossConfig` [BossConfig.cs]: Multi-phase health thresholds, boss speed, weapon slots, and attack modes.

### Ships & Flight
- `class ShipDefinition` [ShipDefinition.cs]: ScriptableObject defining ship classes, base stats, and starting skill sets.
- `class ShipStats` [ShipStats.cs]: Baseline statistics (shield, energy, speed, agility, pulse damage).
- `struct StatValues` [StatValues.cs]: Immutable value container representing evaluated ship attributes.
- `class FlightConfig` [FlightConfig.cs]: Acceleration, drag, turn smoothing, and thrust physics constants.

### Skills
- `enum SkillSlotType` [SkillSlotType.cs]: Slot channel indexing (`Active1`, `Active2`, `Active3`, `Ultimate`, `Passive1`, `Passive2`).
- `class SkillDefinition` [SkillDefinition.cs]: Ability metadata, cooldown, energy cost, and slot binding.

### World Entities & Loot
- `enum AsteroidTier` [AsteroidConfig.cs]: `Large`, `Medium`, `Small` asteroid categories.
- `class AsteroidTierData` [AsteroidConfig.cs]: Per-tier health, drop count, and visual scale.
- `class AsteroidConfig` [AsteroidConfig.cs]: Fracture split counts, child ejection speed, and drop rates.
- `class LootConfig` [LootConfig.cs]: Gem values, magnet radius, pickup ejection velocities, and power-up bonuses.

### Session & Progression
- `enum GameMode` [GameMode.cs]: `Normal`, `Hardcore`, `Endless`.
- `class GameConfig` [GameConfig.cs]: Global arena bounds, starting meters, and game mode rules.
- `struct RunSummary` [RunSummary.cs]: Post-run statistics and deterministic scoring formula.
- `class UpgradeDefinition` [UpgradeDefinition.cs]: Tier cost multipliers and stat progression definitions.
- `class UIThemeConfig` [UIThemeConfig.cs]: Shared UI styling tokens and palette definitions.

## Invariants
- All classes remain data-pure with zero dependencies on scene GameObjects or MonoBehaviours.
- ScriptableObject asset instances are treated as read-only at runtime.
- Preserves `namespace StarStrike.Core` for complete backwards compatibility.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
