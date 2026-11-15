using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace StarStrike.Editor
{
    public static class AddressablesSetup
    {
        private const string ConfigGroupName = "StarStrike_Config";
        private const string PrefabsGroupName = "StarStrike_Prefabs";
        private const string AudioGroupName = "StarStrike_Audio";

        [MenuItem("StarStrike/Addressables/Configure Addressables Pipeline")]
        public static void ConfigureAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null)
            {
                Debug.LogError("[AddressablesSetup] Failed to get or create AddressableAssetSettings.");
                return;
            }

            settings.AddLabel("config", true);
            settings.AddLabel("prefab", true);
            settings.AddLabel("audio", true);

            var configGroup = GetOrCreateGroup(settings, ConfigGroupName);
            var prefabsGroup = GetOrCreateGroup(settings, PrefabsGroupName);
            var audioGroup = GetOrCreateGroup(settings, AudioGroupName);

            int configuredCount = 0;

            // 1. Configs
            string[] configAssets = new[]
            {
                "Assets/_Project/Resources/Data/GameConfig.asset",
                "Assets/_Project/Resources/Data/AudioConfig.asset",
                "Assets/_Project/Resources/Data/CombatConfig.asset",
                "Assets/_Project/Resources/Data/FlightConfig.asset",
                "Assets/_Project/Resources/Data/AsteroidConfig.asset",
                "Assets/_Project/Resources/Data/BossConfig.asset",
                "Assets/_Project/Resources/Data/LootConfig.asset",
                "Assets/_Project/Resources/Data/UIThemeConfig.asset",
                "Assets/_Project/Resources/Data/WaveConfig.asset",
                "Assets/_Project/Resources/Data/UpgradeDefinition.asset",
                "Assets/_Project/Resources/Data/Ship_Scout.asset",
                "Assets/_Project/Resources/Data/Ship_Fighter.asset",
                "Assets/_Project/Resources/Data/Ship_Interceptor.asset",
                "Assets/_Project/Resources/Data/Ship_Miner.asset",
                "Assets/_Project/Resources/Data/Ship_Juggernaut.asset",
                "Assets/_Project/Resources/Data/Enemy_Drone.asset",
                "Assets/_Project/Resources/Data/Waves/Wave_1.asset",
                "Assets/_Project/Resources/Data/Waves/Wave_2.asset",
                "Assets/_Project/Resources/Data/Waves/Wave_3.asset"
            };

            foreach (var path in configAssets)
            {
                string address = PathToAddress(path, "Assets/_Project/Resources/");
                if (AddOrUpdateEntry(settings, configGroup, path, address, "config"))
                {
                    configuredCount++;
                }
            }

            // 2. Prefabs
            string[] prefabAssets = new[]
            {
                "Assets/_Project/Prefabs/Enemies/BossDreadnought.prefab",
                "Assets/_Project/Prefabs/Enemies/EnemyDrone.prefab",
                "Assets/_Project/Prefabs/Environment/Asteroid_Large.prefab",
                "Assets/_Project/Prefabs/Environment/Asteroid_Medium.prefab",
                "Assets/_Project/Prefabs/Environment/Asteroid_Small.prefab",
                "Assets/_Project/Prefabs/Environment/GemCrystal.prefab",
                "Assets/_Project/Prefabs/Environment/PowerUpItem.prefab",
                "Assets/_Project/Prefabs/Projectiles/PlayerLaser.prefab",
                "Assets/_Project/Prefabs/Projectiles/EnemyLaser.prefab",
                "Assets/_Project/Prefabs/UI/CanvasHUD.prefab",
                "Assets/_Project/Prefabs/Ships/Ship_Scout.prefab",
                "Assets/_Project/Prefabs/Ships/Ship_Fighter.prefab",
                "Assets/_Project/Prefabs/Ships/Ship_Interceptor.prefab",
                "Assets/_Project/Prefabs/Ships/Ship_Miner.prefab",
                "Assets/_Project/Prefabs/Ships/Ship_Juggernaut.prefab"
            };

            foreach (var path in prefabAssets)
            {
                string address = PathToAddress(path, "Assets/_Project/");
                if (AddOrUpdateEntry(settings, prefabsGroup, path, address, "prefab"))
                {
                    configuredCount++;
                }
            }

            // 3. Audio Clips
            string[] audioAssets = new[]
            {
                "Assets/_Project/Audio/Music/ambient_music.ogg",
                "Assets/_Project/Audio/SFX/World/gem.ogg",
                "Assets/_Project/Audio/SFX/Combat/laser.ogg",
                "Assets/_Project/Audio/SFX/Combat/explosion.ogg",
                "Assets/_Project/Audio/SFX/Combat/enemy_laser.ogg",
                "Assets/_Project/Audio/SFX/UI/click.ogg",
                "Assets/_Project/Audio/SFX/UI/upgrade.ogg"
            };

            foreach (var path in audioAssets)
            {
                string filename = Path.GetFileNameWithoutExtension(path);
                string address = $"Audio/{filename}";
                if (AddOrUpdateEntry(settings, audioGroup, path, address, "audio"))
                {
                    configuredCount++;
                }
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"[AddressablesSetup] Addressables configured successfully: {configuredCount} entries configured across 3 groups.");
        }

        private static AddressableAssetGroup GetOrCreateGroup(AddressableAssetSettings settings, string groupName)
        {
            var group = settings.FindGroup(groupName);
            if (group != null) return group;

            group = settings.CreateGroup(
                groupName,
                false,
                false,
                false,
                new List<AddressableAssetGroupSchema>(),
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema)
            );

            var bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
            if (bundledSchema != null)
            {
                bundledSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            }

            return group;
        }

        private static bool AddOrUpdateEntry(AddressableAssetSettings settings, AddressableAssetGroup group, string assetPath, string address, string label)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"[AddressablesSetup] Asset not found at path '{assetPath}'");
                return false;
            }

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = address;
            entry.SetLabel(label, true, true, false);
            return true;
        }

        private static string PathToAddress(string fullPath, string prefixToRemove)
        {
            string normalized = fullPath.Replace('\\', '/');
            if (normalized.StartsWith(prefixToRemove))
            {
                normalized = normalized.Substring(prefixToRemove.Length);
            }
            int extIdx = normalized.LastIndexOf('.');
            if (extIdx >= 0)
            {
                normalized = normalized.Substring(0, extIdx);
            }
            return normalized;
        }
    }
}
