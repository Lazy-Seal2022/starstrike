# Network

## Overview
Multiplayer networking, client interpolation, and server-authoritative state synchronization.

## Interface
- `class NetworkLaserProjectile` [NetworkLaserProjectile.cs]
- `OnNetworkSpawn() -> void` [NetworkLaserProjectile.cs]
- `class NetworkMatchManager` [NetworkMatchManager.cs]
- `OnNetworkSpawn() -> void` [NetworkMatchManager.cs]
- `OnNetworkDespawn() -> void` [NetworkMatchManager.cs]
- `RegisterPlayer(ulong clientId, string playerName, string shipId) -> void` [NetworkMatchManager.cs]
- `SubmitPilotProfileServerRpc(ulong clientId, FixedString32Bytes pilotName, FixedString32Bytes shipId) -> void` [NetworkMatchManager.cs]
- `GetSpawnPoint(ulong clientId, int totalSlots = 8, float radius = 35f) -> Vector2` [NetworkMatchManager.cs]
- `RecordKill(ulong killerClientId, ulong victimClientId) -> void` [NetworkMatchManager.cs]
- `DisconnectAndReturnToMenu() -> void` [NetworkMatchManager.cs]
- `class NetworkPlayerShip` [NetworkPlayerShip.cs]
- `OnNetworkSpawn() -> void` [NetworkPlayerShip.cs]
- `OnNetworkDespawn() -> void` [NetworkPlayerShip.cs]
- `ApplyNetworkDamage(float amount, ulong attackerClientId) -> void` [NetworkPlayerShip.cs]
- `struct NetworkScoreEntry` [NetworkScoreEntry.cs]
- `Equals(NetworkScoreEntry other) -> bool` [NetworkScoreEntry.cs]
- `Equals(object obj) -> bool` [NetworkScoreEntry.cs]
- `GetHashCode() -> int` [NetworkScoreEntry.cs]
- `class NetworkSessionData` [NetworkSessionData.cs]
- `GenerateJoinCode(ushort port = 7777) -> string` [NetworkSessionData.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [NetworkLaserProjectile.cs]
- Mandatory component dependency enforced via `RequireComponent` [NetworkPlayerShip.cs]
- MonoBehaviour lifecycle contracts strictly preserved [NetworkScoreEntry.cs]
- MonoBehaviour lifecycle contracts strictly preserved [NetworkSessionData.cs]
- Singleton instance exclusivity maintained via `Awake` guard [NetworkMatchManager.cs]
- Singleton instance exclusivity maintained via `Awake` guard [NetworkPlayerShip.cs]
- State mutation restricted to authoritative server execution [NetworkLaserProjectile.cs]
- State mutation restricted to authoritative server execution [NetworkMatchManager.cs]
- State mutation restricted to authoritative server execution [NetworkPlayerShip.cs]
- State mutation restricted to authoritative server execution [NetworkSessionManager.cs]

## Side Effects
- Event broadcast across decoupled game listeners [NetworkMatchManager.cs]
- Event broadcast across decoupled game listeners [NetworkPlayerShip.cs]
- Event broadcast across decoupled game listeners [NetworkSessionManager.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [NetworkMatchManager.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [NetworkPlayerShip.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [NetworkSessionManager.cs]
- In-memory state manipulation [NetworkLaserProjectile.cs]
- In-memory state manipulation [NetworkScoreEntry.cs]
- In-memory state manipulation [NetworkSessionData.cs]
- Synchronous asset loading from Resources directory [NetworkPlayerShip.cs]
- Synchronous asset loading from Resources directory [NetworkSessionManager.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

