# Bootstrap

## Overview
Application composition root, service initialization, and dependency wiring.

## Interface
- `class GameBootstrapper` [GameBootstrapper.cs]
- `Initialize() -> void` [GameBootstrapper.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [GameBootstrapper.cs]

## Side Effects
- GameObject hierarchy mutation via Instantiate/Destroy [GameBootstrapper.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

