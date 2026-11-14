# Ships

## Overview
Data definitions, stats, stat value containers, and flight dynamics configurations for playable ships and evolved ship classes.

## Interface
- `class ShipDefinition` [ShipDefinition.cs]
  - `id: string`
  - `shipName: string`
  - `shipClass: ShipClass`
  - `baseStats: ShipStats`
  - `passiveSkills: SkillDefinition[]`
  - `activeSkills: SkillDefinition[]`
- `class ShipStats` [ShipStats.cs]
  - `maxShield: float`
  - `shieldRegen: float`
  - `maxEnergy: float`
  - `energyRegen: float`
  - `speed: float`
  - `agility: float`
  - `pulseDamage: float`
  - `pulseSpeed: float`
  - `fireRate: float`
  - `CreateScout() -> ShipStats`
  - `CreateFighter() -> ShipStats`
  - `CreateMiner() -> ShipStats`
- `struct StatValues` [StatValues.cs]
  - `maxShield, shieldRegen, maxEnergy, energyRegen, speed, agility, pulseDamage, pulseSpeed, fireRate: float`
- `class FlightConfig` [FlightConfig.cs]
  - `acceleration: float`
  - `drag: float`
  - `turnSmoothing: float`
  - `thrustPower: float`

## Invariants
- ScriptableObject asset instances are treated as read-only at runtime; dynamic stat modifications must flow through runtime instances or calculation helpers (`ProgressionMath`).
- `fireRate` minimum clamp enforced at `0.10f` to prevent weapon lockup and division-by-zero.

## Side Effects
- In-memory stat generation and asset configuration lookup.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
