using StarStrike.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using StarStrike.Gameplay;

namespace StarStrike.Bootstrap
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            // 1. Create GameManager
            if (GameManager.Instance == null)
            {
                var gm = new GameObject("GameManager");
                gm.AddComponent<GameManager>();
                // GameManager.Awake will DontDestroyOnLoad
            }

            // 2. Create EventSystem if not exists
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                Object.DontDestroyOnLoad(eventSystemGO);
            }

            // 3. Create Canvas
            var canvasGO = new GameObject("GameCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
            Object.DontDestroyOnLoad(canvasGO);

            // 4. Create MainMenuUI
            var mainMenuGO = new GameObject("MainMenuUI");
            mainMenuGO.transform.SetParent(canvasGO.transform, false);
            mainMenuGO.AddComponent<MainMenuUI>();

            // 5. Create CanvasHUD
            var hudGO = new GameObject("CanvasHUD");
            hudGO.transform.SetParent(canvasGO.transform, false);
            hudGO.AddComponent<CanvasHUD>();

            // 6. Create NetworkLobbyUI
            var lobbyGO = new GameObject("NetworkLobbyUI");
            lobbyGO.transform.SetParent(canvasGO.transform, false);
            lobbyGO.AddComponent<StarStrike.Network.NetworkLobbyUI>();

            // 7. Create NetworkScoreboardUI
            var scoreboardGO = new GameObject("NetworkScoreboardUI");
            scoreboardGO.transform.SetParent(canvasGO.transform, false);
            scoreboardGO.AddComponent<StarStrike.Network.NetworkScoreboardUI>();

            Debug.Log("GameBootstrapper: Core systems instantiated.");
        }
    }
}
