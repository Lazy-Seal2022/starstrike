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
        private Transform weaponButtonContainer;
        private Transform skillButtonContainer;
        private TMP_Text weaponDetailTitle;
        private TMP_Text weaponDetailStats;
        private TMP_Text weaponDetailDesc;
        private TMP_Text skillDetailTitle;
        private TMP_Text skillDetailDesc;

        private string currentShipId = "scout";
        private string inspectedWeaponId = "pulse";
        private string inspectedSkillId = "forward_shockwave";

        public string CurrentShipId => currentShipId;
        public string InspectedWeaponId => inspectedWeaponId;
        public string InspectedSkillId => inspectedSkillId;

        public struct WeaponDefinitionInfo
        {
            public string id;
            public string name;
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
                classType = "DIRECT ENERGY // STANDARD ISSUE",
                damage = 22f,
                fireRate = 4.5f,
                energyCost = 6f,
                spread = "0.0° Pinpoint Accuracy",
                description = "Balanced rapid-cycling plasma bolts delivering continuous frontline fire with zero recoil drift."
            },
            new WeaponDefinitionInfo {
                id = "scattergun",
                name = "Scattergun",
                classType = "HIGH BURST // SPREAD CONE",
                damage = 75f, // 15 * 5 pellets
                fireRate = 2.2f,
                energyCost = 14f,
                spread = "18.0° 5-Pellet Cone",
                description = "Heavy particle dispersal cannon firing dense multi-pellet clusters for catastrophic close-range impact."
            },
            new WeaponDefinitionInfo {
                id = "missile",
                name = "Homing Missiles",
                classType = "GUIDED ORDNANCE // SMART TRACKING",
                damage = 60f,
                fireRate = 1.4f,
                energyCost = 16f,
                spread = "Predictive Target Tracking",
                description = "Self-propelled smart warheads with autonomous thrusters that seek and destroy nearest hostiles."
            },
            new WeaponDefinitionInfo {
                id = "beam",
                name = "Continuous Beam",
                classType = "PARTICLE STREAM // INFINITE PIERCE",
                damage = 80f, // DPS
                fireRate = 60.0f,
                energyCost = 20f, // /s
                spread = "Instant Collimated Ray",
                description = "Focused coherent particle beam slicing continuously through multiple targets with instantaneous impact."
            },
            new WeaponDefinitionInfo {
                id = "gatling",
                name = "Rotary Gatling Gun",
                classType = "KINETIC BALLISTIC // SPOOL-UP ACCELERATION",
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
            inspectedWeaponId = StorageService.GetEquippedWeapon(currentShipId);
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
                inspectedWeaponId = StorageService.GetEquippedWeapon(currentShipId);
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
            inspectedWeaponId = StorageService.GetEquippedWeapon(currentShipId);
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

            // Dark background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(rootContainer.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.GetComponent<Image>();
            bgImg.color = new Color(0.02f, 0.04f, 0.07f, 1.0f);

            // Title Header
            CreateText(rootContainer.transform, "Title", "ARMORY & LOADOUT // WEAPON & SKILL CUSTOMIZATION", 30, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.94f), new Vector2(0.5f, 0.94f), new Vector2(-450, -22), new Vector2(450, 22), defaultFont);

            // Gems Counter
            GameObject gemsObj = CreateText(rootContainer.transform, "GemsText", "Meta-Gems: 0", 18, FontStyle.Bold, Color.cyan, TextAnchor.MiddleRight, new Vector2(1f, 0.94f), new Vector2(1f, 0.94f), new Vector2(-280, -18), new Vector2(-30, 18), defaultFont);
            gemsText = gemsObj.GetComponent<TMP_Text>();

            // Ship Subtitle Banner
            GameObject bannerObj = CreateText(rootContainer.transform, "ShipHeaderBanner", "CONFIGURING LOADOUT FOR: DELTA SCOUT", 18, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(-350, -18), new Vector2(350, 18), defaultFont);
            shipHeaderBanner = bannerObj.GetComponent<TMP_Text>();

            // Change Ship Button (Top Right of Banner)
            GameObject chgShipBtn = CreateButton(rootContainer.transform, "ChangeShipBtn", "CHANGE SHIP [H]", new Vector2(0.5f, 0.87f), new Vector2(0.5f, 0.87f), new Vector2(340, 0), new Vector2(160, 36), new Color(0.18f, 0.28f, 0.40f), defaultFont, 13);
            chgShipBtn.GetComponent<Button>().onClick.AddListener(GoToHangar);

            // SECTION 1: PRIMARY WEAPONS (Left Panel)
            GameObject wpnPanel = new GameObject("WeaponPanel", typeof(RectTransform), typeof(Image));
            wpnPanel.transform.SetParent(rootContainer.transform, false);
            RectTransform wpRt = wpnPanel.GetComponent<RectTransform>();
            wpRt.anchorMin = new Vector2(0.04f, 0.17f);
            wpRt.anchorMax = new Vector2(0.52f, 0.82f);
            wpRt.offsetMin = Vector2.zero;
            wpRt.offsetMax = Vector2.zero;
            var wpImg = wpnPanel.GetComponent<Image>();
            wpImg.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);

            CreateText(wpnPanel.transform, "WpnHeader", "1. PRIMARY WEAPONS ARSENAL (EQUIP PER SHIP)", 16, FontStyle.Bold, new Color(1.0f, 0.65f, 0.15f), TextAnchor.MiddleLeft, new Vector2(0.05f, 0.93f), new Vector2(0.05f, 0.93f), new Vector2(0, -15), new Vector2(400, 15), defaultFont);

            GameObject wpnBtnContainerObj = new GameObject("WeaponButtons", typeof(RectTransform));
            wpnBtnContainerObj.transform.SetParent(wpnPanel.transform, false);
            RectTransform wbcRt = wpnBtnContainerObj.GetComponent<RectTransform>();
            wbcRt.anchorMin = new Vector2(0.05f, 0.52f);
            wbcRt.anchorMax = new Vector2(0.95f, 0.85f);
            wbcRt.offsetMin = Vector2.zero;
            wbcRt.offsetMax = Vector2.zero;
            weaponButtonContainer = wpnBtnContainerObj.transform;

            // Weapon Detail Card
            GameObject wpnDetailObj = new GameObject("WeaponDetailCard", typeof(RectTransform), typeof(Image));
            wpnDetailObj.transform.SetParent(wpnPanel.transform, false);
            RectTransform wdRt = wpnDetailObj.GetComponent<RectTransform>();
            wdRt.anchorMin = new Vector2(0.05f, 0.06f);
            wdRt.anchorMax = new Vector2(0.95f, 0.48f);
            wdRt.offsetMin = Vector2.zero;
            wdRt.offsetMax = Vector2.zero;
            var wdImg = wpnDetailObj.GetComponent<Image>();
            wdImg.color = new Color(0.02f, 0.04f, 0.08f, 0.8f);

            GameObject wdtObj = CreateText(wpnDetailObj.transform, "WpnTitle", "PULSE LASER", 18, FontStyle.Bold, Color.yellow, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.84f), new Vector2(0.05f, 0.84f), new Vector2(0, -15), new Vector2(400, 15), defaultFont);
            weaponDetailTitle = wdtObj.GetComponent<TMP_Text>();

            GameObject wdsObj = CreateText(wpnDetailObj.transform, "WpnStats", "STATS", 14, FontStyle.Normal, Color.white, TextAnchor.UpperLeft, new Vector2(0.05f, 0.52f), new Vector2(0.05f, 0.52f), new Vector2(0, -35), new Vector2(400, 35), defaultFont);
            weaponDetailStats = wdsObj.GetComponent<TMP_Text>();

            GameObject wddObj = CreateText(wpnDetailObj.transform, "WpnDesc", "DESCRIPTION", 13, FontStyle.Italic, new Color(0.85f, 0.9f, 1f), TextAnchor.UpperLeft, new Vector2(0.05f, 0.18f), new Vector2(0.05f, 0.18f), new Vector2(0, -25), new Vector2(400, 25), defaultFont);
            weaponDetailDesc = wddObj.GetComponent<TMP_Text>();

            // SECTION 2: COMBAT SKILLS (Right Panel)
            GameObject sklPanel = new GameObject("SkillPanel", typeof(RectTransform), typeof(Image));
            sklPanel.transform.SetParent(rootContainer.transform, false);
            RectTransform skpRt = sklPanel.GetComponent<RectTransform>();
            skpRt.anchorMin = new Vector2(0.55f, 0.17f);
            skpRt.anchorMax = new Vector2(0.96f, 0.82f);
            skpRt.offsetMin = Vector2.zero;
            skpRt.offsetMax = Vector2.zero;
            var skpImg = sklPanel.GetComponent<Image>();
            skpImg.color = new Color(0.04f, 0.07f, 0.12f, 0.92f);

            CreateText(sklPanel.transform, "SklHeader", "2. TACTICAL COMBAT SKILLS (SPACE / RIGHT CLICK)", 16, FontStyle.Bold, Color.cyan, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.93f), new Vector2(0.05f, 0.93f), new Vector2(0, -15), new Vector2(400, 15), defaultFont);

            GameObject sklBtnContainerObj = new GameObject("SkillButtons", typeof(RectTransform));
            sklBtnContainerObj.transform.SetParent(sklPanel.transform, false);
            RectTransform sbcRt = sklBtnContainerObj.GetComponent<RectTransform>();
            sbcRt.anchorMin = new Vector2(0.05f, 0.52f);
            sbcRt.anchorMax = new Vector2(0.95f, 0.85f);
            sbcRt.offsetMin = Vector2.zero;
            sbcRt.offsetMax = Vector2.zero;
            skillButtonContainer = sklBtnContainerObj.transform;

            // Skill Detail Card
            GameObject sklDetailObj = new GameObject("SkillDetailCard", typeof(RectTransform), typeof(Image));
            sklDetailObj.transform.SetParent(sklPanel.transform, false);
            RectTransform sdRt = sklDetailObj.GetComponent<RectTransform>();
            sdRt.anchorMin = new Vector2(0.05f, 0.06f);
            sdRt.anchorMax = new Vector2(0.95f, 0.48f);
            sdRt.offsetMin = Vector2.zero;
            sdRt.offsetMax = Vector2.zero;
            var sdImg = sklDetailObj.GetComponent<Image>();
            sdImg.color = new Color(0.02f, 0.04f, 0.08f, 0.8f);

            GameObject sktObj = CreateText(sklDetailObj.transform, "SklTitle", "KINETIC SHOCKWAVE", 18, FontStyle.Bold, Color.cyan, TextAnchor.MiddleLeft, new Vector2(0.05f, 0.84f), new Vector2(0.05f, 0.84f), new Vector2(0, -15), new Vector2(400, 15), defaultFont);
            skillDetailTitle = sktObj.GetComponent<TMP_Text>();

            GameObject skdObj = CreateText(sklDetailObj.transform, "SklDesc", "DESCRIPTION", 13, FontStyle.Normal, new Color(0.85f, 0.9f, 1f), TextAnchor.UpperLeft, new Vector2(0.05f, 0.45f), new Vector2(0.05f, 0.45f), new Vector2(0, -45), new Vector2(400, 45), defaultFont);
            skillDetailDesc = skdObj.GetComponent<TMP_Text>();

            // FLOW BUTTONS (Bottom Row)
            GameObject backBtnObj = CreateButton(rootContainer.transform, "BackBtn", "<- MAIN MENU [ESC]", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(-340, 0), new Vector2(220, 44), new Color(0.25f, 0.25f, 0.32f), defaultFont, 15);
            backBtnObj.GetComponent<Button>().onClick.AddListener(GoToMainMenu);

            GameObject hangarBtnObj = CreateButton(rootContainer.transform, "HangarBtn", "<- HANGAR SHIPS [H]", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(0, 0), new Vector2(240, 46), new Color(0.20f, 0.50f, 0.70f), defaultFont, 15);
            hangarBtnObj.GetComponent<Button>().onClick.AddListener(GoToHangar);

            GameObject playBtnObj = CreateButton(rootContainer.transform, "PlayBtn", "LAUNCH SURVIVAL (PvE) ->", new Vector2(0.5f, 0.07f), new Vector2(0.5f, 0.07f), new Vector2(340, 0), new Vector2(220, 44), new Color(0.15f, 0.65f, 0.30f), defaultFont, 15);
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
                EquipWeapon(WeaponCatalog[index].id);
            }
        }

        public void EquipWeapon(string weaponId)
        {
            EnsureInitialized();
            inspectedWeaponId = weaponId;
            StorageService.SaveEquippedWeapon(currentShipId, weaponId);
            StorageService.SaveSelectedWeapon(weaponId);

            var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
            if (weaponCtrl != null)
            {
                weaponCtrl.EquipFromStorage(currentShipId);
            }

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            RefreshUI();
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
                gemsText.text = $"Meta-Gems: {StorageService.Profile.totalMetaGems}";
            }

            if (shipHeaderBanner != null)
            {
                shipHeaderBanner.text = $"CONFIGURING LOADOUT FOR: {currentShipId.ToUpper()}";
            }

            string equippedWpn = StorageService.GetEquippedWeapon(currentShipId);
            string equippedSkl = StorageService.GetSelectedSkill();

            // Populate Weapon Buttons
            if (weaponButtonContainer != null)
            {
                for (int i = weaponButtonContainer.childCount - 1; i >= 0; i--)
                {
                    var child = weaponButtonContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                float btnSpacing = 84f;
                float startX = -((WeaponCatalog.Length - 1) * btnSpacing) / 2f;

                for (int i = 0; i < WeaponCatalog.Length; i++)
                {
                    var w = WeaponCatalog[i];
                    bool isEquipped = string.Equals(w.id, equippedWpn, System.StringComparison.OrdinalIgnoreCase);
                    bool isInspected = string.Equals(w.id, inspectedWeaponId, System.StringComparison.OrdinalIgnoreCase);

                    string label = isEquipped ? $"[{w.name.ToUpper()}]\n★ EQUIPPED" : $"{w.name.ToUpper()}";
                    Color col;
                    if (isEquipped) col = new Color(1.0f, 0.55f, 0.12f);
                    else if (isInspected) col = new Color(0.70f, 0.45f, 0.20f);
                    else col = new Color(0.20f, 0.22f, 0.32f);

                    GameObject btnObj = CreateButton(weaponButtonContainer, $"WpnBtn_{w.id}", label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(80, 48), col, defaultFont, 11);
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => EquipWeapon(w.id));
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

                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                float btnSpacing = 135f;
                float startX = -((SkillCatalog.Length - 1) * btnSpacing) / 2f;

                for (int i = 0; i < SkillCatalog.Length; i++)
                {
                    var s = SkillCatalog[i];
                    bool isEquipped = string.Equals(s.id, equippedSkl, System.StringComparison.OrdinalIgnoreCase);
                    bool isInspected = string.Equals(s.id, inspectedSkillId, System.StringComparison.OrdinalIgnoreCase);

                    string label = isEquipped ? $"[{s.name.ToUpper()}]\n★ EQUIPPED" : $"{s.name.ToUpper()}";
                    Color col;
                    if (isEquipped) col = new Color(0.12f, 0.75f, 0.90f);
                    else if (isInspected) col = new Color(0.20f, 0.45f, 0.60f);
                    else col = new Color(0.20f, 0.22f, 0.32f);

                    GameObject btnObj = CreateButton(skillButtonContainer, $"SklBtn_{s.id}", label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * btnSpacing), 0), new Vector2(130, 48), col, defaultFont, 12);
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => EquipSkill(s.id));
                }
            }

            // Update Weapon Inspector Card
            var wDef = GetWeaponDef(inspectedWeaponId);
            if (weaponDetailTitle != null) weaponDetailTitle.text = $"{wDef.name.ToUpper()} ({wDef.classType})";
            if (weaponDetailStats != null)
            {
                weaponDetailStats.text = $"• Base Damage:   {wDef.damage}\n" +
                                         $"• Cyclic Rate:   {wDef.fireRate:F1} shots/s\n" +
                                         $"• Energy Cost:   {wDef.energyCost} per activation\n" +
                                         $"• Spread Offset: {wDef.spread}";
            }
            if (weaponDetailDesc != null) weaponDetailDesc.text = $"\"{wDef.description}\"";

            // Update Skill Inspector Card
            var sDef = GetSkillDef(inspectedSkillId);
            if (skillDetailTitle != null) skillDetailTitle.text = $"{sDef.name.ToUpper()} ({sDef.role})";
            if (skillDetailDesc != null)
            {
                skillDetailDesc.text = $"• Energy Drain: {sDef.energyCost} ENG\n" +
                                       $"• Cooldown:     {sDef.cooldown:F1} seconds\n\n" +
                                       $"\"{sDef.description}\"";
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
