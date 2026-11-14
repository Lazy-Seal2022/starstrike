# Combat

## Overview
Combat mechanics parameters, laser projectile configurations, weapon system defaults, enemy definitions, and multi-phase boss encounter configurations.

## Interface
- `class CombatConfig` [CombatConfig.cs]
  - `defaultPulseDamage: float`
  - `defaultPulseSpeed: float`
  - `projectileLifetime: float`
  - `laserColliderRadius: float`
  - `defaultFireRate: float`
  - `minFireRate: float`
  - `defaultEnergyCost: float`
  - `recoilForce: float`
  - `muzzleForwardOffset: float`
  - `dualWeaponSpread: float`
- `class EnemyDefinition` [EnemyDefinition.cs]
  - `id: string`
  - `enemyName: string`
  - `maxShield, shieldRegen, speed, agility: float`
  - `pulseDamage, pulseSpeed, fireRate: float`
  - `sightRadius, attackRadius: float`
  - `sprite: Sprite`
- `class BossConfig` [BossConfig.cs]
  - `spawnDelaySeconds: float`
  - `maxShield, shieldRegen, maxSpeed, acceleration, agility: float`
  - `pulseDamage, pulseSpeed, fireRate: float`
  - `weaponSlots: int`
  - `phase1Threshold, phase2Threshold, phase3Threshold: float`

## Invariants
- Minimum fire rate clamped to `0.10f` or higher to prevent unbounded firing loops.
- Boss phase thresholds strictly descend (`1.0 > phase1 > phase2 > phase3 > 0.0`).

## Side Effects
- Read-only ScriptableObject configurations referenced by projectile controllers, combat AI, and boss controllers.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
