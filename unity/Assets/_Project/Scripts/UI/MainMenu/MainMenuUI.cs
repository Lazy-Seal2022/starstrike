using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class MainMenuUI : MonoBehaviour, IScreenController
    {
        public static MainMenuUI Instance { get; private set; }

        public string ScreenId => "MainMenu";

        private GameObject menuContainer;
        private TMP_Text gemsText;
        private Image shipPreviewIcon;
        private TMP_Text deploymentShipText;
        private TMP_Text deploymentWeaponText;
        private TMP_Text deploymentSkillText;

        public bool IsVisible => menuContainer != null && menuContainer.activeSelf;

        public void Show() => SetMenuVisible(true);
        public void Hide() => SetMenuVisible(false);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            RectTransform selfRt = GetComponent<RectTransform>();
            if (selfRt == null) selfRt = gameObject.AddComponent<RectTransform>();
            selfRt.anchorMin = Vector2.zero;
            selfRt.anchorMax = Vector2.one;
            selfRt.offsetMin = Vector2.zero;
            selfRt.offsetMax = Vector2.zero;

            BuildUI();
        }

        private void OnEnable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.RegisterScreen(this);
            }
            GameEvents.OnGameStarted += HandleGameStarted;
            NetworkLobbyUI.OnLobbyVisibilityChanged += HandleLobbyVisibilityChanged;
            RefreshData();
        }

        private void OnDisable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.UnregisterScreen(this);
            }
            GameEvents.OnGameStarted -= HandleGameStarted;
            NetworkLobbyUI.OnLobbyVisibilityChanged -= HandleLobbyVisibilityChanged;
        }

        private void HandleGameStarted()
        {
            SetMenuVisible(false);
        }

        private void HandleLobbyVisibilityChanged(bool isVisible)
        {
            if (isVisible)
            {
                SetMenuVisible(false);
            }
            else if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Menu)
            {
                SetMenuVisible(true);
            }
        }

        public void SetMenuVisible(bool visible)
        {
            if (menuContainer != null)
            {
                menuContainer.SetActive(visible);
            }
            if (visible)
            {
                RefreshData();
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && menuContainer != null)
            {
                bool isOverlayOpen = (SettingsUI.Instance != null && SettingsUI.Instance.IsVisible) ||
                                     (NetworkLobbyUI.Instance != null && NetworkLobbyUI.Instance.IsVisible);

                bool shouldShow = GameManager.Instance.CurrentState == GameState.Menu && !isOverlayOpen;
                if (menuContainer.activeSelf != shouldShow)
                {
                    SetMenuVisible(shouldShow);
                }

                if (shouldShow && Keyboard.current != null)
                {
                    if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        SetMenuVisible(false);
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.LoadGameplay(GameMode.PvE);
                        }
                    }
                    else if (Keyboard.current.hKey.wasPressedThisFrame)
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.LoadShipSelect();
                        }
                    }
                    else if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.LoadEquipmentSelect();
                        }
                    }
                    else if (Keyboard.current.oKey.wasPressedThisFrame)
                    {
                        if (SettingsUI.Instance != null)
                        {
                            SettingsUI.Instance.Open();
                        }
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

            Canvas menuCanvas = menuContainer.GetComponent<Canvas>();
            if (menuCanvas == null) menuCanvas = menuContainer.AddComponent<Canvas>();
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 105;
            if (menuContainer.GetComponent<GraphicRaycaster>() == null)
            {
                menuContainer.AddComponent<GraphicRaycaster>();
            }

            // Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(menuContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.02f, 0.04f, 0.07f, 1.0f);
            bgImg.raycastTarget = true;

            // Title
            CreateText(menuContainer.transform, "TitleText", "STARSTRIKE", 60, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(-350, -40), new Vector2(350, 40), defaultFont);

            // Subtitle
            CreateText(menuContainer.transform, "SubtitleText", "DEEP SPACE SURVIVAL & WARFARE", 15, FontStyle.Normal, new Color(0.7f, 0.85f, 1f, 0.9f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), new Vector2(-250, -15), new Vector2(250, 15), defaultFont);

            // Gems
            GameObject gemsObj = CreateText(menuContainer.transform, "GemsText", "Meta-Gems: 0", 20, FontStyle.Bold, Color.cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), new Vector2(-200, -18), new Vector2(200, 18), defaultFont);
            gemsText = gemsObj.GetComponent<TMP_Text>();

            // Settings Button (Top Right)
            GameObject settingsBtnObj = CreateButton(menuContainer.transform, "SettingsButton", "SETTINGS [O]", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -40f), new Vector2(180f, 44f), new Color(0.12f, 0.22f, 0.35f), defaultFont, 16);
            settingsBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                if (ScreenManager.Instance != null && ScreenManager.Instance.PushModal("Settings"))
                {
                    // modal pushed
                }
                else if (SettingsUI.Instance != null)
                {
                    SettingsUI.Instance.Open();
                }
            });

            // DEPLOYMENT SUMMARY CARD (Center)
            GameObject summaryBox = new GameObject("DeploymentSummary", typeof(RectTransform), typeof(Image));
            summaryBox.transform.SetParent(menuContainer.transform, false);
            RectTransform sbRt = summaryBox.GetComponent<RectTransform>();
            sbRt.anchorMin = new Vector2(0.5f, 0.58f);
            sbRt.anchorMax = new Vector2(0.5f, 0.58f);
            sbRt.anchoredPosition = Vector2.zero;
            sbRt.sizeDelta = new Vector2(700, 150);
            var sbImg = summaryBox.GetComponent<Image>();
            sbImg.color = new Color(0.05f, 0.08f, 0.14f, 0.90f);

            CreateText(summaryBox.transform, "CardHeader", "FLEET DEPLOYMENT READINESS", 14, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.86f), new Vector2(0.5f, 0.86f), new Vector2(-250, -12), new Vector2(250, 12), defaultFont);

            // Ship Icon (Left of card)
            GameObject iconObj = new GameObject("ShipPreviewIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(summaryBox.transform, false);
            RectTransform icRt = iconObj.GetComponent<RectTransform>();
            icRt.anchorMin = new Vector2(0.04f, 0.12f);
            icRt.anchorMax = new Vector2(0.24f, 0.78f);
            icRt.offsetMin = Vector2.zero;
            icRt.offsetMax = Vector2.zero;
            shipPreviewIcon = iconObj.GetComponent<Image>();
            shipPreviewIcon.preserveAspect = true;

            // Details (Right of card)
            GameObject shipTxtObj = CreateText(summaryBox.transform, "ShipTxt", "• Selected Vessel: DELTA SCOUT", 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.28f, 0.68f), new Vector2(0.28f, 0.68f), new Vector2(0, -12), new Vector2(450, 12), defaultFont);
            deploymentShipText = shipTxtObj.GetComponent<TMP_Text>();

            GameObject wpnTxtObj = CreateText(summaryBox.transform, "WpnTxt", "• Primary Weapon:  PULSE LASER", 15, FontStyle.Bold, new Color(1.0f, 0.65f, 0.15f), TextAnchor.MiddleLeft, new Vector2(0.28f, 0.45f), new Vector2(0.28f, 0.45f), new Vector2(0, -12), new Vector2(450, 12), defaultFont);
            deploymentWeaponText = wpnTxtObj.GetComponent<TMP_Text>();

            GameObject sklTxtObj = CreateText(summaryBox.transform, "SklTxt", "• Tactical Skill:  KINETIC SHOCKWAVE", 15, FontStyle.Bold, Color.cyan, TextAnchor.MiddleLeft, new Vector2(0.28f, 0.22f), new Vector2(0.28f, 0.22f), new Vector2(0, -12), new Vector2(450, 12), defaultFont);
            deploymentSkillText = sklTxtObj.GetComponent<TMP_Text>();

            // MAIN NAVIGATION BUTTONS
            // 1. Play PvE Survival Button (Big Primary)
            GameObject pveBtnObj = CreateButton(menuContainer.transform, "PvEStartButton", "► LAUNCH PVE SURVIVAL [SPACE / ENTER]", new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f), new Vector2(0, 0), new Vector2(440, 52), new Color(0.15f, 0.70f, 0.35f), defaultFont, 18);
            pveBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                SetMenuVisible(false);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadGameplay(GameMode.PvE);
                }
            });

            // 2. Hangar (Choose Ship) Button
            GameObject hangarBtnObj = CreateButton(menuContainer.transform, "HangarButton", "HANGAR // CHOOSE SHIP [H]", new Vector2(0.5f, 0.27f), new Vector2(0.5f, 0.27f), new Vector2(-155, 0), new Vector2(290, 48), new Color(0.15f, 0.45f, 0.65f), defaultFont, 16);
            hangarBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadShipSelect();
                }
            });

            // 3. Armory (Choose Equipment) Button
            GameObject armoryBtnObj = CreateButton(menuContainer.transform, "ArmoryButton", "ARMORY // EQUIPMENT [E]", new Vector2(0.5f, 0.27f), new Vector2(0.5f, 0.27f), new Vector2(155, 0), new Vector2(290, 48), new Color(0.85f, 0.45f, 0.12f), defaultFont, 16);
            armoryBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadEquipmentSelect();
                }
            });

            // 4. Multiplayer PvP Button
            GameObject pvpBtnObj = CreateButton(menuContainer.transform, "PvPStartButton", "MULTIPLAYER PvP ARENA", new Vector2(0.5f, 0.17f), new Vector2(0.5f, 0.17f), new Vector2(0, 0), new Vector2(360, 46), new Color(0.85f, 0.30f, 0.18f), defaultFont, 16);
            pvpBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                SetMenuVisible(false);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetGameMode(GameMode.PvP);
                }
                if (ScreenManager.Instance != null && ScreenManager.Instance.PushModal("NetworkLobby"))
                {
                    // modal pushed
                }
                else if (NetworkLobbyUI.Instance != null)
                {
                    NetworkLobbyUI.Instance.SetVisible(true);
                }
            });

            // Start Key Hint
            CreateText(menuContainer.transform, "StartHintText", "[ ENTER: Launch PvE  |  H: Ship Hangar  |  E: Armory Equipment  |  O: Settings ]", 13, FontStyle.Normal, new Color(0.85f, 0.9f, 1f, 0.85f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(-350, -15), new Vector2(350, 15), defaultFont);
        }

        public void RefreshData()
        {
            if (gemsText != null)
            {
                gemsText.text = $"Meta-Gems: {StorageService.Profile.totalMetaGems}";
            }

            string activeShip = StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(activeShip)) activeShip = "scout";

            string activeWeapon = StorageService.GetEquippedWeapon(activeShip);
            string activeSkill = StorageService.GetSelectedSkill();

            if (deploymentShipText != null)
            {
                deploymentShipText.text = $"• Selected Vessel: {activeShip.ToUpper()}";
            }
            if (deploymentWeaponText != null)
            {
                deploymentWeaponText.text = $"• Primary Weapon:  {GetWeaponDisplayName(activeWeapon).ToUpper()}";
            }
            if (deploymentSkillText != null)
            {
                deploymentSkillText.text = $"• Tactical Skill:  {GetSkillDisplayName(activeSkill).ToUpper()}";
            }
            if (shipPreviewIcon != null)
            {
                shipPreviewIcon.sprite = ProceduralSpriteHelper.GetShipSprite(activeShip);
            }
        }

        private string GetWeaponDisplayName(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return "Pulse Laser";
            switch (weaponId.ToLowerInvariant().Trim())
            {
                case "scattergun": return "Scattergun";
                case "missile": return "Homing Missiles";
                case "beam": return "Continuous Beam Laser";
                case "gatling": return "Rotary Gatling Gun";
                default: return "Pulse Laser";
            }
        }

        private string GetSkillDisplayName(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return "Kinetic Shockwave";
            switch (skillId.ToLowerInvariant().Trim())
            {
                case "blink_teleport": return "Blink Warp Dash";
                case "emp_nova": return "EMP Nova Burst";
                default: return "Kinetic Shockwave";
            }
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font = null)
        {
            FontStyles style = (fontStyle == FontStyle.Bold) ? FontStyles.Bold : FontStyles.Normal;
            var tmp = UIHelper.CreateTextMeshPro(parent, name, text, fontSize, style, color, UIHelper.ConvertTextAnchor(alignment), anchorMin, anchorMax, offsetMin, offsetMax);
            return tmp.gameObject;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 16)
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
            btn.onClick.AddListener(() =>
            {
                StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            });
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
