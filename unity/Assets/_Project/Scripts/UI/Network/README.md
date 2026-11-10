# Network

## Overview
Multiplayer room lobby interface and live leaderboard scoreboards.

## Interface
- `class NetworkLobbyUI` [NetworkLobbyUI.cs]
- `class NetworkLobbyUI` [NetworkLobbyUI.cs]
- `EnsureUIBuilt() -> void` [NetworkLobbyUI.cs]
- `SetVisible(bool visible) -> void` [NetworkLobbyUI.cs]
- `class NetworkScoreboardUI` [NetworkScoreboardUI.cs]
- `class NetworkScoreboardUI` [NetworkScoreboardUI.cs]
- `EnsureUIBuilt() -> void` [NetworkScoreboardUI.cs]
- `SetVisible(bool visible) -> void` [NetworkScoreboardUI.cs]
- `RefreshLeaderboard() -> void` [NetworkScoreboardUI.cs]

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [NetworkLobbyUI.cs]
- Singleton instance exclusivity maintained via `Awake` guard [NetworkScoreboardUI.cs]

## Side Effects
- Event broadcast across decoupled game listeners [NetworkLobbyUI.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [NetworkLobbyUI.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [NetworkScoreboardUI.cs]
- Persistent disk read/write access via StorageService [NetworkLobbyUI.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

