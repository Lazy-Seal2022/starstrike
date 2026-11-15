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

            // 6. Ensure EventSystem exists
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Object.DontDestroyOnLoad(eventSystemGO);
            }

            string activeScene = SceneManager.GetActiveScene().name;

            // 7. Scene-specific UI Setup
            if (activeScene == GameManager.SceneMainMenu)
            {
                SetupMainMenuSceneUI();
            }
            else
            {
                // In GameplayScene or legacy GameScene: setup gameplay systems & HUD
                SetupGameplaySceneUI();
            }

            Debug.Log($"GameBootstrapper: Core systems initialized for scene '{activeScene}'.");
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
                var menuGO = new GameObject("MainMenuUI");
                menuGO.transform.SetParent(canvas.transform, false);
                menuGO.AddComponent<MainMenuUI>();
            }

            // SettingsUI (Overlay)
            if (SettingsUI.Instance == null && Object.FindAnyObjectByType<SettingsUI>() == null)
            {
                var settingsGO = new GameObject("SettingsUI");
                settingsGO.transform.SetParent(canvas.transform, false);
                settingsGO.AddComponent<SettingsUI>();
            }

            // NetworkLobbyUI (Overlay)
            if (NetworkLobbyUI.Instance == null && Object.FindAnyObjectByType<NetworkLobbyUI>() == null)
            {
                var lobbyGO = new GameObject("NetworkLobbyUI");
                lobbyGO.transform.SetParent(canvas.transform, false);
                lobbyGO.AddComponent<NetworkLobbyUI>();
            }
        }

        private static void SetupGameplaySceneUI()
        {
            // Ensure WaveManager exists in gameplay
            if (Object.FindAnyObjectByType<WaveManager>() == null)
            {
                var waveGO = new GameObject("[WaveManager]");
                waveGO.AddComponent<WaveManager>();
                Object.DontDestroyOnLoad(waveGO);
            }

            Canvas canvas = EnsureCanvas();

            // CanvasHUD (Combat HUD)
            if (CanvasHUD.Instance == null && Object.FindAnyObjectByType<CanvasHUD>() == null)
            {
                var hudGO = new GameObject("CanvasHUD");
                hudGO.transform.SetParent(canvas.transform, false);
                hudGO.AddComponent<CanvasHUD>();
            }

            // SettingsUI (Pause Menu)
            if (SettingsUI.Instance == null && Object.FindAnyObjectByType<SettingsUI>() == null)
            {
                var settingsGO = new GameObject("SettingsUI");
                settingsGO.transform.SetParent(canvas.transform, false);
                settingsGO.AddComponent<SettingsUI>();
            }

            // NetworkScoreboardUI
            if (NetworkScoreboardUI.Instance == null && Object.FindAnyObjectByType<NetworkScoreboardUI>() == null)
            {
                var scoreboardGO = new GameObject("NetworkScoreboardUI");
                scoreboardGO.transform.SetParent(canvas.transform, false);
                scoreboardGO.AddComponent<NetworkScoreboardUI>();
            }
        }
    }
}
