using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using StarStrike.UI;

namespace StarStrike.Editor
{
    public static class SceneSplitterUtility
    {
        public const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenuScene.unity";
        public const string GameplayScenePath = "Assets/_Project/Scenes/GameplayScene.unity";
        public const string LegacyGameScenePath = "Assets/_Project/Scenes/GameScene.unity";

        [MenuItem("StarStrike/Setup Split Scenes (MainMenu & Gameplay)")]
        public static void SetupSplitScenes()
        {
            Debug.Log("--- STARTING SCENE SPLIT & BUILD SETTINGS SETUP ---");

            // 1. Build MainMenuScene
            BuildMainMenuScene();

            // 2. Build GameplayScene from GameScene
            BuildGameplayScene();

            // 3. Update Player Colliders in scenes
            UpdateScenePlayerColliders();

            // 4. Configure EditorBuildSettings
            ConfigureBuildSettings();

            // 5. Leave MainMenuScene open as default active scene in editor
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== SCENE SPLIT SETUP COMPLETE: MainMenuScene (Index 0) & GameplayScene (Index 1) ===");
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Main Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.05f, 0.09f, 1f);
            cam.depth = -1;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();

            // 2. EventSystem
            var eventGo = new GameObject("EventSystem");
            eventGo.AddComponent<EventSystem>();
            eventGo.AddComponent<InputSystemUIInputModule>();

            // 4. GameCanvas
            var canvasGo = new GameObject("GameCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 5. ScreenManager
            var smGo = new GameObject("ScreenManager");
            smGo.transform.SetParent(canvasGo.transform, false);
            smGo.AddComponent<ScreenManager>();

            // 6. MainMenuUI
            var menuGo = new GameObject("MainMenuUI");
            menuGo.transform.SetParent(canvasGo.transform, false);
            menuGo.AddComponent<MainMenuUI>();

            // 7. SettingsUI
            var settingsGo = new GameObject("SettingsUI");
            settingsGo.transform.SetParent(canvasGo.transform, false);
            settingsGo.AddComponent<SettingsUI>();

            // 8. NetworkLobbyUI
            var lobbyGo = new GameObject("NetworkLobbyUI");
            lobbyGo.transform.SetParent(canvasGo.transform, false);
            lobbyGo.AddComponent<NetworkLobbyUI>();

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            Debug.Log($"[PASS] Created MainMenuScene at '{MainMenuScenePath}'");
        }

        private static void BuildGameplayScene()
        {
            if (!File.Exists(LegacyGameScenePath))
            {
                Debug.LogWarning($"Legacy scene '{LegacyGameScenePath}' not found, skipping GameplayScene copy.");
                return;
            }

            // Open legacy GameScene
            var scene = EditorSceneManager.OpenScene(LegacyGameScenePath, OpenSceneMode.Single);

            // Clean up any rogue MainMenuContainer under HUDManager if present
            var hudManager = Object.FindAnyObjectByType<HUDManager>();
            if (hudManager != null)
            {
                var rogueMenu = hudManager.transform.Find("MainMenuContainer");
                if (rogueMenu != null)
                {
                    Object.DestroyImmediate(rogueMenu.gameObject);
                }
            }

            // Save as GameplayScene
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
            Debug.Log($"[PASS] Created GameplayScene at '{GameplayScenePath}'");
        }

        public static void ConfigureBuildSettings()
        {
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };

            EditorBuildSettings.scenes = buildScenes;
            Debug.Log($"[PASS] Configured EditorBuildSettings with {buildScenes.Length} scenes.");
        }

        public static void UpdateScenePlayerColliders()
        {
            UpdateScenePlayer(LegacyGameScenePath);
            UpdateScenePlayer(GameplayScenePath);
        }

        private static void UpdateScenePlayer(string scenePath)
        {
            if (!File.Exists(scenePath)) return;
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject player = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.CompareTag("Player") || root.name == "Player")
                {
                    player = root;
                    break;
                }
            }
            if (player != null)
            {
                var circle = player.GetComponent<CircleCollider2D>();
                if (circle != null)
                {
                    Object.DestroyImmediate(circle);
                }
                var poly = player.GetComponent<PolygonCollider2D>();
                if (poly == null)
                {
                    poly = player.AddComponent<PolygonCollider2D>();
                }
                var sr = player.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = player.AddComponent<SpriteRenderer>();
                }
                if (sr.sprite == null)
                {
                    sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Sprites/Ships/Ship_Scout.png");
                }
                if (sr.sprite != null)
                {
                    StarStrike.Gameplay.PlayerProgression.UpdatePolygonFromSprite(poly, sr.sprite);
                }
                var shield = player.GetComponent<StarStrike.Gameplay.FX.ShipShieldField>();
                if (shield == null)
                {
                    shield = player.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
                }
                shield.EnsureInitialized();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[PASS] Updated Player with PolygonCollider2D in '{scenePath}'");
            }
        }
    }
}
