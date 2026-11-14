# Input

## Overview
Unity Input System action readers, mouse aim direction, and virtual controls.

## Interface
- `class PlayerInputReader` [PlayerInputReader.cs]
- `ApplySettings() -> void` [PlayerInputReader.cs]
- `SetThrust(bool value) -> void` [PlayerInputReader.cs]
- `SetBrake(bool value) -> void` [PlayerInputReader.cs]
- `SetBoost(bool value) -> void` [PlayerInputReader.cs]
- `SetFire(bool value) -> void` [PlayerInputReader.cs]
- `SetAim(Vector2 dir) -> void` [PlayerInputReader.cs]
- `ResetInputState() -> void` [PlayerInputReader.cs]
- `ProcessFiring(float dt) -> void` [PlayerInputReader.cs]
- `TriggerSkill(SkillSlotType slot) -> void` [PlayerInputReader.cs]

## Invariants
- Singleton instance exclusivity maintained via `Awake` guard [PlayerInputReader.cs]

## Side Effects
- GameObject hierarchy mutation via Instantiate/Destroy [PlayerInputReader.cs]
- Persistent disk read/write access via StorageService [PlayerInputReader.cs]

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`

