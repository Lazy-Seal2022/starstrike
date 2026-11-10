# MainMenu

## Overview
Title screen, game mode selection (PvE / PvP), and hangar ship selector.

## Interface
- `class MainMenuUI` [MainMenuUI.cs]
- `EnsureUIBuilt() -> void` [MainMenuUI.cs]
- `SetMenuVisible(bool visible) -> void` [MainMenuUI.cs]

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [MainMenuUI.cs]

## Side Effects
- Event broadcast across decoupled game listeners [MainMenuUI.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [MainMenuUI.cs]
- Persistent disk read/write access via StorageService [MainMenuUI.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

