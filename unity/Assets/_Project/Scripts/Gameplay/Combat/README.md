# Combat

## Overview
Player and enemy ship weaponry, projectile collision systems, ship durability, and ability execution.

## Structure
- `ShipHealth.cs`: Shield and hull durability management, damage mitigation, and death dispatch.
- **[Weapons](./Weapons/README.md)**: Weapon implementations (`ShipWeapon`, `ScattergunWeapon`, `HomingMissileWeapon`, `ContinuousBeamWeapon`) and `ShipWeaponController`.
- **[Projectiles](./Projectiles/README.md)**: Physical ammunition bodies (`LaserProjectile`, `MissileProjectile`, `ShockwaveProjectile`).
- **[Skills](./Skills/README.md)**: Active and passive ability controllers and behaviors (`ShipSkillController`, `ForwardShockwaveBehavior`, `ReactiveBarrierPassiveBehavior`).

## Invariants
- Mandatory component dependencies enforced via `RequireComponent`.
- All weapons conform to `IWeapon` and projectiles conform to `IProjectile` and `IPoolable`.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
