# Asteroids

## Overview
Configurations, tier definitions, fracture split counts, and resource drop probabilities for breakable asteroids.

## Interface
- `enum AsteroidTier` [AsteroidConfig.cs]
  - `Large`, `Medium`, `Small`
- `class AsteroidTierData` [AsteroidConfig.cs]
  - `maxHp: float`
  - `gemDropCount: int`
  - `visualScale: float`
  - `colliderRadius: float`
- `class AsteroidConfig` [AsteroidConfig.cs]
  - `largeTier: AsteroidTierData`
  - `mediumTier: AsteroidTierData`
  - `smallTier: AsteroidTierData`
  - `powerUpDropChance: float`
  - `fractureSplitCount: int`
  - `minChildSpeed: float`
  - `maxChildSpeed: float`
  - `GetTierData(AsteroidTier tier) -> AsteroidTierData`

## Invariants
- Tier data defines strictly non-negative health and drop counts.
- `fractureSplitCount >= 0`.

## Side Effects
- Read-only balance configurations used by spawner and asteroid collision systems.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
