# Core

## Overview
StarStrike foundational architectural primitives and damage interfaces.

## Interface
- `interface IDamageable` [IDamageable.cs]
- `interface ISkillBehavior` [ISkillBehavior.cs]

### Subdirectories
- **[Events](./Events/README.md)**: Type-safe decoupled event bus channels for game-wide notifications.
- **[Models](./Models/README.md)**: Data definitions, ScriptableObject configurations, and gameplay stats.
- **[Progression](./Progression/README.md)**: Mathematical formulas for upgrade curves and player progression state.
- **[Utils](./Utils/README.md)**: Juice curves, mathematical easing formulas, and geometric helpers.

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [IDamageable.cs]

## Side Effects
- In-memory state manipulation [IDamageable.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

