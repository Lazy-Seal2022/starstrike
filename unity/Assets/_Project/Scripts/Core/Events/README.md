# Events

## Overview
Type-safe decoupled event bus channels for game-wide notifications.

## Interface
- `class GameEvents` [GameEvents.cs]
- `TriggerShieldChanged(float current, float max) -> void` [GameEvents.cs]
- `TriggerEnergyChanged(float current, float max) -> void` [GameEvents.cs]
- `TriggerGemCollected(int amount, int total) -> void` [GameEvents.cs]
- `TriggerPlayerEvolved(string shipId) -> void` [GameEvents.cs]
- `TriggerAsteroidDestroyed(Vector2 pos, string size) -> void` [GameEvents.cs]
- `TriggerDamageDealt(Vector3 pos, float damage, bool isCrit = false) -> void` [GameEvents.cs]
- `TriggerEnemyDestroyed(string enemyId) -> void` [GameEvents.cs]
- `TriggerBossDefeated() -> void` [GameEvents.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [GameEvents.cs]

## Side Effects
- Event broadcast across decoupled game listeners [GameEvents.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [GameEvents.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

