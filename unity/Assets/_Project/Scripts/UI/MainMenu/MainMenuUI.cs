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
        private Transform shipListContainer;
        private Transform weaponListContainer;
        private Transform skillListContainer;

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

        private void HandleLobbyVisibilityChanged(bool isLobbyVisible)
        {
            if (isLobbyVisible)
            {
                SetMenuVisible(false);
            }
            else if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Menu)
            {
                SetMenuVisible(true);
            }
        }

        private void Start()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.RegisterScreen(this);
            }
            // Initial state check
            if (GameManager.Instance != null)
            {
                menuContainer.SetActive(GameManager.Instance.CurrentState == GameState.Menu);
            }
        }

        public void EnsureUIBuilt()
        {
            if (Instance == null) Instance = this;
            if (menuContainer == null)
            {
                BuildUI();
            }
        }

        public void SetMenuVisible(bool visible)
        {
            EnsureUIBuilt();
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

            // Sorting order for main menu
            Canvas menuCanvas = menuContainer.GetComponent<Canvas>();
            if (menuCanvas == null) menuCanvas = menuContainer.AddComponent<Canvas>();
            menuCanvas.overrideSorting = true;
            menuCanvas.sortingOrder = 105;
            if (menuContainer.GetComponent<GraphicRaycaster>() == null)
            {
                menuContainer.AddComponent<GraphicRaycaster>();
            }

            // Solid opaque background (100% covers previous screen)
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(menuContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.03f, 0.05f, 0.08f, 1.0f);
            bgImg.raycastTarget = true;

            // Title
            CreateText(menuContainer.transform, "TitleText", "STARSTRIKE", 56, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.88f), new Vector2(-350, -35), new Vector2(350, 35), defaultFont);

            // Gems
            GameObject gemsObj = CreateText(menuContainer.transform, "GemsText", "Meta-Gems: 0", 20, FontStyle.Bold, Color.cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.80f), new Vector2(0.5f, 0.80f), new Vector2(-200, -20), new Vector2(200, 20), defaultFont);
            gemsText = gemsObj.GetComponent<TMP_Text>();

            // Mode 1: PvE Survival Button
            GameObject pveBtnObj = CreateButton(menuContainer.transform, "PvEStartButton", "PvE SURVIVAL", new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(-160, 0), new Vector2(280, 48), new Color(0.15f, 0.70f, 0.35f), defaultFont, 18);
            pveBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                SetMenuVisible(false);
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadGameplay(GameMode.PvE);
                }
            });

            // Mode 2: Multiplayer PvP Button
            GameObject pvpBtnObj = CreateButton(menuContainer.transform, "PvPStartButton", "MULTIPLAYER PvP", new Vector2(0.5f, 0.13f), new Vector2(0.5f, 0.13f), new Vector2(160, 0), new Vector2(280, 48), new Color(0.90f, 0.35f, 0.15f), defaultFont, 18);
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
            CreateText(menuContainer.transform, "StartHintText", "[ ENTER / SPACE: Launch PvE  |  Or Click Multiplayer PvP ]", 13, FontStyle.Normal, new Color(0.85f, 0.9f, 1f, 0.85f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.05f), new Vector2(0.5f, 0.05f), new Vector2(-350, -15), new Vector2(350, 15), defaultFont);

            // Settings Button (Top Right)
            GameObject settingsBtnObj = CreateButton(menuContainer.transform, "SettingsButton", "SETTINGS [O]", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120f, -40f), new Vector2(180f, 44f), new Color(0.12f, 0.22f, 0.35f), defaultFont, 16);
            settingsBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                if (ScreenManager.Instance != null && ScreenManager.Instance.PushModal("Settings"))
                {
                    // modal pushed
                }
                else if (StarStrike.UI.SettingsUI.Instance != null)
                {
                    StarStrike.UI.SettingsUI.Instance.Open();
                }
            });

            // 1. Ship Hangar
            GameObject shipHeader = CreateText(menuContainer.transform, "ShipHeader", "1. SELECT HANGAR SHIP", 15, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f), new Vector2(-250, -15), new Vector2(250, 15), defaultFont);

            GameObject shipListObj = new GameObject("ShipListContainer", typeof(RectTransform));
            shipListObj.transform.SetParent(menuContainer.transform, false);
            RectTransform slRt = shipListObj.GetComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.5f, 0.63f);
            slRt.anchorMax = new Vector2(0.5f, 0.63f);
            slRt.anchoredPosition = Vector2.zero;
            slRt.sizeDelta = new Vector2(720, 70);
            shipListContainer = shipListObj.transform;

            // 2. Weapon Loadout Selector
            CreateText(menuContainer.transform, "WeaponHeader", "2. CHOOSE EQUIPPED WEAPON", 14, FontStyle.Bold, new Color(1.0f, 0.65f, 0.15f), TextAnchor.MiddleCenter, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(-250, -14), new Vector2(250, 14), defaultFont);

            GameObject weaponListObj = new GameObject("WeaponListContainer", typeof(RectTransform));
            weaponListObj.transform.SetParent(menuContainer.transform, false);
            RectTransform wlRt = weaponListObj.GetComponent<RectTransform>();
            wlRt.anchorMin = new Vector2(0.5f, 0.44f);
            wlRt.anchorMax = new Vector2(0.5f, 0.44f);
            wlRt.anchoredPosition = Vector2.zero;
            wlRt.sizeDelta = new Vector2(740, 46);
            weaponListContainer = weaponListObj.transform;

            // 3. Combat Skill Selector
            CreateText(menuContainer.transform, "SkillHeader", "3. ACTIVE COMBAT SKILL (SPACE / RIGHT-CLICK)", 14, FontStyle.Bold, Color.cyan, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.34f), new Vector2(0.5f, 0.34f), new Vector2(-300, -14), new Vector2(300, 14), defaultFont);

            GameObject skillListObj = new GameObject("SkillListContainer", typeof(RectTransform));
            skillListObj.transform.SetParent(menuContainer.transform, false);
            RectTransform sklRt = skillListObj.GetComponent<RectTransform>();
            sklRt.anchorMin = new Vector2(0.5f, 0.26f);
            sklRt.anchorMax = new Vector2(0.5f, 0.26f);
            sklRt.anchoredPosition = Vector2.zero;
            sklRt.sizeDelta = new Vector2(650, 44);
            skillListContainer = skillListObj.transform;
            
            // We'll populate ships, weapons, and skills in RefreshData
        }

        private void RefreshData()
        {
            if (gemsText != null)
            {
                gemsText.text = $"Meta-Gems: {StorageService.Profile.totalMetaGems}";
            }

            // 1. Ships
            for (int i = shipListContainer.childCount - 1; i >= 0; i--)
            {
                var child = shipListContainer.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            string[] availableShips = new string[] { "scout", "fighter", "miner", "interceptor", "juggernaut", "phantom", "vanguard" };
            int[] costs = new int[] { 0, 500, 500, 1500, 2000, 2500, 2500 };

            float spacing = 100f;
            float startX = -((availableShips.Length - 1) * spacing) / 2f;

            for (int i = 0; i < availableShips.Length; i++)
            {
                string shipId = availableShips[i];
                int cost = costs[i];
                bool isUnlocked = StorageService.Profile.unlockedShips.Contains(shipId);
                bool isSelected = StorageService.GetLastShipEvolved() == shipId;

                string btnText = isUnlocked ? (isSelected ? $"[ {shipId.ToUpper()} ]" : shipId.ToUpper()) : $"{shipId.ToUpper()}\n{cost} G";
                Color btnColor = isUnlocked ? (isSelected ? Color.green : Color.gray) : new Color(0.8f, 0.2f, 0.2f);

                GameObject btnObj = CreateButton(shipListContainer, $"Btn_{shipId}", btnText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * spacing), 0), new Vector2(94, 64), btnColor, defaultFont, 13);
                
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => {
                    if (isUnlocked)
                    {
                        StorageService.SaveLastShipEvolved(shipId);
                        var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
                        if (weaponCtrl != null)
                        {
                            weaponCtrl.EquipFromStorage(shipId);
                        }
                        RefreshData();
                    }
                    else if (StorageService.Profile.totalMetaGems >= cost)
                    {
                        StorageService.Profile.totalMetaGems -= cost;
                        StorageService.UnlockShip(shipId);
                        StorageService.SaveLastShipEvolved(shipId);
                        var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
                        if (weaponCtrl != null)
                        {
                            weaponCtrl.EquipFromStorage(shipId);
                        }
                        RefreshData();
                    }
                });
            }

            // 2. Weapons (Per-ship loadout)
            if (weaponListContainer != null)
            {
                for (int i = weaponListContainer.childCount - 1; i >= 0; i--)
                {
                    var child = weaponListContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                string currentSelectedShip = StorageService.GetLastShipEvolved();
                if (string.IsNullOrEmpty(currentSelectedShip)) currentSelectedShip = "scout";

                string[] weaponIds = new string[] { "pulse", "scattergun", "missile", "beam", "gatling" };
                string[] weaponLabels = new string[] { "PULSE LASER", "SCATTERGUN", "MISSILES", "BEAM LASER", "GATLING GUN" };
                string equippedWeapon = StorageService.GetEquippedWeapon(currentSelectedShip);

                float weaponSpacing = 144f;
                float weaponStartX = -((weaponIds.Length - 1) * weaponSpacing) / 2f;

                for (int i = 0; i < weaponIds.Length; i++)
                {
                    string wId = weaponIds[i];
                    string wLabel = weaponLabels[i];
                    bool isSelected = string.Equals(equippedWeapon, wId, System.StringComparison.OrdinalIgnoreCase);

                    string btnLabel = isSelected ? $"[ {wLabel} ]" : wLabel;
                    Color btnColor = isSelected ? new Color(1.0f, 0.55f, 0.12f) : new Color(0.24f, 0.22f, 0.32f);

                    GameObject weaponBtnObj = CreateButton(weaponListContainer, $"WeaponBtn_{wId}", btnLabel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(weaponStartX + (i * weaponSpacing), 0), new Vector2(138, 40), btnColor, defaultFont, 13);
                    Button weaponBtn = weaponBtnObj.GetComponent<Button>();
                    weaponBtn.onClick.AddListener(() => {
                        StorageService.SaveEquippedWeapon(currentSelectedShip, wId);
                        StorageService.SaveSelectedWeapon(wId);

                        var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
                        if (weaponCtrl != null)
                        {
                            weaponCtrl.EquipFromStorage(currentSelectedShip);
                        }
                        RefreshData();
                    });
                }
            }

            // 3. Active Skills
            if (skillListContainer != null)
            {
                for (int i = skillListContainer.childCount - 1; i >= 0; i--)
                {
                    var child = skillListContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                string[] skillIds = new string[] { "forward_shockwave", "blink_teleport", "emp_nova" };
                string[] skillLabels = new string[] { "SHOCKWAVE", "BLINK WARP", "EMP NOVA" };
                string currentSkill = StorageService.GetSelectedSkill();

                float skillSpacing = 190f;
                float skillStartX = -((skillIds.Length - 1) * skillSpacing) / 2f;

                for (int i = 0; i < skillIds.Length; i++)
                {
                    string sId = skillIds[i];
                    string sLabel = skillLabels[i];
                    bool isSelected = currentSkill == sId;

                    string btnLabel = isSelected ? $"[ {sLabel} ]" : sLabel;
                    Color btnColor = isSelected ? new Color(0.1f, 0.7f, 0.85f) : new Color(0.2f, 0.25f, 0.35f);

                    GameObject skillBtnObj = CreateButton(skillListContainer, $"SkillBtn_{sId}", btnLabel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(skillStartX + (i * skillSpacing), 0), new Vector2(175, 40), btnColor, defaultFont, 14);
                    Button skillBtn = skillBtnObj.GetComponent<Button>();
                    skillBtn.onClick.AddListener(() => {
                        StorageService.SaveSelectedSkill(sId);
                        var shipSkill = FindAnyObjectByType<ShipSkillController>();
                        if (shipSkill != null)
                        {
                            shipSkill.EquipActiveSkill(sId);
                        }
                        RefreshData();
                    });
                }
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
