# StarStrike — Architectural Roadmap & Future Improvements

This document tracks large architectural improvements identified during codebase analysis for scaling the game across Web, Android, iOS, and Windows.

---

## 1. Cross-Platform Input & UI Overhaul (Mobile & Gamepad)
- **Current State**: `ShipController.cs` queries `Keyboard.current`, `Mouse.current`, and `Gamepad.current` directly. Touch controls currently rely on Unity's immediate-mode `OnGUI`, which lacks multi-touch tracking (cannot steer, thrust, and fire concurrently).
- **Target Architecture**:
  - Connect actions directly to `Assets/Settings/InputSystem_Actions.inputactions` (`Player/Move`, `Player/Look`, `Player/Attack`, `Player/Sprint`).
  - Replace `OnGUI` with a modern URP 2D Screen Space Canvas or UI Toolkit interface.
  - Implement an on-screen **Floating Virtual Joystick** for touch steering/thrust and dedicated multi-touch buttons for firing and afterburners.

---

## 2. Decouple Ship Controller (Single Responsibility Principle)
- **Current State**: `ShipController.cs` manages input, physics, weapon cooldowns, shield/energy regeneration, upgrade math, gems inventory, and evolution (~350 lines).
- **Target Architecture**:
  - `ShipMovement2D`: Handles velocity, inertial damping, thrust forces, and angular steering.
  - `ShipWeapons`: Handles barrel offsets, projectile spawning, recoil impulse, and fire rate timing.
  - `ShipHealth`: Manages shield/energy meters, regeneration timers, and damage events.
  - `ShipProgression`: Tracks upgrade levels, gem spending, and evolution state.
  - Reusable across player vessels and future NPC/AI factions.

---

## 3. ScriptableObject-Driven Game Data
- **Current State**: Ship stats and upgrade values are instantiated via static methods (`ShipStats.CreateScout()`, etc.) with hardcoded multipliers.
- **Target Architecture**:
  - Create `ShipDefinitionSO` assets stored in `Assets/_Project/Data/Ships/` (stats, tier, weapon slots, sprite assets, evolution paths).
  - Create `UpgradeLadderSO` assets to configure stat progression curves without modifying code.
  - Enables rapid balance iteration and adding new ships directly in the Unity Inspector.

---

## 4. Combat & Damage Abstraction (`IDamageable`)
- **Current State**: `LaserProjectile.cs` explicitly checks for `Asteroid`, `ShipController`, and `EnemyAI` in `OnTriggerEnter2D`.
- **Target Architecture**:
  - Introduce `IDamageable` in `StarStrike.Core`:
    ```csharp
    public interface IDamageable
    {
        void TakeDamage(float amount, GameObject source);
    }
    ```
  - Any entity (Asteroids, Drones, Bosses, Ships) implements `IDamageable`. `LaserProjectile` simply queries `collision.GetComponent<IDamageable>()`.

---

## 5. High-Performance Object Pooling
- **Current State**: Projectiles, gem drops, and asteroid fragments instantiate and destroy GameObjects at runtime.
- **Target Architecture**:
  - Implement `UnityEngine.Pool.ObjectPool<T>` (built into Unity) for:
    - Player and enemy laser projectiles
    - Gem crystals
    - Hit impact spark VFX
  - Drastically reduces GC allocations and prevents frame drops on mobile and WebGL targets.

---

## 6. Game State & Scene Loop
- **Current State**: Game boots directly into `SampleScene` / `GameScene` without menu flow, pause handling, or game over screens.
- **Target Architecture**:
  - Introduce `GameStateManager` with states: `Boot`, `MainMenu`, `InGame`, `Paused`, `GameOver`.
  - Player death screen with gem payout and restart/respawn flow.
  - Persistent save system in `StarStrike.Platform` to retain total collected gems and unlocked ship tiers.
