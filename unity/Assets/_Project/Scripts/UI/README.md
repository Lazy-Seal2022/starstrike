# UI

## Overview
User interface presentation layer, screens, flight HUD, controls, and holographic theme styling.

## Subdirectories & Boundaries
- **[Common](./Common/README.md)**: Holographic pedestals, vector themes, event system singletons, and UI helpers.
- **[Screens](./Screens/README.md)**: Full-screen view controllers (`MainMenuUI`, `ShipSelectUI`, `EquipmentSelectUI`, `SettingsUI`, `NetworkLobbyUI`, `NetworkScoreboardUI`) orchestrated by `ScreenManager`.
- **[HUD](./HUD/README.md)**: In-game flight HUD, status gauges, upgrade drawer, floating combat text, minimap radar, and carrier deck overlay.
- **[Controls](./Controls/README.md)**: Reusable on-screen touch joysticks, touch buttons, and safe-area padding.

## Invariants
- UI screens implement `IScreenController` and register with `ScreenManager`.
- HUD overlays query or subscribe to game events, never mutating simulation state directly.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
