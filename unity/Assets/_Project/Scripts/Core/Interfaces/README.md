# Interfaces

## Overview
Pure interface abstractions enforcing OOP polymorphism across damage handling, skill execution, weapon systems, energy consumption, power-ups, projectiles, pooling, and input providers.

## Interface
- `interface IDamageable` [IDamageable.cs]
  - `TakeDamage(float amount) -> void`
- `interface ISkillBehavior` [ISkillBehavior.cs]
  - `Execute(GameObject caster, SkillDefinition definition) -> void`
- `interface IPoolable` [IPoolable.cs]
  - `OnSpawned() -> void`
  - `OnDespawned() -> void`
- `interface IWeapon` [IWeapon.cs]
  - `WeaponName -> string`
  - `FireRate -> float`
  - `EnergyCost -> float`
  - `WeaponSlots -> int`
  - `TryFire(IEnergyConsumer energySource) -> bool`
  - `StopFiring() -> void`
  - `ResetCooldown() -> void`
- `interface IEnergyConsumer` [IWeapon.cs]
  - `EnergyCost -> float`
  - `HasEnergy(float availableEnergy) -> bool`
  - `ConsumeEnergy(ref float currentEnergy) -> bool`
- `interface IPowerUpEffect` [IPowerUpEffect.cs]
  - `Apply(GameObject target, LootConfig config) -> void`
- `interface IProjectile` [IProjectile.cs]
  - `Initialize(Vector2 velocity, float damage, float lifetime) -> void`
- `interface IShipInputProvider` [IShipInputProvider.cs]
  - `MovementInput -> Vector2`
  - `IsFiring -> bool`
  - `SkillInputTriggered(SkillSlotType slot) -> bool`

## Invariants
- All interfaces remain pure contracts with zero direct dependencies on concrete implementations or scene Singletons.
- Interface methods preserve exception safety and idempotency where applicable.

## Side Effects
- State changes determined by concrete implementors.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
