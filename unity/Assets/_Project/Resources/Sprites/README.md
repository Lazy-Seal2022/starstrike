# Resources/Sprites Directory Structure

Runtime sprite assets loaded on-demand via `Resources.Load<Sprite>()`. Managed and accessed safely through `ProceduralSpriteHelper.cs`.

## Subdirectories

- **`Ships/`**: Ship sprites for runtime instantiation.
- **`Projectiles/`**: Projectile visuals loaded at runtime.
- **`World/`**: Environmental entities, celestial bodies, and loot drops.
- **`UI/`**: Runtime UI graphics.

## Access Convention

Prefer loading via `ProceduralSpriteHelper`:
```csharp
ProceduralSpriteHelper.GetShipSprite("fighter");
ProceduralSpriteHelper.GetLaserSprite(isEnemy: false);
```

Direct `Resources.Load<Sprite>` paths:
```csharp
Resources.Load<Sprite>("Sprites/Ships/Ship_Fighter");
Resources.Load<Sprite>("Sprites/Projectiles/Laser_Player");
```
`ProceduralSpriteHelper.LoadAssetOrDefault` also includes fallback resolution for legacy unqualified paths.
