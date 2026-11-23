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

        public string ScreenId => "EquipmentSelect";

        private GameObject rootContainer;
        private TMP_Text gemsText;
        private TMP_Text shipHeaderBanner;
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
        private string inspectedWeaponId = "pulse";
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

        public string InspectedWeaponId
        {
            get
            {
                EnsureInitialized();
                return inspectedWeaponId;
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
            if (activeSocketIndex >= sockets.Length) activeSocketIndex = 0;
            inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            inspectedSkillId = StorageService.GetSelectedSkill();

            BuildUI();
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

            if (Keyboard.current.digit1Key.wasPressedThisFrame) EquipWeaponByIndex(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) EquipWeaponByIndex(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) EquipWeaponByIndex(2);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) EquipWeaponByIndex(3);
            else if (Keyboard.current.digit5Key.wasPressedThisFrame) EquipWeaponByIndex(4);
            else if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) SelectPreviousSocket();
            else if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame) SelectNextSocket();
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

            // SECTION 1: PRIMARY WEAPONS (Left Chamfer Panel)
            GameObject wpnPanel = new GameObject("WeaponPanel", typeof(RectTransform), typeof(Image));
            wpnPanel.transform.SetParent(rootContainer.transform, false);
            RectTransform wpRt = wpnPanel.GetComponent<RectTransform>();
            wpRt.anchorMin = new Vector2(0.04f, 0.17f);
            wpRt.anchorMax = new Vector2(0.52f, 0.82f);
            wpRt.offsetMin = Vector2.zero;
            wpRt.offsetMax = Vector2.zero;
            var wpImg = wpnPanel.GetComponent<Image>();
            wpImg.sprite = CyberVectorTheme.GetChamferCardSprite();
            wpImg.type = Image.Type.Sliced;
            wpImg.color = Color.white;

            CreateText(wpnPanel.transform, "WpnHeader", "<color=#ff8514><b>1. PRIMARY WEAPON HARDPOINTS</b></color> <color=#00f2ff>[SELECT SOCKET TO MOUNT]</color>", 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.94f), new Vector2(0.05f, 0.94f), new Vector2(0, -15), new Vector2(460, 15), defaultFont);

            // Hardpoint Sockets Bar (Interactive Socket Tabs)
            GameObject socketBtnContainerObj = new GameObject("SocketButtons", typeof(RectTransform));
            socketBtnContainerObj.transform.SetParent(wpnPanel.transform, false);
            RectTransform sbcRt = socketBtnContainerObj.GetComponent<RectTransform>();
            sbcRt.anchorMin = new Vector2(0.04f, 0.77f);
            sbcRt.anchorMax = new Vector2(0.96f, 0.91f);
            sbcRt.offsetMin = Vector2.zero;
            sbcRt.offsetMax = Vector2.zero;
            socketButtonContainer = socketBtnContainerObj.transform;

            // Prompt Subtitle
            GameObject promptObj = CreateText(wpnPanel.transform, "SocketPrompt", "<color=#00f2ff>SELECT WEAPON FOR ACTIVE SOCKET [Q/E]:</color>", 12, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.73f), new Vector2(0.05f, 0.73f), new Vector2(0, -10), new Vector2(460, 10), defaultFont);
            socketHeaderPrompt = promptObj.GetComponent<TMP_Text>();

            // Weapon Selection Buttons
            GameObject wpnBtnContainerObj = new GameObject("WeaponButtons", typeof(RectTransform));
            wpnBtnContainerObj.transform.SetParent(wpnPanel.transform, false);
            RectTransform wbcRt = wpnBtnContainerObj.GetComponent<RectTransform>();
            wbcRt.anchorMin = new Vector2(0.04f, 0.54f);
            wbcRt.anchorMax = new Vector2(0.96f, 0.70f);
            wbcRt.offsetMin = Vector2.zero;
            wbcRt.offsetMax = Vector2.zero;
            weaponButtonContainer = wpnBtnContainerObj.transform;

            // Weapon Detail Card (Inner Sliced Frame)
            GameObject wpnDetailObj = new GameObject("WeaponDetailCard", typeof(RectTransform), typeof(Image));
            wpnDetailObj.transform.SetParent(wpnPanel.transform, false);
            RectTransform wdRt = wpnDetailObj.GetComponent<RectTransform>();
            wdRt.anchorMin = new Vector2(0.04f, 0.04f);
            wdRt.anchorMax = new Vector2(0.96f, 0.50f);
            wdRt.offsetMin = Vector2.zero;
            wdRt.offsetMax = Vector2.zero;
            var wdImg = wpnDetailObj.GetComponent<Image>();
            wdImg.sprite = CyberVectorTheme.GetChamferCardSprite(128, 12);
            wdImg.type = Image.Type.Sliced;
            wdImg.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);

            GameObject wdtObj = CreateText(wpnDetailObj.transform, "WpnTitle", "PULSE LASER", 16, FontStyle.Bold, CyberVectorTheme.VoltYellow, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.86f), new Vector2(0.05f, 0.86f), new Vector2(0, -15), new Vector2(400, 15), defaultFont);
            weaponDetailTitle = wdtObj.GetComponent<TMP_Text>();

            // Weapon Icon Preview in Detail Card
            GameObject iconObj = new GameObject("WeaponIconPreview", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(wpnDetailObj.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.82f, 0.44f);
            iconRt.anchorMax = new Vector2(0.96f, 0.90f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            weaponDetailIcon = iconObj.GetComponent<Image>();
            weaponDetailIcon.preserveAspect = true;

            GameObject wdsObj = CreateText(wpnDetailObj.transform, "WpnStats", "STATS", 12, FontStyle.Normal, Color.white, TextAnchor.UpperLeft, new Vector2(0.05f, 0.53f), new Vector2(0.05f, 0.53f), new Vector2(0, -35), new Vector2(420, 35), defaultFont);
            weaponDetailStats = wdsObj.GetComponent<TMP_Text>();

            GameObject wddObj = CreateText(wpnDetailObj.transform, "WpnDesc", "DESCRIPTION", 11, FontStyle.Italic, new Color(0.75f, 0.85f, 0.98f), TextAnchor.UpperLeft, new Vector2(0.05f, 0.18f), new Vector2(0.05f, 0.18f), new Vector2(0, -25), new Vector2(420, 25), defaultFont);
            weaponDetailDesc = wddObj.GetComponent<TMP_Text>();

            // SECTION 2: COMBAT SKILLS (Right Chamfer Panel)
            GameObject sklPanel = new GameObject("SkillPanel", typeof(RectTransform), typeof(Image));
            sklPanel.transform.SetParent(rootContainer.transform, false);
            RectTransform skpRt = sklPanel.GetComponent<RectTransform>();
            skpRt.anchorMin = new Vector2(0.55f, 0.17f);
            skpRt.anchorMax = new Vector2(0.96f, 0.82f);
            skpRt.offsetMin = Vector2.zero;
            skpRt.offsetMax = Vector2.zero;
            var skpImg = sklPanel.GetComponent<Image>();
            skpImg.sprite = CyberVectorTheme.GetChamferCardSprite();
            skpImg.type = Image.Type.Sliced;
            skpImg.color = Color.white;

            CreateText(sklPanel.transform, "SklHeader", "<color=#00f2ff><b>2. TACTICAL COMBAT MODULES</b></color> <color=#526b94>[TRIGGER: SPACE]</color>", 15, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.93f), new Vector2(0.05f, 0.93f), new Vector2(0, -15), new Vector2(420, 15), defaultFont);

            GameObject sklBtnContainerObj = new GameObject("SkillButtons", typeof(RectTransform));
            sklBtnContainerObj.transform.SetParent(sklPanel.transform, false);
            RectTransform skbcRt = sklBtnContainerObj.GetComponent<RectTransform>();
            skbcRt.anchorMin = new Vector2(0.04f, 0.52f);
            skbcRt.anchorMax = new Vector2(0.96f, 0.85f);
            skbcRt.offsetMin = Vector2.zero;
            skbcRt.offsetMax = Vector2.zero;
            skillButtonContainer = sklBtnContainerObj.transform;

            // Skill Detail Card (Inner Sliced Frame)
            GameObject sklDetailObj = new GameObject("SkillDetailCard", typeof(RectTransform), typeof(Image));
            sklDetailObj.transform.SetParent(sklPanel.transform, false);
            RectTransform sdRt = sklDetailObj.GetComponent<RectTransform>();
            sdRt.anchorMin = new Vector2(0.04f, 0.05f);
            sdRt.anchorMax = new Vector2(0.96f, 0.48f);
            sdRt.offsetMin = Vector2.zero;
            sdRt.offsetMax = Vector2.zero;
            var sdImg = sklDetailObj.GetComponent<Image>();
            sdImg.sprite = CyberVectorTheme.GetChamferCardSprite(128, 12);
            sdImg.type = Image.Type.Sliced;
            sdImg.color = new Color(0.02f, 0.05f, 0.10f, 0.95f);

            GameObject sktObj = CreateText(sklDetailObj.transform, "SklTitle", "KINETIC SHOCKWAVE", 17, FontStyle.Bold, CyberVectorTheme.NeonCyan, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.84f), new Vector2(0.05f, 0.84f), new Vector2(0, -15), new Vector2(400, 15), defaultFont);
            skillDetailTitle = sktObj.GetComponent<TMP_Text>();

            GameObject skdObj = CreateText(sklDetailObj.transform, "SklDesc", "DESCRIPTION", 13, FontStyle.Normal, new Color(0.85f, 0.9f, 1f), TextAnchor.UpperLeft, new Vector2(0.05f, 0.45f), new Vector2(0.05f, 0.45f), new Vector2(0, -45), new Vector2(400, 45), defaultFont);
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

        public void SelectSocket(int socketIndex)
        {
            EnsureInitialized();
            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (sockets.Length == 0) return;
            activeSocketIndex = Mathf.Clamp(socketIndex, 0, sockets.Length - 1);
            inspectedWeaponId = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
        }

        public void SelectNextSocket()
        {
            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (sockets.Length <= 1) return;
            SelectSocket((activeSocketIndex + 1) % sockets.Length);
        }

        public void SelectPreviousSocket()
        {
            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (sockets.Length <= 1) return;
            SelectSocket((activeSocketIndex - 1 + sockets.Length) % sockets.Length);
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

            var sockets = ShipWeaponController.GetShipHardpointSockets(currentShipId);
            if (activeSocketIndex >= sockets.Length) activeSocketIndex = 0;

            if (shipHeaderBanner != null)
            {
                shipHeaderBanner.text = $"<color=#ffe600>ACTIVE CHASSIS: [ {currentShipId.ToUpper()} // {sockets.Length} HARDPOINTS DETECTED ]</color>";
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Populate Socket Selection Tabs
            if (socketButtonContainer != null)
            {
                for (int i = socketButtonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = socketButtonContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                int sCount = sockets.Length;
                float totalWidth = 440f;
                float btnWidth = sCount == 1 ? 360f : (sCount == 2 ? 210f : 142f);
                float btnSpacing = sCount > 1 ? (totalWidth - (sCount * btnWidth)) / (sCount - 1) + btnWidth : 0f;
                float startX = sCount == 1 ? 0f : -((sCount - 1) * btnSpacing) / 2f;

                for (int i = 0; i < sCount; i++)
                {
                    var sData = sockets[i];
                    string mountedWId = StorageService.GetSocketWeapon(currentShipId, sData.index);
                    var mountedDef = GetWeaponDef(mountedWId);
                    bool isSelected = (i == activeSocketIndex);

                    string sLabel = isSelected
                        ? $"<color=#ffe600><b>► {sData.name}</b></color>\n<color=#00f2ff>[{mountedDef.name.ToUpper()}]</color>"
                        : $"<color=#ffffff>{sData.name}</color>\n<color=#7e9bc4>[{mountedDef.name.ToUpper()}]</color>";

                    Color sCol = isSelected ? CyberVectorTheme.VoltYellow : CyberVectorTheme.DimMuted;
                    int capturedIdx = i;

                    GameObject sBtnObj = CreateButton(socketButtonContainer, $"SocketBtn_{i}", "", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(btnWidth, 48), sCol, defaultFont, 10);
                    
                    // Socket Icon on Left
                    GameObject sBaseObj = new GameObject("SocketBaseIcon", typeof(RectTransform), typeof(Image));
                    sBaseObj.transform.SetParent(sBtnObj.transform, false);
                    RectTransform sBaseRt = sBaseObj.GetComponent<RectTransform>();
                    sBaseRt.anchorMin = new Vector2(0.04f, 0.5f);
                    sBaseRt.anchorMax = new Vector2(0.04f, 0.5f);
                    sBaseRt.anchoredPosition = new Vector2(18f, 0f);
                    sBaseRt.sizeDelta = new Vector2(28f, 28f);
                    var sBaseImg = sBaseObj.GetComponent<Image>();
                    sBaseImg.color = Color.white;
                    sBaseImg.preserveAspect = true;

                    // If weapon socket is installed, replace it with the weapon sprite.
                    // Otherwise, display the empty weapon socket sprite.
                    if (!string.IsNullOrEmpty(mountedWId))
                    {
                        sBaseImg.sprite = ProceduralSpriteHelper.GetWeaponSprite(mountedWId);
                    }
                    else
                    {
                        sBaseImg.sprite = ProceduralSpriteHelper.GetWeaponSocketSprite();
                    }

                    // Label offset to right of icon
                    CreateText(sBtnObj.transform, "Text", sLabel, 9, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, new Vector2(0.24f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, defaultFont);

                    Button btn = sBtnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => SelectSocket(capturedIdx));
                }
            }

            if (socketHeaderPrompt != null && sockets.Length > 0)
            {
                socketHeaderPrompt.text = $"<color=#00f2ff>SELECT WEAPON FOR <b>[{sockets[activeSocketIndex].name}]</b> [1-5]:</color>";
            }

            string mountedOnActiveSocket = StorageService.GetSocketWeapon(currentShipId, activeSocketIndex);
            string equippedSkl = StorageService.GetSelectedSkill();

            // Populate Weapon Buttons with Icons
            if (weaponButtonContainer != null)
            {
                for (int i = weaponButtonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = weaponButtonContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                float btnSpacing = 86f;
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
                    Color col;
                    if (isMountedOnActive) col = CyberVectorTheme.VoltYellow;
                    else if (isInspected) col = CyberVectorTheme.NeonCyan;
                    else col = CyberVectorTheme.DimMuted;

                    string capturedWId = w.id;
                    GameObject btnObj = CreateWeaponButtonWithIcon(weaponButtonContainer, $"WpnBtn_{w.id}", label, w.id, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(82, 54), col, defaultFont, 8);
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => EquipWeaponToSocket(activeSocketIndex, capturedWId));
                }
            }

            // Populate Skill Buttons
            if (skillButtonContainer != null)
            {
                for (int i = skillButtonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = skillButtonContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                float btnSpacing = 138f;
                float startX = -((SkillCatalog.Length - 1) * btnSpacing) / 2f;

                for (int i = 0; i < SkillCatalog.Length; i++)
                {
                    var s = SkillCatalog[i];
                    bool isEquipped = string.Equals(s.id, equippedSkl, System.StringComparison.OrdinalIgnoreCase);
                    bool isInspected = string.Equals(s.id, inspectedSkillId, System.StringComparison.OrdinalIgnoreCase);

                    string label = isEquipped ? $"<color=#00f2ff>[{s.name.ToUpper()}]</color>\n★ MOUNTED" : $"{s.name.ToUpper()}";
                    Color col;
                    if (isEquipped) col = CyberVectorTheme.NeonCyan;
                    else if (isInspected) col = CyberVectorTheme.VoltYellow;
                    else col = CyberVectorTheme.DimMuted;

                    string capturedSId = s.id;
                    GameObject btnObj = CreateButton(skillButtonContainer, $"SklBtn_{s.id}", label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(132, 48), col, defaultFont, 11);
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => EquipSkill(capturedSId));
                }
            }

            // Update Weapon Inspector Card
            var wDef = GetWeaponDef(inspectedWeaponId);
            var activeSocket = (sockets.Length > 0 && activeSocketIndex < sockets.Length) ? sockets[activeSocketIndex] : new ShipWeaponController.HardpointSocketData { name = "NOSE HARDPOINT", localPosition = Vector2.zero };

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
                weaponDetailDesc.text = $"<color=#7e9bc4>\"{wDef.description}\"</color>\n<color=#00f2ff>HARDPOINT TELEMETRY: </color><color=#ffe600>[{activeSocket.name}]</color> <color=#526b94>LOCAL: ({activeSocket.localPosition.x:F2}, {activeSocket.localPosition.y:F2})</color>";
            }

            // Update Skill Inspector Card
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
        {
            FontStyles style = (fontStyle == FontStyle.Bold) ? FontStyles.Bold : FontStyles.Normal;
            var tmp = UIHelper.CreateTextMeshPro(parent, name, text, fontSize, style, color, UIHelper.ConvertTextAnchor(alignment), anchorMin, anchorMax, offsetMin, offsetMax);
            return tmp.gameObject;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 16, bool isPrimary = false)
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
            CyberVectorTheme.ApplyVectorButtonStyle(btn, bgColor, isPrimary);

            CreateText(btnObj.transform, "Text", label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, font);
            return btnObj;
        }

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
