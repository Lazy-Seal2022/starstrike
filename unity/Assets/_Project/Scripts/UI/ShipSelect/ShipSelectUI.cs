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
    public class ShipSelectUI : MonoBehaviour, IScreenController
    {
        public static ShipSelectUI Instance { get; private set; }

        public string ScreenId => "ShipSelect";

        private GameObject rootContainer;
        private TMP_Text gemsText;
        private Image shipPreviewImage;
        private TMP_Text shipTitleText;
        private TMP_Text shipRoleText;
        private TMP_Text shipStatsText;
        private TMP_Text shipDescText;
        private TMP_Text equippedWeaponText;
        private Button actionButton;
        private TMP_Text actionButtonText;
        private Transform shipStripContainer;

        private int currentIndex = 0;

        public int CurrentIndex => currentIndex;
        public string CurrentShipId => Catalog[currentIndex].id;

        public struct ShipInfo
        {
            public string id;
            public string name;
            public int tier;
            public string role;
            public int cost;
            public int hull;
            public int shield;
            public int shieldRegen;
            public int energy;
            public int energyRegen;
            public int speed;
            public float agility;
            public int weaponSlots;
            public string description;
        }

        private static readonly ShipInfo[] Catalog = new ShipInfo[]
        {
            new ShipInfo {
                id = "scout",
                name = "Delta Scout",
                tier = 1,
                role = "LIGHT RECON RECON // BALANCED MOBILITY",
                cost = 0,
                hull = 80,
                shield = 100,
                shieldRegen = 8,
                energy = 80,
                energyRegen = 18,
                speed = 260,
                agility = 4.5f,
                weaponSlots = 1,
                description = "Standard frontline reconnaissance craft. Agile maneuvering with reliable plasma systems."
            },
            new ShipInfo {
                id = "fighter",
                name = "Ares Fighter",
                tier = 2,
                role = "ASSAULT STRIKER // DUAL HARDPOINTS",
                cost = 500,
                hull = 120,
                shield = 150,
                shieldRegen = 12,
                energy = 120,
                energyRegen = 24,
                speed = 310,
                agility = 5.0f,
                weaponSlots = 2,
                description = "Aggressive dogfighter equipped with twin synchronized weapon mounts and enhanced afterburners."
            },
            new ShipInfo {
                id = "miner",
                name = "Goliath Miner",
                tier = 2,
                role = "HEAVY EXTRACTION // REINFORCED HULL",
                cost = 500,
                hull = 180,
                shield = 220,
                shieldRegen = 16,
                energy = 150,
                energyRegen = 20,
                speed = 210,
                agility = 3.5f,
                weaponSlots = 2,
                description = "Industrial demolition vessel featuring dense reinforced titanium plating and high shield resilience."
            },
            new ShipInfo {
                id = "interceptor",
                name = "Viper Interceptor",
                tier = 3,
                role = "HIGH-SPEED PURSUIT // MAX AGILITY",
                cost = 1500,
                hull = 100,
                shield = 180,
                shieldRegen = 15,
                energy = 140,
                energyRegen = 30,
                speed = 400,
                agility = 6.5f,
                weaponSlots = 2,
                description = "Apex interceptor designed for rapid flanking and evasive dogfighting at blistering combat velocities."
            },
            new ShipInfo {
                id = "juggernaut",
                name = "Titan Juggernaut",
                tier = 3,
                role = "HEAVY DREADNOUGHT // TRIPLE HARDPOINTS",
                cost = 2000,
                hull = 220,
                shield = 260,
                shieldRegen = 18,
                energy = 180,
                energyRegen = 22,
                speed = 190,
                agility = 3.0f,
                weaponSlots = 3,
                description = "Fortified flying battery boasting three forward hardpoints and an impenetrable spherical kinetic shield."
            },
            new ShipInfo {
                id = "phantom",
                name = "Phantom Ghost",
                tier = 3,
                role = "STEALTH INFILTRATOR // RAPID RECHARGE",
                cost = 2500,
                hull = 110,
                shield = 200,
                shieldRegen = 22,
                energy = 160,
                energyRegen = 35,
                speed = 350,
                agility = 5.8f,
                weaponSlots = 2,
                description = "Covert operations vessel engineered with rapid-cycling capacitor arrays and sensor-deflecting composites."
            },
            new ShipInfo {
                id = "vanguard",
                name = "Vanguard Flagship",
                tier = 3,
                role = "FLEET COMMANDER // ALL-ROUND SUPERIORITY",
                cost = 2500,
                hull = 200,
                shield = 240,
                shieldRegen = 20,
                energy = 170,
                energyRegen = 26,
                speed = 280,
                agility = 4.2f,
                weaponSlots = 3,
                description = "Versatile flagship vessel commanding unmatched battlefield presence and triple weapon batteries."
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

            // Find current equipped ship index
            string selectedShip = StorageService.GetLastShipEvolved();
            if (!string.IsNullOrEmpty(selectedShip))
            {
                for (int i = 0; i < Catalog.Length; i++)
                {
                    if (string.Equals(Catalog[i].id, selectedShip, System.StringComparison.OrdinalIgnoreCase))
                    {
                        currentIndex = i;
                        break;
                    }
                }
            }

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

            if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
            {
                NavigateShip(-1);
            }
            else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            {
                NavigateShip(1);
            }
            else if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                GoToEquipment();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                GoToMainMenu();
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PerformShipAction();
            }
        }

        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            rootContainer = new GameObject("ShipSelectContainer", typeof(RectTransform));
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

            // Dark space background
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
            CreateText(rootContainer.transform, "Title", "STARFLEET HANGAR // SHIP SELECTION", 34, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.93f), new Vector2(0.5f, 0.93f), new Vector2(-400, -25), new Vector2(400, 25), defaultFont);

            // Gems Counter (Top Right)
            GameObject gemsObj = CreateText(rootContainer.transform, "GemsText", "Meta-Gems: 0", 20, FontStyle.Bold, Color.cyan, TextAnchor.MiddleRight, new Vector2(1f, 0.93f), new Vector2(1f, 0.93f), new Vector2(-280, -20), new Vector2(-30, 20), defaultFont);
            gemsText = gemsObj.GetComponent<TMP_Text>();

            // Ship Quick Selection Strip
            GameObject stripObj = new GameObject("ShipStrip", typeof(RectTransform));
            stripObj.transform.SetParent(rootContainer.transform, false);
            RectTransform sRt = stripObj.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.85f);
            sRt.anchorMax = new Vector2(0.5f, 0.85f);
            sRt.anchoredPosition = Vector2.zero;
            sRt.sizeDelta = new Vector2(900, 50);
            shipStripContainer = stripObj.transform;

            // Center Showcase Panel
            GameObject showcaseObj = new GameObject("ShowcasePanel", typeof(RectTransform), typeof(Image));
            showcaseObj.transform.SetParent(rootContainer.transform, false);
            RectTransform scRt = showcaseObj.GetComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 0.50f);
            scRt.anchorMax = new Vector2(0.5f, 0.50f);
            scRt.anchoredPosition = Vector2.zero;
            scRt.sizeDelta = new Vector2(960, 430);
            var scImg = showcaseObj.GetComponent<Image>();
            scImg.color = new Color(0.05f, 0.08f, 0.14f, 0.92f);

            // Ship Visual Box (Left)
            GameObject previewBox = new GameObject("PreviewBox", typeof(RectTransform), typeof(Image));
            previewBox.transform.SetParent(showcaseObj.transform, false);
            RectTransform pbRt = previewBox.GetComponent<RectTransform>();
            pbRt.anchorMin = new Vector2(0.04f, 0.12f);
            pbRt.anchorMax = new Vector2(0.40f, 0.88f);
            pbRt.offsetMin = Vector2.zero;
            pbRt.offsetMax = Vector2.zero;
            var pbImg = previewBox.GetComponent<Image>();
            pbImg.color = new Color(0.02f, 0.04f, 0.08f, 0.8f);

            GameObject shipImgObj = new GameObject("ShipSprite", typeof(RectTransform), typeof(Image));
            shipImgObj.transform.SetParent(previewBox.transform, false);
            RectTransform siRt = shipImgObj.GetComponent<RectTransform>();
            siRt.anchorMin = new Vector2(0.1f, 0.1f);
            siRt.anchorMax = new Vector2(0.9f, 0.9f);
            siRt.offsetMin = Vector2.zero;
            siRt.offsetMax = Vector2.zero;
            shipPreviewImage = shipImgObj.GetComponent<Image>();
            shipPreviewImage.preserveAspect = true;

            // Ship Info Details (Right)
            GameObject titleObj = CreateText(showcaseObj.transform, "ShipTitle", "VESSEL NAME", 28, FontStyle.Bold, Color.yellow, TextAnchor.MiddleLeft, new Vector2(0.44f, 0.88f), new Vector2(0.44f, 0.88f), new Vector2(0, -25), new Vector2(500, 25), defaultFont);
            shipTitleText = titleObj.GetComponent<TMP_Text>();

            GameObject roleObj = CreateText(showcaseObj.transform, "ShipRole", "CLASS ROLE", 16, FontStyle.Bold, Color.cyan, TextAnchor.MiddleLeft, new Vector2(0.44f, 0.79f), new Vector2(0.44f, 0.79f), new Vector2(0, -15), new Vector2(500, 15), defaultFont);
            shipRoleText = roleObj.GetComponent<TMP_Text>();

            GameObject statsObj = CreateText(showcaseObj.transform, "ShipStats", "STATS", 16, FontStyle.Normal, Color.white, TextAnchor.UpperLeft, new Vector2(0.44f, 0.52f), new Vector2(0.44f, 0.52f), new Vector2(0, -85), new Vector2(500, 85), defaultFont);
            shipStatsText = statsObj.GetComponent<TMP_Text>();

            GameObject descObj = CreateText(showcaseObj.transform, "ShipDesc", "DESCRIPTION", 14, FontStyle.Italic, new Color(0.8f, 0.85f, 0.95f), TextAnchor.UpperLeft, new Vector2(0.44f, 0.22f), new Vector2(0.44f, 0.22f), new Vector2(0, -35), new Vector2(500, 35), defaultFont);
            shipDescText = descObj.GetComponent<TMP_Text>();

            GameObject eqObj = CreateText(showcaseObj.transform, "EquippedWeapon", "Equipped Weapon: Pulse Laser", 15, FontStyle.Bold, new Color(1f, 0.65f, 0.15f), TextAnchor.MiddleLeft, new Vector2(0.44f, 0.08f), new Vector2(0.44f, 0.08f), new Vector2(0, -15), new Vector2(500, 15), defaultFont);
            equippedWeaponText = eqObj.GetComponent<TMP_Text>();

            // Navigation Buttons (Prev / Next)
            GameObject prevBtnObj = CreateButton(rootContainer.transform, "PrevBtn", "< PREVIOUS", new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), new Vector2(-380, 0), new Vector2(160, 48), new Color(0.2f, 0.25f, 0.35f), defaultFont, 16);
            prevBtnObj.GetComponent<Button>().onClick.AddListener(() => NavigateShip(-1));

            GameObject nextBtnObj = CreateButton(rootContainer.transform, "NextBtn", "NEXT >", new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), new Vector2(380, 0), new Vector2(160, 48), new Color(0.2f, 0.25f, 0.35f), defaultFont, 16);
            nextBtnObj.GetComponent<Button>().onClick.AddListener(() => NavigateShip(1));

            // Select / Unlock Action Button
            GameObject actionBtnObj = CreateButton(rootContainer.transform, "ActionBtn", "SELECT SHIP", new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.20f), new Vector2(0, 0), new Vector2(260, 52), new Color(0.15f, 0.70f, 0.35f), defaultFont, 18);
            actionButton = actionBtnObj.GetComponent<Button>();
            actionButtonText = actionBtnObj.transform.Find("Text")?.GetComponent<TMP_Text>();
            actionButton.onClick.AddListener(PerformShipAction);

            // Flow Navigation (Bottom Row)
            GameObject backBtnObj = CreateButton(rootContainer.transform, "BackBtn", "<- MAIN MENU [ESC]", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(-340, 0), new Vector2(220, 44), new Color(0.25f, 0.25f, 0.32f), defaultFont, 15);
            backBtnObj.GetComponent<Button>().onClick.AddListener(GoToMainMenu);

            GameObject armoryBtnObj = CreateButton(rootContainer.transform, "ArmoryBtn", "ARMORY / LOADOUT [E] ->", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(0, 0), new Vector2(240, 46), new Color(0.95f, 0.55f, 0.12f), defaultFont, 15);
            armoryBtnObj.GetComponent<Button>().onClick.AddListener(GoToEquipment);

            GameObject playBtnObj = CreateButton(rootContainer.transform, "PlayBtn", "LAUNCH SURVIVAL (PvE) ->", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), new Vector2(340, 0), new Vector2(220, 44), new Color(0.15f, 0.65f, 0.30f), defaultFont, 15);
            playBtnObj.GetComponent<Button>().onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadGameplay(GameMode.PvE);
                }
            });
        }

        public void NavigateShip(int direction)
        {
            EnsureInitialized();
            currentIndex = (currentIndex + direction + Catalog.Length) % Catalog.Length;
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("menu_navigate", 0.4f);
            RefreshUI();
        }

        public void SelectShipById(string shipId)
        {
            EnsureInitialized();
            for (int i = 0; i < Catalog.Length; i++)
            {
                if (string.Equals(Catalog[i].id, shipId, System.StringComparison.OrdinalIgnoreCase))
                {
                    currentIndex = i;
                    StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("menu_navigate", 0.4f);
                    RefreshUI();
                    break;
                }
            }
        }

        private void PerformShipAction()
        {
            ShipInfo info = Catalog[currentIndex];
            bool isUnlocked = StorageService.Profile.unlockedShips.Contains(info.id);
            string activeShip = StorageService.GetLastShipEvolved();

            if (isUnlocked)
            {
                StorageService.SaveLastShipEvolved(info.id);
                var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
                if (weaponCtrl != null)
                {
                    weaponCtrl.EquipFromStorage(info.id);
                }
                StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
                RefreshUI();
            }
            else if (StorageService.Profile.totalMetaGems >= info.cost)
            {
                StorageService.Profile.totalMetaGems -= info.cost;
                StorageService.UnlockShip(info.id);
                StorageService.SaveLastShipEvolved(info.id);
                var weaponCtrl = FindAnyObjectByType<ShipWeaponController>();
                if (weaponCtrl != null)
                {
                    weaponCtrl.EquipFromStorage(info.id);
                }
                StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("level_up");
                RefreshUI();
            }
        }

        private void GoToEquipment()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadEquipmentSelect();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(GameManager.SceneEquipmentSelect);
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

        public void RefreshUI()
        {
            if (Catalog.Length == 0) return;
            if (currentIndex < 0 || currentIndex >= Catalog.Length) currentIndex = 0;

            ShipInfo info = Catalog[currentIndex];
            bool isUnlocked = StorageService.Profile.unlockedShips.Contains(info.id);
            string activeShip = StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(activeShip)) activeShip = "scout";
            bool isSelected = string.Equals(info.id, activeShip, System.StringComparison.OrdinalIgnoreCase);

            if (gemsText != null)
            {
                gemsText.text = $"Meta-Gems: {StorageService.Profile.totalMetaGems}";
            }

            if (shipTitleText != null)
            {
                shipTitleText.text = $"{info.name.ToUpper()} [TIER {info.tier}]";
            }

            if (shipRoleText != null)
            {
                shipRoleText.text = info.role;
            }

            if (shipPreviewImage != null)
            {
                shipPreviewImage.sprite = ProceduralSpriteHelper.GetShipSprite(info.id);
            }

            if (shipStatsText != null)
            {
                shipStatsText.text = $"• Hull Capacity:   {info.hull} HP\n" +
                                     $"• Shield Barrier:  {info.shield} SHD (+{info.shieldRegen}/s)\n" +
                                     $"• Energy Reactor:  {info.energy} ENG (+{info.energyRegen}/s)\n" +
                                     $"• Speed & Agility: {info.speed} vel / {info.agility:F1} turn\n" +
                                     $"• Weapon Sockets:  {info.weaponSlots} Hardpoints";
            }

            if (shipDescText != null)
            {
                shipDescText.text = $"\"{info.description}\"";
            }

            if (equippedWeaponText != null)
            {
                string weaponId = StorageService.GetEquippedWeapon(info.id);
                string weaponName = GetWeaponName(weaponId);
                equippedWeaponText.text = $"Equipped Weapon Loadout:  {weaponName.ToUpper()}";
            }

            // Action button state
            if (actionButton != null && actionButtonText != null)
            {
                if (isSelected)
                {
                    actionButtonText.text = "[ ACTIVE SHIP ]";
                    actionButton.image.color = new Color(0.12f, 0.72f, 0.35f);
                    actionButton.interactable = true;
                }
                else if (isUnlocked)
                {
                    actionButtonText.text = "DEPLOY SHIP";
                    actionButton.image.color = new Color(0.10f, 0.55f, 0.75f);
                    actionButton.interactable = true;
                }
                else
                {
                    bool canAfford = StorageService.Profile.totalMetaGems >= info.cost;
                    actionButtonText.text = $"UNLOCK SHIP ({info.cost} G)";
                    actionButton.image.color = canAfford ? new Color(0.85f, 0.40f, 0.15f) : new Color(0.55f, 0.20f, 0.20f);
                    actionButton.interactable = canAfford;
                }
            }

            // Quick select strip
            if (shipStripContainer != null)
            {
                for (int i = shipStripContainer.childCount - 1; i >= 0; i--)
                {
                    var child = shipStripContainer.GetChild(i);
                    if (Application.isPlaying) Destroy(child.gameObject);
                    else DestroyImmediate(child.gameObject);
                }

                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                float spacing = 125f;
                float startX = -((Catalog.Length - 1) * spacing) / 2f;

                for (int i = 0; i < Catalog.Length; i++)
                {
                    int idx = i;
                    ShipInfo s = Catalog[i];
                    bool sUnlocked = StorageService.Profile.unlockedShips.Contains(s.id);
                    bool sSelected = string.Equals(s.id, activeShip, System.StringComparison.OrdinalIgnoreCase);
                    bool isViewed = (i == currentIndex);

                    string btnLabel = sUnlocked ? (sSelected ? $"[ {s.id.ToUpper()} ]" : s.id.ToUpper()) : $"{s.id.ToUpper()}\n{s.cost}G";
                    Color btnColor;
                    if (isViewed)
                        btnColor = new Color(0.95f, 0.75f, 0.15f);
                    else if (sSelected)
                        btnColor = new Color(0.15f, 0.70f, 0.35f);
                    else if (sUnlocked)
                        btnColor = new Color(0.25f, 0.30f, 0.40f);
                    else
                        btnColor = new Color(0.40f, 0.18f, 0.20f);

                    GameObject btnObj = CreateButton(shipStripContainer, $"StripBtn_{s.id}", btnLabel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + (i * spacing), 0), new Vector2(115, 42), btnColor, defaultFont, 12);
                    Button btn = btnObj.GetComponent<Button>();
                    btn.onClick.AddListener(() => {
                        currentIndex = idx;
                        StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("menu_navigate", 0.4f);
                        RefreshUI();
                    });
                }
            }
        }

        private string GetWeaponName(string weaponId)
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
