using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.Audio;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class P1ArchitectureTests
    {
        private GameObject testRoot;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot_P1");
        }

        [TearDown]
        public void TearDown()
        {
            if (testRoot != null) Object.DestroyImmediate(testRoot);
        }

        [Test]
        public void AudioService_LinearToDecibels_ComputesAccurateAttenuation()
        {
            Assert.AreEqual(0f, AudioService.LinearToDecibels(1.0f), 0.01f, "1.0 volume should be 0 dB");
            Assert.AreEqual(-20f, AudioService.LinearToDecibels(0.1f), 0.01f, "0.1 volume should be -20 dB");
            Assert.AreEqual(-80f, AudioService.LinearToDecibels(0.00001f), 0.01f, "Near-zero volume should attenuate to -80 dB threshold");
            Assert.AreEqual(-80f, AudioService.LinearToDecibels(0f), 0.01f, "Zero volume should attenuate to -80 dB threshold");
        }

        [Test]
        public void ProceduralSpriteHelper_ReturnsValidSprites_ForAllStandardKeys()
        {
            string[] keys = { "scout", "fighter", "miner", "interceptor", "juggernaut", "drone", "boss" };
            foreach (var key in keys)
            {
                var sprite = ProceduralSpriteHelper.GetShipSprite(key);
                Assert.IsNotNull(sprite, $"Sprite for ship '{key}' must be valid and non-null");
                Assert.IsTrue(sprite.rect.width > 0 && sprite.rect.height > 0, $"Sprite '{key}' must have valid dimensions");
            }

            var laser = ProceduralSpriteHelper.GetLaserSprite(true);
            Assert.IsNotNull(laser, "Player laser sprite must not be null");

            var gem = ProceduralSpriteHelper.GetGemSprite();
            Assert.IsNotNull(gem, "Gem sprite must not be null");

            var asteroid = ProceduralSpriteHelper.GetAsteroidSprite();
            Assert.IsNotNull(asteroid, "Asteroid sprite must not be null");
        }

        [Test]
        public void UIHelper_CreateTextMeshProExplicit_ConfiguresTextMeshProUGUIProperly()
        {
            var tmp = UIHelper.CreateTextMeshProExplicit(
                testRoot.transform,
                "TestLabel",
                "STAR STRIKE",
                22,
                FontStyles.Bold,
                Color.cyan,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(200f, 40f));

            Assert.IsNotNull(tmp);
            Assert.AreEqual("STAR STRIKE", tmp.text);
            Assert.AreEqual(22, tmp.fontSize);
            Assert.AreEqual(FontStyles.Bold, tmp.fontStyle);
            Assert.AreEqual(Color.cyan, tmp.color);
            Assert.AreEqual(TextAlignmentOptions.Center, tmp.alignment);
            Assert.IsFalse(tmp.raycastTarget, "Non-interactive label should not be a raycast target");
        }

        [Test]
        public void SettingsUI_ConstructsWithTextMeshPro_AndContainsNoLegacyText()
        {
            var settingsGo = new GameObject("SettingsUI");
            settingsGo.transform.SetParent(testRoot.transform, false);
            var settings = settingsGo.AddComponent<SettingsUI>();
            settings.EnsureUIBuilt();

            var legacyTexts = settingsGo.GetComponentsInChildren<Text>(true);
            Assert.AreEqual(0, legacyTexts.Length, "SettingsUI hierarchy should not contain legacy UnityEngine.UI.Text elements");

            var tmpTexts = settingsGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            Assert.Greater(tmpTexts.Length, 5, "SettingsUI should contain TextMeshProUGUI components for titles, buttons, and volume labels");
        }

        [Test]
        public void NetworkLobbyUI_ConstructsWithTextMeshPro_AndContainsNoLegacyText()
        {
            var lobbyGo = new GameObject("NetworkLobbyUI");
            lobbyGo.transform.SetParent(testRoot.transform, false);
            var lobby = lobbyGo.AddComponent<NetworkLobbyUI>();
            lobby.EnsureUIBuilt();

            var legacyTexts = lobbyGo.GetComponentsInChildren<Text>(true);
            Assert.AreEqual(0, legacyTexts.Length, "NetworkLobbyUI hierarchy should not contain legacy UnityEngine.UI.Text elements");

            var tmpTexts = lobbyGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            Assert.Greater(tmpTexts.Length, 4, "NetworkLobbyUI should contain TextMeshProUGUI components");
        }

        [Test]
        public void NetworkScoreboardUI_ConstructsWithTextMeshPro_AndContainsNoLegacyText()
        {
            var scoreboardGo = new GameObject("NetworkScoreboardUI");
            scoreboardGo.transform.SetParent(testRoot.transform, false);
            var scoreboard = scoreboardGo.AddComponent<NetworkScoreboardUI>();
            scoreboard.EnsureUIBuilt();

            var legacyTexts = scoreboardGo.GetComponentsInChildren<Text>(true);
            Assert.AreEqual(0, legacyTexts.Length, "NetworkScoreboardUI hierarchy should not contain legacy UnityEngine.UI.Text elements");

            var tmpTexts = scoreboardGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            Assert.Greater(tmpTexts.Length, 0, "NetworkScoreboardUI should contain TextMeshProUGUI components");
        }
    }
}
