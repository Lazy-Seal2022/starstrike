using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Collections;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Network;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class NetworkLobbyUI : MonoBehaviour, IScreenController
    {
        public static NetworkLobbyUI Instance { get; private set; }
        public static event Action<bool> OnLobbyVisibilityChanged;

        public string ScreenId => "NetworkLobby";
        public void Show() => SetVisible(true);
        public void Hide() => SetVisible(false);

        private GameObject lobbyContainer;
        private TMP_InputField nameInput;
        private TMP_InputField ipInput;
        private TMP_InputField portInput;
        private TMP_Text statusText;
        private string selectedShipId = "scout";

        public bool IsVisible => lobbyContainer != null && lobbyContainer.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUI();
            SetVisible(false);
        }

        private void OnEnable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.RegisterScreen(this);
            }
        }

        private void OnDisable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.UnregisterScreen(this);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void EnsureUIBuilt()
        {
            if (Instance == null) Instance = this;
            if (lobbyContainer == null)
            {
                BuildUI();
            }
        }

        public void SetVisible(bool visible)
        {
            EnsureUIBuilt();
            if (lobbyContainer != null)
            {
                lobbyContainer.SetActive(visible);
                if (visible)
                {
                    selectedShipId = StorageService.GetLastShipEvolved();
                    if (string.IsNullOrEmpty(selectedShipId)) selectedShipId = "scout";
                    if (statusText != null) statusText.text = "Ready to engage.";
                }
            }

            // Direct strongly-typed UI state synchronization
            if (MainMenuUI.Instance != null)
            {
                MainMenuUI.Instance.SetMenuVisible(!visible);
            }
            if (visible && CanvasHUD.Instance != null)
            {
                CanvasHUD.Instance.SetHUDVisible(false);
            }

            OnLobbyVisibilityChanged?.Invoke(visible);
        }

        private void BuildUI()
        {
            lobbyContainer = new GameObject("NetworkLobbyContainer", typeof(RectTransform));
            lobbyContainer.transform.SetParent(transform, false);
            RectTransform rt = lobbyContainer.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Sorting order for Lobby (above MainMenu & HUD)
            Canvas lobbyCanvas = lobbyContainer.GetComponent<Canvas>();
            if (lobbyCanvas == null) lobbyCanvas = lobbyContainer.AddComponent<Canvas>();
            lobbyCanvas.overrideSorting = true;
            lobbyCanvas.sortingOrder = 300;
            if (lobbyContainer.GetComponent<GraphicRaycaster>() == null)
            {
                lobbyContainer.AddComponent<GraphicRaycaster>();
            }

            // 100% Solid Opaque Background (Completely covers the previous screen)
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(lobbyContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.03f, 0.05f, 0.09f, 1.0f);
            bgImg.raycastTarget = true;

            // Title
            UIHelper.CreateTextMeshPro(lobbyContainer.transform, "TitleText", "MULTIPLAYER DOGFIGHT", 44, FontStyles.Bold, new Color(1f, 0.35f, 0.2f), TextAlignmentOptions.Center, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(-300, -35), new Vector2(300, 35));

            // Subtitle
            UIHelper.CreateTextMeshPro(lobbyContainer.transform, "SubtitleText", "ONLINE & LAN FREE-FOR-ALL COMBAT", 18, FontStyles.Normal, Color.cyan, TextAlignmentOptions.Center, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(-250, -20), new Vector2(250, 20));

            // Pilot Name Label & Input
            UIHelper.CreateTextMeshPro(lobbyContainer.transform, "NameLabel", "PILOT CALL-SIGN:", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, new Vector2(0.35f, 0.68f), new Vector2(0.35f, 0.68f), new Vector2(-150, -20), new Vector2(0, 20));
            nameInput = CreateInputField(lobbyContainer.transform, "NameInput", "AcePilot", new Vector2(0.37f, 0.68f), new Vector2(0.65f, 0.68f), new Vector2(0, -20), new Vector2(0, 20));

            // Room Code / Server IP Label & Input
            UIHelper.CreateTextMeshPro(lobbyContainer.transform, "IpLabel", "ROOM CODE / IP:", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, new Vector2(0.35f, 0.58f), new Vector2(0.35f, 0.58f), new Vector2(-150, -20), new Vector2(0, 20));
            ipInput = CreateInputField(lobbyContainer.transform, "IpInput", "STAR-1001", new Vector2(0.37f, 0.58f), new Vector2(0.65f, 0.58f), new Vector2(0, -20), new Vector2(0, 20));

            // Server Port Label & Input
            UIHelper.CreateTextMeshPro(lobbyContainer.transform, "PortLabel", "PORT:", 16, FontStyles.Bold, Color.white, TextAlignmentOptions.Right, new Vector2(0.35f, 0.48f), new Vector2(0.35f, 0.48f), new Vector2(-150, -20), new Vector2(0, 20));
            portInput = CreateInputField(lobbyContainer.transform, "PortInput", "7777", new Vector2(0.37f, 0.48f), new Vector2(0.65f, 0.48f), new Vector2(0, -20), new Vector2(0, 20));

            // Status Text
            statusText = UIHelper.CreateTextMeshPro(lobbyContainer.transform, "StatusText", "Ready to engage.", 16, FontStyles.Italic, Color.yellow, TextAlignmentOptions.Center, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(-250, -20), new Vector2(250, 20));

            // Buttons: Host, Join, Back
            GameObject hostBtnObj = CreateButton(lobbyContainer.transform, "HostBtn", "HOST MATCH", new Vector2(0.32f, 0.25f), new Vector2(0.32f, 0.25f), Vector2.zero, new Vector2(200, 55), new Color(0.85f, 0.3f, 0.15f), 20);
            hostBtnObj.GetComponent<Button>().onClick.AddListener(OnHostClicked);

            GameObject joinBtnObj = CreateButton(lobbyContainer.transform, "JoinBtn", "JOIN MATCH", new Vector2(0.52f, 0.25f), new Vector2(0.52f, 0.25f), Vector2.zero, new Vector2(200, 55), new Color(0.18f, 0.65f, 0.35f), 20);
            joinBtnObj.GetComponent<Button>().onClick.AddListener(OnJoinClicked);

            GameObject backBtnObj = CreateButton(lobbyContainer.transform, "BackBtn", "BACK TO MENU", new Vector2(0.72f, 0.25f), new Vector2(0.72f, 0.25f), Vector2.zero, new Vector2(200, 55), new Color(0.3f, 0.35f, 0.45f), 18);
            backBtnObj.GetComponent<Button>().onClick.AddListener(OnBackClicked);
        }

        private void OnHostClicked()
        {
            string pilotName = string.IsNullOrEmpty(nameInput.text) ? "HostPilot" : nameInput.text;
            ushort port = 7777;
            if (ushort.TryParse(portInput.text, out ushort parsedPort)) port = parsedPort;

            EnsureNetworkInfrastructure();

            bool success = NetworkSessionManager.Instance.CreateSession(pilotName, 8, port);
            if (success)
            {
                var session = NetworkSessionManager.Instance.CurrentSession;
                if (statusText != null)
                {
                    statusText.text = $"<color=#00e5ff>HOSTING ROOM: [{session.JoinCode}]</color> on port {port}...";
                }
                OnSessionStarted(pilotName);
            }
            else
            {
                if (statusText != null) statusText.text = "Failed to start host.";
            }
        }

        private void OnJoinClicked()
        {
            string pilotName = string.IsNullOrEmpty(nameInput.text) ? "ClientPilot" : nameInput.text;
            string target = string.IsNullOrEmpty(ipInput.text) ? "STAR-1001" : ipInput.text.Trim();

            EnsureNetworkInfrastructure();

            bool success = NetworkSessionManager.Instance.JoinSessionByCode(target, pilotName);
            if (success)
            {
                if (statusText != null)
                {
                    statusText.text = $"<color=#4ade80>CONNECTING TO [{target}]...</color>";
                }
                OnSessionStarted(pilotName);
            }
            else
            {
                if (statusText != null) statusText.text = $"Failed to connect to room '{target}'.";
            }
        }

        private void OnSessionStarted(string pilotName)
        {
            var pvePlayer = GameObject.FindGameObjectWithTag("Player");
            if (pvePlayer != null && pvePlayer.GetComponent<NetworkPlayerShip>() == null)
            {
                pvePlayer.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.PvP);
                GameManager.Instance.ChangeState(GameState.Playing);
            }

            SetVisible(false);

            if (NetworkScoreboardUI.Instance != null)
            {
                NetworkScoreboardUI.Instance.SetVisible(true);
            }

            // Sync pilot info after connection
            StartCoroutine(SubmitPilotWhenConnected(pilotName));
        }

        private System.Collections.IEnumerator SubmitPilotWhenConnected(string pilotName)
        {
            while (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsConnectedClient)
            {
                yield return null;
            }

            if (NetworkMatchManager.Instance != null && NetworkManager.Singleton != null)
            {
                ulong localId = NetworkManager.Singleton.LocalClientId;
                NetworkMatchManager.Instance.SubmitPilotProfileServerRpc(
                    localId,
                    new FixedString32Bytes(pilotName),
                    new FixedString32Bytes(selectedShipId));
            }
        }

        private void OnBackClicked()
        {
            SetVisible(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameMode(GameMode.PvE);
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }

        private void EnsureNetworkInfrastructure()
        {
            if (NetworkSessionManager.Instance != null)
            {
                NetworkSessionManager.Instance.EnsureInfrastructure();
            }
        }

        private TMP_InputField CreateInputField(Transform parent, string name, string defaultText, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject inputObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObj.transform.SetParent(parent, false);
            RectTransform rt = inputObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = inputObj.GetComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

            TMP_InputField ifield = inputObj.GetComponent<TMP_InputField>();

            GameObject textViewport = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textViewport.transform.SetParent(inputObj.transform, false);
            RectTransform tvRt = textViewport.GetComponent<RectTransform>();
            tvRt.anchorMin = Vector2.zero;
            tvRt.anchorMax = Vector2.one;
            tvRt.offsetMin = new Vector2(10, 0);
            tvRt.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI txt = UIHelper.CreateTextMeshPro(textViewport.transform, "Text", defaultText, 16, FontStyles.Normal, Color.white, TextAlignmentOptions.Left, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            ifield.textViewport = tvRt;
            ifield.textComponent = txt;
            ifield.text = defaultText;

            return ifield;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, int fontSize = 16)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.2f;
            cb.pressedColor = bgColor * 0.8f;
            btn.colors = cb;

            UIHelper.CreateTextMeshPro(btnObj.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return btnObj;
        }
    }
}

namespace StarStrike.Network
{
    [System.Obsolete("NetworkLobbyUI has moved to StarStrike.UI namespace")]
    public class NetworkLobbyUI : StarStrike.UI.NetworkLobbyUI { }
}
