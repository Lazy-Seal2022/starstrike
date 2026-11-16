# 06 — UI, UX & HUD Design System

> **Target:** UI/UX Designers, Graphic Designers, Motion Graphic Artists  
> **Topic:** In-Game HUD Telemetry, Menu Screens, Icons & Dynamic Modals  

---

## 1. UI/UX Vision: Aerospace Tactical Telemetry

The user interface in StarStrike is designed to feel like an **advanced holographic pilot flight helmet display (HUD)**. It strikes a balance between retro sci-fi vectors and ultra-crisp modern esports readability.

### Core Interface Rules
* **Peripheral Focus:** The center of the screen is kept as clear as possible for combat action. Crucial meters are placed along screen perimeters or curve around the player ship.
* **45-Degree Chamfered Geometry:** Rectangular boxes are avoided in favor of angled corners, chamfers, and tech brackets.
* **High-Contrast Digital Typography:** Crisp monospace numbers with uppercase geometric headers (e.g., *Rajdhani*, *Orbitron*, or *Share Tech Mono*).

---

## 2. In-Game HUD Layout & Component Wireframe

```
+-----------------------------------------------------------------------------+
| [TOP-LEFT: VITAL STATUS]       [TOP-CENTER: WAVE & ALERTS]      [TOP-RIGHT] |
| (()) SHIELD ARC (140/140)      == WAVE 04 / 10 [01:42] ==      SCORE: 18450 |
| (()) ENERGY ARC (090/090)      WARNING: SOLAR FLARE IMMINENT   GEMS:  # 320 |
| [||||||||] HULL (100%)                                                      |
|                                                                             |
|                                                                             |
|                                   [SCREEN CENTER]             [RADAR MINIMAP]
|                                     (PLAYER SHIP)             (O) Sweep Radar
|                                                                   * Enemy Blips
|                                                                   . Asteroids
|                                                                             |
|                                                                             |
| [BOTTOM-LEFT: FLIGHT]                                      [BOTTOM-RIGHT]   |
| SPEED: 11.4 m/s                                            WEAPONS:         |
| BOOST: [||||||||||] 100%                                   [1][2][3][4]     |
| VECTOR: <--->                                              SKILL: [E] EMP   |
+-----------------------------------------------------------------------------+
```

---

## 3. Detailed In-Game Widgets

### 3.1. Vital Status Widget (Top-Left)
* **Shield Meter:** Concentric curved outer arc glowing electric blue (`#3B82F6`). Flashes white upon taking damage; pulses red when broken.
* **Energy Meter:** Inner concentric curved arc glowing high-volt cyan/yellow (`#00F0FF`). Shrinks when boosting or firing heavy weapons; recharges smoothly.
* **Hull Integrity Bar:** Segmented horizontal bar beneath the arcs (`#10B981` Green -> `#EAB308` Yellow -> `#EF4444` Critical Red).

### 3.2. Wave Progress & Sector Alerts (Top-Center)
* **Wave Counter Banner:** Clean digital display (`WAVE 05 / 10`) accompanied by a ticking circular countdown clock.
* **Boss Alarm Flasher:** When a capital boss enters, an emergency warning flasher drops down with hazard stripes and a flashing skull icon: `[ ! ] DREADNOUGHT SIGNATURE DETECTED [ ! ]`.

### 3.3. Tactical Arsenal & Skill Widget (Bottom-Right)
* **Weapon Selector (Keys 1–4):**
  - 4 horizontal equipment slots showing custom weapon icons:
    1. *Pulse Laser* (Dual laser bolts)
    2. *Scattergun* (Conical pellet burst)
    3. *Homing Missiles* (Micro-rocket silhouette)
    4. *Tachyon Beam* (Focused continuous laser line)
  - The currently active weapon is framed in a glowing cyan border; inactive weapons are dimmed to 40% gray.
* **Active Skill Cooldown Dial (Key E):**
  - Circular icon of the equipped skill (*Shockwave*, *Blink*, or *EMP Nova*).
  - Radial sweep fill overlay (clock-hand wipe) showing remaining cooldown seconds.
* **Passive Skill Indicator:** Small shield icon displaying *Deflector Barrier* readiness.

### 3.4. Minimap Radar (Center-Right)
* **Display Frame:** Circular tactical scope (`Diameter: 160 px`) with concentric distance rings and a continuous rotating 360-degree radar sweep line.
* **Blip Legend:**
  - Player: Bright cyan triangle pointing in the current ship heading.
  - Hostile Ships: Pulsing red diamond blips.
  - Heavy Bosses: Large flashing crimson skulls.
  - Asteroids: Subtle slate-gray dots.
  - Mining Gems: Soft glowing green pings.

---

## 4. Modal Screens & Menus

### 4.1. Mid-Run Evolution Selection Cards
When reaching a level threshold, the game pauses and presents the player with **2 large Branch Evolution Cards** side-by-side:

```
+--------------------------------+   +--------------------------------+
|         ARES FIGHTER           |   |         GOLIATH MINER          |
|      [ TIER 2 ASSAULT ]        |   |      [ TIER 2 INDUSTRIAL ]     |
|                                |   |                                |
|   [ HIGH-RES SHIP ARTWORK ]    |   |   [ HIGH-RES SHIP ARTWORK ]    |
|                                |   |                                |
| - 2x Twin Wing Hardpoints      |   | - 2x Heavy Mining Lasers       |
| - +25% Turn & Flight Speed     |   | - +80% Shield Capacity         |
| - Primary: Combat Dogfighting  |   | - +100% Gem Mining Yield       |
|                                |   |                                |
|       [ SELECT HULL ]          |   |       [ SELECT HULL ]          |
+--------------------------------+   +--------------------------------+
```

* **Card Dimensions:** 320 x 480 px each.
* **Styling:** Holographic glass backing with glowing neon borders. Hovering over a card triggers a subtle forward scale bump and energy surge SFX.

### 4.2. Hangar Screen (Ship Showcase)
* **Layout:**
  - Left Panel: Ship Hull Roster (Scout, Fighter, Miner, Interceptor, Juggernaut, Carrier, Phantom).
  - Center Stage: High-resolution rotating 2D/3D orthographic showcase of the selected vessel.
  - Right Panel: Detailed telemetry stat radar chart (Speed, Turn Rate, Shield, Energy, Weapon Slots).
  - Bottom Panel: Active Skill selector loadout buttons (`[ SHOCKWAVE ]`, `[ BLINK WARP ]`, `[ EMP NOVA ]`).

### 4.3. Run Summary Screen (Game Over / Victory)
* **Victory / Defeat Header:** Bold metallic typography with glowing under-glow.
* **Stats Breakdown Grid:**
  - Waves Survived, Enemies Vaporized, Bosses Defeated, Gems Extracted, Accuracy %.
* **New Unlocks Banner:** Unlocked ship hulls and achievements presented with gold celebratory wreaths.

---

## 5. UI Iconography Production Checklist

The graphic artist must deliver the following vector/sprite icons at **64x64 px** (and 128x128 px for high-res cards):

| Category | Icon Name | Description |
| :--- | :--- | :--- |
| **Weapons** | `icon_weap_laser.png` | Dual parallel glowing cyan laser bolts. |
| **Weapons** | `icon_weap_scatter.png` | Conical multi-pellet kinetic spread. |
| **Weapons** | `icon_weap_missile.png` | Sleek finned micro-rocket angled at 45°. |
| **Weapons** | `icon_weap_beam.png` | Searing continuous laser beam core. |
| **Skills** | `icon_skill_shockwave.png` | Expanding crescent sonic shockwave. |
| **Skills** | `icon_skill_blink.png` | Ship silhouette snapping into a warp streak. |
| **Skills** | `icon_skill_emp.png` | Electromagnetic pulsing orb with lightning sparks. |
| **Skills** | `icon_skill_barrier.png` | Hexagonal defensive deflector shield mesh. |
| **Stats** | `icon_stat_shield.png` | Crest shield with blue energy pulse. |
| **Stats** | `icon_stat_speed.png` | Triple supersonic thrust chevrons. |
| **Stats** | `icon_stat_power.png` | High-voltage energy spark bolt. |
| **Economy** | `icon_currency_gem.png` | Faceted glowing emerald crystal shard. |
