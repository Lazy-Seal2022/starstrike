using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;
using StarStrike.Gameplay.FX;
using StarStrike.UI;

namespace StarStrike.Bootstrap
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            // 0. Ensure PoolManager exists (Cross-scene singleton)
            if (PoolManager.Instance == null)
            {
                var poolGO = new GameObject("[PoolManager]");
                var pm = poolGO.AddComponent<PoolManager>();
                pm.Initialize();
                Object.DontDestroyOnLoad(poolGO);
            }

            // 1. Ensure AudioService exists
            if (AudioService.Instance == null)
            {
                var audioGO = new GameObject("[AudioService]");
                audioGO.AddComponent<AudioService>();
                Object.DontDestroyOnLoad(audioGO);
            }

            // 2. Ensure FXService exists
            if (FXService.Instance == null)
            {
                var fxGO = new GameObject("[FXService]");
                fxGO.AddComponent<FXService>();
                Object.DontDestroyOnLoad(fxGO);
            }

            // 3. Ensure AchievementManager exists
            if (Object.FindAnyObjectByType<AchievementManager>() == null)
            {
                var achGO = new GameObject("[AchievementManager]");
                achGO.AddComponent<AchievementManager>();
                Object.DontDestroyOnLoad(achGO);
            }

            // 4. Ensure GameManager exists
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                Object.DontDestroyOnLoad(gm);
            }

            // 5. Ensure ScreenManager exists
            if (ScreenManager.Instance == null && Object.FindAnyObjectByType<ScreenManager>() == null)
            {
                var smGO = new GameObject("[ScreenManager]");
                smGO.AddComponent<ScreenManager>();
                Object.DontDestroyOnLoad(smGO);
            }

            // 6. Ensure exactly one EventSystem exists and persists
            EventSystemSingleton.EnforceSingleEventSystem();

            string activeScene = SceneManager.GetActiveScene().name;
            OnSceneLoaded(activeScene);

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;

            Debug.Log($"GameBootstrapper: Core systems initialized for scene '{activeScene}'.");
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EventSystemSingleton.EnforceSingleEventSystem();
            OnSceneLoaded(scene.name);
        }

        public static void OnSceneLoaded(string activeScene)
        {
            // Ensure exactly one EventSystem exists on scene transition
            EventSystemSingleton.EnforceSingleEventSystem();

            // 7. Scene-specific UI Setup
            if (activeScene == GameManager.SceneMainMenu)
            {
                SetupMainMenuSceneUI();
            }
            else if (activeScene == GameManager.SceneShipSelect)
            {
                SetupShipSelectSceneUI();
            }
            else if (activeScene == GameManager.SceneEquipmentSelect)
            {
                SetupEquipmentSelectSceneUI();
            }
            else
            {
                // In GameplayScene or legacy GameScene: setup gameplay systems & HUD
                SetupGameplaySceneUI();
            }
        }

        private static Canvas EnsureCanvas()
        {
            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("GameCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            return canvas;
        }

        private static void EnsureFullScreenUI<T>(Canvas canvas, string name, bool existingSingletonNull) where T : Component
        {
            if (!existingSingletonNull || Object.FindAnyObjectByType<T>() != null) return;

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<T>();
        }

        private static void SetupMainMenuSceneUI()
        {
            Canvas canvas = EnsureCanvas();
            EnsureFullScreenUI<MainMenuUI>(canvas, "MainMenuUI", MainMenuUI.Instance == null);
            EnsureFullScreenUI<SettingsUI>(canvas, "SettingsUI", SettingsUI.Instance == null);
            EnsureFullScreenUI<NetworkLobbyUI>(canvas, "NetworkLobbyUI", NetworkLobbyUI.Instance == null);
        }

        private static void SetupShipSelectSceneUI()
        {
            Canvas canvas = EnsureCanvas();
            EnsureFullScreenUI<ShipSelectUI>(canvas, "ShipSelectUI", ShipSelectUI.Instance == null);
            EnsureFullScreenUI<SettingsUI>(canvas, "SettingsUI", SettingsUI.Instance == null);
        }

        private static void SetupEquipmentSelectSceneUI()
        {
            Canvas canvas = EnsureCanvas();
            EnsureFullScreenUI<EquipmentSelectUI>(canvas, "EquipmentSelectUI", EquipmentSelectUI.Instance == null);
            EnsureFullScreenUI<SettingsUI>(canvas, "SettingsUI", SettingsUI.Instance == null);
        }

        private static void SetupGameplaySceneUI()
        {
            // Ensure WaveManager exists in gameplay
            if (Object.FindAnyObjectByType<WaveManager>() == null)
            {
                var waveGO = new GameObject("[WaveManager]");
                waveGO.AddComponent<WaveManager>();
                waveGO.AddComponent<SectorHazardController>();
                Object.DontDestroyOnLoad(waveGO);
            }
            else if (Object.FindAnyObjectByType<SectorHazardController>() == null)
            {
                var hazardGO = new GameObject("[SectorHazardController]");
                hazardGO.AddComponent<SectorHazardController>();
                Object.DontDestroyOnLoad(hazardGO);
            }

            Canvas canvas = EnsureCanvas();
            EnsureFullScreenUI<CanvasHUD>(canvas, "CanvasHUD", CanvasHUD.Instance == null);
            EnsureFullScreenUI<SettingsUI>(canvas, "SettingsUI", SettingsUI.Instance == null);
            EnsureFullScreenUI<NetworkScoreboardUI>(canvas, "NetworkScoreboardUI", NetworkScoreboardUI.Instance == null);
        }
    }
}
