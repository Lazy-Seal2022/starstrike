# Gameplay

## Overview
Gameplay systems, session lifecycle management, wave orchestration, ship mechanics, combat, environment, and juice.

### Subdirectories
- **[Managers](./Managers/README.md)**: Game session lifecycle (`GameManager.cs`), world boundaries (`WorldManager.cs`), and wave pacing (`WaveManager.cs`).
- **[Ships](./Ships/README.md)**: Flight physics (`ShipMovement.cs`), carrier hangar, and drone systems (`CarrierHangarController.cs`, `CarrierEscortDrone.cs`).
- **[Combat](./Combat/README.md)**: Ship durability (`ShipHealth.cs`), weapons, projectiles, and tactical skills.
- **[Environment](./Environment/README.md)**: Asteroids, gem crystals, powerup items, weapon drops, and sector hazard fields.
- **[Pooling](./Pooling/README.md)**: General high-performance object pooling infrastructure (`PoolManager.cs`, `PooledObjectTracker.cs`).
- **[AI](./AI/README.md)**: Authoritative enemy behavior trees, flocking, and multi-phase boss AI.
- **[Audio](./Audio/README.md)**: Audio management, CC0 audio resource loading, and procedural tone synthesis fallback.
- **[FX](./FX/README.md)**: Juice visual effects, camera trauma shake, hit stop, and thruster trails.
- **[Input](./Input/README.md)**: Unity Input System action readers, mouse aim direction, and virtual controls.
- **[Progression](./Progression/README.md)**: Player XP level ladder, ship evolution unlocking, and achievement milestones.
- **[Utils](./Utils/README.md)**: Runtime procedural sprite helpers and rendering tools.
- **[World](./World/README.md)**: Orthographic camera tracking, parallax starfields, and minimap tracking targets.

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard in managers.
- Zero cyclic dependencies between domain subsystems.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
