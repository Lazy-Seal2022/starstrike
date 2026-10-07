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
    public class CanvasHUD : MonoBehaviour
    {
        public static CanvasHUD Instance { get; private set; }

        [Header("Configuration")]
        public bool forceShowMobileControls = false;

        [Header("UI References (Optional - Auto-built if null)")]
        public Canvas canvas;
        public Text shipNameText;
        public Text gemsText;
        public Image shieldFill;
        public Text shieldValueText;
        public Image energyFill;
        public Text energyValueText;
        public GameObject upgradesDrawer;
        public Button toggleUpgradesButton;
        public Text toggleUpgradesButtonText;
        public GameObject evolutionBanner;
        public Button evolveFighterButton;
        public Button evolveMinerButton;
        public GameObject mobileControlsRoot;

        private float currentShield = 100f;
        private float maxShield = 100f;
        private float currentEnergy = 80f;
        private float maxEnergy = 80f;
        private int totalGems = 0;
        private bool isUpgradesOpen = false;

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
            Instance = this;
            EnsureEventSystem();
            BuildCanvasIfNeeded();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            GameEvents.OnShieldChanged += HandleShieldChanged;
            GameEvents.OnEnergyChanged += HandleEnergyChanged;
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnShieldChanged -= HandleShieldChanged;
            GameEvents.OnEnergyChanged -= HandleEnergyChanged;
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
        }

        private void Start()
        {
            SyncPlayerInitialState();
            UpdateAllHUDVisuals();
        }

        private void Update()
        {
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
            ShipController player = ShipController.Instance;
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.GetComponent<ShipController>();
            }

            if (player != null && player.stats != null)
            {
                maxShield = player.stats.shieldCapacity;
                currentShield = player.currentShield > 0f ? player.currentShield : maxShield;
                maxEnergy = player.stats.energyCapacity;
                currentEnergy = player.currentEnergy > 0f ? player.currentEnergy : maxEnergy;
                totalGems = player.gems;
                UpdateShipHeader(player.stats.tier, player.stats.shipName);
            }
        }

        private void HandleShieldChanged(float current, float max)
        {
            currentShield = current;
            maxShield = max;
            UpdateShieldVisuals();
        }

        private void HandleEnergyChanged(float current, float max)
        {
            currentEnergy = current;
            maxEnergy = max;
            UpdateEnergyVisuals();
        }

        private void HandleGemCollected(int added, int total)
        {
            totalGems = total;
            if (gemsText != null)
            {
                gemsText.text = $"💎 <color=#4ade80><b>{totalGems}</b></color> Gems";
            }
            if (isUpgradesOpen)
            {
                UpdateUpgradeRows();
            }
        }

        private void HandlePlayerEvolved(string shipId)
        {
            ShipController player = ShipController.Instance;
            if (player != null && player.stats != null)
            {
                UpdateShipHeader(player.stats.tier, player.stats.shipName);
                currentShield = player.stats.shieldCapacity;
                maxShield = player.stats.shieldCapacity;
                currentEnergy = player.stats.energyCapacity;
                maxEnergy = player.stats.energyCapacity;
                UpdateShieldVisuals();
                UpdateEnergyVisuals();
            }
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
            if (shieldValueText != null) shieldValueText.text = $"{Mathf.RoundToInt(currentShield)} / {Mathf.RoundToInt(maxShield)}";
        }

        private void UpdateEnergyVisuals()
        {
            float ratio = Mathf.Clamp01(currentEnergy / Mathf.Max(1f, maxEnergy));
            if (energyFill != null) energyFill.fillAmount = ratio;
            if (energyValueText != null) energyValueText.text = $"{Mathf.RoundToInt(currentEnergy)} / {Mathf.RoundToInt(maxEnergy)}";
        }

        private void UpdateAllHUDVisuals()
        {
            UpdateShieldVisuals();
            UpdateEnergyVisuals();
            if (gemsText != null) gemsText.text = $"💎 <color=#4ade80><b>{totalGems}</b></color> Gems";
            UpdateEvolutionBanner();
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
                toggleUpgradesButtonText.text = isUpgradesOpen ? "▲ HIDE [U]" : "▼ UPGRADES [U]";
            }
            if (isUpgradesOpen)
            {
                UpdateUpgradeRows();
            }
        }

        private void UpdateUpgradeRows()
        {
            ShipController player = ShipController.Instance;
            if (player == null) return;

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
                    btnTxt.text = $"+ {cost}💎";
                }

                if (upgradeRowButtons.TryGetValue(def.Key, out Button btn) && btn != null)
                {
                    btn.interactable = (player.gems >= cost);
                }
            }
        }

        private int GetStatLevel(ShipController player, string key)
        {
            switch (key)
            {
                case "shieldCap": return player.lvlShieldCap;
                case "shieldRegen": return player.lvlShieldRegen;
                case "energyCap": return player.lvlEnergyCap;
                case "energyRegen": return player.lvlEnergyRegen;
                case "damage": return player.lvlDamage;
                case "pulseSpeed": return player.lvlPulseSpeed;
                case "speed": return player.lvlSpeed;
                case "agility": return player.lvlAgility;
                default: return 0;
            }
        }

        private void OnUpgradeClicked(string key)
        {
            ShipController player = ShipController.Instance;
            if (player != null && player.UpgradeStat(key))
            {
                UpdateUpgradeRows();
                UpdateEvolutionBanner();
            }
        }

        private void UpdateEvolutionBanner()
        {
            if (evolutionBanner == null) return;

            ShipController player = ShipController.Instance;
            bool canEvolve = player != null && player.CanEvolve();
            if (evolutionBanner.activeSelf != canEvolve)
            {
                evolutionBanner.SetActive(canEvolve);
            }
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }
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

            // 1. Top-Left Status Panel
            GameObject statusCard = CreatePanel(transform, "StatusPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(360f, 160f), new Color(0.04f, 0.08f, 0.16f, 0.88f), roundedSprite);

            // Ship Name
            GameObject nameObj = CreateText(statusCard.transform, "ShipNameText", "T1 • DELTA SCOUT", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -12f), new Vector2(-16f, -36f), defaultFont);
            shipNameText = nameObj.GetComponent<Text>();

            // Gem Count
            GameObject gemsObj = CreateText(statusCard.transform, "GemsText", "💎 0 Gems", 15, FontStyle.Bold, new Color(0.3f, 0.95f, 0.5f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -40f), new Vector2(-16f, -64f), defaultFont);
            gemsText = gemsObj.GetComponent<Text>();

            // Shield Bar
            CreateText(statusCard.transform, "ShieldLabel", "SHIELD", 11, FontStyle.Bold, new Color(0.6f, 0.75f, 0.9f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -70f), new Vector2(70f, -90f), defaultFont);
            GameObject shieldBg = CreatePanel(statusCard.transform, "ShieldBg", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(75f, -73f), new Vector2(190f, 14f), new Color(0.08f, 0.15f, 0.25f, 1f), roundedSprite);
            GameObject shieldFillObj = CreateImage(shieldBg.transform, "Fill", new Color(0.22f, 0.74f, 0.97f), roundedSprite, Image.Type.Filled);
            shieldFill = shieldFillObj.GetComponent<Image>();
            shieldFill.fillMethod = Image.FillMethod.Horizontal;
            shieldFill.fillOrigin = 0;
            shieldFill.fillAmount = 1f;
            GameObject shieldValObj = CreateText(statusCard.transform, "ShieldVal", "100 / 100", 11, FontStyle.Normal, Color.white, TextAnchor.MiddleRight, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -70f), new Vector2(345f, -90f), defaultFont);
            shieldValueText = shieldValObj.GetComponent<Text>();

            // Energy Bar
            CreateText(statusCard.transform, "EnergyLabel", "ENERGY", 11, FontStyle.Bold, new Color(0.95f, 0.75f, 0.4f), TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -96f), new Vector2(70f, -116f), defaultFont);
            GameObject energyBg = CreatePanel(statusCard.transform, "EnergyBg", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(75f, -99f), new Vector2(190f, 14f), new Color(0.25f, 0.15f, 0.05f, 1f), roundedSprite);
            GameObject energyFillObj = CreateImage(energyBg.transform, "Fill", new Color(0.96f, 0.62f, 0.04f), roundedSprite, Image.Type.Filled);
            energyFill = energyFillObj.GetComponent<Image>();
            energyFill.fillMethod = Image.FillMethod.Horizontal;
            energyFill.fillOrigin = 0;
            energyFill.fillAmount = 1f;
            GameObject energyValObj = CreateText(statusCard.transform, "EnergyVal", "80 / 80", 11, FontStyle.Normal, Color.white, TextAnchor.MiddleRight, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(270f, -96f), new Vector2(345f, -116f), defaultFont);
            energyValueText = energyValObj.GetComponent<Text>();

            // Toggle Upgrades Button
            GameObject toggleBtnObj = CreateButton(statusCard.transform, "ToggleUpgradesBtn", "▼ UPGRADES [U]", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -122f), new Vector2(170f, 30f), new Color(0.12f, 0.25f, 0.45f, 0.95f), roundedSprite, defaultFont);
            toggleUpgradesButton = toggleBtnObj.GetComponent<Button>();
            toggleUpgradesButton.onClick.AddListener(ToggleUpgrades);
            toggleUpgradesButtonText = toggleBtnObj.GetComponentInChildren<Text>();

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

                GameObject rowBtnObj = CreateButton(drawerObj.transform, $"Btn_{key}", "+ 10💎", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(245f, rowY - 28f), new Vector2(95f, 26f), new Color(0.12f, 0.45f, 0.25f, 1f), roundedSprite, defaultFont);
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
            CreateText(evoObj.transform, "EvoHeader", "★ SHIP EVOLUTION AVAILABLE ★", 15, FontStyle.Bold, new Color(1f, 0.84f, 0.25f), TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(0f, -32f), defaultFont);

            GameObject fighterBtnObj = CreateButton(evoObj.transform, "AresBtn", "⚔ ARES FIGHTER", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -42f), new Vector2(190f, 40f), new Color(0.85f, 0.2f, 0.25f, 1f), roundedSprite, defaultFont);
            evolveFighterButton = fighterBtnObj.GetComponent<Button>();
            evolveFighterButton.onClick.AddListener(() =>
            {
                if (ShipController.Instance != null) ShipController.Instance.Evolve("fighter");
            });

            GameObject minerBtnObj = CreateButton(evoObj.transform, "MinerBtn", "⛏ GOLIATH MINER", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-210f, -42f), new Vector2(190f, 40f), new Color(0.9f, 0.55f, 0.1f, 1f), roundedSprite, defaultFont);
            evolveMinerButton = minerBtnObj.GetComponent<Button>();
            evolveMinerButton.onClick.AddListener(() =>
            {
                if (ShipController.Instance != null) ShipController.Instance.Evolve("miner");
            });
            evolutionBanner.SetActive(false);

            // 4. Mobile Touch Controls Container
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

            mobileControlsRoot.SetActive(showMobile);
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
            txt.fontSize = fontSize;
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
