# Utils

## Overview
Juice curves, mathematical easing formulas, and geometric helpers.

## Interface
- `class Ease` [Ease.cs]
- `Linear(float t) -> float` [Ease.cs]
- `OutQuad(float t) -> float` [Ease.cs]
- `OutCubic(float t) -> float` [Ease.cs]
- `OutBack(float t) -> float` [Ease.cs]
- `Tween(float duration, System.Action<float> apply, System.Func<float, float> ease, bool unscaled = true) -> IEnumerator` [Ease.cs]

## Invariants
- Values clamped within operational bounds via `Mathf.Clamp` [Ease.cs]

## Side Effects
- Event broadcast across decoupled game listeners [Ease.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

