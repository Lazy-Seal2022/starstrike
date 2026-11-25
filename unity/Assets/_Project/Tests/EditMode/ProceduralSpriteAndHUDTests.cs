using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class ProceduralSpriteAndHUDTests
    {
        private GameObject hudGo;
        private CanvasHUD hud;
        private GameObject playerGo;
        private PlayerProgression progression;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("TestPlayer");
            playerGo.AddComponent<Rigidbody2D>();
            playerGo.AddComponent<ShipHealth>();
            playerGo.AddComponent<ShipMovement>();
            playerGo.AddComponent<ShipWeapon>();

            progression = playerGo.AddComponent<PlayerProgression>();
            progression.Initialize();

            hudGo = new GameObject("TestHUD");
            hud = hudGo.AddComponent<CanvasHUD>();
        }

        [TearDown]
        public void TearDown()
        {
            if (hud != null) hud.UnsubscribeEvents();
            if (hudGo != null) Object.DestroyImmediate(hudGo);
            if (playerGo != null) Object.DestroyImmediate(playerGo);
        }

        [Test]
        public void ProceduralSpriteHelper_GeneratesDistinctSpritesForAllShipTypes()
        {
            Sprite scout = ProceduralSpriteHelper.GetShipSprite("scout");
            Sprite fighter = ProceduralSpriteHelper.GetShipSprite("fighter");
            Sprite miner = ProceduralSpriteHelper.GetShipSprite("miner");
            Sprite drone = ProceduralSpriteHelper.GetShipSprite("drone");
            Sprite interceptor = ProceduralSpriteHelper.GetShipSprite("interceptor");
            Sprite juggernaut = ProceduralSpriteHelper.GetShipSprite("juggernaut");
            Sprite boss = ProceduralSpriteHelper.GetShipSprite("boss");

            Assert.IsNotNull(scout, "Scout sprite must not be null");
            Assert.IsNotNull(fighter, "Fighter sprite must not be null");
            Assert.IsNotNull(miner, "Miner sprite must not be null");
            Assert.IsNotNull(drone, "Drone sprite must not be null");
            Assert.IsNotNull(interceptor, "Interceptor sprite must not be null");
            Assert.IsNotNull(juggernaut, "Juggernaut sprite must not be null");
            Assert.IsNotNull(boss, "Boss sprite must not be null");

            Assert.AreNotSame(scout, interceptor, "Interceptor must have its own distinct sprite");
            Assert.AreNotSame(scout, juggernaut, "Juggernaut must have its own distinct sprite");
            Assert.AreNotSame(scout, boss, "Boss must have its own distinct sprite");
        }

        [Test]
        public void CanvasHUD_RebuildEvolutionButtons_SpawnsTier3ButtonsCorrectly()
        {
            ShipDefinition root = PlayerProgression.CreateDefaultShipTree();
            ShipDefinition fighter = root.evolvesTo[0];
            progression.currentShip = fighter;

            hud.RebuildEvolutionButtons(progression);

            Button[] buttons = hud.GetComponentsInChildren<Button>(true);
            bool foundInterceptor = false;
            foreach (var b in buttons)
            {
                Text t = b.GetComponentInChildren<Text>();
                if (t != null && t.text.Contains("VIPER INTERCEPTOR"))
                {
                    foundInterceptor = true;
                    break;
                }
            }
            Assert.IsTrue(foundInterceptor, "HUD must dynamically generate a button for VIPER INTERCEPTOR when player is a Fighter");

            ShipDefinition miner = root.evolvesTo[1];
            progression.currentShip = miner;

            hud.RebuildEvolutionButtons(progression);

            buttons = hud.GetComponentsInChildren<Button>(true);
            bool foundJuggernaut = false;
            foreach (var b in buttons)
            {
                Text t = b.GetComponentInChildren<Text>();
                if (t != null && t.text.Contains("TITAN JUGGERNAUT"))
                {
                    foundJuggernaut = true;
                    break;
                }
            }
            Assert.IsTrue(foundJuggernaut, "HUD must dynamically generate a button for TITAN JUGGERNAUT when player is a Miner");
        }

        [Test]
        public void CanvasHUD_DisplaysWaveBossAndHazardEvents()
        {
            hud.EnsureInitialized();
            GameEvents.TriggerWaveStarted(2, "Asteroid Incursion", 45f);
            Assert.IsNotNull(hud.waveTitleText);
            StringAssert.Contains("WAVE 2", hud.waveTitleText.text);
            Assert.AreEqual("TIME  00:45", hud.waveTimerText.text);

            GameEvents.TriggerWaveTimeUpdated(12f);
            Assert.AreEqual("TIME  00:12", hud.waveTimerText.text);

            GameEvents.TriggerBossSpawned("Dreadnought Flagship", 1500f, 1500f);
            Assert.IsTrue(hud.bossHealthPanel.activeSelf, "Boss health panel must show on boss spawn");
            StringAssert.Contains("1500 / 1500", hud.bossHpText.text);

            GameEvents.TriggerBossHealthChanged(750f, 1500f, "ENRAGED");
            Assert.AreEqual(0.5f, hud.bossHealthFill.fillAmount, 0.01f);
            StringAssert.Contains("ENRAGED", hud.bossTitleText.text);

            GameEvents.TriggerBossDefeated();
            Assert.IsFalse(hud.bossHealthPanel.activeSelf, "Boss health panel must hide after boss defeat");

            GameEvents.TriggerHazardWarning("SOLAR FLARE DETECTED", 3f);
            Assert.IsTrue(hud.hazardBannerPanel.activeSelf, "Hazard banner must show on warning");
            StringAssert.Contains("WARNING:", hud.hazardBannerText.text);

            GameEvents.TriggerHazardTriggered("SOLAR FLARE");
            StringAssert.Contains("ACTIVE", hud.hazardBannerText.text);
        }
    }
}
