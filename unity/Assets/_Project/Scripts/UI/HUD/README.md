# HUD

## Overview
In-game flight HUD, status gauges, upgrade drawer, floating combat text, and touch joystick.

## Interface
- `class AchievementUI` [AchievementUI.cs]
- `class CanvasHUD` [CanvasHUD.cs]
- `SetHUDVisible(bool visible) -> void` [CanvasHUD.cs]
- `ToggleUpgrades() -> void` [CanvasHUD.cs]
- `RebuildEvolutionButtons(PlayerProgression player) -> void` [CanvasHUD.cs]
- `class HUDEndGameModal` [HUDEndGameModal.cs]
- `Show(RunSummary summary) -> void` [HUDEndGameModal.cs]
- `SetVisible(bool visible) -> void` [HUDEndGameModal.cs]
- `OnClickPlayAgain() -> void` [HUDEndGameModal.cs]
- `OnClickReturnToHangar() -> void` [HUDEndGameModal.cs]
- `class HUDEvolutionBanner` [HUDEvolutionBanner.cs]
- `SetBannerVisible(bool visible) -> void` [HUDEvolutionBanner.cs]
- `RebuildEvolutionButtons(PlayerProgression player) -> void` [HUDEvolutionBanner.cs]
- `class HUDFloatingText` [HUDFloatingText.cs]
- `SpawnWorldDamage(Vector3 worldPos, float damage, bool isCrit = false) -> void` [HUDFloatingText.cs]
- `SpawnWorldNotice(Vector3 worldPos, string message, Color color, float fontSize = 16f, bool isCrit = false) -> void` [HUDFloatingText.cs]
- `SpawnFloatingNotice(string message, Color color) -> void` [HUDFloatingText.cs]
- `class HUDManager` [HUDManager.cs]
- `class HUDPlayerStatus` [HUDPlayerStatus.cs]
- `ApplyThemeStyles() -> void` [HUDPlayerStatus.cs]
- `class HUDSkillSlotView` [HUDSkillSlotView.cs]
- `enum TouchButtonAction` [TouchButton.cs]

## Invariants
- Mandatory component dependency enforced via `RequireComponent` [SafeArea.cs]
- MonoBehaviour lifecycle contracts strictly preserved [AchievementUI.cs]
- MonoBehaviour lifecycle contracts strictly preserved [HUDEvolutionBanner.cs]
- MonoBehaviour lifecycle contracts strictly preserved [HUDUpgradeDrawer.cs]
- MonoBehaviour lifecycle contracts strictly preserved [TouchButton.cs]
- Singleton instance exclusivity maintained via `Awake` guard [CanvasHUD.cs]
- Singleton instance exclusivity maintained via `Awake` guard [HUDEndGameModal.cs]
- Singleton instance exclusivity maintained via `Awake` guard [HUDManager.cs]
- Singleton instance exclusivity maintained via `Awake` guard [VirtualJoystick.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [CanvasHUD.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [HUDFloatingText.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [HUDManager.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [HUDPlayerStatus.cs]
- Values clamped within operational bounds via `Mathf.Clamp` [VirtualJoystick.cs]

## Side Effects
- Event broadcast across decoupled game listeners [AchievementUI.cs]
- Event broadcast across decoupled game listeners [CanvasHUD.cs]
- Event broadcast across decoupled game listeners [HUDEndGameModal.cs]
- Event broadcast across decoupled game listeners [HUDEvolutionBanner.cs]
- Event broadcast across decoupled game listeners [HUDFloatingText.cs]
- Event broadcast across decoupled game listeners [HUDManager.cs]
- Event broadcast across decoupled game listeners [HUDPlayerStatus.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [AchievementUI.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [CanvasHUD.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [HUDEndGameModal.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [HUDEvolutionBanner.cs]
- GameObject hierarchy mutation via Instantiate/Destroy [HUDFloatingText.cs]
- In-memory state manipulation [SafeArea.cs]
- In-memory state manipulation [TouchButton.cs]
- In-memory state manipulation [VirtualJoystick.cs]
- Persistent disk read/write access via StorageService [HUDEndGameModal.cs]
- Synchronous asset loading from Resources directory [HUDEndGameModal.cs]
- Synchronous asset loading from Resources directory [HUDEvolutionBanner.cs]
- Synchronous asset loading from Resources directory [HUDFloatingText.cs]
- Synchronous asset loading from Resources directory [HUDPlayerStatus.cs]
- Synchronous asset loading from Resources directory [HUDUpgradeDrawer.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

