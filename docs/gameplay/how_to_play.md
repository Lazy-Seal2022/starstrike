# StarStrike Player Manual & How-to-Play Guide

Welcome pilot to the operational manual for **StarStrike**. This guide details controls, flight dynamics, tactical combat, mining economics, and progression strategies necessary to survive hostile space sectors.

---

## 1. Game Objective

In **StarStrike**, you pilot a specialized spacecraft through deep space sectors populated by dense asteroid belts and hostile automated drones. Your primary mission objectives are:
1. **Mine Asteroids**: Blast mineral-rich asteroids to harvest valuable glowing **Gems**.
2. **Upgrade Your Ship**: Spend harvested gems to boost shields, energy, laser power, top speed, and maneuverability.
3. **Evolve into Advanced Hulls**: Maximize your tier stats to evolve from a fragile scout into specialized combat fighters or heavy mining cruisers.
4. **Survive Escalating Waves**: Repel autonomous enemy drone wings and eliminate the sector **Dreadnought Boss** in Wave 3 to achieve sector victory.

---

## 2. Controls & Input Schemes

StarStrike supports three distinct control schemes: **Mouse Aim (Default)**, **Classic Keyboard Steering**, and **Gamepad / Twin-Stick Controller**.

### Keyboard & Mouse Controls (Default)

| Action | Primary Binding | Secondary Binding | Description |
| :--- | :--- | :--- | :--- |
| **Aim Direction** | Mouse Position | Right Stick (Pad) | The ship rotates towards the crosshair/mouse cursor. |
| **Thrust Forward** | `W` | `Up Arrow` / Stick Up | Fires main engines to accelerate in ship heading. |
| **Reverse / Brake** | `S` | `Down Arrow` / Stick Down | Applies retro-thrusters to rapidly decelerate linear speed. |
| **Rotate (Turn Mode)** | `A` / `D` | `Left` / `Right Arrow` | Rotates ship counter-clockwise or clockwise (in Aim Mode 1). |
| **Fire Active Weapon** | `Space` / Left Mouse | Right Trigger / Button West | Fires equipped weapon; tap for single shot, hold to auto-fire. |
| **Boost Engines** | `Left Shift` / Right Mouse | Left Shoulder / Button South | Multiplies acceleration (1.8x) & speed (1.4x), costs 25 energy/s. |
| **Activate Skill** | `E` | Button North (Y) | Casts active skill (Forward Shockwave) forward along nose. |
| **Switch Weapon** | `1`, `2`, `3`, `4` | Mouse Scroll Wheel / D-Pad | 1: Pulse Laser, 2: Scattergun, 3: Missiles, 4: Beam Laser. |
| **Open Upgrade Drawer** | `U` / In-Game Button | Virtual Button / UI | Opens the in-run upgrade drawer to spend collected gems. |

### Control Modes (Configurable in Settings)
- **Mouse / Direct Aim (Mode 0 - Recommended)**: Your ship dynamically tracks your mouse cursor or gamepad right stick. Pressing `W` pushes the ship in whatever direction you are pointing.
- **Classic Asteroids Steering (Mode 1)**: `A` and `D` apply rotational torque to spin the ship, while `W` thrusts along the ship's nose heading. Ideal for retro Asteroids veterans.
- **Auto-Brake**: When enabled (default: ON), releasing thrust keys engages dampening retro-thrusters, bringing you to a controlled stop instead of drifting endlessly into hazards.

---

## 3. Heads-Up Display (HUD) Telemetry

The in-game HUD keeps you informed of your vital systems without cluttering your tactical view:

```
+-------------------------------------------------------------------------+
| [SHIELD: 100/100] [ENERGY: 80/80]        [WAVE 1: 00:24]   [GEMS: 48]   |
|                                                                         |
|                                                                         |
|                                  ( ^ )                                  |
|                               Player Ship                               |
|                                                                         |
|                                                                         |
| [1: Pulse] [2: Shotgun] [3: Missiles] [4: Beam]     [MINIMAP RADAR]     |
| [ACTIVE SKILL: Shockwave (READY)]                   [ O   .   x   ! ]   |
+-------------------------------------------------------------------------+
```

1. **Shield Meter (Blue)**: Absorbs damage from enemy lasers, missiles, and asteroid collisions. Shields regenerate after a brief delay if not taking fire. If shields hit 0, further hits destroy your ship.
2. **Energy Meter (Cyan/Gold)**: Powers weapon discharges and engine boosts. Recharges continuously. Firing weapons or boosting with 0 energy fails immediately.
3. **Gems Counter (Green)**: The quantity of unspent gems mined in the current run.
4. **Wave Timer / Progress**: Shows the active wave sector and countdown until the next wave or boss arrival.
5. **Skill Slot**: Shows the cooldown timer of your active ability (Forward Shockwave).
6. **Tactical Radar (Minimap)**:
   - **Cyan Circle**: Your ship.
   - **Red Triangles**: Hostile drone fighters.
   - **Gray Disks**: Asteroid rocks.
   - **Green Dots**: Uncollected gem crystals.
   - **Flashing Yellow Star**: Sector Dreadnought Boss.

---

## 4. Flight & Movement Mechanics

StarStrike simulates 2D space flight with Newtonian physics:
- **Momentum & Inertia**: When you cut thrusters, your ship maintains its drift velocity unless Auto-Braking or manual reverse thrust (`S`) is applied.
- **Toroidal World Boundary**: The sector spans $160 \times 160$ units. Flying beyond the outer sector boundary clamps or gently repels your craft back toward the operational combat zone.
- **Boost Mechanics**: Tapping or holding `Shift` activates boost. Use boost to escape flanking enemy drones or close distance quickly to collect floating gems before they drift away. Be mindful: boosting burns energy rapidly, leaving less capacity for sustained weapon fire.

---

## 5. Mining & Resource Extraction

Asteroids are your primary source of ship advancement:
- **Asteroid Sizes**:
  - **Large (120 HP)**: Fractures into 2 Medium Asteroids upon destruction, dropping 8 gems.
  - **Medium (50 HP)**: Fractures into 2 Small Asteroids upon destruction, dropping 4 gems.
  - **Small (20 HP)**: Explodes completely, dropping 1 gem.
- **Tractor Magnet**: Your ship is equipped with an automated tractor beam. When within **6.5 units** of floating gems, they accelerate toward your ship and are absorbed into your hold.
- **Power-Up Pickups**: Shattered asteroids occasionally yield rare utility crates:
  - **Shield Overcharge**: Instantly restores shields and grants +100 temporary bonus shield capacity.
  - **Weapon Overdrive**: Temporarily increases weapon slots (+1 muzzle) and increases firing cadence.

---

## 6. The Ship Evolution Loop

Collecting gems allows you to purchase stat upgrades mid-run via the **Upgrade Drawer** (`U` or clicking the drawer tab on screen):

```
+------------------------------------------------------+
|                     STAT UPGRADES                    |
|------------------------------------------------------|
| Shield Capacity: [LVL 2]   Cost: 15 Gems  [+ UPGRADE] |
| Shield Regen:    [LVL 1]   Cost: 10 Gems  [+ UPGRADE] |
| Energy Capacity: [LVL 2]   Cost: 15 Gems  [+ UPGRADE] |
| Energy Regen:    [LVL 1]   Cost: 10 Gems  [+ UPGRADE] |
| Pulse Damage:    [LVL 3]   Cost: 20 Gems  [+ UPGRADE] |
| Pulse Speed:     [LVL 1]   Cost: 10 Gems  [+ UPGRADE] |
| Top Speed:       [LVL 2]   Cost: 15 Gems  [+ UPGRADE] |
| Agility:         [LVL 1]   Cost: 10 Gems  [+ UPGRADE] |
+------------------------------------------------------+
```

### Triggering Evolution
Once you maximize all 8 stats for your current ship tier (e.g., Level 4 for the Delta Scout):
1. An **Evolution Banner** appears across your screen.
2. You are presented with a choice of specialized next-tier hulls (e.g., evolve into the high-damage **Ares Fighter** or the high-capacity **Goliath Miner**).
3. Evolving equips new ship sprites, enhanced base stats, additional weapon slots, and unlocks higher stat caps.

---

## 7. Combat Tactics & Enemy Encounters

### Enemy Drones
- Enemy drones spawn in wings of 4 to 12 ships.
- Drones attempt to circle your craft at medium range ($10\text{--}18$ units) while firing red pulse lasers.
- **Counter-Tactic**: Use the **Scattergun** at close range to obliterate drones in a single spread, or launch **Homing Missiles** while focusing on evasive maneuvers.

### Dreadnought Boss (Wave 3)
- The Dreadnought flagship arrives in Wave 3, boasting **2,500 HP** and a heavy 3-slot pulse battery.
- The boss passes through 3 combat phases, accelerating its rate of fire as its shield depletes.
- **Counter-Tactic**: Use **Continuous Beam** to rapidly melt its shields, fire **Forward Shockwave** (`E`) to neutralize incoming red laser barrages, and boost laterally to avoid its heavy frontal fire arc.

---

## 8. Win & Loss Conditions

- **Victory**: Defeating the Wave 3 Dreadnought Boss triggers the **Victory Screen**, displaying your run duration, total gems harvested, and asteroids shattered. Run gems are permanently deposited into your meta-progression account.
- **Defeat**: If your ship shields reach 0 and you sustain lethal damage, your ship is destroyed. The **Game Over Screen** summarizes your run statistics and returns you to the main menu hangar.
