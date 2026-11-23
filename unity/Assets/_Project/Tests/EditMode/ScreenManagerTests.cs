using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class ScreenManagerTests
    {
        private class MockScreenController : IScreenController
        {
            public string ScreenId { get; }
            public bool IsVisible { get; private set; }

            public MockScreenController(string id)
            {
                ScreenId = id;
                IsVisible = false;
            }

            public void Show() => IsVisible = true;
            public void Hide() => IsVisible = false;
        }

        private GameObject managerGo;
        private ScreenManager screenManager;

        [SetUp]
        public void SetUp()
        {
            managerGo = new GameObject("TestScreenManager");
            screenManager = managerGo.AddComponent<ScreenManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (managerGo != null)
            {
                Object.DestroyImmediate(managerGo);
            }
        }

        [Test]
        public void ScreenManager_RegistrationAndShow_SwitchesScreensCleanly()
        {
            var menuScreen = new MockScreenController("MainMenu");
            var hudScreen = new MockScreenController("HUD");

            screenManager.RegisterScreen(menuScreen);
            screenManager.RegisterScreen(hudScreen);

            // Show MainMenu
            bool shownMenu = screenManager.ShowScreen("MainMenu");
            Assert.IsTrue(shownMenu);
            Assert.IsTrue(menuScreen.IsVisible);
            Assert.IsFalse(hudScreen.IsVisible);
            Assert.AreEqual("MainMenu", screenManager.CurrentScreenId);

            // Switch to HUD (mutually exclusive)
            bool shownHUD = screenManager.ShowScreen("HUD");
            Assert.IsTrue(shownHUD);
            Assert.IsFalse(menuScreen.IsVisible);
            Assert.IsTrue(hudScreen.IsVisible);
            Assert.AreEqual("HUD", screenManager.CurrentScreenId);

            // Hide HUD
            screenManager.HideScreen("HUD");
            Assert.IsFalse(hudScreen.IsVisible);
            Assert.IsNull(screenManager.CurrentScreenId);
        }

        [Test]
        public void ScreenManager_ModalStack_PushesAndPopsLIFO()
        {
            var hudScreen = new MockScreenController("HUD");
            var settingsModal = new MockScreenController("Settings");
            var confirmModal = new MockScreenController("Confirm");

            screenManager.RegisterScreen(hudScreen);
            screenManager.RegisterScreen(settingsModal);
            screenManager.RegisterScreen(confirmModal);

            screenManager.ShowScreen("HUD");
            Assert.IsTrue(hudScreen.IsVisible);

            // Push Settings modal
            bool pushedSettings = screenManager.PushModal("Settings");
            Assert.IsTrue(pushedSettings);
            Assert.IsTrue(settingsModal.IsVisible);

            // Push Confirm modal
            bool pushedConfirm = screenManager.PushModal("Confirm");
            Assert.IsTrue(pushedConfirm);
            Assert.IsTrue(confirmModal.IsVisible);

            // Pop Confirm modal
            bool poppedConfirm = screenManager.PopModal();
            Assert.IsTrue(poppedConfirm);
            Assert.IsFalse(confirmModal.IsVisible);
            Assert.IsTrue(settingsModal.IsVisible);

            // Pop Settings modal
            bool poppedSettings = screenManager.PopModal();
            Assert.IsTrue(poppedSettings);
            Assert.IsFalse(settingsModal.IsVisible);

            // Modal stack empty
            bool poppedEmpty = screenManager.PopModal();
            Assert.IsFalse(poppedEmpty);
        }

        [Test]
        public void ScreenManager_Events_FireOnLifecycleTransitions()
        {
            var screen = new MockScreenController("TestScreen");
            screenManager.RegisterScreen(screen);

            string shownId = null;
            string hiddenId = null;
            string modalPushedId = null;
            string modalPoppedId = null;

            screenManager.OnScreenShown += id => shownId = id;
            screenManager.OnScreenHidden += id => hiddenId = id;
            screenManager.OnModalPushed += id => modalPushedId = id;
            screenManager.OnModalPopped += id => modalPoppedId = id;

            screenManager.ShowScreen("TestScreen");
            Assert.AreEqual("TestScreen", shownId);

            screenManager.HideScreen("TestScreen");
            Assert.AreEqual("TestScreen", hiddenId);

            screenManager.PushModal("TestScreen");
            Assert.AreEqual("TestScreen", modalPushedId);

            screenManager.PopModal();
            Assert.AreEqual("TestScreen", modalPoppedId);
        }

        [Test]
        public void GameManager_SceneConstants_ConfiguredCorrectly()
        {
            Assert.AreEqual("MainMenuScene", GameManager.SceneMainMenu);
            Assert.AreEqual("ShipSelectScene", GameManager.SceneShipSelect);
            Assert.AreEqual("EquipmentSelectScene", GameManager.SceneEquipmentSelect);
            Assert.AreEqual("GameplayScene", GameManager.SceneGameplay);
        }

        [Test]
        public void ShipSelectUI_InitializesAndNavigatesCatalog()
        {
            var go = new GameObject("TestShipSelectUI");
            try
            {
                var shipUI = go.AddComponent<ShipSelectUI>();
                Assert.AreEqual("ShipSelect", shipUI.ScreenId);

                int initialIndex = shipUI.CurrentIndex;
                shipUI.NavigateShip(1);
                Assert.AreNotEqual(initialIndex, shipUI.CurrentIndex);

                shipUI.SelectShipById("scout");
                Assert.AreEqual("scout", shipUI.CurrentShipId);

                shipUI.Show();
                Assert.IsTrue(shipUI.IsVisible);

                shipUI.Hide();
                Assert.IsFalse(shipUI.IsVisible);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void EquipmentSelectUI_EquipsWeaponsAndSkillsPerShip()
        {
            var go = new GameObject("TestEquipmentSelectUI");
            try
            {
                var equipUI = go.AddComponent<EquipmentSelectUI>();
                Assert.AreEqual("EquipmentSelect", equipUI.ScreenId);

                equipUI.EquipWeapon("gatling");
                Assert.AreEqual("gatling", equipUI.InspectedWeaponId);
                Assert.AreEqual("gatling", StorageService.GetEquippedWeapon(equipUI.CurrentShipId));

                equipUI.EquipSkill("blink_teleport");
                Assert.AreEqual("blink_teleport", equipUI.InspectedSkillId);
                Assert.AreEqual("blink_teleport", StorageService.GetSelectedSkill());

                equipUI.Show();
                Assert.IsTrue(equipUI.IsVisible);

                equipUI.Hide();
                Assert.IsFalse(equipUI.IsVisible);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void CyberVectorTheme_GeneratesValidSpritesAndMeters()
        {
            Assert.IsNotNull(CyberVectorTheme.GetVectorGridSprite());
            Assert.IsNotNull(CyberVectorTheme.GetChamferCardSprite());
            Assert.IsNotNull(CyberVectorTheme.GetHoloPedestalSprite());
            Assert.IsNotNull(CyberVectorTheme.GetButtonSprite());
            Assert.IsNotNull(CyberVectorTheme.GetGlowBarSprite());

            string meter = CyberVectorTheme.FormatStatMeter("HULL", 80f, 100f, "HP", CyberVectorTheme.LaserGreen);
            Assert.IsTrue(meter.Contains("■"));
            Assert.IsTrue(meter.Contains("<color="));
            Assert.IsTrue(meter.Contains("80"));
        }
    }
}
