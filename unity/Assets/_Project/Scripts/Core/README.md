# Core

## Overview
StarStrike foundational architectural primitives, domain data models, and polymorphic interface abstractions.

## Subdirectories
- **[Interfaces](./Interfaces/README.md)**: Pure interface definitions decoupling gameplay logic, damage handling, weapon systems, skills, pooling, and input providers.
- **[Ships](./Ships/README.md)**: Playable ship definitions, stat containers, flight dynamics configs, and baseline stats.
- **[Skills](./Skills/README.md)**: Skill metadata ScriptableObjects, ability slot enumerations, and cooldown data.
- **[Asteroids](./Asteroids/README.md)**: Asteroid tier definitions, fracture counts, and drop rate configurations.
- **[Combat](./Combat/README.md)**: Projectile combat parameters, weapon tuning, enemy stats, and multi-phase boss encounter configs.
- **[Objects](./Objects/README.md)**: World loot definitions, gems, and power-up modifiers.
- **[Game](./Game/README.md)**: Game modes, global session configs, run summaries, and UI theme styling tokens.
- **[Progression](./Progression/README.md)**: Mathematical upgrade curves, stat multipliers, and player upgrade state.
- **[Events](./Events/README.md)**: Type-safe decoupled event bus channels for game-wide notifications.
- **[Utils](./Utils/README.md)**: Juice curves, mathematical easing formulas, and geometric helpers.

## Invariants
- All models, configs, and interfaces preserve the `StarStrike.Core` namespace for complete backward compatibility.
- Assemblies consuming `StarStrike.Core.asmdef` require no namespace restructuring.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
