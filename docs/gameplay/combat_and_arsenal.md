# Combat, Arsenal & Tactical Skills Guide

Combat in **StarStrike** combines precision dual-stick projectile aiming, hitscan beam weapons, seeking ordnance, and active defensive skills against swarming autonomous drones and capital-class boss flagships.

---

## 1. Weapon Arsenal Breakdown

The player's vessel is equipped with 4 distinct weapons managed through the [ShipWeaponController.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ShipWeaponController.cs). Weapons can be swapped mid-flight at zero energy penalty using keys `1`, `2`, `3`, `4`, mouse scroll wheel, or gamepad D-pad.

| Weapon | Type | Damage Profile | Fire Rate | Energy Cost | Recoil Force | Ideal Tactical Role |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Pulse Laser** | Kinetic Energy Bolt | 18 base (scales with ship tier) | 0.22s (4.55/s) | 6.0 Units | 1.5 N | All-around skirmishing, long-range sniper fire, steady sustained DPS. |
| **Scattergun** | Multi-Pellet Spread | 10 per pellet (5-9 pellets) | 0.48s (2.08/s) | 14.0 Units | 3.2 N | Close-range burst damage, shattering dense asteroid fields, instant drone kills. |
| **Homing Missiles** | Guided Ordnance | 35 explosive damage | 0.85s (1.18/s) | 22.0 Units | 2.0 N | Evasive target tracking, fire-and-forget combat while fleeing drone wings. |
| **Continuous Beam** | Continuous Hitscan | 75 DPS (ticks every 0.05s) | Continuous | 22.0 U/s | 0.0 N | Precision melting of capital shields (Dreadnought Boss), zero lead time. |

---

## 2. Weapon In-Depth Mechanics

### Pulse Laser ([ShipWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ShipWeapon.cs))
- **Muzzle Offset Math**: As your ship evolves and adds weapon slots (1 $\rightarrow$ 2 $\rightarrow$ 3), the weapon dynamically distributes muzzle points along the lateral wing axis:
  - 1 Slot: `[0.0]` (center nose)
  - 2 Slots: `[-0.15, +0.15]` (dual wingtip muzzles)
  - 3 Slots: `[-0.30, 0.0, +0.30]` (center + dual wingtip muzzles)
- **Recoil Physics**: Every pulse discharge applies a counter-impulse (`-transform.right * recoilForce`) to the firing craft, which can be leveraged by skilled pilots to initiate quick kiting maneuvers.
- **Orientation**: Projectile sprite bolts are oriented longitudinally along $+X$, aligning with their velocity vector across all $360^\circ$ flight angles.

### Scattergun ([ScattergunWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ScattergunWeapon.cs))
- **Spread Cone**: Discharges a conical fan over a $32^\circ$ arc.
- **Pellet Count**: Base count of 5 pellets, scaling with ship slots up to 9 pellets.
- **Damage Falloff**: Individual pellets despawn after **0.85 seconds**, enforcing close-range combat risk-reward play.

### Homing Missiles ([HomingMissileWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/HomingMissileWeapon.cs))
- **Guidance Radar**: Scans for targets within a 20-unit radius. Prioritizes the Dreadnought Boss, then hostile drone fighters, then large asteroids.
- **Turning Torque**: Guided rockets turn smoothly at **220 deg/s**, allowing them to curl around asteroids and pursue maneuvering targets.

### Continuous Beam Laser ([ContinuousBeamWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ContinuousBeamWeapon.cs))
- **Raycast Geometry**: Evaluates instantaneous 2D physics raycasts up to a maximum range of **16 units**.
- **Tick Evaluation**: Evaluates hits in real time (every 0.05s), delivering immediate floating damage numbers and bypassing target projectile evasion.
- **Energy Demand**: Consumes 22 energy units per second, demanding high Energy Regeneration upgrades or careful burst usage.

---

## 3. Tactical Combat Skills

Skills provide situational utility during extreme combat scenarios and are orchestrated via [ShipSkillController.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Skills/ShipSkillController.cs).

### Active Skill: Forward Shockwave ([ForwardShockwaveBehavior.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Skills/Behaviors/ForwardShockwaveBehavior.cs))
- **Trigger**: Key `E` or Gamepad Button North (Y).
- **Cost**: 20 Energy Units | **Cooldown**: 6.0 seconds.
- **Behavior**:
  - Releases a broad sonic shockwave traveling forward at 24 units/s.
  - Deals **40 kinetic damage** to all enemies caught in its path.
  - Applies a heavy **14 N knockback impulse**, scattering attacking drone swarms.
  - **Defensive Utility**: Instantly neutralizes and destroys all enemy red laser projectiles in its flight path.

### Passive Skill: Deflector Surge / Reactive Barrier ([ReactiveBarrierPassiveBehavior.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Skills/Behaviors/ReactiveBarrierPassiveBehavior.cs))
- **Trigger**: Automated on sustaining heavy shield or hull damage.
- **Cost**: 0 Energy | **Cooldown**: 25.0 seconds.
- **Behavior**:
  - Automatically triggers when shields fall below critical threshold ($< 25\%$).
  - Grants **1.5 seconds of total damage invulnerability**.
  - Instantly recharges **+50 Shield HP**, providing a clutch survival window to boost out of danger.

---

## 4. Hostile Threat Directory

### Autonomous Drone Fighters
- **Hitpoints**: 40 HP Shields.
- **Behavior Loop**:
  - **Patrol**: Roams the asteroid field searching for the player.
  - **Pursuit**: Closes distance toward player within 30 units.
  - **Orbit & Fire**: Maintains a standoff distance of $10\text{--}18$ units, circling the player while discharging red pulse lasers.
- **Reward**: Drops 6 glowing green gem crystals upon destruction.

### Sector Dreadnought Flagship (Boss)
- **Hitpoints**: 2,500 HP Massive Shield Matrix.
- **Armament**: Heavy 3-slot pulse battery firing high-damage plasma bolts (30 damage each).
- **Phased Combat Escalation**:
  - **Phase 1 ($100\%\text{--}75\%$ HP)**: Steady cruising speed, standard 3-shot laser bursts every 0.60s.
  - **Phase 2 ($75\%\text{--}50\%$ HP)**: Acceleration increases by $+25\%$; fire rate accelerates to 0.45s intervals.
  - **Phase 3 ($< 50\%$ HP)**: Dreadnought enters berserk pursuit; fire rate increases to 0.30s; ramming aggression increases.
- **Reward**: Shattering the Dreadnought awards **100 bonus meta-gems** and triggers sector Victory.

---

## 5. Temporary Combat Power-Ups

Shattering asteroids occasionally releases glowing power-up modules:

1. **Shield Overcharge (Cyan Capsule)**:
   - Immediately restores shields to $100\%$.
   - Grants **+100 temporary bonus shield capacity** lasting until depleted by combat impacts.
2. **Weapon Overdrive (Gold Diamond)**:
   - Increases active weapon slots by **+1 muzzle** (clamped to a max of 3 slots).
   - Accelerates weapon fire rate by **$1.5\times$** (clamped to a minimum fire floor of 0.10s to prevent broken frame-rate bursts).
   - Lasts 15 seconds, creating an overwhelming damage window to melt drone swarms.
