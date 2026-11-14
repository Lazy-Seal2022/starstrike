# Projectiles

## Overview
Live ammunition and physical projectiles traveling through the 2D physics simulation, detecting targets, and applying damage.

## Interface
- `class LaserProjectile` [LaserProjectile.cs]: Linear pulse laser ammunition.
- `class MissileProjectile` [MissileProjectile.cs]: Autonomous homing missile with AoE detonation.
- `class ShockwaveProjectile` [ShockwaveProjectile.cs]: Expanding circular energy wave.

## Invariants
- All projectiles implement `IProjectile` and `IPoolable`.
- Collision triggers verify target tags (`Player`, `Enemy`, `Boss`, `Asteroid`) before invoking `IDamageable`.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
