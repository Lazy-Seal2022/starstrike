# Minimap

## Overview
Tactical radar minimap tracking friendly ships, hostiles, and objectives.

## Interface
- `class MinimapController` [MinimapController.cs]
- `BuildUI() -> void` [MinimapController.cs]
- `RegisterTrackable(MinimapTrackable trackable) -> void` [MinimapController.cs]
- `UnregisterTrackable(MinimapTrackable trackable) -> void` [MinimapController.cs]

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [MinimapController.cs]

## Side Effects
- GameObject hierarchy mutation via Instantiate/Destroy [MinimapController.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

