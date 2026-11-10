# FX

## Overview
Juice visual effects, camera trauma shake, hit stop, and thruster trails.

## Interface
- `class FXService` [FXService.cs]
- `ResetStaticState() -> void` [FXService.cs]
- `CameraShake(float duration, float magnitude) -> void` [FXService.cs]
- `CameraShake(float duration, float magnitude, float frequency) -> void` [FXService.cs]
- `CameraShakeImpulse(Vector2 direction, float force, float duration = 0.2f) -> void` [FXService.cs]
- `TriggerPlayerDamageShake(float damageRatio) -> void` [FXService.cs]
- `ApplyHitStop(float durationSeconds = 0.05f, float targetTimeScale = 0.05f) -> void` [FXService.cs]
- `FlashSprite(SpriteRenderer sr, Color flashColor, float duration = 0.05f) -> void` [FXService.cs]
- `CreateThrustFX() -> GameObject` [FXService.cs]
- `class PostProcessingSetup` [PostProcessingSetup.cs]
- `class ShipThrusterEffect` [ShipThrusterEffect.cs]
- `EnsureInitialized() -> void` [ShipThrusterEffect.cs]
- `SetThrusting(bool thrusting, bool boosting) -> void` [ShipThrusterEffect.cs]

## Invariants
- Camera offset state must be reset between test runs [FXService.cs]
- MonoBehaviour lifecycle contracts strictly preserved [PostProcessingSetup.cs]
- Singleton instance exclusivity maintained via `Awake` guard [FXService.cs]
- Singleton instance exclusivity maintained via `Awake` guard [ShipThrusterEffect.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [FXService.cs]

## Side Effects
- Event broadcast across decoupled game listeners [FXService.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [FXService.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [PostProcessingSetup.cs]
- In-memory state manipulation [ShipThrusterEffect.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

