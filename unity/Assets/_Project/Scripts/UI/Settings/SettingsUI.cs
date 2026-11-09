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

            if (GameManager.Instance != null && stateBeforeOpening == GameState.Playing && GameManager.Instance.CurrentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
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
            // Audio
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

            if (muteBtnText != null)
            {
                muteBtnText.text = muted ? "🔇 AUDIO: MUTED" : "🔊 AUDIO: ACTIVE";
                muteBtn.GetComponent<Image>().color = muted
                    ? (themeConfig != null ? themeConfig.dangerColor : new Color(0.85f, 0.2f, 0.2f))
                    : (themeConfig != null ? themeConfig.buttonNormal : new Color(0.12f, 0.35f, 0.25f));
            }

            // Controls
            int aimMode = StorageService.GetControlAimMode();
            if (aimModeBtnText != null)
            {
                aimModeBtnText.text = aimMode == 0 ? "AIM MODE: [MOUSE DIRECT / STICK]" : "AIM MODE: [KEYBOARD A/D TURN]";
            }
            if (aimModeDesc != null)
            {
                aimModeDesc.text = aimMode == 0
                    ? "• Ship points towards Mouse Cursor or Right Gamepad Stick.\n• Press W to thrust forward."
                    : "• Classic Asteroids style: A/D rotates ship, W thrusts forward, S brakes.\n• Mouse aim is deactivated.";
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
                invertTurnBtnText.text = invert ? "STEERING INVERSION: [INVERTED]" : "STEERING INVERSION: [NORMAL]";
            }

            bool touch = StorageService.GetShowMobileControls();
            if (touchHudBtnText != null)
            {
                touchHudBtnText.text = touch ? "ON-SCREEN CONTROLS: [FORCE SHOWN]" : "ON-SCREEN CONTROLS: [AUTO / MOBILE ONLY]";
            }

            SwitchTab(activeTab);
        }

        // --- Audio Event Triggers ---
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

        // --- Controls Event Triggers ---
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

            // Container Panel
            settingsContainer = new GameObject("SettingsContainer", typeof(RectTransform));
            settingsContainer.transform.SetParent(transform, false);

            var cRt = settingsContainer.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.sizeDelta = new Vector2(740f, 620f);

            // Backdrop
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(settingsContainer.transform, false);
            var bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = roundedSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = themeConfig != null ? themeConfig.panelBackground : new Color(0.04f, 0.07f, 0.12f, 0.96f);

            // Header Title
            CreateText(settingsContainer.transform, "HeaderTitle", "⚙ SYSTEM SETTINGS", 26, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -15f), new Vector2(0f, -50f), defaultFont);
            CreateText(settingsContainer.transform, "HeaderSub", "CONFIGURE AUDIO FIDELITY & FLIGHT CONTROLS", 12, FontStyle.Normal, themeConfig != null ? themeConfig.textMuted : Color.gray, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -50f), new Vector2(0f, -70f), defaultFont);

            // Tab Navigation Bar
            GameObject tabNav = new GameObject("TabNavBar", typeof(RectTransform));
            tabNav.transform.SetParent(settingsContainer.transform, false);
            var tnRt = tabNav.GetComponent<RectTransform>();
            tnRt.anchorMin = new Vector2(0f, 1f);
            tnRt.anchorMax = new Vector2(1f, 1f);
            tnRt.offsetMin = new Vector2(40f, -125f);
            tnRt.offsetMax = new Vector2(-40f, -80f);

            audioTabBtn = CreateButton(tabNav.transform, "BtnAudioTab", "🔊 AUDIO", new Vector2(0f, 0f), new Vector2(0.48f, 1f), new Color(0.15f, 0.35f, 0.6f), defaultFont, 16);
            audioTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Audio));
            audioTabBtnText = audioTabBtn.GetComponentInChildren<Text>();

            controlsTabBtn = CreateButton(tabNav.transform, "BtnControlsTab", "🎮 CONTROLS", new Vector2(0.52f, 0f), new Vector2(1f, 1f), new Color(0.1f, 0.18f, 0.28f), defaultFont, 16);
            controlsTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Controls));
            controlsTabBtnText = controlsTabBtn.GetComponentInChildren<Text>();

            // Build Panels
            BuildAudioPanel(settingsContainer.transform, defaultFont, roundedSprite);
            BuildControlsPanel(settingsContainer.transform, defaultFont, roundedSprite);

            // Bottom Action Bar
            GameObject footer = new GameObject("Footer", typeof(RectTransform));
            footer.transform.SetParent(settingsContainer.transform, false);
            var fRt = footer.GetComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0f, 0f);
            fRt.anchorMax = new Vector2(1f, 0f);
            fRt.offsetMin = new Vector2(40f, 20f);
            fRt.offsetMax = new Vector2(-40f, 65f);

            var resetBtn = CreateButton(footer.transform, "BtnResetDefaults", "↺ RESET DEFAULTS", new Vector2(0f, 0f), new Vector2(0.35f, 1f), new Color(0.35f, 0.18f, 0.18f), defaultFont, 14);
            resetBtn.onClick.AddListener(ResetDefaults);

            var closeBtn = CreateButton(footer.transform, "BtnClose", "✔ SAVE & CLOSE [ESC]", new Vector2(0.40f, 0f), new Vector2(1f, 1f), new Color(0.12f, 0.55f, 0.35f), defaultFont, 15);
            closeBtn.onClick.AddListener(Close);

            // Top-right close 'X'
            var xBtn = CreateButton(settingsContainer.transform, "BtnCloseX", "✕", new Vector2(1f, 1f), new Vector2(1f, 1f), new Color(0.3f, 0.12f, 0.15f), defaultFont, 16);
            var xRt = xBtn.GetComponent<RectTransform>();
            xRt.anchoredPosition = new Vector2(-28f, -28f);
            xRt.sizeDelta = new Vector2(36f, 36f);
            xBtn.onClick.AddListener(Close);
        }

        private void BuildAudioPanel(Transform parent, Font font, Sprite roundedSprite)
        {
            audioTabPanel = new GameObject("AudioTabPanel", typeof(RectTransform));
            audioTabPanel.transform.SetParent(parent, false);
            var rt = audioTabPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(40f, 80f);
            rt.offsetMax = new Vector2(-40f, -135f);

            float y = -15f;

            // 1. Master Volume
            CreateVolumeRow(audioTabPanel.transform, "Master", "MASTER VOLUME: 100%", y, font, roundedSprite,
                out masterVolText, out masterVolFill, () => AdjustMasterVolume(-0.1f), () => AdjustMasterVolume(0.1f));

            y -= 80f;
            // 2. SFX Volume
            CreateVolumeRow(audioTabPanel.transform, "SFX", "SFX VOLUME: 80%", y, font, roundedSprite,
                out sfxVolText, out sfxVolFill, () => AdjustSfxVolume(-0.1f), () => AdjustSfxVolume(0.1f));

            y -= 80f;
            // 3. Music Volume
            CreateVolumeRow(audioTabPanel.transform, "Music", "MUSIC VOLUME: 70%", y, font, roundedSprite,
                out musicVolText, out musicVolFill, () => AdjustMusicVolume(-0.1f), () => AdjustMusicVolume(0.1f));

            y -= 85f;
            // 4. Mute Audio Button & Test Sound Button
            muteBtn = CreateButton(audioTabPanel.transform, "BtnMute", "🔊 AUDIO: ACTIVE", new Vector2(0f, 1f), new Vector2(0.55f, 1f), new Color(0.12f, 0.35f, 0.25f), font, 14);
            var mRt = muteBtn.GetComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(0f, y);
            mRt.sizeDelta = new Vector2(0f, 40f);
            mRt.pivot = new Vector2(0f, 1f);
            muteBtn.onClick.AddListener(ToggleMute);
            muteBtnText = muteBtn.GetComponentInChildren<Text>();

            var testBtn = CreateButton(audioTabPanel.transform, "BtnTestSfx", "► TEST SFX", new Vector2(0.60f, 1f), new Vector2(1f, 1f), new Color(0.18f, 0.35f, 0.55f), font, 14);
            var tRt = testBtn.GetComponent<RectTransform>();
            tRt.anchoredPosition = new Vector2(0f, y);
            tRt.sizeDelta = new Vector2(0f, 40f);
            tRt.pivot = new Vector2(0f, 1f);
            testBtn.onClick.AddListener(PlayTestAudio);
        }

        private void CreateVolumeRow(Transform parent, string prefix, string label, float topY, Font font, Sprite roundedSprite,
            out Text labelText, out Image fillImg, Action onMinus, Action onPlus)
        {
            GameObject rowObj = new GameObject($"Row_{prefix}", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            var rt = rowObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topY);
            rt.sizeDelta = new Vector2(0f, 65f);

            var lblObj = CreateText(rowObj.transform, "Label", label, 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, -24f), font);
            labelText = lblObj.GetComponent<Text>();

            // [-] button
            var minusBtn = CreateButton(rowObj.transform, "BtnMinus", "-", new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(0.12f, 0.22f, 0.35f), font, 18);
            var mRt = minusBtn.GetComponent<RectTransform>();
            mRt.anchoredPosition = new Vector2(25f, 15f);
            mRt.sizeDelta = new Vector2(50f, 30f);
            minusBtn.onClick.AddListener(() => onMinus());

            // Progress bar
            GameObject barBg = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
            barBg.transform.SetParent(rowObj.transform, false);
            var bRt = barBg.GetComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0f, 0f);
            bRt.anchorMax = new Vector2(1f, 0f);
            bRt.offsetMin = new Vector2(65f, 0f);
            bRt.offsetMax = new Vector2(-65f, 30f);
            var bgImage = barBg.GetComponent<Image>();
            bgImage.sprite = roundedSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.08f, 0.12f, 0.20f, 1f);

            GameObject barFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            barFill.transform.SetParent(barBg.transform, false);
            var fRt = barFill.GetComponent<RectTransform>();
            fRt.anchorMin = Vector2.zero;
            fRt.anchorMax = Vector2.one;
            fRt.offsetMin = new Vector2(3f, 3f);
            fRt.offsetMax = new Vector2(-3f, -3f);
            fillImg = barFill.GetComponent<Image>();
            fillImg.sprite = roundedSprite;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.color = themeConfig != null ? themeConfig.primaryAccent : new Color(0.2f, 0.85f, 1f, 1f);

            // [+] button
            var plusBtn = CreateButton(rowObj.transform, "BtnPlus", "+", new Vector2(1f, 0f), new Vector2(1f, 0f), new Color(0.12f, 0.22f, 0.35f), font, 18);
            var pRt = plusBtn.GetComponent<RectTransform>();
            pRt.anchoredPosition = new Vector2(-25f, 15f);
            pRt.sizeDelta = new Vector2(50f, 30f);
            plusBtn.onClick.AddListener(() => onPlus());
        }

        private void BuildControlsPanel(Transform parent, Font font, Sprite roundedSprite)
        {
            controlsTabPanel = new GameObject("ControlsTabPanel", typeof(RectTransform));
            controlsTabPanel.transform.SetParent(parent, false);
            var rt = controlsTabPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(40f, 80f);
            rt.offsetMax = new Vector2(-40f, -135f);

            float y = -10f;

            // 1. Aim / Steering Mode
            aimModeBtn = CreateButton(controlsTabPanel.transform, "BtnAimMode", "AIM MODE: [MOUSE DIRECT]", new Vector2(0f, 1f), new Vector2(1f, 1f), new Color(0.12f, 0.25f, 0.45f), font, 15);
            var amRt = aimModeBtn.GetComponent<RectTransform>();
            amRt.pivot = new Vector2(0.5f, 1f);
            amRt.anchoredPosition = new Vector2(0f, y);
            amRt.sizeDelta = new Vector2(0f, 36f);
            aimModeBtn.onClick.AddListener(ToggleAimMode);
            aimModeBtnText = aimModeBtn.GetComponentInChildren<Text>();

            y -= 40f;
            var desc1 = CreateText(controlsTabPanel.transform, "AimDesc", "• Ship points towards Mouse Cursor or Gamepad Right Stick.", 12, FontStyle.Normal, new Color(0.75f, 0.85f, 0.95f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, y), new Vector2(-10f, y - 32f), font);
            aimModeDesc = desc1.GetComponent<Text>();

            y -= 44f;
            // 2. Auto-Brake (Inertia Stabilizer)
            autoBrakeBtn = CreateButton(controlsTabPanel.transform, "BtnAutoBrake", "INERTIA STABILIZER: [AUTO-BRAKE ON]", new Vector2(0f, 1f), new Vector2(1f, 1f), new Color(0.12f, 0.25f, 0.45f), font, 15);
            var abRt = autoBrakeBtn.GetComponent<RectTransform>();
            abRt.pivot = new Vector2(0.5f, 1f);
            abRt.anchoredPosition = new Vector2(0f, y);
            abRt.sizeDelta = new Vector2(0f, 36f);
            autoBrakeBtn.onClick.AddListener(ToggleAutoBrake);
            autoBrakeBtnText = autoBrakeBtn.GetComponentInChildren<Text>();

            y -= 40f;
            var desc2 = CreateText(controlsTabPanel.transform, "AutoBrakeDesc", "• Automatically decelerates ship when thrust is released.", 12, FontStyle.Normal, new Color(0.75f, 0.85f, 0.95f), TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, y), new Vector2(-10f, y - 28f), font);
            autoBrakeDesc = desc2.GetComponent<Text>();

            y -= 40f;
            // 3. Invert Steering (A/D) & Touch Controls Row
            invertTurnBtn = CreateButton(controlsTabPanel.transform, "BtnInvertTurn", "STEERING: [NORMAL]", new Vector2(0f, 1f), new Vector2(0.48f, 1f), new Color(0.12f, 0.22f, 0.35f), font, 13);
            var itRt = invertTurnBtn.GetComponent<RectTransform>();
            itRt.pivot = new Vector2(0f, 1f);
            itRt.anchoredPosition = new Vector2(0f, y);
            itRt.sizeDelta = new Vector2(0f, 36f);
            invertTurnBtn.onClick.AddListener(ToggleInvertTurn);
            invertTurnBtnText = invertTurnBtn.GetComponentInChildren<Text>();

            touchHudBtn = CreateButton(controlsTabPanel.transform, "BtnTouchHud", "TOUCH HUD: [AUTO]", new Vector2(0.52f, 1f), new Vector2(1f, 1f), new Color(0.12f, 0.22f, 0.35f), font, 13);
            var thRt = touchHudBtn.GetComponent<RectTransform>();
            thRt.pivot = new Vector2(0f, 1f);
            thRt.anchoredPosition = new Vector2(0f, y);
            thRt.sizeDelta = new Vector2(0f, 36f);
            touchHudBtn.onClick.AddListener(ToggleTouchHud);
            touchHudBtnText = touchHudBtn.GetComponentInChildren<Text>();

            y -= 48f;
            // 4. Quick Key Reference Box
            GameObject keyBox = new GameObject("KeyReferenceBox", typeof(RectTransform), typeof(Image));
            keyBox.transform.SetParent(controlsTabPanel.transform, false);
            var kbRt = keyBox.GetComponent<RectTransform>();
            kbRt.anchorMin = new Vector2(0f, 1f);
            kbRt.anchorMax = new Vector2(1f, 1f);
            kbRt.pivot = new Vector2(0.5f, 1f);
            kbRt.anchoredPosition = new Vector2(0f, y);
            kbRt.sizeDelta = new Vector2(0f, 100f);
            var kbImg = keyBox.GetComponent<Image>();
            kbImg.sprite = roundedSprite;
            kbImg.type = Image.Type.Sliced;
            kbImg.color = new Color(0.06f, 0.09f, 0.15f, 0.90f);

            string keyGuide = "<b>FLIGHT CONTROLS REFERENCE:</b>\n" +
                              "<color=#38BDF8>W / Up / Left Stick</color>: Thrust Forward    •    <color=#F87171>S / Down</color>: Brake\n" +
                              "<color=#38BDF8>A / D</color>: Steer Ship (or Mouse Cursor Aim)    •    <color=#4ADE80>Shift / Right Click</color>: Boost\n" +
                              "<color=#FBBF24>Space / Left Click / RT</color>: Primary Weapon    •    <color=#A78BFA>U</color>: Upgrades Drawer\n" +
                              "<color=#94A3B8>ESC / O</color>: Toggle Settings / Pause";

            CreateText(keyBox.transform, "KeyGuideText", keyGuide, 12, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft,
                Vector2.zero, Vector2.one, new Vector2(16f, 8f), new Vector2(-16f, -8f), font);
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyle style, Color color, TextAnchor align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

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

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, Font font, int fontSize)
        {
            Sprite roundedSprite = ProceduralSpriteHelper.GetRoundedBoxSprite(32, 6);

            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            var rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

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
