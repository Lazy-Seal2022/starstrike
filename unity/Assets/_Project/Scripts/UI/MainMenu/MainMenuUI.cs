using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance { get; private set; }

        private GameObject menuContainer;
        private Text gemsText;
        private Transform shipListContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildUI();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= HandleGameStarted;
        }

        private void HandleGameStarted()
        {
            SetMenuVisible(false);
        }

        private void Start()
        {
            // Initial state check
            if (GameManager.Instance != null)
            {
                menuContainer.SetActive(GameManager.Instance.CurrentState == GameState.Menu);
            }
        }

        public void SetMenuVisible(bool visible)
        {
            if (menuContainer != null)
            {
                menuContainer.SetActive(visible);
                if (visible)
                {
                    RefreshData();
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && menuContainer != null)
            {
                bool shouldShow = GameManager.Instance.CurrentState == GameState.Menu;
                if (menuContainer.activeSelf != shouldShow)
                {
                    SetMenuVisible(shouldShow);
                }

                if (shouldShow && Keyboard.current != null)
                {
                    if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        SetMenuVisible(false);
                        GameManager.Instance.StartGame();
                    }
                }
            }
        }

        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            menuContainer = new GameObject("MainMenuContainer", typeof(RectTransform));
            menuContainer.transform.SetParent(transform, false);
            RectTransform rt = menuContainer.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(menuContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            bgObj.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.12f, 0.70f);

            // Title
            CreateText(menuContainer.transform, "TitleText", "STARSTRIKE", 64, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.8f), new Vector2(0.5f, 0.8f), new Vector2(-300, -50), new Vector2(300, 50), defaultFont);

            // Gems
            GameObject gemsObj = CreateText(menuContainer.transform, "GemsText", "Meta-Gems: 0", 24, FontStyle.Bold, Color.cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), new Vector2(-200, -30), new Vector2(200, 30), defaultFont);
            gemsText = gemsObj.GetComponent<Text>();

            // Start Button
            GameObject startBtnObj = CreateButton(menuContainer.transform, "StartButton", "START MISSION", new Vector2(0.5f, 0.16f), new Vector2(0.5f, 0.16f), new Vector2(-150, -35), new Vector2(300, 70), new Color(0.18f, 0.75f, 0.35f), defaultFont);
            startBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                SetMenuVisible(false);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
            });

            // Start Key Hint
            CreateText(menuContainer.transform, "StartHintText", "[ Press ENTER or SPACE to Launch ]", 14, FontStyle.Normal, new Color(0.8f, 0.8f, 0.9f, 0.85f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.10f), new Vector2(-200, -15), new Vector2(200, 15), defaultFont);

            // Ship Hangar
            GameObject shipHeader = CreateText(menuContainer.transform, "ShipHeader", "SELECT HANGAR SHIP", 20, FontStyle.Normal, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.55f), new Vector2(0.5f, 0.55f), new Vector2(-200, -25), new Vector2(200, 25), defaultFont);

            GameObject shipListObj = new GameObject("ShipListContainer", typeof(RectTransform));
            shipListObj.transform.SetParent(menuContainer.transform, false);
            RectTransform slRt = shipListObj.GetComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.5f, 0.35f);
            slRt.anchorMax = new Vector2(0.5f, 0.35f);
            slRt.anchoredPosition = Vector2.zero;
            slRt.sizeDelta = new Vector2(600, 100);
            shipListContainer = shipListObj.transform;
            
            // We'll populate ships in RefreshData
        }

        private void RefreshData()
        {
            if (gemsText != null)
            {
                gemsText.text = $"Meta-Gems: {StorageService.Profile.totalMetaGems}";
            }

            // Clear old children
            foreach (Transform child in shipListContainer)
            {
                Destroy(child.gameObject);
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            // For this demo, let's just hardcode the known ships or load from resources.
            // Let's assume we can load them if they were in Resources, but since they are in Data, we can't easily load them dynamically without a reference.
            // As a quick workaround, we'll just show the IDs.
            string[] availableShips = new string[] { "scout", "fighter", "miner", "interceptor", "juggernaut" };
            int[] costs = new int[] { 0, 500, 500, 1500, 2000 };

            float spacing = 120f;
            float startX = -((availableShips.Length - 1) * spacing) / 2f;

            for (int i = 0; i < availableShips.Length; i++)
            {
                string shipId = availableShips[i];
                int cost = costs[i];
                bool isUnlocked = StorageService.Profile.unlockedShips.Contains(shipId);
                bool isSelected = StorageService.GetLastShipEvolved() == shipId;

                string btnText = isUnlocked ? (isSelected ? $"[ {shipId.ToUpper()} ]" : shipId.ToUpper()) : $"{shipId.ToUpper()}\n{cost} G";
                Color btnColor = isUnlocked ? (isSelected ? Color.green : Color.gray) : new Color(0.8f, 0.2f, 0.2f);

                GameObject btnObj = CreateButton(shipListContainer, $"Btn_{shipId}", btnText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * spacing), 0), new Vector2(110, 80), btnColor, defaultFont);
                
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => {
                    if (isUnlocked)
                    {
                        StorageService.SaveLastShipEvolved(shipId);
                        RefreshData();
                    }
                    else if (StorageService.Profile.totalMetaGems >= cost)
                    {
                        StorageService.Profile.totalMetaGems -= cost;
                        StorageService.UnlockShip(shipId);
                        StorageService.SaveLastShipEvolved(shipId);
                        RefreshData();
                    }
                });
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

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.2f;
            cb.pressedColor = bgColor * 0.8f;
            btn.colors = cb;

            CreateText(btnObj.transform, "Text", label, 14, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, font);
            return btnObj;
        }
    }
}
