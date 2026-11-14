# Mining

## Overview
Procedural asteroid fields, destructible rocks, gem drops, and powerups.

## Interface
- `class Asteroid` [Asteroid.cs]
- `SetupStats() -> void` [Asteroid.cs]
- `TakeDamage(float amount) -> void` [Asteroid.cs]
- `class GemCrystal` [GemCrystal.cs]
- `class PowerUpItem` [PowerUpItem.cs]
- `enum PowerUpType` [PowerUpItem.cs]
- `ApplyPowerUp(GameObject player) -> void` [PowerUpItem.cs]
- `class WeaponPickupItem` [WeaponPickupItem.cs]
- `enum WeaponPickupType` [WeaponPickupItem.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [Asteroid.cs]
- Mandatory component dependency enforced via `RequireComponent` [GemCrystal.cs]
- Mandatory component dependency enforced via `RequireComponent` [PowerUpItem.cs]
- Singleton instance exclusivity maintained via `Awake` guard [Asteroid.cs]

## Side Effects
- Event broadcast across decoupled game listeners [Asteroid.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [Asteroid.cs]
- Synchronous asset loading from Resources directory [Asteroid.cs]
- Synchronous asset loading from Resources directory [GemCrystal.cs]
- Synchronous asset loading from Resources directory [PowerUpItem.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

