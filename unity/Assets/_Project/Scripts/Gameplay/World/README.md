# World

## Overview
Orthographic camera tracking, parallax starfields, and runtime sprite synthesis.

## Interface
- `class CameraController` [CameraController.cs]
- `EnsureInitialized() -> void` [CameraController.cs]
- `SetTarget(Transform newTarget) -> void` [CameraController.cs]
- `SetBaseZoom(float zoom) -> void` [CameraController.cs]
- `GetBaseZoom() -> float` [CameraController.cs]
- `AdjustZoom(float delta) -> void` [CameraController.cs]
- `enum TrackableType` [MinimapTrackable.cs]
- `class MinimapTrackable` [MinimapTrackable.cs]
- `class ParallaxBackground` [ParallaxBackground.cs]
- `class PoolManager` [PoolManager.cs]
- `Initialize() -> void` [PoolManager.cs]
- `Prewarm(string tag, Func<GameObject> factory, int count) -> void` [PoolManager.cs]
- `Despawn(GameObject obj) -> void` [PoolManager.cs]
- `Spawn(string tag, Vector3 position, Quaternion rotation, Func<GameObject> factory) -> GameObject` [PoolManager.cs]
- `NotifyDeactivated(GameObject obj, string tag) -> void` [PoolManager.cs]
- `class PooledObjectTracker` [PooledObjectTracker.cs]
- `class WaveManager` [WaveManager.cs]
- `StartWave(int index) -> void` [WaveManager.cs]
- `class ProceduralSpriteHelper` [ProceduralSpriteHelper.cs]
- `GetShipSprite(string type) -> Sprite` [ProceduralSpriteHelper.cs]
- `GetLaserSprite(bool isEnemy = false) -> Sprite` [ProceduralSpriteHelper.cs]
- `GetAsteroidSprite() -> Sprite` [ProceduralSpriteHelper.cs]
- `GetGemSprite() -> Sprite` [ProceduralSpriteHelper.cs]
- `GetStarSprite() -> Sprite` [ProceduralSpriteHelper.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [CameraController.cs]
- Mandatory component dependency enforced via `RequireComponent` [WorldManager.cs]
- MonoBehaviour lifecycle contracts strictly preserved [MinimapTrackable.cs]
- MonoBehaviour lifecycle contracts strictly preserved [ParallaxBackground.cs]
- MonoBehaviour lifecycle contracts strictly preserved [PoolManager.cs]
- Singleton instance exclusivity maintained via `Awake` guard [CameraController.cs]
- Singleton instance exclusivity maintained via `Awake` guard [WorldManager.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [CameraController.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [ProceduralSpriteHelper.cs]

## Side Effects
- Event broadcast across decoupled game listeners [MinimapTrackable.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [CameraController.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [PoolManager.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [WorldManager.cs]
- In-memory state manipulation [ParallaxBackground.cs]
- Persistent disk read/write access via StorageService [CameraController.cs]
- Synchronous asset loading from Resources directory [ProceduralSpriteHelper.cs]
- Synchronous asset loading from Resources directory [WorldManager.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

