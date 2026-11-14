# Skills

## Overview
Skill activation controllers, ability execution behavior strategies, and cooldown management.

## Interface
- `class ShipSkillController` [ShipSkillController.cs]: Evaluates active skills, cooldowns, and triggers.
- `class ForwardShockwaveBehavior` [Behaviors/ForwardShockwaveBehavior.cs]: Active 1 skill spawning forward pulse.
- `class ReactiveBarrierPassiveBehavior` [Behaviors/ReactiveBarrierPassiveBehavior.cs]: Passive skill granting emergency barrier on damage.

## Invariants
- All behaviors implement `ISkillBehavior`.

## Verification
- Run: `Unity Test Runner (EditMode tests in active Unity Editor or via unity-cli)`
