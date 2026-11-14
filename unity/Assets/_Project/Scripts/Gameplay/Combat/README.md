# Combat

## Overview
Player and enemy ship weaponry, projectile collision, and shield/health systems.

## Interface
- `class LaserProjectile` [LaserProjectile.cs]
- `SetupPhysics() -> void` [LaserProjectile.cs]
- `Initialize(float dmg, float spd, bool enemy) -> void` [LaserProjectile.cs]
- `OnTriggerEnter2D(Collider2D collision) -> void` [LaserProjectile.cs]
- `class ShipHealth` [ShipHealth.cs]
- `ResetMeters() -> void` [ShipHealth.cs]
- `RegenerateMeters(float dt) -> void` [ShipHealth.cs]
- `TakeDamage(float amount) -> void` [ShipHealth.cs]
- `HasEnergy(float amount) -> bool` [ShipHealth.cs]
- `ConsumeEnergy(float amount) -> bool` [ShipHealth.cs]
- `class ShipWeapon` [ShipWeapon.cs]
- `TryFire(IEnergyConsumer energySource) -> bool` [ShipWeapon.cs]
- `TryFire(ShipHealth health) -> bool` [ShipWeapon.cs]
- `ResetCooldown() -> void` [ShipWeapon.cs]
- `class ShipSkillController` [ShipSkillController.cs]
- `TryCastActiveSkill(SkillSlotType slotType) -> bool` [ShipSkillController.cs]
- `class ShockwaveProjectile` [ShockwaveProjectile.cs]
- `class ForwardShockwaveBehavior` [Behaviors/ForwardShockwaveBehavior.cs]
- `class ReactiveBarrierPassiveBehavior` [Behaviors/ReactiveBarrierPassiveBehavior.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [LaserProjectile.cs]
- Singleton instance exclusivity maintained via `Awake` guard [ShipHealth.cs]
- Singleton instance exclusivity maintained via `Awake` guard [ShipWeapon.cs]

## Side Effects
- Event broadcast across decoupled game listeners [LaserProjectile.cs]
- Event broadcast across decoupled game listeners [ShipHealth.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [ShipWeapon.cs]
- Synchronous asset loading from Resources directory [LaserProjectile.cs]
- Synchronous asset loading from Resources directory [ShipWeapon.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

