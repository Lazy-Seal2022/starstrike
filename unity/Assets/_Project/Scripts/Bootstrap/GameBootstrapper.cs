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

            // 6. Ensure EventSystem exists and persists
            var existingEventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (existingEventSystem == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Object.DontDestroyOnLoad(eventSystemGO);
            }
            else if (existingEventSystem.transform.parent == null)
            {
                Object.DontDestroyOnLoad(existingEventSystem.gameObject);
            }

            string activeScene = SceneManager.GetActiveScene().name;
            OnSceneLoaded(activeScene);

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;

            Debug.Log($"GameBootstrapper: Core systems initialized for scene '{activeScene}'.");
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            OnSceneLoaded(scene.name);
        }

        public static void OnSceneLoaded(string activeScene)
        {
            // Ensure EventSystem exists on scene transition
            var es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Object.DontDestroyOnLoad(eventSystemGO);
            }
            else if (es.transform.parent == null)
            {
                Object.DontDestroyOnLoad(es.gameObject);
            }

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

        private static void SetupMainMenuSceneUI()
        {
            Canvas canvas = EnsureCanvas();

            // MainMenuUI
            if (MainMenuUI.Instance == null && Object.FindAnyObjectByType<MainMenuUI>() == null)
            {
                var menuGO = new GameObject("MainMenuUI", typeof(RectTransform));
                menuGO.transform.SetParent(canvas.transform, false);
                var rt = menuGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                menuGO.AddComponent<MainMenuUI>();
            }

            // SettingsUI (Overlay)
            if (SettingsUI.Instance == null && Object.FindAnyObjectByType<SettingsUI>() == null)
            {
                var settingsGO = new GameObject("SettingsUI", typeof(RectTransform));
                settingsGO.transform.SetParent(canvas.transform, false);
                var rt = settingsGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                settingsGO.AddComponent<SettingsUI>();
            }

            // NetworkLobbyUI (Overlay)
            if (NetworkLobbyUI.Instance == null && Object.FindAnyObjectByType<NetworkLobbyUI>() == null)
            {
                var lobbyGO = new GameObject("NetworkLobbyUI", typeof(RectTransform));
                lobbyGO.transform.SetParent(canvas.transform, false);
                var rt = lobbyGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                lobbyGO.AddComponent<NetworkLobbyUI>();
            }
        }

        private static void SetupShipSelectSceneUI()
        {
            Canvas canvas = EnsureCanvas();

            if (ShipSelectUI.Instance == null && Object.FindAnyObjectByType<ShipSelectUI>() == null)
            {
                var shipSelectGO = new GameObject("ShipSelectUI", typeof(RectTransform));
                shipSelectGO.transform.SetParent(canvas.transform, false);
                var rt = shipSelectGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                shipSelectGO.AddComponent<ShipSelectUI>();
            }

            if (SettingsUI.Instance == null && Object.FindAnyObjectByType<SettingsUI>() == null)
            {
                var settingsGO = new GameObject("SettingsUI", typeof(RectTransform));
                settingsGO.transform.SetParent(canvas.transform, false);
                var rt = settingsGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                settingsGO.AddComponent<SettingsUI>();
            }
        }

        private static void SetupEquipmentSelectSceneUI()
        {
            Canvas canvas = EnsureCanvas();

            if (EquipmentSelectUI.Instance == null && Object.FindAnyObjectByType<EquipmentSelectUI>() == null)
            {
                var equipSelectGO = new GameObject("EquipmentSelectUI", typeof(RectTransform));
                equipSelectGO.transform.SetParent(canvas.transform, false);
                var rt = equipSelectGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                equipSelectGO.AddComponent<EquipmentSelectUI>();
            }

            if (SettingsUI.Instance == null && Object.FindAnyObjectByType<SettingsUI>() == null)
            {
                var settingsGO = new GameObject("SettingsUI", typeof(RectTransform));
                settingsGO.transform.SetParent(canvas.transform, false);
                var rt = settingsGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                settingsGO.AddComponent<SettingsUI>();
            }
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

            // CanvasHUD (Combat HUD)
            if (CanvasHUD.Instance == null && Object.FindAnyObjectByType<CanvasHUD>() == null)
            {
                var hudGO = new GameObject("CanvasHUD", typeof(RectTransform));
                hudGO.transform.SetParent(canvas.transform, false);
                var rt = hudGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                hudGO.AddComponent<CanvasHUD>();
            }

            // SettingsUI (Pause Menu)
            if (SettingsUI.Instance == null && Object.FindAnyObjectByType<SettingsUI>() == null)
            {
                var settingsGO = new GameObject("SettingsUI", typeof(RectTransform));
                settingsGO.transform.SetParent(canvas.transform, false);
                var rt = settingsGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                settingsGO.AddComponent<SettingsUI>();
            }

            // NetworkScoreboardUI
            if (NetworkScoreboardUI.Instance == null && Object.FindAnyObjectByType<NetworkScoreboardUI>() == null)
            {
                var scoreboardGO = new GameObject("NetworkScoreboardUI", typeof(RectTransform));
                scoreboardGO.transform.SetParent(canvas.transform, false);
                var rt = scoreboardGO.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                scoreboardGO.AddComponent<NetworkScoreboardUI>();
            }
        }
    }
}
