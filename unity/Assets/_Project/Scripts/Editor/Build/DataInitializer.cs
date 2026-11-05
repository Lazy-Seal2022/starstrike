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
            EnsureFolder("Assets/_Project", "Data");

            string dataPath = "Assets/_Project/Resources/Data";

            // 1. Upgrade Definition
            var upgradeDef = CreateOrUpdateAsset(ScriptableObject.CreateInstance<UpgradeDefinition>(), $"{dataPath}/UpgradeDefinition.asset");

            // 2. Flight Config
            var flightConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<FlightConfig>(), $"{dataPath}/FlightConfig.asset");

            // 3. Combat Config
            var combatConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<CombatConfig>(), $"{dataPath}/CombatConfig.asset");

            // 4. Asteroid Config
            var asteroidConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<AsteroidConfig>(), $"{dataPath}/AsteroidConfig.asset");

            // 5. Boss Config
            var bossConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<BossConfig>(), $"{dataPath}/BossConfig.asset");

            // 6. Loot Config
            var lootConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<LootConfig>(), $"{dataPath}/LootConfig.asset");

            // 7. UI Theme Config
            var uiThemeConfig = CreateOrUpdateAsset(ScriptableObject.CreateInstance<UIThemeConfig>(), $"{dataPath}/UIThemeConfig.asset");

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
            drone = CreateOrUpdateAsset(drone, $"{dataPath}/Enemy_Drone.asset");

            // 9. Ships
            var scout = ScriptableObject.CreateInstance<ShipDefinition>();
            scout.id = "scout";
            scout.shipName = "Delta Scout";
            scout.tier = 1;
            scout.maxUpgradeLevel = 4;
            scout.weaponSlots = 1;
            scout.baseStats = new StatValues { shieldCapacity = 100, shieldRegen = 8, energyCapacity = 80, energyRegen = 18, pulseDamage = 18, pulseSpeed = 600, speed = 260, agility = 4.5f };
            scout = CreateOrUpdateAsset(scout, $"{dataPath}/Ship_Scout.asset");

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            fighter.id = "fighter";
            fighter.shipName = "Ares Fighter";
            fighter.tier = 2;
            fighter.maxUpgradeLevel = 5;
            fighter.weaponSlots = 2;
            fighter.baseStats = new StatValues { shieldCapacity = 150, shieldRegen = 12, energyCapacity = 120, energyRegen = 24, pulseDamage = 28, pulseSpeed = 680, speed = 310, agility = 5.0f };
            fighter = CreateOrUpdateAsset(fighter, $"{dataPath}/Ship_Fighter.asset");

            var miner = ScriptableObject.CreateInstance<ShipDefinition>();
            miner.id = "miner";
            miner.shipName = "Goliath Miner";
            miner.tier = 2;
            miner.maxUpgradeLevel = 5;
            miner.weaponSlots = 2;
            miner.baseStats = new StatValues { shieldCapacity = 220, shieldRegen = 16, energyCapacity = 150, energyRegen = 20, pulseDamage = 40, pulseSpeed = 520, speed = 210, agility = 3.5f };
            miner = CreateOrUpdateAsset(miner, $"{dataPath}/Ship_Miner.asset");

            var interceptor = ScriptableObject.CreateInstance<ShipDefinition>();
            interceptor.id = "interceptor";
            interceptor.shipName = "Viper Interceptor";
            interceptor.tier = 3;
            interceptor.maxUpgradeLevel = 6;
            interceptor.weaponSlots = 2;
            interceptor.baseStats = new StatValues { shieldCapacity = 180, shieldRegen = 15, energyCapacity = 140, energyRegen = 30, pulseDamage = 35, pulseSpeed = 800, speed = 400, agility = 6.5f };
            interceptor = CreateOrUpdateAsset(interceptor, $"{dataPath}/Ship_Interceptor.asset");

            var juggernaut = ScriptableObject.CreateInstance<ShipDefinition>();
            juggernaut.id = "juggernaut";
            juggernaut.shipName = "Titan Juggernaut";
            juggernaut.tier = 3;
            juggernaut.maxUpgradeLevel = 6;
            juggernaut.weaponSlots = 3;
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
            gameConfig.upgradeDefinition = upgradeDef;
            gameConfig.defaultShip = scout;
            CreateOrUpdateAsset(gameConfig, $"{dataPath}/GameConfig.asset");

            // 11. Generate Prefabs
            PrefabInitializer.GeneratePrefabs();

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
    }
}
