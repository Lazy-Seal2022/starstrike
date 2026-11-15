using UnityEditor;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Editor
{
    public class DataInitializer
    {
        [MenuItem("StarStrike/Initialize Data")]
        public static void InitializeData()
        {
            EnsureFolder("Assets/_Project", "Resources");
            EnsureFolder("Assets/_Project/Resources", "Data");

            string dataPath = "Assets/_Project/Resources/Data";

            // 1. Upgrade Definition
            var upgradeDef = CreateOrUpdateAsset(ScriptableObject.CreateInstance<UpgradeDefinition>(), $"{dataPath}/UpgradeDefinition.asset");

            // 2. Flight Config
            var flightConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<FlightConfig>(), $"{dataPath}/FlightConfig.asset");

            // 3. Combat Config
            var combatConfig = ScriptableObject.CreateInstance<CombatConfig>();
            combatConfig.playerLaserSprite = LoadArtSprite("Projectiles/Laser_Player.png");
            combatConfig.enemyLaserSprite = LoadArtSprite("Projectiles/Laser_Enemy.png");
            combatConfig = CreateOrUpdateAsset(combatConfig, $"{dataPath}/CombatConfig.asset");

            // 4. Asteroid Config
            var asteroidConfig = ScriptableObject.CreateInstance<AsteroidConfig>();
            asteroidConfig.sprite = LoadArtSprite("World/Asteroid.png");
            asteroidConfig = CreateOrUpdateAsset(asteroidConfig, $"{dataPath}/AsteroidConfig.asset");

            // 5. Boss Config
            var bossConfig = ScriptableObject.CreateInstance<BossConfig>();
            bossConfig.sprite = LoadArtSprite("Ships/Ship_Boss.png");
            bossConfig = CreateOrUpdateAsset(bossConfig, $"{dataPath}/BossConfig.asset");

            // 6. Loot Config
            var lootConfig = ScriptableObject.CreateInstance<LootConfig>();
            lootConfig.gemSprite = LoadArtSprite("World/Gem.png");
            lootConfig.powerUpSprite = LoadArtSprite("World/PowerUp.png");
            lootConfig = CreateOrUpdateAsset(lootConfig, $"{dataPath}/LootConfig.asset");

            // 7. UI Theme Config
            var uiThemeConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<UIThemeConfig>(), $"{dataPath}/UIThemeConfig.asset");

            // 7b. Audio Config
            var audioConfig = ScriptableObject.CreateInstance<AudioConfig>();
            audioConfig.ambientMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Music/ambient_music.ogg");
            audioConfig.laser = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Combat/laser.ogg");
            audioConfig.enemyLaser = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Combat/enemy_laser.ogg");
            audioConfig.explosion = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/Combat/explosion.ogg");
            audioConfig.gem = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/World/gem.ogg");
            audioConfig.click = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/click.ogg");
            audioConfig.upgrade = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/SFX/UI/upgrade.ogg");
            audioConfig.masterMixer = AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>("Assets/_Project/Audio/StarStrikeMasterMixer.mixer");
            if (audioConfig.masterMixer != null)
            {
                var masterGroups = audioConfig.masterMixer.FindMatchingGroups("Master");
                if (masterGroups != null && masterGroups.Length > 0) audioConfig.masterGroup = masterGroups[0];
                var musicGroups = audioConfig.masterMixer.FindMatchingGroups("Music");
                if (musicGroups != null && musicGroups.Length > 0) audioConfig.musicGroup = musicGroups[0];
                var sfxGroups = audioConfig.masterMixer.FindMatchingGroups("SFX");
                if (sfxGroups != null && sfxGroups.Length > 0) audioConfig.sfxGroup = sfxGroups[0];
                var uiGroups = audioConfig.masterMixer.FindMatchingGroups("UI");
                if (uiGroups != null && uiGroups.Length > 0) audioConfig.uiGroup = uiGroups[0];
            }
            audioConfig = CreateOrUpdateAsset(audioConfig, $"{dataPath}/AudioConfig.asset");

            // 8. Enemy Drone Definition
            var drone = ScriptableObject.CreateInstance<EnemyDefinition>();
            drone.id = "drone";
            drone.enemyName = "Hostile Drone";
            drone.maxShield = 60f;
            drone.speed = 6f;
            drone.agility = 5f;
            drone.pulseDamage = 14f;
            drone.pulseSpeed = 18f;
            drone.fireRate = 1.1f;
            drone.sightRadius = 16f;
            drone.attackRadius = 10f;
            drone.sprite = LoadArtSprite("Ships/Ship_Drone.png");
            drone = CreateOrUpdateAsset(drone, $"{dataPath}/Enemy_Drone.asset");

            // 9. Ships
            var scout = ScriptableObject.CreateInstance<ShipDefinition>();
            scout.id = "scout";
            scout.shipName = "Delta Scout";
            scout.tier = 1;
            scout.maxUpgradeLevel = 4;
            scout.weaponSlots = 1;
            scout.sprite = LoadArtSprite("Ships/Ship_Scout.png");
            scout.baseStats = new StatValues { shieldCapacity = 100, shieldRegen = 8, energyCapacity = 80, energyRegen = 18, pulseDamage = 18, pulseSpeed = 600, speed = 260, agility = 4.5f };
            scout = CreateOrUpdateAsset(scout, $"{dataPath}/Ship_Scout.asset");

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            fighter.id = "fighter";
            fighter.shipName = "Ares Fighter";
            fighter.tier = 2;
            fighter.maxUpgradeLevel = 5;
            fighter.weaponSlots = 2;
            fighter.sprite = LoadArtSprite("Ships/Ship_Fighter.png");
            fighter.baseStats = new StatValues { shieldCapacity = 150, shieldRegen = 12, energyCapacity = 120, energyRegen = 24, pulseDamage = 28, pulseSpeed = 680, speed = 310, agility = 5.0f };
            fighter = CreateOrUpdateAsset(fighter, $"{dataPath}/Ship_Fighter.asset");

            var miner = ScriptableObject.CreateInstance<ShipDefinition>();
            miner.id = "miner";
            miner.shipName = "Goliath Miner";
            miner.tier = 2;
            miner.maxUpgradeLevel = 5;
            miner.weaponSlots = 2;
            miner.sprite = LoadArtSprite("Ships/Ship_Miner.png");
            miner.baseStats = new StatValues { shieldCapacity = 220, shieldRegen = 16, energyCapacity = 150, energyRegen = 20, pulseDamage = 40, pulseSpeed = 520, speed = 210, agility = 3.5f };
            miner = CreateOrUpdateAsset(miner, $"{dataPath}/Ship_Miner.asset");

            var interceptor = ScriptableObject.CreateInstance<ShipDefinition>();
            interceptor.id = "interceptor";
            interceptor.shipName = "Viper Interceptor";
            interceptor.tier = 3;
            interceptor.maxUpgradeLevel = 6;
            interceptor.weaponSlots = 2;
            interceptor.sprite = LoadArtSprite("Ships/Ship_Interceptor.png");
            interceptor.baseStats = new StatValues { shieldCapacity = 180, shieldRegen = 15, energyCapacity = 140, energyRegen = 30, pulseDamage = 35, pulseSpeed = 800, speed = 400, agility = 6.5f };
            interceptor = CreateOrUpdateAsset(interceptor, $"{dataPath}/Ship_Interceptor.asset");

            var juggernaut = ScriptableObject.CreateInstance<ShipDefinition>();
            juggernaut.id = "juggernaut";
            juggernaut.shipName = "Titan Juggernaut";
            juggernaut.tier = 3;
            juggernaut.maxUpgradeLevel = 6;
            juggernaut.weaponSlots = 3;
            juggernaut.sprite = LoadArtSprite("Ships/Ship_Juggernaut.png");
            juggernaut.baseStats = new StatValues { shieldCapacity = 400, shieldRegen = 25, energyCapacity = 200, energyRegen = 25, pulseDamage = 75, pulseSpeed = 480, speed = 180, agility = 2.5f };
            juggernaut = CreateOrUpdateAsset(juggernaut, $"{dataPath}/Ship_Juggernaut.asset");

            scout.evolvesTo = new ShipDefinition[] { fighter, miner };
            fighter.evolvesTo = new ShipDefinition[] { interceptor };
            miner.evolvesTo = new ShipDefinition[] { juggernaut };
            interceptor.evolvesTo = new ShipDefinition[0];
            juggernaut.evolvesTo = new ShipDefinition[0];

            EditorUtility.SetDirty(scout);
            EditorUtility.SetDirty(fighter);
            EditorUtility.SetDirty(miner);
            EditorUtility.SetDirty(interceptor);
            EditorUtility.SetDirty(juggernaut);

            // 10. Master Game Config
            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.worldWidth = 140f;
            gameConfig.worldHeight = 140f;
            gameConfig.asteroidCount = 50;
            gameConfig.initialDroneCount = 6;
            gameConfig.flightConfig = flightConfig;
            gameConfig.combatConfig = combatConfig;
            gameConfig.asteroidConfig = asteroidConfig;
            gameConfig.bossConfig = bossConfig;
            gameConfig.lootConfig = lootConfig;
            gameConfig.uiThemeConfig = uiThemeConfig;
            gameConfig.audioConfig = audioConfig;
            gameConfig.upgradeDefinition = upgradeDef;
            gameConfig.defaultShip = scout;
            gameConfig = CreateOrUpdateAsset(gameConfig, $"{dataPath}/GameConfig.asset");

            // 11. Generate Prefabs
            PrefabInitializer.GeneratePrefabs();

            // 12. Wire Prefab References into Pure Configurations
            combatConfig.playerLaserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/PlayerLaser.prefab");
            combatConfig.enemyLaserPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Projectiles/EnemyLaser.prefab");
            EditorUtility.SetDirty(combatConfig);

            asteroidConfig.largePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Environment/Asteroid_Large.prefab");
            asteroidConfig.mediumPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Environment/Asteroid_Medium.prefab");
            asteroidConfig.smallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Environment/Asteroid_Small.prefab");
            asteroidConfig.gemCrystalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Environment/GemCrystal.prefab");
            asteroidConfig.powerUpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Environment/PowerUpItem.prefab");
            EditorUtility.SetDirty(asteroidConfig);

            gameConfig.enemyDronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/EnemyDrone.prefab");
            gameConfig.bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/BossDreadnought.prefab");
            EditorUtility.SetDirty(gameConfig);

            scout.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Ships/Ship_Scout.prefab");
            fighter.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Ships/Ship_Fighter.prefab");
            miner.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Ships/Ship_Miner.prefab");
            interceptor.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Ships/Ship_Interceptor.prefab");
            juggernaut.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Ships/Ship_Juggernaut.prefab");
            EditorUtility.SetDirty(scout);
            EditorUtility.SetDirty(fighter);
            EditorUtility.SetDirty(miner);
            EditorUtility.SetDirty(interceptor);
            EditorUtility.SetDirty(juggernaut);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("StarStrike pure ScriptableObject configuration assets and prefabs successfully generated!");
        }

        private static void EnsureFolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(full))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static T CreateOrUpdateAsset<T>(T asset, string path) where T : UnityEngine.Object
        {
            asset.name = System.IO.Path.GetFileNameWithoutExtension(path);
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
                return asset;
            }
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
            return null;
        }
    }
}
