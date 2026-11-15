# Art/Sprites Directory Structure

Single source of truth for all sprite assets in StarStrike. These textures feed into `StarStrikeMasterAtlas.spriteatlasv2` for GPU draw call batching and are referenced directly by ScriptableObject definitions (`ShipDefinition`, `EnemyDefinition`, `BossConfig`, `AsteroidConfig`, `LootConfig`, `CombatConfig`) and Prefabs.

## Subdirectories

- **`Ships/`**: Player and enemy spaceship textures (`Ship_Scout`, `Ship_Fighter`, `Ship_Miner`, `Ship_Drone`, `Ship_Interceptor`, `Ship_Juggernaut`, `Ship_Boss`).
- **`Projectiles/`**: Lasers, missiles, pellets, and other projectile textures (`Laser_Player`, `Laser_Enemy`).
- **`World/`**: Environmental and drop sprites (`Asteroid`, `Gem`, `PowerUp`, `Star`).
- **`UI/`**: In-game HUD and UI textures (`Radar`, `Circle_16`).

## Sprite Atlas
- **`StarStrikeMasterAtlas.spriteatlasv2`**: Master sprite atlas packing all domain sprites for draw call batching and runtime performance.

## Design Note
All runtime components reference sprites via their assigned definition ScriptableObjects or prefab `SpriteRenderer.sprite` references. Procedural math in `ProceduralSpriteHelper.cs` serves strictly as an in-memory fallback for unit testing and headless execution. No duplicate sprites exist in `Resources/`.
