using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class CanvasHUD : MonoBehaviour, IScreenController
    {
        public static CanvasHUD Instance { get; private set; }

        public string ScreenId => "HUD";
        public bool IsVisible { get; private set; }

        public void Show() => SetHUDVisible(true);
        public void Hide() => SetHUDVisible(false);

        [Header("Configuration")]
        public bool forceShowMobileControls = false;

        [Header("Modular Sub-Controllers")]
        public HUDPlayerStatus playerStatus;
        public HUDEvolutionBanner evolutionBannerController;
        public HUDUpgradeDrawer upgradeDrawerController;
        public HUDFloatingText floatingTextController;
        public HUDEndGameModal endGameModal;

        [Header("UI References (Optional - Auto-built if null)")]
        public Canvas canvas;
        public Text shipNameText;
        public Text gemsText;
        public Image shieldFill;
        public Text shieldValueText;
        public Image hullFill;
        public Text hullValueText;
        public Image energyFill;
        public Text energyValueText;
        public GameObject upgradesDrawer;
        public Button toggleUpgradesButton;
        public Text toggleUpgradesButtonText;
        public GameObject evolutionBanner;
        public Button evolveFighterButton;
        public Button evolveMinerButton;
        public GameObject mobileControlsRoot;
        public HUDSkillSlotView skillSlotView;

        private float currentShield = 100f;
        private float maxShield = 100f;
        private float currentHull = 80f;
        private float maxHull = 80f;
        private float currentEnergy = 80f;
        private float maxEnergy = 80f;
        private bool isEnergyLockedOut = false;
        private float energyRecoveryTarget = 0f;
        private bool justOverdrawn = false;
        private int totalGems = 0;
        private bool isUpgradesOpen = false;
        private bool hasAppliedVisibility = false;
        private int lastShieldCurInt = int.MinValue;
        private int lastShieldMaxInt = int.MinValue;
        private int lastHullCurInt = int.MinValue;
        private int lastHullMaxInt = int.MinValue;
        private int lastEnergyCurInt = int.MinValue;
        private int lastEnergyMaxInt = int.MinValue;
        private int lastEnergyRecInt = int.MinValue;
        private bool lastEnergyLockedVisual = false;
        private bool hasEnergyVisualInit = false;
        private int lastGemsInt = int.MinValue;
        private int lastUpgradeGems = int.MinValue;
        private int lastUpgradeLevelSum = int.MinValue;
        private bool forceRefreshUpgrades = true;
        
        private GameObject gameplayContainer;

        private readonly struct UpgradeDefinition
        {
            public readonly string Key;
            public readonly string Label;
            public UpgradeDefinition(string key, string label) { Key = key; Label = label; }
        }

        private static readonly UpgradeDefinition[] StatUpgrades = new[]
        {
            new UpgradeDefinition("shieldCap", "1. Shield Cap"),
            new UpgradeDefinition("shieldRegen", "2. Shield Regen"),
            new UpgradeDefinition("energyCap", "3. Energy Cap"),
            new UpgradeDefinition("energyRegen", "4. Energy Regen"),
            new UpgradeDefinition("damage", "5. Pulse Damage"),
            new UpgradeDefinition("pulseSpeed", "6. Pulse Speed"),
            new UpgradeDefinition("speed", "7. Engine Speed"),
            new UpgradeDefinition("agility", "8. Turn Agility")
        };

        private readonly Dictionary<string, Text> upgradeRowLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, Button> upgradeRowButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> upgradeRowButtonTexts = new Dictionary<string, Text>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureEventSystem();
            BuildCanvasIfNeeded();
            SetHUDVisible(false);

            if (FindAnyObjectByType<AchievementUI>() == null)
            {
                new GameObject("AchievementUI").AddComponent<AchievementUI>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.RegisterScreen(this);
            }
            GameEvents.OnShieldChanged += HandleShieldChanged;
            GameEvents.OnHullChanged += HandleHullChanged;
            GameEvents.OnEnergyChanged += HandleEnergyChanged;
            GameEvents.OnEnergyLockoutChanged += HandleEnergyLockoutChanged;
            GameEvents.OnCapacitorOverdrawn += HandleCapacitorOverdrawn;
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnWaveStarted += HandleWaveStarted;
            GameEvents.OnWaveCompleted += HandleWaveCompleted;
            NetworkLobbyUI.OnLobbyVisibilityChanged += HandleLobbyVisibilityChanged;
        }

        private void OnDisable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.UnregisterScreen(this);
            }
            GameEvents.OnShieldChanged -= HandleShieldChanged;
            GameEvents.OnHullChanged -= HandleHullChanged;
            GameEvents.OnEnergyChanged -= HandleEnergyChanged;
            GameEvents.OnEnergyLockoutChanged -= HandleEnergyLockoutChanged;
            GameEvents.OnCapacitorOverdrawn -= HandleCapacitorOverdrawn;
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnWaveStarted -= HandleWaveStarted;
            GameEvents.OnWaveCompleted -= HandleWaveCompleted;
            NetworkLobbyUI.OnLobbyVisibilityChanged -= HandleLobbyVisibilityChanged;
        }

        private void HandleWaveStarted(int waveNumber, string waveName, float duration)
        {
            if (floatingTextController != null)
            {
                floatingTextController.SpawnFloatingNotice($"WAVE {waveNumber}: {waveName.ToUpper()}", new Color(0.38f, 0.85f, 1f));
            }
        }

        private void HandleWaveCompleted(int waveNumber, int rewardGems)
        {
            if (floatingTextController != null)
            {
                floatingTextController.SpawnFloatingNotice($"WAVE {waveNumber} CLEARED! +{rewardGems} GEMS", new Color(0.3f, 0.95f, 0.5f));
            }
        }

        private void HandleLobbyVisibilityChanged(bool isLobbyVisible)
        {
            if (isLobbyVisible)
            {
                SetHUDVisible(false);
            }
        }

        public void SetHUDVisible(bool visible)
        {
            IsVisible = visible;
            hasAppliedVisibility = true;
            foreach (Transform child in transform)
            {
                if (child.name == "MainMenuContainer" ||
                    child.name == "FloatingTextContainer" ||
                    child.name == "NetworkLobbyContainer" ||
                    child.name == "NetworkScoreboardContainer" ||
                    child.name == "NetworkVictoryModal" ||
                    child.name == "ArenaVictoryModal" ||
                    child.name == "SettingsContainer")
                {
                    continue;
                }

                if (child.name == "EndGameModalPanel" || (endGameModal != null && child.gameObject == endGameModal.modalRoot))
                {
                    if (!visible) child.gameObject.SetActive(false);
                    continue;
                }

                if (child.name == "UpgradesDrawer")
                {
                    child.gameObject.SetActive(visible && isUpgradesOpen);
                }
                else if (child.name == "EvolutionBanner")
                {
                    bool canEvolve = PlayerProgression.Instance != null && PlayerProgression.Instance.CanEvolve();
                    child.gameObject.SetActive(visible && canEvolve);
                }
                else if (child.name == "MobileControlsRoot")
                {
                    bool showMobile = forceShowMobileControls || Application.isMobilePlatform || (Touchscreen.current != null && Touchscreen.current.wasUpdatedThisFrame);
                    child.gameObject.SetActive(visible && showMobile);
                }
                else
                {
                    child.gameObject.SetActive(visible);
                }
            }
        }

        private void HandleGameStarted()
        {
            SetHUDVisible(true);
        }

        private void Start()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.RegisterScreen(this);
            }
            SyncPlayerInitialState();
            UpdateAllHUDVisuals();
            bool isPlaying = GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing;
            SetHUDVisible(isPlaying);
        }

        private void Update()
        {
            if (GameManager.Instance != null)
            {
                bool isOverlayOpen = (SettingsUI.Instance != null && SettingsUI.Instance.IsVisible) ||
                                     (MainMenuUI.Instance != null && MainMenuUI.Instance.IsVisible) ||
                                     (NetworkLobbyUI.Instance != null && NetworkLobbyUI.Instance.IsVisible);

                bool isPlaying = GameManager.Instance.CurrentState == GameState.Playing && !isOverlayOpen;
                if (!hasAppliedVisibility || IsVisible != isPlaying)
                {
                    SetHUDVisible(isPlaying);
                }
                if (!isPlaying) return;
            }

            // Keyboard shortcut [U] for Upgrades Drawer
            if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
            {
                ToggleUpgrades();
            }

            // Keyboard shortcut [M] to toggle Mobile Controls overlay (useful for testing on desktop)
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                forceShowMobileControls = !forceShowMobileControls;
                if (mobileControlsRoot != null)
                {
                    bool show = forceShowMobileControls || Application.isMobilePlatform || Touchscreen.current != null;
                    mobileControlsRoot.SetActive(show);
                }
            }

            // Periodically refresh upgrade buttons & evolution banner
            UpdateEvolutionBanner();
            if (isUpgradesOpen)
            {
                UpdateUpgradeRows();
            }
        }

        private void SyncPlayerInitialState()
        {
            PlayerProgression player = PlayerProgression.Instance;
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<PlayerProgression>();
            }

            if (player != null && player.currentShip != null)
            {
                maxShield = player.currentShip.baseStats.shieldCapacity;
                currentShield = player.health != null && player.health.currentShield > 0f ? player.health.currentShield : maxShield;
                maxHull = player.currentShip.baseStats.hullCapacity > 0f ? player.currentShip.baseStats.hullCapacity : 80f;
                currentHull = player.health != null && player.health.currentHull > 0f ? player.health.currentHull : maxHull;
                maxEnergy = player.currentShip.baseStats.energyCapacity;
                currentEnergy = player.health != null && player.health.currentEnergy > 0f ? player.health.currentEnergy : maxEnergy;
                totalGems = player.gems;
                UpdateShipHeader(player.currentShip.tier, player.currentShip.shipName);
            }
        }

        private void HandleShieldChanged(float current, float max)
        {
            currentShield = current;
            maxShield = max;
            UpdateShieldVisuals();
            if (playerStatus != null) playerStatus.HandleShieldChanged(current, max);
        }

        private void HandleHullChanged(float current, float max)
        {
            currentHull = current;
            maxHull = max;
            UpdateHullVisuals();
            if (playerStatus != null) playerStatus.HandleHullChanged(current, max);
        }

        private void HandleEnergyChanged(float current, float max)
        {
            currentEnergy = current;
            maxEnergy = max;
            UpdateEnergyVisuals();
            if (playerStatus != null) playerStatus.HandleEnergyChanged(current, max);
        }

        private void HandleCapacitorOverdrawn()
        {
            justOverdrawn = true;
            if (floatingTextController != null)
            {
                floatingTextController.SpawnFloatingNotice("OVERDRAW! CAPACITOR DEPLETED", new Color(1f, 0.35f, 0.1f));
            }
        }

        private void HandleEnergyLockoutChanged(bool isLocked, float current, float target)
        {
            isEnergyLockedOut = isLocked;
            energyRecoveryTarget = target;
            UpdateEnergyVisuals();

            if (floatingTextController != null)
            {
                if (isLocked)
                {
                    if (!justOverdrawn)
                    {
                        floatingTextController.SpawnFloatingNotice("CAPACITOR DEPLETED // SYSTEM REBOOT", new Color(1f, 0.45f, 0.1f));
                    }
                    justOverdrawn = false;
                }
                else
                {
                    floatingTextController.SpawnFloatingNotice("CAPACITOR RESTORED // WEAPONS ONLINE", new Color(0.2f, 0.9f, 0.45f));
                }
            }
        }

        private void HandleGemCollected(int added, int total)
        {
            totalGems = total;
            if (gemsText != null && lastGemsInt != totalGems)
            {
                lastGemsInt = totalGems;
                gemsText.text = $"<color=#4ade80><b>{totalGems}</b></color> GEMS";
            }
            if (playerStatus != null) playerStatus.HandleGemCollected(added, total);
            if (isUpgradesOpen)
            {
                forceRefreshUpgrades = true;
                UpdateUpgradeRows();
            }
        }

        private void HandlePlayerEvolved(string shipId)
        {
            lastEvoShipId = "";
            forceRefreshUpgrades = true;
            PlayerProgression player = PlayerProgression.Instance;
            if (player != null && player.currentShip != null)
            {
                UpdateShipHeader(player.currentShip.tier, player.currentShip.shipName);
                currentShield = player.currentShip.baseStats.shieldCapacity;
                maxShield = player.currentShip.baseStats.shieldCapacity;
                currentHull = player.currentShip.baseStats.hullCapacity > 0f ? player.currentShip.baseStats.hullCapacity : 80f;
                maxHull = currentHull;
                currentEnergy = player.currentShip.baseStats.energyCapacity;
                maxEnergy = player.currentShip.baseStats.energyCapacity;
                UpdateShieldVisuals();
                UpdateHullVisuals();
                UpdateEnergyVisuals();
            }
            if (playerStatus != null) playerStatus.HandlePlayerEvolved(shipId);
            UpdateEvolutionBanner();
        }

        private void UpdateShipHeader(int tier, string shipName)
        {
            if (shipNameText != null)
            {
                shipNameText.text = $"<color=#38bdf8><b>T{tier} • {shipName.ToUpper()}</b></color>";
            }
        }

        private void UpdateShieldVisuals()
        {
            float ratio = Mathf.Clamp01(currentShield / Mathf.Max(1f, maxShield));
            if (shieldFill != null) shieldFill.fillAmount = ratio;
            if (shieldValueText != null)
            {
                int curInt = Mathf.RoundToInt(currentShield);
                int maxInt = Mathf.RoundToInt(maxShield);
                if (curInt != lastShieldCurInt || maxInt != lastShieldMaxInt)
                {
                    lastShieldCurInt = curInt;
                    lastShieldMaxInt = maxInt;
                    shieldValueText.text = $"{curInt} / {maxInt}";
                }
            }
        }

        private void UpdateHullVisuals()
        {
            float ratio = Mathf.Clamp01(currentHull / Mathf.Max(1f, maxHull));
            if (hullFill != null) hullFill.fillAmount = ratio;
            if (hullValueText != null)
            {
                int curInt = Mathf.RoundToInt(currentHull);
                int maxInt = Mathf.RoundToInt(maxHull);
                if (curInt != lastHullCurInt || maxInt != lastHullMaxInt)
                {
                    lastHullCurInt = curInt;
                    lastHullMaxInt = maxInt;
                    hullValueText.text = $"{curInt} / {maxInt}";
                }
            }
        }

        private void UpdateEnergyVisuals()
        {
            float ratio = Mathf.Clamp01(currentEnergy / Mathf.Max(1f, maxEnergy));
            if (energyFill != null)
            {
                energyFill.fillAmount = ratio;
                if (!hasEnergyVisualInit || lastEnergyLockedVisual != isEnergyLockedOut)
                {
                    energyFill.color = isEnergyLockedOut
                        ? new Color(1f, 0.55f, 0.1f, 0.95f)
                        : new Color(0.18f, 0.8f, 0.44f, 1f);
                }
            }
            if (energyValueText != null)
            {
                int curInt = Mathf.RoundToInt(currentEnergy);
                int maxInt = Mathf.RoundToInt(maxEnergy);
                int recInt = Mathf.RoundToInt(energyRecoveryTarget);
                if (!hasEnergyVisualInit || lastEnergyLockedVisual != isEnergyLockedOut ||
                    curInt != lastEnergyCurInt ||
                    (isEnergyLockedOut ? recInt != lastEnergyRecInt : maxInt != lastEnergyMaxInt))
                {
                    lastEnergyCurInt = curInt;
                    lastEnergyMaxInt = maxInt;
                    lastEnergyRecInt = recInt;
                    energyValueText.text = isEnergyLockedOut
                        ? $"<color=#ff8514>REBOOTING [{curInt}/{recInt}]</color>"
                        : $"{curInt} / {maxInt}";
                }
            }
            lastEnergyLockedVisual = isEnergyLockedOut;
            hasEnergyVisualInit = true;
        }

        private void UpdateAllHUDVisuals()
        {
            UpdateShieldVisuals();
            UpdateHullVisuals();
            UpdateEnergyVisuals();
            if (gemsText != null && lastGemsInt != totalGems)
            {
                lastGemsInt = totalGems;
                gemsText.text = $"<color=#4ade80><b>{totalGems}</b></color> GEMS";
            }
            UpdateEvolutionBanner();
            forceRefreshUpgrades = true;
            UpdateUpgradeRows();
        }

        public void ToggleUpgrades()
        {
            isUpgradesOpen = !isUpgradesOpen;
            if (upgradesDrawer != null)
            {
                upgradesDrawer.SetActive(isUpgradesOpen);
            }
            if (toggleUpgradesButtonText != null)
            {
                toggleUpgradesButtonText.text = isUpgradesOpen ? "HIDE [U]" : "UPGRADES [U]";
            }
            if (isUpgradesOpen)
            {
                forceRefreshUpgrades = true;
                UpdateUpgradeRows();
            }
        }

        private void UpdateUpgradeRows()
        {
            PlayerProgression player = PlayerProgression.Instance;
            if (player == null) return;

            var s = player.upgradeState;
            int levelSum = s.shieldCapacityLevel + s.shieldRegenLevel + s.energyCapacityLevel + s.energyRegenLevel +
                           s.pulseDamageLevel + s.pulseSpeedLevel + s.speedLevel + s.agilityLevel;
            if (!forceRefreshUpgrades && lastUpgradeGems == player.gems && lastUpgradeLevelSum == levelSum)
            {
                return;
            }
            forceRefreshUpgrades = false;
            lastUpgradeGems = player.gems;
            lastUpgradeLevelSum = levelSum;

            foreach (UpgradeDefinition def in StatUpgrades)
            {
                int cost = player.GetUpgradeCost(def.Key);
                int lvl = GetStatLevel(player, def.Key);

                if (upgradeRowLabels.TryGetValue(def.Key, out Text lbl) && lbl != null)
                {
                    lbl.text = $"{def.Label} (Lv.{lvl})";
                }

                if (upgradeRowButtonTexts.TryGetValue(def.Key, out Text btnTxt) && btnTxt != null)
                {
                    btnTxt.text = $"+ {cost} G";
                }

                if (upgradeRowButtons.TryGetValue(def.Key, out Button btn) && btn != null)
                {
                    btn.interactable = (player.gems >= cost);
                }
            }
        }

        private int GetStatLevel(PlayerProgression player, string key)
        {
            switch (key)
            {
                case "shieldCap": return player.upgradeState.shieldCapacityLevel;
                case "shieldRegen": return player.upgradeState.shieldRegenLevel;
                case "energyCap": return player.upgradeState.energyCapacityLevel;
                case "energyRegen": return player.upgradeState.energyRegenLevel;
                case "damage": return player.upgradeState.pulseDamageLevel;
                case "pulseSpeed": return player.upgradeState.pulseSpeedLevel;
                case "speed": return player.upgradeState.speedLevel;
                case "agility": return player.upgradeState.agilityLevel;
                default: return 0;
            }
        }

        private void OnUpgradeClicked(string key)
        {
            PlayerProgression player = PlayerProgression.Instance;
            if (player != null && player.UpgradeStat(key))
            {
                UpdateUpgradeRows();
                UpdateEvolutionBanner();
            }
        }

        private GameObject evolutionButtonsContainer;
        private readonly List<GameObject> activeEvoButtons = new List<GameObject>();
        private string lastEvoShipId = "";

        private void UpdateEvolutionBanner()
        {
            if (evolutionBanner == null) return;

            PlayerProgression player = PlayerProgression.Instance;
            bool canEvolve = player != null && player.CanEvolve();

            if (canEvolve)
            {
                string curId = player.currentShip != null ? player.currentShip.id : "";
                if (!evolutionBanner.activeSelf || lastEvoShipId != curId)
                {
                    lastEvoShipId = curId;
                    RebuildEvolutionButtons(player);
                    evolutionBanner.SetActive(true);
                }
            }
            else
            {
                if (evolutionBanner.activeSelf)
                {
                    evolutionBanner.SetActive(false);
                }
            }
        }

        public void RebuildEvolutionButtons(PlayerProgression player)
        {
            if (player == null || player.currentShip == null || player.currentShip.evolvesTo == null) return;

            Transform parent = evolutionButtonsContainer != null ? evolutionButtonsContainer.transform : (evolutionBanner != null ? evolutionBanner.transform : transform);

            for (int i = activeEvoButtons.Count - 1; i >= 0; i--)
            {
                if (activeEvoButtons[i] != null)
                {
                    if (Application.isPlaying) Destroy(activeEvoButtons[i]);
                    else DestroyImmediate(activeEvoButtons[i]);
                }
            }
            activeEvoButtons.Clear();

            if (evolveFighterButton != null && evolveFighterButton.gameObject != null && !activeEvoButtons.Contains(evolveFighterButton.gameObject))
            {
                evolveFighterButton.gameObject.SetActive(false);
            }
            if (evolveMinerButton != null && evolveMinerButton.gameObject != null && !activeEvoButtons.Contains(evolveMinerButton.gameObject))
            {
                evolveMinerButton.gameObject.SetActive(false);
            }

            var targets = player.currentShip.evolvesTo;
            int count = targets.Length;
            if (count == 0) return;

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sprite roundedSprite = ProceduralSpriteHelper.GetRadarSprite(32);

            for (int i = 0; i < count; i++)
            {
                var target = targets[i];
                if (target == null) continue;

                Vector2 pos;
                Vector2 size;
                Vector2 anchorMin;
                Vector2 anchorMax;

                if (count == 1)
                {
                    anchorMin = new Vector2(0.5f, 1f);
                    anchorMax = new Vector2(0.5f, 1f);
                    pos = new Vector2(-120f, -42f);
                    size = new Vector2(240f, 40f);
                }
                else
                {
                    if (i == 0)
                    {
                        anchorMin = new Vector2(0f, 1f);
                        anchorMax = new Vector2(0f, 1f);
                        pos = new Vector2(20f, -42f);
                        size = new Vector2(190f, 40f);
                    }
                    else
                    {
                        anchorMin = new Vector2(1f, 1f);
                        anchorMax = new Vector2(1f, 1f);
                        pos = new Vector2(-210f, -42f);
                        size = new Vector2(190f, 40f);
                    }
                }

                Color btnColor = GetEvoButtonColor(target.id);
                string label = target.shipName.ToUpper();

                GameObject btnObj = CreateButton(parent, $"EvoBtn_{target.id}", label, anchorMin, anchorMax, pos, size, btnColor, roundedSprite, defaultFont);
                Button btn = btnObj.GetComponent<Button>();
                string targetId = target.id;
                btn.onClick.AddListener(() =>
                {
                    if (PlayerProgression.Instance != null)
                    {
                        PlayerProgression.Instance.EvolveToId(targetId);
                        UpdateEvolutionBanner();
                    }
                });

                if (targetId == "fighter") evolveFighterButton = btn;
                if (targetId == "miner") evolveMinerButton = btn;

                activeEvoButtons.Add(btnObj);
            }
        }

        private Color GetEvoButtonColor(string id)
        {
            switch (id)
            {
                case "fighter": return new Color(0.85f, 0.2f, 0.25f, 1f);
                case "miner": return new Color(0.9f, 0.55f, 0.1f, 1f);
                case "interceptor": return new Color(0.35f, 0.4f, 0.95f, 1f);
                case "juggernaut": return new Color(0.95f, 0.45f, 0.15f, 1f);
                default: return new Color(0.2f, 0.6f, 0.85f, 1f);
            }
        }

        private void EnsureEventSystem()
        {
            EventSystemSingleton.EnforceSingleEventSystem();
        }

        private void BuildCanvasIfNeeded()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
                if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

            Sprite whiteSprite = ProceduralSpriteHelper.GetWhiteSprite();
            Sprite circleSprite = ProceduralSpriteHelper.GetCircleSprite(64);
            Sprite roundedSprite = ProceduralSpriteHelper.GetRoundedBoxSprite(32, 6);

            // 1. Top-Left Status Panel (Isolated into a sub-canvas to prevent frequent fill updates from rebuilding root canvas mesh)
            GameObject statusCard = CreatePanel(transform, "StatusPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(360f, 225f), new Color(0.04f, 0.08f, 0.16f, 0.88f), roundedSprite);
            Canvas statusCanvas = statusCard.GetComponent<Canvas>();
            if (statusCanvas == null) statusCanvas = statusCard.AddComponent<Canvas>();
            Canvas rootCanvas = GetComponent<Canvas>();
            if (rootCanvas != null && rootCanvas != statusCanvas)
            {
                statusCanvas.overrideSorting = true;
                statusCanvas.sortingLayerID = rootCanvas.sortingLayerID;
                statusCanvas.sortingOrder = rootCanvas.sortingOrder + 1;
            }

            // Ship Name
            GameObject nameObj = CreateText(statusCard.transform, "ShipNameText", "T1 • DELTA SCOUT", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -10f), new Vector2(-16f, -32f), defaultFont);
            shipNameText = nameObj.GetComponent<Text>();

            // Gem Count
            GameObject gemsObj = CreateText(statusCard.transform, "GemsText", "0 GEMS", 15, FontStyle.Bold, new Color(0.3f, 0.95f, 0.5f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -34f), new Vector2(-16f, -54f), defaultFont);
            gemsText = gemsObj.GetComponent<Text>();

            // Shield Bar
            CreateText(statusCard.transform, "ShieldLabel", "SHIELD", 11, FontStyle.Bold, new Color(0.6f, 0.75f, 0.9f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -58f), new Vector2(70f, -74f), defaultFont);
            GameObject shieldBg = CreatePanel(statusCard.transform, "ShieldBg", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(75f, -60f), new Vector2(190f, 13f), new Color(0.08f, 0.15f, 0.25f, 1f), roundedSprite);
            GameObject shieldFillObj = CreateImage(shieldBg.transform, "Fill", new Color(0.22f, 0.74f, 0.97f), roundedSprite, Image.Type.Filled);
            shieldFill = shieldFillObj.GetComponent<Image>();
            shieldFill.fillMethod = Image.FillMethod.Horizontal;
            shieldFill.fillOrigin = 0;
            shieldFill.fillAmount = 1f;
            GameObject shieldValObj = CreateText(statusCard.transform, "ShieldVal", "100 / 100", 11, FontStyle.Normal, Color.white, TextAnchor.MiddleRight, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -58f), new Vector2(345f, -74f), defaultFont);
            shieldValueText = shieldValObj.GetComponent<Text>();

            // Hull Bar
            CreateText(statusCard.transform, "HullLabel", "HULL", 11, FontStyle.Bold, new Color(0.55f, 0.88f, 0.65f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -77f), new Vector2(70f, -93f), defaultFont);
            GameObject hullBg = CreatePanel(statusCard.transform, "HullBg", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(75f, -79f), new Vector2(190f, 13f), new Color(0.06f, 0.20f, 0.12f, 1f), roundedSprite);
            GameObject hullFillObj = CreateImage(hullBg.transform, "Fill", new Color(0.25f, 0.88f, 0.42f), roundedSprite, Image.Type.Filled);
            hullFill = hullFillObj.GetComponent<Image>();
            hullFill.fillMethod = Image.FillMethod.Horizontal;
            hullFill.fillOrigin = 0;
            hullFill.fillAmount = 1f;
            GameObject hullValObj = CreateText(statusCard.transform, "HullVal", "80 / 80", 11, FontStyle.Normal, Color.white, TextAnchor.MiddleRight, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -77f), new Vector2(345f, -93f), defaultFont);
            hullValueText = hullValObj.GetComponent<Text>();

            // Energy Bar
            CreateText(statusCard.transform, "EnergyLabel", "ENERGY", 11, FontStyle.Bold, new Color(0.95f, 0.75f, 0.4f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -96f), new Vector2(70f, -112f), defaultFont);
            GameObject energyBg = CreatePanel(statusCard.transform, "EnergyBg", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(75f, -98f), new Vector2(190f, 13f), new Color(0.25f, 0.15f, 0.05f, 1f), roundedSprite);
            GameObject energyFillObj = CreateImage(energyBg.transform, "Fill", new Color(0.96f, 0.62f, 0.04f), roundedSprite, Image.Type.Filled);
            energyFill = energyFillObj.GetComponent<Image>();
            energyFill.fillMethod = Image.FillMethod.Horizontal;
            energyFill.fillOrigin = 0;
            energyFill.fillAmount = 1f;
            GameObject energyValObj = CreateText(statusCard.transform, "EnergyVal", "80 / 80", 11, FontStyle.Normal, Color.white, TextAnchor.MiddleRight, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -96f), new Vector2(345f, -112f), defaultFont);
            energyValueText = energyValObj.GetComponent<Text>();

            // Toggle Upgrades Button
            GameObject toggleBtnObj = CreateButton(statusCard.transform, "ToggleUpgradesBtn", "UPGRADES [U]", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -118f), new Vector2(170f, 28f), new Color(0.12f, 0.25f, 0.45f, 0.95f), roundedSprite, defaultFont);
            toggleUpgradesButton = toggleBtnObj.GetComponent<Button>();
            toggleUpgradesButton.onClick.AddListener(ToggleUpgrades);
            toggleUpgradesButtonText = toggleBtnObj.GetComponentInChildren<Text>();

            // Settings Button
            GameObject hudSettingsBtnObj = CreateButton(statusCard.transform, "HUDSettingsBtn", "SETTINGS [ESC]", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(195f, -118f), new Vector2(150f, 28f), new Color(0.12f, 0.22f, 0.35f, 0.95f), roundedSprite, defaultFont);
            Button hudSettingsBtn = hudSettingsBtnObj.GetComponent<Button>();
            hudSettingsBtn.onClick.AddListener(() => {
                if (StarStrike.UI.SettingsUI.Instance != null)
                {
                    StarStrike.UI.SettingsUI.Instance.Toggle();
                }
            });

            // Active Skill Slot [E]
            GameObject skillSlotObj = CreatePanel(statusCard.transform, "SkillSlot_Active1", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -152f), new Vector2(330f, 28f), new Color(0.08f, 0.16f, 0.28f, 0.95f), roundedSprite);
            skillSlotView = skillSlotObj.AddComponent<HUDSkillSlotView>();
            skillSlotView.borderImage = skillSlotObj.GetComponent<Image>();
            GameObject cdOverlayObj = CreateImage(skillSlotObj.transform, "CooldownOverlay", new Color(0f, 0f, 0f, 0.65f), roundedSprite, Image.Type.Filled);
            skillSlotView.cooldownOverlay = cdOverlayObj.GetComponent<Image>();
            skillSlotView.cooldownOverlay.fillMethod = Image.FillMethod.Horizontal;
            skillSlotView.cooldownOverlay.fillAmount = 0f;
            GameObject skillTxtObj = CreateText(skillSlotObj.transform, "SkillLabel", "SKILL [E]: SHOCKWAVE (20 EN)", 11, FontStyle.Bold, new Color(0.38f, 0.85f, 1f), TextAnchor.MiddleLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(10f, 0f), new Vector2(-60f, 0f), defaultFont);
            skillSlotView.keybindLabel = skillTxtObj.GetComponent<Text>();
            GameObject cdTxtObj = CreateText(skillSlotObj.transform, "CooldownVal", "", 11, FontStyle.Bold, Color.yellow, TextAnchor.MiddleRight, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-60f, 0f), new Vector2(-10f, 0f), defaultFont);
            skillSlotView.cooldownText = cdTxtObj.GetComponent<Text>();
            Button slotBtn = skillSlotObj.AddComponent<Button>();
            skillSlotView.triggerButton = slotBtn;

            // 2. Upgrades Drawer
            GameObject drawerObj = CreatePanel(transform, "UpgradesDrawer", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -190f), new Vector2(360f, 360f), new Color(0.04f, 0.08f, 0.16f, 0.94f), roundedSprite);
            upgradesDrawer = drawerObj;
            CreateText(drawerObj.transform, "DrawerHeader", "SHIP UPGRADES", 14, FontStyle.Bold, new Color(0.38f, 0.74f, 0.98f), TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(0f, -32f), defaultFont);

            float rowY = -40f;
            foreach (UpgradeDefinition def in StatUpgrades)
            {
                string key = def.Key;
                GameObject rowLblObj = CreateText(drawerObj.transform, $"Label_{key}", def.Label, 13, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, rowY - 26f), new Vector2(230f, rowY), defaultFont);
                upgradeRowLabels[key] = rowLblObj.GetComponent<Text>();

                GameObject rowBtnObj = CreateButton(drawerObj.transform, $"Btn_{key}", "+ 10 G", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(245f, rowY - 28f), new Vector2(95f, 26f), new Color(0.12f, 0.45f, 0.25f, 1f), roundedSprite, defaultFont);
                Button btn = rowBtnObj.GetComponent<Button>();
                btn.onClick.AddListener(() => OnUpgradeClicked(key));
                upgradeRowButtons[key] = btn;
                upgradeRowButtonTexts[key] = rowBtnObj.GetComponentInChildren<Text>();

                rowY -= 36f;
            }
            upgradesDrawer.SetActive(false);

            // 3. Evolution Banner
            GameObject evoObj = CreatePanel(transform, "EvolutionBanner", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-220f, -20f), new Vector2(440f, 95f), new Color(0.12f, 0.08f, 0.25f, 0.95f), roundedSprite);
            evolutionBanner = evoObj;
            CreateText(evoObj.transform, "EvoHeader", "SHIP EVOLUTION AVAILABLE", 15, FontStyle.Bold, new Color(1f, 0.84f, 0.25f), TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(0f, -32f), defaultFont);

            evolutionButtonsContainer = new GameObject("ButtonsContainer", typeof(RectTransform));
            evolutionButtonsContainer.transform.SetParent(evoObj.transform, false);
            RectTransform ebRt = evolutionButtonsContainer.GetComponent<RectTransform>();
            ebRt.anchorMin = Vector2.zero;
            ebRt.anchorMax = Vector2.one;
            ebRt.offsetMin = Vector2.zero;
            ebRt.offsetMax = Vector2.zero;

            if (PlayerProgression.Instance != null)
            {
                RebuildEvolutionButtons(PlayerProgression.Instance);
            }
            evolutionBanner.SetActive(false);

            // 4. Minimap Radar
            GameObject minimapObj = new GameObject("MinimapRadar", typeof(RectTransform));
            minimapObj.transform.SetParent(transform, false);
            RectTransform mmRt = minimapObj.GetComponent<RectTransform>();
            mmRt.anchorMin = Vector2.zero;
            mmRt.anchorMax = Vector2.one;
            mmRt.offsetMin = Vector2.zero;
            mmRt.offsetMax = Vector2.zero;
            MinimapController minimap = minimapObj.AddComponent<MinimapController>();
            // MinimapController will auto-build its UI on Awake

            // 5. Mobile Touch Controls Container
            bool showMobile = forceShowMobileControls || Application.isMobilePlatform || Touchscreen.current != null;
            GameObject mobileRoot = new GameObject("MobileControlsRoot", typeof(RectTransform));
            mobileRoot.transform.SetParent(transform, false);
            RectTransform mRt = mobileRoot.GetComponent<RectTransform>();
            mRt.anchorMin = Vector2.zero;
            mRt.anchorMax = Vector2.one;
            mRt.offsetMin = Vector2.zero;
            mRt.offsetMax = Vector2.zero;
            mobileControlsRoot = mobileRoot;

            // Virtual Joystick (Bottom Left)
            GameObject joyBg = CreatePanel(mobileRoot.transform, "VirtualJoystick", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(60f, 60f), new Vector2(170f, 170f), new Color(1f, 1f, 1f, 0.15f), circleSprite);
            GameObject joyHandle = CreatePanel(joyBg.transform, "Handle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-35f, -35f), new Vector2(70f, 70f), new Color(0.38f, 0.74f, 0.98f, 0.75f), circleSprite);
            VirtualJoystick vj = joyBg.AddComponent<VirtualJoystick>();
            vj.background = joyBg.GetComponent<RectTransform>();
            vj.handle = joyHandle.GetComponent<RectTransform>();
            vj.handleRange = 65f;
            vj.autoThrustOnMove = true;

            // Mobile Action Buttons (Bottom Right Cluster)
            // Fire Button (Large)
            GameObject fireBtn = CreatePanel(mobileRoot.transform, "Btn_Fire", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-180f, 60f), new Vector2(120f, 120f), new Color(0.92f, 0.25f, 0.25f, 0.65f), circleSprite);
            fireBtn.AddComponent<TouchButton>().action = TouchButtonAction.Fire;
            CreateText(fireBtn.transform, "Label", "FIRE", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, defaultFont);

            // Boost Button (Above Fire)
            GameObject boostBtn = CreatePanel(mobileRoot.transform, "Btn_Boost", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-155f, 205f), new Vector2(75f, 75f), new Color(0.2f, 0.75f, 0.95f, 0.65f), circleSprite);
            boostBtn.AddComponent<TouchButton>().action = TouchButtonAction.Boost;
            CreateText(boostBtn.transform, "Label", "BOOST", 12, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, defaultFont);

            // Brake Button (To the left of Fire)
            GameObject brakeBtn = CreatePanel(mobileRoot.transform, "Btn_Brake", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-280f, 60f), new Vector2(75f, 75f), new Color(0.45f, 0.5f, 0.6f, 0.65f), circleSprite);
            brakeBtn.AddComponent<TouchButton>().action = TouchButtonAction.Brake;
            CreateText(brakeBtn.transform, "Label", "BRAKE", 12, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, defaultFont);

            // Skill Button (Above Brake / Radial Cluster)
            GameObject skillBtn = CreatePanel(mobileRoot.transform, "Btn_Skill", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260f, 150f), new Vector2(75f, 75f), new Color(0.7f, 0.3f, 0.95f, 0.75f), circleSprite);
            skillBtn.AddComponent<TouchButton>().action = TouchButtonAction.Skill1;
            CreateText(skillBtn.transform, "Label", "SKILL", 12, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, defaultFont);

            mobileControlsRoot.SetActive(showMobile);
            
            // Sub-Controllers
            if (playerStatus == null)
            {
                playerStatus = gameObject.AddComponent<HUDPlayerStatus>();
                playerStatus.shieldFill = shieldFill;
                playerStatus.shieldText = shieldValueText;
                playerStatus.hullFill = hullFill;
                playerStatus.hullText = hullValueText;
                playerStatus.energyFill = energyFill;
                playerStatus.energyText = energyValueText;
                playerStatus.shipNameText = shipNameText;
                playerStatus.gemsText = gemsText;
                playerStatus.ApplyThemeStyles();
            }

            if (floatingTextController == null)
            {
                floatingTextController = gameObject.AddComponent<HUDFloatingText>();
            }

            if (endGameModal == null)
            {
                endGameModal = gameObject.AddComponent<HUDEndGameModal>();
            }

            if (GetComponent<StarStrike.UI.HUD.CarrierHUDOverlay>() == null)
            {
                gameObject.AddComponent<StarStrike.UI.HUD.CarrierHUDOverlay>();
            }
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color, Sprite sprite)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMin;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            return obj;
        }

        private GameObject CreateImage(Transform parent, string name, Color color, Sprite sprite, Image.Type type = Image.Type.Simple)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = type;
            return obj;
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
            txt.fontSize = StarStrike.UI.UIHelper.ScaleFontSize(fontSize);
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.font = font;
            txt.supportRichText = true;
            return obj;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Sprite sprite, Font font)
        {
            GameObject btnObj = CreatePanel(parent, name, anchorMin, anchorMax, pos, size, bgColor, sprite);
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.2f;
            cb.pressedColor = bgColor * 0.8f;
            cb.disabledColor = new Color(bgColor.r * 0.4f, bgColor.g * 0.4f, bgColor.b * 0.4f, 0.4f);
            btn.colors = cb;

            CreateText(btnObj.transform, "Text", label, 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, font);
            return btnObj;
        }
    }
}
