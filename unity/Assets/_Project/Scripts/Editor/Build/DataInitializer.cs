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
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data"))
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Data");
            }

            // Create Upgrade Definition
            var upgradeDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            AssetDatabase.CreateAsset(upgradeDef, "Assets/_Project/Data/UpgradeDefinition.asset");

            // Create Game Config
            var gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.upgradeDefinition = upgradeDef;
            AssetDatabase.CreateAsset(gameConfig, "Assets/_Project/Data/GameConfig.asset");

            // Create Ships
            var scout = ScriptableObject.CreateInstance<ShipDefinition>();
            scout.id = "scout";
            scout.shipName = "Delta Scout";
            scout.tier = 1;
            scout.maxUpgradeLevel = 4;
            scout.weaponSlots = 1;
            scout.baseStats = new StatValues { shieldCapacity = 100, shieldRegen = 8, energyCapacity = 80, energyRegen = 18, pulseDamage = 18, pulseSpeed = 600, speed = 260, agility = 4.5f };
            AssetDatabase.CreateAsset(scout, "Assets/_Project/Data/Ship_Scout.asset");

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            fighter.id = "fighter";
            fighter.shipName = "Ares Fighter";
            fighter.tier = 2;
            fighter.maxUpgradeLevel = 5;
            fighter.weaponSlots = 2;
            fighter.baseStats = new StatValues { shieldCapacity = 150, shieldRegen = 12, energyCapacity = 120, energyRegen = 24, pulseDamage = 28, pulseSpeed = 680, speed = 310, agility = 5.0f };
            AssetDatabase.CreateAsset(fighter, "Assets/_Project/Data/Ship_Fighter.asset");

            var miner = ScriptableObject.CreateInstance<ShipDefinition>();
            miner.id = "miner";
            miner.shipName = "Goliath Miner";
            miner.tier = 2;
            miner.maxUpgradeLevel = 5;
            miner.weaponSlots = 2;
            miner.baseStats = new StatValues { shieldCapacity = 220, shieldRegen = 16, energyCapacity = 150, energyRegen = 20, pulseDamage = 40, pulseSpeed = 520, speed = 210, agility = 3.5f };
            AssetDatabase.CreateAsset(miner, "Assets/_Project/Data/Ship_Miner.asset");

            // Links
            scout.evolvesTo = new ShipDefinition[] { fighter, miner };
            fighter.evolvesTo = new ShipDefinition[0];
            miner.evolvesTo = new ShipDefinition[0];

            EditorUtility.SetDirty(scout);
            EditorUtility.SetDirty(fighter);
            EditorUtility.SetDirty(miner);
            AssetDatabase.SaveAssets();

            Debug.Log("StarStrike Data Initialized!");
        }
    }
}
