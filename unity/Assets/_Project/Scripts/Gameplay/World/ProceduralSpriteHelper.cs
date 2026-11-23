using UnityEngine;

namespace StarStrike.Gameplay
{
    public static class ProceduralSpriteHelper
    {
        private static Sprite cachedScout;
        private static Sprite cachedFighter;
        private static Sprite cachedMiner;
        private static Sprite cachedDrone;
        private static Sprite cachedInterceptor;
        private static Sprite cachedJuggernaut;
        private static Sprite cachedBoss;
        private static Sprite cachedKamikaze;
        private static Sprite cachedSniper;
        private static Sprite cachedSupport;
        private static Sprite cachedVanguard;
        private static Sprite cachedPhantom;
        private static Sprite cachedLaser;
        private static Sprite cachedEnemyLaser;
        private static Sprite cachedAsteroid;
        private static Sprite cachedGem;
        private static Sprite cachedStar;
        private static Sprite cachedRadar;
        private static Sprite cachedPowerUp;
        private static Sprite cachedMissile;
        private static Sprite cachedPellet;
        private static Sprite cachedGatlingTracer;
        private static Sprite cachedShieldRipple;
        private static Sprite cachedShieldDome;
        private static Sprite cachedWeaponPulse;
        private static Sprite cachedWeaponScattergun;
        private static Sprite cachedWeaponMissile;
        private static Sprite cachedWeaponBeam;
        private static Sprite cachedWeaponGatling;
        private static Sprite cachedWeaponSocket;

        private static Sprite GetCachedOrGenerate(ref Sprite cache, System.Func<Sprite> generator, bool forceNew = false)
        {
            if (forceNew) return generator();
            if (cache == null)
            {
                cache = generator();
            }
            return cache;
        }

        public static Sprite GenerateShipSprite(string type)
        {
            if (type == "fighter") return GenerateShip(new Color(0.96f, 0.25f, 0.37f), 64, 2);
            if (type == "miner") return GenerateShip(new Color(0.96f, 0.62f, 0.04f), 64, 3);
            if (type == "drone") return GenerateShip(new Color(0.9f, 0.15f, 0.15f), 64, 4);
            if (type == "interceptor") return GenerateShip(new Color(0.45f, 0.35f, 0.98f), 64, 5);
            if (type == "juggernaut") return GenerateShip(new Color(0.95f, 0.45f, 0.15f), 64, 6);
            if (type == "boss") return GenerateShip(new Color(0.95f, 0.12f, 0.22f), 96, 7);
            if (type == "kamikaze") return GenerateShip(new Color(0.95f, 0.15f, 0.22f), 64, 8);
            if (type == "sniper") return GenerateShip(new Color(0.68f, 0.25f, 0.98f), 64, 9);
            if (type == "support" || type == "support_shield") return GenerateShip(new Color(0.95f, 0.82f, 0.15f), 64, 10);
            if (type == "vanguard") return GenerateShip(new Color(0.18f, 0.85f, 0.65f), 64, 11);
            if (type == "phantom") return GenerateShip(new Color(0.35f, 0.45f, 0.62f), 64, 12);
            return GenerateShip(new Color(0.22f, 0.74f, 0.97f), 64, 1);
        }

        public static Sprite GenerateLaserSprite(bool isEnemy = false)
        {
            if (isEnemy) return GenerateCapsule(new Color(1f, 0.25f, 0.25f), 48, 16);
            return GenerateCapsule(new Color(0.2f, 0.9f, 1f), 48, 16);
        }

        public static Sprite GenerateGatlingTracerSprite()
        {
            return GenerateCapsule(new Color(1f, 0.88f, 0.28f), 36, 10);
        }

        public static Sprite GetGatlingTracerSprite(bool forceProcedural = false)
        {
            if (!forceProcedural)
            {
                var combatConfig = Resources.Load<StarStrike.Core.CombatConfig>("Data/CombatConfig");
                if (combatConfig != null && combatConfig.gatlingBulletSprite != null)
                {
                    return combatConfig.gatlingBulletSprite;
                }
            }
            return GetCachedOrGenerate(ref cachedGatlingTracer, GenerateGatlingTracerSprite, forceProcedural);
        }

        public static Sprite GenerateAsteroidSprite()
        {
            return GenerateAsteroid(new Color(0.6f, 0.65f, 0.72f), 64);
        }

        public static Sprite GenerateGemSprite()
        {
            return GenerateDiamond(new Color(0.2f, 0.95f, 0.45f), 32);
        }

        public static Sprite GenerateStarSprite()
        {
            return GenerateCircle(Color.white, 16);
        }

        public static Sprite GeneratePowerUpSprite()
        {
            return GenerateDiamond(new Color(1f, 0.85f, 0.2f), 32);
        }

        public static Sprite GenerateMissileSprite()
        {
            return GenerateCapsule(new Color(1f, 0.4f, 0.1f), 40, 16);
        }

        public static Sprite GeneratePelletSprite()
        {
            return GenerateCircle(new Color(1f, 0.8f, 0.2f), 16);
        }

        public static Sprite GetShipSprite(string type, bool forceProcedural = false)
        {
            if (!forceProcedural)
            {
                string assetName = "Ship_Scout";
                switch (type != null ? type.ToLowerInvariant() : "scout")
                {
                    case "scout": assetName = "Ship_Scout"; break;
                    case "fighter": assetName = "Ship_Fighter"; break;
                    case "miner": assetName = "Ship_Miner"; break;
                    case "interceptor": assetName = "Ship_Interceptor"; break;
                    case "juggernaut": assetName = "Ship_Juggernaut"; break;
                    case "vanguard":
                    case "carrier": assetName = "Ship_Carrier"; break;
                    case "phantom": assetName = "Ship_Phantom"; break;
                    default:
                        if (!string.IsNullOrEmpty(type))
                        {
                            string cap = char.ToUpper(type[0]) + type.Substring(1);
                            assetName = $"Ship_{cap}";
                        }
                        break;
                }
                var def = Resources.Load<StarStrike.Core.ShipDefinition>($"Data/{assetName}");
                if (def == null && assetName == "Ship_Phantom")
                {
                    def = Resources.Load<StarStrike.Core.ShipDefinition>("Data/Ship_Interceptor");
                }
                if (def != null && def.sprite != null)
                {
                    return def.sprite;
                }

                if (type == "drone")
                {
                    var enemyDef = Resources.Load<StarStrike.Core.EnemyDefinition>("Data/Enemy_Drone");
                    if (enemyDef != null && enemyDef.sprite != null) return enemyDef.sprite;
                }
                else if (type == "boss")
                {
                    var bossDef = Resources.Load<StarStrike.Core.BossConfig>("Data/BossConfig");
                    if (bossDef != null && bossDef.sprite != null) return bossDef.sprite;
                }
            }

            if (type == "fighter")
                return GetCachedOrGenerate(ref cachedFighter, () => GenerateShipSprite("fighter"), forceProcedural);
            if (type == "miner")
                return GetCachedOrGenerate(ref cachedMiner, () => GenerateShipSprite("miner"), forceProcedural);
            if (type == "drone")
                return GetCachedOrGenerate(ref cachedDrone, () => GenerateShipSprite("drone"), forceProcedural);
            if (type == "interceptor")
                return GetCachedOrGenerate(ref cachedInterceptor, () => GenerateShipSprite("interceptor"), forceProcedural);
            if (type == "juggernaut")
                return GetCachedOrGenerate(ref cachedJuggernaut, () => GenerateShipSprite("juggernaut"), forceProcedural);
            if (type == "boss")
                return GetCachedOrGenerate(ref cachedBoss, () => GenerateShipSprite("boss"), forceProcedural);
            if (type == "kamikaze")
                return GetCachedOrGenerate(ref cachedKamikaze, () => GenerateShipSprite("kamikaze"), forceProcedural);
            if (type == "sniper")
                return GetCachedOrGenerate(ref cachedSniper, () => GenerateShipSprite("sniper"), forceProcedural);
            if (type == "support" || type == "support_shield")
                return GetCachedOrGenerate(ref cachedSupport, () => GenerateShipSprite("support"), forceProcedural);
            if (type == "vanguard")
                return GetCachedOrGenerate(ref cachedVanguard, () => GenerateShipSprite("vanguard"), forceProcedural);
            if (type == "phantom")
                return GetCachedOrGenerate(ref cachedPhantom, () => GenerateShipSprite("phantom"), forceProcedural);

            return GetCachedOrGenerate(ref cachedScout, () => GenerateShipSprite("scout"), forceProcedural);
        }

        public static Sprite GetLaserSprite(bool isEnemy = false, bool forceProcedural = false)
        {
            if (isEnemy)
                return GetCachedOrGenerate(ref cachedEnemyLaser, () => GenerateLaserSprite(true), forceProcedural);

            return GetCachedOrGenerate(ref cachedLaser, () => GenerateLaserSprite(false), forceProcedural);
        }

        public static Sprite GetAsteroidSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedAsteroid, () => GenerateAsteroidSprite(), forceProcedural);
        }

        public static Sprite GetGemSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedGem, () => GenerateGemSprite(), forceProcedural);
        }

        public static Sprite GetStarSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedStar, () => GenerateStarSprite(), forceProcedural);
        }

        public static Sprite GetPowerUpSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedPowerUp, () => GeneratePowerUpSprite(), forceProcedural);
        }

        public static Sprite GetMissileSprite(bool forceProcedural = false)
        {
            if (!forceProcedural)
            {
                var combatConfig = Resources.Load<StarStrike.Core.CombatConfig>("Data/CombatConfig");
                if (combatConfig != null && combatConfig.missileSprite != null)
                {
                    return combatConfig.missileSprite;
                }
            }
            return GetCachedOrGenerate(ref cachedMissile, () => GenerateMissileSprite(), forceProcedural);
        }

        public static Sprite GetPelletSprite(bool forceProcedural = false)
        {
            if (!forceProcedural)
            {
                var combatConfig = Resources.Load<StarStrike.Core.CombatConfig>("Data/CombatConfig");
                if (combatConfig != null && combatConfig.scattergunPelletSprite != null)
                {
                    return combatConfig.scattergunPelletSprite;
                }
            }
            return GetCachedOrGenerate(ref cachedPellet, () => GeneratePelletSprite(), forceProcedural);
        }

        public static Sprite GetWeaponSprite(string weaponId, bool forceProcedural = false)
        {
            if (string.IsNullOrEmpty(weaponId)) weaponId = "pulse";
            string lower = weaponId.ToLowerInvariant().Trim();

            if (!forceProcedural)
            {
                string resName = "Weapon_Pulse";
                if (lower.Contains("gatling") || lower == "4") resName = "Weapon_Gatling";
                else if (lower.Contains("beam") || lower == "3") resName = "Weapon_Beam";
                else if (lower.Contains("missile") || lower.Contains("homing") || lower == "2") resName = "Weapon_Missile";
                else if (lower.Contains("scatter") || lower == "1") resName = "Weapon_Scattergun";

                var loaded = Resources.Load<Sprite>($"Sprites/Weapons/{resName}");
                if (loaded != null) return loaded;
            }

            if (lower.Contains("gatling") || lower == "4") return GetCachedOrGenerate(ref cachedWeaponGatling, () => GenerateWeaponIcon(new Color(1f, 0.85f, 0.25f), 1), forceProcedural);
            if (lower.Contains("beam") || lower == "3") return GetCachedOrGenerate(ref cachedWeaponBeam, () => GenerateWeaponIcon(new Color(0.2f, 0.95f, 1f), 2), forceProcedural);
            if (lower.Contains("missile") || lower.Contains("homing") || lower == "2") return GetCachedOrGenerate(ref cachedWeaponMissile, () => GenerateWeaponIcon(new Color(1f, 0.25f, 0.35f), 3), forceProcedural);
            if (lower.Contains("scatter") || lower == "1") return GetCachedOrGenerate(ref cachedWeaponScattergun, () => GenerateWeaponIcon(new Color(1f, 0.55f, 0.15f), 4), forceProcedural);
            return GetCachedOrGenerate(ref cachedWeaponPulse, () => GenerateWeaponIcon(new Color(0.1f, 0.95f, 0.65f), 0), forceProcedural);
        }

        public static Sprite GetWeaponSocketSprite(bool forceProcedural = false)
        {
            if (!forceProcedural)
            {
                var loaded = Resources.Load<Sprite>("Sprites/Weapons/Weapon_Socket");
                if (loaded != null) return loaded;
            }
            return GetCachedOrGenerate(ref cachedWeaponSocket, () => GenerateWeaponSocketIcon(), forceProcedural);
        }

        private static Sprite GenerateWeaponSocketIcon()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Color clear = new Color(0, 0, 0, 0);
            Color darkMetal = new Color(0.08f, 0.11f, 0.16f, 1f);
            Color midMetal = new Color(0.18f, 0.23f, 0.32f, 1f);
            Color rimMetal = new Color(0.35f, 0.45f, 0.60f, 1f);
            Color cyanAccent = new Color(0f, 0.94f, 1f, 1f);
            Color yellowAccent = new Color(1f, 0.90f, 0.10f, 1f);

            for (int i = 0; i < cols.Length; i++) cols[i] = clear;

            float cx = size / 2f;
            float cy = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    int dx = Mathf.Abs((int)(x - cx));
                    int dy = Mathf.Abs((int)(y - cy));

                    // Outer chamfered base plate
                    if (dx <= 24 && dy <= 24 && (dx + dy <= 38))
                    {
                        cols[y * size + x] = (dx == 24 || dy == 24 || dx + dy >= 36) ? rimMetal : midMetal;
                    }

                    // Circular collar
                    if (dist <= 20f && dist >= 16f)
                    {
                        cols[y * size + x] = cyanAccent;
                    }
                    else if (dist < 16f && dist >= 8f)
                    {
                        cols[y * size + x] = darkMetal;
                    }
                    else if (dist < 8f && dist >= 5f)
                    {
                        cols[y * size + x] = yellowAccent;
                    }
                    else if (dist < 5f)
                    {
                        cols[y * size + x] = Color.white;
                    }

                    // Alignment notches
                    if (dist <= 22f && (dx <= 1 || dy <= 1) && dist >= 14f)
                    {
                        cols[y * size + x] = cyanAccent;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        private static Sprite GenerateWeaponIcon(Color accent, int variant)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Color clear = new Color(0, 0, 0, 0);
            Color darkMetal = new Color(0.12f, 0.14f, 0.18f, 1f);
            Color midMetal = new Color(0.22f, 0.26f, 0.32f, 1f);

            for (int i = 0; i < cols.Length; i++) cols[i] = clear;

            int cx = size / 2;
            int cy = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = Mathf.Abs(x - cx);

                    // Base mount wedge
                    if (y >= cy - 8 && y <= cy + 22 && dx <= (22 - (y - cy + 8) * 0.3f))
                    {
                        cols[y * size + x] = (dx < 14 && y > cy) ? darkMetal : midMetal;
                    }

                    // Weapon specific muzzle/barrels
                    if (variant == 0) // Pulse (Dual Barrels)
                    {
                        if (y >= cy - 24 && y < cy - 8 && ((dx >= 6 && dx <= 11) || (dx <= 2 && y > cy - 14)))
                        {
                            cols[y * size + x] = (y < cy - 18) ? accent : midMetal;
                        }
                    }
                    else if (variant == 1) // Gatling (Rotary Bundle)
                    {
                        if (y >= cy - 26 && y < cy - 6 && dx <= 12)
                        {
                            cols[y * size + x] = (dx % 4 <= 1) ? accent : midMetal;
                        }
                    }
                    else if (variant == 2) // Beam (Lance Core)
                    {
                        if (y >= cy - 28 && y < cy - 6 && dx <= (4 + (cy - 6 - y) * 0.3f))
                        {
                            cols[y * size + x] = (dx <= 1) ? Color.white : accent;
                        }
                    }
                    else if (variant == 3) // Missile (Pod Silos)
                    {
                        if (y >= cy - 22 && y < cy - 6 && dx <= 16)
                        {
                            cols[y * size + x] = (y > cy - 14 && (dx == 4 || dx == 12)) ? accent : darkMetal;
                        }
                    }
                    else // Scattergun (Flared Cowl)
                    {
                        if (y >= cy - 24 && y < cy - 6 && dx <= (10 + (cy - 6 - y) * 0.4f))
                        {
                            cols[y * size + x] = (y < cy - 16 && dx % 5 <= 2) ? accent : midMetal;
                        }
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        private static Sprite GenerateCircle(Color color, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float r = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center.x, y - center.y);
                    float dist = p.magnitude;
                    if (dist <= r)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist / r));
                        cols[y * size + x] = new Color(color.r, color.g, color.b, alpha * alpha);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
        }

        private static Sprite GenerateShip(Color color, int size, int style)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float r = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center.x, y - center.y);
                    float dist = p.magnitude;
                    bool inside = false;

                    if (style == 1) // Scout (Delta Arrow)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        inside = nx >= -0.5f && nx <= 0.85f && ny <= (0.85f - nx) * 0.65f;
                    }
                    else if (style == 2) // Fighter (Twin Wings)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        inside = (nx >= -0.6f && nx <= 0.9f && ny <= (0.9f - nx) * 0.7f) ||
                                 (nx >= -0.8f && nx <= 0.2f && ny >= 0.4f && ny <= 0.85f);
                    }
                    else if (style == 3) // Miner (Heavy Cruiser)
                    {
                        float nx = Mathf.Abs(p.x) / r;
                        float ny = Mathf.Abs(p.y) / r;
                        inside = (nx <= 0.7f && ny <= 0.7f) || (p.x >= 0 && p.x <= r * 0.95f && ny <= 0.45f);
                    }
                    else if (style == 4) // Drone (Diamond saucer)
                    {
                        float nx = Mathf.Abs(p.x) / (r * 0.8f);
                        float ny = Mathf.Abs(p.y) / (r * 0.8f);
                        inside = (nx + ny) <= 1.0f;
                    }
                    else if (style == 5) // Interceptor (Viper Interceptor - sleek needle + swept wings)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool needle = (nx >= -0.7f && nx <= 0.95f && ny <= (0.95f - nx) * 0.35f);
                        bool sweptWings = (nx >= -0.5f && nx <= 0.25f && ny <= 0.85f && ny >= 0.3f && (nx - ny * 0.35f) >= -0.45f);
                        bool engines = (nx >= -0.85f && nx <= -0.3f && ny >= 0.2f && ny <= 0.45f);
                        inside = needle || sweptWings || engines;
                    }
                    else if (style == 6) // Juggernaut (Titan Juggernaut - armored tank chassis + prongs)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool mainBlock = (Mathf.Abs(nx) <= 0.75f && ny <= 0.75f);
                        bool frontProngs = (nx >= 0.2f && nx <= 0.95f && ny >= 0.25f && ny <= 0.7f);
                        bool centerRam = (nx >= 0.6f && nx <= 0.9f && ny <= 0.25f);
                        bool rearPlates = (nx <= -0.4f && nx >= -0.9f && ny <= 0.85f);
                        inside = mainBlock || frontProngs || centerRam || rearPlates;
                    }
                    else if (style == 7) // Boss (Dreadnought flagship)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool core = (nx >= -0.85f && nx <= 0.9f && ny <= (0.95f - nx) * 0.75f);
                        bool claws = (nx >= -0.2f && nx <= 0.85f && ny >= 0.5f && ny <= 0.95f);
                        bool rearEngines = (nx <= -0.4f && nx >= -0.95f && ny >= 0.3f && ny <= 0.85f);
                        inside = core || claws || rearEngines;
                    }
                    else if (style == 8) // Kamikaze (Razor Spear / Forward Arrowhead)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool spear = (nx >= -0.7f && nx <= 0.95f && ny <= (0.95f - nx) * 0.45f);
                        bool barbWings = (nx >= -0.5f && nx <= -0.1f && ny <= 0.85f && (ny - (nx + 0.5f) * 0.8f) >= 0f);
                        inside = spear || barbWings;
                    }
                    else if (style == 9) // Sniper (Elongated needle railgun hull)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool railgunBarrel = (nx >= 0.0f && nx <= 0.98f && ny <= 0.12f);
                        bool fuselage = (nx >= -0.8f && nx <= 0.3f && ny <= (0.5f - nx * 0.4f) * 0.45f);
                        bool stabilizerFins = (nx >= -0.75f && nx <= -0.2f && ny <= 0.75f && ny >= 0.25f);
                        inside = railgunBarrel || fuselage || stabilizerFins;
                    }
                    else if (style == 10) // Support Shield (Hexagonal Bastion with core dome)
                    {
                        float nx = Mathf.Abs(p.x) / (r * 0.85f);
                        float ny = Mathf.Abs(p.y) / (r * 0.85f);
                        bool hexagon = (nx <= 0.85f && ny <= 0.85f && (nx * 0.5f + ny * 0.866f) <= 0.95f);
                        bool core = (dist <= r * 0.45f);
                        inside = hexagon || core;
                    }
                    else if (style == 11) // Vanguard Carrier (Dual flight bay catamaran)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool bridge = (nx >= -0.7f && nx <= 0.4f && ny <= 0.35f);
                        bool bays = (nx >= -0.85f && nx <= 0.8f && ny >= 0.4f && ny <= 0.85f);
                        bool crossbars = (nx >= -0.3f && nx <= 0.2f && ny <= 0.8f);
                        inside = bridge || bays || crossbars;
                    }
                    else if (style == 12) // Shadow Phantom (Stealth delta with forward swept wings)
                    {
                        float nx = p.x / r;
                        float ny = Mathf.Abs(p.y) / r;
                        bool fuselage = (nx >= -0.6f && nx <= 0.9f && ny <= (0.9f - nx) * 0.35f);
                        bool sweptWings = (nx >= -0.4f && nx <= 0.6f && ny <= 0.85f && (ny + nx * 0.4f) <= 0.9f && (ny + nx * 0.4f) >= 0.35f);
                        inside = fuselage || sweptWings;
                    }

                    if (inside)
                    {
                        float edge = 1f - Mathf.Clamp01(dist / r);
                        cols[y * size + x] = Color.Lerp(color, Color.white, edge * 0.5f);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            float ppu = (size <= 48) ? 16f : ((size <= 64) ? 21.333f : 32f);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        }

        private static Sprite GenerateCapsule(Color color, int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x - w * 0.5f) / (w * 0.5f);
                    float ny = (y - h * 0.5f) / (h * 0.5f);
                    float d = (w >= h)
                        ? Mathf.Sqrt(nx * nx * 0.25f + ny * ny)
                        : Mathf.Sqrt(nx * nx + ny * ny * 0.25f);
                    if (d < 1f)
                    {
                        float glow = Mathf.Pow(1f - d, 1.5f);
                        cols[y * w + x] = Color.Lerp(color, Color.white, glow);
                    }
                    else
                    {
                        cols[y * w + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16);
        }

        private static Sprite GenerateAsteroid(Color color, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float baseR = size * 0.38f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 p = new Vector2(x - center.x, y - center.y);
                    float angle = Mathf.Atan2(p.y, p.x);
                    float r = baseR * (1f + 0.18f * Mathf.Sin(angle * 5f) + 0.12f * Mathf.Cos(angle * 3f + 1f));

                    if (p.magnitude <= r)
                    {
                        float shade = 0.7f + 0.3f * Mathf.Sin(p.x * 0.2f + p.y * 0.15f);
                        cols[y * size + x] = new Color(color.r * shade, color.g * shade, color.b * shade, 1f);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            float ppu = (size <= 48) ? 16f : ((size <= 64) ? 21.333f : 32f);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        }

        private static Sprite GenerateDiamond(Color color, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float r = size * 0.4f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs(x - center.x) / r;
                    float ny = Mathf.Abs(y - center.y) / r;
                    if (nx + ny <= 1f)
                    {
                        float shine = 1f - (nx + ny);
                        cols[y * size + x] = Color.Lerp(color, Color.white, shine * 0.7f);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            float ppu = (size <= 48) ? 16f : ((size <= 64) ? 21.333f : 32f);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        }

        private static Sprite cachedWhite;
        private static Sprite cachedCircle;
        private static Sprite cachedRoundedBox;

        public static Sprite GetWhiteSprite()
        {
            if (cachedWhite == null)
            {
                Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                Color[] cols = new Color[16];
                for (int i = 0; i < 16; i++) cols[i] = Color.white;
                tex.SetPixels(cols);
                tex.Apply();
                cachedWhite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100);
            }
            return cachedWhite;
        }

        public static Sprite GetCircleSprite(int size = 64, bool forceProcedural = false)
        {
            if (forceProcedural) return GenerateCircle(Color.white, size);
            if (cachedCircle == null)
            {
                cachedCircle = GenerateCircle(Color.white, size);
            }
            return cachedCircle;
        }

        public static Sprite GetRadarSprite(int size = 128, bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedRadar, () => GenerateRadarInternal(size), forceProcedural);
        }

        private static Sprite GenerateRadarInternal(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.48f;
            float innerRingRadius = radius * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        Color col = new Color(0.02f, 0.05f, 0.12f, 0.75f);

                        if (dist >= radius - 2.5f)
                        {
                            col = new Color(0.2f, 0.8f, 1f, 0.85f);
                        }
                        else if (Mathf.Abs(dist - innerRingRadius) <= 1.0f)
                        {
                            col = new Color(0.2f, 0.8f, 1f, 0.3f);
                        }
                        else if ((Mathf.Abs(x - center.x) <= 0.6f || Mathf.Abs(y - center.y) <= 0.6f) && dist > 4f)
                        {
                            col = new Color(0.2f, 0.8f, 1f, 0.25f);
                        }

                        float aa = Mathf.Clamp01((radius - dist) + 0.5f);
                        cols[y * size + x] = new Color(col.r, col.g, col.b, col.a * aa);
                    }
                    else
                    {
                        cols[y * size + x] = Color.clear;
                    }
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }

        public static Sprite GetRoundedBoxSprite(int size = 32, int cornerRadius = 6)
        {
            if (cachedRoundedBox == null)
            {
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                Color[] cols = new Color[size * size];

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int cx = x < cornerRadius ? cornerRadius : (x >= size - cornerRadius ? size - 1 - cornerRadius : x);
                        int cy = y < cornerRadius ? cornerRadius : (y >= size - cornerRadius ? size - 1 - cornerRadius : y);
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        float alpha = dist > cornerRadius ? 0f : 1f;
                        cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                }
                tex.SetPixels(cols);
                tex.Apply();
                cachedRoundedBox = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
            }
            return cachedRoundedBox;
        }

        public static Sprite GetShieldRippleSprite(int size = 64, bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedShieldRipple, () => GenerateShieldRippleInternal(size), forceProcedural);
        }

        private static Sprite GenerateShieldRippleInternal(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.48f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius)
                    {
                        cols[y * size + x] = Color.clear;
                        continue;
                    }

                    float normDist = dist / radius;
                    float angle = Mathf.Atan2(dy, dx);

                    // Concentric energy shockwave rings
                    float rings = Mathf.Sin(normDist * Mathf.PI * 4f);
                    rings = Mathf.Pow(Mathf.Clamp01(rings * 0.5f + 0.5f), 2.5f);

                    // 6-fold hexagonal facets
                    float hex = Mathf.Clamp01(Mathf.Cos(6f * angle) * 0.5f + 0.5f);

                    // Central kinetic impact core
                    float core = Mathf.Exp(-normDist * normDist * 8f);

                    // Feathered soft edge falloff
                    float edgeFade = Mathf.Pow(1f - normDist, 2f);

                    float intensity = Mathf.Clamp01((core * 1.5f + rings * 0.7f + hex * rings * 0.5f) * edgeFade);
                    cols[y * size + x] = new Color(1f, 1f, 1f, intensity);
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }

        public static Sprite GetShieldDomeSprite(int size = 128, bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedShieldDome, () => GenerateShieldDomeInternal(size), forceProcedural);
        }

        private static Sprite GenerateShieldDomeInternal(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.48f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius)
                    {
                        cols[y * size + x] = Color.clear;
                        continue;
                    }

                    float normDist = dist / radius;
                    float angle = Mathf.Atan2(dy, dx);

                    // Glowing outer perimeter rim
                    float rim = Mathf.Exp(-Mathf.Pow((normDist - 0.92f) * 12f, 2f));

                    // Subtle interior hex grid shimmer
                    float hexField = Mathf.Cos(12f * angle) * 0.08f + 0.05f;

                    float alpha = Mathf.Clamp01(rim * 0.9f + hexField * (1f - normDist));
                    cols[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }
    }
}
