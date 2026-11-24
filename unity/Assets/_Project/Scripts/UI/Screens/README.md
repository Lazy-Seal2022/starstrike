# Screens

## Overview
Top-level UI views and transitions conforming to `IScreenController` and orchestrated by `ScreenManager`.

## Components
- `IScreenController.cs`: Lifecycle contract (`Show`, `Hide`, `ScreenId`).
- `ScreenManager.cs`: Modal and page routing service.
- `MainMenuUI.cs`: Main title screen and game mode entry.
- `ShipSelectUI.cs`: Hangar vessel selection carousel and stats preview.
- `EquipmentSelectUI.cs`: Hardpoint weapon loadout and module customization.
- `SettingsUI.cs`: Audio sliders, flight control toggle, and graphics options.
- `NetworkLobbyUI.cs`: Multiplayer lobby hosting and matchmaking.
- `NetworkScoreboardUI.cs`: Match end leaderboards and live scores.
