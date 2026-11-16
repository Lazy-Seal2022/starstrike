# 03 — Enemy Armada & Flagship Bosses

> **Target:** Monster/Vehicle Concept Artists, 2D Illustrators, VFX Artists  
> **Topic:** Hostile AI Archetypes, Tactical Telegraphs & Capital Boss Specifications  

---

## 1. Enemy Design Philosophy: Hostile Menace & Threat Clarity

Enemies in StarStrike must communicate their **attack intent and danger level instantly**. A player scanning a swarm of 20 ships must immediately differentiate a suicidal rammer from a charging long-range sniper.

### Universal Enemy Art Rules
1. **Aggressive Color Palette:** Hostile craft are dominated by dark charcoal hulls (`#0F172A`), blood crimson accents (`#FF0055`), and burning orange thruster plumes (`#FF5500`).
2. **Telegraphing Visuals:** Every high-threat enemy attack (snipers charging, kamikazes locking on, bosses firing beams) MUST have a dedicated visual warning animation before the projectile or impact occurs.
3. **EMP Stun State Compatibility:** When hit by the player's EMP Nova, all enemies must display electric cyan flickering arcs across their hulls and shut off all weapon and engine lights.

---

## 2. Hostile AI Ship Archetypes

### 2.1. Swarm Drone (`EnemyArchetype.Drone`)
* **Role:** High-speed patrol and swarming harassment fighter.
* **Canvas Size:** 48 x 48 px (1.5 x 1.5 Unity Units).
* **Silhouette:** Compact predatory dart or flying wing with a glowing crimson cyclops eye at the nose.
* **Color Scheme:** Matte obsidian gray with blood-red striping.
* **Hardware Layout:**
  - **Weapon Hardpoint:** 1 Central Muzzle firing light red laser bolts.
  - **Thrusters:** 1 Rear micro-thruster with continuous red flame.
* **Animation Needs:**
  - Idle patrol cruise.
  - Muzzle flash burst when firing.
  - Shrapnel breakup: Splits into 3 small hull fragments upon destruction.

---

### 2.2. Kamikaze Rammer (`EnemyArchetype.Kamikaze`)
* **Role:** High-velocity suicide bomber that charges the player and detonates on impact or destruction.
* **Canvas Size:** 56 x 56 px (1.75 x 1.75 Unity Units).
* **Silhouette:** Volatile torpedo-wedge chassis. Large exposed explosive warhead core at the prow with containment cages.
* **Color Scheme:** Industrial black with caution-hazard stripes and an intensely glowing orange-red molten core (`#FF3B00`).
* **Visual Telegraphs & States:**
  - **Cruising State:** Core pulses at a slow 1 Hz glow.
  - **Lock-On / Dive State:** When diving toward the player, the rear thruster triples in flame length, and the warhead core strobes at a rapid 8 Hz flash.
  - **Detonation VFX:** 4.5-unit radial fireball with expanding shockwave and molten metal droplets.

---

### 2.3. Railgun Sniper (`EnemyArchetype.Sniper`)
* **Role:** Long-range artillery craft that holds standoff distance and fires hypersonic piercing beams.
* **Canvas Size:** 72 x 48 px (2.25 x 1.5 Unity Units).
* **Silhouette:** Elongated asymmetric rifle-ship. One extended magnetic accelerator barrel running the full length, balanced by capacitor heat-sink fins on the opposing wing.
* **Color Scheme:** Dark gunmetal with violet / ultraviolet capacitor highlights (`#A855F7`).
* **Visual Telegraphs & States:**
  - **Standoff Mode:** Fin radiators glow faintly.
  - **Charging Telegraph (1.2 Seconds):**
    - Capacitors along the barrel illuminate sequentially from back to front.
    - Emits a thin, pulsing purple targeting laser line (`LineRenderer`) stretching toward the player's position.
    - Barrel tip sparks with concentrated purple plasma particles.
  - **Fire Release:** A blinding white-violet beam flash with dramatic recoil kickback.

---

### 2.4. Support Shield Cruiser (`EnemyArchetype.SupportShield`)
* **Role:** Fleet protector that projects invulnerability energy fields to adjacent allies.
* **Canvas Size:** 80 x 80 px (2.5 x 2.5 Unity Units).
* **Silhouette:** Broad hexagonal platform with three floating orbital shield projector coils rotating slowly around its perimeter.
* **Color Scheme:** Slate armor with deep azure and cobalt energy conduits (`#2563EB`, `#60A5FA`).
* **Hardware Layout & FX:**
  - **Shield Tether Beam:** Renders a continuous, electric blue beam connecting the cruiser to all shielded drone allies.
  - **Tethered Ally State:** Shielded allies display a glowing hexagonal translucent barrier bubble matching the cruiser's frequency.
* **Animation Needs:**
  - Continuous rotation loop for the 3 orbital projector coils.
  - Overload / EMP collapse: Coils spark and drop offline when the cruiser is destroyed or EMP-stunned.

---

## 3. Capital Flagship Boss: The Dreadnought (`EnemyArchetype.Boss`)

The **Dreadnought** is a screen-filling boss warship that anchors high-tier sector climaxes.

```
                  [ FORWARD HEAVY TORPEDO PROW ]
                             /      \
               [ PORT TURRET ]      [ STARBOARD TURRET ]
                      \                  /
                       [ ARMORED BRIDGE ]
                                |
                    [ EXPOSED FUSION CORE ]
                    [ (PHASE 2 RAGE VENT) ]
                                |
               [ QUAD HEAVY WARP PROPULSION BLOCKS ]
```

* **Canvas Size:** 256 x 256 px (8.0 x 8.0 Unity Units).
* **Visual Concept:** Imposing, brutalist star cruiser. Tiered armor decking, recessed missile silos, heavy rotating flak turrets, and glowing engine manifolds.
* **Color Scheme:** Blackened dread-hull steel (`#0B0F19`) with crimson command deck glass and burning orange plasma reactors.

### Multi-Phase Visual Transformation

#### Phase 1: Heavy Shielded Siege (100% – 50% HP)
* Pristine heavy composite armor plating.
* Massive forward deflector shield bubble (`Radius: 4.5 units`) rippling blue upon bullet impacts.
* Left and Right sponson turrets rotate independently, tracking the player with twin crimson laser volleys.

#### Phase 2: Overheated Berserk Rage (< 50% HP)
* **Visual Damage:** Outer wing plates are blown off, spewing black smoke and electrical fires.
* **Core Meltdown Exposure:** The dorsal armor peels open, exposing the pulsating molten orange-yellow Fusion Core.
* **Rage Attacks:**
  - **360° Radial Flak Barrage:** Fires spiraling waves of fiery red ordnance.
  - **Core Laser Sweep:** Forward reactor fires a gigantic beam that sweeps across a 90° frontal arc with screen-shaking intensity.

---

## 4. Enemy Deliverables & Sprite Breakdown

For each enemy class, the art team must deliver:

| Asset Name | Format | Specifications |
| :--- | :--- | :--- |
| `enemy_drone_sheet.png` | RGBA 32-bit (48x48) | Base hull, thruster plume, muzzle flash frame. |
| `enemy_kamikaze_sheet.png` | RGBA 32-bit (56x56) | Base hull, charging strobe loop (4 frames), thruster boost flame. |
| `enemy_sniper_sheet.png` | RGBA 32-bit (72x48) | Base hull, capacitor charge sequence (4 frames), recoil frame. |
| `enemy_support_sheet.png` | RGBA 32-bit (80x80) | Cruiser base chassis + separate sprites for the rotating shield coils. |
| `boss_dreadnought_base.png` | RGBA 32-bit (256x256) | Capital ship hull with detached modular turret sprites. |
| `boss_dreadnought_damaged.png` | RGBA 32-bit (256x256) | Phase 2 variant with torn hull plates and exposed glowing core. |
| `enemy_debris_atlas.png` | RGBA 32-bit (128x128) | Collection of 16-32px generic shrapnel chunks, plating shards, and scorched wiring for destruction physics. |
