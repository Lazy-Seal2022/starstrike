# Audio

## Overview
Audio management, CC0 audio resource loading, and procedural tone synthesis fallback.

## Interface
- `class AudioService` [AudioService.cs]
- `Initialize() -> void` [AudioService.cs]
- `ApplyMixerRouting() -> void` [AudioService.cs]
- `LoadSettings() -> void` [AudioService.cs]
- `SetMasterVolume(float volume) -> void` [AudioService.cs]
- `SetSfxVolume(float volume) -> void` [AudioService.cs]
- `SetMusicVolume(float volume) -> void` [AudioService.cs]
- `SetMuted(bool muted) -> void` [AudioService.cs]
- `LinearToDecibels(float linear) -> float` [AudioService.cs]

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [AudioService.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [AudioService.cs]

## Side Effects
- Audio playback dispatch to AudioSource/Mixer channel [AudioService.cs]
- Event broadcast across decoupled game listeners [AudioService.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [AudioService.cs]
- Persistent disk read/write access via StorageService [AudioService.cs]
- Synchronous asset loading from Resources directory [AudioService.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

