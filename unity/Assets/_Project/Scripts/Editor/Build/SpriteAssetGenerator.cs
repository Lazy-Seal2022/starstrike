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
        private const string ResourcesDir = "Assets/_Project/Resources/Sprites";
        private const string AtlasPath = "Assets/_Project/Art/Sprites/StarStrikeMasterAtlas.spriteatlasv2";

        [MenuItem("StarStrike/Generate Sprite Assets & Master Atlas")]
        public static void GenerateAllSpritesAndAtlas()
        {
            EnsureDirectory(ArtDir);
            EnsureDirectory(ResourcesDir);

            var spritePairs = new Dictionary<string, Sprite>
            {
                { "Ship_Scout", ProceduralSpriteHelper.GetShipSprite("scout") },
                { "Ship_Fighter", ProceduralSpriteHelper.GetShipSprite("fighter") },
                { "Ship_Miner", ProceduralSpriteHelper.GetShipSprite("miner") },
                { "Ship_Drone", ProceduralSpriteHelper.GetShipSprite("drone") },
                { "Ship_Interceptor", ProceduralSpriteHelper.GetShipSprite("interceptor") },
                { "Ship_Juggernaut", ProceduralSpriteHelper.GetShipSprite("juggernaut") },
                { "Ship_Boss", ProceduralSpriteHelper.GetShipSprite("boss") },
                { "Laser_Player", ProceduralSpriteHelper.GetLaserSprite(false) },
                { "Laser_Enemy", ProceduralSpriteHelper.GetLaserSprite(true) },
                { "Asteroid", ProceduralSpriteHelper.GetAsteroidSprite() },
                { "Gem", ProceduralSpriteHelper.GetGemSprite() },
                { "PowerUp", ProceduralSpriteHelper.GetPowerUpSprite() },
                { "Radar", ProceduralSpriteHelper.GetRadarSprite(128) },
                { "Circle_16", ProceduralSpriteHelper.GetCircleSprite(16) },
                { "Star", ProceduralSpriteHelper.GetStarSprite() }
            };

            var loadedSpriteAssets = new List<UnityEngine.Object>();

            foreach (var kvp in spritePairs)
            {
                string name = kvp.Key;
                Sprite sprite = kvp.Value;
                if (sprite == null || sprite.texture == null) continue;

                byte[] pngBytes = sprite.texture.EncodeToPNG();
                if (pngBytes == null || pngBytes.Length == 0) continue;

                // Save to Resources/Sprites for instant runtime loading
                string resPath = Path.Combine(ResourcesDir, name + ".png");
                File.WriteAllBytes(resPath, pngBytes);

                // Save to Art/Sprites for the master atlas source
                string artPath = Path.Combine(ArtDir, name + ".png");
                File.WriteAllBytes(artPath, pngBytes);
            }

            AssetDatabase.Refresh();

            // Configure texture importers
            foreach (var kvp in spritePairs)
            {
                string name = kvp.Key;
                string artPath = $"{ArtDir}/{name}.png";
                string resPath = $"{ResourcesDir}/{name}.png";

                ConfigureImporter(artPath);
                ConfigureImporter(resPath);

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
