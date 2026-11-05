using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class HUDUpgradeDrawer : MonoBehaviour
    {
        [Header("Configuration")]
        public UIThemeConfig themeConfig;

        [Header("Hierarchy References")]
        public GameObject drawerPanel;
        public Button toggleButton;
        public Text toggleButtonText;
        public Transform itemsContainer;

        public bool IsOpen { get; private set; } = false;

        private PlayerProgression player;
        private Vector2 closedPosition;
        private Vector2 openPosition;
        private RectTransform drawerRt;

        private readonly struct UpgradeItem
        {
            public readonly string Key;
            public readonly string Label;
            public UpgradeItem(string key, string label) { Key = key; Label = label; }
        }

        private static readonly UpgradeItem[] Upgrades = new[]
        {
            new UpgradeItem("shieldCap", "1. Shield Cap"),
            new UpgradeItem("shieldRegen", "2. Shield Regen"),
            new UpgradeItem("energyCap", "3. Energy Cap"),
            new UpgradeItem("energyRegen", "4. Energy Regen"),
            new UpgradeItem("damage", "5. Pulse Damage"),
            new UpgradeItem("pulseSpeed", "6. Pulse Speed"),
            new UpgradeItem("speed", "7. Engine Speed"),
            new UpgradeItem("agility", "8. Agility")
        };

        private List<Text> itemCostTexts = new List<Text>();
        private List<Button> itemButtons = new List<Button>();

        private void Awake()
        {
            if (themeConfig == null) themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            if (themeConfig == null) themeConfig = ScriptableObject.CreateInstance<UIThemeConfig>();

            if (drawerPanel != null)
            {
                drawerRt = drawerPanel.GetComponent<RectTransform>();
            }

            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleDrawer);
            }
        }

        private void Start()
        {
            player = PlayerProgression.Instance;
            RefreshUpgradeButtons();
        }

        private void Update()
        {
            if (drawerRt == null || themeConfig == null) return;

            Vector2 targetPos = IsOpen ? openPosition : closedPosition;
            drawerRt.anchoredPosition = Vector2.Lerp(drawerRt.anchoredPosition, targetPos, Time.deltaTime * themeConfig.drawerSlideSpeed);
        }

        public void SetDrawerPositions(Vector2 closed, Vector2 open)
        {
            closedPosition = closed;
            openPosition = open;
        }

        public void ToggleDrawer()
        {
            IsOpen = !IsOpen;
            if (toggleButtonText != null)
            {
                toggleButtonText.text = IsOpen ? "✖ CLOSE" : "▲ UPGRADES";
            }
            if (IsOpen)
            {
                RefreshUpgradeButtons();
            }
        }

        public void RefreshUpgradeButtons()
        {
            if (player == null) player = PlayerProgression.Instance;
            if (player == null) return;

            for (int i = 0; i < Upgrades.Length && i < itemButtons.Count; i++)
            {
                string key = Upgrades[i].Key;
                int currentLvl = player.upgradeState.GetLevel(key);
                int cost = player.GetUpgradeCost(key);
                bool canAfford = player.gems >= cost && currentLvl < player.GetMaxLevel();

                if (itemButtons[i] != null)
                {
                    itemButtons[i].interactable = canAfford;
                }

                if (i < itemCostTexts.Count && itemCostTexts[i] != null)
                {
                    if (currentLvl >= player.GetMaxLevel())
                    {
                        itemCostTexts[i].text = "MAX";
                        itemCostTexts[i].color = themeConfig != null ? themeConfig.textMuted : Color.gray;
                    }
                    else
                    {
                        itemCostTexts[i].text = $"Lvl {currentLvl} ({cost} ◈)";
                        itemCostTexts[i].color = canAfford ? (themeConfig != null ? themeConfig.textHighlight : Color.cyan) : (themeConfig != null ? themeConfig.dangerColor : Color.red);
                    }
                }
            }
        }

        public void RegisterItemWidgets(List<Button> buttons, List<Text> costTexts)
        {
            itemButtons = buttons;
            itemCostTexts = costTexts;
        }
    }
}
