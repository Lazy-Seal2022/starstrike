using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class SettingsUI : MonoBehaviour
    {
        public static SettingsUI Instance { get; private set; }

        public enum SettingsTab
        {
            Audio,
            Controls
        }

        [Header("Theme & Visuals")]
        public UIThemeConfig themeConfig;

        [Header("Roots")]
        public GameObject settingsContainer;

        private SettingsTab activeTab = SettingsTab.Audio;
        private GameObject audioTabPanel;
        private GameObject controlsTabPanel;

        private Button audioTabBtn;
        private Button controlsTabBtn;
        private Text audioTabBtnText;
        private Text controlsTabBtnText;

        // Audio controls
        private Text masterVolText;
        private Image masterVolFill;
        private Text sfxVolText;
        private Image sfxVolFill;
        private Text musicVolText;
        private Image musicVolFill;
        private Button muteBtn;
        private Text muteBtnText;

        // Controls
        private Button aimModeBtn;
        private Text aimModeBtnText;
        private Text aimModeDesc;
        private Button autoBrakeBtn;
        private Text autoBrakeBtnText;
        private Text autoBrakeDesc;
        private Button invertTurnBtn;
        private Text invertTurnBtnText;
        private Button touchHudBtn;
        private Text touchHudBtnText;

        private GameState stateBeforeOpening = GameState.Menu;
        private bool wasMainMenuVisible = false;

        public bool IsVisible => settingsContainer != null && settingsContainer.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (themeConfig == null)
            {
                themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            }

            BuildUI();
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame || kb.oKey.wasPressedThisFrame)
                {
                    if (IsVisible)
                    {
                        Close();
                    }
                    else if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
                    {
                        Open();
                    }
                }
            }
        }

        public void EnsureUIBuilt()
        {
            if (Instance == null) Instance = this;
            if (settingsContainer == null)
            {
                if (themeConfig == null)
                {
                    themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
                }
                BuildUI();
            }
        }

        public void Open()
        {
            EnsureUIBuilt();

            wasMainMenuVisible = false;
            var menu = MainMenuUI.Instance ?? FindAnyObjectByType<MainMenuUI>();
            if (menu != null && menu.IsVisible)
            {
                wasMainMenuVisible = true;
                menu.SetMenuVisible(false);
            }

            var hud = CanvasHUD.Instance ?? FindAnyObjectByType<CanvasHUD>();
            if (hud != null)
            {
                hud.SetHUDVisible(false);
            }

            if (GameManager.Instance != null)
            {
                stateBeforeOpening = GameManager.Instance.CurrentState;
                if (stateBeforeOpening == GameState.Playing)
                {
                    GameManager.Instance.PauseGame();
                }
            }

            RefreshAllUI();
            SetVisible(true);
        }

        public void Close()
        {
            EnsureUIBuilt();
            SetVisible(false);

            if (PlayerInputReader.Instance != null)
            {
                PlayerInputReader.Instance.ApplySettings();
            }

            if (CanvasHUD.Instance != null)
            {
                CanvasHUD.Instance.forceShowMobileControls = StorageService.GetShowMobileControls();
            }

            var menu = MainMenuUI.Instance ?? FindAnyObjectByType<MainMenuUI>();
            if (wasMainMenuVisible && (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Menu))
            {
                if (menu != null)
                {
                    menu.SetMenuVisible(true);
                }
            }
            else if (GameManager.Instance != null && stateBeforeOpening == GameState.Playing)
            {
                if (GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.ResumeGame();
                }
                var hud = CanvasHUD.Instance ?? FindAnyObjectByType<CanvasHUD>();
                if (hud != null)
                {
                    hud.SetHUDVisible(true);
                }
            }
        }

        public void Toggle()
        {
            if (IsVisible) Close();
            else Open();
        }

        public void SetVisible(bool visible)
        {
            EnsureUIBuilt();
            if (settingsContainer != null)
            {
                settingsContainer.SetActive(visible);
            }
        }

        public void SwitchTab(SettingsTab tab)
        {
            EnsureUIBuilt();
            activeTab = tab;

            if (audioTabPanel != null) audioTabPanel.SetActive(activeTab == SettingsTab.Audio);
            if (controlsTabPanel != null) controlsTabPanel.SetActive(activeTab == SettingsTab.Controls);

            Color activeColor = themeConfig != null ? themeConfig.buttonHover : new Color(0.18f, 0.45f, 0.75f, 1f);
            Color normalColor = themeConfig != null ? themeConfig.buttonNormal : new Color(0.1f, 0.18f, 0.28f, 0.9f);

            if (audioTabBtn != null)
            {
                audioTabBtn.GetComponent<Image>().color = activeTab == SettingsTab.Audio ? activeColor : normalColor;
            }
            if (controlsTabBtn != null)
            {
                controlsTabBtn.GetComponent<Image>().color = activeTab == SettingsTab.Controls ? activeColor : normalColor;
            }
        }

        public void RefreshAllUI()
        {
            EnsureUIBuilt();

            // Audio values
            float mVol = StorageService.GetMasterVolume();
            float sVol = StorageService.GetSfxVolume();
            float muVol = StorageService.GetMusicVolume();
            bool muted = StorageService.GetAudioMuted();

            if (masterVolText != null) masterVolText.text = $"MASTER VOLUME: {Mathf.RoundToInt(mVol * 100f)}%";
            if (masterVolFill != null) masterVolFill.fillAmount = mVol;

            if (sfxVolText != null) sfxVolText.text = $"SFX VOLUME: {Mathf.RoundToInt(sVol * 100f)}%";
            if (sfxVolFill != null) sfxVolFill.fillAmount = sVol;

            if (musicVolText != null) musicVolText.text = $"MUSIC VOLUME: {Mathf.RoundToInt(muVol * 100f)}%";
            if (musicVolFill != null) musicVolFill.fillAmount = muVol;

            if (muteBtnText != null && muteBtn != null)
            {
                muteBtnText.text = muted ? "🔇 AUDIO: MUTED" : "🔊 AUDIO: ACTIVE";
                muteBtn.GetComponent<Image>().color = muted
                    ? (themeConfig != null ? themeConfig.dangerColor : new Color(0.85f, 0.2f, 0.2f))
                    : (themeConfig != null ? themeConfig.buttonNormal : new Color(0.12f, 0.35f, 0.25f));
            }

            // Controls values
            int aimMode = StorageService.GetControlAimMode();
            if (aimModeBtnText != null)
            {
                aimModeBtnText.text = aimMode == 0 ? "AIM MODE: [MOUSE DIRECT / STICK]" : "AIM MODE: [KEYBOARD A/D TURN]";
            }
            if (aimModeDesc != null)
            {
                aimModeDesc.text = aimMode == 0
                    ? "• Ship points towards Mouse Cursor or Gamepad Stick. Press W to thrust forward."
                    : "• Classic Asteroids: A/D rotates ship, W thrusts forward, S brakes. Mouse aim deactivated.";
            }

            bool autoBrake = StorageService.GetAutoBrake();
            if (autoBrakeBtnText != null)
            {
                autoBrakeBtnText.text = autoBrake ? "INERTIA STABILIZER: [AUTO-BRAKE ON]" : "INERTIA STABILIZER: [FREE DRIFT]";
            }
            if (autoBrakeDesc != null)
            {
                autoBrakeDesc.text = autoBrake
                    ? "• Ship automatically decelerates when thrust is released."
                    : "• Realistic zero-G momentum: Ship maintains speed until you press S to brake.";
            }

            bool invert = StorageService.GetInvertTurn();
            if (invertTurnBtnText != null)
            {
                invertTurnBtnText.text = invert ? "STEERING: [INVERTED A/D]" : "STEERING: [NORMAL]";
            }

            bool touch = StorageService.GetShowMobileControls();
            if (touchHudBtnText != null)
            {
                touchHudBtnText.text = touch ? "ON-SCREEN HUD: [FORCE SHOWN]" : "ON-SCREEN HUD: [AUTO / MOBILE]";
            }

            SwitchTab(activeTab);
        }

        // --- Audio Event Handlers ---
        public void AdjustMasterVolume(float delta)
        {
            float cur = StorageService.GetMasterVolume();
            float next = Mathf.Clamp01((float)Math.Round(cur + delta, 2));
            if (AudioService.Instance != null) AudioService.Instance.SetMasterVolume(next);
            else StorageService.SaveMasterVolume(next);
            RefreshAllUI();
        }

        public void AdjustSfxVolume(float delta)
        {
            float cur = StorageService.GetSfxVolume();
            float next = Mathf.Clamp01((float)Math.Round(cur + delta, 2));
            if (AudioService.Instance != null) AudioService.Instance.SetSfxVolume(next);
            else StorageService.SaveSfxVolume(next);
            RefreshAllUI();
        }

        public void AdjustMusicVolume(float delta)
        {
            float cur = StorageService.GetMusicVolume();
            float next = Mathf.Clamp01((float)Math.Round(cur + delta, 2));
            if (AudioService.Instance != null) AudioService.Instance.SetMusicVolume(next);
            else StorageService.SaveMusicVolume(next);
            RefreshAllUI();
        }

        public void ToggleMute()
        {
            bool cur = StorageService.GetAudioMuted();
            bool next = !cur;
            if (AudioService.Instance != null) AudioService.Instance.SetMuted(next);
            else StorageService.SaveAudioMuted(next);
            RefreshAllUI();
        }

        public void PlayTestAudio()
        {
            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySound("laser", 0.9f);
            }
        }

        // --- Controls Event Handlers ---
        public void ToggleAimMode()
        {
            int cur = StorageService.GetControlAimMode();
            int next = (cur == 0) ? 1 : 0;
            StorageService.SaveControlAimMode(next);
            if (PlayerInputReader.Instance != null) PlayerInputReader.Instance.ApplySettings();
            RefreshAllUI();
        }

        public void ToggleAutoBrake()
        {
            bool cur = StorageService.GetAutoBrake();
            bool next = !cur;
            StorageService.SaveAutoBrake(next);
            if (PlayerInputReader.Instance != null) PlayerInputReader.Instance.ApplySettings();
            RefreshAllUI();
        }

        public void ToggleInvertTurn()
        {
            bool cur = StorageService.GetInvertTurn();
            bool next = !cur;
            StorageService.SaveInvertTurn(next);
            if (PlayerInputReader.Instance != null) PlayerInputReader.Instance.ApplySettings();
            RefreshAllUI();
        }

        public void ToggleTouchHud()
        {
            bool cur = StorageService.GetShowMobileControls();
            bool next = !cur;
            StorageService.SaveShowMobileControls(next);
            if (CanvasHUD.Instance != null) CanvasHUD.Instance.forceShowMobileControls = next;
            RefreshAllUI();
        }

        public void ResetDefaults()
        {
            StorageService.ResetSettingsToDefaults();
            if (AudioService.Instance != null) AudioService.Instance.LoadSettings();
            if (PlayerInputReader.Instance != null) PlayerInputReader.Instance.ApplySettings();
            if (CanvasHUD.Instance != null) CanvasHUD.Instance.forceShowMobileControls = false;
            RefreshAllUI();
        }

        // --- UI Construction ---
        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sprite roundedSprite = ProceduralSpriteHelper.GetRoundedBoxSprite(32, 6);

            // Container stretches to cover the entire screen so backdrop dims everything behind
            settingsContainer = new GameObject("SettingsContainer", typeof(RectTransform));
            settingsContainer.transform.SetParent(transform, false);

            var cRt = settingsContainer.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;

            // 1. Full-screen Dimmer Backdrop (blocks clicks and prevents background bleeding)
            GameObject backdropObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdropObj.transform.SetParent(settingsContainer.transform, false);
            var bdRt = backdropObj.GetComponent<RectTransform>();
            bdRt.anchorMin = Vector2.zero;
            bdRt.anchorMax = Vector2.one;
            bdRt.offsetMin = Vector2.zero;
            bdRt.offsetMax = Vector2.zero;
            var bdImg = backdropObj.GetComponent<Image>();
            bdImg.color = new Color(0.02f, 0.04f, 0.08f, 0.88f);

            // 2. Centered Dialog Card
            GameObject dialogCard = new GameObject("DialogCard", typeof(RectTransform), typeof(Image));
            dialogCard.transform.SetParent(settingsContainer.transform, false);
            var cardRt = dialogCard.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(780f, 640f);

            var cardImg = dialogCard.GetComponent<Image>();
            cardImg.sprite = roundedSprite;
            cardImg.type = Image.Type.Sliced;
            cardImg.color = themeConfig != null ? themeConfig.panelBackground : new Color(0.06f, 0.09f, 0.15f, 0.98f);

            // 3. Header Section
            CreateTextExplicit(dialogCard.transform, "HeaderTitle", "⚙ SYSTEM SETTINGS", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(700f, 36f), defaultFont);

            CreateTextExplicit(dialogCard.transform, "HeaderSub", "Audio Fidelity & Flight Controls Configuration", 12, FontStyle.Normal, new Color(0.6f, 0.8f, 0.95f, 0.85f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0f, -56f), new Vector2(700f, 20f), defaultFont);

            // Top-Right Close '✕' Button
            var xBtn = CreateButtonExplicit(dialogCard.transform, "BtnCloseX", "✕", new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(36f, 36f),
                new Color(0.35f, 0.15f, 0.18f), defaultFont, 16);
            xBtn.onClick.AddListener(Close);

            // 4. Tab Navigation Bar (y = -85)
            GameObject tabNav = new GameObject("TabNavBar", typeof(RectTransform));
            tabNav.transform.SetParent(dialogCard.transform, false);
            var tnRt = tabNav.GetComponent<RectTransform>();
            tnRt.anchorMin = new Vector2(0.5f, 1f);
            tnRt.anchorMax = new Vector2(0.5f, 1f);
            tnRt.pivot = new Vector2(0.5f, 1f);
            tnRt.anchoredPosition = new Vector2(0f, -85f);
            tnRt.sizeDelta = new Vector2(700f, 44f);

            audioTabBtn = CreateButtonExplicit(tabNav.transform, "BtnAudioTab", "🔊 AUDIO SETTINGS", new Vector2(0f, 0.5f), new Vector2(170f, 0f), new Vector2(340f, 42f),
                new Color(0.18f, 0.45f, 0.75f), defaultFont, 15);
            audioTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Audio));
            audioTabBtnText = audioTabBtn.GetComponentInChildren<Text>();

            controlsTabBtn = CreateButtonExplicit(tabNav.transform, "BtnControlsTab", "🎮 FLIGHT CONTROLS", new Vector2(1f, 0.5f), new Vector2(-170f, 0f), new Vector2(340f, 42f),
                new Color(0.1f, 0.18f, 0.28f), defaultFont, 15);
            controlsTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Controls));
            controlsTabBtnText = controlsTabBtn.GetComponentInChildren<Text>();

            // 5. Content Area Panels
            BuildAudioPanel(dialogCard.transform, defaultFont, roundedSprite);
            BuildControlsPanel(dialogCard.transform, defaultFont, roundedSprite);

            // 6. Footer Section (Bottom 20px)
            GameObject footer = new GameObject("Footer", typeof(RectTransform));
            footer.transform.SetParent(dialogCard.transform, false);
            var fRt = footer.GetComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0.5f, 0f);
            fRt.anchorMax = new Vector2(0.5f, 0f);
            fRt.pivot = new Vector2(0.5f, 0f);
            fRt.anchoredPosition = new Vector2(0f, 20f);
            fRt.sizeDelta = new Vector2(700f, 46f);

            var resetBtn = CreateButtonExplicit(footer.transform, "BtnResetDefaults", "↺ RESET DEFAULTS", new Vector2(0f, 0.5f), new Vector2(105f, 0f), new Vector2(210f, 44f),
                new Color(0.35f, 0.18f, 0.18f), defaultFont, 14);
            resetBtn.onClick.AddListener(ResetDefaults);

            var closeBtn = CreateButtonExplicit(footer.transform, "BtnClose", "✔ SAVE & CLOSE [ESC]", new Vector2(1f, 0.5f), new Vector2(-140f, 0f), new Vector2(280f, 44f),
                new Color(0.12f, 0.55f, 0.35f), defaultFont, 15);
            closeBtn.onClick.AddListener(Close);

            // Initially set active tab visibility
            SwitchTab(SettingsTab.Audio);
        }

        private void BuildAudioPanel(Transform parent, Font font, Sprite roundedSprite)
        {
            audioTabPanel = new GameObject("AudioTabPanel", typeof(RectTransform));
            audioTabPanel.transform.SetParent(parent, false);
            var rt = audioTabPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -145f);
            rt.sizeDelta = new Vector2(700f, 410f);

            float rowY = -15f;

            // Row 1: Master Volume
            CreateVolumeRow(audioTabPanel.transform, "Master", "MASTER VOLUME: 100%", rowY, font, roundedSprite,
                out masterVolText, out masterVolFill, () => AdjustMasterVolume(-0.1f), () => AdjustMasterVolume(0.1f));

            rowY -= 85f;
            // Row 2: SFX Volume
            CreateVolumeRow(audioTabPanel.transform, "SFX", "SFX VOLUME: 80%", rowY, font, roundedSprite,
                out sfxVolText, out sfxVolFill, () => AdjustSfxVolume(-0.1f), () => AdjustSfxVolume(0.1f));

            rowY -= 85f;
            // Row 3: Music Volume
            CreateVolumeRow(audioTabPanel.transform, "Music", "MUSIC VOLUME: 70%", rowY, font, roundedSprite,
                out musicVolText, out musicVolFill, () => AdjustMusicVolume(-0.1f), () => AdjustMusicVolume(0.1f));

            rowY -= 95f;
            // Row 4: Mute Audio Button & Test Sound Button
            muteBtn = CreateButtonExplicit(audioTabPanel.transform, "BtnMute", "🔊 AUDIO: ACTIVE", new Vector2(0f, 1f), new Vector2(165f, rowY), new Vector2(330f, 46f),
                new Color(0.12f, 0.35f, 0.25f), font, 14);
            muteBtn.onClick.AddListener(ToggleMute);
            muteBtnText = muteBtn.GetComponentInChildren<Text>();

            var testBtn = CreateButtonExplicit(audioTabPanel.transform, "BtnTestSfx", "► TEST SFX AUDIO", new Vector2(1f, 1f), new Vector2(-165f, rowY), new Vector2(330f, 46f),
                new Color(0.18f, 0.35f, 0.55f), font, 14);
            testBtn.onClick.AddListener(PlayTestAudio);
        }

        private void CreateVolumeRow(Transform parent, string prefix, string label, float topY, Font font, Sprite roundedSprite,
            out Text labelText, out Image fillImg, Action onMinus, Action onPlus)
        {
            GameObject rowObj = new GameObject($"Row_{prefix}", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            var rt = rowObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topY);
            rt.sizeDelta = new Vector2(700f, 75f);

            var lblObj = CreateTextExplicit(rowObj.transform, "Label", label, 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(700f, 24f), font);
            labelText = lblObj.GetComponent<Text>();

            // Stepper controls container
            GameObject ctrlObj = new GameObject("Controls", typeof(RectTransform));
            ctrlObj.transform.SetParent(rowObj.transform, false);
            var cRt = ctrlObj.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 1f);
            cRt.anchorMax = new Vector2(0.5f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = new Vector2(0f, -40f);
            cRt.sizeDelta = new Vector2(700f, 32f);

            // [-] button
            var minusBtn = CreateButtonExplicit(ctrlObj.transform, "BtnMinus", "-", new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(44f, 32f),
                new Color(0.12f, 0.22f, 0.35f), font, 18);
            minusBtn.onClick.AddListener(() => onMinus());

            // Progress bar (between minus and plus)
            GameObject barBg = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
            barBg.transform.SetParent(ctrlObj.transform, false);
            var bRt = barBg.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0.5f);
            bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.pivot = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = Vector2.zero;
            bRt.sizeDelta = new Vector2(580f, 22f);

            var bgImage = barBg.GetComponent<Image>();
            bgImage.sprite = roundedSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.08f, 0.12f, 0.20f, 1f);

            GameObject barFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            var fRt = barFill.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.offsetMin = new Vector2(2f, 2f);
            fRt.offsetMax = new Vector2(-2f, -2f);

            fillImg = barFill.GetComponent<Image>();
            fillImg.sprite = roundedSprite;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = themeConfig != null ? themeConfig.primaryAccent : new Color(0.2f, 0.85f, 1f, 1f);

            // [+] button
            var plusBtn = CreateButtonExplicit(ctrlObj.transform, "BtnPlus", "+", new Vector2(1f, 0.5f), new Vector2(-22f, 0f), new Vector2(44f, 32f),
                new Color(0.12f, 0.22f, 0.35f), font, 18);
            plusBtn.onClick.AddListener(() => onPlus());
        }

        private void BuildControlsPanel(Transform parent, Font font, Sprite roundedSprite)
        {
            controlsTabPanel = new GameObject("ControlsTabPanel", typeof(RectTransform));
            controlsTabPanel.transform.SetParent(parent, false);
            var rt = controlsTabPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -145f);
            rt.sizeDelta = new Vector2(700f, 410f);

            float rowY = -10f;

            // 1. Aim / Steering Mode Card
            aimModeBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnAimMode", "AIM MODE: [MOUSE DIRECT / STICK]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 18f), new Vector2(700f, 38f),
                new Color(0.12f, 0.25f, 0.45f), font, 15);
            aimModeBtn.onClick.AddListener(ToggleAimMode);
            aimModeBtnText = aimModeBtn.GetComponentInChildren<Text>();

            rowY -= 44f;
            var desc1 = CreateTextExplicit(controlsTabPanel.transform, "AimDesc", "• Ship points towards Mouse Cursor or Gamepad Stick. Press W to thrust forward.", 12, FontStyle.Normal, new Color(0.75f, 0.88f, 0.98f), TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1f), new Vector2(0f, rowY - 15f), new Vector2(680f, 30f), font);
            aimModeDesc = desc1.GetComponent<Text>();

            rowY -= 45f;
            // 2. Auto-Brake (Inertia Stabilizer) Card
            autoBrakeBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnAutoBrake", "INERTIA STABILIZER: [AUTO-BRAKE ON]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 18f), new Vector2(700f, 38f),
                new Color(0.12f, 0.25f, 0.45f), font, 15);
            autoBrakeBtn.onClick.AddListener(ToggleAutoBrake);
            autoBrakeBtnText = autoBrakeBtn.GetComponentInChildren<Text>();

            rowY -= 44f;
            var desc2 = CreateTextExplicit(controlsTabPanel.transform, "AutoBrakeDesc", "• Automatically decelerates ship when thrust is released.", 12, FontStyle.Normal, new Color(0.75f, 0.88f, 0.98f), TextAnchor.MiddleLeft,
                new Vector2(0.5f, 1f), new Vector2(0f, rowY - 15f), new Vector2(680f, 30f), font);
            autoBrakeDesc = desc2.GetComponent<Text>();

            rowY -= 48f;
            // 3. Quick Toggles Row (Side-by-side)
            invertTurnBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnInvertTurn", "STEERING: [NORMAL]", new Vector2(0f, 1f), new Vector2(165f, rowY - 18f), new Vector2(330f, 38f),
                new Color(0.12f, 0.22f, 0.35f), font, 13);
            invertTurnBtn.onClick.AddListener(ToggleInvertTurn);
            invertTurnBtnText = invertTurnBtn.GetComponentInChildren<Text>();

            touchHudBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnTouchHud", "ON-SCREEN HUD: [AUTO / MOBILE]", new Vector2(1f, 1f), new Vector2(-165f, rowY - 18f), new Vector2(330f, 38f),
                new Color(0.12f, 0.22f, 0.35f), font, 13);
            touchHudBtn.onClick.AddListener(ToggleTouchHud);
            touchHudBtnText = touchHudBtn.GetComponentInChildren<Text>();

            rowY -= 54f;
            // 4. Quick Key Reference Card
            GameObject keyBox = new GameObject("KeyReferenceBox", typeof(RectTransform), typeof(Image));
            keyBox.transform.SetParent(controlsTabPanel.transform, false);
            var kbRt = keyBox.GetComponent<RectTransform>();
            kbRt.anchorMin = new Vector2(0.5f, 1f);
            kbRt.anchorMax = new Vector2(0.5f, 1f);
            kbRt.pivot = new Vector2(0.5f, 1f);
            kbRt.anchoredPosition = new Vector2(0f, rowY);
            kbRt.sizeDelta = new Vector2(700f, 110f);

            var kbImg = keyBox.GetComponent<Image>();
            kbImg.sprite = roundedSprite;
            kbImg.type = Image.Type.Sliced;
            kbImg.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);

            string keyGuide = "<b>FLIGHT CONTROLS REFERENCE:</b>\n" +
                              "<color=#38BDF8>W / Up / Left Stick</color>: Thrust Forward    •    <color=#F87171>S / Down</color>: Brake Ship\n" +
                              "<color=#38BDF8>A / D Keys</color>: Steer Ship (or Mouse Aim)      •    <color=#4ADE80>Shift / Right Click</color>: Boost Engines\n" +
                              "<color=#FBBF24>Space / Left Click / RT</color>: Fire Weapon     •    <color=#A78BFA>U Key</color>: Upgrades Drawer\n" +
                              "<color=#94A3B8>ESC / O Key</color>: Toggle Settings / Pause";

            CreateTextExplicit(keyBox.transform, "KeyGuideText", keyGuide, 12, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 95f), font);
        }

        // --- Explicit Helpers (Safe RectTransform geometry) ---
        private GameObject CreateTextExplicit(Transform parent, string name, string text, int fontSize, FontStyle style, Color color, TextAnchor align,
            Vector2 anchor, Vector2 pos, Vector2 size, Font font)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var txt = obj.GetComponent<Text>();
            txt.text = text;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = align;
            txt.supportRichText = true;
            return obj;
        }

        private Button CreateButtonExplicit(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color color, Font font, int fontSize)
        {
            Sprite roundedSprite = ProceduralSpriteHelper.GetRoundedBoxSprite(32, 6);

            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = obj.GetComponent<Image>();
            img.sprite = roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = color;

            var btn = obj.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = color * 1.25f;
            colors.pressedColor = color * 0.85f;
            btn.colors = colors;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            var txt = textObj.GetComponent<Text>();
            txt.text = label;
            txt.font = font;
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return btn;
        }
    }
}
