using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class UIThemeAndSubControllerTests
    {
        private List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in createdObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            createdObjects.Clear();
        }

        private GameObject Track(GameObject go)
        {
            createdObjects.Add(go);
            return go;
        }

        [Test]
        public void UIThemeConfig_LoadsFromResources_HasSensibleColorsAndSpeeds()
        {
            var theme = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            if (theme == null) theme = ScriptableObject.CreateInstance<UIThemeConfig>();

            Assert.AreNotEqual(Color.clear, theme.shieldColor);
            Assert.AreNotEqual(Color.clear, theme.energyColor);
            Assert.AreNotEqual(Color.clear, theme.boostColor);
            Assert.AreNotEqual(Color.clear, theme.shieldGhostColor);
            Assert.Greater(theme.barFillSmoothSpeed, 0f);
            Assert.Greater(theme.ghostBarDelaySeconds, 0f);
            Assert.Greater(theme.ghostBarDrainSpeed, 0f);
        }

        [Test]
        public void HUDPlayerStatus_ShieldDamage_UpdatesRatioAndText()
        {
            var go = Track(new GameObject("TestPlayerStatus"));
            var status = go.AddComponent<HUDPlayerStatus>();

            var shieldGo = new GameObject("ShieldFill", typeof(RectTransform), typeof(Image));
            shieldGo.transform.SetParent(go.transform);
            status.shieldFill = shieldGo.GetComponent<Image>();

            var ghostGo = new GameObject("GhostFill", typeof(RectTransform), typeof(Image));
            ghostGo.transform.SetParent(go.transform);
            status.shieldGhostFill = ghostGo.GetComponent<Image>();

            var textGo = new GameObject("ShieldText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform);
            status.shieldText = textGo.GetComponent<Text>();

            status.HandleShieldChanged(60f, 100f);

            Assert.AreEqual("60 / 100", status.shieldText.text);
        }

        [Test]
        public void HUDPlayerStatus_EnergyChange_UpdatesTextAndTarget()
        {
            var go = Track(new GameObject("TestEnergyStatus"));
            var status = go.AddComponent<HUDPlayerStatus>();

            var energyGo = new GameObject("EnergyFill", typeof(RectTransform), typeof(Image));
            energyGo.transform.SetParent(go.transform);
            status.energyFill = energyGo.GetComponent<Image>();

            var textGo = new GameObject("EnergyText", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform);
            status.energyText = textGo.GetComponent<Text>();

            status.HandleEnergyChanged(40f, 80f);

            Assert.AreEqual("40 / 80", status.energyText.text);
        }

        [Test]
        public void HUDEvolutionBanner_RebuildEvolutionButtons_SpawnsExpectedButtons()
        {
            var bannerGo = Track(new GameObject("TestEvoBanner"));
            var banner = bannerGo.AddComponent<HUDEvolutionBanner>();
            banner.bannerRoot = bannerGo;

            var playerGo = Track(new GameObject("TestPlayer"));
            var player = playerGo.AddComponent<PlayerProgression>();

            var baseShip = ScriptableObject.CreateInstance<ShipDefinition>();
            baseShip.id = "base_scout";
            baseShip.shipName = "Test Scout";
            baseShip.tier = 1;

            var branchA = ScriptableObject.CreateInstance<ShipDefinition>();
            branchA.id = "fighter";
            branchA.shipName = "Ares Fighter";
            branchA.tier = 2;

            var branchB = ScriptableObject.CreateInstance<ShipDefinition>();
            branchB.id = "miner";
            branchB.shipName = "Goliath Miner";
            branchB.tier = 2;

            baseShip.evolvesTo = new[] { branchA, branchB };
            player.currentShip = baseShip;

            banner.RebuildEvolutionButtons(player);

            Assert.IsTrue(banner.bannerRoot.activeSelf, "Evolution banner should become visible when options exist");
            Assert.AreEqual(2, bannerGo.transform.childCount, "Banner should contain 2 child evolution buttons");
        }

        [Test]
        public void HUDUpgradeDrawer_ToggleDrawer_TogglesStateCorrectly()
        {
            var go = Track(new GameObject("TestUpgradeDrawer"));
            var drawer = go.AddComponent<HUDUpgradeDrawer>();

            Assert.IsFalse(drawer.IsOpen);

            drawer.ToggleDrawer();
            Assert.IsTrue(drawer.IsOpen);

            drawer.ToggleDrawer();
            Assert.IsFalse(drawer.IsOpen);
        }

        [Test]
        public void CanvasHUDPrefab_LoadsFromResources_HasAllSubControllersPreWired()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/UI/CanvasHUD");
            Assert.IsNotNull(prefab, "CanvasHUD.prefab must exist in Resources/Prefabs/UI/CanvasHUD");

            var hud = prefab.GetComponent<CanvasHUD>();
            Assert.IsNotNull(hud, "CanvasHUD component must be on prefab");
            Assert.IsNotNull(hud.playerStatus, "HUDPlayerStatus must be wired on CanvasHUD");
            Assert.IsNotNull(hud.evolutionBannerController, "HUDEvolutionBanner must be wired on CanvasHUD");
            Assert.IsNotNull(hud.upgradeDrawerController, "HUDUpgradeDrawer must be wired on CanvasHUD");
            Assert.IsNotNull(hud.floatingTextController, "HUDFloatingText must be wired on CanvasHUD");
        }
    }
}
