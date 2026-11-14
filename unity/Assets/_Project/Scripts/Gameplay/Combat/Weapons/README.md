# Weapons

## Overview
Live weapon systems attached to ship actors implementing `IWeapon`, managing fire rate cooldowns, energy costs, and firing patterns.

## Interface
- `class ShipWeapon` [ShipWeapon.cs]: Standard pulse laser cannon.
- `class ScattergunWeapon` [ScattergunWeapon.cs]: Multi-pellet short-range scattergun.
- `class HomingMissileWeapon` [HomingMissileWeapon.cs]: Autonomous seeking missile launcher.
- `class ContinuousBeamWeapon` [ContinuousBeamWeapon.cs]: Sustained raycast energy beam.
- `class ShipWeaponController` [ShipWeaponController.cs]: Equipped weapon inventory and hotkey swapping controller.

## Invariants
- All weapons implement `IWeapon`.
- Non-player weapons (AI) function identically via polymorphic `TryFire`.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
