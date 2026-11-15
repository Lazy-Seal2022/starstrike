# Assets

## Overview
Asynchronous content delivery pipeline built on Unity Addressables with synchronous fallback and lifecycle release management.

## Components
- `AddressableAssetProvider.cs`: Production asynchronous loader using Unity Addressables (`Addressables.LoadAssetAsync`, `Addressables.InstantiateAsync`, `Addressables.Release`).
- `DirectAssetProvider.cs`: Synchronous and mockable fallback asset provider using `Resources.Load` and test mocks. Ensures EditMode unit testing runs without depending on uninitialized Addressables runtime.
- `AssetService.cs`: Global facade and service locator providing simple async/sync accessors (`AssetService.LoadAssetAsync`, `AssetService.LoadAsset`, `AssetService.ReleaseAsset`).

## Verification
- Unit Tests: `AssetServiceTests.cs` (EditMode)
- Editor Tooling: `StarStrike > Addressables > Configure Addressables Pipeline`
