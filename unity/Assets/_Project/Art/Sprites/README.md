# Art/Sprites Directory Structure

Source sprite assets categorized by domain. These textures feed into `StarStrikeMasterAtlas.spriteatlasv2` for draw call batching.

## Subdirectories

- **`Ships/`**: Player and enemy spaceship textures (`Ship_Scout`, `Ship_Fighter`, `Ship_Miner`, `Ship_Drone`, `Ship_Interceptor`, `Ship_Juggernaut`, `Ship_Boss`).
- **`Projectiles/`**: Lasers, missiles, pellets, and other projectile textures (`Laser_Player`, `Laser_Enemy`).
- **`World/`**: Environmental and drop sprites (`Asteroid`, `Gem`, `PowerUp`, `Star`).
- **`UI/`**: In-game HUD and UI textures (`Radar`, `Circle_16`).

## Sprite Atlas
- **`StarStrikeMasterAtlas.spriteatlasv2`**: Master sprite atlas packing all domain sprites for batching and runtime performance.
