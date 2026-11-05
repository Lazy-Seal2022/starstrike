using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Collections;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;

namespace StarStrike.Network
{
    public class NetworkLobbyUI : MonoBehaviour
    {
        public static NetworkLobbyUI Instance { get; private set; }

        private GameObject lobbyContainer;
        private InputField nameInput;
        private InputField ipInput;
        private InputField portInput;
        private Text statusText;
        private string selectedShipId = "scout";

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

        public void SetVisible(bool visible)
        {
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
        }

        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            lobbyContainer = new GameObject("NetworkLobbyContainer", typeof(RectTransform));
            lobbyContainer.transform.SetParent(transform, false);
            RectTransform rt = lobbyContainer.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Semi-transparent background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(lobbyContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bgObj.GetComponent<Image>().color = new Color(0.03f, 0.05f, 0.10f, 0.90f);

            // Title
            CreateText(lobbyContainer.transform, "TitleText", "MULTIPLAYER DOGFIGHT", 44, FontStyle.Bold, new Color(1f, 0.35f, 0.2f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), new Vector2(-300, -35), new Vector2(300, 35), defaultFont);

            // Subtitle
            CreateText(lobbyContainer.transform, "SubtitleText", "ONLINE & LAN FREE-FOR-ALL COMBAT", 18, FontStyle.Normal, Color.cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f), new Vector2(-250, -20), new Vector2(250, 20), defaultFont);

            // Pilot Name Label & Input
            CreateText(lobbyContainer.transform, "NameLabel", "PILOT CALL-SIGN:", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleRight, new Vector2(0.35f, 0.68f), new Vector2(0.35f, 0.68f), new Vector2(-150, -20), new Vector2(0, 20), defaultFont);
            nameInput = CreateInputField(lobbyContainer.transform, "NameInput", "AcePilot", new Vector2(0.37f, 0.68f), new Vector2(0.65f, 0.68f), new Vector2(0, -20), new Vector2(0, 20), defaultFont);

            // Server IP Label & Input
            CreateText(lobbyContainer.transform, "IpLabel", "SERVER IP / HOST:", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleRight, new Vector2(0.35f, 0.58f), new Vector2(0.35f, 0.58f), new Vector2(-150, -20), new Vector2(0, 20), defaultFont);
            ipInput = CreateInputField(lobbyContainer.transform, "IpInput", "127.0.0.1", new Vector2(0.37f, 0.58f), new Vector2(0.65f, 0.58f), new Vector2(0, -20), new Vector2(0, 20), defaultFont);

            // Server Port Label & Input
            CreateText(lobbyContainer.transform, "PortLabel", "PORT:", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleRight, new Vector2(0.35f, 0.48f), new Vector2(0.35f, 0.48f), new Vector2(-150, -20), new Vector2(0, 20), defaultFont);
            portInput = CreateInputField(lobbyContainer.transform, "PortInput", "7777", new Vector2(0.37f, 0.48f), new Vector2(0.65f, 0.48f), new Vector2(0, -20), new Vector2(0, 20), defaultFont);

            // Status Text
            GameObject stObj = CreateText(lobbyContainer.transform, "StatusText", "Ready to engage.", 16, FontStyle.Italic, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(-250, -20), new Vector2(250, 20), defaultFont);
            statusText = stObj.GetComponent<Text>();

            // Buttons: Host, Join, Back
            GameObject hostBtnObj = CreateButton(lobbyContainer.transform, "HostBtn", "HOST MATCH", new Vector2(0.32f, 0.25f), new Vector2(0.32f, 0.25f), Vector2.zero, new Vector2(200, 55), new Color(0.85f, 0.3f, 0.15f), defaultFont, 20);
            hostBtnObj.GetComponent<Button>().onClick.AddListener(OnHostClicked);

            GameObject joinBtnObj = CreateButton(lobbyContainer.transform, "JoinBtn", "JOIN MATCH", new Vector2(0.52f, 0.25f), new Vector2(0.52f, 0.25f), Vector2.zero, new Vector2(200, 55), new Color(0.18f, 0.65f, 0.35f), defaultFont, 20);
            joinBtnObj.GetComponent<Button>().onClick.AddListener(OnJoinClicked);

            GameObject backBtnObj = CreateButton(lobbyContainer.transform, "BackBtn", "BACK TO MENU", new Vector2(0.72f, 0.25f), new Vector2(0.72f, 0.25f), Vector2.zero, new Vector2(200, 55), new Color(0.3f, 0.35f, 0.45f), defaultFont, 18);
            backBtnObj.GetComponent<Button>().onClick.AddListener(OnBackClicked);
        }

        private void OnHostClicked()
        {
            string pilotName = string.IsNullOrEmpty(nameInput.text) ? "HostPilot" : nameInput.text;
            ushort port = 7777;
            if (ushort.TryParse(portInput.text, out ushort parsedPort)) port = parsedPort;

            EnsureNetworkInfrastructure();

            if (NetworkManager.Singleton != null)
            {
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport != null)
                {
                    transport.SetConnectionData("127.0.0.1", port);
                }

                bool success = NetworkManager.Singleton.StartHost();
                if (success)
                {
                    statusText.text = $"Hosting match on port {port}...";
                    OnSessionStarted(pilotName);
                }
                else
                {
                    statusText.text = "Failed to start host.";
                }
            }
        }

        private void OnJoinClicked()
        {
            string pilotName = string.IsNullOrEmpty(nameInput.text) ? "ClientPilot" : nameInput.text;
            string ip = string.IsNullOrEmpty(ipInput.text) ? "127.0.0.1" : ipInput.text.Trim();
            ushort port = 7777;
            if (ushort.TryParse(portInput.text, out ushort parsedPort)) port = parsedPort;

            EnsureNetworkInfrastructure();

            if (NetworkManager.Singleton != null)
            {
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (transport != null)
                {
                    transport.SetConnectionData(ip, port);
                }

                bool success = NetworkManager.Singleton.StartClient();
                if (success)
                {
                    statusText.text = $"Connecting to {ip}:{port}...";
                    OnSessionStarted(pilotName);
                }
                else
                {
                    statusText.text = "Failed to start client.";
                }
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
            if (NetworkManager.Singleton == null)
            {
                var netMgrObj = new GameObject("NetworkManager", typeof(NetworkManager), typeof(UnityTransport));
                var netMgr = netMgrObj.GetComponent<NetworkManager>();
                var transport = netMgrObj.GetComponent<UnityTransport>();
                netMgr.NetworkConfig = new NetworkConfig();
                netMgr.NetworkConfig.NetworkTransport = transport;
            }

            var singleton = NetworkManager.Singleton;
            if (singleton != null && singleton.NetworkConfig != null && singleton.NetworkConfig.PlayerPrefab == null)
            {
                var shipPrefab = Resources.Load<GameObject>("Prefabs/Network/NetworkPlayerShip");
                var laserPrefab = Resources.Load<GameObject>("Prefabs/Network/NetworkLaserProjectile");
                if (shipPrefab != null)
                {
                    singleton.NetworkConfig.PlayerPrefab = shipPrefab;
                    singleton.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = shipPrefab });
                }
                if (laserPrefab != null)
                {
                    singleton.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = laserPrefab });
                }
            }

            if (NetworkMatchManager.Instance == null)
            {
                var matchMgrObj = new GameObject("NetworkMatchManager", typeof(NetworkMatchManager), typeof(NetworkObject));
                var netObj = matchMgrObj.GetComponent<NetworkObject>();
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer && !netObj.IsSpawned)
                {
                    netObj.Spawn();
                }
            }
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Text txt = obj.GetComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.font = font;
            return obj;
        }

        private InputField CreateInputField(Transform parent, string name, string defaultText, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font)
        {
            GameObject inputObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
            inputObj.transform.SetParent(parent, false);
            RectTransform rt = inputObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = inputObj.GetComponent<Image>();
            img.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);

            InputField ifield = inputObj.GetComponent<InputField>();

            GameObject textObj = CreateText(inputObj.transform, "Text", defaultText, 16, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0), font);
            Text txt = textObj.GetComponent<Text>();
            ifield.textComponent = txt;
            ifield.text = defaultText;

            return ifield;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 16)
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

            CreateText(btnObj.transform, "Text", label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, font);
            return btnObj;
        }
    }
}
