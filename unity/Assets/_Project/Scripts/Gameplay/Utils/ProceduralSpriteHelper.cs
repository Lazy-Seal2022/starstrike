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
        private static Sprite cachedFrigate;
        private static Sprite cachedDestroyer;
        private static Sprite cachedCruiser;
        private static Sprite cachedCarrier;
        private static Sprite cachedBattleship;
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
        private static Sprite cachedEscortDrone;
        private static Sprite cachedShieldRipple;
        private static Sprite cachedShieldDome;
        private static Sprite cachedWeaponPulse;
        private static Sprite cachedWeaponScattergun;
        private static Sprite cachedWeaponMissile;
        private static Sprite cachedWeaponBeam;
        private static Sprite cachedWeaponGatling;
        private static Sprite cachedWeaponSocket;
        private static Sprite cachedDefenseIcon;
        private static Sprite cachedEngineIcon;
        private static Sprite cachedReactorIcon;
        private static Sprite cachedAuxiliaryIcon;

        public static void ClearCache()
        {
            cachedScout = null;
            cachedFighter = null;
            cachedMiner = null;
            cachedDrone = null;
            cachedInterceptor = null;
            cachedJuggernaut = null;
            cachedBoss = null;
            cachedKamikaze = null;
            cachedSniper = null;
            cachedSupport = null;
            cachedVanguard = null;
            cachedPhantom = null;
            cachedFrigate = null;
            cachedDestroyer = null;
            cachedCruiser = null;
            cachedCarrier = null;
            cachedBattleship = null;
            cachedLaser = null;
            cachedEnemyLaser = null;
            cachedAsteroid = null;
            cachedGem = null;
            cachedStar = null;
            cachedRadar = null;
            cachedPowerUp = null;
            cachedMissile = null;
            cachedPellet = null;
            cachedGatlingTracer = null;
            cachedEscortDrone = null;
        }

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
            string key = string.IsNullOrEmpty(type) ? "scout" : type.ToLowerInvariant();
            Color hullColor;
            Color stripeColor;
            int size = 64;
            int style = 1;
            bool isCapital = false;

            switch (key)
            {
                case "scout":
                    hullColor = new Color(0.40f, 0.43f, 0.46f); // Naval Slate Steel
                    stripeColor = new Color(0.0f, 0.82f, 1.0f);  // Allied Cyan
                    size = 64; style = 1; isCapital = false;
                    break;
                case "fighter":
                    hullColor = new Color(0.36f, 0.39f, 0.43f); // Gunmetal Grey
                    stripeColor = new Color(0.0f, 0.82f, 1.0f);  // Allied Cyan
                    size = 64; style = 2; isCapital = false;
                    break;
                case "miner":
                    hullColor = new Color(0.50f, 0.47f, 0.40f); // Muted Desert Tan / Industrial Ochre
                    stripeColor = new Color(0.96f, 0.60f, 0.05f); // Hazard Amber
                    size = 64; style = 3; isCapital = false;
                    break;
                case "drone":
                    hullColor = new Color(0.26f, 0.28f, 0.31f); // Imperial Gunmetal
                    stripeColor = new Color(0.88f, 0.15f, 0.20f); // Pirate Crimson
                    size = 64; style = 4; isCapital = false;
                    break;
                case "interceptor":
                    hullColor = new Color(0.24f, 0.26f, 0.29f); // Stealth Charcoal
                    stripeColor = new Color(0.65f, 0.35f, 0.98f); // Neon Violet
                    size = 64; style = 5; isCapital = false;
                    break;
                case "juggernaut":
                    hullColor = new Color(0.34f, 0.33f, 0.31f); // Heavy Cast Iron
                    stripeColor = new Color(0.92f, 0.48f, 0.10f); // Hazard Orange
                    size = 64; style = 6; isCapital = true;
                    break;
                case "boss":
                    hullColor = new Color(0.18f, 0.20f, 0.23f); // Imperial Obsidian
                    stripeColor = new Color(0.92f, 0.12f, 0.18f); // Blood Crimson
                    size = 96; style = 7; isCapital = true;
                    break;
                case "kamikaze":
                    hullColor = new Color(0.35f, 0.37f, 0.39f); // Weathered Iron
                    stripeColor = new Color(0.92f, 0.18f, 0.18f); // Warning Crimson
                    size = 64; style = 8; isCapital = false;
                    break;
                case "sniper":
                    hullColor = new Color(0.28f, 0.31f, 0.36f); // Navy Slate
                    stripeColor = new Color(0.70f, 0.30f, 1.0f);  // Electric Violet
                    size = 64; style = 9; isCapital = false;
                    break;
                case "support":
                case "support_shield":
                    hullColor = new Color(0.72f, 0.74f, 0.76f); // Ceramic Off-White
                    stripeColor = new Color(0.95f, 0.80f, 0.15f); // Shield Gold
                    size = 64; style = 10; isCapital = false;
                    break;
                case "vanguard":
                case "carrier":
                    hullColor = new Color(0.70f, 0.73f, 0.75f); // Ceramic Off-White
                    stripeColor = new Color(0.15f, 0.55f, 0.95f); // Fleet Cobalt
                    size = 80; style = 11; isCapital = true;
                    break;
                case "phantom":
                    hullColor = new Color(0.16f, 0.17f, 0.20f); // Stealth Basalt
                    stripeColor = new Color(0.25f, 0.75f, 0.85f); // Ghost Cyan
                    size = 64; style = 12; isCapital = false;
                    break;
                case "frigate":
                    hullColor = new Color(0.34f, 0.37f, 0.41f); // Naval Gunmetal
                    stripeColor = new Color(0.0f, 0.80f, 0.95f);  // Allied Cyan
                    size = 64; style = 13; isCapital = false;
                    break;
                case "destroyer":
                    hullColor = new Color(0.32f, 0.34f, 0.36f); // Heavy Armor Steel
                    stripeColor = new Color(0.92f, 0.55f, 0.12f); // Hazard Amber
                    size = 72; style = 14; isCapital = true;
                    break;
                case "cruiser":
                    hullColor = new Color(0.35f, 0.38f, 0.42f); // Naval Battleship Grey
                    stripeColor = new Color(0.20f, 0.50f, 0.95f); // Royal Cobalt
                    size = 80; style = 15; isCapital = true;
                    break;
                case "battleship":
                    hullColor = new Color(0.22f, 0.24f, 0.27f); // Imperial Dreadnought Gunmetal
                    stripeColor = new Color(0.85f, 0.15f, 0.20f); // Imperial Crimson
                    size = 96; style = 16; isCapital = true;
                    break;
                default:
                    hullColor = new Color(0.40f, 0.43f, 0.46f);
                    stripeColor = new Color(0.0f, 0.82f, 1.0f);
                    size = 64; style = 1; isCapital = false;
                    break;
            }

            return GenerateShip(hullColor, stripeColor, size, style, isCapital);
        }

        public static Sprite GenerateLaserSprite(bool isEnemy = false)
        {
            if (isEnemy) return GenerateCapsule(new Color(1f, 0.15f, 0.22f), 48, 18);
            return GenerateCapsule(new Color(0.1f, 0.85f, 1f), 48, 18);
        }

        public static Sprite GenerateGatlingTracerSprite()
        {
            return GenerateCapsule(new Color(1f, 0.82f, 0.20f), 38, 12);
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

        public static Sprite GenerateEscortDroneSprite()
        {
            return GenerateShip(new Color(0.38f, 0.42f, 0.46f), new Color(0.0f, 0.85f, 1.0f), 32, 5, false);
        }

        public static Sprite GetEscortDroneSprite(bool forceProcedural = false)
        {
            return GetCachedOrGenerate(ref cachedEscortDrone, GenerateEscortDroneSprite, forceProcedural);
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
                    case "frigate": assetName = "Ship_Frigate"; break;
                    case "destroyer": assetName = "Ship_Destroyer"; break;
                    case "cruiser": assetName = "Ship_Cruiser"; break;
                    case "battleship": assetName = "Ship_Battleship"; break;
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
            if (type == "carrier")
                return GetCachedOrGenerate(ref cachedCarrier, () => GenerateShipSprite("carrier"), forceProcedural);
            if (type == "phantom")
                return GetCachedOrGenerate(ref cachedPhantom, () => GenerateShipSprite("phantom"), forceProcedural);
            if (type == "frigate")
                return GetCachedOrGenerate(ref cachedFrigate, () => GenerateShipSprite("frigate"), forceProcedural);
            if (type == "destroyer")
                return GetCachedOrGenerate(ref cachedDestroyer, () => GenerateShipSprite("destroyer"), forceProcedural);
            if (type == "cruiser")
                return GetCachedOrGenerate(ref cachedCruiser, () => GenerateShipSprite("cruiser"), forceProcedural);
            if (type == "battleship")
                return GetCachedOrGenerate(ref cachedBattleship, () => GenerateShipSprite("battleship"), forceProcedural);

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
                var combatConfig = Resources.Load<StarStrike.Core.CombatConfig>("Data/CombatConfig");
                if (combatConfig != null)
                {
                    GameObject prefab = null;
                    if (lower.Contains("gatling") || lower == "4") prefab = combatConfig.gatlingWeaponPrefab;
                    else if (lower.Contains("beam") || lower == "3") prefab = combatConfig.continuousBeamWeaponPrefab;
                    else if (lower.Contains("missile") || lower.Contains("homing") || lower == "2") prefab = combatConfig.homingMissileWeaponPrefab;
                    else if (lower.Contains("scatter") || lower == "1") prefab = combatConfig.scattergunWeaponPrefab;
                    else prefab = combatConfig.pulseLaserWeaponPrefab;

                    if (prefab != null)
                    {
                        var sr = prefab.GetComponent<SpriteRenderer>();
                        if (sr != null && sr.sprite != null) return sr.sprite;
                    }
                }
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
                var combatConfig = Resources.Load<StarStrike.Core.CombatConfig>("Data/CombatConfig");
                if (combatConfig != null && combatConfig.socketMountPrefab != null)
                {
                    var sr = combatConfig.socketMountPrefab.GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null) return sr.sprite;
                }
            }
            return GetCachedOrGenerate(ref cachedWeaponSocket, () => GenerateWeaponSocketIcon(), forceProcedural);
        }

        public static Sprite GetWeaponSlotSprite(bool forceProcedural = false)
        {
            return GetWeaponSocketSprite(forceProcedural);
        }

        public static Sprite GetCategoryIcon(StarStrike.Core.EquipmentSlotCategory category, bool forceProcedural = false)
        {
            switch (category)
            {
                case StarStrike.Core.EquipmentSlotCategory.Defense:
                    return GetCachedOrGenerate(ref cachedDefenseIcon, () => GenerateCategoryIcon(StarStrike.Core.EquipmentSlotCategory.Defense, new Color(0.2f, 0.85f, 1f)), forceProcedural);
                case StarStrike.Core.EquipmentSlotCategory.Engine:
                    return GetCachedOrGenerate(ref cachedEngineIcon, () => GenerateCategoryIcon(StarStrike.Core.EquipmentSlotCategory.Engine, new Color(1f, 0.55f, 0.15f)), forceProcedural);
                case StarStrike.Core.EquipmentSlotCategory.Reactor:
                    return GetCachedOrGenerate(ref cachedReactorIcon, () => GenerateCategoryIcon(StarStrike.Core.EquipmentSlotCategory.Reactor, new Color(1f, 0.90f, 0.20f)), forceProcedural);
                case StarStrike.Core.EquipmentSlotCategory.Auxiliary:
                    return GetCachedOrGenerate(ref cachedAuxiliaryIcon, () => GenerateCategoryIcon(StarStrike.Core.EquipmentSlotCategory.Auxiliary, new Color(0.75f, 0.35f, 1f)), forceProcedural);
                default:
                    return GetWeaponSocketSprite(forceProcedural);
            }
        }

        public static Sprite GetModuleIcon(string moduleId, StarStrike.Core.EquipmentSlotCategory category, bool forceProcedural = false)
        {
            if (category == StarStrike.Core.EquipmentSlotCategory.Weapon)
            {
                return GetWeaponSprite(moduleId, forceProcedural);
            }
            return GetCategoryIcon(category, forceProcedural);
        }

        private static Sprite GenerateCategoryIcon(StarStrike.Core.EquipmentSlotCategory category, Color accent)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Color clear = new Color(0, 0, 0, 0);
            Color darkMetal = new Color(0.08f, 0.10f, 0.14f, 0.95f);
            Color steelBorder = new Color(0.40f, 0.52f, 0.65f, 1f);

            for (int i = 0; i < cols.Length; i++) cols[i] = clear;

            float cx = size / 2f;
            float cy = size / 2f;
            float r = size * 0.42f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - cx);
                    float dy = Mathf.Abs(y - cy);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist <= r)
                    {
                        if (dist >= r - 2f)
                        {
                            cols[y * size + x] = steelBorder;
                        }
                        else
                        {
                            cols[y * size + x] = darkMetal;

                            bool isGlyph = false;
                            if (category == StarStrike.Core.EquipmentSlotCategory.Defense)
                            {
                                float ny = (cy + 10 - y) / (r * 0.8f);
                                float nx = dx / (r * 0.75f);
                                isGlyph = (ny >= -0.7f && ny <= 0.8f && nx <= (1f - Mathf.Max(0f, -ny) * 0.7f));
                            }
                            else if (category == StarStrike.Core.EquipmentSlotCategory.Engine)
                            {
                                float py = y - cy;
                                isGlyph = (py >= -14 && py <= 14 && (dx - Mathf.Abs(py) * 0.6f) <= 4f && (dx - Mathf.Abs(py) * 0.6f) >= -3f);
                            }
                            else if (category == StarStrike.Core.EquipmentSlotCategory.Reactor)
                            {
                                isGlyph = (dist <= 6f) || (dist >= 12f && dist <= 16f);
                            }
                            else if (category == StarStrike.Core.EquipmentSlotCategory.Auxiliary)
                            {
                                isGlyph = (dx + dy <= 12f) || (dx <= 2f && dy <= 18f) || (dy <= 2f && dx <= 18f);
                            }

                            if (isGlyph)
                            {
                                cols[y * size + x] = accent;
                            }
                        }
                    }
                }
            }

            tex.SetPixels(cols);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        private static Sprite GenerateWeaponSocketIcon()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] cols = new Color[size * size];
            Color clear = new Color(0, 0, 0, 0);
            Color darkMetal = new Color(0.07f, 0.09f, 0.13f, 0.92f);
            Color steelBorder = new Color(0.45f, 0.57f, 0.69f, 1f); // Battlevoid tactical military steel
            Color innerBevel = new Color(0.25f, 0.33f, 0.43f, 0.7f);

            for (int i = 0; i < cols.Length; i++) cols[i] = clear;

            float cx = size / 2f;
            float cy = size / 2f;
            float halfBox = 26f;
            float cornerRadius = 8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float qx = Mathf.Abs(x - cx);
                    float qy = Mathf.Abs(y - cy);

                    // Outer rounded square distance
                    float rx = Mathf.Max(0f, qx - (halfBox - cornerRadius));
                    float ry = Mathf.Max(0f, qy - (halfBox - cornerRadius));
                    float outerDist = Mathf.Sqrt(rx * rx + ry * ry);

                    if (outerDist <= cornerRadius)
                    {
                        if (outerDist >= cornerRadius - 1.8f)
                        {
                            // Crisp military steel border (no neon glow)
                            cols[y * size + x] = steelBorder;
                        }
                        else if (outerDist >= cornerRadius - 2.8f)
                        {
                            // Subtle inner bevel line
                            cols[y * size + x] = innerBevel;
                        }
                        else
                        {
                            // Pure simple dark tactical armor fill, zero inside decorations
                            cols[y * size + x] = darkMetal;
                        }
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

        private static float Hash2D(int x, int y, int seed = 0)
        {
            int n = x + y * 57 + seed * 131;
            n = (n << 13) ^ n;
            return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483648.0f;
        }

        private static Sprite GenerateShip(Color color, int size, int style)
        {
            bool isCap = (style == 6 || style == 7 || style == 11 || style == 14 || style == 15 || style == 16);
            Color stripe = isCap ? new Color(0.85f, 0.18f, 0.20f) : new Color(0.0f, 0.80f, 1.0f);
            return GenerateShip(color, stripe, size, style, isCap);
        }

        private static Sprite GenerateShip(Color hullColor, Color stripeColor, int size, int style, bool isCapital)
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
                    float nx = p.x / r;
                    float ny = Mathf.Abs(p.y) / r;

                    bool inside = false;
                    int tier = 1;
                    bool isStripe = false;
                    bool isCockpit = false;

                    if (style == 1) // Scout (Delta Arrow)
                    {
                        inside = nx >= -0.5f && nx <= 0.85f && ny <= (0.85f - nx) * 0.65f;
                        if (inside)
                        {
                            tier = (nx >= -0.1f && nx <= 0.7f && ny <= (0.85f - nx) * 0.28f) ? 2 : 1;
                            isStripe = Mathf.Abs((nx + ny * 1.1f) - 0.28f) < 0.055f && ny >= 0.15f;
                            isCockpit = (nx >= 0.10f && nx <= 0.40f && ny <= 0.12f);
                        }
                    }
                    else if (style == 2) // Fighter (Twin Wings)
                    {
                        inside = (nx >= -0.6f && nx <= 0.9f && ny <= (0.9f - nx) * 0.7f) ||
                                 (nx >= -0.8f && nx <= 0.2f && ny >= 0.4f && ny <= 0.85f);
                        if (inside)
                        {
                            tier = (nx >= -0.4f && nx <= 0.8f && ny <= 0.25f) ? 2 : 1;
                            isStripe = (Mathf.Abs(nx - 0.05f) < 0.05f && ny >= 0.35f && ny <= 0.82f) || (Mathf.Abs(nx - 0.45f) < 0.04f && ny <= 0.20f);
                            isCockpit = (nx >= 0.10f && nx <= 0.38f && ny <= 0.12f);
                        }
                    }
                    else if (style == 3) // Miner (Heavy Cruiser)
                    {
                        inside = (Mathf.Abs(p.x) / r <= 0.7f && ny <= 0.7f) || (p.x >= 0 && p.x <= r * 0.95f && ny <= 0.45f);
                        if (inside)
                        {
                            tier = (Mathf.Abs(nx) <= 0.45f && ny <= 0.45f) ? 2 : 1;
                            isStripe = (nx >= 0.45f && (Mathf.FloorToInt((x + y) * 0.4f) % 2 == 0));
                        }
                    }
                    else if (style == 4) // Drone (Diamond saucer)
                    {
                        inside = (Mathf.Abs(p.x) / (r * 0.8f) + ny / 0.8f) <= 1.0f;
                        if (inside)
                        {
                            tier = ((Mathf.Abs(nx) + ny) <= 0.55f) ? 2 : 1;
                            isStripe = Mathf.Abs((Mathf.Abs(nx) + ny) - 0.70f) < 0.06f;
                        }
                    }
                    else if (style == 5) // Interceptor (Viper Interceptor - sleek needle + swept wings)
                    {
                        bool needle = (nx >= -0.7f && nx <= 0.95f && ny <= (0.95f - nx) * 0.35f);
                        bool sweptWings = (nx >= -0.5f && nx <= 0.25f && ny <= 0.85f && ny >= 0.3f && (nx - ny * 0.35f) >= -0.45f);
                        bool engines = (nx >= -0.85f && nx <= -0.3f && ny >= 0.2f && ny <= 0.45f);
                        inside = needle || sweptWings || engines;
                        if (inside)
                        {
                            tier = needle ? 2 : 1;
                            isStripe = (Mathf.Abs((nx - ny * 0.4f) - (-0.25f)) < 0.05f && ny >= 0.35f);
                            isCockpit = (nx >= 0.12f && nx <= 0.42f && ny <= 0.10f);
                        }
                    }
                    else if (style == 6) // Juggernaut (Titan Juggernaut - armored tank chassis + prongs)
                    {
                        bool mainBlock = (Mathf.Abs(nx) <= 0.75f && ny <= 0.75f);
                        bool frontProngs = (nx >= 0.2f && nx <= 0.95f && ny >= 0.25f && ny <= 0.7f);
                        bool centerRam = (nx >= 0.6f && nx <= 0.9f && ny <= 0.25f);
                        bool rearPlates = (nx <= -0.4f && nx >= -0.9f && ny <= 0.85f);
                        inside = mainBlock || frontProngs || centerRam || rearPlates;
                        if (inside)
                        {
                            tier = (mainBlock && nx >= -0.2f && ny <= 0.5f) ? 2 : 1;
                            isStripe = (nx >= 0.4f && nx <= 0.6f && ny >= 0.25f) || (nx <= -0.5f && ny <= 0.6f && (x % 4 < 2));
                        }
                    }
                    else if (style == 7) // Boss (Dreadnought flagship)
                    {
                        bool core = (nx >= -0.85f && nx <= 0.9f && ny <= (0.95f - nx) * 0.75f);
                        bool claws = (nx >= -0.2f && nx <= 0.85f && ny >= 0.5f && ny <= 0.95f);
                        bool rearEngines = (nx <= -0.4f && nx >= -0.95f && ny >= 0.3f && ny <= 0.85f);
                        inside = core || claws || rearEngines;
                        if (inside)
                        {
                            tier = (core && nx >= -0.3f && ny <= 0.45f) ? 2 : 1;
                            isStripe = (Mathf.Abs(nx - 0.2f) < 0.05f && ny >= 0.45f) || (Mathf.Abs(nx - (-0.3f)) < 0.04f);
                        }
                    }
                    else if (style == 8) // Kamikaze (Razor Spear / Forward Arrowhead)
                    {
                        bool spear = (nx >= -0.7f && nx <= 0.95f && ny <= (0.95f - nx) * 0.45f);
                        bool barbWings = (nx >= -0.5f && nx <= -0.1f && ny <= 0.85f && (ny - (nx + 0.5f) * 0.8f) >= 0f);
                        inside = spear || barbWings;
                        if (inside)
                        {
                            tier = (spear && nx <= 0.5f && ny <= 0.2f) ? 2 : 1;
                            isStripe = Mathf.Abs((nx + ny * 0.9f) - 0.2f) < 0.05f;
                            isCockpit = (nx >= 0.05f && nx <= 0.30f && ny <= 0.10f);
                        }
                    }
                    else if (style == 9) // Sniper (Elongated needle railgun hull)
                    {
                        bool railgunBarrel = (nx >= 0.0f && nx <= 0.98f && ny <= 0.12f);
                        bool fuselage = (nx >= -0.8f && nx <= 0.3f && ny <= (0.5f - nx * 0.4f) * 0.45f);
                        bool stabilizerFins = (nx >= -0.75f && nx <= -0.2f && ny <= 0.75f && ny >= 0.25f);
                        inside = railgunBarrel || fuselage || stabilizerFins;
                        if (inside)
                        {
                            tier = railgunBarrel ? 2 : (fuselage ? 1 : 0);
                            isStripe = (Mathf.Abs(nx - (-0.1f)) < 0.04f && ny <= 0.6f);
                            isCockpit = (nx >= -0.15f && nx <= 0.15f && ny <= 0.14f);
                        }
                    }
                    else if (style == 10) // Support Shield (Hexagonal Bastion with core dome)
                    {
                        float hnx = Mathf.Abs(p.x) / (r * 0.85f);
                        float hny = Mathf.Abs(p.y) / (r * 0.85f);
                        bool hexagon = (hnx <= 0.85f && hny <= 0.85f && (hnx * 0.5f + hny * 0.866f) <= 0.95f);
                        bool coreCircle = (dist <= r * 0.45f);
                        inside = hexagon || coreCircle;
                        if (inside)
                        {
                            tier = coreCircle ? 2 : 1;
                            isStripe = (dist >= r * 0.48f && dist <= r * 0.62f);
                        }
                    }
                    else if (style == 11) // Vanguard Carrier (Dual flight bay catamaran)
                    {
                        bool bridge = (nx >= -0.7f && nx <= 0.4f && ny <= 0.35f);
                        bool bays = (nx >= -0.85f && nx <= 0.8f && ny >= 0.4f && ny <= 0.85f);
                        bool crossbars = (nx >= -0.3f && nx <= 0.2f && ny <= 0.8f);
                        inside = bridge || bays || crossbars;
                        if (inside)
                        {
                            tier = bridge ? 2 : 1;
                            isStripe = (Mathf.Abs(ny - 0.58f) < 0.035f && nx >= -0.7f && nx <= 0.65f);
                        }
                    }
                    else if (style == 12) // Shadow Phantom (Stealth delta with forward swept wings)
                    {
                        bool pFuselage = (nx >= -0.6f && nx <= 0.9f && ny <= (0.9f - nx) * 0.35f);
                        bool pWings = (nx >= -0.4f && nx <= 0.6f && ny <= 0.85f && (ny + nx * 0.4f) <= 0.9f && (ny + nx * 0.4f) >= 0.35f);
                        inside = pFuselage || pWings;
                        if (inside)
                        {
                            tier = (pFuselage && ny <= 0.18f) ? 2 : 1;
                            isStripe = (Mathf.Abs((ny + nx * 0.35f) - 0.58f) < 0.045f && ny >= 0.3f);
                            isCockpit = (nx >= 0.10f && nx <= 0.40f && ny <= 0.11f);
                        }
                    }
                    else if (style == 13) // Viper Frigate (Twin swept outriggers, forward prow lance)
                    {
                        bool lance = (nx >= 0.0f && nx <= 0.95f && ny <= 0.16f);
                        bool outriggers = (nx >= -0.7f && nx <= 0.65f && ny >= 0.25f && ny <= 0.68f && (nx - ny * 0.4f) >= -0.6f);
                        bool centerCore = (nx >= -0.5f && nx <= 0.35f && ny <= 0.35f);
                        inside = lance || outriggers || centerCore;
                        if (inside)
                        {
                            tier = (lance || centerCore) ? 2 : 1;
                            isStripe = (Mathf.Abs(ny - 0.12f) < 0.03f && nx >= 0.1f && nx <= 0.7f);
                            isCockpit = (nx >= 0.05f && nx <= 0.30f && ny <= 0.10f);
                        }
                    }
                    else if (style == 14) // Goliath Destroyer (Hammerhead prow, flank torpedo sponsons)
                    {
                        bool hammerhead = (nx >= 0.35f && nx <= 0.85f && ny <= 0.75f);
                        bool mainProw = (nx >= 0.80f && nx <= 0.95f && ny <= 0.30f);
                        bool flankPods = (nx >= -0.30f && nx <= 0.30f && ny >= 0.55f && ny <= 0.88f);
                        bool spine = (nx >= -0.85f && nx <= 0.40f && ny <= 0.45f);
                        inside = hammerhead || mainProw || flankPods || spine;
                        if (inside)
                        {
                            tier = (hammerhead && nx >= 0.45f && ny <= 0.4f) ? 2 : 1;
                            isStripe = (Mathf.Abs(nx - 0.62f) < 0.05f && ny >= 0.35f && ny <= 0.72f);
                        }
                    }
                    else if (style == 15) // Hyperion Cruiser (Armored chevron battleline, broadside sponsons)
                    {
                        bool citadel = (nx >= 0.15f && nx <= 0.95f && ny <= (0.95f - nx) * 0.55f);
                        bool armorWings = (nx >= -0.55f && nx <= 0.25f && ny <= 0.82f && ny >= 0.35f);
                        bool aftHull = (nx <= -0.35f && nx >= -0.92f && ny <= 0.58f);
                        inside = citadel || armorWings || aftHull;
                        if (inside)
                        {
                            tier = (citadel && nx <= 0.65f && ny <= 0.3f) ? 2 : 1;
                            isStripe = (ny <= 0.06f && nx >= -0.2f && nx <= 0.7f) || (Mathf.Abs(nx * 0.7f + ny * 0.6f - 0.25f) < 0.045f && ny >= 0.35f);
                        }
                    }
                    else if (style == 16) // Titan Dreadnought / Battleship (Spinal lance prow, massive citadel fortress)
                    {
                        bool spinalLance = (nx >= 0.40f && nx <= 1.0f && ny <= 0.18f);
                        bool citadelFortress = (nx >= -0.50f && nx <= 0.55f && ny <= 0.68f);
                        bool heavyArmorBelt = (nx >= -0.75f && nx <= 0.20f && ny >= 0.45f && ny <= 0.92f);
                        bool engineBlocks = (nx <= -0.45f && nx >= -0.98f && ny <= 0.78f);
                        inside = spinalLance || citadelFortress || heavyArmorBelt || engineBlocks;
                        if (inside)
                        {
                            tier = (spinalLance || (citadelFortress && ny <= 0.38f)) ? 2 : 1;
                            isStripe = (Mathf.Abs((nx * 0.7f + ny * 0.6f) - 0.35f) < 0.045f && ny >= 0.25f) || (Mathf.Abs(nx - 0.05f) < 0.04f && ny <= 0.55f);
                        }
                    }

                    if (!inside)
                    {
                        cols[y * size + x] = Color.clear;
                        continue;
                    }

                    // 1. Base hull shading & tier elevation
                    Color currentCol = hullColor;
                    if (tier == 2)
                    {
                        currentCol = Color.Lerp(currentCol, Color.white, 0.08f);
                    }
                    else if (tier == 0)
                    {
                        currentCol *= 0.88f;
                    }

                    // 2. Armor plating / panel lines & seams
                    int panelFreq = isCapital ? 9 : 6;
                    float panelCoordX = (nx + 1.5f) * panelFreq;
                    float panelCoordY = (ny + 1.5f) * panelFreq;
                    int px = Mathf.FloorToInt(panelCoordX);
                    int py = Mathf.FloorToInt(panelCoordY);
                    float fracX = panelCoordX - px;
                    float fracY = panelCoordY - py;

                    float plateRand = Hash2D(px, py, style * 19);
                    float plateTone = (plateRand - 0.5f) * 0.12f;
                    currentCol = Color.Lerp(currentCol, Color.white, plateTone);

                    bool isSeam = (fracX < 0.09f || fracY < 0.09f);
                    bool isSeamBevel = (fracX >= 0.09f && fracX < 0.16f) || (fracY >= 0.09f && fracY < 0.16f);
                    if (isSeam)
                    {
                        currentCol *= 0.68f;
                    }
                    else if (isSeamBevel)
                    {
                        currentCol = Color.Lerp(currentCol, Color.white, 0.12f);
                    }

                    // 3. Greebles (vents, conduit piping, access hatches)
                    if (nx < -0.15f && nx > -0.65f && ny >= 0.15f && ny <= 0.45f && (x % 3 == 0))
                    {
                        currentCol *= 0.65f;
                    }
                    if (Mathf.Abs(ny - 0.25f) < 0.025f && nx >= -0.5f && nx <= 0.4f)
                    {
                        currentCol *= 0.72f;
                    }
                    if (plateRand > 0.72f && fracX > 0.35f && fracX < 0.65f && fracY > 0.35f && fracY < 0.65f)
                    {
                        currentCol = (fracX < 0.42f || fracX > 0.58f || fracY < 0.42f || fracY > 0.58f)
                            ? Color.Lerp(currentCol, Color.white, 0.18f)
                            : currentCol * 0.75f;
                    }

                    // 4. Grime, oil streaks & chipped paint
                    float grime = Hash2D((int)(y * 1.6f), 0, 71);
                    if (grime > 0.80f && nx < 0.2f)
                    {
                        currentCol *= 0.82f;
                    }
                    float chipNoise = Hash2D(x, y, 113);
                    if (isSeam && chipNoise > 0.82f)
                    {
                        currentCol = new Color(0.36f, 0.38f, 0.40f);
                    }

                    // 5. Engine exhaust scorch marks & nozzle bloom
                    if (nx < -0.3f)
                    {
                        float scorch = Mathf.Clamp01((-nx - 0.3f) / 0.65f);
                        currentCol = Color.Lerp(currentCol, currentCol * 0.35f, scorch * 0.7f);
                    }
                    if (nx <= -0.75f && ny <= 0.5f)
                    {
                        if (nx <= -0.85f)
                        {
                            currentCol = Color.Lerp(new Color(0.15f, 0.75f, 1.0f), Color.white, (-nx - 0.85f) * 6f);
                        }
                        else
                        {
                            currentCol = new Color(0.12f, 0.12f, 0.15f);
                        }
                    }

                    // 6. Faction accent stripes
                    if (isStripe)
                    {
                        Color stripeBase = Color.Lerp(stripeColor, Color.white, plateTone * 0.5f);
                        if (isSeam) stripeBase *= 0.70f;
                        if (chipNoise > 0.84f) stripeBase = new Color(0.36f, 0.38f, 0.40f);
                        currentCol = stripeBase;
                    }

                    // 7. Scale: Tiny glowing windows vs Pilot cockpit canopy
                    if (isCapital)
                    {
                        int localDeckY = Mathf.Abs(y - (int)center.y);
                        bool isWindow = false;
                        Color winColor = new Color(1.0f, 0.96f, 0.78f, 1f);

                        if (style == 16) // Battleship
                        {
                            if ((localDeckY == 5 || localDeckY == 10 || localDeckY == 16 || localDeckY == 22) &&
                                nx >= -0.35f && nx <= 0.42f && (x % 3 == 0) && Hash2D(x, y, 314) > 0.22f)
                            {
                                isWindow = true;
                                winColor = (localDeckY == 5) ? new Color(0.70f, 0.95f, 1.0f, 1f) : new Color(1.0f, 0.96f, 0.78f, 1f);
                            }
                        }
                        else if (style == 15) // Cruiser
                        {
                            if ((localDeckY == 6 || localDeckY == 12 || localDeckY == 18) &&
                                nx >= -0.25f && nx <= 0.38f && (x % 3 == 0) && Hash2D(x, y, 271) > 0.25f)
                            {
                                isWindow = true;
                            }
                        }
                        else if (style == 11) // Carrier
                        {
                            if (localDeckY >= 21 && localDeckY <= 23 && nx >= -0.6f && nx <= 0.6f && (x % 4 == 0))
                            {
                                isWindow = true;
                                winColor = (x % 8 == 0) ? new Color(0.2f, 1.0f, 0.4f, 1f) : new Color(1.0f, 0.8f, 0.15f, 1f);
                            }
                            else if (localDeckY <= 4 && nx >= 0.05f && nx <= 0.25f && (x % 2 == 0))
                            {
                                isWindow = true;
                                winColor = new Color(0.75f, 0.95f, 1.0f, 1f);
                            }
                        }
                        else if (style == 14) // Destroyer
                        {
                            if (localDeckY <= 3 && nx >= 0.48f && nx <= 0.70f && (x % 2 == 0))
                            {
                                isWindow = true;
                                winColor = new Color(0.85f, 0.95f, 1.0f, 1f);
                            }
                            else if (localDeckY >= 22 && localDeckY <= 24 && nx >= -0.1f && nx <= 0.15f && (x % 4 == 0))
                            {
                                isWindow = true;
                                winColor = new Color(1.0f, 0.25f, 0.2f, 1f);
                            }
                        }
                        else if (style == 6 || style == 7) // Juggernaut & Boss
                        {
                            if ((localDeckY == 8 || localDeckY == 16) && nx >= -0.2f && nx <= 0.3f && (x % 3 == 0))
                            {
                                isWindow = true;
                                winColor = (style == 7) ? new Color(1.0f, 0.2f, 0.2f, 1f) : new Color(1.0f, 0.85f, 0.2f, 1f);
                            }
                        }

                        if (isWindow)
                        {
                            currentCol = winColor;
                        }
                    }
                    else
                    {
                        if (isCockpit)
                        {
                            bool isFrame = (nx < 0.11f || nx > 0.36f || ny > 0.09f);
                            if (isFrame)
                            {
                                currentCol = Color.Lerp(hullColor, Color.white, 0.25f);
                            }
                            else
                            {
                                float glint = Mathf.Abs((p.x - p.y * 1.4f) - r * 0.16f);
                                if (glint < 1.1f)
                                {
                                    currentCol = new Color(0.85f, 0.95f, 1.0f, 1f);
                                }
                                else
                                {
                                    currentCol = Color.Lerp(new Color(0.08f, 0.12f, 0.18f), stripeColor * 0.7f, 0.35f);
                                }
                            }
                        }
                    }

                    cols[y * size + x] = currentCol;
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

                    float xDist = (nx > 0f) ? nx : Mathf.Abs(nx) * 0.75f;
                    float d = Mathf.Sqrt(xDist * xDist * 0.28f + ny * ny);

                    if (d < 1.0f)
                    {
                        Color pixelColor;
                        if (d < 0.28f)
                        {
                            // Ultra-hot blinding pure white core
                            pixelColor = Color.Lerp(Color.white, color, (d / 0.28f) * 0.15f);
                            pixelColor.a = 1f;
                        }
                        else if (d < 0.65f)
                        {
                            // Intense saturated chromatic mantle
                            float t = (d - 0.28f) / 0.37f;
                            pixelColor = Color.Lerp(color, color * 0.85f, t);
                            pixelColor.a = 1f;
                        }
                        else
                        {
                            // Soft blooming halo / chromatic corona falloff
                            float t = (d - 0.65f) / 0.35f;
                            float falloff = Mathf.Pow(1f - t, 1.6f);
                            pixelColor = new Color(color.r, color.g, color.b, falloff * 0.9f);
                        }
                        cols[y * w + x] = pixelColor;
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
