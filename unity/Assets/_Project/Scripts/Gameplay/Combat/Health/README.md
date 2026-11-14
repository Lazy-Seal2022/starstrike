# Health

## Overview
Durability systems managing shield capacity, regeneration, energy pools, and fatal damage events.

## Interface
- `class ShipHealth` [ShipHealth.cs]
  - `ResetMeters() -> void`
  - `RegenerateMeters(float dt) -> void`
  - `TakeDamage(float amount) -> void`
  - `HasEnergy(float amount) -> bool`
  - `ConsumeEnergy(float amount) -> bool`

## Invariants
- Implements `IDamageable` and `IEnergyConsumer`.
- In-memory meter clamping prevents energy or shield underflow.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
