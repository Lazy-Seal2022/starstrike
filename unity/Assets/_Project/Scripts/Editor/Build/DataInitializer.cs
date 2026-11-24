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
            bossConfig.maxShield = 2500f;
            bossConfig.maxHull = 1500f;
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
            drone.maxHull = 40f;
            drone.speed = 6f;
            drone.agility = 5f;
            drone.pulseDamage = 14f;
            drone.pulseSpeed = 18f;
            drone.fireRate = 1.1f;
            drone.sightRadius = 16f;
            drone.attackRadius = 10f;
            drone.visualScale = 1.0f;
            drone.sprite = LoadArtSprite("Ships/Ship_Drone.png");
            drone = CreateOrUpdateAsset(drone, $"{dataPath}/Enemy_Drone.asset");

            // 9. Ships
            var scout = ScriptableObject.CreateInstance<ShipDefinition>();
            scout.id = "scout";
            scout.shipName = "Delta Scout";
            scout.tier = 1;
            scout.maxUpgradeLevel = 4;
            scout.weaponSlots = 1;
            scout.hitboxRadius = 0.55f;
            scout.weaponSockets = new Vector2[] { new Vector2(1.55f, 0f) };
            scout.thrusterSockets = new Vector2[] { new Vector2(-1.55f, 0f) };
            scout.sprite = LoadArtSprite("Ships/Ship_Scout.png");
            scout.emissiveTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Sprites/Ships_Imported/Ship_Scout_HD_emissive.png");
            scout.baseStats = new StatValues { hullCapacity = 80, shieldCapacity = 100, shieldRegen = 8, energyCapacity = 80, energyRegen = 18, pulseDamage = 18, pulseSpeed = 600, speed = 260, agility = 4.5f };
            scout = CreateOrUpdateAsset(scout, $"{dataPath}/Ship_Scout.asset");

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            fighter.id = "fighter";
            fighter.shipName = "Ares Fighter";
            fighter.tier = 2;
            fighter.maxUpgradeLevel = 5;
            fighter.weaponSlots = 2;
            fighter.hitboxRadius = 0.55f;
            fighter.weaponSockets = new Vector2[] { new Vector2(0.35f, 1.25f), new Vector2(0.35f, -1.25f) };
            fighter.thrusterSockets = new Vector2[] { new Vector2(-1.5f, 0.5f), new Vector2(-1.5f, -0.5f) };
            fighter.sprite = LoadArtSprite("Ships/Ship_Fighter.png");
            fighter.baseStats = new StatValues { hullCapacity = 120, shieldCapacity = 150, shieldRegen = 12, energyCapacity = 120, energyRegen = 24, pulseDamage = 28, pulseSpeed = 680, speed = 310, agility = 5.0f };
            fighter = CreateOrUpdateAsset(fighter, $"{dataPath}/Ship_Fighter.asset");

            var miner = ScriptableObject.CreateInstance<ShipDefinition>();
            miner.id = "miner";
            miner.shipName = "Goliath Miner";
            miner.tier = 2;
            miner.maxUpgradeLevel = 5;
            miner.weaponSlots = 2;
            miner.hitboxRadius = 0.65f;
            miner.weaponSockets = new Vector2[] { new Vector2(1.25f, 0.85f), new Vector2(1.25f, -0.85f) };
            miner.thrusterSockets = new Vector2[] { new Vector2(-1.4f, 0.4f), new Vector2(-1.4f, -0.4f) };
            miner.sprite = LoadArtSprite("Ships/Ship_Miner.png");
            miner.baseStats = new StatValues { hullCapacity = 180, shieldCapacity = 220, shieldRegen = 16, energyCapacity = 150, energyRegen = 20, pulseDamage = 40, pulseSpeed = 520, speed = 210, agility = 3.5f };
            miner = CreateOrUpdateAsset(miner, $"{dataPath}/Ship_Miner.asset");

            var interceptor = ScriptableObject.CreateInstance<ShipDefinition>();
            interceptor.id = "interceptor";
            interceptor.shipName = "Viper Interceptor";
            interceptor.tier = 3;
            interceptor.maxUpgradeLevel = 6;
            interceptor.weaponSlots = 2;
            interceptor.hitboxRadius = 0.50f;
            interceptor.weaponSockets = new Vector2[] { new Vector2(0.45f, 1.15f), new Vector2(0.45f, -1.15f) };
            interceptor.thrusterSockets = new Vector2[] { new Vector2(-1.5f, 0f) };
            interceptor.sprite = LoadArtSprite("Ships/Ship_Interceptor.png");
            interceptor.baseStats = new StatValues { hullCapacity = 100, shieldCapacity = 180, shieldRegen = 15, energyCapacity = 140, energyRegen = 30, pulseDamage = 35, pulseSpeed = 800, speed = 400, agility = 6.5f };
            interceptor = CreateOrUpdateAsset(interceptor, $"{dataPath}/Ship_Interceptor.asset");

            var juggernaut = ScriptableObject.CreateInstance<ShipDefinition>();
            juggernaut.id = "juggernaut";
            juggernaut.shipName = "Titan Juggernaut";
            juggernaut.tier = 3;
            juggernaut.maxUpgradeLevel = 6;
            juggernaut.weaponSlots = 3;
            juggernaut.hitboxRadius = 0.85f;
            juggernaut.weaponSockets = new Vector2[] {
                new Vector2(2.75f, 0f),      // Central spinal cannon
                new Vector2(0.70f, 1.50f),   // Port sponson turret
                new Vector2(0.70f, -1.50f)   // Starboard sponson turret
            };
            juggernaut.thrusterSockets = new Vector2[] {
                new Vector2(-2.75f, 0.85f),
                new Vector2(-2.75f, 0.30f),
                new Vector2(-2.75f, -0.30f),
                new Vector2(-2.75f, -0.85f)
            };
            juggernaut.sprite = LoadArtSprite("Ships/Ship_Juggernaut.png");
            juggernaut.emissiveTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Sprites/Ships_Imported/Ship_Juggernaut_HD_emissive.png");
            juggernaut.shieldHitboxOffset = 0.30f;
            juggernaut.baseStats = new StatValues { hullCapacity = 300, shieldCapacity = 400, shieldRegen = 25, energyCapacity = 200, energyRegen = 25, pulseDamage = 75, pulseSpeed = 480, speed = 180, agility = 2.5f };
            juggernaut = CreateOrUpdateAsset(juggernaut, $"{dataPath}/Ship_Juggernaut.asset");

            var carrier = ScriptableObject.CreateInstance<ShipDefinition>();
            carrier.id = "vanguard";
            carrier.shipName = "Vanguard Carrier";
            carrier.tier = 3;
            carrier.maxUpgradeLevel = 6;
            carrier.weaponSlots = 3;
            carrier.hitboxRadius = 0.80f;
            carrier.shieldHitboxOffset = 0.30f;
            carrier.weaponSockets = new Vector2[] {
                new Vector2(2.50f, 0f),      // Central deck battery
                new Vector2(2.75f, 1.12f),   // Port prow battery
                new Vector2(2.75f, -1.12f)   // Starboard prow battery
            };
            carrier.thrusterSockets = new Vector2[] {
                new Vector2(-2.75f, 1.10f),
                new Vector2(-2.75f, -1.10f)
            };
            carrier.sprite = LoadArtSprite("Ships/Ship_Carrier.png");
            carrier.emissiveTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Sprites/Ships_Imported/Ship_Carrier_HD_emissive.png");
            carrier.baseStats = new StatValues { hullCapacity = 240, shieldCapacity = 320, shieldRegen = 22, energyCapacity = 280, energyRegen = 32, pulseDamage = 55, pulseSpeed = 580, speed = 220, agility = 3.8f };
            carrier = CreateOrUpdateAsset(carrier, $"{dataPath}/Ship_Carrier.asset");

            var phantom = ScriptableObject.CreateInstance<ShipDefinition>();
            phantom.id = "phantom";
            phantom.shipName = "Shadow Phantom";
            phantom.tier = 3;
            phantom.maxUpgradeLevel = 6;
            phantom.weaponSlots = 2;
            phantom.hitboxRadius = 0.50f;
            phantom.weaponSockets = new Vector2[] { new Vector2(0.50f, 0.80f), new Vector2(0.50f, -0.80f) };
            phantom.thrusterSockets = new Vector2[] { new Vector2(-1.4f, 0f) };
            phantom.sprite = LoadArtSprite("Ships/Ship_Interceptor.png");
            phantom.baseStats = new StatValues { hullCapacity = 110, shieldCapacity = 160, shieldRegen = 16, energyCapacity = 160, energyRegen = 35, pulseDamage = 45, pulseSpeed = 850, speed = 420, agility = 7.0f };
            phantom = CreateOrUpdateAsset(phantom, $"{dataPath}/Ship_Phantom.asset");

            // Additional Naval Ship Classes
            EnsureShipAssets();
            var frigate = AssetDatabase.LoadAssetAtPath<ShipDefinition>($"{dataPath}/Ship_Frigate.asset");
            var destroyer = AssetDatabase.LoadAssetAtPath<ShipDefinition>($"{dataPath}/Ship_Destroyer.asset");
            var cruiser = AssetDatabase.LoadAssetAtPath<ShipDefinition>($"{dataPath}/Ship_Cruiser.asset");
            var battleship = AssetDatabase.LoadAssetAtPath<ShipDefinition>($"{dataPath}/Ship_Battleship.asset");

            scout.equipmentSlots = scout.GetEquipmentSlots();
            fighter.equipmentSlots = fighter.GetEquipmentSlots();
            miner.equipmentSlots = miner.GetEquipmentSlots();
            interceptor.equipmentSlots = interceptor.GetEquipmentSlots();
            juggernaut.equipmentSlots = juggernaut.GetEquipmentSlots();
            carrier.equipmentSlots = carrier.GetEquipmentSlots();
            phantom.equipmentSlots = phantom.GetEquipmentSlots();

            scout.evolvesTo = new ShipDefinition[] { fighter, frigate, miner };
            fighter.evolvesTo = new ShipDefinition[] { interceptor, destroyer };
            frigate.evolvesTo = new ShipDefinition[] { cruiser, phantom };
            miner.evolvesTo = new ShipDefinition[] { juggernaut, carrier };
            destroyer.evolvesTo = new ShipDefinition[] { battleship };
            cruiser.evolvesTo = new ShipDefinition[] { battleship, carrier };
            interceptor.evolvesTo = new ShipDefinition[0];
            juggernaut.evolvesTo = new ShipDefinition[0];
            phantom.evolvesTo = new ShipDefinition[0];
            carrier.evolvesTo = new ShipDefinition[0];
            battleship.evolvesTo = new ShipDefinition[0];

            EditorUtility.SetDirty(scout);
            EditorUtility.SetDirty(fighter);
            EditorUtility.SetDirty(miner);
            EditorUtility.SetDirty(interceptor);
            EditorUtility.SetDirty(juggernaut);
            EditorUtility.SetDirty(phantom);
            EditorUtility.SetDirty(carrier);
            EditorUtility.SetDirty(frigate);
            EditorUtility.SetDirty(destroyer);
            EditorUtility.SetDirty(cruiser);
            EditorUtility.SetDirty(battleship);

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

            // 10. Data-Driven Waves Configuration
            string wavesDir = $"{dataPath}/Waves";
            if (!AssetDatabase.IsValidFolder(wavesDir))
            {
                AssetDatabase.CreateFolder(dataPath, "Waves");
            }

            var dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/EnemyDrone.prefab");
            var bossDreadnoughtPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Enemies/BossDreadnought.prefab");

            var wave1 = ScriptableObject.CreateInstance<WaveDefinition>();
            wave1.waveNumber = 1;
            wave1.waveName = "Sector Recon";
            wave1.durationSeconds = 30f;
            wave1.targetAsteroidCount = 35;
            wave1.clearGemReward = 15;
            wave1.enemySpawns = new[]
            {
                new SpawnEntry { enemyId = "EnemyDrone", prefab = dronePrefab, spawnInterval = 4.5f, maxConcurrent = 5, totalToSpawn = -1 }
            };
            wave1 = CreateOrUpdateAsset(wave1, $"{wavesDir}/Wave_1.asset");

            var wave2 = ScriptableObject.CreateInstance<WaveDefinition>();
            wave2.waveNumber = 2;
            wave2.waveName = "Asteroid Incursion";
            wave2.durationSeconds = 45f;
            wave2.targetAsteroidCount = 50;
            wave2.clearGemReward = 30;
            wave2.enemySpawns = new[]
            {
                new SpawnEntry { enemyId = "EnemyDrone", prefab = dronePrefab, spawnInterval = 3.0f, maxConcurrent = 8, totalToSpawn = -1 }
            };
            wave2 = CreateOrUpdateAsset(wave2, $"{wavesDir}/Wave_2.asset");

            var wave3 = ScriptableObject.CreateInstance<WaveDefinition>();
            wave3.waveNumber = 3;
            wave3.waveName = "Dreadnought Assault";
            wave3.durationSeconds = 60f;
            wave3.targetAsteroidCount = 40;
            wave3.bossPrefab = bossDreadnoughtPrefab;
            wave3.clearGemReward = 100;
            wave3.enemySpawns = new[]
            {
                new SpawnEntry { enemyId = "EnemyDrone", prefab = dronePrefab, spawnInterval = 5.0f, maxConcurrent = 4, totalToSpawn = -1 }
            };
            wave3 = CreateOrUpdateAsset(wave3, $"{wavesDir}/Wave_3.asset");

            var waveConfig = ScriptableObject.CreateInstance<WaveConfig>();
            waveConfig.waves = new[] { wave1, wave2, wave3 };
            waveConfig.intermissionSeconds = 4f;
            waveConfig.loopWaves = true;
            waveConfig.difficultyMultiplierPerLoop = 1.25f;
            waveConfig = CreateOrUpdateAsset(waveConfig, $"{dataPath}/WaveConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("StarStrike pure ScriptableObject configuration assets and prefabs successfully generated!");
        }

        public static void EnsureShipAssets()
        {
            string dataPath = "Assets/_Project/Resources/Data";
            EnsureFolder("Assets/_Project", "Resources");
            EnsureFolder("Assets/_Project/Resources", "Data");

            var frigate = ShipArchetypeFactory.CreateArchetype(ShipClass.Frigate);
            frigate.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("frigate");
            CreateOrUpdateAsset(frigate, $"{dataPath}/Ship_Frigate.asset");

            var destroyer = ShipArchetypeFactory.CreateArchetype(ShipClass.Destroyer);
            destroyer.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("destroyer");
            CreateOrUpdateAsset(destroyer, $"{dataPath}/Ship_Destroyer.asset");

            var cruiser = ShipArchetypeFactory.CreateArchetype(ShipClass.Cruiser);
            cruiser.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("cruiser");
            cruiser.canAimIndependently = true;
            CreateOrUpdateAsset(cruiser, $"{dataPath}/Ship_Cruiser.asset");

            var battleship = ShipArchetypeFactory.CreateArchetype(ShipClass.Battleship);
            battleship.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("battleship");
            battleship.canAimIndependently = true;
            CreateOrUpdateAsset(battleship, $"{dataPath}/Ship_Battleship.asset");

            var carrier = ShipArchetypeFactory.CreateArchetype(ShipClass.Carrier);
            carrier.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("carrier");
            carrier.canAimIndependently = true;
            CreateOrUpdateAsset(carrier, $"{dataPath}/Ship_Carrier.asset");

            var fighter = ShipArchetypeFactory.CreateArchetype(ShipClass.Fighter);
            fighter.sprite = StarStrike.Gameplay.ProceduralSpriteHelper.GenerateShipSprite("fighter");
            CreateOrUpdateAsset(fighter, $"{dataPath}/Ship_Fighter.asset");

            AssetDatabase.SaveAssets();
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
