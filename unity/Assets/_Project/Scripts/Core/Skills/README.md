# Skills

## Overview
Skill configurations, metadata ScriptableObjects, and slot index enumerations governing active abilities, ultimate attacks, and passive modifiers.

## Interface
- `enum SkillSlotType` [SkillSlotType.cs]
  - `Active1`, `Active2`, `Active3`, `Ultimate`, `Passive1`, `Passive2`
- `class SkillDefinition` [SkillDefinition.cs]
  - `id: string`
  - `skillName: string`
  - `icon: Sprite`
  - `energyCost: float`
  - `cooldownSeconds: float`
  - `slotType: SkillSlotType`
  - `behavior: ISkillBehavior`

## Invariants
- `energyCost >= 0` and `cooldownSeconds >= 0`.
- Slot types categorize ability invocation semantics and UI binding channels.

## Side Effects
- Read-only metadata read by skill controllers and UI displays.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
