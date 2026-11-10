# Gameplay

## Overview
Session lifecycle manager, wave orchestration, and match state machine.

## Interface
- `enum GameState` [GameManager.cs]
- `class GameManager` [GameManager.cs]
- `SetGameMode(GameMode mode) -> void` [GameManager.cs]
- `ChangeState(GameState newState) -> void` [GameManager.cs]
- `StartGame() -> void` [GameManager.cs]
- `PauseGame() -> void` [GameManager.cs]
- `ResumeGame() -> void` [GameManager.cs]
- `GameOver() -> void` [GameManager.cs]
- `Victory() -> void` [GameManager.cs]
- `RestartGame() -> void` [GameManager.cs]

### Subdirectories
- **[AI](./AI/README.md)**: Authoritative enemy behavior trees, flocking, and multi-phase boss AI.
- **[Audio](./Audio/README.md)**: Audio management, CC0 audio resource loading, and procedural tone synthesis fallback.
- **[Combat](./Combat/README.md)**: Player and enemy ship weaponry, projectile collision, and shield/health systems.
- **[FX](./FX/README.md)**: Juice visual effects, camera trauma shake, hit stop, and thruster trails.
- **[Input](./Input/README.md)**: Unity Input System action readers, mouse aim direction, and virtual controls.
- **[Mining](./Mining/README.md)**: Procedural asteroid fields, destructible rocks, gem drops, and powerups.
- **[Movement](./Movement/README.md)**: Newtonian spaceflight physics, rotational steering, and inertial dampening.
- **[Progression](./Progression/README.md)**: Player XP level ladder, ship evolution unlocking, and achievement milestones.
- **[World](./World/README.md)**: Orthographic camera tracking, parallax starfields, and runtime sprite synthesis.

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [GameManager.cs]

## Side Effects
- Event broadcast across decoupled game listeners [GameManager.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [GameManager.cs]
- Persistent disk read/write access via StorageService [GameManager.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

