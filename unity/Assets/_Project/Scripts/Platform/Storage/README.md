# Storage

## Overview
Persistent audio volume, flight controls, and player preference storage.

## Interface
- `class PlayerProfile` [StorageService.cs]
- `struct StatEntry` [StorageService.cs]
- `class StorageService` [StorageService.cs]
- `LoadProfile() -> void` [StorageService.cs]
- `SaveProfile() -> void` [StorageService.cs]
- `SaveHighScore(int score) -> void` [StorageService.cs]
- `GetHighScore() -> int` [StorageService.cs]
- `SaveAudioMuted(bool isMuted) -> void` [StorageService.cs]
- `GetAudioMuted() -> bool` [StorageService.cs]
- `GetMasterVolume() -> float` [StorageService.cs]
- `SaveMasterVolume(float vol) -> void` [StorageService.cs]

## Invariants
- Values clamped within operational bounds via `Mathf.Clamp` [StorageService.cs]

## Side Effects
- Event broadcast across decoupled game listeners [StorageService.cs]
- Persistent disk read/write access via StorageService [StorageService.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

