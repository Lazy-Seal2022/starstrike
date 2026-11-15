# StarStrike Current Feature Matrix

This document provides an exhaustive inventory of all functional features and subsystems currently implemented in **StarStrike**.

---

## 1. Flight & Movement Mechanics

| Feature | Implementation | Description | Status |
| :--- | :--- | :--- | :--- |
| **Newtonian 2D Inertia** | [ShipMovement.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Movement/ShipMovement.cs) | Realistic linear velocity, mass-based acceleration, and rotational momentum governed by [FlightConfig.asset](file:///home/quan/projects/starstrike/unity/Assets/_Project/Resources/Data/FlightConfig.asset). | Fully Functional |
| **Twin-Stick / Direct Mouse Aim** | [PlayerInputReader.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Input/PlayerInputReader.cs) | Smooth cursor-following aim direction with configurable agility rate and dead-zone filtering. | Fully Functional |
| **Classic Keyboard Steering** | [ShipMovement.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Movement/ShipMovement.cs) | Retro Asteroids-style rotational torque (A/D or Left/Right arrows) with optional turn inversion. | Fully Functional |
| **Engine Boost** | [ShipMovement.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Movement/ShipMovement.cs) | 1.8x acceleration, 1.4x top speed surge draining 25 energy/sec with particle thruster elongation. | Fully Functional |
| **Dynamic Auto-Braking** | [ShipMovement.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Movement/ShipMovement.cs) | Automated counter-thrust deceleration when thrust inputs are released, preserving flight control. | Fully Functional |
| **Thruster Visual FX** | [ShipThrusterEffect.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/FX/ShipThrusterEffect.cs) | Dynamic ParticleSystem emission scaling with thrust and boost state. | Fully Functional |

---

## 2. Combat & Arsenal Systems

| Feature | Implementation | Description | Status |
| :--- | :--- | :--- | :--- |
| **Pulse Laser** | [ShipWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ShipWeapon.cs) | Rapid-fire projectile cannon supporting 1 to 3 concurrent parallel muzzles with recoil physics. | Fully Functional |
| **Scattergun** | [ScattergunWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ScattergunWeapon.cs) | Short-range shotgun blast firing 5 to 9 pellets in a spread arc with high impulse kickback. | Fully Functional |
| **Homing Missiles** | [HomingMissileWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/HomingMissileWeapon.cs) | Seeking ordnance that scans a $360^\circ$ radius and maneuvers towards the nearest enemy drone or asteroid. | Fully Functional |
| **Continuous Beam Laser** | [ContinuousBeamWeapon.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ContinuousBeamWeapon.cs) | High-DPS hitscan energy laser evaluating continuous raycast intersections and applying damage per second. | Fully Functional |
| **Dynamic Weapon Controller** | [ShipWeaponController.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Weapons/ShipWeaponController.cs) | Hot-swapping across 4 arsenals via keyboard keys (1-4), mouse scroll wheel, or D-pad. | Fully Functional |
| **Forward Shockwave Skill** | [ForwardShockwaveBehavior.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Skills/Behaviors/ForwardShockwaveBehavior.cs) | Active combat skill releasing a penetrating cone that destroys enemy lasers and knocks back enemies. | Fully Functional |
| **Reactive Barrier Skill** | [ReactiveBarrierPassiveBehavior.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Skills/Behaviors/ReactiveBarrierPassiveBehavior.cs) | Passive defense matrix triggering an invulnerability surge and shield boost on taking heavy damage. | Fully Functional |
| **Shield & Energy Economy** | [ShipHealth.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Combat/Health/ShipHealth.cs) | Split health model: regenerative energy powering weapons and boost, and regenerative shields absorbing damage. | Fully Functional |

---

## 3. World Generation, Mining & Enemy AI

| Feature | Implementation | Description | Status |
| :--- | :--- | :--- | :--- |
| **World Arena & Wrapping** | [WorldManager.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/World/WorldManager.cs) | Toroidal $160 \times 160$ unit space arena with boundary clamping, starfield parallax, and exclusion spawn zones. | Fully Functional |
| **Tiered Asteroid Mining** | [Asteroid.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Mining/Asteroid.cs) | Large, Medium, and Small destructible asteroids fracturing into smaller sub-asteroids and spawning loot crystals. | Fully Functional |
| **Magnetic Gem Crystals** | [GemCrystal.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Mining/GemCrystal.cs) | Mining currency pulled into player ship via tractor magnet physics, used for ship upgrades and evolution. | Fully Functional |
| **Power-Up Items** | [PowerUpItem.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Mining/PowerUpItem.cs) | Dropped consumables granting Shield Overcharge (+100 temporary shields) or Weapon Overdrive (rapid-fire + extra muzzle). | Fully Functional |
| **Enemy Drone AI** | [EnemyAI.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/AI/EnemyAI.cs) | Autonomous patrol, player pursuit, orbit maneuvering, and pulse laser engagement. | Fully Functional |
| **Dreadnought Boss Encounter** | [BossConfig.asset](file:///home/quan/projects/starstrike/unity/Assets/_Project/Resources/Data/BossConfig.asset) | Multi-phase flagship featuring 2,500 HP shield, 3-slot pulse battery, and escalating attack cadence. | Fully Functional |
| **Wave Director** | [WaveConfig.asset](file:///home/quan/projects/starstrike/unity/Assets/_Project/Resources/Data/WaveConfig.asset) | 3-stage escalation director managing intermission timers, enemy density caps, and boss spawn triggers. | Fully Functional |
| **High-Performance Object Pooling** | [PoolManager.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/World/PoolManager.cs) | Zero-allocation pooling for player lasers, enemy lasers, shotgun pellets, missiles, gems, and asteroids. | Fully Functional |

---

## 4. Ships & Player Progression

| Feature | Implementation | Description | Status |
| :--- | :--- | :--- | :--- |
| **Playable Ship Classes (5 Ships)** | [ShipDefinition.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Core/Definitions/Ships/ShipDefinition.cs) | Delta Scout (T1), Ares Fighter (T2), Goliath Miner (T2), Viper Interceptor (T3), Titan Juggernaut (T3). | Fully Functional |
| **Branching Evolution Tree** | [PlayerProgression.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Progression/PlayerProgression.cs) | Scout evolves into Fighter (combat focus) or Miner (mining focus), unlocking Tier 3 Interceptor or Juggernaut. | Fully Functional |
| **In-Run Stat Upgrades** | [UpgradeState.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Core/Progression/UpgradeState.cs) | Spend mined gems in real time across 8 attributes (Shield Cap, Shield Regen, Energy Cap, Energy Regen, Damage, Pulse Speed, Speed, Agility). | Fully Functional |
| **Persistent Meta Progression** | [StorageService.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Platform/Storage/StorageService.cs) | Mined gems carry over between runs to unlock starting evolved hulls and track high scores. | Fully Functional |
| **Achievement System** | [AchievementManager.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Progression/AchievementManager.cs) | 6 trackable milestones (First Blood, Asteroid Ace, Dreadnought Down, Full Evolution, Gem Collector, Overdrive Master). | Fully Functional |

---

## 5. User Interface & Audio

| Feature | Implementation | Description | Status |
| :--- | :--- | :--- | :--- |
| **Decoupled Screen Architecture** | [ScreenManager.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/Screens/ScreenManager.cs) | Non-overlapping, full-screen `RectTransform` screens managed independently without layout collisions. | Fully Functional |
| **HUD & Vital Telemetry** | [CanvasHUD.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/HUD/CanvasHUD.cs) | Shield/energy radial or linear bars, collected gems tally, active weapon icon, and wave progress timer. | Fully Functional |
| **Radar Minimap** | [MinimapController.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/Minimap/MinimapController.cs) | Real-time tactical radar tracking player (cyan), drones (red), asteroids (gray), gems (green), and boss (yellow). | Fully Functional |
| **In-Run Upgrade Drawer** | [HUDUpgradeDrawer.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/HUD/HUDUpgradeDrawer.cs) | Slide-out overlay to inspect stat levels and purchase upgrades mid-flight. | Fully Functional |
| **Evolution Banner** | [HUDEvolutionBanner.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/HUD/HUDEvolutionBanner.cs) | Visual modal prompting ship evolution choices when stat requirements are fulfilled. | Fully Functional |
| **Damage Floating Text** | [HUDFloatingText.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/HUD/HUDFloatingText.cs) | World-space floating numbers displaying damage taken, damage dealt, and gem pickups. | Fully Functional |
| **Settings Menu** | [SettingsUI.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/Settings/SettingsUI.cs) | Volume controls (Master, Music, SFX), aim mode selection, mouse sensitivity, auto-brake, and invert turn. | Fully Functional |
| **Audio Service** | [AudioService.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Gameplay/Audio/AudioService.cs) | Background music looping, laser fire, explosions, gem collection, and UI click audio feedback. | Fully Functional |

---

## 6. Multiplayer & Networking

| Feature | Implementation | Description | Status |
| :--- | :--- | :--- | :--- |
| **Netcode for GameObjects (NGO)** | [NetworkMatchManager.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Network/NetworkMatchManager.cs) | Authoritative host/client session framework managing network lifecycle and connection handshakes. | Fully Functional |
| **Network Player Ship** | [NetworkPlayerShip.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Network/NetworkPlayerShip.cs) | Synchronized network transform, linear velocity smoothing, server-authoritative damage, and health synchronization. | Fully Functional |
| **Network Projectile Replication** | [NetworkLaserProjectile.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Network/NetworkLaserProjectile.cs) | Server-spawned network lasers with client interpolation and hit registration. | Fully Functional |
| **Multiplayer Lobby & Scoreboard** | [NetworkLobbyUI.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/UI/Network/NetworkLobbyUI.cs) | Host/Join IP lobby, player latency ping display, and match kill/death scoreboards. | Fully Functional |

---

## 7. Verification & Automated Test Suites

| Test Suite | Location | Tests Passing | Description |
| :--- | :--- | :--- | :--- |
| **EditMode Unit Tests** | [Assets/_Project/Tests/EditMode/](file:///home/quan/projects/starstrike/unity/Assets/_Project/Tests/EditMode/) | **150 / 150 (100%)** | Tests covering physics, combat, damage, weapons, progression, pooling, prefabs, network, and architecture boundaries. |
| **Runtime Verification Menu** | [PrefabInitializer.cs](file:///home/quan/projects/starstrike/unity/Assets/_Project/Scripts/Editor/Build/PrefabInitializer.cs#L480) | **8 / 8 (100%)** | Automated regression checks verifying overdrive clamping, single-tap fire rate, hold auto-fire, skills, and projectile orientation. |
