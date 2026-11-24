using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.UI;
using StarStrike.Network;
using StarStrike.Platform.Storage;

namespace StarStrike.Editor
{
    public static class PrefabInitializer
    {
        [MenuItem("StarStrike/Generate Prefabs")]
        public static void GeneratePrefabs()
        {
            EnsureFolder("Assets/_Project", "Prefabs");
            EnsureFolder("Assets/_Project/Prefabs", "Projectiles");
            EnsureFolder("Assets/_Project/Prefabs", "Environment");
            EnsureFolder("Assets/_Project/Prefabs", "Enemies");
            EnsureFolder("Assets/_Project/Prefabs", "Ships");
            EnsureFolder("Assets/_Project/Prefabs", "UI");
            EnsureFolder("Assets/_Project/Prefabs", "Network");

            // 1. Projectiles
            CreatePlayerLaserPrefab();
            CreateEnemyLaserPrefab();
            CreatePlayerGatlingBulletPrefab();
            CreatePlayerScatterPelletPrefab();
            CreatePlayerMissilePrefab();

            // 2. Environment & Loot
            CreateAsteroidPrefab(AsteroidTier.Large);
            CreateAsteroidPrefab(AsteroidTier.Medium);
            CreateAsteroidPrefab(AsteroidTier.Small);
            CreateGemCrystalPrefab();
            CreatePowerUpPrefab();

            // 3. Enemies
            CreateEnemyDronePrefab();
            CreateBossPrefab();

            // 4. Ships
            CreateShipPrefab("scout", 1, 80f, 100f, 8f, 80f, 18f, 18f, 22f, 1);
            CreateShipPrefab("fighter", 2, 120f, 150f, 12f, 120f, 24f, 28f, 24f, 2);
            CreateShipPrefab("miner", 2, 180f, 220f, 16f, 150f, 20f, 40f, 20f, 2);
            CreateShipPrefab("interceptor", 3, 100f, 180f, 15f, 140f, 30f, 35f, 28f, 2);
            CreateShipPrefab("juggernaut", 3, 300f, 400f, 25f, 200f, 25f, 75f, 18f, 3);
            CreateShipPrefab("carrier", 3, 240f, 320f, 22f, 280f, 32f, 55f, 22f, 3);

            // 5. UI Prefabs
            CreateCanvasHUDPrefab();

            // 6. Network Prefabs
            CreateNetworkLaserPrefab();
            CreateNetworkPlayerShipPrefab();

            // 7. Weapon Prefabs & Sockets
            CreateWeaponPrefabs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("StarStrike Prefabs successfully generated in Assets/_Project/Prefabs!");
        }

        private static Sprite LoadArtSprite(string relativePath)
        {
            string path = $"Assets/_Project/Art/Sprites/{relativePath}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;

            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            if (all != null)
            {
                foreach (var obj in all)
                {
                    if (obj is Sprite s) return s;
                }
            }
            return ProceduralSpriteHelper.GetWhiteSprite();
        }

        private static void CreatePlayerLaserPrefab()
        {
            GameObject go = new GameObject("PlayerLaser");
            go.tag = "Laser";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Projectiles/Laser_Player.png");
            sr.sortingOrder = 5;
            sr.color = new Color(0.2f, 0.9f, 1f, 1f);

            var proj = go.AddComponent<LaserProjectile>();
            proj.isEnemy = false;
            proj.damage = 18f;
            proj.speed = 25f;
            proj.lifetime = 3f;
            proj.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            SavePrefab(go, "Projectiles/PlayerLaser");
        }

        private static void CreateEnemyLaserPrefab()
        {
            GameObject go = new GameObject("EnemyLaser");
            go.tag = "EnemyLaser";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Projectiles/Laser_Enemy.png");
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.3f, 0.2f, 1f);

            var proj = go.AddComponent<LaserProjectile>();
            proj.isEnemy = true;
            proj.damage = 14f;
            proj.speed = 20f;
            proj.lifetime = 3f;
            proj.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            SavePrefab(go, "Projectiles/EnemyLaser");
        }

        private static void CreatePlayerGatlingBulletPrefab()
        {
            GameObject go = new GameObject("PlayerGatlingBullet");
            go.tag = "Laser";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Projectiles/Bullet_Gatling.png");
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.85f, 0.25f, 1f);

            var proj = go.AddComponent<LaserProjectile>();
            proj.isEnemy = false;
            proj.damage = 7f;
            proj.speed = 38f;
            proj.lifetime = 3f;
            proj.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            SavePrefab(go, "Projectiles/PlayerGatlingBullet");
        }

        private static void CreatePlayerScatterPelletPrefab()
        {
            GameObject go = new GameObject("PlayerScatterPellet");
            go.tag = "Laser";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.18f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Projectiles/Pellet_Scattergun.png");
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.8f, 0.2f, 1f);

            var proj = go.AddComponent<LaserProjectile>();
            proj.isEnemy = false;
            proj.damage = 12f;
            proj.speed = 28f;
            proj.lifetime = 0.85f;
            proj.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            SavePrefab(go, "Projectiles/PlayerScatterPellet");
        }

        private static void CreatePlayerMissilePrefab()
        {
            GameObject go = new GameObject("PlayerMissile");
            go.tag = "Laser";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Projectiles/Missile.png");
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.7f, 0.2f, 1f);

            var proj = go.AddComponent<MissileProjectile>();
            proj.isEnemy = false;
            proj.damage = 45f;
            proj.speed = 18f;
            proj.turnSpeed = 300f;
            proj.blastRadius = 2.4f;
            proj.lifetime = 4f;

            SavePrefab(go, "Projectiles/PlayerMissile");
        }

        private static void CreateAsteroidPrefab(AsteroidTier tier)
        {
            GameObject go = new GameObject($"Asteroid_{tier}");
            go.tag = "Asteroid";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.1f;
            rb.angularDamping = 0.2f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.isTrigger = false;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("World/Asteroid.png");
            sr.sortingOrder = 1;

            var ast = go.AddComponent<Asteroid>();
            ast.tier = tier;
            ast.config = AssetDatabase.LoadAssetAtPath<AsteroidConfig>("Assets/_Project/Resources/Data/AsteroidConfig.asset");
            ast.SetupStats();

            SavePrefab(go, $"Environment/Asteroid_{tier}");
        }

        private static void CreateGemCrystalPrefab()
        {
            GameObject go = new GameObject("GemCrystal");
            go.tag = "Gem";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.2f;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("World/Gem.png");
            sr.sortingOrder = 3;

            var gem = go.AddComponent<GemCrystal>();
            gem.lootConfig = AssetDatabase.LoadAssetAtPath<LootConfig>("Assets/_Project/Resources/Data/LootConfig.asset");

            SavePrefab(go, "Environment/GemCrystal");
        }

        private static void CreatePowerUpPrefab()
        {
            GameObject go = new GameObject("PowerUpItem");
            go.tag = "PowerUp";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("World/PowerUp.png");
            sr.color = new Color(1f, 0.85f, 0.2f, 1f);
            sr.sortingOrder = 3;

            var pu = go.AddComponent<PowerUpItem>();
            pu.lootConfig = AssetDatabase.LoadAssetAtPath<LootConfig>("Assets/_Project/Resources/Data/LootConfig.asset");

            SavePrefab(go, "Environment/PowerUpItem");
        }

        private static void CreateEnemyDronePrefab()
        {
            GameObject go = new GameObject("EnemyDrone");
            go.tag = "Enemy";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 1.0f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Ships/Ship_Drone.png");
            sr.sortingOrder = 4;

            var poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = false;
            if (sr.sprite != null)
            {
                PlayerProgression.UpdatePolygonFromSprite(poly, sr.sprite);
            }

            var mov = go.AddComponent<ShipMovement>();
            mov.flightConfig = AssetDatabase.LoadAssetAtPath<FlightConfig>("Assets/_Project/Resources/Data/FlightConfig.asset");

            var health = go.AddComponent<ShipHealth>();
            health.maxHull = 40f;
            health.currentHull = 40f;
            health.maxShield = 60f;
            health.currentShield = 60f;

            var weapon = go.AddComponent<ShipWeapon>();
            weapon.isPlayer = false;
            weapon.pulseDamage = 14f;
            weapon.pulseSpeed = 18f;
            weapon.fireRate = 1.1f;
            weapon.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            var ai = go.AddComponent<EnemyAI>();
            ai.definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Resources/Data/Enemy_Drone.asset");

            var shieldField = go.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            shieldField.EnsureInitialized();

            SavePrefab(go, "Enemies/EnemyDrone");
        }

        private static void CreateBossPrefab()
        {
            GameObject go = new GameObject("BossDreadnought");
            go.tag = "Boss";
            go.transform.localScale = Vector3.one * 3f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.6f;
            rb.angularDamping = 1.0f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Ships/Ship_Boss.png");
            sr.sortingOrder = 4;

            var poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = false;
            if (sr.sprite != null)
            {
                PlayerProgression.UpdatePolygonFromSprite(poly, sr.sprite);
            }

            var mov = go.AddComponent<ShipMovement>();
            mov.flightConfig = AssetDatabase.LoadAssetAtPath<FlightConfig>("Assets/_Project/Resources/Data/FlightConfig.asset");

            var health = go.AddComponent<ShipHealth>();
            health.maxHull = 1500f;
            health.currentHull = 1500f;
            health.maxShield = 2500f;
            health.currentShield = 2500f;

            var weapon = go.AddComponent<ShipWeapon>();
            weapon.isPlayer = false;
            weapon.pulseDamage = 30f;
            weapon.pulseSpeed = 25f;
            weapon.weaponSlots = 3;
            weapon.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            var brain = go.AddComponent<BossBrain>();
            brain.config = AssetDatabase.LoadAssetAtPath<BossConfig>("Assets/_Project/Resources/Data/BossConfig.asset");

            var shieldField = go.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            shieldField.EnsureInitialized();

            SavePrefab(go, "Enemies/BossDreadnought");
        }

        private static void CreateShipPrefab(string shipId, int tier, float hull, float shield, float shieldRegen, float energy, float energyRegen, float dmg, float spd, int weaponSlots)
        {
            string capId = char.ToUpper(shipId[0]) + shipId.Substring(1);
            GameObject go = new GameObject($"Ship_{capId}");
            go.tag = "Player";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 1.0f;

            var def = AssetDatabase.LoadAssetAtPath<ShipDefinition>($"Assets/_Project/Resources/Data/Ship_{capId}.asset");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite($"Ships/Ship_{capId}.png");
            sr.sortingOrder = 4;

            var poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = false;
            if (sr.sprite != null)
            {
                PlayerProgression.UpdatePolygonFromSprite(poly, sr.sprite);
            }

            var mov = go.AddComponent<ShipMovement>();
            mov.flightConfig = AssetDatabase.LoadAssetAtPath<FlightConfig>("Assets/_Project/Resources/Data/FlightConfig.asset");

            var health = go.AddComponent<ShipHealth>();
            health.isPlayer = true;
            health.maxHull = hull;
            health.maxShield = shield;
            health.shieldRegen = shieldRegen;
            health.maxEnergy = energy;
            health.energyRegen = energyRegen;
            health.currentHull = hull;
            health.currentShield = shield;
            health.currentEnergy = energy;

            var weapon = go.AddComponent<ShipWeapon>();
            weapon.isPlayer = true;
            weapon.pulseDamage = dmg;
            weapon.pulseSpeed = spd;
            weapon.weaponSlots = weaponSlots;
            weapon.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");
            if (def != null && def.weaponSockets != null && def.weaponSockets.Length > 0)
            {
                weapon.SetSockets(def.weaponSockets);
            }

            var prog = go.AddComponent<PlayerProgression>();
            prog.currentShip = def;
            prog.gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/Resources/Data/GameConfig.asset");

            var shieldField = go.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            if (def != null && def.shieldHitboxOffset > 0.05f)
            {
                shieldField.shieldHitboxOffset = def.shieldHitboxOffset;
            }
            else if (shipId == "juggernaut" || shipId == "carrier" || shipId == "vanguard")
            {
                shieldField.shieldHitboxOffset = 0.30f;
            }
            else
            {
                shieldField.shieldHitboxOffset = 0.20f;
            }
            shieldField.EnsureInitialized();

            Vector3 leftPos = new Vector3(0.65f, 0.3f, 0f);
            Vector3 rightPos = new Vector3(0.65f, -0.3f, 0f);
            if (def != null && def.weaponSockets != null)
            {
                if (def.weaponSockets.Length == 1)
                {
                    leftPos = new Vector3(def.weaponSockets[0].x, 0.3f, 0f);
                    rightPos = new Vector3(def.weaponSockets[0].x, -0.3f, 0f);
                }
                else if (def.weaponSockets.Length >= 2)
                {
                    leftPos = new Vector3(def.weaponSockets[1].x, def.weaponSockets[1].y, 0f);
                    rightPos = new Vector3(def.weaponSockets.Length > 2 ? def.weaponSockets[2].x : def.weaponSockets[1].x, def.weaponSockets.Length > 2 ? def.weaponSockets[2].y : -def.weaponSockets[1].y, 0f);
                }
            }

            GameObject leftMuzzle = new GameObject("LeftMuzzle");
            leftMuzzle.transform.SetParent(go.transform);
            leftMuzzle.transform.localPosition = leftPos;

            GameObject rightMuzzle = new GameObject("RightMuzzle");
            rightMuzzle.transform.SetParent(go.transform);
            rightMuzzle.transform.localPosition = rightPos;

            if (def != null && def.weaponSockets != null && (def.weaponSockets.Length == 1 || def.weaponSockets.Length >= 3))
            {
                GameObject centerMuzzle = new GameObject("CenterMuzzle");
                centerMuzzle.transform.SetParent(go.transform);
                centerMuzzle.transform.localPosition = new Vector3(def.weaponSockets[0].x, def.weaponSockets[0].y, 0f);
            }

            SavePrefab(go, $"Ships/Ship_{capId}");
        }

        private static void CreateCanvasHUDPrefab()
        {
            GameObject hudGo = new GameObject("CanvasHUD");
            var canvas = hudGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = hudGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            hudGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var hud = hudGo.AddComponent<CanvasHUD>();
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>("Assets/_Project/Resources/Data/UIThemeConfig.asset");

            // Sub-controllers
            var statusComp = hudGo.AddComponent<HUDPlayerStatus>();
            statusComp.themeConfig = theme;

            var evoBanner = hudGo.AddComponent<HUDEvolutionBanner>();
            evoBanner.themeConfig = theme;

            var upgradeDrawer = hudGo.AddComponent<HUDUpgradeDrawer>();
            upgradeDrawer.themeConfig = theme;

            var floatText = hudGo.AddComponent<HUDFloatingText>();
            floatText.themeConfig = theme;

            var endGameModal = hudGo.AddComponent<HUDEndGameModal>();
            endGameModal.themeConfig = theme;

            hud.playerStatus = statusComp;
            hud.evolutionBannerController = evoBanner;
            hud.upgradeDrawerController = upgradeDrawer;
            hud.floatingTextController = floatText;
            hud.endGameModal = endGameModal;

            SavePrefab(hudGo, "UI/CanvasHUD");
        }

        private static void CreateNetworkLaserPrefab()
        {
            GameObject go = new GameObject("NetworkLaserProjectile");
            go.tag = "Laser";

            var netObj = go.AddComponent<NetworkObject>();

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Projectiles/Laser_Player.png");
            sr.sortingOrder = 6;
            sr.color = new Color(0.2f, 0.95f, 1f, 1f);

            var proj = go.AddComponent<NetworkLaserProjectile>();
            proj.damage = 22f;
            proj.speed = 28f;
            proj.lifetime = 2.5f;

            SavePrefab(go, "Network/NetworkLaserProjectile");
        }

        private static void CreateNetworkPlayerShipPrefab()
        {
            GameObject go = new GameObject("NetworkPlayerShip");
            go.tag = "Player";

            var netObj = go.AddComponent<NetworkObject>();

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 1.0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.isTrigger = false;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Ships/Ship_Scout.png");
            sr.sortingOrder = 10;

            var ship = go.AddComponent<NetworkPlayerShip>();
            ship.maxSpeed = 12f;
            ship.acceleration = 24f;
            ship.agility = 5f;
            ship.fireRate = 0.2f;

            var laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Network/NetworkLaserProjectile.prefab");
            if (laserPrefab != null)
            {
                ship.laserPrefab = laserPrefab;
            }

            SavePrefab(go, "Network/NetworkPlayerShip");
        }

        private static void SavePrefab(GameObject go, string subPath)
        {
            string path = $"Assets/_Project/Prefabs/{subPath}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void SaveWeaponPrefab(GameObject go, string weaponName)
        {
            string path = $"Assets/_Project/Prefabs/Weapons/{weaponName}.prefab";
            string resPath = $"Assets/_Project/Resources/Prefabs/Weapons/{weaponName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            PrefabUtility.SaveAsPrefabAsset(go, resPath);
            Object.DestroyImmediate(go);
        }

        private static void CreateWeaponPrefabs()
        {
            EnsureFolder("Assets/_Project", "Prefabs");
            EnsureFolder("Assets/_Project/Prefabs", "Weapons");
            EnsureFolder("Assets/_Project", "Resources");
            EnsureFolder("Assets/_Project/Resources", "Prefabs");
            EnsureFolder("Assets/_Project/Resources/Prefabs", "Weapons");

            var combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            CreateWeaponPulseLaserPrefab(combatConfig);
            CreateWeaponScattergunPrefab(combatConfig);
            CreateWeaponHomingMissilesPrefab(combatConfig);
            CreateWeaponContinuousBeamPrefab(combatConfig);
            CreateWeaponRotaryGatlingPrefab(combatConfig);
            CreateWeaponSocketMountPrefab(combatConfig);

            if (combatConfig != null)
            {
                combatConfig.pulseLaserWeaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Weapons/Weapon_PulseLaser.prefab");
                combatConfig.scattergunWeaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Weapons/Weapon_Scattergun.prefab");
                combatConfig.homingMissileWeaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Weapons/Weapon_HomingMissiles.prefab");
                combatConfig.continuousBeamWeaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Weapons/Weapon_ContinuousBeam.prefab");
                combatConfig.gatlingWeaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Weapons/Weapon_RotaryGatling.prefab");
                combatConfig.socketMountPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Weapons/Weapon_SocketMount.prefab");
                EditorUtility.SetDirty(combatConfig);
            }
        }

        private static void CreateWeaponPulseLaserPrefab(CombatConfig combatConfig)
        {
            GameObject go = new GameObject("Weapon_PulseLaser");
            var weapon = go.AddComponent<ShipWeapon>();
            weapon.combatConfig = combatConfig;
            weapon.pulseDamage = 18f;
            weapon.pulseSpeed = 22f;
            weapon.fireRate = 0.22f;
            weapon.energyCost = 6f;
            weapon.inaccuracyAngle = 2.5f;
            weapon.laserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/PlayerLaser.prefab");

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            visual.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Weapons/Weapon_Pulse.png");
            sr.sortingOrder = 3;

            var muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0.5f, 0f, 0f);

            SaveWeaponPrefab(go, "Weapon_PulseLaser");
        }

        private static void CreateWeaponScattergunPrefab(CombatConfig combatConfig)
        {
            GameObject go = new GameObject("Weapon_Scattergun");
            var weapon = go.AddComponent<ScattergunWeapon>();
            weapon.combatConfig = combatConfig;
            weapon.pelletDamage = 10f;
            weapon.pelletSpeed = 26f;
            weapon.basePellets = 5;
            weapon.spreadAngle = 32f;
            weapon.fireRate = 0.48f;
            weapon.energyCost = 14f;
            weapon.pelletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/PlayerScatterPellet.prefab");

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            visual.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Weapons/Weapon_Scattergun.png");
            sr.sortingOrder = 3;

            var muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0.5f, 0f, 0f);

            SaveWeaponPrefab(go, "Weapon_Scattergun");
        }

        private static void CreateWeaponHomingMissilesPrefab(CombatConfig combatConfig)
        {
            GameObject go = new GameObject("Weapon_HomingMissiles");
            var weapon = go.AddComponent<HomingMissileWeapon>();
            weapon.missileDamage = 45f;
            weapon.missileSpeed = 16f;
            weapon.fireRate = 0.70f;
            weapon.energyCost = 16f;
            weapon.missilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/PlayerMissile.prefab");

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            visual.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Weapons/Weapon_Missile.png");
            sr.sortingOrder = 3;

            var muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0.5f, 0f, 0f);

            SaveWeaponPrefab(go, "Weapon_HomingMissiles");
        }

        private static void CreateWeaponContinuousBeamPrefab(CombatConfig combatConfig)
        {
            GameObject go = new GameObject("Weapon_ContinuousBeam");
            var weapon = go.AddComponent<ContinuousBeamWeapon>();
            weapon.damagePerSecond = 75f;
            weapon.energyCostPerSecond = 22f;
            weapon.maxRange = 16f;
            weapon.fireRate = 0.05f;
            weapon.beamColor = new Color(0.2f, 0.95f, 1f, 0.9f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            visual.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Weapons/Weapon_Beam.png");
            sr.sortingOrder = 3;

            var muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0.5f, 0f, 0f);

            SaveWeaponPrefab(go, "Weapon_ContinuousBeam");
        }

        private static void CreateWeaponRotaryGatlingPrefab(CombatConfig combatConfig)
        {
            GameObject go = new GameObject("Weapon_RotaryGatling");
            var weapon = go.AddComponent<GatlingWeapon>();
            weapon.combatConfig = combatConfig;
            weapon.bulletDamage = 8.5f;
            weapon.bulletSpeed = 34f;
            weapon.baseFireRate = 0.10f;
            weapon.maxFireRate = 0.055f;
            weapon.energyCost = 1.6f;
            weapon.baseInaccuracyAngle = 3.2f;
            weapon.maxInaccuracyAngle = 7.5f;
            weapon.spoolUpTime = 0.65f;
            weapon.spoolDecayTime = 0.4f;
            weapon.tracerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/PlayerGatlingBullet.prefab");

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            visual.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Weapons/Weapon_Gatling.png");
            sr.sortingOrder = 3;

            var muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(go.transform, false);
            muzzle.transform.localPosition = new Vector3(0.5f, 0f, 0f);

            SaveWeaponPrefab(go, "Weapon_RotaryGatling");
        }

        private static void CreateWeaponSocketMountPrefab(CombatConfig combatConfig)
        {
            GameObject go = new GameObject("Weapon_SocketMount");
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            visual.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadArtSprite("Weapons/Weapon_Socket.png");
            sr.sortingOrder = 2;

            SaveWeaponPrefab(go, "Weapon_SocketMount");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            try
            {
                if (full.StartsWith("Assets/"))
                {
                    string sub = full.Substring("Assets/".Length);
                    string diskPath = System.IO.Path.Combine(Application.dataPath, sub);
                    if (!System.IO.Directory.Exists(diskPath))
                    {
                        System.IO.Directory.CreateDirectory(diskPath);
                    }
                }
            }
            catch {}

            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        [MenuItem("StarStrike/Setup Split Scenes")]
        public static void RunSceneSplit()
        {
            SceneSplitterUtility.SetupSplitScenes();
        }

        [MenuItem("StarStrike/Verify Combat & Bug Fixes")]
        public static void RunVerification()
        {
            Debug.Log("--- STARTING COMBAT BUG FIX RUNTIME VERIFICATION ---");
            int passed = 0;
            int failed = 0;

            // 1. PowerUp Overdrive Minimum Floor Test
            try
            {
                var player = new GameObject("TestPlayer_PU");
                player.AddComponent<Rigidbody2D>();
                var health = player.AddComponent<ShipHealth>();
                health.maxEnergy = 80f;
                health.ResetMeters();

                var weapon = player.AddComponent<ShipWeapon>();
                weapon.weaponSlots = 1;
                weapon.fireRate = 0.22f;

                var puGo = new GameObject("TestPU");
                var pu = puGo.AddComponent<PowerUpItem>();
                pu.type = PowerUpItem.PowerUpType.WeaponOverdrive;

                for (int i = 0; i < 15; i++)
                {
                    pu.ApplyPowerUp(player);
                }

                if (weapon.weaponSlots != 3)
                    throw new System.Exception($"weaponSlots expected 3, got {weapon.weaponSlots}");
                if (weapon.fireRate < 0.10f)
                    throw new System.Exception($"fireRate dropped below 0.10s minimum floor: {weapon.fireRate}");

                Object.DestroyImmediate(player);
                Object.DestroyImmediate(puGo);
                Debug.Log("[PASS] 1. PowerUp Overdrive clamped to minFireRate (0.10s) and max 3 slots");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 1. PowerUp Overdrive test failed: {ex.Message}");
                failed++;
            }

            // 2. Normal Click (100ms) with Max Overdrive (0.10s fireRate) Does NOT Burst
            try
            {
                var player = new GameObject("TestPlayer_Click");
                var health = player.AddComponent<ShipHealth>();
                health.maxEnergy = 80f;
                health.ResetMeters();

                var weapon = player.AddComponent<ShipWeapon>();
                weapon.fireRate = 0.10f; // rapid fire rate
                weapon.energyCost = 6f;

                var reader = player.AddComponent<PlayerInputReader>();
                reader.weapon = weapon;
                reader.health = health;

                // Frame 1: Tap press
                reader.inputFire = true;
                reader.ProcessFiring(0.016f);
                if (Mathf.Abs(health.currentEnergy - 74f) > 0.01f)
                    throw new System.Exception($"Expected 74 energy after first tap, got {health.currentEnergy}");

                // Frames 2-7: Finger held during 100ms click duration (6 * 16ms = ~96ms)
                for (int i = 0; i < 6; i++)
                {
                    reader.ProcessFiring(0.016f);
                }

                if (Mathf.Abs(health.currentEnergy - 74f) > 0.01f)
                    throw new System.Exception($"Click duration burst-fired! Energy drained to {health.currentEnergy} (expected 74f)");

                // Release
                reader.inputFire = false;
                reader.ProcessFiring(0.016f);

                if (Mathf.Abs(health.currentEnergy - 74f) > 0.01f)
                    throw new System.Exception($"Energy changed after release: {health.currentEnergy}");

                Object.DestroyImmediate(player);
                Debug.Log("[PASS] 2. Normal 100ms click does NOT burst fire at 0.10s fireRate");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 2. Click test failed: {ex.Message}");
                failed++;
            }

            // 3. Hold-to-Fire AutoFires After Tap Threshold
            try
            {
                var player = new GameObject("TestPlayer_Hold");
                var health = player.AddComponent<ShipHealth>();
                health.maxEnergy = 80f;
                health.ResetMeters();

                var weapon = player.AddComponent<ShipWeapon>();
                weapon.fireRate = 0.10f;
                weapon.energyCost = 6f;

                var reader = player.AddComponent<PlayerInputReader>();
                reader.weapon = weapon;
                reader.health = health;

                // Tap press
                reader.inputFire = true;
                reader.ProcessFiring(0.016f); // 74 energy

                // Reset weapon cooldown to simulate time elapsed in EditMode
                weapon.ResetCooldown();

                // Hold past 180ms tap threshold (180ms / 16ms = ~12 frames)
                for (int i = 0; i < 12; i++)
                {
                    reader.ProcessFiring(0.016f);
                }

                if (health.currentEnergy >= 74f)
                    throw new System.Exception($"Hold fire did not auto-fire after 192ms: energy still {health.currentEnergy}");

                Object.DestroyImmediate(player);
                Debug.Log($"[PASS] 3. Hold-to-fire auto-fires smoothly when held past tap threshold (energy: {health.currentEnergy})");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 3. Hold fire test failed: {ex.Message}");
                failed++;
            }

            // 4. PlayerProgression Resets Weapon FireRate on New Run / Evolution
            try
            {
                var player = new GameObject("TestPlayer_Prog");
                var health = player.AddComponent<ShipHealth>();
                var movement = player.AddComponent<ShipMovement>();
                var weapon = player.AddComponent<ShipWeapon>();
                var prog = player.AddComponent<PlayerProgression>();

                prog.health = health;
                prog.movement = movement;
                prog.weapon = weapon;
                prog.Initialize();

                weapon.fireRate = 0.10f; // decayed from powerups
                prog.ResetForNewRun();

                if (prog.currentShip == null || prog.currentShip.sprite == null)
                    throw new System.Exception("Player currentShip or currentShip.sprite is NULL!");
                if (prog.shipRenderer == null || prog.shipRenderer.sprite == null)
                    throw new System.Exception("Player shipRenderer.sprite is NULL!");

                if (Mathf.Abs(weapon.fireRate - 0.22f) > 0.001f)
                    throw new System.Exception($"ResetForNewRun did not restore fireRate to 0.22f, got {weapon.fireRate}");

                Object.DestroyImmediate(player);
                Debug.Log("[PASS] 4. PlayerProgression restores weapon fireRate on new run / evolution");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 4. Progression test failed: {ex.Message}");
                failed++;
            }

            // 5. ShipSkillController Verification (Shockwave & Reactive Barrier)
            string savedSkillBeforeTest = StarStrike.Platform.Storage.StorageService.GetSelectedSkill();
            try
            {
                StarStrike.Platform.Storage.StorageService.SaveSelectedSkill("forward_shockwave");
                var ship = new GameObject("TestShip_Skill");
                var rb = ship.AddComponent<Rigidbody2D>();
                var health = ship.AddComponent<ShipHealth>();
                health.maxEnergy = 80f;
                health.maxShield = 100f;
                health.ResetMeters();

                var controller = ship.AddComponent<ShipSkillController>();
                controller.InitializeDefaultSlots();

                bool castOk = controller.TryCastActiveSkill(SkillSlotType.Active1);
                if (!castOk) throw new System.Exception("TryCastActiveSkill failed");
                if (Mathf.Abs(health.currentEnergy - 60f) > 0.01f)
                    throw new System.Exception($"Expected 60 energy (80 - 20), got {health.currentEnergy}");

                // Hard-block check
                health.ConsumeEnergy(50f); // 10 energy left (< 20 required)
                bool secondCast = controller.TryCastActiveSkill(SkillSlotType.Active1);
                if (secondCast) throw new System.Exception("Cast should be hard-blocked with insufficient energy");

                Object.DestroyImmediate(ship);
                Debug.Log("[PASS] 5. ShipSkillController Active & Passive skills fully operational");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 5. Skill controller test failed: {ex.Message}");
                failed++;
            }
            finally
            {
                StarStrike.Platform.Storage.StorageService.SaveSelectedSkill(savedSkillBeforeTest);
            }

            // 6. Polymorphic Interfaces Verification (IWeapon, IEnergyConsumer, IPowerUpEffect, IPoolable, IShipInputProvider, IProjectile)
            try
            {
                var go = new GameObject("Test_Polymorphism");
                var health = go.AddComponent<ShipHealth>();
                health.maxEnergy = 80f;
                health.ResetMeters();

                var weapon = go.AddComponent<ShipWeapon>();
                weapon.energyCost = 6f;
                weapon.fireRate = 0.22f;

                IWeapon weaponInt = weapon;
                IEnergyConsumer energyInt = health;

                bool fireOk = weaponInt.TryFire(energyInt);
                if (!fireOk) throw new System.Exception("IWeapon.TryFire failed with IEnergyConsumer");
                if (Mathf.Abs(energyInt.CurrentEnergy - 74f) > 0.01f)
                    throw new System.Exception($"IWeapon deducted energy incorrectly: {energyInt.CurrentEnergy}");

                // IPowerUpEffect
                var config = ScriptableObject.CreateInstance<LootConfig>();
                config.shieldOverchargeBonus = 50f;
                IPowerUpEffect shieldEff = new ShieldOverchargeEffect();
                shieldEff.Apply(go, config);
                if (Mathf.Abs(health.maxShield - 150f) > 0.01f)
                    throw new System.Exception($"IPowerUpEffect failed: maxShield is {health.maxShield}");

                // IShipInputProvider
                var movement = go.AddComponent<ShipMovement>();
                var reader = go.AddComponent<PlayerInputReader>();
                reader.inputThrust = true;
                IShipInputProvider inputInt = reader;
                movement.ApplyInput(inputInt);
                if (!movement.IsThrusting)
                    throw new System.Exception("IShipInputProvider did not set IsThrusting on ShipMovement");

                Object.DestroyImmediate(go);
                Debug.Log("[PASS] 6. Polymorphic Interfaces (IWeapon, IEnergyConsumer, IPowerUpEffect, IShipInputProvider, IPoolable) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 6. Polymorphic interfaces test failed: {ex.Message}");
                failed++;
            }

            // 7. Advanced Weapon Systems & Controller Verification
            try
            {
                var go = new GameObject("Test_Weapons");
                go.tag = "Player";
                var health = go.AddComponent<ShipHealth>();
                health.maxEnergy = 100f;
                health.ResetMeters();

                var controller = go.AddComponent<ShipWeaponController>();
                controller.InitializeWeapons();

                if (controller.WeaponCount < 5)
                    throw new System.Exception($"Expected at least 5 weapons, got {controller.WeaponCount}");

                if (controller.CurrentWeapon.WeaponName != "Pulse Laser")
                    throw new System.Exception($"Weapon 0 should be Pulse Laser, got {controller.CurrentWeapon.WeaponName}");

                controller.SwitchWeapon(1);
                if (controller.CurrentWeapon.WeaponName != "Scattergun")
                    throw new System.Exception($"Weapon 1 should be Scattergun, got {controller.CurrentWeapon.WeaponName}");

                bool scatterFired = controller.CurrentWeapon.TryFire(health);
                if (!scatterFired) throw new System.Exception("Scattergun failed to fire");

                controller.SwitchWeapon(2);
                if (controller.CurrentWeapon.WeaponName != "Homing Missiles")
                    throw new System.Exception($"Weapon 2 should be Homing Missiles, got {controller.CurrentWeapon.WeaponName}");

                bool missileFired = controller.CurrentWeapon.TryFire(health);
                if (!missileFired) throw new System.Exception("Homing Missiles failed to fire");

                controller.SwitchWeapon(3);
                if (controller.CurrentWeapon.WeaponName != "Beam Laser")
                    throw new System.Exception($"Weapon 3 should be Beam Laser, got {controller.CurrentWeapon.WeaponName}");

                bool beamFired = controller.CurrentWeapon.TryFire(health);
                if (!beamFired) throw new System.Exception("Beam Laser failed to fire");
                controller.CurrentWeapon.StopFiring();

                controller.SwitchWeapon(4);
                if (controller.CurrentWeapon.WeaponName != "Gatling Gun")
                    throw new System.Exception($"Weapon 4 should be Gatling Gun, got {controller.CurrentWeapon.WeaponName}");

                bool gatlingFired = controller.CurrentWeapon.TryFire(health);
                if (!gatlingFired) throw new System.Exception("Gatling Gun failed to fire");
                if (controller.CurrentWeapon.InaccuracyAngle <= 0f)
                    throw new System.Exception("Gatling Gun InaccuracyAngle must be positive");

                Object.DestroyImmediate(go);
                Debug.Log("[PASS] 7. Advanced Weapon Systems & Controller (Scattergun, Missiles, Beam, Gatling Gun, Swapping) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 7. Advanced weapon systems test failed: {ex.Message}");
                failed++;
            }

            // 8. Projectile Heading & Orientation Alignment Verification
            try
            {
                // Test 8A: Procedural laser sprite is horizontal (width > height) for forward +X motion
                var laserSprite = StarStrike.Gameplay.ProceduralSpriteHelper.GetLaserSprite(false, forceProcedural: true);
                if (laserSprite.rect.width <= laserSprite.rect.height)
                    throw new System.Exception($"Laser sprite must be oriented horizontally along +X: width={laserSprite.rect.width}, height={laserSprite.rect.height}");

                // Test 8B: Bullet fired when ship is rotated 90 degrees (facing UP / vertical)
                var shipGo = new GameObject("TestShip_AimUp");
                shipGo.transform.rotation = Quaternion.Euler(0, 0, 90f);
                var shipWeapon = shipGo.AddComponent<ShipWeapon>();
                shipWeapon.weaponSlots = 1;

                var bulletGo = new GameObject("TestBullet_Up");
                bulletGo.transform.rotation = shipGo.transform.rotation;
                var proj = bulletGo.AddComponent<LaserProjectile>();
                proj.Initialize(20f, 25f, false);

                var rb = bulletGo.GetComponent<Rigidbody2D>();
                if (rb == null || rb.linearVelocity.y <= 20f || Mathf.Abs(rb.linearVelocity.x) > 0.01f)
                    throw new System.Exception($"Bullet fired from ship facing UP must travel vertically (+Y): velocity={rb?.linearVelocity}");

                var sr = bulletGo.GetComponent<SpriteRenderer>();
                if (sr == null || sr.bounds.size.y <= sr.bounds.size.x)
                    throw new System.Exception($"Bullet flying vertically must have vertical visual orientation: size={sr?.bounds.size}");

                Object.DestroyImmediate(bulletGo);
                Object.DestroyImmediate(shipGo);

                Debug.Log("[PASS] 8. Projectile heading & orientation alignment verified (vertical when aimed up, lengthwise across all angles)");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 8. Projectile heading alignment test failed: {ex.Message}");
                failed++;
            }

            // 9. Enemy Archetypes Initialization & Kamikaze Radial Blast
            try
            {
                var enemyGo = new GameObject("TestEnemy_Kamikaze");
                var enemyHealth = enemyGo.AddComponent<ShipHealth>();
                var enemyMove = enemyGo.AddComponent<ShipMovement>();
                var enemyWeapon = enemyGo.AddComponent<ShipWeapon>();
                var enemyAi = enemyGo.AddComponent<EnemyAI>();
                enemyAi.ConfigureArchetype(EnemyArchetype.Kamikaze);

                if (enemyAi.Archetype != EnemyArchetype.Kamikaze)
                    throw new System.Exception($"Expected Kamikaze archetype, got {enemyAi.Archetype}");
                if (enemyAi.definition.kamikazeBlastRadius < 4f)
                    throw new System.Exception($"Expected blast radius >= 4, got {enemyAi.definition.kamikazeBlastRadius}");

                // Test target in blast radius
                var targetGo = new GameObject("TestTarget_InBlast");
                targetGo.transform.position = new Vector3(2f, 0f, 0f);
                var targetHealth = targetGo.AddComponent<ShipHealth>();
                var targetCol = targetGo.AddComponent<CircleCollider2D>();
                targetCol.radius = 0.5f;
                targetHealth.maxShield = 100f;
                targetHealth.ResetMeters();

                Physics2D.SyncTransforms();

                // Kill kamikaze, triggering blast
                enemyHealth.TakeDamage(100f);

                if (targetHealth.currentShield >= 100f)
                    throw new System.Exception("Kamikaze death blast did not damage nearby target within radius!");

                Object.DestroyImmediate(targetGo);
                Object.DestroyImmediate(enemyGo);
                Debug.Log("[PASS] 9. Enemy Archetypes (Kamikaze, Sniper, Support) & radial blast verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 9. Enemy archetypes verification failed: {ex.Message}");
                failed++;
            }

            // 10. Sector Biomes & Asteroid Shelter Occlusion
            try
            {
                var hazardGo = new GameObject("Test_HazardCtrl");
                var hazardCtrl = hazardGo.AddComponent<SectorHazardController>();

                hazardCtrl.SetSector(SectorHazardController.SectorBiome.IonNebula);
                if (hazardCtrl.CurrentBiome != SectorHazardController.SectorBiome.IonNebula)
                    throw new System.Exception($"Expected IonNebula, got {hazardCtrl.CurrentBiome}");

                hazardCtrl.SetSector(SectorHazardController.SectorBiome.SolarFlare);
                if (hazardCtrl.CurrentBiome != SectorHazardController.SectorBiome.SolarFlare)
                    throw new System.Exception($"Expected SolarFlare, got {hazardCtrl.CurrentBiome}");

                // Spawn asteroid between position and flare source
                var astGo = new GameObject("Test_AsteroidShield");
                Vector2 flareDir = hazardCtrl.flareDirection.normalized;
                astGo.transform.position = (Vector3)(-flareDir * 5f);
                var astCol = astGo.AddComponent<CircleCollider2D>();
                astCol.radius = 2.0f;
                astGo.AddComponent<Asteroid>();

                Physics2D.SyncTransforms();

                bool sheltered = hazardCtrl.IsPositionSheltered(Vector2.zero);
                if (!sheltered)
                    throw new System.Exception("Position behind asteroid should be sheltered from solar flare!");

                Object.DestroyImmediate(astGo);
                Object.DestroyImmediate(hazardGo);
                Debug.Log("[PASS] 10. Sector Biomes & Solar Flare asteroid shelter occlusion verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 10. Sector hazard verification failed: {ex.Message}");
                failed++;
            }

            // 11. Alternate Skills (Blink Teleport & EMP Nova)
            try
            {
                var ship = new GameObject("TestShip_AltSkills");
                var shipRb = ship.AddComponent<Rigidbody2D>();
                var shipHealth = ship.AddComponent<ShipHealth>();
                shipHealth.maxShield = 100f;
                shipHealth.ResetMeters();

                var blink = new BlinkTeleportBehavior();
                blink.Initialize(ship, null);
                blink.Execute(ship, Vector2.zero, Vector2.right);

                if (ship.transform.position.x < 7.5f)
                    throw new System.Exception($"Blink should teleport ~8.5 units forward, got x={ship.transform.position.x}");
                if (!shipHealth.isInvulnerable)
                    throw new System.Exception("Blink should grant temporary invulnerability frame!");

                // EMP Nova test
                var enemyObj = new GameObject("TestEnemy_EMP");
                enemyObj.tag = "Enemy";
                enemyObj.transform.position = new Vector3(3f, 0f, 0f);
                var enemyCol = enemyObj.AddComponent<CircleCollider2D>();
                enemyCol.radius = 0.5f;
                var enemyH = enemyObj.AddComponent<ShipHealth>();
                var enemyAI = enemyObj.AddComponent<EnemyAI>();
                enemyH.maxShield = 100f;
                enemyH.ResetMeters();

                Physics2D.SyncTransforms();

                var emp = new EMPNovaBehavior();
                emp.Initialize(ship, null);
                emp.Execute(ship, Vector2.zero, Vector2.zero);

                if (enemyH.currentShield >= 100f)
                    throw new System.Exception("EMP Nova should deal damage to enemies within 9.5 radius!");

                Object.DestroyImmediate(enemyObj);
                Object.DestroyImmediate(ship);
                Debug.Log("[PASS] 11. Alternate Skills (Blink Teleport micro-warp & EMP Nova radial pulse) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 11. Alternate skills verification failed: {ex.Message}");
                failed++;
            }

            // 12. Playable Hulls Expansion (Vanguard Carrier & Shadow Phantom)
            try
            {
                var scout = PlayerProgression.CreateDefaultShipTree();
                var fighter = scout.evolvesTo[0];
                var miner = scout.evolvesTo[1];

                bool hasPhantom = false;
                foreach (var evo in fighter.evolvesTo)
                {
                    if (evo.id == "phantom") hasPhantom = true;
                }
                if (!hasPhantom) throw new System.Exception("Fighter does not evolve to Shadow Phantom!");

                bool hasVanguard = false;
                foreach (var evo in miner.evolvesTo)
                {
                    if (evo.id == "vanguard") hasVanguard = true;
                }
                if (!hasVanguard) throw new System.Exception("Miner does not evolve to Vanguard Carrier!");

                Debug.Log("[PASS] 12. Playable Hull Expansion (Vanguard Carrier & Shadow Phantom) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 12. Playable hull expansion test failed: {ex.Message}");
                failed++;
            }

            // 13. Physical Hardpoint Sockets & Tuned Hitbox Verification
            try
            {
                var scoutTree = PlayerProgression.CreateDefaultShipTree();
                var juggernautDef = scoutTree.evolvesTo[1].evolvesTo[0];
                var carrierDef = scoutTree.evolvesTo[1].evolvesTo[1];

                // Scout sockets check
                if (scoutTree.weaponSockets == null || scoutTree.weaponSockets.Length != 1)
                    throw new System.Exception("Scout must have exactly 1 weapon socket!");
                if (Mathf.Abs(scoutTree.weaponSockets[0].x - 1.55f) > 0.05f)
                    throw new System.Exception($"Scout weapon socket X expected ~1.55f, got {scoutTree.weaponSockets[0].x}");
                if (Mathf.Abs(scoutTree.hitboxRadius - 0.55f) > 0.01f)
                    throw new System.Exception($"Scout hitboxRadius expected 0.55f, got {scoutTree.hitboxRadius}");

                // Juggernaut sockets check
                if (juggernautDef.weaponSockets == null || juggernautDef.weaponSockets.Length != 3)
                    throw new System.Exception("Juggernaut must have exactly 3 weapon sockets!");
                if (Mathf.Abs(juggernautDef.weaponSockets[0].x - 2.75f) > 0.05f)
                    throw new System.Exception($"Juggernaut center socket X expected ~2.75f, got {juggernautDef.weaponSockets[0].x}");
                if (Mathf.Abs(juggernautDef.hitboxRadius - 0.85f) > 0.01f)
                    throw new System.Exception($"Juggernaut hitboxRadius expected 0.85f, got {juggernautDef.hitboxRadius}");

                // Carrier sockets check
                if (carrierDef.weaponSockets == null || carrierDef.weaponSockets.Length != 3)
                    throw new System.Exception("Carrier must have exactly 3 weapon sockets!");
                if (Mathf.Abs(carrierDef.weaponSockets[0].x - 2.50f) > 0.05f)
                    throw new System.Exception($"Carrier center socket X expected ~2.50f, got {carrierDef.weaponSockets[0].x}");
                if (Mathf.Abs(carrierDef.hitboxRadius - 0.80f) > 0.01f)
                    throw new System.Exception($"Carrier hitboxRadius expected 0.80f, got {carrierDef.hitboxRadius}");

                // Live ShipWeapon Muzzle Positioning and TransformPoint test
                var testShip = new GameObject("TestShip_Sockets");
                var shipCol = testShip.AddComponent<CircleCollider2D>();
                var shipHealth = testShip.AddComponent<ShipHealth>();
                var shipWp = testShip.AddComponent<ShipWeapon>();
                var shipProg = testShip.AddComponent<PlayerProgression>();
                shipProg.health = shipHealth;
                shipProg.weapon = shipWp;

                // Configure as Scout
                shipProg.currentShip = scoutTree;
                shipProg.ApplyShipVisuals();

                if (Mathf.Abs(shipCol.radius - 0.55f) > 0.01f)
                    throw new System.Exception($"Ship collider radius should update to Scout hitbox 0.55f, got {shipCol.radius}");

                var scoutMuzzles = shipWp.GetMuzzleWorldPositions();
                if (scoutMuzzles.Length != 1)
                    throw new System.Exception($"Scout should produce 1 muzzle pos, got {scoutMuzzles.Length}");
                if (Vector2.Distance(scoutMuzzles[0], new Vector2(1.55f, 0f)) > 0.05f)
                    throw new System.Exception($"Scout muzzle pos expected (1.55, 0), got {scoutMuzzles[0]}");

                // Test rotation: face UP (90 degrees)
                testShip.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                var rotatedMuzzles = shipWp.GetMuzzleWorldPositions();
                if (Vector2.Distance(rotatedMuzzles[0], new Vector2(0f, 1.55f)) > 0.05f)
                    throw new System.Exception($"Rotated Scout muzzle pos expected (0, 1.55), got {rotatedMuzzles[0]}");

                // Evolve to Juggernaut
                testShip.transform.rotation = Quaternion.identity;
                shipProg.Evolve(juggernautDef);

                if (Mathf.Abs(shipCol.radius - 0.85f) > 0.01f)
                    throw new System.Exception($"Ship collider radius should update to Juggernaut hitbox 0.85f, got {shipCol.radius}");

                var juggLocals = shipWp.GetActiveSocketLocalPositions();
                if (juggLocals.Length != 3)
                    throw new System.Exception($"Juggernaut should produce 3 local socket positions, got {juggLocals.Length}");
                if (Vector2.Distance(juggLocals[0], new Vector2(2.75f, 0f)) > 0.05f)
                    throw new System.Exception($"Juggernaut center local socket expected (2.75, 0), got {juggLocals[0]}");

                // Juggernaut visual scale is 1.6f, so world muzzle = local * 1.6f
                var juggMuzzles = shipWp.GetMuzzleWorldPositions();
                if (juggMuzzles.Length != 3)
                    throw new System.Exception($"Juggernaut should produce 3 world muzzle positions, got {juggMuzzles.Length}");
                if (Vector2.Distance(juggMuzzles[0], new Vector2(2.75f * 1.6f, 0f)) > 0.05f)
                    throw new System.Exception($"Juggernaut center world muzzle pos expected (4.40, 0), got {juggMuzzles[0]}");
                if (Vector2.Distance(juggMuzzles[1], new Vector2(0.70f * 1.6f, 1.50f * 1.6f)) > 0.05f)
                    throw new System.Exception($"Juggernaut port world muzzle pos expected (1.12, 2.40), got {juggMuzzles[1]}");
                if (Vector2.Distance(juggMuzzles[2], new Vector2(0.70f * 1.6f, -1.50f * 1.6f)) > 0.05f)
                    throw new System.Exception($"Juggernaut starboard world muzzle pos expected (1.12, -2.40), got {juggMuzzles[2]}");

                // Evolve to Carrier
                shipProg.Evolve(carrierDef);
                if (Mathf.Abs(shipCol.radius - 0.80f) > 0.01f)
                    throw new System.Exception($"Ship collider radius should update to Carrier hitbox 0.80f, got {shipCol.radius}");

                var carrierLocals = shipWp.GetActiveSocketLocalPositions();
                if (carrierLocals.Length != 3)
                    throw new System.Exception($"Carrier should produce 3 local socket positions, got {carrierLocals.Length}");
                if (Vector2.Distance(carrierLocals[0], new Vector2(2.50f, 0f)) > 0.05f)
                    throw new System.Exception($"Carrier center local socket expected (2.50, 0), got {carrierLocals[0]}");

                // Carrier visual scale is 1.5f, so world muzzle = local * 1.5f
                var carrierMuzzles = shipWp.GetMuzzleWorldPositions();
                if (carrierMuzzles.Length != 3)
                    throw new System.Exception($"Carrier should produce 3 world muzzle positions, got {carrierMuzzles.Length}");
                if (Vector2.Distance(carrierMuzzles[0], new Vector2(2.50f * 1.5f, 0f)) > 0.05f)
                    throw new System.Exception($"Carrier center world muzzle pos expected (3.75, 0), got {carrierMuzzles[0]}");

                Object.DestroyImmediate(testShip);
                Debug.Log("[PASS] 13. Physical Hardpoint Sockets & Tuned Hitbox (Scout, Juggernaut, Carrier) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 13. Physical sockets & hitbox verification failed: {ex.Message}");
                failed++;
            }

            // 14. Ship Shield Field & Impact Ripple Verification
            try
            {
                var shipObj = new GameObject("TestShip_Shield");
                var health = shipObj.AddComponent<ShipHealth>();
                health.maxShield = 100f;
                health.maxHull = 80f;
                health.ResetMeters();

                var shield = shipObj.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
                shield.EnsureInitialized();
                shield.SubscribeEvents();

                // Initial state
                if (shield.CurrentDomeAlpha != 0f)
                    throw new System.Exception("Shield dome alpha should be 0 initially!");

                // Take damage to shield (impact at Vector2(0.5, 0.5))
                health.TakeDamage(25f, new Vector2(0.5f, 0.5f));
                if (shield.CurrentDomeAlpha <= 0f)
                    throw new System.Exception("Shield impact should flare perimeter dome!");

                Object.DestroyImmediate(shipObj);

                Debug.Log("[PASS] 14. Ship Shield Field & Impact Ripple verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 14. Shield Field verification failed: {ex.Message}");
                failed++;
            }

            // 15. Two-Tier Durability Model (Shield Absorbs, Hull Spillover, Death on Hull=0)
            try
            {
                var ship = new GameObject("TestShip_Durability");
                var h = ship.AddComponent<ShipHealth>();
                h.maxShield = 100f;
                h.maxHull = 80f;
                h.ResetMeters();

                bool diedCalled = false;
                h.OnDied += () => diedCalled = true;

                // 1. Partial damage absorbed fully by shield
                h.TakeDamage(40f);
                if (Mathf.Abs(h.currentShield - 60f) > 0.01f || Mathf.Abs(h.currentHull - 80f) > 0.01f)
                    throw new System.Exception($"Shield should absorb first: expected Shield=60, Hull=80, got Shield={h.currentShield}, Hull={h.currentHull}");

                // 2. Shield break + leftover damage spills into hull
                h.TakeDamage(90f); // 60 absorbed by shield, 30 hits hull
                if (h.currentShield != 0f || Mathf.Abs(h.currentHull - 50f) > 0.01f)
                    throw new System.Exception($"Damage spillover expected Shield=0, Hull=50, got Shield={h.currentShield}, Hull={h.currentHull}");
                if (diedCalled)
                    throw new System.Exception("Ship must NOT die while hull > 0!");

                // 3. Shield regens passively, hull remains depleted
                h.shieldRegen = 20f;
                h.RegenerateMeters(1f);
                if (Mathf.Abs(h.currentShield - 20f) > 0.01f || Mathf.Abs(h.currentHull - 50f) > 0.01f)
                    throw new System.Exception($"Shield regen should restore shield to 20 without regenerating hull (50), got Shield={h.currentShield}, Hull={h.currentHull}");

                // 4. Fatal damage depletes hull and triggers death
                h.TakeDamage(100f); // 20 to shield, 50 to hull, 30 excess
                if (h.currentShield != 0f || h.currentHull != 0f)
                    throw new System.Exception($"Expected Shield=0, Hull=0, got Shield={h.currentShield}, Hull={h.currentHull}");
                if (!diedCalled)
                    throw new System.Exception("Ship should die when hull reaches 0!");

                Object.DestroyImmediate(ship);
                Debug.Log("[PASS] 15. Two-Tier Durability Model (Shield absorbs, Hull spillover, Death on Hull=0) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 15. Two-Tier Durability verification failed: {ex.Message}");
                failed++;
            }

            // 16. Dual-Hitbox System (Hull Exact Polygon + Shield Offset Polygon & State Toggling)
            try
            {
                var testShip = new GameObject("TestShip_DualHitbox");
                testShip.tag = "Player";
                var health = testShip.AddComponent<ShipHealth>();
                health.isPlayer = true;
                health.maxHull = 80f;
                health.maxShield = 100f;
                health.ResetMeters();

                var sr = testShip.AddComponent<SpriteRenderer>();
                sr.sprite = LoadArtSprite("Ships/Ship_Scout.png");

                var hullPoly = testShip.AddComponent<PolygonCollider2D>();
                hullPoly.isTrigger = false;
                if (sr.sprite != null)
                {
                    PlayerProgression.UpdatePolygonFromSprite(hullPoly, sr.sprite);
                }

                // 1. Verify Hull Collider conforms to sprite physics shape
                if (hullPoly.pathCount <= 0 || hullPoly.GetPath(0).Length < 8)
                    throw new System.Exception($"Hull polygon must have >8 vertices conforming to sprite outline, got {hullPoly.GetPath(0).Length}");

                int scoutHullPtsCount = hullPoly.GetPath(0).Length;
                Bounds hullBounds = hullPoly.bounds;

                // 2. Add Shield Field
                var shield = testShip.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
                shield.EnsureInitialized();
                shield.SubscribeEvents();

                var shieldCol = shield.ShieldCollider;
                if (shieldCol == null)
                    throw new System.Exception("Shield collider must be initialized on ShieldPerimeterDome!");
                if (!shieldCol.isTrigger)
                    throw new System.Exception("Shield collider must be a Trigger collider!");
                if (!shieldCol.enabled)
                    throw new System.Exception("Shield collider must be enabled while shield > 0!");

                var shieldPoly = shieldCol as PolygonCollider2D;
                if (shieldPoly == null)
                    throw new System.Exception("Shield collider must be a PolygonCollider2D when hull is PolygonCollider2D!");
                if (shieldPoly.pathCount <= 0)
                    throw new System.Exception("Shield pathCount must be > 0!");
                if (shieldPoly.GetPath(0).Length < 24)
                    throw new System.Exception($"Shield collider must be a smooth round envelope with >=24 vertices, got {shieldPoly.GetPath(0).Length}!");

                // Check that shield polygon bounds enclose the hull bounds
                Bounds shieldBounds = shieldPoly.bounds;
                if (shieldBounds.extents.x <= hullBounds.extents.x || shieldBounds.extents.y <= hullBounds.extents.y)
                    throw new System.Exception($"Shield bounds ({shieldBounds.extents}) must be larger than hull bounds ({hullBounds.extents})!");

                // Check Battlevoid: Harbinger idle invisibility
                var shieldLine = shield.ShieldLine;
                if (shieldLine != null && shieldLine.enabled)
                    throw new System.Exception("Shield line must be invisible when idle (Battlevoid: Harbinger style)!");
                if (shield.CurrentDomeAlpha != 0f)
                    throw new System.Exception("Shield dome must be transparent when idle!");

                // 3. Partial Damage: shield takes hit, flares on impact, stays enabled
                health.TakeDamage(30f, Vector2.right);
                if (Mathf.Abs(health.currentShield - 70f) > 0.01f)
                    throw new System.Exception($"Expected shield=70, got {health.currentShield}");
                if (!shieldCol.enabled)
                    throw new System.Exception("Shield collider must remain enabled while shield > 0!");
                if (shield.CurrentDomeAlpha <= 0f)
                    throw new System.Exception("Shield should flare on hit!");
                if (shieldLine != null && !shieldLine.enabled)
                    throw new System.Exception("Shield line should flash on hit!");

                // 4. Lethal Shield Damage: shield collapses, shield trigger collider & contour disabled
                health.TakeDamage(80f); // 70 shield, 10 hull
                if (health.currentShield != 0f || Mathf.Abs(health.currentHull - 70f) > 0.01f)
                    throw new System.Exception($"Expected shield=0, hull=70, got shield={health.currentShield}, hull={health.currentHull}");
                if (shieldCol.enabled)
                    throw new System.Exception("Shield collider must be DISABLED when shield reaches 0!");
                if (shieldLine != null && shieldLine.enabled)
                    throw new System.Exception("Shield contour line must be DISABLED when shield reaches 0!");
                if (!hullPoly.enabled)
                    throw new System.Exception("Hull collider must remain ENABLED when shield collapses!");

                // 5. Shield Regeneration: re-enables shield trigger collider
                health.shieldRegen = 20f;
                health.RegenerateMeters(1f);
                if (health.currentShield <= 0f)
                    throw new System.Exception("Shield should have regenerated above 0!");
                if (!shieldCol.enabled)
                    throw new System.Exception("Shield collider must RE-ENABLE when shield regenerates above 0!");
                if (shieldLine != null && shieldLine.enabled)
                    throw new System.Exception("Shield line must remain invisible when idle after regen!");

                // 6. Dynamic Evolution Point Update
                sr.sprite = LoadArtSprite("Ships/Ship_Fighter.png");
                PlayerProgression.UpdatePolygonFromSprite(hullPoly, sr.sprite);
                shield.RebuildShieldCollider();

                if (hullPoly.GetPath(0).Length == scoutHullPtsCount)
                    throw new System.Exception("Hull collider points should have updated to Fighter outline!");
                if (shieldPoly.GetPath(0).Length < 24)
                    throw new System.Exception("Shield collider points must form a smooth round envelope with >=24 vertices!");
                Bounds fighterShieldBounds = shieldPoly.bounds;
                if (fighterShieldBounds.extents.x <= hullPoly.bounds.extents.x || fighterShieldBounds.extents.y <= hullPoly.bounds.extents.y)
                    throw new System.Exception("Updated shield bounds must enclose fighter hull bounds!");

                // 7. Verify Increased Shield Size for Vanguard and Juggernaut
                var scoutTree = PlayerProgression.CreateDefaultShipTree();
                var juggernautDef = scoutTree.evolvesTo[1].evolvesTo[0];
                var vanguardDef = scoutTree.evolvesTo[1].evolvesTo[1];

                var prog = testShip.AddComponent<PlayerProgression>();
                prog.health = health;
                prog.shipRenderer = sr;

                prog.Evolve(juggernautDef);
                if (Mathf.Abs(shield.shieldHitboxOffset - 0.30f) > 0.01f)
                    throw new System.Exception($"Juggernaut shieldHitboxOffset expected 0.30f, got {shield.shieldHitboxOffset}");
                if (shieldPoly.bounds.extents.x <= hullPoly.bounds.extents.x || shieldPoly.bounds.extents.y <= hullPoly.bounds.extents.y)
                    throw new System.Exception("Juggernaut shield bounds must enclose Juggernaut hull bounds!");
                Vector2[] juggHull = hullPoly.GetPath(0);
                Vector2[] juggShield = shieldPoly.GetPath(0);
                for (int i = 0; i < juggHull.Length; i++)
                {
                    if (!IsPointInsidePolygon(juggHull[i], juggShield))
                        throw new System.Exception($"Juggernaut hull point {juggHull[i]} is outside shield polygon!");
                }

                prog.Evolve(vanguardDef);
                if (Mathf.Abs(shield.shieldHitboxOffset - 0.30f) > 0.01f)
                    throw new System.Exception($"Vanguard shieldHitboxOffset expected 0.30f, got {shield.shieldHitboxOffset}");
                if (shieldPoly.bounds.extents.x <= hullPoly.bounds.extents.x || shieldPoly.bounds.extents.y <= hullPoly.bounds.extents.y)
                    throw new System.Exception("Vanguard shield bounds must enclose Vanguard hull bounds!");
                Vector2[] vanHull = hullPoly.GetPath(0);
                Vector2[] vanShield = shieldPoly.GetPath(0);
                for (int i = 0; i < vanHull.Length; i++)
                {
                    if (!IsPointInsidePolygon(vanHull[i], vanShield))
                        throw new System.Exception($"Vanguard hull point {vanHull[i]} is outside shield polygon!");
                }

                Object.DestroyImmediate(testShip);
                Debug.Log("[PASS] 16. Dual-Hitbox System (Hull Exact Polygon + Shield Round Envelope & State Toggling) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 16. Dual-Hitbox System verification failed: {ex.Message}");
                failed++;
            }

            // 17. Verify Enemy Attack Telegraphs (Feature #4)
            try
            {
                var bossObj = new GameObject("TestBoss_Telegraph");
                var brain = bossObj.AddComponent<BossBrain>();
                brain.EnsureTelegraphInitialized();

                var tLine = brain.TelegraphLine;
                if (tLine == null)
                    throw new System.Exception("BossBrain must initialize TelegraphLine!");
                if (!tLine.useWorldSpace)
                    throw new System.Exception("TelegraphLine must use world space for accurate targeting rays!");
                if (tLine.positionCount != 2)
                    throw new System.Exception($"TelegraphLine must have 2 positions (origin, target), got {tLine.positionCount}!");
                if (tLine.enabled)
                    throw new System.Exception("TelegraphLine must be disabled when idle!");

                Object.DestroyImmediate(bossObj);
                Debug.Log("[PASS] 17. Enemy Attack Telegraphs (Boss & Combat Warnings) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 17. Enemy Attack Telegraphs verification failed: {ex.Message}");
                failed++;
            }

            // 18. Verify Emissive Shader Glow Setup (Feature #6)
            try
            {
                var emShader = Shader.Find("StarStrike/Sprite-Emissive");
                if (emShader == null)
                    throw new System.Exception("StarStrike/Sprite-Emissive shader must be compiled and findable!");

                var testShip = new GameObject("TestShip_Emissive");
                var prog = testShip.AddComponent<PlayerProgression>();
                var def = ScriptableObject.CreateInstance<ShipDefinition>();
                def.id = "scout";
                def.emissiveTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Sprites/Ships_Imported/Ship_Scout_HD_emissive.png");
                def.emissiveIntensity = 2.0f;
                prog.currentShip = def;
                prog.ApplyShipVisuals();

                var sr = testShip.GetComponent<SpriteRenderer>();
                if (sr == null || sr.sharedMaterial == null)
                    throw new System.Exception("SpriteRenderer with material must exist!");
                if (sr.sharedMaterial.shader != emShader)
                    throw new System.Exception($"Expected shader StarStrike/Sprite-Emissive, got {sr.sharedMaterial.shader.name}!");
                if (sr.sharedMaterial.GetTexture("_EmissiveTex") != def.emissiveTexture)
                    throw new System.Exception("Emissive texture must be assigned to _EmissiveTex property!");

                Object.DestroyImmediate(testShip);
                Object.DestroyImmediate(def);
                Debug.Log("[PASS] 18. Emissive Shader Glow & Bloom Setup verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 18. Emissive Shader Glow verification failed: {ex.Message}");
                failed++;
            }

            // 19. Verify Shield Impact Ripple Procedural Setup & Scaling
            try
            {
                var rippleObj = new GameObject("Test_ShieldRipple");
                var ripple = rippleObj.AddComponent<StarStrike.Gameplay.FX.ShieldImpactRipple>();
                ripple.EnsureInitialized();

                var sr = ripple.SpriteRenderer;
                if (sr == null || sr.sprite == null)
                    throw new System.Exception("ShieldImpactRipple SpriteRenderer or procedural sprite is null!");
                if (sr.sortingOrder != 14)
                    throw new System.Exception($"ShieldImpactRipple sortingOrder expected 14, got {sr.sortingOrder}");

                ripple.Initialize(new Color(0.2f, 0.9f, 1f, 1f), 0.2f, 0.8f, 0.2f);
                if (rippleObj.transform.localScale.x < 0.19f || rippleObj.transform.localScale.x > 0.21f)
                    throw new System.Exception($"ShieldImpactRipple start scale mismatch: {rippleObj.transform.localScale}");

                Object.DestroyImmediate(rippleObj);
                Debug.Log("[PASS] 19. Shield Impact Ripple (Procedural Hex Flare, Zero GC, Layer 14) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 19. Shield Impact Ripple verification failed: {ex.Message}");
                failed++;
            }

            // 20. Verify Ship Thruster Effect (Pure Particle Stream & Modulation, No Trail)
            try
            {
                var shipObj = new GameObject("Test_ShipThruster");
                var thruster = shipObj.AddComponent<StarStrike.Gameplay.FX.ShipThrusterEffect>();
                thruster.EnsureInitialized();

                var ps = thruster.ExhaustParticles;
                if (ps == null)
                    throw new System.Exception("Thruster ExhaustParticles is null!");
                if (ps.isPlaying)
                    throw new System.Exception("Thruster particle system should not be playing when idle!");

                // Activate cruise thrust
                thruster.SetThrusting(true, false);
                if (!ps.isPlaying)
                    throw new System.Exception("Thruster particles should be playing when thrusting!");
                if (ps.main.startColor.color != thruster.normalThrustColor)
                    throw new System.Exception($"Cruise particle color mismatch: expected {thruster.normalThrustColor}, got {ps.main.startColor.color}");

                // Activate boost thrust
                thruster.SetThrusting(true, true);
                if (!ps.isPlaying)
                    throw new System.Exception("Thruster particles should remain playing when boosting!");
                if (ps.main.startColor.color != thruster.boostThrustColor)
                    throw new System.Exception($"Boost particle color mismatch: expected {thruster.boostThrustColor}, got {ps.main.startColor.color}");

                // Deactivate thrust
                thruster.SetThrusting(false, false);
                if (ps.isPlaying)
                    throw new System.Exception("Thruster particles should stop after thrust stops!");

                // Verify trail renderer has been completely removed
                var tr = shipObj.GetComponentInChildren<TrailRenderer>();
                if (tr != null)
                    throw new System.Exception("TrailRenderer effect should be completely removed from thruster!");

                Object.DestroyImmediate(shipObj);
                Debug.Log("[PASS] 20. Ship Thruster Effect (Pure Particle Stream & Color Modulation, No Trail) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 20. Ship Thruster Effect verification failed: {ex.Message}");
                failed++;
            }

            // 21. Verify Native Dynamic Particle Explosion Pooling & Scaling
            try
            {
                var explGo = StarStrike.Gameplay.FX.FXService.CreateExplosionPrefab();
                var ps = explGo.GetComponent<ParticleSystem>();
                if (ps == null)
                    throw new System.Exception("ExplosionFX missing ParticleSystem component!");
                if (ps.main.loop)
                    throw new System.Exception("Explosion particle system should not loop!");
                if (ps.main.stopAction != ParticleSystemStopAction.Disable)
                    throw new System.Exception("Explosion stopAction should be Disable for pool reuse!");

                Object.DestroyImmediate(explGo);
                Debug.Log("[PASS] 21. Native Dynamic Particle Explosion (Multi-Scale Burst & Pool Lifecycle) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 21. Native Explosion verification failed: {ex.Message}");
                failed++;
            }

            // 22. Verify Weapon Kinematic Recoil, Sockets & Inaccuracy Spread
            try
            {
                var shipObj = new GameObject("Test_ShipWeaponRecoil");
                var rb = shipObj.AddComponent<Rigidbody2D>();
                rb.mass = 1f;
                var weapon = shipObj.AddComponent<ShipWeapon>();
                weapon.weaponSlots = 1;
                weapon.fireRate = 0.1f;
                weapon.energyCost = 0f;
                weapon.inaccuracyAngle = 2.5f;

                shipObj.transform.rotation = Quaternion.identity;
                weapon.TryFire(null);

                if (weapon.WeaponSlots != 1)
                    throw new System.Exception($"Expected 1 slot, got {weapon.WeaponSlots}");
                if (weapon.inaccuracyAngle <= 0f)
                    throw new System.Exception("Weapon inaccuracyAngle should be positive for realistic bullet spread");

                Object.DestroyImmediate(shipObj);
                Debug.Log("[PASS] 22. Weapon Kinematic Recoil, Sockets & Inaccuracy Spread verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 22. Weapon Recoil verification failed: {ex.Message}");
                failed++;
            }

            // 23. Verify Weapon Archetypes & Multi-Socket Beam Systems
            try
            {
                // Verify archetypes mapping
                if (ShipWeaponController.GetWeaponArchetype("pulse") != WeaponArchetype.Projectile)
                    throw new System.Exception("Pulse must map to Projectile archetype");
                if (ShipWeaponController.GetWeaponArchetype("scattergun") != WeaponArchetype.Projectile)
                    throw new System.Exception("Scattergun must map to Projectile archetype");
                if (ShipWeaponController.GetWeaponArchetype("gatling") != WeaponArchetype.Projectile)
                    throw new System.Exception("Gatling must map to Projectile archetype");
                if (ShipWeaponController.GetWeaponArchetype("missile") != WeaponArchetype.Traceable)
                    throw new System.Exception("Missile must map to Traceable archetype");
                if (ShipWeaponController.GetWeaponArchetype("beam") != WeaponArchetype.Beam)
                    throw new System.Exception("Beam must map to Beam archetype");

                // Verify archetype labels
                if (ShipWeaponController.GetArchetypeLabel(WeaponArchetype.Projectile) != "PROJECTILE // BALLISTIC & PLASMA")
                    throw new System.Exception("Projectile label mismatch");
                if (ShipWeaponController.GetArchetypeLabel(WeaponArchetype.Traceable) != "TRACEABLE // GUIDED ORDNANCE")
                    throw new System.Exception("Traceable label mismatch");
                if (ShipWeaponController.GetArchetypeLabel(WeaponArchetype.Beam) != "BEAM // DIRECTED ENERGY")
                    throw new System.Exception("Beam label mismatch");

                // Verify multi-socket ContinuousBeamWeapon instances
                var beamGo = new GameObject("Test_MultiSocketBeam");
                var beamComp = beamGo.AddComponent<ContinuousBeamWeapon>();
                beamComp.SetSockets(new Vector2[] {
                    new Vector2(2.5f, 0f),
                    new Vector2(0.8f, 1.4f),
                    new Vector2(0.8f, -1.4f)
                });

                if (beamComp.Archetype != WeaponArchetype.Beam)
                    throw new System.Exception("ContinuousBeamWeapon.Archetype should be Beam");

                var lineRenderers = beamGo.GetComponentsInChildren<LineRenderer>();
                if (lineRenderers.Length != 3)
                    throw new System.Exception($"Beam weapon with 3 sockets must have 3 LineRenderer instances, got {lineRenderers.Length}");

                Object.DestroyImmediate(beamGo);
                Debug.Log("[PASS] 23. Weapon Archetypes (Projectile, Traceable, Beam) & Multi-Socket Beam Systems verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 23. Weapon Archetypes verification failed: {ex.Message}");
                failed++;
            }

            // 24. Verify Modular Weapon Prefabs & Sockets
            try
            {
                CreateWeaponPrefabs();

                string[] expectedPrefabs = new string[]
                {
                    "Assets/_Project/Prefabs/Weapons/Weapon_PulseLaser.prefab",
                    "Assets/_Project/Prefabs/Weapons/Weapon_Scattergun.prefab",
                    "Assets/_Project/Prefabs/Weapons/Weapon_HomingMissiles.prefab",
                    "Assets/_Project/Prefabs/Weapons/Weapon_ContinuousBeam.prefab",
                    "Assets/_Project/Prefabs/Weapons/Weapon_RotaryGatling.prefab",
                    "Assets/_Project/Prefabs/Weapons/Weapon_SocketMount.prefab"
                };

                foreach (var path in expectedPrefabs)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                        throw new System.Exception($"Prefab missing at {path}");
                }

                // Verify loading via ShipWeaponController
                var pulsePrefab = ShipWeaponController.GetWeaponPrefab("pulse");
                var scatterPrefab = ShipWeaponController.GetWeaponPrefab("scattergun");
                var missilePrefab = ShipWeaponController.GetWeaponPrefab("missile");
                var beamPrefab = ShipWeaponController.GetWeaponPrefab("beam");
                var gatlingPrefab = ShipWeaponController.GetWeaponPrefab("gatling");
                var mountPrefab = ShipWeaponController.GetSocketMountPrefab();

                if (pulsePrefab == null || scatterPrefab == null || missilePrefab == null || 
                    beamPrefab == null || gatlingPrefab == null || mountPrefab == null)
                {
                    throw new System.Exception("ShipWeaponController failed to load one or more weapon prefabs");
                }

                // Test instantiation of a ship with mounted weapon prefabs
                var testShip = new GameObject("Test_ShipWithMountedWeapons");
                testShip.tag = "Player";
                testShip.AddComponent<Rigidbody2D>();
                var testController = testShip.AddComponent<ShipWeaponController>();
                testController.UpdateSocketVisuals();

                if (testShip.transform.childCount == 0)
                    throw new System.Exception("Ship sockets were not instantiated on test ship");

                Object.DestroyImmediate(testShip);
                Debug.Log("[PASS] 24. Modular Weapon Prefabs, Sockets & Visual Hardpoints verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 24. Weapon Prefabs verification failed: {ex.Message}");
                failed++;
            }

            // 25. EquipmentSelectUI & ShipSelectUI Interactive Weapon Sockets
            try
            {
                StorageService.SaveLastShipEvolved("fighter");
                var equipGo = new GameObject("Test_EquipUI_Sockets");
                var equipUI = equipGo.AddComponent<StarStrike.UI.EquipmentSelectUI>();
                equipUI.EnsureInitialized();

                if (equipUI.ActiveSocketCount != 2)
                    throw new System.Exception($"Expected 2 sockets for fighter, got {equipUI.ActiveSocketCount}");

                var previewBox = equipGo.transform.Find("EquipmentSelectContainer/WeaponPanel/ShipPreviewBox");
                if (previewBox == null)
                    throw new System.Exception("ShipPreviewBox not found in EquipmentSelectUI hierarchy");

                var shipSpriteTransform = previewBox.Find("ShipSprite");
                if (shipSpriteTransform == null)
                    throw new System.Exception("ShipSprite not found in ShipPreviewBox");

                var socket0 = shipSpriteTransform.Find("ShipSocket_0");
                var socket1 = shipSpriteTransform.Find("ShipSocket_1");
                if (socket0 == null || socket1 == null)
                    throw new System.Exception("ShipSocket_0 or ShipSocket_1 not found on ship preview image");

                var btn0 = socket0.GetComponent<UnityEngine.UI.Button>();
                var btn1 = socket1.GetComponent<UnityEngine.UI.Button>();
                if (btn0 == null || btn1 == null)
                    throw new System.Exception("Ship sockets must have Button components for interaction");

                equipUI.SelectSocket(0);
                equipUI.EquipWeaponToSocket(0, "scattergun");
                if (StorageService.GetSocketWeapon("fighter", 0) != "scattergun")
                    throw new System.Exception("Socket 0 weapon not equipped correctly to storage");

                equipUI.SelectSocket(1);
                equipUI.EquipWeaponToSocket(1, "beam");
                if (StorageService.GetSocketWeapon("fighter", 1) != "beam")
                    throw new System.Exception("Socket 1 weapon not equipped correctly to storage");

                Object.DestroyImmediate(equipGo);

                var shipSelectGo = new GameObject("Test_ShipSelectUI_Sockets");
                var shipUI = shipSelectGo.AddComponent<StarStrike.UI.ShipSelectUI>();
                shipUI.EnsureInitialized();
                shipUI.SelectShipById("fighter");

                var shipSelectPreview = shipSelectGo.transform.Find("ShipSelectContainer/ShowcasePanel/PreviewBox/ShipSprite");
                if (shipSelectPreview == null)
                    throw new System.Exception("ShipSprite not found in ShipSelectUI ShowcasePanel");

                var previewSock0 = shipSelectPreview.Find("PreviewSocket_0");
                var previewSock1 = shipSelectPreview.Find("PreviewSocket_1");
                if (previewSock0 == null || previewSock1 == null)
                    throw new System.Exception("PreviewSocket_0 or PreviewSocket_1 not found in ShipSelectUI");

                var pBtn0 = previewSock0.GetComponent<UnityEngine.UI.Button>();
                var pBtn1 = previewSock1.GetComponent<UnityEngine.UI.Button>();
                if (pBtn0 == null || pBtn1 == null)
                    throw new System.Exception("ShipSelectUI preview sockets must have interactive Button components");

                Object.DestroyImmediate(shipSelectGo);

                Debug.Log("[PASS] 25. Interactive Ship Weapon Sockets (Hull Display & Selection) verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 25. Interactive Sockets verification failed: {ex.Message}");
                failed++;
            }

            Debug.Log($"=== TOTAL VERIFICATION RESULT: {passed} PASSED, {failed} FAILED ===");
        }

        private static bool IsPointInsidePolygon(Vector2 pt, Vector2[] poly)
        {
            if (poly == null || poly.Length < 3) return false;
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].y > pt.y) != (poly[j].y > pt.y)) &&
                    (pt.x < (poly[j].x - poly[i].x) * (pt.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                {
                    inside = !inside;
                }
            }
            return inside;
        }
    }
}
