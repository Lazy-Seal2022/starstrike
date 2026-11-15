# StarStrike Gap Analysis & Missing Features

This document provides a comprehensive technical and game design gap analysis of **StarStrike**, identifying what features, assets, systems, and polish the game is currently missing to reach commercial-grade quality.

---

## Executive Summary of Deficits

While StarStrike possesses a rock-solid, decoupled architectural foundation (Layer-first ASMDEF, clean 60+ FPS physics, multi-weapon switching, 5 ship classes, and zero-allocation object pooling), it currently resembles a **highly polished vertical slice** rather than a feature-complete commercial game.

```
+-------------------------------------------------------------------------------+
|                           STARSTRIKE MATURITY MATRIX                          |
+-------------------------------------------------------------------------------+
| System Layer                | Current State          | Production Target      |
+-----------------------------+------------------------+------------------------+
| Core Architecture           | 95% Complete           | 100% (Solid)           |
| Flight Physics & Controls   | 90% Complete           | 95% (Key remapping)    |
| Combat & Weapons            | 80% Complete           | 95% (Mod attachments)  |
| Tactical Skills             | 50% Complete           | 85% (Loadout select)   |
| Enemy & Boss Roster         | 35% Complete           | 85% (Need 6+ archetypes|
| Wave & Environment Content  | 30% Complete           | 85% (Need 10+ sectors) |
| Meta Progression (Hangar)   | 40% Complete           | 90% (Tech tree needed) |
| Audio & Dynamic Music       | 35% Complete           | 90% (Mixer & dynamic)  |
| Visuals, VFX & Polish       | 45% Complete           | 90% (Shaders & shake)  |
| Multiplayer (UGS Relay)     | 40% Complete           | 85% (Need co-op & UGS) |
| UI/UX & Localization        | 55% Complete           | 90% (Multi-language)   |
+-------------------------------------------------------------------------------+
```

---

## 1. Audio & Dynamic Music Deficits

### Current State
- [AudioConfig.asset](file:///home/quan/projects/starstrike/unity/Assets/_Project/Resources/Data/AudioConfig.asset) contains single one-shot audio clips (`laser`, `enemy_laser`, `gem`, `explosion`, `upgrade`, `click`) and a single ambient loop (`ambientMusic`).
- [AudioService.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Audio/AudioService.cs) spawns audio via basic `AudioSource.PlayClipAtPoint` or simple background looping.

### Missing Audio Features
1. **Dynamic / Adaptive Combat Soundtrack**:
   - The game lacks music state transitions. There is no shift in musical tempo or stems between peaceful asteroid mining, drone swarm engagements, and boss encounters.
   - Missing low-health tension stingers (heartbeat pulse or low-pass filter on ambient music when shields drop below 20%).
2. **Audio Mixer Routing & Sound Hierarchy**:
   - `masterMixer`, `masterGroup`, `musicGroup`, `sfxGroup`, and `uiGroup` in `AudioConfig.asset` are currently unassigned (`null`).
   - Sounds play without ducking (e.g., loud explosions do not duck background music; thruster hum does not filter during menu drawer opening).
3. **Positional Spatialization & Sound FX**:
   - No looping engine thruster audio with pitch-shifting tied to velocity or boost.
   - No directional warning klaxons when missiles lock on or when enemies approach off-screen.

---

## 2. Visual Effects, Shaders & Combat Juice

### Current State
- Ship and projectile visuals rely on procedural rasterization or 2D sprites.
- Visual FX is limited to a single thruster particle trail and simple floating damage numbers.

### Missing Visual Features
1. **Camera Shake & Impact Juice**:
   - Zero screen shake on sustaining hull damage, detonating large asteroids, or firing the heavy recoil Scattergun.
   - No brief hit-stop / micro-pause (1-2 frames) on landing heavy hits or destroying the Dreadnought Boss.
2. **URP 2D Dynamic Lighting**:
   - Although the project uses Universal Render Pipeline 2D (`Renderer2D.asset`), scene entities lack dynamic `Light2D` components.
   - Lasers, gem crystals, explosion fireballs, and ship thrusters should cast colored dynamic 2D point lights illuminating passing asteroid surfaces.
3. **Advanced Shaders & Hull Feedback**:
   - Ships lack shield impact ripple shaders when taking fire.
   - Ships lack white damage-flash shaders upon taking projectile hits.
   - Lack of animated engine exhaust distortion shaders (heat haze / chromatic aberration).
4. **Volumetric Explosion & Debris Systems**:
   - Destroyed asteroids currently split into smaller child GameObjects without dust clouds, fractured rock particle chunks, or lingering asteroid vapor.

---

## 3. Content Variety: Ships, Enemies & Environment

### Current State
- 5 playable ships across 3 tiers (Scout $\rightarrow$ Fighter / Miner $\rightarrow$ Interceptor / Juggernaut).
- 1 enemy drone type (`EnemyDrone.prefab`) and 1 boss (`BossDreadnought.prefab`).
- 3 waves in [WaveConfig.asset](file:///home/quan/projects/starstrike/unity/Assets/_Project/Resources/Data/WaveConfig.asset) that simply loop after Wave 3 with a $1.25\times$ difficulty multiplier.

### Missing Content
1. **Expanded Ship Archetypes**:
   - **Carrier / Drone Commander**: Spawns autonomous mining/defense helper drones.
   - **Electronic Warfare / Stealth Cruiser**: Deploys radar cloaking, holo-decoys, and EMP disruption fields.
   - **Railgun Sniper**: Long-range, piercing kinetic slugs requiring charging time.
2. **Diverse Enemy Roster**:
   - **Kamikaze Swarmers**: Fragile, high-speed drones that self-destruct upon collision.
   - **Sniper Turret Platforms**: Static or drifting defense stations firing high-velocity, high-damage railgun beams.
   - **Shield Support Frigates**: Defensive craft that project energy bubbles protecting nearby drones.
   - **Asteroid Burrowers**: Stealth creatures or automated rigs that pop out from large asteroid interiors.
3. **Environmental Hazards & Space Biomes**:
   - Currently, space is an empty black backdrop with a parallax starfield.
   - Missing cosmic hazards:
     - **Ion Storms**: Scrambles tactical radar and disables auto-braking.
     - **Solar Flares**: Periodic directional waves damaging unshielded craft.
     - **Gravitational Anomalies / Black Holes**: Pulls ships and asteroids into an event horizon.
     - **Nebula Clouds**: Obscures vision and masks drone blips from radar.

---

## 4. Systems Depth & Meta-Progression

### Current State
- Mined gems can be spent mid-run on 8 basic stat upgrades.
- Gems persist across runs into `StorageService.GetMetaGems()`.
- Players can select previously evolved hulls from the Main Menu.

### Missing Systems Depth
1. **Hangar Meta-Tech Tree**:
   - There is currently no permanent tech tree menu to spend accumulated meta-gems on permanent perks (e.g., +10% base tractor beam magnet radius, +1 starting weapon slot, +5% gem drop chance from asteroids, passive energy regeneration baseline).
2. **Skill Loadout Customization**:
   - Ships are currently locked to `ForwardShockwave` (Active 1) and `ReactiveBarrier` (Passive 1).
   - Missing an ability equip screen allowing pilots to swap skills for alternate options:
     - *Blink Teleport*: Instant forward micro-jump bypassing hazards.
     - *EMP Nova*: Disables all nearby enemy weapons for 4 seconds.
     - *Overcharge Matrix*: Instantly refills energy and doubles weapon fire rate for 5s.
3. **In-Run Quests & Dynamic Bounties**:
   - No procedural contracts (e.g., "Destroy 5 asteroids in 15 seconds", "Harvest 50 gems without taking shield damage", "Eliminate Elite Drone target").
4. **Weapon Attachments & Elemental Effects**:
   - All weapon damage is flat kinetic/energy.
   - Missing elemental status effects:
     - *Plasma Burn*: Damage-over-time melting armor.
     - *Cryo Freeze*: Slows enemy turn rate and speed by 40%.
     - *Chain EMP*: Arcs electricity across clustered drone wings.

---

## 5. Multiplayer & Networking Infrastructure

### Current State
- Netcode for GameObjects (NGO) is functional for local host/client dogfights.
- [NetworkLobbyUI.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/Network/NetworkLobbyUI.cs) allows entering an IP address and port to connect.

### Missing Multiplayer Features
1. **Unity Gaming Services (UGS) Relay & Lobby Integration**:
   - Currently requires players to know direct IP addresses or run on LAN.
   - Missing automatic matchmaking, room codes, and NAT punch-through via UGS Relay and Lobby SDKs.
2. **Co-Op PvE Mode**:
   - Network mode only supports PvP arena dogfighting.
   - The game lacks 2-4 player cooperative wave survival where pilots mine asteroids and fight boss flagships together.
3. **Client-Side Prediction & Lag Compensation**:
   - High-latency connections (>100ms) will experience rubber-banding or delayed laser hit registration due to lack of rollback interpolation.

---

## 6. UI / UX, Accessibility & Quality of Life

### Current State
- UI screens are decoupled and functional, with basic volume and flight sliders in [SettingsUI.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/Settings/SettingsUI.cs).

### Missing UI/UX Features
1. **Keybinding Re-mapping**:
   - Bindings for `W`, `A`, `S`, `D`, `Space`, `Shift`, `E`, and `1-4` are hardcoded in `PlayerInputReader.cs`. Players cannot rebind controls to ESDF, AZERTY, or custom mouse buttons.
2. **Graphics & Display Settings**:
   - No resolution selector, fullscreen/windowed toggle, V-Sync toggle, target framerate limiter, or bloom/post-processing toggles.
3. **Multi-Language Localization**:
   - All UI text, HUD labels, ship names, and upgrade descriptions are hardcoded in English strings. No integration with Unity Localization package.
4. **Adaptive Input Display**:
   - Virtual joysticks and touch buttons exist in code, but do not automatically hide on PC or show on touch devices (`Application.isMobilePlatform`).
   - Missing gamepad button glyphs (showing Xbox 'A' / PlayStation 'X' icons dynamically based on connected controller).
