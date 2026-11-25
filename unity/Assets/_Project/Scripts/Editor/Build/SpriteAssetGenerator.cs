using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;
using StarStrike.Gameplay;

namespace StarStrike.Editor
{
    public static class SpriteAssetGenerator
    {
        private const string ArtDir = "Assets/_Project/Art/Sprites";
        private const string AtlasPath = "Assets/_Project/Art/Sprites/StarStrikeMasterAtlas.spriteatlasv2";

        [MenuItem("StarStrike/Generate Sprite Assets & Master Atlas")]
        public static void GenerateAllSpritesAndAtlas()
        {
            EnsureDirectory(ArtDir);

            var spritePairs = new Dictionary<string, Sprite>
            {
                { "Ships/Ship_Scout", ProceduralSpriteHelper.GetShipSprite("scout", forceProcedural: true) },
                { "Ships/Ship_Fighter", ProceduralSpriteHelper.GetShipSprite("fighter", forceProcedural: true) },
                { "Ships/Ship_Miner", ProceduralSpriteHelper.GetShipSprite("miner", forceProcedural: true) },
                { "Ships/Ship_Drone", ProceduralSpriteHelper.GetShipSprite("drone", forceProcedural: true) },
                { "Ships/Ship_Interceptor", ProceduralSpriteHelper.GetShipSprite("interceptor", forceProcedural: true) },
                { "Ships/Ship_Juggernaut", ProceduralSpriteHelper.GetShipSprite("juggernaut", forceProcedural: true) },
                { "Ships/Ship_Boss", ProceduralSpriteHelper.GetShipSprite("boss", forceProcedural: true) },
                { "Ships/Ship_Frigate", ProceduralSpriteHelper.GetShipSprite("frigate", forceProcedural: true) },
                { "Ships/Ship_Destroyer", ProceduralSpriteHelper.GetShipSprite("destroyer", forceProcedural: true) },
                { "Ships/Ship_Cruiser", ProceduralSpriteHelper.GetShipSprite("cruiser", forceProcedural: true) },
                { "Ships/Ship_Carrier", ProceduralSpriteHelper.GetShipSprite("carrier", forceProcedural: true) },
                { "Ships/Ship_Battleship", ProceduralSpriteHelper.GetShipSprite("battleship", forceProcedural: true) },
                { "Ships/Ship_Phantom", ProceduralSpriteHelper.GetShipSprite("phantom", forceProcedural: true) },
                { "Projectiles/Tracer_Gatling", ProceduralSpriteHelper.GetGatlingTracerSprite(forceProcedural: true) },
                { "Projectiles/Laser_Player", ProceduralSpriteHelper.GetLaserSprite(false, forceProcedural: true) },
                { "Projectiles/Laser_Enemy", ProceduralSpriteHelper.GetLaserSprite(true, forceProcedural: true) },
                { "World/Asteroid", ProceduralSpriteHelper.GetAsteroidSprite(forceProcedural: true) },
                { "World/Gem", ProceduralSpriteHelper.GetGemSprite(forceProcedural: true) },
                { "World/PowerUp", ProceduralSpriteHelper.GetPowerUpSprite(forceProcedural: true) },
                { "World/Star", ProceduralSpriteHelper.GetStarSprite(forceProcedural: true) },
                { "UI/Radar", ProceduralSpriteHelper.GetRadarSprite(128, forceProcedural: true) },
                { "UI/Circle_16", ProceduralSpriteHelper.GetCircleSprite(16, forceProcedural: true) }
            };

            var loadedSpriteAssets = new List<UnityEngine.Object>();

            foreach (var kvp in spritePairs)
            {
                string relPath = kvp.Key;
                Sprite sprite = kvp.Value;
                if (sprite == null || sprite.texture == null) continue;

                byte[] pngBytes = sprite.texture.EncodeToPNG();
                if (pngBytes == null || pngBytes.Length == 0) continue;

                // Save to Art/Sprites as single source of truth for assets & master atlas
                string artPath = Path.Combine(ArtDir, relPath + ".png");
                EnsureDirectory(Path.GetDirectoryName(artPath));
                File.WriteAllBytes(artPath, pngBytes);
            }

            AssetDatabase.Refresh();

            // Configure texture importers
            foreach (var kvp in spritePairs)
            {
                string relPath = kvp.Key;
                string artPath = $"{ArtDir}/{relPath}.png";

                ConfigureImporter(artPath);

                Sprite loadedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
                if (loadedSprite != null)
                {
                    loadedSpriteAssets.Add(loadedSprite);
                }
            }

            // Create Master Sprite Atlas V2
            if (loadedSpriteAssets.Count > 0)
            {
                CreateOrUpdateMasterAtlas(AtlasPath, loadedSpriteAssets.ToArray());
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[SpriteAssetGenerator] Successfully generated {loadedSpriteAssets.Count} sprites and compiled master atlas at {AtlasPath}");
        }

        [MenuItem("StarStrike/Refresh Master Atlas (All Art Sprites)")]
        public static void RefreshMasterAtlas()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.SpriteAtlasV2;

            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { ArtDir });
            var sprites = new List<UnityEngine.Object>(spriteGuids.Length);
            foreach (string guid in spriteGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath) || assetPath.Contains("Ships_Imported")) continue;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            if (sprites.Count > 0)
            {
                CreateOrUpdateMasterAtlas(AtlasPath, sprites.ToArray());
                AssetDatabase.SaveAssets();
                Debug.Log($"[SpriteAssetGenerator] Refreshed master atlas at {AtlasPath} with {sprites.Count} sprites.");
            }
        }

        private static void ConfigureImporter(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.isReadable = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static void CreateOrUpdateMasterAtlas(string path, UnityEngine.Object[] sprites)
        {
            SpriteAtlasAsset atlasAsset = new SpriteAtlasAsset();
            atlasAsset.Add(sprites);
            SpriteAtlasAsset.Save(atlasAsset, path);
            AssetDatabase.ImportAsset(path);

            SpriteAtlasImporter importer = AssetImporter.GetAtPath(path) as SpriteAtlasImporter;
            if (importer != null)
            {
                var packingSettings = importer.packingSettings;
                packingSettings.padding = 4;
                packingSettings.enableAlphaDilation = true;
                importer.packingSettings = packingSettings;

                var textureSettings = importer.textureSettings;
                textureSettings.filterMode = FilterMode.Bilinear;
                textureSettings.generateMipMaps = false;
                importer.textureSettings = textureSettings;

                importer.includeInBuild = true;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
