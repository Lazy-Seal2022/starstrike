using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class HUDEvolutionBanner : MonoBehaviour
    {
        [Header("Configuration")]
        public UIThemeConfig themeConfig;

        [Header("Hierarchy References")]
        public GameObject bannerRoot;
        public Transform buttonsContainer;
        public Text promptText;

        [Header("Prefabs / Templates")]
        public GameObject buttonTemplate;

        private List<GameObject> activeButtons = new List<GameObject>();
        private bool isVisible = false;

        private void Awake()
        {
            if (themeConfig == null) themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            if (themeConfig == null) themeConfig = ScriptableObject.CreateInstance<UIThemeConfig>();

            if (bannerRoot == null) bannerRoot = gameObject;
            if (promptText != null && themeConfig != null) promptText.color = themeConfig.textHighlight;

            SetBannerVisible(false);
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
        }

        private void HandlePlayerEvolved(string shipId)
        {
            // Close banner after evolving
            SetBannerVisible(false);
        }

        public void SetBannerVisible(bool visible)
        {
            isVisible = visible;
            if (bannerRoot != null)
            {
                bannerRoot.SetActive(visible);
            }
        }

        public void RebuildEvolutionButtons(PlayerProgression player)
        {
            if (player == null || player.currentShip == null || player.currentShip.evolvesTo == null)
            {
                SetBannerVisible(false);
                return;
            }

            var nextShips = player.currentShip.evolvesTo;
            if (nextShips.Length == 0)
            {
                SetBannerVisible(false);
                return;
            }

            // Clear old buttons
            for (int i = activeButtons.Count - 1; i >= 0; i--)
            {
                if (activeButtons[i] != null) DestroyImmediate(activeButtons[i]);
            }
            activeButtons.Clear();

            Transform parent = buttonsContainer != null ? buttonsContainer : (bannerRoot != null ? bannerRoot.transform : transform);

            foreach (var shipDef in nextShips)
            {
                if (shipDef == null) continue;

                GameObject btnObj = CreateEvolutionButton(shipDef, parent, player);
                if (btnObj != null)
                {
                    activeButtons.Add(btnObj);
                }
            }

            SetBannerVisible(activeButtons.Count > 0);
        }

        private GameObject CreateEvolutionButton(ShipDefinition shipDef, Transform parent, PlayerProgression player)
        {
            GameObject btnObj = new GameObject($"Evolve_{shipDef.id}");
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(170f, 48f);

            var img = btnObj.AddComponent<Image>();
            img.color = themeConfig != null ? themeConfig.buttonNormal : new Color(0.12f, 0.22f, 0.35f, 0.95f);

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            if (themeConfig != null)
            {
                colors.normalColor = themeConfig.buttonNormal;
                colors.highlightedColor = themeConfig.buttonHover;
                colors.pressedColor = themeConfig.buttonPressed;
                colors.disabledColor = themeConfig.buttonDisabled;
            }
            btn.colors = colors;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            var iconRt = iconObj.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(24f, 0f);
            iconRt.sizeDelta = new Vector2(32f, 32f);
            var iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = shipDef.sprite != null ? shipDef.sprite : ProceduralSpriteHelper.GetShipSprite(shipDef.id);

            // Label
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.offsetMin = new Vector2(46f, 0f);
            textRt.offsetMax = new Vector2(-6f, 0f);
            var label = textObj.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = $"{shipDef.shipName}\n<color=#66CCFF>TIER {shipDef.tier}</color>";
            label.fontSize = 12;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = themeConfig != null ? themeConfig.textPrimary : Color.white;
            label.supportRichText = true;

            btn.onClick.AddListener(() =>
            {
                player.EvolveTo(shipDef);
                SetBannerVisible(false);
            });

            return btnObj;
        }
    }
}
