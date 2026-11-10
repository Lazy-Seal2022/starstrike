# Settings

## Overview
Modal settings dialogue for audio mixing and customizable flight control schemes.

## Interface
- `class SettingsUI` [SettingsUI.cs]
- `enum SettingsTab` [SettingsUI.cs]
- `EnsureUIBuilt() -> void` [SettingsUI.cs]
- `Open() -> void` [SettingsUI.cs]
- `Close() -> void` [SettingsUI.cs]
- `Toggle() -> void` [SettingsUI.cs]
- `SetVisible(bool visible) -> void` [SettingsUI.cs]
- `SwitchTab(SettingsTab tab) -> void` [SettingsUI.cs]
- `RefreshAllUI() -> void` [SettingsUI.cs]
- `AdjustMasterVolume(float delta) -> void` [SettingsUI.cs]

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [SettingsUI.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [SettingsUI.cs]

## Side Effects
- GameObject hierarchy mutation via Instantiate/Destroy [SettingsUI.cs]
- Persistent disk read/write access via StorageService [SettingsUI.cs]
- Synchronous asset loading from Resources directory [SettingsUI.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

