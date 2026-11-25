using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class EquipmentSelectUI : MonoBehaviour, IScreenController
    {
        public static EquipmentSelectUI Instance { get; private set; }
        public static int InitialSocketIndex = -1;

        public string ScreenId => "EquipmentSelect";

        private GameObject rootContainer;
        private TMP_Text gemsText;
        private TMP_Text shipHeaderBanner;
        private Image shipPreviewImage;
        private Transform socketButtonContainer;
        private TMP_Text socketHeaderPrompt;
        private Transform weaponButtonContainer;
        private Transform skillButtonContainer;
        private Image weaponDetailIcon;
        private TMP_Text weaponDetailTitle;
        private TMP_Text weaponDetailStats;
        private TMP_Text weaponDetailDesc;
        private TMP_Text skillDetailTitle;
        private TMP_Text skillDetailDesc;

        private string currentShipId = "scout";
        private int activeSocketIndex = 0;
        private int activeSlotIndex = 0;
        private string inspectedWeaponId = "pulse";
        private string inspectedModuleId = "def_shield_booster";
        private string inspectedSkillId = "forward_shockwave";

        public string CurrentShipId
        {
            get
            {
                EnsureInitialized();
                return currentShipId;
            }
        }

        public int ActiveSocketIndex
        {
            get
            {
                EnsureInitialized();
                return activeSocketIndex;
            }
        }

        public int ActiveSocketCount => ShipWeaponController.GetShipHardpointSockets(CurrentShipId).Length;

        public int ActiveSlotIndex
        {
            get
            {
                EnsureInitialized();
                return activeSlotIndex;
            }
        }

        public int ActiveSlotCount => GetCurrentShipSlots().Length;

        public EquipmentSlotCategory ActiveSlotCategory => GetCurrentSlot().category;

        public string InspectedWeaponId
        {
            get
            {
                EnsureInitialized();
                return inspectedWeaponId;
            }
        }

        public string InspectedModuleId
        {
            get
            {
                EnsureInitialized();
                return inspectedModuleId;
            }
        }

        public string InspectedSkillId
        {
            get
            {
                EnsureInitialized();
                return inspectedSkillId;
            }
        }

        public struct WeaponDefinitionInfo
        {
            public string id;
            public string name;
            public WeaponArchetype archetype;
            public string classType;
            public float damage;
            public float fireRate;
            public float energyCost;
            public string spread;
            public string description;
        }

        public struct SkillDefinitionInfo
        {
            public string id;
            public string name;
            public string role;
            public float energyCost;
            public float cooldown;
            public string description;
        }

        private static readonly WeaponDefinitionInfo[] WeaponCatalog = new WeaponDefinitionInfo[]
        {
            new WeaponDefinitionInfo {
                id = "pulse",
                name = "Pulse Laser",
                archetype = WeaponArchetype.Projectile,
                classType = "PROJECTILE // DIRECT PLASMA",
                damage = 22f,
                fireRate = 4.5f,
                energyCost = 6f,
                spread = "0.0° Pinpoint Accuracy",
                description = "Balanced rapid-cycling plasma bolts delivering continuous frontline fire with zero recoil drift."
            },
            new WeaponDefinitionInfo {
                id = "scattergun",
                name = "Scattergun",
                archetype = WeaponArchetype.Projectile,
                classType = "PROJECTILE // SPREAD CONE",
                damage = 75f, // 15 * 5 pellets
                fireRate = 2.2f,
                energyCost = 14f,
                spread = "18.0° 5-Pellet Cone",
                description = "Heavy particle dispersal cannon firing dense multi-pellet clusters for catastrophic close-range impact."
            },
            new WeaponDefinitionInfo {
                id = "missile",
                name = "Homing Missiles",
                archetype = WeaponArchetype.Traceable,
                classType = "TRACEABLE // GUIDED ORDNANCE",
                damage = 60f,
                fireRate = 1.4f,
                energyCost = 16f,
                spread = "Predictive Target Tracking",
                description = "Self-propelled smart warheads with autonomous thrusters that seek and destroy nearest hostiles."
            },
            new WeaponDefinitionInfo {
                id = "beam",
                name = "Continuous Beam",
                archetype = WeaponArchetype.Beam,
                classType = "BEAM // DIRECTED ENERGY",
                damage = 80f, // DPS
                fireRate = 60.0f,
                energyCost = 20f, // /s
                spread = "Instant Collimated Ray",
                description = "Focused coherent particle beam slicing continuously through multiple targets with instantaneous impact."
            },
            new WeaponDefinitionInfo {
                id = "gatling",
                name = "Rotary Gatling Gun",
                archetype = WeaponArchetype.Projectile,
                classType = "PROJECTILE // ROTARY KINETIC",
                damage = 18f,
                fireRate = 14.5f, // up to 20/s at max spool
                energyCost = 2.5f,
                spread = "3.2° Base -> 7.5° Spool Bloom",
                description = "Multi-barrel rotary cannon accelerating to blistering cyclic fire rates with dynamic spread bloom."
            }
        };

        private static readonly SkillDefinitionInfo[] SkillCatalog = new SkillDefinitionInfo[]
        {
            new SkillDefinitionInfo {
                id = "forward_shockwave",
                name = "Kinetic Shockwave",
                role = "DEFENSIVE CLEARANCE // CONE DISRUPTION",
                energyCost = 25f,
                cooldown = 4.0f,
                description = "Discharges a high-frequency kinetic concussive wave, vaporizing enemy bullets and fracturing asteroids."
            },
            new SkillDefinitionInfo {
                id = "blink_teleport",
                name = "Blink Warp Dash",
                role = "TACTICAL MOBILITY // PHASE ESCAPE",
                energyCost = 30f,
                cooldown = 3.5f,
                description = "Instantly warps the vessel along its current vector with brief invulnerability frames, bypassing collisions."
            },
            new SkillDefinitionInfo {
                id = "emp_nova",
                name = "EMP Nova Burst",
                role = "AREA SHIELD STRIP // HOSTILE STUN",
                energyCost = 35f,
                cooldown = 6.0f,
                description = "Releases an expanding electromagnetic field, destabilizing enemy shielding and briefly freezing weapon systems."
            }
        };

        public bool IsVisible => rootContainer != null && rootContainer.activeSelf;

        public void Show() => SetVisible(true);
        public void Hide() => SetVisible(false);

        public void EnsureInitialized()
        {
            if (rootContainer != null) return;

            if (Instance == null) Instance = this;

            RectTransform selfRt = GetComponent<RectTransform>();
            if (selfRt == null) selfRt = gameObject.AddComponent<RectTransform>();
            selfRt.anchorMin = Vector2.zero;
            selfRt.anchorMax = Vector2.one;
            selfRt.offsetMin = Vector2.zero;
            selfRt.offsetMax = Vector2.zero;

            currentShipId = StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(currentShipId)) currentShipId = "scout";
            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (InitialSocketIndex >= 0)
            {
                activeSocketIndex = Mathf.Clamp(InitialSocketIndex, 0, Mathf.Max(0, sockets.Length - 1));
                InitialSocketIndex = -1;
            }
            if (activeSocketIndex >= sockets.Length) activeSocketIndex = 0;
            inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            inspectedSkillId = StorageService.GetSelectedSkill();

            BuildUI();
            RefreshUI();
        }

        public void SetVisible(bool visible)
        {
            EnsureInitialized();
            if (rootContainer != null)
            {
                rootContainer.SetActive(visible);
            }
            if (visible)
            {
                currentShipId = StorageService.GetLastShipEvolved();
                if (string.IsNullOrEmpty(currentShipId)) currentShipId = "scout";
                var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
                if (InitialSocketIndex >= 0)
                {
                    activeSocketIndex = Mathf.Clamp(InitialSocketIndex, 0, Mathf.Max(0, sockets.Length - 1));
                    InitialSocketIndex = -1;
                }
                if (activeSocketIndex >= sockets.Length) activeSocketIndex = 0;
                inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
                inspectedSkillId = StorageService.GetSelectedSkill();
                RefreshUI();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            EnsureInitialized();
        }

        private void OnEnable()
        {
            if (ScreenManager.Instance != null)
            {
                ScreenManager.Instance.RegisterScreen(this);
            }
            currentShipId = StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(currentShipId)) currentShipId = "scout";
            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (InitialSocketIndex >= 0)
            {
                activeSocketIndex = Mathf.Clamp(InitialSocketIndex, 0, Mathf.Max(0, sockets.Length - 1));
                InitialSocketIndex = -1;
            }
            if (activeSocketIndex >= sockets.Length) activeSocketIndex = 0;
            inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            inspectedSkillId = StorageService.GetSelectedSkill();
            RefreshUI();
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
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) EquipItemByIndex(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) EquipItemByIndex(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) EquipItemByIndex(2);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) EquipItemByIndex(3);
            else if (Keyboard.current.digit5Key.wasPressedThisFrame) EquipItemByIndex(4);
            else if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) SelectPreviousSlot();
            else if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame) SelectNextSlot();
            else if (Keyboard.current.hKey.wasPressedThisFrame) GoToHangar();
            else if (Keyboard.current.escapeKey.wasPressedThisFrame) GoToMainMenu();
            else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadGameplay(GameMode.PvE);
                }
            }
        }

        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            rootContainer = new GameObject("EquipmentSelectContainer", typeof(RectTransform));
            rootContainer.transform.SetParent(transform, false);
            RectTransform rt = rootContainer.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Canvas canvas = rootContainer.GetComponent<Canvas>();
            if (canvas == null) canvas = rootContainer.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            if (rootContainer.GetComponent<GraphicRaycaster>() == null)
            {
                rootContainer.AddComponent<GraphicRaycaster>();
            }

            // Deep Cyber Void Background (Clean, minimal, no pixel grid)
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(rootContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = CyberVectorTheme.VoidDeep;

            // Title Header
            CreateText(rootContainer.transform, "Title", "<color=#00f2ff><b>// ARMORY LOGISTICS // TACTICAL HARDPOINT MATRIX</b></color>", 28, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.94f), new Vector2(0.5f, 0.94f), new Vector2(-480, -22), new Vector2(480, 22), defaultFont);

            // Gems Counter
            GameObject gemsObj = CreateText(rootContainer.transform, "GemsText", "META-GEMS: 0", 18, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleRight, new Vector2(1f, 0.94f), new Vector2(1f, 0.94f), new Vector2(-280, -18), new Vector2(-30, 18), defaultFont);
            gemsText = gemsObj.GetComponent<TMP_Text>();

            // Ship Subtitle Banner
            GameObject bannerObj = CreateText(rootContainer.transform, "ShipHeaderBanner", "ACTIVE CHASSIS: [ DELTA SCOUT // HARDPOINT CONFIG ]", 17, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(-350, -18), new Vector2(350, 18), defaultFont);
            shipHeaderBanner = bannerObj.GetComponent<TMP_Text>();

            // Change Ship Button (Top Right of Banner)
            GameObject chgShipBtn = CreateButton(rootContainer.transform, "ChangeShipBtn", "VESSEL HANGAR [H]", new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(340, 0), new Vector2(180, 36), CyberVectorTheme.NeonCyan, defaultFont, 13);
            chgShipBtn.GetComponent<Button>().onClick.AddListener(GoToHangar);

            // SECTION 1: PRIMARY WEAPONS & SHIP HARDPOINTS (Left Chamfer Panel)
            GameObject wpnPanel = new GameObject("WeaponPanel", typeof(RectTransform), typeof(Image));
            wpnPanel.transform.SetParent(rootContainer.transform, false);
            RectTransform wpRt = wpnPanel.GetComponent<RectTransform>();
            wpRt.anchorMin = new Vector2(0.04f, 0.16f);
            wpRt.anchorMax = new Vector2(0.48f, 0.84f);
            wpRt.offsetMin = Vector2.zero;
            wpRt.offsetMax = Vector2.zero;
            var wpImg = wpnPanel.GetComponent<Image>();
            wpImg.sprite = CyberVectorTheme.GetChamferCardSprite();
            wpImg.type = Image.Type.Sliced;
            wpImg.color = Color.white;

            CreateText(wpnPanel.transform, "WpnHeader", "<color=#00f2ff><b>1. VESSEL HARDPOINTS SCHEMATIC</b></color> <color=#ffe600>[CLICK TO SELECT]</color>", 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.95f), new Vector2(0.05f, 0.95f), new Vector2(0, -14), new Vector2(460, 14), defaultFont);

            // Ship Visual Preview Box (Top subcard)
            GameObject previewBox = new GameObject("ShipPreviewBox", typeof(RectTransform), typeof(Image));
            previewBox.transform.SetParent(wpnPanel.transform, false);
            RectTransform pbRt = previewBox.GetComponent<RectTransform>();
            pbRt.anchorMin = new Vector2(0.04f, 0.36f);
            pbRt.anchorMax = new Vector2(0.96f, 0.92f);
            pbRt.offsetMin = Vector2.zero;
            pbRt.offsetMax = Vector2.zero;
            var pbImg = previewBox.GetComponent<Image>();
            pbImg.sprite = CyberVectorTheme.GetChamferCardSprite(128, 12);
            pbImg.type = Image.Type.Sliced;
            pbImg.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);

            // Holo Pedestal under ship preview
            GameObject pedestalObj = new GameObject("HoloPedestal", typeof(RectTransform), typeof(Image), typeof(CyberHoloPedestal));
            pedestalObj.transform.SetParent(previewBox.transform, false);
            RectTransform pdRt = pedestalObj.GetComponent<RectTransform>();
            pdRt.anchorMin = new Vector2(0.05f, 0.04f);
            pdRt.anchorMax = new Vector2(0.95f, 0.36f);
            pdRt.offsetMin = Vector2.zero;
            pdRt.offsetMax = Vector2.zero;
            var pdImg = pedestalObj.GetComponent<Image>();
            pdImg.sprite = CyberVectorTheme.GetHoloPedestalSprite();
            pdImg.color = CyberVectorTheme.NeonCyan;
            pdImg.preserveAspect = true;

            GameObject shipImgObj = new GameObject("ShipSprite", typeof(RectTransform), typeof(Image));
            shipImgObj.transform.SetParent(previewBox.transform, false);
            RectTransform siRt = shipImgObj.GetComponent<RectTransform>();
            siRt.anchorMin = new Vector2(0.10f, 0.16f);
            siRt.anchorMax = new Vector2(0.90f, 0.88f);
            siRt.offsetMin = Vector2.zero;
            siRt.offsetMax = Vector2.zero;
            shipPreviewImage = shipImgObj.GetComponent<Image>();
            shipPreviewImage.preserveAspect = true;

            var pedestalComp = pedestalObj.GetComponent<CyberHoloPedestal>();
            pedestalComp.SetTargetShip(siRt);

            // Sockets Matrix & Quick Tabs Box (Bottom subcard)
            GameObject telemetryBox = new GameObject("SocketTelemetryBox", typeof(RectTransform), typeof(Image));
            telemetryBox.transform.SetParent(wpnPanel.transform, false);
            RectTransform tbRt = telemetryBox.GetComponent<RectTransform>();
            tbRt.anchorMin = new Vector2(0.04f, 0.04f);
            tbRt.anchorMax = new Vector2(0.96f, 0.33f);
            tbRt.offsetMin = Vector2.zero;
            tbRt.offsetMax = Vector2.zero;
            var tbImg = telemetryBox.GetComponent<Image>();
            tbImg.sprite = CyberVectorTheme.GetChamferCardSprite(128, 12);
            tbImg.type = Image.Type.Sliced;
            tbImg.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);

            CreateText(telemetryBox.transform, "TelHeader", "<color=#526b94>// HARDPOINTS MATRIX & TABS [Q/E]:</color>", 11, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.86f), new Vector2(0.05f, 0.86f), new Vector2(0, -10), new Vector2(400, 10), defaultFont);

            GameObject socketBtnContainerObj = new GameObject("SocketButtons", typeof(RectTransform));
            socketBtnContainerObj.transform.SetParent(telemetryBox.transform, false);
            RectTransform sbcRt = socketBtnContainerObj.GetComponent<RectTransform>();
            sbcRt.anchorMin = new Vector2(0.03f, 0.08f);
            sbcRt.anchorMax = new Vector2(0.97f, 0.78f);
            sbcRt.offsetMin = Vector2.zero;
            sbcRt.offsetMax = Vector2.zero;
            socketButtonContainer = socketBtnContainerObj.transform;

            // SECTION 2: ARSENAL WEAPONS & COMBAT MODULES (Right Chamfer Panel)
            GameObject outfittingPanel = new GameObject("OutfittingPanel", typeof(RectTransform), typeof(Image));
            outfittingPanel.transform.SetParent(rootContainer.transform, false);
            RectTransform opRt = outfittingPanel.GetComponent<RectTransform>();
            opRt.anchorMin = new Vector2(0.50f, 0.16f);
            opRt.anchorMax = new Vector2(0.96f, 0.84f);
            opRt.offsetMin = Vector2.zero;
            opRt.offsetMax = Vector2.zero;
            var opImg = outfittingPanel.GetComponent<Image>();
            opImg.sprite = CyberVectorTheme.GetChamferCardSprite();
            opImg.type = Image.Type.Sliced;
            opImg.color = Color.white;

            CreateText(outfittingPanel.transform, "OutfittingHeader", "<color=#ff8514><b>2. WEAPON ARSENAL & MODULES</b></color>", 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.95f), new Vector2(0.05f, 0.95f), new Vector2(0, -14), new Vector2(460, 14), defaultFont);

            // Active Socket Prompt Subtitle
            GameObject promptObj = CreateText(outfittingPanel.transform, "SocketPrompt", "<color=#00f2ff>SELECT WEAPON FOR ACTIVE SOCKET [1-5]:</color>", 12, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.90f), new Vector2(0.05f, 0.90f), new Vector2(0, -10), new Vector2(460, 10), defaultFont);
            socketHeaderPrompt = promptObj.GetComponent<TMP_Text>();

            // Weapon Selection Buttons
            GameObject wpnBtnContainerObj = new GameObject("WeaponButtons", typeof(RectTransform));
            wpnBtnContainerObj.transform.SetParent(outfittingPanel.transform, false);
            RectTransform wbcRt = wpnBtnContainerObj.GetComponent<RectTransform>();
            wbcRt.anchorMin = new Vector2(0.04f, 0.76f);
            wbcRt.anchorMax = new Vector2(0.96f, 0.88f);
            wbcRt.offsetMin = Vector2.zero;
            wbcRt.offsetMax = Vector2.zero;
            weaponButtonContainer = wpnBtnContainerObj.transform;

            // Weapon Detail Card
            GameObject wpnDetailObj = new GameObject("WeaponDetailCard", typeof(RectTransform), typeof(Image));
            wpnDetailObj.transform.SetParent(outfittingPanel.transform, false);
            RectTransform wdRt = wpnDetailObj.GetComponent<RectTransform>();
            wdRt.anchorMin = new Vector2(0.04f, 0.49f);
            wdRt.anchorMax = new Vector2(0.96f, 0.74f);
            wdRt.offsetMin = Vector2.zero;
            wdRt.offsetMax = Vector2.zero;
            var wdImg = wpnDetailObj.GetComponent<Image>();
            wdImg.sprite = CyberVectorTheme.GetChamferCardSprite(128, 12);
            wdImg.type = Image.Type.Sliced;
            wdImg.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);

            GameObject wdtObj = CreateText(wpnDetailObj.transform, "WpnTitle", "PULSE LASER", 15, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleLeft, new Vector2(0.04f, 0.86f), new Vector2(0.04f, 0.86f), new Vector2(0, -14), new Vector2(360, 14), defaultFont);
            weaponDetailTitle = wdtObj.GetComponent<TMP_Text>();

            GameObject iconObj = new GameObject("WeaponIconPreview", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(wpnDetailObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.85f, 0.40f);
            iconRt.anchorMax = new Vector2(0.96f, 0.90f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            weaponDetailIcon = iconObj.GetComponent<Image>();
            weaponDetailIcon.preserveAspect = true;

            GameObject wdsObj = CreateText(wpnDetailObj.transform, "WpnStats", "STATS", 11, FontStyle.Normal, Color.white, TextAnchor.UpperLeft, new Vector2(0.04f, 0.54f), new Vector2(0.04f, 0.54f), new Vector2(0, -30), new Vector2(380, 30), defaultFont);
            weaponDetailStats = wdsObj.GetComponent<TMP_Text>();

            GameObject wddObj = CreateText(wpnDetailObj.transform, "WpnDesc", "DESCRIPTION", 10, FontStyle.Italic, new Color(0.75f, 0.85f, 0.98f), TextAnchor.UpperLeft, new Vector2(0.04f, 0.18f), new Vector2(0.04f, 0.18f), new Vector2(0, -22), new Vector2(380, 22), defaultFont);
            weaponDetailDesc = wddObj.GetComponent<TMP_Text>();

            // Tactical Combat Modules Header
            CreateText(outfittingPanel.transform, "SklHeader", "<color=#00f2ff><b>3. TACTICAL COMBAT MODULES</b></color> <color=#526b94>[TRIGGER: SPACE]</color>", 13, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.44f), new Vector2(0.05f, 0.44f), new Vector2(0, -12), new Vector2(420, 12), defaultFont);

            GameObject sklBtnContainerObj = new GameObject("SkillButtons", typeof(RectTransform));
            sklBtnContainerObj.transform.SetParent(outfittingPanel.transform, false);
            RectTransform skbcRt = sklBtnContainerObj.GetComponent<RectTransform>();
            skbcRt.anchorMin = new Vector2(0.04f, 0.29f);
            skbcRt.anchorMax = new Vector2(0.96f, 0.41f);
            skbcRt.offsetMin = Vector2.zero;
            skbcRt.offsetMax = Vector2.zero;
            skillButtonContainer = sklBtnContainerObj.transform;

            // Skill Detail Card
            GameObject sklDetailObj = new GameObject("SkillDetailCard", typeof(RectTransform), typeof(Image));
            sklDetailObj.transform.SetParent(outfittingPanel.transform, false);
            RectTransform sdRt = sklDetailObj.GetComponent<RectTransform>();
            sdRt.anchorMin = new Vector2(0.04f, 0.04f);
            sdRt.anchorMax = new Vector2(0.96f, 0.26f);
            sdRt.offsetMin = Vector2.zero;
            sdRt.offsetMax = Vector2.zero;
            var sdImg = sklDetailObj.GetComponent<Image>();
            sdImg.sprite = CyberVectorTheme.GetChamferCardSprite(128, 12);
            sdImg.type = Image.Type.Sliced;
            sdImg.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);

            GameObject sktObj = CreateText(sklDetailObj.transform, "SklTitle", "KINETIC SHOCKWAVE", 14, FontStyle.Bold, CyberVectorTheme.NeonCyan, TextAnchor.MiddleLeft, new Vector2(0.04f, 0.82f), new Vector2(0.04f, 0.82f), new Vector2(0, -12), new Vector2(380, 12), defaultFont);
            skillDetailTitle = sktObj.GetComponent<TMP_Text>();

            GameObject skdObj = CreateText(sklDetailObj.transform, "SklDesc", "DESCRIPTION", 11, FontStyle.Normal, new Color(0.85f, 0.9f, 1f), TextAnchor.UpperLeft, new Vector2(0.04f, 0.44f), new Vector2(0.04f, 0.44f), new Vector2(0, -32), new Vector2(380, 32), defaultFont);
            skillDetailDesc = skdObj.GetComponent<TMP_Text>();

            // FLOW BUTTONS (Bottom Row)
            GameObject backBtnObj = CreateButton(rootContainer.transform, "BackBtn", "◄ MAIN MENU [ESC]", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(-340, 0), new Vector2(220, 44), CyberVectorTheme.DimMuted, defaultFont, 14);
            backBtnObj.GetComponent<Button>().onClick.AddListener(GoToMainMenu);

            GameObject hangarBtnObj = CreateButton(rootContainer.transform, "HangarBtn", "◄ HANGAR SHIPS [H]", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(0, 0), new Vector2(240, 46), CyberVectorTheme.NeonCyan, defaultFont, 15);
            hangarBtnObj.GetComponent<Button>().onClick.AddListener(GoToHangar);

            GameObject playBtnObj = CreateButton(rootContainer.transform, "PlayBtn", "LAUNCH SORTIE (PvE) ►", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(340, 0), new Vector2(230, 44), CyberVectorTheme.LaserGreen, defaultFont, 15, true);
            playBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadGameplay(GameMode.PvE);
                }
            });
        }

        private ShipEquipmentSlotDefinition[] GetCurrentShipSlots()
        {
            var def = PlayerProgression.GetShipDefinitionById(currentShipId);
            if (def != null)
            {
                var s = def.GetEquipmentSlots();
                if (s != null && s.Length > 0) return s;
            }

            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            var list = new List<ShipEquipmentSlotDefinition>();
            for (int i = 0; i < sockets.Length; i++)
            {
                list.Add(new ShipEquipmentSlotDefinition($"wpn_{i}", sockets[i].name, EquipmentSlotCategory.Weapon, sockets[i].localPosition, SocketSize.Medium, SocketMountType.FixedForward, "pulse"));
            }
            list.Add(new ShipEquipmentSlotDefinition("def_0", "Defensive Shield Bay", EquipmentSlotCategory.Defense, Vector2.zero, SocketSize.Medium, SocketMountType.FixedForward, "def_shield_booster"));
            list.Add(new ShipEquipmentSlotDefinition("eng_0", "Vector Thrusters", EquipmentSlotCategory.Engine, new Vector2(-1.2f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "eng_afterburner"));
            list.Add(new ShipEquipmentSlotDefinition("rea_0", "Flux Capacitor Core", EquipmentSlotCategory.Reactor, new Vector2(-0.4f, 0f), SocketSize.Medium, SocketMountType.FixedForward, "rea_flux_capacitor"));
            return list.ToArray();
        }

        public ShipEquipmentSlotDefinition GetCurrentSlot()
        {
            var slots = GetCurrentShipSlots();
            if (slots.Length == 0) return default;
            int idx = Mathf.Clamp(activeSlotIndex, 0, slots.Length - 1);
            return slots[idx];
        }

        public void EquipItemByIndex(int index)
        {
            var slot = GetCurrentSlot();
            if (slot.category == EquipmentSlotCategory.Weapon)
            {
                EquipWeaponByIndex(index);
            }
            else
            {
                var modules = EquipmentModuleCatalog.GetModulesByCategory(slot.category);
                if (index >= 0 && index < modules.Count)
                {
                    EquipModuleToSlot(activeSlotIndex, modules[index].id);
                }
            }
        }

        private void EquipWeaponByIndex(int index)
        {
            if (index >= 0 && index < WeaponCatalog.Length)
            {
                EquipWeaponToSocket(activeSocketIndex, WeaponCatalog[index].id);
            }
        }

        public void EquipWeapon(string weaponId)
        {
            EquipWeaponToSocket(activeSocketIndex, weaponId);
        }

        public void EquipWeaponToSocket(int socketIndex, string weaponId)
        {
            EnsureInitialized();
            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (socketIndex < 0 || socketIndex >= sockets.Length) socketIndex = 0;
            activeSocketIndex = socketIndex;
            inspectedWeaponId = weaponId;

            StorageService.SaveSocketWeapon(currentShipId, socketIndex, weaponId);

            var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
            if (weaponCtrl != null)
            {
                weaponCtrl.EquipFromStorage(currentShipId);
            }

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
        }

        public void EquipModuleToSlot(int slotIndex, string moduleId)
        {
            EnsureInitialized();
            var slots = GetCurrentShipSlots();
            if (slotIndex < 0 || slotIndex >= slots.Length) slotIndex = 0;
            activeSlotIndex = slotIndex;
            var slot = slots[slotIndex];

            if (slot.category == EquipmentSlotCategory.Weapon)
            {
                EquipWeaponToSocket(activeSocketIndex, moduleId);
                return;
            }

            inspectedModuleId = moduleId;
            StorageService.SaveEquippedModule(currentShipId, slot.slotId, moduleId);
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
        }

        public void SelectSlot(int slotIndex)
        {
            EnsureInitialized();
            var slots = GetCurrentShipSlots();
            if (slots.Length == 0) return;
            activeSlotIndex = Mathf.Clamp(slotIndex, 0, slots.Length - 1);
            var slot = slots[activeSlotIndex];

            if (slot.category == EquipmentSlotCategory.Weapon)
            {
                int wpnIdx = 0;
                for (int i = 0; i < activeSlotIndex; i++)
                {
                    if (slots[i].category == EquipmentSlotCategory.Weapon) wpnIdx++;
                }
                activeSocketIndex = wpnIdx;
                inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            }
            else
            {
                inspectedModuleId = StorageService.GetEquippedModule(currentShipId, slot.slotId, slot.defaultItemId);
            }

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
        }

        public void SelectSocket(int socketIndex)
        {
            EnsureInitialized();
            var slots = GetCurrentShipSlots();
            int wpnCount = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].category == EquipmentSlotCategory.Weapon)
                {
                    if (wpnCount == socketIndex)
                    {
                        SelectSlot(i);
                        return;
                    }
                    wpnCount++;
                }
            }

            activeSocketIndex = Mathf.Clamp(socketIndex, 0, Mathf.Max(0, ShipWeaponController.GetShipHardpointSockets(currentShipId).Length - 1));
            inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
        }

        public void SelectNextSocket()
        {
            SelectNextSlot();
        }

        public void SelectPreviousSocket()
        {
            SelectPreviousSlot();
        }

        public void SelectNextSlot()
        {
            var slots = GetCurrentShipSlots();
            if (slots.Length <= 1) return;
            SelectSlot((activeSlotIndex + 1) % slots.Length);
        }

        public void SelectPreviousSlot()
        {
            var slots = GetCurrentShipSlots();
            if (slots.Length <= 1) return;
            SelectSlot((activeSlotIndex - 1 + slots.Length) % slots.Length);
        }

        public void EquipSkill(string skillId)
        {
            EnsureInitialized();
            inspectedSkillId = skillId;
            StorageService.SaveSelectedSkill(skillId);

            var shipSkill = FindAnyObjectByType<ShipSkillController>();
            if (shipSkill != null)
            {
                shipSkill.EquipActiveSkill(skillId);
            }

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (gemsText != null)
            {
                gemsText.text = $"<color=#ffe600>META-GEMS: {StorageService.Profile.totalMetaGems}</color>";
            }

            var slots = GetCurrentShipSlots();
            if (activeSlotIndex >= slots.Length) activeSlotIndex = 0;
            var currentSlot = slots.Length > 0 ? slots[activeSlotIndex] : default;

            if (shipHeaderBanner != null)
            {
                shipHeaderBanner.text = $"<color=#ffe600>ACTIVE CHASSIS: [ {currentShipId.ToUpper()} // {slots.Length} EQUIPMENT SLOTS ]</color>";
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Render Interactive Sockets & Modules on Ship Hull Preview
            if (shipPreviewImage != null)
            {
                shipPreviewImage.sprite = ProceduralSpriteHelper.GetShipSprite(currentShipId);

                for (int i = shipPreviewImage.transform.childCount - 1; i >= 0; i--)
                {
                    var child = shipPreviewImage.transform.GetChild(i);
                    if (child.name.StartsWith("ShipSocket_") || child.name.StartsWith("ShipSlot_"))
                    {
                        if (Application.isPlaying) Destroy(child.gameObject);
                        else DestroyImmediate(child.gameObject);
                    }
                }

                Sprite spr = shipPreviewImage.sprite;
                float halfW = (spr != null && spr.bounds.size.x > 0f) ? spr.bounds.size.x * 0.5f : 2f;
                float halfH = (spr != null && spr.bounds.size.y > 0f) ? spr.bounds.size.y * 0.5f : 2f;

                float boxW = 200f;
                float boxH = 200f;

                int wpnCounter = 0;
                for (int i = 0; i < slots.Length; i++)
                {
                    var sData = slots[i];
                    bool isSelected = (i == activeSlotIndex);
                    int capturedIdx = i;

                    GameObject sockGo = new GameObject($"ShipSocket_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                    sockGo.transform.SetParent(shipPreviewImage.transform, false);

                    RectTransform sRt = sockGo.GetComponent<RectTransform>();
                    sRt.anchorMin = new Vector2(0.5f, 0.5f);
                    sRt.anchorMax = new Vector2(0.5f, 0.5f);
                    sRt.anchoredPosition = new Vector2((sData.localPosition.x / halfW) * (boxW * 0.44f), (sData.localPosition.y / halfH) * (boxH * 0.44f));
                    sRt.sizeDelta = new Vector2(34f, 34f);
                    sRt.localRotation = Quaternion.Euler(0f, 0f, -90f);

                    Image sImg = sockGo.GetComponent<Image>();
                    sImg.color = Color.white;
                    sImg.preserveAspect = true;

                    if (sData.category == EquipmentSlotCategory.Weapon)
                    {
                        string eqWpn = StorageService.GetSocketWeapon(currentShipId, wpnCounter);
                        sImg.sprite = !string.IsNullOrEmpty(eqWpn) ? ProceduralSpriteHelper.GetWeaponSprite(eqWpn) : ProceduralSpriteHelper.GetWeaponSocketSprite();
                        wpnCounter++;
                    }
                    else
                    {
                        string eqMod = StorageService.GetEquippedModule(currentShipId, sData.slotId, sData.defaultItemId);
                        sImg.sprite = ProceduralSpriteHelper.GetModuleIcon(eqMod, sData.category);
                    }

                    // Tactical Reticle
                    GameObject reticleGo = new GameObject("Reticle", typeof(RectTransform), typeof(Image));
                    reticleGo.transform.SetParent(sockGo.transform, false);
                    RectTransform rRt = reticleGo.GetComponent<RectTransform>();
                    rRt.anchorMin = new Vector2(0.5f, 0.5f);
                    rRt.anchorMax = new Vector2(0.5f, 0.5f);
                    rRt.anchoredPosition = Vector2.zero;
                    rRt.sizeDelta = isSelected ? new Vector2(46f, 46f) : new Vector2(36f, 36f);
                    rRt.localRotation = Quaternion.Euler(0f, 0f, 90f);

                    Image rImg = reticleGo.GetComponent<Image>();
                    rImg.sprite = CyberVectorTheme.GetTacticalReticleSprite();
                    rImg.color = isSelected ? CyberVectorTheme.VoltYellow : GetCategoryColor(sData.category, 0.55f);
                    rImg.raycastTarget = false;

                    // Tactical Badge Tag
                    GameObject tagGo = new GameObject("SocketTag", typeof(RectTransform));
                    tagGo.transform.SetParent(sockGo.transform, false);
                    RectTransform tRt = tagGo.GetComponent<RectTransform>();
                    tRt.anchorMin = new Vector2(0.5f, 0.5f);
                    tRt.anchorMax = new Vector2(0.5f, 0.5f);
                    tRt.anchoredPosition = new Vector2(0f, -28f);
                    tRt.sizeDelta = new Vector2(110f, 18f);
                    tRt.localRotation = Quaternion.Euler(0f, 0f, 90f);

                    string catCode = sData.category switch
                    {
                        EquipmentSlotCategory.Weapon => "WPN",
                        EquipmentSlotCategory.Defense => "DEF",
                        EquipmentSlotCategory.Engine => "ENG",
                        EquipmentSlotCategory.Reactor => "REA",
                        EquipmentSlotCategory.Auxiliary => "AUX",
                        _ => "SLOT"
                    };
                    string shortName = sData.slotName.Replace(" Hardpoint", "").Replace(" Socket", "").Replace(" Mount", "").ToUpper();
                    string tagText = isSelected
                        ? $"<color=#ffe600><b>► [{catCode}] {shortName}</b></color>"
                        : $"<color=#7e9bc4>[{catCode}] {shortName}</color>";

                    CreateText(tagGo.transform, "TagText", tagText, 8, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, defaultFont);

                    Button sBtn = sockGo.GetComponent<Button>();
                    CyberVectorTheme.ApplyVectorButtonStyle(sBtn, isSelected ? CyberVectorTheme.VoltYellow : GetCategoryColor(sData.category), isSelected);
                    sBtn.onClick.AddListener(() => SelectSlot(capturedIdx));
                }
            }

            // 2. Populate Equipment Slot Selection Tabs
            if (socketButtonContainer != null)
            {
                for (int i = socketButtonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = socketButtonContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                int sCount = slots.Length;
                float totalWidth = 430f;
                float btnWidth = Mathf.Clamp(totalWidth / Mathf.Max(1, sCount), 70f, 140f);
                float btnSpacing = sCount > 1 ? (totalWidth - (sCount * btnWidth)) / (sCount - 1) + btnWidth : 0f;
                float startX = sCount == 1 ? 0f : -((sCount - 1) * btnSpacing) / 2f;

                int wpnCounter = 0;
                for (int i = 0; i < sCount; i++)
                {
                    var sData = slots[i];
                    bool isSelected = (i == activeSlotIndex);
                    string mountedItemName;
                    Sprite itemIcon;

                    if (sData.category == EquipmentSlotCategory.Weapon)
                    {
                        string eqWpn = StorageService.GetSocketWeapon(currentShipId, wpnCounter);
                        var wDefLocal = GetWeaponDef(eqWpn);
                        mountedItemName = wDefLocal.name;
                        itemIcon = ProceduralSpriteHelper.GetWeaponSprite(eqWpn);
                        wpnCounter++;
                    }
                    else
                    {
                        string eqMod = StorageService.GetEquippedModule(currentShipId, sData.slotId, sData.defaultItemId);
                        var modInfo = EquipmentModuleCatalog.GetModule(eqMod);
                        mountedItemName = modInfo != null ? modInfo.name : "EMPTY";
                        itemIcon = ProceduralSpriteHelper.GetModuleIcon(eqMod, sData.category);
                    }

                    string catCode = sData.category switch
                    {
                        EquipmentSlotCategory.Weapon => "WPN",
                        EquipmentSlotCategory.Defense => "DEF",
                        EquipmentSlotCategory.Engine => "ENG",
                        EquipmentSlotCategory.Reactor => "REA",
                        EquipmentSlotCategory.Auxiliary => "AUX",
                        _ => "MOD"
                    };

                    string sLabel = isSelected
                        ? $"<color=#ffe600><b>► [{catCode}]</b></color>\n<color=#00f2ff>[{mountedItemName.ToUpper()}]</color>"
                        : $"<color=#ffffff>[{catCode}]</color>\n<color=#7e9bc4>[{mountedItemName.ToUpper()}]</color>";

                    Color sCol = isSelected ? CyberVectorTheme.VoltYellow : CyberVectorTheme.DimMuted;
                    int capturedIdx = i;

                    GameObject sBtnObj = CreateButton(socketButtonContainer, $"SlotBtn_{i}", "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(btnWidth, 44), sCol, defaultFont, 8);

                    GameObject sBaseObj = new GameObject("SlotBaseIcon", typeof(RectTransform), typeof(Image));
                    sBaseObj.transform.SetParent(sBtnObj.transform, false);
                    RectTransform sBaseRt = sBaseObj.GetComponent<RectTransform>();
                    sBaseRt.anchorMin = new Vector2(0.04f, 0.5f);
                    sBaseRt.anchorMax = new Vector2(0.04f, 0.5f);
                    sBaseRt.anchoredPosition = new Vector2(12f, 0f);
                    sBaseRt.sizeDelta = new Vector2(22f, 22f);
                    var sBaseImg = sBaseObj.GetComponent<Image>();
                    sBaseImg.color = Color.white;
                    sBaseImg.preserveAspect = true;
                    sBaseImg.sprite = itemIcon;

                    CreateText(sBtnObj.transform, "Text", sLabel, 8, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.32f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, defaultFont);

                    Button btn = sBtnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => SelectSlot(capturedIdx));
                }
            }

            // 3. Populate Outfitting Matrix based on Active Slot Category
            if (currentSlot.category == EquipmentSlotCategory.Weapon)
            {
                if (socketHeaderPrompt != null)
                {
                    socketHeaderPrompt.text = $"<color=#00f2ff>SELECT WEAPON FOR <b>[{currentSlot.slotName}]</b> [1-5]:</color>";
                }

                string mountedOnActiveSocket = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);

                if (weaponButtonContainer != null)
                {
                    for (int i = weaponButtonContainer.childCount - 1; i >= 0; i--)
                    {
                        var child = weaponButtonContainer.GetChild(i);
                        if (Application.isPlaying) Destroy(child.gameObject);
                        else DestroyImmediate(child.gameObject);
                    }

                    float btnSpacing = 82f;
                    float startX = -((WeaponCatalog.Length - 1) * btnSpacing) / 2f;

                    for (int i = 0; i < WeaponCatalog.Length; i++)
                    {
                        var w = WeaponCatalog[i];
                        bool isMountedOnActive = string.Equals(w.id, mountedOnActiveSocket, System.StringComparison.OrdinalIgnoreCase);
                        bool isInspected = string.Equals(w.id, inspectedWeaponId, System.StringComparison.OrdinalIgnoreCase);

                        string archTag = w.archetype switch
                        {
                            WeaponArchetype.Beam => "BEAM",
                            WeaponArchetype.Traceable => "TRACE",
                            _ => "PROJ"
                        };

                        string label = isMountedOnActive
                            ? $"<color=#ffe600>[{w.name.ToUpper()}]</color>\n★ MOUNTED"
                            : $"{w.name.ToUpper()}\n<color=#7e9bc4>[{archTag}]</color>";
                        Color col = isMountedOnActive ? CyberVectorTheme.VoltYellow : (isInspected ? CyberVectorTheme.NeonCyan : CyberVectorTheme.DimMuted);

                        string capturedWId = w.id;
                        GameObject btnObj = CreateWeaponButtonWithIcon(weaponButtonContainer, $"WpnBtn_{w.id}", label, w.id, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(78, 50), col, defaultFont, 8);
                        Button btn = btnObj.GetComponent<Button>();
                        btn.onClick.AddListener(() => EquipWeaponToSocket(activeSocketIndex, capturedWId));
                    }
                }

                // Weapon Inspector Card
                var wDef = GetWeaponDef(inspectedWeaponId);
                if (weaponDetailIcon != null)
                {
                    weaponDetailIcon.sprite = ProceduralSpriteHelper.GetWeaponSprite(inspectedWeaponId);
                }
                if (weaponDetailTitle != null)
                {
                    string archBadge = wDef.archetype switch
                    {
                        WeaponArchetype.Beam => "<color=#5dade2>[ BEAM ]</color>",
                        WeaponArchetype.Traceable => "<color=#e67e22>[ TRACEABLE ]</color>",
                        _ => "<color=#95a5a6>[ PROJECTILE ]</color>"
                    };
                    weaponDetailTitle.text = $"{archBadge} <color=#ffe600><b>{wDef.name.ToUpper()}</b></color>  <color=#7e9bc4>[ {wDef.classType} ]</color>";
                }
                if (weaponDetailStats != null)
                {
                    weaponDetailStats.text =
                        CyberVectorTheme.FormatStatMeter("BASE DAMAGE", wDef.damage, 100, "DMG", CyberVectorTheme.VoltYellow) + "\n" +
                        CyberVectorTheme.FormatStatMeter("CYCLIC RATE", wDef.fireRate, 10, "/SEC", CyberVectorTheme.NeonCyan) + "\n" +
                        CyberVectorTheme.FormatStatMeter("ENERGY DRAIN", wDef.energyCost, 20, "ENG", CyberVectorTheme.NeonMagenta) + "\n" +
                        $"<color=#00f2ff>DISPERSION     </color>  <color=#ffffff>{wDef.spread}</color>";
                }
                if (weaponDetailDesc != null)
                {
                    weaponDetailDesc.text = $"<color=#7e9bc4>\"{wDef.description}\"</color>\n<color=#00f2ff>HARDPOINT TELEMETRY: </color><color=#ffe600>[{currentSlot.slotName}]</color> <color=#526b94>LOCAL: ({currentSlot.localPosition.x:F2}, {currentSlot.localPosition.y:F2})</color>";
                }
            }
            else
            {
                // Non-weapon equipment module category
                if (socketHeaderPrompt != null)
                {
                    socketHeaderPrompt.text = $"<color=#00f2ff>SELECT {currentSlot.category.ToString().ToUpper()} MODULE FOR <b>[{currentSlot.slotName}]</b> [1-3]:</color>";
                }

                string mountedModId = StorageService.GetEquippedModule(currentShipId, currentSlot.slotId, currentSlot.defaultItemId);
                var moduleList = EquipmentModuleCatalog.GetModulesByCategory(currentSlot.category);

                if (weaponButtonContainer != null)
                {
                    for (int i = weaponButtonContainer.childCount - 1; i >= 0; i--)
                    {
                        var child = weaponButtonContainer.GetChild(i);
                        if (Application.isPlaying) Destroy(child.gameObject);
                        else DestroyImmediate(child.gameObject);
                    }

                    float btnSpacing = 135f;
                    float startX = -((moduleList.Count - 1) * btnSpacing) / 2f;

                    for (int i = 0; i < moduleList.Count; i++)
                    {
                        var mod = moduleList[i];
                        bool isMountedOnActive = string.Equals(mod.id, mountedModId, System.StringComparison.OrdinalIgnoreCase);
                        bool isInspected = string.Equals(mod.id, inspectedModuleId, System.StringComparison.OrdinalIgnoreCase);

                        string label = isMountedOnActive
                            ? $"<color=#ffe600>[{mod.name.ToUpper()}]</color>\n★ INSTALLED"
                            : $"{mod.name.ToUpper()}\n<color=#7e9bc4>[{currentSlot.category}]</color>";

                        Color col = isMountedOnActive ? CyberVectorTheme.VoltYellow : (isInspected ? CyberVectorTheme.NeonCyan : CyberVectorTheme.DimMuted);
                        string capturedModId = mod.id;

                        GameObject btnObj = CreateModuleButtonWithIcon(weaponButtonContainer, $"ModBtn_{mod.id}", label, mod.id, currentSlot.category, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(130, 50), col, defaultFont, 8);
                        Button btn = btnObj.GetComponent<Button>();
                        btn.onClick.AddListener(() => EquipModuleToSlot(activeSlotIndex, capturedModId));
                    }
                }

                // Module Inspector Card
                var modDef = EquipmentModuleCatalog.GetModule(inspectedModuleId) ?? (moduleList.Count > 0 ? moduleList[0] : null);
                if (modDef != null)
                {
                    if (weaponDetailIcon != null)
                    {
                        weaponDetailIcon.sprite = ProceduralSpriteHelper.GetModuleIcon(modDef.id, modDef.category);
                    }
                    if (weaponDetailTitle != null)
                    {
                        weaponDetailTitle.text = $"<color=#00f2ff>[ {modDef.category.ToString().ToUpper()} ]</color> <color=#ffe600><b>{modDef.name.ToUpper()}</b></color>  <color=#7e9bc4>[ {modDef.role} ]</color>";
                    }
                    if (weaponDetailStats != null)
                    {
                        string stat1 = "";
                        string stat2 = "";
                        string stat3 = "";

                        switch (modDef.category)
                        {
                            case EquipmentSlotCategory.Defense:
                                stat1 = CyberVectorTheme.FormatStatMeter("SHIELD CAPACITY", modDef.shieldCapacityBonus, 100, "+SHD", CyberVectorTheme.NeonCyan);
                                stat2 = CyberVectorTheme.FormatStatMeter("HULL INTEGRITY", modDef.hullCapacityBonus, 150, "+HULL", CyberVectorTheme.VoltYellow);
                                stat3 = $"<color=#00f2ff>COLLISION ARMOR: </color><color=#ffe600>+{modDef.collisionArmorBonus:P0} RESIST</color>  <color=#7e9bc4>REGEN: +{modDef.shieldRegenBonus}/s</color>";
                                break;
                            case EquipmentSlotCategory.Engine:
                                stat1 = CyberVectorTheme.FormatStatMeter("CRUISE VELOCITY", modDef.speedBonus, 100, "+SPD", CyberVectorTheme.LaserGreen);
                                stat2 = CyberVectorTheme.FormatStatMeter("ANGULAR AGILITY", modDef.agilityBonus * 10f, 40, "+AGL", CyberVectorTheme.VoltYellow);
                                stat3 = $"<color=#00f2ff>POWER CONSUMPTION: </color><color=#7e9bc4>OPTIMIZED VECTOR RCS</color>";
                                break;
                            case EquipmentSlotCategory.Reactor:
                                stat1 = CyberVectorTheme.FormatStatMeter("CAPACITOR POOL", modDef.energyCapacityBonus, 100, "+ENG", CyberVectorTheme.NeonMagenta);
                                stat2 = CyberVectorTheme.FormatStatMeter("FAST RECHARGE", modDef.energyRegenBonus, 20, "+REG/S", CyberVectorTheme.VoltYellow);
                                stat3 = $"<color=#00f2ff>OVERDRAW MITIGATION: </color><color=#ffe600>-{modDef.overdrawResistanceBonus:P0} BURNOUT</color>";
                                break;
                            case EquipmentSlotCategory.Auxiliary:
                                stat1 = CyberVectorTheme.FormatStatMeter("DAMAGE BOOST", (modDef.damageMultiplier - 1f) * 100f, 50, "%AMP", CyberVectorTheme.VoltYellow);
                                stat2 = CyberVectorTheme.FormatStatMeter("TRACTOR RADIUS", modDef.pickupRadiusBonus * 10f, 60, "+RAD", CyberVectorTheme.NeonCyan);
                                stat3 = $"<color=#00f2ff>TACTICAL TELEMETRY: </color><color=#7e9bc4>AUTONOMOUS FLEET BUS</color>";
                                break;
                        }

                        weaponDetailStats.text = $"{stat1}\n{stat2}\n{stat3}";
                    }
                    if (weaponDetailDesc != null)
                    {
                        weaponDetailDesc.text = $"<color=#7e9bc4>\"{modDef.description}\"</color>\n<color=#00f2ff>SLOT TELEMETRY: </color><color=#ffe600>[{currentSlot.slotName}]</color> <color=#526b94>LOCAL: ({currentSlot.localPosition.x:F2}, {currentSlot.localPosition.y:F2})</color>";
                    }
                }
            }

            // 4. Populate Tactical Combat Modules (Skills)
            string equippedSkl = StorageService.GetSelectedSkill();
            if (skillButtonContainer != null)
            {
                for (int i = skillButtonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = skillButtonContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                float btnSpacing = 136f;
                float startX = -((SkillCatalog.Length - 1) * btnSpacing) / 2f;

                for (int i = 0; i < SkillCatalog.Length; i++)
                {
                    var s = SkillCatalog[i];
                    bool isEquipped = string.Equals(s.id, equippedSkl, System.StringComparison.OrdinalIgnoreCase);
                    bool isInspected = string.Equals(s.id, inspectedSkillId, System.StringComparison.OrdinalIgnoreCase);

                    string label = isEquipped ? $"<color=#00f2ff>[{s.name.ToUpper()}]</color>\n★ MOUNTED" : $"{s.name.ToUpper()}";
                    Color col = isEquipped ? CyberVectorTheme.NeonCyan : (isInspected ? CyberVectorTheme.VoltYellow : CyberVectorTheme.DimMuted);

                    string capturedSId = s.id;
                    GameObject btnObj = CreateButton(skillButtonContainer, $"SklBtn_{s.id}", label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(130, 44), col, defaultFont, 10);
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => EquipSkill(capturedSId));
                }
            }

            // 5. Update Skill Inspector Card
            var sDef = GetSkillDef(inspectedSkillId);
            if (skillDetailTitle != null)
            {
                skillDetailTitle.text = $"<color=#00f2ff><b>{sDef.name.ToUpper()}</b></color>  <color=#ffe600>[ {sDef.role} ]</color>";
            }
            if (skillDetailDesc != null)
            {
                skillDetailDesc.text =
                    CyberVectorTheme.FormatStatMeter("CAPACITOR DRAIN", sDef.energyCost, 50, "ENG", CyberVectorTheme.VoltYellow) + "\n" +
                    CyberVectorTheme.FormatStatMeter("RECHARGE CYCLE", sDef.cooldown, 10, "SEC", CyberVectorTheme.NeonCyan) + "\n\n" +
                    $"<color=#7e9bc4>\"{sDef.description}\"</color>";
            }
        }

        private Color GetCategoryColor(EquipmentSlotCategory category, float alpha = 1.0f)
        {
            Color c = category switch
            {
                EquipmentSlotCategory.Weapon => CyberVectorTheme.NeonCyan,
                EquipmentSlotCategory.Defense => new Color(0.2f, 0.85f, 1f),
                EquipmentSlotCategory.Engine => new Color(1f, 0.55f, 0.15f),
                EquipmentSlotCategory.Reactor => CyberVectorTheme.VoltYellow,
                EquipmentSlotCategory.Auxiliary => new Color(0.75f, 0.35f, 1f),
                _ => CyberVectorTheme.NeonCyan
            };
            c.a = alpha;
            return c;
        }

        private GameObject CreateModuleButtonWithIcon(Transform parent, string name, string label, string moduleId, EquipmentSlotCategory category, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 8)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.sprite = CyberVectorTheme.GetButtonSprite();
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            });
            CyberVectorTheme.ApplyVectorButtonStyle(btn, bgColor, false);

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.68f);
            iconRt.anchorMax = new Vector2(0.5f, 0.68f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(26f, 26f);
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = ProceduralSpriteHelper.GetModuleIcon(moduleId, category);
            iconImg.preserveAspect = true;

            CreateText(btnObj.transform, "Text", label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 0.02f), new Vector2(1f, 0.45f), Vector2.zero, Vector2.zero, font);
            return btnObj;
        }

        private WeaponDefinitionInfo GetWeaponDef(string id)
        {
            if (string.IsNullOrEmpty(id)) return WeaponCatalog[0];
            foreach (var w in WeaponCatalog)
            {
                if (string.Equals(w.id, id, System.StringComparison.OrdinalIgnoreCase)) return w;
            }
            return WeaponCatalog[0];
        }

        private SkillDefinitionInfo GetSkillDef(string id)
        {
            if (string.IsNullOrEmpty(id)) return SkillCatalog[0];
            foreach (var s in SkillCatalog)
            {
                if (string.Equals(s.id, id, System.StringComparison.OrdinalIgnoreCase)) return s;
            }
            return SkillCatalog[0];
        }

        private void GoToHangar()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadShipSelect();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GameManager.SceneShipSelect);
            }
        }

        private void GoToMainMenu()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GameManager.SceneMainMenu);
            }
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font = null)
            => UIHelper.CreateText(parent, name, text, fontSize, fontStyle, color, alignment, anchorMin, anchorMax, offsetMin, offsetMax, font);

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 16, bool isPrimary = false)
            => UIHelper.CreateButton(parent, name, label, anchorMin, anchorMax, pos, size, bgColor, font, fontSize, isPrimary);

        private GameObject CreateWeaponButtonWithIcon(Transform parent, string name, string label, string weaponId, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 9)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.sprite = CyberVectorTheme.GetButtonSprite();
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            });
            CyberVectorTheme.ApplyVectorButtonStyle(btn, bgColor, false);

            // Weapon Icon on upper area
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(btnObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.68f);
            iconRt.anchorMax = new Vector2(0.5f, 0.68f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(28f, 28f);
            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = ProceduralSpriteHelper.GetWeaponSprite(weaponId);
            iconImg.preserveAspect = true;

            // Label on lower area
            CreateText(btnObj.transform, "Text", label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0f, 0.02f), new Vector2(1f, 0.45f), Vector2.zero, Vector2.zero, font);

            return btnObj;
        }
    }
}
