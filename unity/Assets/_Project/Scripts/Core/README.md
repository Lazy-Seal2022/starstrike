# Core

## Overview
StarStrike foundational architectural primitives, domain data models, and polymorphic interface abstractions. Adheres to Unity's Layer-First Enterprise Architecture standard (zero scene dependencies).

## Subdirectories
- **[Definitions](./Definitions/README.md)**: All pure C# data models, enumerations, and ScriptableObject schemas (weapons, ships, enemies, asteroids, loot, game modes).
- **[Interfaces](./Interfaces/README.md)**: Pure interface definitions decoupling gameplay logic, damage handling, weapon systems, skills, pooling, and input providers.
- **[Events](./Events/README.md)**: Type-safe decoupled event bus channels and payload structs for game-wide notifications.
- **[Progression](./Progression/README.md)**: Mathematical upgrade curves, stat multipliers, and player upgrade state.
- **[Utils](./Utils/README.md)**: Juice curves, mathematical easing formulas, and geometric helpers.

## Invariants
- All models, configs, and interfaces preserve the `StarStrike.Core` namespace for complete backward compatibility.
- Assemblies consuming `StarStrike.Core.asmdef` require no namespace restructuring.
- Core remains strictly independent of `Gameplay`, `UI`, and `Network`.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
