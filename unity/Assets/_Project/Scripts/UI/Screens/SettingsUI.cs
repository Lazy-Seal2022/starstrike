using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class SettingsUI : MonoBehaviour, IScreenController
    {
        public static SettingsUI Instance { get; private set; }

        public string ScreenId => "Settings";
        public void Show() => Open();
        public void Hide() => Close();

        public enum SettingsTab
        {
            Audio,
            Display,
            Controls
        }

        [Header("Theme & Visuals")]
        public UIThemeConfig themeConfig;

        [Header("Roots")]
        public GameObject settingsContainer;

        private SettingsTab activeTab = SettingsTab.Audio;
        private GameObject audioTabPanel;
        private GameObject displayTabPanel;
        private GameObject controlsTabPanel;

        private Button audioTabBtn;
        private Button displayTabBtn;
        private Button controlsTabBtn;
        private TMP_Text audioTabBtnText;
        private TMP_Text displayTabBtnText;
        private TMP_Text controlsTabBtnText;

        // Display controls
        private TMP_Text shakeText;
        private Image shakeFill;
        private Button fullscreenBtn;
        private TMP_Text fullscreenBtnText;
        private Button vsyncBtn;
        private TMP_Text vsyncBtnText;
        private Button resolutionBtn;
        private TMP_Text resolutionBtnText;

        // Audio controls
        private TMP_Text masterVolText;
        private Image masterVolFill;
        private TMP_Text sfxVolText;
        private Image sfxVolFill;
        private TMP_Text musicVolText;
        private Image musicVolFill;
        private Button muteBtn;
        private TMP_Text muteBtnText;

        // Controls
        private Button aimModeBtn;
        private TMP_Text aimModeBtnText;
        private TMP_Text aimModeDesc;
        private Button autoBrakeBtn;
        private TMP_Text autoBrakeBtnText;
        private TMP_Text autoBrakeDesc;
        private Button invertTurnBtn;
        private TMP_Text invertTurnBtnText;
        private Button touchHudBtn;
        private TMP_Text touchHudBtnText;
        private Button zoomBtn;
        private TMP_Text zoomBtnText;

        private GameState stateBeforeOpening = GameState.Menu;
        private bool wasMainMenuVisible = false;
        private bool wasLobbyVisible = false;

        public bool IsVisible => settingsContainer != null && settingsContainer.activeSelf;

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

            if (themeConfig == null)
            {
                themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            }

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

            wasLobbyVisible = false;
            var lobby = NetworkLobbyUI.Instance ?? FindAnyObjectByType<NetworkLobbyUI>();
            if (lobby != null && lobby.IsVisible)
            {
                wasLobbyVisible = true;
                lobby.SetVisible(false);
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

            if (wasLobbyVisible)
            {
                var lobby = NetworkLobbyUI.Instance ?? FindAnyObjectByType<NetworkLobbyUI>();
                if (lobby != null)
                {
                    lobby.SetVisible(true);
                }
            }
            else if (wasMainMenuVisible && (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Menu))
            {
                var menu = MainMenuUI.Instance ?? FindAnyObjectByType<MainMenuUI>();
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
            if (displayTabPanel != null) displayTabPanel.SetActive(activeTab == SettingsTab.Display);
            if (controlsTabPanel != null) controlsTabPanel.SetActive(activeTab == SettingsTab.Controls);

            Color activeColor = themeConfig != null ? themeConfig.buttonHover : new Color(0.18f, 0.45f, 0.75f, 1f);
            Color normalColor = themeConfig != null ? themeConfig.buttonNormal : new Color(0.1f, 0.18f, 0.28f, 0.9f);

            if (audioTabBtn != null)
            {
                audioTabBtn.GetComponent<Image>().color = activeTab == SettingsTab.Audio ? activeColor : normalColor;
            }
            if (displayTabBtn != null)
            {
                displayTabBtn.GetComponent<Image>().color = activeTab == SettingsTab.Display ? activeColor : normalColor;
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
                muteBtnText.text = muted ? "AUDIO: MUTED" : "AUDIO: ACTIVE";
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

            float zoom = StorageService.GetCameraZoom();
            if (zoomBtnText != null)
            {
                zoomBtnText.text = $"CAMERA VIEW: [{GetZoomLabel(zoom)}]";
            }

            // Display values
            float shake = StorageService.GetScreenShakeIntensity();
            if (shakeText != null) shakeText.text = $"SCREEN SHAKE: {Mathf.RoundToInt(shake * 100f)}%";
            if (shakeFill != null) shakeFill.fillAmount = shake;

            bool isFs = StorageService.GetFullscreen();
            if (fullscreenBtnText != null)
            {
                fullscreenBtnText.text = isFs ? "DISPLAY MODE: [FULLSCREEN]" : "DISPLAY MODE: [WINDOWED]";
            }

            int vsync = StorageService.GetVSync();
            if (vsyncBtnText != null)
            {
                vsyncBtnText.text = vsync > 0 ? "V-SYNC: [ENABLED (60 FPS)]" : "V-SYNC: [DISABLED]";
            }

            if (resolutionBtnText != null)
            {
                resolutionBtnText.text = $"RESOLUTION: [{GetResolutionLabel()}]";
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

        // --- Display & Juice Event Handlers ---
        public void AdjustScreenShake(float delta)
        {
            float cur = StorageService.GetScreenShakeIntensity();
            float next = Mathf.Clamp01((float)Math.Round(cur + delta, 2));
            StorageService.SaveScreenShakeIntensity(next);
            if (StarStrike.Gameplay.FX.FXService.Instance != null)
            {
                StarStrike.Gameplay.FX.FXService.Instance.ShakeIntensity = next;
            }
            RefreshAllUI();
        }

        public void ToggleFullscreen()
        {
            bool next = !StorageService.GetFullscreen();
            Screen.fullScreen = next;
            StorageService.SaveFullscreen(next);
            RefreshAllUI();
        }

        public void ToggleVSync()
        {
            int cur = StorageService.GetVSync();
            int next = (cur == 0) ? 1 : 0;
            QualitySettings.vSyncCount = next;
            StorageService.SaveVSync(next);
            RefreshAllUI();
        }

        private static readonly (int w, int h)[] SupportedResolutions = new (int, int)[]
        {
            (1920, 1080),
            (2560, 1440),
            (3840, 2160),
            (1280, 720),
            (1600, 900)
        };

        public void CycleResolution()
        {
            int curIdx = StorageService.GetResolutionIndex();
            int nextIdx = (curIdx + 1) % (SupportedResolutions.Length + 1); // 0 = native, 1..N = presets
            StorageService.SaveResolutionIndex(nextIdx);

            if (nextIdx > 0 && nextIdx <= SupportedResolutions.Length)
            {
                var res = SupportedResolutions[nextIdx - 1];
                Screen.SetResolution(res.w, res.h, Screen.fullScreenMode);
            }
            RefreshAllUI();
        }

        private string GetResolutionLabel()
        {
            int idx = StorageService.GetResolutionIndex();
            if (idx == 0 || idx > SupportedResolutions.Length)
            {
                return $"NATIVE ({Screen.width}x{Screen.height})";
            }
            var res = SupportedResolutions[idx - 1];
            return $"{res.w}x{res.h}";
        }

        public void ResetKeybindings()
        {
            if (PlayerInputReader.Instance != null)
            {
                PlayerInputReader.Instance.ResetBindingOverrides();
            }
            AudioService.Instance?.PlayUISound("click");
            RefreshAllUI();
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

        public void CycleZoom()
        {
            float cur = StorageService.GetCameraZoom();
            float next;
            if (cur < 16f) next = 18f;
            else if (cur < 20f) next = 22f;
            else if (cur < 24f) next = 26f;
            else next = 14f;

            StorageService.SaveCameraZoom(next);
            if (CameraController.Instance != null)
            {
                CameraController.Instance.SetBaseZoom(next);
            }
            RefreshAllUI();
        }

        private string GetZoomLabel(float zoom)
        {
            if (zoom <= 15f) return "CLOSE (14.0)";
            if (zoom <= 19f) return "BROAD (18.0)";
            if (zoom <= 23f) return "WIDE (22.0)";
            return "ULTRA-WIDE (26.0)";
        }

        public void ResetDefaults()
        {
            StorageService.ResetSettingsToDefaults();
            if (AudioService.Instance != null) AudioService.Instance.LoadSettings();
            if (PlayerInputReader.Instance != null)
            {
                PlayerInputReader.Instance.ApplySettings();
                PlayerInputReader.Instance.ResetBindingOverrides();
            }
            if (StarStrike.Gameplay.FX.FXService.Instance != null)
            {
                StarStrike.Gameplay.FX.FXService.Instance.ShakeIntensity = 1.0f;
            }
            if (CanvasHUD.Instance != null) CanvasHUD.Instance.forceShowMobileControls = false;
            if (CameraController.Instance != null) CameraController.Instance.SetBaseZoom(18f);
            Screen.fullScreen = true;
            QualitySettings.vSyncCount = 1;
            RefreshAllUI();
        }

        // --- UI Construction ---
        private void BuildUI()
        {
            Sprite roundedSprite = ProceduralSpriteHelper.GetRoundedBoxSprite(32, 6);

            // Container stretches to cover the entire screen so backdrop covers everything behind
            settingsContainer = new GameObject("SettingsContainer", typeof(RectTransform));
            settingsContainer.transform.SetParent(transform, false);

            var cRt = settingsContainer.GetComponent<RectTransform>();
            cRt.anchorMin = Vector2.zero;
            cRt.anchorMax = Vector2.one;
            cRt.offsetMin = Vector2.zero;
            cRt.offsetMax = Vector2.zero;

            // Sorting order for Settings: 500 (renders on top of everything)
            Canvas settingsCanvas = settingsContainer.GetComponent<Canvas>();
            if (settingsCanvas == null) settingsCanvas = settingsContainer.AddComponent<Canvas>();
            settingsCanvas.overrideSorting = true;
            settingsCanvas.sortingOrder = 500;
            if (settingsContainer.GetComponent<GraphicRaycaster>() == null)
            {
                settingsContainer.AddComponent<GraphicRaycaster>();
            }

            // 1. Full-screen Solid Opaque Backdrop (100% covers previous UI, completely prevents text bleed-through)
            GameObject backdropObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
            backdropObj.transform.SetParent(settingsContainer.transform, false);
            var bdRt = backdropObj.GetComponent<RectTransform>();
            bdRt.anchorMin = Vector2.zero;
            bdRt.anchorMax = Vector2.one;
            bdRt.offsetMin = Vector2.zero;
            bdRt.offsetMax = Vector2.zero;
            var bdImg = backdropObj.GetComponent<Image>();
            bdImg.color = new Color(0.02f, 0.03f, 0.06f, 1.0f);
            bdImg.raycastTarget = true;

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
            cardImg.color = new Color(0.05f, 0.08f, 0.14f, 1.0f);

            // 3. Header Section
            CreateTextExplicit(dialogCard.transform, "HeaderTitle", "SYSTEM SETTINGS", 24, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(700f, 36f));

            CreateTextExplicit(dialogCard.transform, "HeaderSub", "Audio Fidelity, Display Juice & Flight Controls Configuration", 12, FontStyles.Normal, new Color(0.6f, 0.8f, 0.95f, 0.85f), TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f), new Vector2(0f, -56f), new Vector2(700f, 20f));

            // Top-Right Close 'X' Button
            var xBtn = CreateButtonExplicit(dialogCard.transform, "BtnCloseX", "X", new Vector2(1f, 1f), new Vector2(-28f, -28f), new Vector2(36f, 36f),
                new Color(0.35f, 0.15f, 0.18f), 16);
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

            audioTabBtn = CreateButtonExplicit(tabNav.transform, "BtnAudioTab", "AUDIO", new Vector2(0.5f, 0.5f), new Vector2(-235f, 0f), new Vector2(225f, 42f),
                new Color(0.18f, 0.45f, 0.75f), 15);
            audioTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Audio));
            audioTabBtnText = audioTabBtn.GetComponentInChildren<TMP_Text>();

            displayTabBtn = CreateButtonExplicit(tabNav.transform, "BtnDisplayTab", "DISPLAY & JUICE", new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(225f, 42f),
                new Color(0.1f, 0.18f, 0.28f), 15);
            displayTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Display));
            displayTabBtnText = displayTabBtn.GetComponentInChildren<TMP_Text>();

            controlsTabBtn = CreateButtonExplicit(tabNav.transform, "BtnControlsTab", "CONTROLS", new Vector2(0.5f, 0.5f), new Vector2(235f, 0f), new Vector2(225f, 42f),
                new Color(0.1f, 0.18f, 0.28f), 15);
            controlsTabBtn.onClick.AddListener(() => SwitchTab(SettingsTab.Controls));
            controlsTabBtnText = controlsTabBtn.GetComponentInChildren<TMP_Text>();

            // 5. Content Area Panels
            BuildAudioPanel(dialogCard.transform, roundedSprite);
            BuildDisplayPanel(dialogCard.transform, roundedSprite);
            BuildControlsPanel(dialogCard.transform, roundedSprite);

            // 6. Footer Section (Bottom 20px)
            GameObject footer = new GameObject("Footer", typeof(RectTransform));
            footer.transform.SetParent(dialogCard.transform, false);
            var fRt = footer.GetComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0.5f, 0f);
            fRt.anchorMax = new Vector2(0.5f, 0f);
            fRt.pivot = new Vector2(0.5f, 0f);
            fRt.anchoredPosition = new Vector2(0f, 20f);
            fRt.sizeDelta = new Vector2(700f, 46f);

            var resetBtn = CreateButtonExplicit(footer.transform, "BtnResetDefaults", "RESET DEFAULTS", new Vector2(0f, 0.5f), new Vector2(105f, 0f), new Vector2(210f, 44f),
                new Color(0.35f, 0.18f, 0.18f), 14);
            resetBtn.onClick.AddListener(ResetDefaults);

            var menuBtn = CreateButtonExplicit(footer.transform, "BtnQuitToMenu", "MAIN MENU", new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(170f, 44f),
                new Color(0.2f, 0.3f, 0.5f), 14);
            menuBtn.onClick.AddListener(() =>
            {
                Close();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadMainMenu();
                }
            });

            var closeBtn = CreateButtonExplicit(footer.transform, "BtnClose", "SAVE & CLOSE [ESC]", new Vector2(1f, 0.5f), new Vector2(-140f, 0f), new Vector2(280f, 44f),
                new Color(0.12f, 0.55f, 0.35f), 15);
            closeBtn.onClick.AddListener(Close);

            // Initially set active tab visibility
            SwitchTab(SettingsTab.Audio);
        }

        private void BuildAudioPanel(Transform parent, Sprite roundedSprite)
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
            CreateVolumeRow(audioTabPanel.transform, "Master", "MASTER VOLUME: 100%", rowY, roundedSprite,
                out masterVolText, out masterVolFill, () => AdjustMasterVolume(-0.1f), () => AdjustMasterVolume(0.1f));

            rowY -= 85f;
            // Row 2: SFX Volume
            CreateVolumeRow(audioTabPanel.transform, "SFX", "SFX VOLUME: 80%", rowY, roundedSprite,
                out sfxVolText, out sfxVolFill, () => AdjustSfxVolume(-0.1f), () => AdjustSfxVolume(0.1f));

            rowY -= 85f;
            // Row 3: Music Volume
            CreateVolumeRow(audioTabPanel.transform, "Music", "MUSIC VOLUME: 70%", rowY, roundedSprite,
                out musicVolText, out musicVolFill, () => AdjustMusicVolume(-0.1f), () => AdjustMusicVolume(0.1f));

            rowY -= 95f;
            // Row 4: Mute Audio Button & Test Sound Button
            muteBtn = CreateButtonExplicit(audioTabPanel.transform, "BtnMute", "AUDIO: ACTIVE", new Vector2(0f, 1f), new Vector2(165f, rowY), new Vector2(330f, 46f),
                new Color(0.12f, 0.35f, 0.25f), 14);
            muteBtn.onClick.AddListener(ToggleMute);
            muteBtnText = muteBtn.GetComponentInChildren<TMP_Text>();

            var testBtn = CreateButtonExplicit(audioTabPanel.transform, "BtnTestSfx", "TEST SFX AUDIO", new Vector2(1f, 1f), new Vector2(-165f, rowY), new Vector2(330f, 46f),
                new Color(0.18f, 0.35f, 0.55f), 14);
            testBtn.onClick.AddListener(PlayTestAudio);
        }

        private void CreateVolumeRow(Transform parent, string prefix, string label, float topY, Sprite roundedSprite,
            out TMP_Text labelText, out Image fillImg, Action onMinus, Action onPlus)
        {
            GameObject rowObj = new GameObject($"Row_{prefix}", typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            var rt = rowObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, topY);
            rt.sizeDelta = new Vector2(700f, 75f);

            labelText = CreateTextExplicit(rowObj.transform, "Label", label, 15, FontStyles.Bold, Color.white, TextAlignmentOptions.Left,
                new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(700f, 24f));

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
                new Color(0.12f, 0.22f, 0.35f), 18);
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
                new Color(0.12f, 0.22f, 0.35f), 18);
            plusBtn.onClick.AddListener(() => onPlus());
        }

        private void BuildDisplayPanel(Transform parent, Sprite roundedSprite)
        {
            displayTabPanel = new GameObject("DisplayTabPanel", typeof(RectTransform));
            displayTabPanel.transform.SetParent(parent, false);
            var rt = displayTabPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -145f);
            rt.sizeDelta = new Vector2(700f, 410f);

            float rowY = -15f;

            // Row 1: Screen Shake Intensity Stepper
            CreateVolumeRow(displayTabPanel.transform, "ScreenShake", "SCREEN SHAKE: 100%", rowY, roundedSprite,
                out shakeText, out shakeFill, () => AdjustScreenShake(-0.1f), () => AdjustScreenShake(0.1f));

            rowY -= 85f;
            // Row 2: Fullscreen Mode Toggle
            fullscreenBtn = CreateButtonExplicit(displayTabPanel.transform, "BtnFullscreen", "DISPLAY MODE: [FULLSCREEN]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 18f), new Vector2(700f, 38f),
                new Color(0.12f, 0.25f, 0.45f), 15);
            fullscreenBtn.onClick.AddListener(ToggleFullscreen);
            fullscreenBtnText = fullscreenBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 48f;
            // Row 3: Resolution Presets Cycle
            resolutionBtn = CreateButtonExplicit(displayTabPanel.transform, "BtnResolution", "RESOLUTION: [NATIVE]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 18f), new Vector2(700f, 38f),
                new Color(0.12f, 0.25f, 0.45f), 15);
            resolutionBtn.onClick.AddListener(CycleResolution);
            resolutionBtnText = resolutionBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 48f;
            // Row 4: V-Sync Toggle
            vsyncBtn = CreateButtonExplicit(displayTabPanel.transform, "BtnVSync", "V-SYNC: [ENABLED (60 FPS)]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 18f), new Vector2(700f, 38f),
                new Color(0.12f, 0.25f, 0.45f), 15);
            vsyncBtn.onClick.AddListener(ToggleVSync);
            vsyncBtnText = vsyncBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 52f;
            // Row 5: Graphics & Post-Processing Status Card
            GameObject postBox = new GameObject("PostProcessingInfoBox", typeof(RectTransform), typeof(Image));
            postBox.transform.SetParent(displayTabPanel.transform, false);
            var pbRt = postBox.GetComponent<RectTransform>();
            pbRt.anchorMin = new Vector2(0.5f, 1f);
            pbRt.anchorMax = new Vector2(0.5f, 1f);
            pbRt.pivot = new Vector2(0.5f, 1f);
            pbRt.anchoredPosition = new Vector2(0f, rowY);
            pbRt.sizeDelta = new Vector2(700f, 85f);

            var pbImg = postBox.GetComponent<Image>();
            pbImg.sprite = roundedSprite;
            pbImg.type = Image.Type.Sliced;
            pbImg.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);

            string postGuide = "<b>URP 2D RENDERING & POST-PROCESSING PIPELINE:</b>\n" +
                               "<color=#38BDF8>Bloom (Intensity 1.2, Threshold 0.9)</color>: Emissive Neon Glow on Lasers & Gems\n" +
                               "<color=#4ADE80>Dynamic Batching</color>: Zero real-time point-light batch breaks in combat loop\n" +
                               "<color=#FBBF24>Zero-Allocation FX</color>: Pre-allocated particle & audio source ring buffer pool";

            CreateTextExplicit(postBox.transform, "PostGuideText", postGuide, 12, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 75f));
        }

        private void BuildControlsPanel(Transform parent, Sprite roundedSprite)
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
                new Color(0.12f, 0.25f, 0.45f), 15);
            aimModeBtn.onClick.AddListener(ToggleAimMode);
            aimModeBtnText = aimModeBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 44f;
            aimModeDesc = CreateTextExplicit(controlsTabPanel.transform, "AimDesc", "• Ship points towards Mouse Cursor or Gamepad Stick. Press W to thrust forward.", 12, FontStyles.Normal, new Color(0.75f, 0.88f, 0.98f), TextAlignmentOptions.Left,
                new Vector2(0.5f, 1f), new Vector2(0f, rowY - 15f), new Vector2(680f, 30f));

            rowY -= 45f;
            // 2. Auto-Brake (Inertia Stabilizer) Card
            autoBrakeBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnAutoBrake", "INERTIA STABILIZER: [AUTO-BRAKE ON]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 18f), new Vector2(700f, 38f),
                new Color(0.12f, 0.25f, 0.45f), 15);
            autoBrakeBtn.onClick.AddListener(ToggleAutoBrake);
            autoBrakeBtnText = autoBrakeBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 44f;
            autoBrakeDesc = CreateTextExplicit(controlsTabPanel.transform, "AutoBrakeDesc", "• Automatically decelerates ship when thrust is released.", 12, FontStyles.Normal, new Color(0.75f, 0.88f, 0.98f), TextAlignmentOptions.Left,
                new Vector2(0.5f, 1f), new Vector2(0f, rowY - 15f), new Vector2(680f, 30f));

            rowY -= 45f;
            // 3. Camera Zoom View Card
            zoomBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnZoom", "CAMERA VIEW: [BROAD (18.0)]", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 16f), new Vector2(700f, 36f),
                new Color(0.14f, 0.28f, 0.45f), 14);
            zoomBtn.onClick.AddListener(CycleZoom);
            zoomBtnText = zoomBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 44f;
            // 4. Quick Toggles Row (Side-by-side)
            invertTurnBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnInvertTurn", "STEERING: [NORMAL]", new Vector2(0f, 1f), new Vector2(165f, rowY - 16f), new Vector2(330f, 36f),
                new Color(0.12f, 0.22f, 0.35f), 13);
            invertTurnBtn.onClick.AddListener(ToggleInvertTurn);
            invertTurnBtnText = invertTurnBtn.GetComponentInChildren<TMP_Text>();

            touchHudBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnTouchHud", "ON-SCREEN HUD: [AUTO / MOBILE]", new Vector2(1f, 1f), new Vector2(-165f, rowY - 16f), new Vector2(330f, 36f),
                new Color(0.12f, 0.22f, 0.35f), 13);
            touchHudBtn.onClick.AddListener(ToggleTouchHud);
            touchHudBtnText = touchHudBtn.GetComponentInChildren<TMP_Text>();

            rowY -= 44f;
            // 5. Quick Key Reference Card
            GameObject keyBox = new GameObject("KeyReferenceBox", typeof(RectTransform), typeof(Image));
            keyBox.transform.SetParent(controlsTabPanel.transform, false);
            var kbRt = keyBox.GetComponent<RectTransform>();
            kbRt.anchorMin = new Vector2(0.5f, 1f);
            kbRt.anchorMax = new Vector2(0.5f, 1f);
            kbRt.pivot = new Vector2(0.5f, 1f);
            kbRt.anchoredPosition = new Vector2(0f, rowY);
            kbRt.sizeDelta = new Vector2(700f, 82f);

            var kbImg = keyBox.GetComponent<Image>();
            kbImg.sprite = roundedSprite;
            kbImg.type = Image.Type.Sliced;
            kbImg.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);

            string keyGuide = "<b>FLIGHT CONTROLS & CAMERA:</b>\n" +
                              "<color=#38BDF8>W / Up / Left Stick</color>: Thrust    •    <color=#F87171>S / Down</color>: Brake    •    <color=#38BDF8>Mouse Wheel</color>: Zoom\n" +
                              "<color=#38BDF8>A / D Keys</color>: Steer Ship (or Mouse Aim)      •    <color=#4ADE80>Shift / Right Click</color>: Boost Engines\n" +
                              "<color=#FBBF24>Space / Left Click / RT</color>: Fire Weapon     •    <color=#A78BFA>U Key</color>: Upgrades Drawer";

            CreateTextExplicit(keyBox.transform, "KeyGuideText", keyGuide, 12, FontStyles.Normal, Color.white, TextAlignmentOptions.Left,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660f, 74f));

            // 6. Reset Keybindings Button
            var resetBindingsBtn = CreateButtonExplicit(controlsTabPanel.transform, "BtnResetBindings", "RESET KEYBINDINGS TO DEFAULT", new Vector2(0.5f, 1f), new Vector2(0f, rowY - 96f), new Vector2(700f, 32f),
                new Color(0.24f, 0.14f, 0.20f), 13);
            resetBindingsBtn.onClick.AddListener(ResetKeybindings);
        }

        // --- Explicit Helpers (Safe RectTransform geometry) ---
        private TextMeshProUGUI CreateTextExplicit(Transform parent, string name, string text, int fontSize, FontStyles style, Color color, TextAlignmentOptions align,
            Vector2 anchor, Vector2 pos, Vector2 size)
        {
            return UIHelper.CreateTextMeshProExplicit(parent, name, text, UIHelper.ScaleFontSize(fontSize), style, color, align, anchor, pos, size);
        }

        private Button CreateButtonExplicit(Transform parent, string name, string label, Vector2 anchor, Vector2 pos, Vector2 size, Color color, int fontSize)
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
            btn.onClick.AddListener(() =>
            {
                AudioService.Instance?.PlayUISound("click");
            });

            UIHelper.CreateTextMeshPro(obj.transform, "Text", label, fontSize, FontStyles.Bold, Color.white, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return btn;
        }
    }
}
