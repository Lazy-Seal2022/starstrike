# Build

## Overview
Automated headless CI build pipeline and asset generation tooling.

## Interface
- `class CIBuilder` [CIBuilder.cs]
- `BuildWindows() -> void` [CIBuilder.cs]
- `BuildAndroid() -> void` [CIBuilder.cs]
- `BuildWebGL() -> void` [CIBuilder.cs]
- `BuildIOS() -> void` [CIBuilder.cs]
- `BuildAll() -> void` [CIBuilder.cs]
- `class DataInitializer` [DataInitializer.cs]
- `InitializeData() -> void` [DataInitializer.cs]
- `class PrefabInitializer` [PrefabInitializer.cs]
- `GeneratePrefabs() -> void` [PrefabInitializer.cs]
- `class SpriteAssetGenerator` [SpriteAssetGenerator.cs]
- `GenerateAllSpritesAndAtlas() -> void` [SpriteAssetGenerator.cs]

## Invariants
- MonoBehaviour lifecycle contracts strictly preserved [CIBuilder.cs]
- MonoBehaviour lifecycle contracts strictly preserved [DataInitializer.cs]
- MonoBehaviour lifecycle contracts strictly preserved [PrefabInitializer.cs]
- MonoBehaviour lifecycle contracts strictly preserved [SpriteAssetGenerator.cs]

## Side Effects
- GameObject hierarchy mutation via Instantiate/Destroy [PrefabInitializer.cs]
- In-memory state manipulation [CIBuilder.cs]
- In-memory state manipulation [DataInitializer.cs]
- In-memory state manipulation [SpriteAssetGenerator.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

