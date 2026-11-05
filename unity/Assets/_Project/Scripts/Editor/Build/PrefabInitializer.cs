using UnityEditor;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.UI;

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

            EnsureFolder("Assets/_Project", "Resources");
            EnsureFolder("Assets/_Project/Resources", "Prefabs");
            EnsureFolder("Assets/_Project/Resources/Prefabs", "Projectiles");
            EnsureFolder("Assets/_Project/Resources/Prefabs", "Environment");
            EnsureFolder("Assets/_Project/Resources/Prefabs", "Enemies");
            EnsureFolder("Assets/_Project/Resources/Prefabs", "Ships");
            EnsureFolder("Assets/_Project/Resources/Prefabs", "UI");

            // 1. Projectiles
            CreatePlayerLaserPrefab();
            CreateEnemyLaserPrefab();

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
            CreateShipPrefab("scout", 1, 100f, 8f, 80f, 18f, 18f, 22f, 1);
            CreateShipPrefab("fighter", 2, 150f, 12f, 120f, 24f, 28f, 24f, 2);
            CreateShipPrefab("miner", 2, 220f, 16f, 150f, 20f, 40f, 20f, 2);
            CreateShipPrefab("interceptor", 3, 180f, 15f, 140f, 30f, 35f, 28f, 2);
            CreateShipPrefab("juggernaut", 3, 400f, 25f, 200f, 25f, 75f, 18f, 3);

            // 5. UI Prefabs
            CreateCanvasHUDPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("StarStrike Prefabs successfully generated in Assets/_Project/Prefabs and Assets/_Project/Resources/Prefabs!");
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
            sr.sprite = ProceduralSpriteHelper.GetLaserSprite(false);
            sr.sortingOrder = 5;
            sr.color = new Color(0.2f, 0.9f, 1f, 1f);

            var proj = go.AddComponent<LaserProjectile>();
            proj.isEnemy = false;
            proj.damage = 18f;
            proj.speed = 25f;
            proj.lifetime = 3f;
            proj.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            var tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.15f;
            tr.startWidth = 0.12f;
            tr.endWidth = 0f;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null) tr.material = new Material(shader);
            tr.startColor = new Color(0.2f, 0.9f, 1f, 0.8f);
            tr.endColor = new Color(0.1f, 0.5f, 1f, 0f);

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
            sr.sprite = ProceduralSpriteHelper.GetLaserSprite(true);
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.3f, 0.2f, 1f);

            var proj = go.AddComponent<LaserProjectile>();
            proj.isEnemy = true;
            proj.damage = 14f;
            proj.speed = 20f;
            proj.lifetime = 3f;
            proj.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            var tr = go.AddComponent<TrailRenderer>();
            tr.time = 0.15f;
            tr.startWidth = 0.12f;
            tr.endWidth = 0f;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null) tr.material = new Material(shader);
            tr.startColor = new Color(1f, 0.3f, 0.2f, 0.8f);
            tr.endColor = new Color(0.8f, 0.1f, 0.1f, 0f);

            SavePrefab(go, "Projectiles/EnemyLaser");
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
            sr.sprite = ProceduralSpriteHelper.GetAsteroidSprite();
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
            sr.sprite = ProceduralSpriteHelper.GetGemSprite();
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
            sr.sprite = ProceduralSpriteHelper.GetStarSprite();
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

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.7f;
            col.isTrigger = false;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite("drone");
            sr.sortingOrder = 4;

            var mov = go.AddComponent<ShipMovement>();
            mov.flightConfig = AssetDatabase.LoadAssetAtPath<FlightConfig>("Assets/_Project/Resources/Data/FlightConfig.asset");

            var health = go.AddComponent<ShipHealth>();
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

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
            col.isTrigger = false;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite("boss");
            sr.sortingOrder = 4;

            var mov = go.AddComponent<ShipMovement>();
            mov.flightConfig = AssetDatabase.LoadAssetAtPath<FlightConfig>("Assets/_Project/Resources/Data/FlightConfig.asset");

            var health = go.AddComponent<ShipHealth>();
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

            SavePrefab(go, "Enemies/BossDreadnought");
        }

        private static void CreateShipPrefab(string shipId, int tier, float shield, float shieldRegen, float energy, float energyRegen, float dmg, float spd, int weaponSlots)
        {
            string capId = char.ToUpper(shipId[0]) + shipId.Substring(1);
            GameObject go = new GameObject($"Ship_{capId}");
            go.tag = "Player";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 1.0f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.6f;
            col.isTrigger = false;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite(shipId);
            sr.sortingOrder = 4;

            var mov = go.AddComponent<ShipMovement>();
            mov.flightConfig = AssetDatabase.LoadAssetAtPath<FlightConfig>("Assets/_Project/Resources/Data/FlightConfig.asset");

            var health = go.AddComponent<ShipHealth>();
            health.isPlayer = true;
            health.maxShield = shield;
            health.shieldRegen = shieldRegen;
            health.maxEnergy = energy;
            health.energyRegen = energyRegen;
            health.currentShield = shield;
            health.currentEnergy = energy;

            var weapon = go.AddComponent<ShipWeapon>();
            weapon.isPlayer = true;
            weapon.pulseDamage = dmg;
            weapon.pulseSpeed = spd;
            weapon.weaponSlots = weaponSlots;
            weapon.combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfig>("Assets/_Project/Resources/Data/CombatConfig.asset");

            var prog = go.AddComponent<PlayerProgression>();
            prog.currentShip = AssetDatabase.LoadAssetAtPath<ShipDefinition>($"Assets/_Project/Resources/Data/Ship_{capId}.asset");
            prog.gameConfig = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Project/Resources/Data/GameConfig.asset");

            GameObject leftMuzzle = new GameObject("LeftMuzzle");
            leftMuzzle.transform.SetParent(go.transform);
            leftMuzzle.transform.localPosition = new Vector3(-0.3f, 0.65f, 0f);

            GameObject rightMuzzle = new GameObject("RightMuzzle");
            rightMuzzle.transform.SetParent(go.transform);
            rightMuzzle.transform.localPosition = new Vector3(0.3f, 0.65f, 0f);

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

            hud.playerStatus = statusComp;
            hud.evolutionBannerController = evoBanner;
            hud.upgradeDrawerController = upgradeDrawer;
            hud.floatingTextController = floatText;

            SavePrefab(hudGo, "UI/CanvasHUD");
        }

        private static void SavePrefab(GameObject go, string subPath)
        {
            string path1 = $"Assets/_Project/Prefabs/{subPath}.prefab";
            string path2 = $"Assets/_Project/Resources/Prefabs/{subPath}.prefab";

            PrefabUtility.SaveAsPrefabAsset(go, path1);
            
            AssetDatabase.DeleteAsset(path2);
            AssetDatabase.CopyAsset(path1, path2);

            Object.DestroyImmediate(go);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
