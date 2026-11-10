# Movement

## Overview
Newtonian spaceflight physics, rotational steering, and inertial dampening.

## Interface
- `class ShipMovement` [ShipMovement.cs]
- `InitializePhysics() -> void` [ShipMovement.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [ShipMovement.cs]

## Side Effects
- Synchronous asset loading from Resources directory [ShipMovement.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

