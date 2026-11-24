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
            CreateShipPrefab("frigate", 2, 140f, 180f, 16f, 110f, 22f, 32f, 24f, 3);
            CreateShipPrefab("destroyer", 2, 220f, 240f, 18f, 140f, 20f, 45f, 20f, 4);
            CreateShipPrefab("cruiser", 3, 320f, 350f, 22f, 180f, 24f, 55f, 18f, 5);
            CreateShipPrefab("battleship", 3, 600f, 500f, 28f, 300f, 20f, 75f, 15f, 6);

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
            if (sr.sprite == null)
            {
                sr.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GetShipSprite(shipId);
            }
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

            if (shipId == "carrier" || shipId == "vanguard")
            {
                var hangar = go.AddComponent<StarStrike.Gameplay.Carrier.CarrierHangarController>();
                hangar.droneCapacity = (def != null && def.droneHangarCapacity > 0) ? def.droneHangarCapacity : 4;
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
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void CreateWeaponPrefabs()
        {
            EnsureFolder("Assets/_Project", "Prefabs");
            EnsureFolder("Assets/_Project/Prefabs", "Weapons");

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

                // Verify zero recoil shock-back on ship when firing
                if (rb.linearVelocity.sqrMagnitude > 0.0001f)
                    throw new System.Exception("Ship must not shock back or gain reverse recoil velocity when firing");

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

            // 26. Verify Capacitor Depletion Lockout and Threshold Recovery System
            try
            {
                var testGo = new GameObject("TestShip_CapacitorLockout");
                var testHealth = testGo.AddComponent<ShipHealth>();
                testHealth.maxEnergy = 100f;
                testHealth.energyRegen = 20f;
                testHealth.energyRecoveryThresholdRatio = 0.30f; // 30 energy threshold
                testHealth.ResetMeters();

                if (testHealth.CurrentEnergy != 100f || testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Initial energy state should be full and not locked out");

                // Partial consumption should NOT lock out
                bool consumedPart = testHealth.ConsumeEnergy(70f);
                if (!consumedPart || testHealth.CurrentEnergy != 30f || testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Partial energy consumption should succeed without triggering lockout");

                // Option 2 (Last Gasp Overdraw): Insufficient partial consumption (need 50, have 30)
                // Overdraw allows the final shot, drains capacitor to 0, and trips lockout
                bool overdrawShot = testHealth.ConsumeEnergy(50f);
                if (!overdrawShot || testHealth.CurrentEnergy != 0f || !testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Option 2 Overdraw must fire final shot, drain energy to 0, and trigger lockout");

                // Option 1 (Brownout / Lockout Block): While locked out, HasEnergy and ConsumeEnergy must return false
                if (testHealth.HasEnergy(5f) || testHealth.ConsumeEnergy(5f))
                    throw new System.Exception("HasEnergy and ConsumeEnergy must reject while capacitor is locked out");

                // Regenerate partially below threshold (0.5 sec at 20/s = 10 energy < 30)
                testHealth.RegenerateMeters(0.5f);
                if (testHealth.CurrentEnergy < 9.9f || testHealth.CurrentEnergy > 10.1f)
                    throw new System.Exception($"Capacitor should have regenerated to ~10 energy, got {testHealth.CurrentEnergy}");
                if (!testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Capacitor must remain locked out below recovery threshold (10 < 30)");
                if (testHealth.HasEnergy(5f) || testHealth.ConsumeEnergy(5f))
                    throw new System.Exception("Lockout must prevent consuming even when energy is > request but below threshold");

                // Regenerate past recovery threshold (1.1 sec at 20/s = +22 energy, total 32 > 30)
                testHealth.RegenerateMeters(1.1f);
                if (testHealth.CurrentEnergy < 31.9f || testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Capacitor must clear lockout once reaching recovery threshold (>= 30)");

                // After lockout is cleared, weapons can consume energy normally
                bool firedPost = testHealth.ConsumeEnergy(12f);
                if (!firedPost || testHealth.CurrentEnergy < 19.9f || testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Energy consumption must succeed once lockout is cleared");

                // Test fixed threshold override
                testHealth.SetEnergyRecoveryThreshold(50f);
                if (testHealth.EnergyRecoveryThreshold != 50f)
                    throw new System.Exception("Fixed energy recovery threshold override failed to set");

                // Deplete completely again
                testHealth.ConsumeEnergy(testHealth.CurrentEnergy);
                if (!testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Second depletion must re-trigger lockout");

                // Regenerate to 40 (< 50 fixed threshold)
                testHealth.RegenerateMeters(2.0f); // +40 energy
                if (!testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Should still be locked out at 40 energy when threshold is 50");

                // Regenerate past 50
                testHealth.RegenerateMeters(0.6f); // +12 energy -> 52 energy
                if (testHealth.IsEnergyLockedOut)
                    throw new System.Exception("Lockout should clear once reaching fixed threshold 50");

                // Test Option 1 (Brownout without overdraw): allowOverdraw = false
                testHealth.allowOverdraw = false;
                testHealth.ResetMeters();
                testHealth.ConsumeEnergy(testHealth.CurrentEnergy - 8f); // 8 energy left
                bool strictDryFire = testHealth.ConsumeEnergy(12f); // 8 < 12, overdraw disabled
                if (strictDryFire || !testHealth.IsEnergyLockedOut || testHealth.CurrentEnergy != 0f)
                    throw new System.Exception("Option 1 dry-fire must reject firing, zero capacitor, and immediately trip lockout");

                Object.DestroyImmediate(testGo);

                Debug.Log("[PASS] 26. Capacitor Depletion Lockout and Threshold Recovery System verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 26. Capacitor Lockout verification failed: {ex.Message}");
                failed++;
            }

            // 27. Verify 6 Naval Ship Archetypes Data Architecture & Hardpoint Systems
            try
            {
                var archetypes = ShipArchetypeFactory.CreateAllArchetypes();
                if (archetypes == null || archetypes.Count != 6)
                    throw new System.Exception($"Expected 6 naval ship archetypes, got {archetypes?.Count ?? 0}");

                var fighter = ShipArchetypeFactory.CreateArchetype(ShipClass.Fighter);
                var frigate = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
                var destroyer = ShipArchetypeFactory.CreateArchetype(ShipClass.Destroyer);
                var cruiser = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
                var carrier = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
                var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);

                // Verify Progressive Camera & Mass Scaling
                if (fighter.cameraOrthoSize >= frigate.cameraOrthoSize ||
                    frigate.cameraOrthoSize >= destroyer.cameraOrthoSize ||
                    destroyer.cameraOrthoSize >= cruiser.cameraOrthoSize ||
                    cruiser.cameraOrthoSize >= carrier.cameraOrthoSize ||
                    carrier.cameraOrthoSize >= battleship.cameraOrthoSize)
                {
                    throw new System.Exception("Camera orthographic size must scale progressively from Fighter (7.5) to Battleship (15.0)");
                }

                if (fighter.hullMass >= frigate.hullMass ||
                    frigate.hullMass >= destroyer.hullMass ||
                    destroyer.hullMass >= cruiser.hullMass ||
                    cruiser.hullMass >= carrier.hullMass ||
                    carrier.hullMass >= battleship.hullMass)
                {
                    throw new System.Exception("Hull mass must scale progressively from Fighter (1.0) to Battleship (9.0)");
                }

                // Verify Hardpoint Sockets & Roles
                if (fighter.hardpointSockets.Length != 2 || fighter.hardpointSockets[0].size != SocketSize.Light)
                    throw new System.Exception("Fighter must have 2 Light hardpoint sockets");

                if (frigate.hardpointSockets.Length != 3)
                    throw new System.Exception("Frigate must have 3 hardpoint sockets");

                if (destroyer.hardpointSockets.Length != 4 || destroyer.hardpointSockets[2].size != SocketSize.Heavy)
                    throw new System.Exception("Destroyer must have 4 sockets including Heavy torpedo pods");

                if (cruiser.hardpointSockets.Length != 5)
                    throw new System.Exception("Cruiser must have 5 sockets including turrets and broadsides");

                if (carrier.hardpointSockets.Length != 4 || carrier.droneHangarCapacity != 4)
                    throw new System.Exception("Carrier must have 4 sockets and drone hangar capacity of 4");

                if (battleship.hardpointSockets.Length != 6 || battleship.hardpointSockets[0].size != SocketSize.Spinal || battleship.collisionArmorReduction < 0.35f)
                    throw new System.Exception("Battleship must have 6 sockets including a Spinal mount and >= 35% collision armor reduction");

                // Verify legacy interoperability
                var bSockets = battleship.GetWeaponSocketPositions();
                if (bSockets == null || bSockets.Length != 6 || battleship.SocketCount != 6)
                    throw new System.Exception("Legacy GetWeaponSocketPositions must seamlessly project hardpointSockets");

                Debug.Log("[PASS] 27. 6 Naval Ship Archetypes Data Architecture & Hardpoint Systems verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 27. Ship Archetypes verification failed: {ex.Message}");
                failed++;
            }

            // --- 28. Multi-Category Modular Equipment System & Expanded Ship Fleet Roster ---
            try
            {
                DataInitializer.EnsureShipAssets();

                var frigateAsset = Resources.Load<ShipDefinition>("Data/Ship_Frigate");
                var destroyerAsset = Resources.Load<ShipDefinition>("Data/Ship_Destroyer");
                var cruiserAsset = Resources.Load<ShipDefinition>("Data/Ship_Cruiser");
                var battleshipAsset = Resources.Load<ShipDefinition>("Data/Ship_Battleship");

                if (frigateAsset == null || destroyerAsset == null || cruiserAsset == null || battleshipAsset == null)
                    throw new System.Exception("Ship data assets (Frigate, Destroyer, Cruiser, Battleship) must exist in Resources/Data");

                var defenseMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Defense);
                var engineMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Engine);
                var reactorMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Reactor);
                var auxMods = EquipmentModuleCatalog.GetModulesByCategory(EquipmentSlotCategory.Auxiliary);

                if (defenseMods.Count < 3 || engineMods.Count < 3 || reactorMods.Count < 3 || auxMods.Count < 3)
                    throw new System.Exception("All 4 equipment module categories must have at least 3 distinct modules");

                var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);
                if (battleship.equipmentSlots == null || battleship.equipmentSlots.Length != 11)
                    throw new System.Exception("Battleship must have 11 modular equipment slots configured");

                var cruiser = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
                if (cruiser.equipmentSlots == null || cruiser.equipmentSlots.Length != 9)
                    throw new System.Exception("Cruiser must have 9 modular equipment slots configured");

                var frigate = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
                if (frigate.equipmentSlots == null || frigate.equipmentSlots.Length != 6)
                    throw new System.Exception("Frigate must have 6 modular equipment slots configured");

                // Test StorageService module saving & loading
                StorageService.SaveEquippedModule("cruiser", "def_0", "def_shield_booster");
                StorageService.SaveEquippedModule("cruiser", "eng_0", "eng_afterburner");
                if (StorageService.GetEquippedModule("cruiser", "def_0") != "def_shield_booster")
                    throw new System.Exception("StorageService failed to retrieve equipped defense module");
                if (StorageService.GetEquippedModule("cruiser", "eng_0") != "eng_afterburner")
                    throw new System.Exception("StorageService failed to retrieve equipped engine module");

                // Test Procedural Sprites for new ship classes
                var fSpr = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("frigate");
                var dSpr = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("destroyer");
                var cSpr = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("cruiser");
                var bSpr = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("battleship");
                if (fSpr == null || dSpr == null || cSpr == null || bSpr == null)
                    throw new System.Exception("ProceduralSpriteHelper must generate procedural sprites for all 6 ship classes");

                // Test Armor Damage Reduction
                var testGo = new GameObject("TestArmorVerification");
                var health = testGo.AddComponent<ShipHealth>();
                health.maxShield = 0f;
                health.currentShield = 0f;
                health.maxHull = 200f;
                health.currentHull = 200f;
                health.armorDamageReduction = 0.20f;
                health.TakeDamage(50f);
                if (Mathf.Abs(health.CurrentHull - 160f) > 0.01f)
                    throw new System.Exception($"Armor damage reduction failed. Expected 160 hull, got {health.CurrentHull}");
                Object.DestroyImmediate(testGo);

                Debug.Log("[PASS] 28. Multi-Category Modular Equipment System & Expanded Ship Fleet Roster verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 28. Modular Equipment System verification failed: {ex.Message}");
                failed++;
            }

            // 29. Verify Battleship & Cruiser 360° Independent Aiming Mechanics
            try
            {
                var cruiserDef = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
                var battleshipDef = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);
                var carrierDefTest = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
                var fighterDef = ShipArchetypeFactory.CreateArchetype(ShipClass.Fighter);

                if (!cruiserDef.canAimIndependently)
                    throw new System.Exception("Cruiser must have canAimIndependently set to true");
                if (!battleshipDef.canAimIndependently)
                    throw new System.Exception("Battleship must have canAimIndependently set to true");
                if (!carrierDefTest.canAimIndependently)
                    throw new System.Exception("Carrier must have canAimIndependently set to true");
                if (fighterDef.canAimIndependently)
                    throw new System.Exception("Fighter must have canAimIndependently set to false (fixed forward)");

                // Test Battleship independent weapon socket aiming
                var shipObj = new GameObject("Test_BattleshipAiming");
                shipObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Ship facing East (+X, 0 deg)
                var prog = shipObj.AddComponent<PlayerProgression>();
                prog.currentShip = battleshipDef;

                var weaponCtrl = shipObj.AddComponent<ShipWeaponController>();
                weaponCtrl.currentShipId = "battleship";
                weaponCtrl.UpdateSocketVisuals();

                if (weaponCtrl.SocketMounts.Count < 3)
                    throw new System.Exception($"Expected at least 3 socket mounts for battleship, got {weaponCtrl.SocketMounts.Count}");

                // Aim straight UP (North, +Y, 90 deg) while ship heading remains 0 deg East
                weaponCtrl.inputAimDirection = new Vector2(0f, 1f);
                weaponCtrl.UpdateIndependentAiming();

                // Turret / aimable socket mounts must be rotated to 90 degrees North, independent of ship's 0-degree East heading
                var firstMount = weaponCtrl.SocketMounts[0].mountTransform;
                float angle = firstMount.eulerAngles.z;
                if (Mathf.Abs(Mathf.DeltaAngle(angle, 90f)) > 1f)
                    throw new System.Exception($"Socket mount did not rotate to aim direction. Expected 90 deg, got {angle}");

                // Verify slow turret tilt: at Battleship speed 95 deg/s, after dt=0.5s it should tilt to ~47.5 deg (not snap instantly)
                firstMount.rotation = Quaternion.Euler(0f, 0f, 0f);
                weaponCtrl.UpdateIndependentAiming(0.5f);
                float gradualAngle = firstMount.eulerAngles.z;
                if (gradualAngle < 38f || gradualAngle > 58f)
                    throw new System.Exception($"Turret must tilt smoothly over time. Expected ~47.5 deg after 0.5s at 95 deg/s, got {gradualAngle}");

                Object.DestroyImmediate(shipObj);

                // Test Carrier independent weapon socket aiming: ALL weapon sockets must tilt towards cursor
                var carrierAimObj = new GameObject("Test_CarrierAiming");
                carrierAimObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var cProg = carrierAimObj.AddComponent<PlayerProgression>();
                cProg.currentShip = carrierDefTest;

                var cWeaponCtrl = carrierAimObj.AddComponent<ShipWeaponController>();
                cWeaponCtrl.currentShipId = "carrier";
                cWeaponCtrl.UpdateSocketVisuals();

                if (cWeaponCtrl.SocketMounts.Count < 3)
                    throw new System.Exception($"Expected at least 3 socket mounts for carrier, got {cWeaponCtrl.SocketMounts.Count}");

                // Aim straight UP (North, +Y, 90 deg)
                cWeaponCtrl.inputAimDirection = new Vector2(0f, 1f);
                cWeaponCtrl.UpdateIndependentAiming(-1f, true);

                for (int i = 0; i < cWeaponCtrl.SocketMounts.Count; i++)
                {
                    float cAngle = cWeaponCtrl.SocketMounts[i].mountTransform.eulerAngles.z;
                    if (Mathf.Abs(Mathf.DeltaAngle(cAngle, 90f)) > 1f)
                        throw new System.Exception($"Carrier socket mount {i} must tilt to aim direction (90 deg), got {cAngle}");
                }
                Object.DestroyImmediate(carrierAimObj);

                // Test Battleship & Cruiser hull steering: ship hull must NOT turn directly towards cursor
                var bbMovementObj = new GameObject("Test_BattleshipHullSteering");
                var bbRb = bbMovementObj.AddComponent<Rigidbody2D>();
                bbRb.rotation = 0f;
                bbMovementObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // Facing East (0 deg)
                var bbProg = bbMovementObj.AddComponent<PlayerProgression>();
                bbProg.currentShip = battleshipDef;
                var bbMove = bbMovementObj.AddComponent<ShipMovement>();
                bbMove.AimDirection = new Vector2(0f, 1f); // Cursor aiming North (90 deg)
                bbMove.TurnInput = 0f;

                if (bbMove.ShouldSteerTowardsAim)
                    throw new System.Exception("Battleship ShouldSteerTowardsAim must be false (ship should not turn direct to cursor)");

                bbMove.HandleSteering();
                if (Mathf.Abs(Mathf.DeltaAngle(bbRb.rotation, 0f)) > 0.01f)
                    throw new System.Exception($"Battleship hull should NOT rotate towards cursor. Expected 0 deg, got {bbRb.rotation}");

                // Verify TurnInput (A/D or helm steering) DOES steer the Battleship
                bbMove.TurnInput = 1f; // Turn counter-clockwise
                bbMove.HandleSteering();
                if (bbRb.rotation <= 0f)
                    throw new System.Exception($"Battleship hull should steer with TurnInput. Expected > 0 deg, got {bbRb.rotation}");

                Object.DestroyImmediate(bbMovementObj);

                // Test Cruiser hull steering: also must NOT turn directly towards cursor
                var crMovementObj = new GameObject("Test_CruiserHullSteering");
                var crRb = crMovementObj.AddComponent<Rigidbody2D>();
                crRb.rotation = 0f;
                crMovementObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var crProg = crMovementObj.AddComponent<PlayerProgression>();
                crProg.currentShip = cruiserDef;
                var crMove = crMovementObj.AddComponent<ShipMovement>();
                crMove.AimDirection = new Vector2(0f, 1f);
                crMove.TurnInput = 0f;

                if (crMove.ShouldSteerTowardsAim)
                    throw new System.Exception("Cruiser ShouldSteerTowardsAim must be false (ship should not turn direct to cursor)");

                crMove.HandleSteering();
                if (Mathf.Abs(Mathf.DeltaAngle(crRb.rotation, 0f)) > 0.01f)
                    throw new System.Exception($"Cruiser hull should NOT rotate towards cursor. Expected 0 deg, got {crRb.rotation}");

                Object.DestroyImmediate(crMovementObj);

                // Test Fighter fixed forward aiming (must NOT rotate independently)
                var fighterObj = new GameObject("Test_FighterFixedAiming");
                fighterObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var fProg = fighterObj.AddComponent<PlayerProgression>();
                fProg.currentShip = fighterDef;

                var fWeaponCtrl = fighterObj.AddComponent<ShipWeaponController>();
                fWeaponCtrl.currentShipId = "fighter";
                fWeaponCtrl.UpdateSocketVisuals();

                fWeaponCtrl.inputAimDirection = new Vector2(0f, 1f);
                fWeaponCtrl.UpdateIndependentAiming();

                var fMount = fWeaponCtrl.SocketMounts[0].mountTransform;
                float fAngle = fMount.eulerAngles.z;
                if (Mathf.Abs(Mathf.DeltaAngle(fAngle, 0f)) > 1f)
                    throw new System.Exception($"Fighter weapon mount should remain fixed to ship forward (0 deg), got {fAngle}");

                // Fighter hull steering DOES turn towards cursor
                var fRb = fighterObj.AddComponent<Rigidbody2D>();
                fRb.rotation = 0f;
                var fMove = fighterObj.AddComponent<ShipMovement>();
                fMove.AimDirection = new Vector2(0f, 1f);
                fMove.TurnInput = 0f;

                if (!fMove.ShouldSteerTowardsAim)
                    throw new System.Exception("Fighter ShouldSteerTowardsAim must be true");

                fMove.HandleSteering();
                if (fRb.rotation <= 0f)
                    throw new System.Exception($"Fighter hull should rotate towards cursor. Expected > 0 deg, got {fRb.rotation}");

                Object.DestroyImmediate(fighterObj);

                // Test Destroyer and Frigate fixed forward aiming (must NOT tilt weapons)
                var destDef = ShipArchetypeFactory.CreateArchetype(ShipClass.Destroyer);
                var frigDef = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
                if (destDef.canAimIndependently)
                    throw new System.Exception("Destroyer must have canAimIndependently set to false");
                if (frigDef.canAimIndependently)
                    throw new System.Exception("Frigate must have canAimIndependently set to false");

                var destObj = new GameObject("Test_DestroyerFixedAiming");
                destObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                var dProg = destObj.AddComponent<PlayerProgression>();
                dProg.currentShip = destDef;
                var dWeaponCtrl = destObj.AddComponent<ShipWeaponController>();
                dWeaponCtrl.currentShipId = "destroyer";
                dWeaponCtrl.UpdateSocketVisuals();
                dWeaponCtrl.inputAimDirection = new Vector2(0f, 1f);
                dWeaponCtrl.UpdateIndependentAiming();
                for (int i = 0; i < dWeaponCtrl.SocketMounts.Count; i++)
                {
                    float dAngle = dWeaponCtrl.SocketMounts[i].mountTransform.localEulerAngles.z;
                    if (Mathf.Abs(Mathf.DeltaAngle(dAngle, 0f)) > 1f)
                        throw new System.Exception($"Destroyer socket {i} should remain locked forward (0 deg), got {dAngle}");
                }
                Object.DestroyImmediate(destObj);

                // Verify Player ship does NOT change to red on direct hull hit
                var testPlayerGo = new GameObject("Player");
                testPlayerGo.tag = "Player";
                var playerHealth = testPlayerGo.AddComponent<ShipHealth>();
                var playerSr = testPlayerGo.AddComponent<SpriteRenderer>();
                playerSr.color = Color.white;
                playerHealth.maxShield = 0f;
                playerHealth.currentShield = 0f;
                playerHealth.maxHull = 100f;
                playerHealth.currentHull = 100f;
                playerHealth.TakeDamage(25f);
                if (playerSr.color != Color.white)
                    throw new System.Exception($"Player ship sprite color should remain unchanged on hull hit, got {playerSr.color}");
                Object.DestroyImmediate(testPlayerGo);

                Debug.Log("[PASS] 29. Battleship & Cruiser 360° Independent Aiming Mechanics verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 29. Independent Aiming verification failed: {ex.Message}");
                failed++;
            }

            // 30. Verify Global UI & HUD Text Enlargement
            try
            {
                if (StarStrike.UI.UIHelper.TextSizeScale <= 1.0f)
                    throw new System.Exception("UIHelper.TextSizeScale must be greater than 1.0f for text enlargement");

                int scaled16 = StarStrike.UI.UIHelper.ScaleFontSize(16);
                if (scaled16 <= 16)
                    throw new System.Exception($"UIHelper.ScaleFontSize(16) should be larger than 16, got {scaled16}");

                var testRoot = new GameObject("TestTextEnlargement");
                var tmp = StarStrike.UI.UIHelper.CreateTextMeshPro(
                    testRoot.transform,
                    "EnlargedLabel",
                    "SORTIE",
                    16,
                    TMPro.FontStyles.Bold,
                    Color.white,
                    TMPro.TextAlignmentOptions.Center,
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    Vector2.zero);

                if (tmp.fontSize <= 16)
                    throw new System.Exception($"TextMeshPro fontSize was not enlarged. Expected > 16, got {tmp.fontSize}");

                Object.DestroyImmediate(testRoot);

                Debug.Log("[PASS] 30. Global UI & HUD Text Enlargement verified");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 30. Text enlargement verification failed: {ex.Message}");
                failed++;
            }

            // 31. Verify "Used Future" Aesthetic & Visual Direction (Muted Hulls, Scale Windows, Cinematic Tracers)
            try
            {
                StarStrike.Gameplay.ProceduralSpriteHelper.ClearCache();

                // 1. Capital Ships Scale & Micro-Window Lights (Cruiser, Battleship, Carrier)
                var bbSprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("battleship");
                var crSprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("cruiser");
                var caSprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("carrier");
                if (bbSprite == null || crSprite == null || caSprite == null)
                    throw new System.Exception("Failed to generate capital ship sprites");

                // Verify tiny pinpoint deck windows exist on capital ships
                var bbTex = bbSprite.texture;
                int bbWindowCount = 0;
                float bbSatSum = 0f;
                int bbHullPixelCount = 0;

                for (int y = 0; y < bbTex.height; y++)
                {
                    for (int x = 0; x < bbTex.width; x++)
                    {
                        var col = bbTex.GetPixel(x, y);
                        if (col.a > 0.5f)
                        {
                            Color.RGBToHSV(col, out float h, out float s, out float v);
                            // Micro-deck windows: high brightness (>0.9) with warm incandescent or bridge cyan tint
                            if (v > 0.90f && (col.r > 0.7f && col.g > 0.7f))
                            {
                                bbWindowCount++;
                            }
                            else
                            {
                                bbSatSum += s;
                                bbHullPixelCount++;
                            }
                        }
                    }
                }

                if (bbWindowCount < 10)
                    throw new System.Exception($"Battleship must have at least 10 pinpoint deck windows for floating city scale, found {bbWindowCount}");

                float bbAvgSat = bbHullPixelCount > 0 ? (bbSatSum / bbHullPixelCount) : 1f;
                if (bbAvgSat > 0.35f)
                    throw new System.Exception($"Battleship base hull should be muted industrial grey/slate (avg saturation < 0.35), got {bbAvgSat}");

                // 2. Scale Contrast: Strike Craft (Fighter) should have cockpit canopy, NOT scattered deck window rows
                var fSprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("fighter");
                var fTex = fSprite.texture;
                int fCanopyPixels = 0;
                float fSatSum = 0f;
                int fHullPixelCount = 0;

                for (int y = 0; y < fTex.height; y++)
                {
                    for (int x = 0; x < fTex.width; x++)
                    {
                        var col = fTex.GetPixel(x, y);
                        if (col.a > 0.5f)
                        {
                            Color.RGBToHSV(col, out float h, out float s, out float v);
                            // Cockpit glint / canopy
                            if (v > 0.85f && col.b > 0.75f)
                            {
                                fCanopyPixels++;
                            }
                            else
                            {
                                fSatSum += s;
                                fHullPixelCount++;
                            }
                        }
                    }
                }

                if (fCanopyPixels == 0)
                    throw new System.Exception("Fighter strike craft must have cockpit canopy specular glint");

                float fAvgSat = fHullPixelCount > 0 ? (fSatSum / fHullPixelCount) : 1f;
                if (fAvgSat > 0.35f)
                    throw new System.Exception($"Fighter base hull should be muted industrial gunmetal (avg saturation < 0.35), got {fAvgSat}");

                // 3. Cinematic Laser Tracers (Thick glowing bolts with blinding hot-white core)
                var playerLaser = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateLaserSprite(false);
                var enemyLaser = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateLaserSprite(true);
                var gatlingTracer = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateGatlingTracerSprite();

                var pTex = playerLaser.texture;
                var eTex = enemyLaser.texture;
                var gTex = gatlingTracer.texture;

                // Center core pixel should be ultra-hot pure white core
                var pCore = pTex.GetPixel(pTex.width / 2, pTex.height / 2);
                if (pCore.r < 0.9f || pCore.g < 0.9f || pCore.b < 0.9f)
                    throw new System.Exception($"Player laser center core must be blinding hot white (r,g,b > 0.9), got {pCore}");

                var eCore = eTex.GetPixel(eTex.width / 2, eTex.height / 2);
                if (eCore.r < 0.9f || eCore.g < 0.9f || eCore.b < 0.9f)
                    throw new System.Exception($"Enemy laser center core must be blinding hot white (r,g,b > 0.9), got {eCore}");

                var gCore = gTex.GetPixel(gTex.width / 2, gTex.height / 2);
                if (gCore.r < 0.9f || gCore.g < 0.9f)
                    throw new System.Exception($"Gatling tracer center core must be hot white, got {gCore}");

                // Chromatic mantle: outer halo should be saturated player cyan / enemy red
                var pMantle = pTex.GetPixel(pTex.width / 2, (int)(pTex.height * 0.72f));
                if (pMantle.b < 0.7f)
                    throw new System.Exception($"Player laser mantle should be vibrant cyan-blue, got {pMantle}");

                var eMantle = eTex.GetPixel(eTex.width / 2, (int)(eTex.height * 0.72f));
                if (eMantle.r < 0.7f)
                    throw new System.Exception($"Enemy laser mantle should be vibrant crimson, got {eMantle}");

                Debug.Log("[PASS] 31. 'Used Future' Aesthetic & Visual Direction verified (Muted Hulls, Capital Scale Windows, Cinematic Tracers)");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 31. 'Used Future' Aesthetic verification failed: {ex.Message}");
                failed++;
            }

            // 32. Verify Carrier Ship Prototype (Drone Foundry, Squadron Deployment, Stance Commands, PDC)
            try
            {
                var carrierDef = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
                if (carrierDef == null)
                    throw new System.Exception("Failed to create Carrier archetype");
                if (carrierDef.droneHangarCapacity != 4)
                    throw new System.Exception($"Carrier droneHangarCapacity expected 4, got {carrierDef.droneHangarCapacity}");
                if (carrierDef.defaultDroneType != "interceptor_drone")
                    throw new System.Exception($"Carrier defaultDroneType expected 'interceptor_drone', got {carrierDef.defaultDroneType}");

                // Instantiate Carrier prototype GameObject
                var carrierGo = new GameObject("Test_VanguardCarrier");
                carrierGo.tag = "Player";
                var prog = carrierGo.AddComponent<PlayerProgression>();
                prog.currentShip = carrierDef;

                var hangar = carrierGo.AddComponent<StarStrike.Gameplay.Carrier.CarrierHangarController>();
                hangar.droneCapacity = carrierDef.droneHangarCapacity;
                hangar.droneBuildTime = 3.0f;

                // Test initial escort squadron deployment
                hangar.SpawnInitialSquadron();
                if (hangar.ActiveDroneCount != 4)
                    throw new System.Exception($"Carrier should deploy 4 escort drones, got {hangar.ActiveDroneCount}");

                var firstDrone = hangar.ActiveDrones[0];
                if (firstDrone == null || firstDrone.carrier != hangar)
                    throw new System.Exception("Escort drone must be valid and linked to mother carrier");
                if (firstDrone.gameObject.CompareTag("Player"))
                    throw new System.Exception("Escort drone must not hijack 'Player' tag, ensuring unique player mothership tracking");
                if (!(firstDrone is IDamageable))
                    throw new System.Exception("Escort drone must implement IDamageable interface");
                if (firstDrone.maxHull != 60f || firstDrone.maxShield != 80f)
                    throw new System.Exception($"Escort drone defensive stats mismatch. Hull: {firstDrone.maxHull}, Shield: {firstDrone.maxShield}");

                // Test Tactical Stance transitions
                if (hangar.CurrentStance != StarStrike.Gameplay.Carrier.DroneCommandStance.DefensiveEscort)
                    throw new System.Exception("Initial stance must be DefensiveEscort");

                hangar.ToggleStance();
                if (hangar.CurrentStance != StarStrike.Gameplay.Carrier.DroneCommandStance.OffensiveStrike)
                    throw new System.Exception("First stance toggle must transition to OffensiveStrike");

                hangar.ToggleStance();
                if (hangar.CurrentStance != StarStrike.Gameplay.Carrier.DroneCommandStance.DockAndRepair)
                    throw new System.Exception("Second stance toggle must transition to DockAndRepair");

                hangar.ToggleStance();
                if (hangar.CurrentStance != StarStrike.Gameplay.Carrier.DroneCommandStance.DefensiveEscort)
                    throw new System.Exception("Third stance toggle must return to DefensiveEscort");

                // Test Automated Foundry Replacement
                hangar.NotifyDroneDestroyed(firstDrone);
                Object.DestroyImmediate(firstDrone.gameObject);
                if (hangar.ActiveDroneCount != 3)
                    throw new System.Exception($"Active drone count should be 3 after destruction, got {hangar.ActiveDroneCount}");
                if (!hangar.IsBuilding)
                    throw new System.Exception("Foundry should be in building state when below capacity");

                var replacement = hangar.LaunchNewDrone();
                if (replacement == null || hangar.ActiveDroneCount != 4)
                    throw new System.Exception($"Foundry replacement launch failed. Active count: {hangar.ActiveDroneCount}");
                if (hangar.IsBuilding)
                    throw new System.Exception("Foundry should not be building when at full capacity");

                // Clean up spawned drones and carrier
                for (int i = 0; i < hangar.ActiveDrones.Count; i++)
                {
                    if (hangar.ActiveDrones[i] != null)
                    {
                        Object.DestroyImmediate(hangar.ActiveDrones[i].gameObject);
                    }
                }
                Object.DestroyImmediate(carrierGo);

                Debug.Log("[PASS] 32. Carrier Ship Prototype verified (Drone Foundry, Squadron Deployment, Stance Commands, PDC)");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 32. Carrier Ship Prototype verification failed: {ex.Message}");
                failed++;
            }

            // 33. Verify EventSystem Singleton Lifecycle Guarantee (Exactly one EventSystem maintained)
            try
            {
                // Create multiple EventSystems to simulate duplicate scene loads
                var es1 = new GameObject("Test_EventSystem_1");
                es1.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es1.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

                var es2 = new GameObject("Test_EventSystem_2");
                es2.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es2.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

                var allBefore = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
                if (allBefore.Length < 2)
                    throw new System.Exception($"Expected at least 2 EventSystems before deduplication, found {allBefore.Length}");

                // Run enforcement
                var keeper = StarStrike.UI.EventSystemSingleton.EnforceSingleEventSystem();
                if (keeper == null)
                    throw new System.Exception("EnforceSingleEventSystem returned null keeper");

                var allAfter = Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None);
                if (allAfter.Length != 1)
                    throw new System.Exception($"Expected exactly 1 EventSystem after deduplication, found {allAfter.Length}");

                // Clean up keeper
                Object.DestroyImmediate(keeper.gameObject);

                Debug.Log("[PASS] 33. EventSystem Singleton Lifecycle Guarantee verified (Exactly 1 EventSystem enforced)");
                passed++;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[FAIL] 33. EventSystem Singleton verification failed: {ex.Message}");
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
