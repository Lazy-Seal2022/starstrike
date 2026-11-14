# Ship Combat Skill System

* **Author:** NVKQ2022
* **Date:** 2026-11-14

## Overview

A modular active and passive combat skill system for StarStrike ships, starting with a single-player slice consisting of 1 active skill slot and 1 passive skill slot. The active ability executes instantly along the ship's forward facing direction without manual aiming reticles or dragging, and is dual-gated by a cooldown timer and a hard energy requirement. Passive abilities hook into gameplay lifecycle events to trigger situational defensive, offensive, or mobility benefits. The underlying data model uses ScriptableObjects and a slot array structure designed to scale to 4-ability MOBA kits and Netcode multiplayer without structural rework.

## Analysis

### 1. The Triple-Energy Contention Problem
StarStrike routes three fundamental ship actions through a single shared Energy pool:
- Primary laser firing (`ShipWeapon.cs`, default 6 energy per shot)
- Engine boost (`ShipMovement.cs`, default 25 energy per second)
- Active combat skill (proposed 20–30 energy per cast)

Under a strict hard-block rule where casting fails if current energy is below the skill cost, players in high-stress defensive situations face an all-or-nothing trap: firing the active skill exhausts the energy needed to boost away from incoming collisions or laser barrages, while boosting disables the ability to cast the defensive/burst skill. 

To eliminate this deadlock without removing resource costs, two inventive mechanisms are proposed:
1. **Recoil Impulse (Newtonian Reaction)**: Because casting fires strictly along the ship's forward heading, the skill applies a reverse physical impulse (`rb.AddForce(-transform.up * recoilImpulse)`). If a player faces backwards while fleeing from pursuers, firing the skill blasts the pursuers and accelerates the ship forward without expending thruster boost energy.
2. **Conditional Energy Rebate**: Hitting multiple targets (such as 2 or more hostiles, or an asteroid) refunds 50% to 75% of the cast cost, converting the active skill into a tactical skill-shot rather than a pure drain.

### 2. Heading Vector vs. Velocity Vector Drift
In StarStrike's 2D physics model, ship velocity rarely matches ship orientation due to linear and angular damping. Casting forward along `transform.up` (or ship heading) decouples ability delivery from the flight vector. While this enables kiting techniques, it can cause narrow projectiles to miss when drifting sideways unless:
- The active projectile has a wide hit volume (such as a conical energy wave or linear piercing rail beam).
- A brief (0.08s) telegraph line or muzzle flare visually clarifies the launch vector.

### 3. Sensory Feedback & Lockout Disambiguation
With dual gating (cooldown AND energy), players who attempt to cast an unavailable skill must receive distinct sensory signals:
- **Cooldown Block**: The HUD skill icon displays a radial clock wipe and numeric second countdown.
- **Energy Starvation Block**: The HUD skill border pulses red, the main energy bar flashes yellow, and an error audio cue (low negative buzz) plays.

### 4. Structural Architecture & Layer Invariants
The system adheres to StarStrike assembly layering (`Core` -> `Platform` -> `Gameplay` -> `UI`):
- `StarStrike.Core`:
  - `SkillDefinition.cs`: ScriptableObject storing skill metadata (id, name, icon, energyCost, cooldownDuration, targetSlotType).
  - `ISkillBehavior.cs`: Execution interface (`void Execute(GameObject caster, Vector2 origin, Vector2 direction)`).
  - `SkillEvents.cs`: Event payload classes (`SkillCastEvent`, `SkillCooldownEvent`, `SkillBlockedEvent`).
- `StarStrike.Gameplay`:
  - `ShipSkillController.cs`: Component attached to the ship GameObject. Tracks cooldown timers, checks energy via `ShipHealth`, invokes behaviors, and manages slot arrays.
  - Concrete behaviors: `ForwardShockwaveBehavior.cs` (Active), `ReactiveBarrierPassiveBehavior.cs` (Passive).
- `StarStrike.UI`:
  - `HUDSkillSlotView.cs`: Visual controller handling cooldown radial fill, energy readiness state, and touch/keyboard input triggers.

## Goals vs. Non-Goals

### In-Scope Goals
- **1 Active Slot + 1 Passive Slot**: A functional MVP ship ability system for local single-player mode.
- **Forward-Cast Execution**: Instant activation along the ship's current heading vector without drag-to-aim reticles.
- **Dual-Gated Resource Validation**: Enforcement of cooldown timers and hard energy requirement checks before execution.
- **Event-Driven Passives**: Passive abilities triggered via decoupled event hooks (e.g., `ShipHealth.OnShieldChanged`, `ShipHealth.OnDamageTaken`, or `ShipWeapon.OnHitEnemy`).
- **Input Integration**: Keybind support on desktop (`E` key or right-click) and dedicated on-screen touch button on mobile.
- **Extensible Slot Data Model**: Architecture using an indexed slot container (`SkillSlot[]`) allowing future expansion to 4 active abilities and 2 passives.

### Out-of-Scope Non-Goals
- **Multiplayer Netcode Synchronization**: Local single-player implementation first; RPC synchronization (`ServerRpc` / `ClientRpc`) is deferred to a future milestone.
- **Drag-to-Aim Reticles & Targeting Indicators**: No on-screen trajectory cones, circular placement reticles, or multi-step targeting interfaces.
- **Multi-Skill MOBA Kits (4 Actives)**: The initial vertical slice evaluates a single active and passive pair before building full ship archetype kits.
- **Skill Tree Progression & Level-Up Upgrades**: Skills are defined per ship loadout; in-run skill upgrading is excluded from the initial slice.

## Alternatives Weighed

- **Energy-Free, Pure Cooldown Gating**:
  - *Description*: Active skills only rely on cooldown timers, completely ignoring the ship's energy pool (similar to many MOBA ability models).
  - *Reason for Rejection*: Rejected by author. Dual-gating with energy forces resource management decisions between boosting, primary fire, and casting, maintaining cohesion with the existing core flight loop.
- **Drag-to-Aim Mobile Skill Controls**:
  - *Description*: Emulating Arena of Valor or Wild Rift touch controls where holding a skill button reveals a targeting reticle and dragging aims the vector.
  - *Reason for Rejection*: Rejected by author for the initial slice. StarStrike's right thumb controls already handle primary fire and directional aim; adding drag-to-aim creates control contention and forces flight drift while aiming.
- **Hardcoding Active & Passive into `ShipWeapon.cs`**:
  - *Description*: Adding skill methods directly into the existing primary weapon script to avoid creating new classes.
  - *Reason for Rejection*: Violates single-responsibility and component modularity. Tightly couples primary pulse lasers with situational skills, complicating testability and slot extensibility.

## Key Decisions & Staging for Next Steps

1. **Dedicated Controller Component**: Implement `ShipSkillController.cs` separate from `ShipWeapon.cs` and `ShipMovement.cs`.
2. **MVP Active & Passive Pairing**:
   - **Active 1 (Forward Impulse Shockwave)**: High-speed piercing energy blast dealing 40 damage, knocking back asteroids, and providing a slight reverse recoil impulse to the ship (Cooldown: 6.0s, Energy: 20).
   - **Passive 1 (Reactive Barrier)**: When ship shield drops below 25%, automatically triggers an omnidirectional kinetic pulse clearing hostile projectiles and knocking back nearby enemies (Cooldown: 25.0s).
3. **HUD and Audio Feedback for Lockout**: Implement distinct visual and acoustic cues when casting fails due to energy starvation versus cooldown expiration.
4. **Staging for `/egm:egm-draft`**: Once user feedback on energy contention and mobile button placement is confirmed, draft the formal design document for `ShipSkillController` and `HUDSkillSlotView`.
