using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.UI;
using StarStrike.Editor;

namespace StarStrike.Tests.EditMode
{
    public class GameLoopAndWeaponTests
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
        public void ShipWeapon_CalculateMuzzleOffsets_SupportsSlots1_2_3()
        {
            float spread = 0.35f;

            // 1 Slot (Center only)
            float[] offsets1 = ShipWeapon.CalculateMuzzleOffsets(1, spread);
            Assert.AreEqual(1, offsets1.Length);
            Assert.AreEqual(0f, offsets1[0]);

            // 2 Slots (Dual spread)
            float[] offsets2 = ShipWeapon.CalculateMuzzleOffsets(2, spread);
            Assert.AreEqual(2, offsets2.Length);
            Assert.AreEqual(-spread, offsets2[0]);
            Assert.AreEqual(spread, offsets2[1]);

            // 3 Slots (Triple spread: Left, Center, Right)
            float[] offsets3 = ShipWeapon.CalculateMuzzleOffsets(3, spread);
            Assert.AreEqual(3, offsets3.Length);
            Assert.AreEqual(-spread, offsets3[0]);
            Assert.AreEqual(0f, offsets3[1]);
            Assert.AreEqual(spread, offsets3[2]);

            // Fallback for > 3 slots (Triple spread)
            float[] offsets4 = ShipWeapon.CalculateMuzzleOffsets(4, spread);
            Assert.AreEqual(3, offsets4.Length);
        }

        [Test]
        public void RunSummary_ScoreCalculation_IsAccurate()
        {
            int scoreVictory = RunSummary.CalculateScore(gems: 10, asteroids: 5, enemies: 4, bossKilled: true, time: 65f);
            // (10 * 15) + (5 * 10) + (4 * 50) + 1000 + (65 * 2) = 150 + 50 + 200 + 1000 + 130 = 1530
            Assert.AreEqual(1530, scoreVictory);

            int scoreDefeat = RunSummary.CalculateScore(gems: 4, asteroids: 2, enemies: 1, bossKilled: false, time: 20f);
            // (4 * 15) + (2 * 10) + (1 * 50) + 0 + (20 * 2) = 60 + 20 + 50 + 40 = 170
            Assert.AreEqual(170, scoreDefeat);
        }

        [Test]
        public void GameManager_Victory_TransitionsStateAndRecordsStats()
        {
            var gmGo = Track(new GameObject("TestGameManager"));
            var gm = gmGo.AddComponent<GameManager>();

            RunSummary receivedSummary = default;
            bool eventTriggered = false;
            System.Action<RunSummary> handler = s => { receivedSummary = s; eventTriggered = true; };
            GameEvents.OnVictory += handler;

            try
            {
                gm.StartGame();
                Assert.AreEqual(GameState.Playing, gm.CurrentState);

                gm.Victory();

                Assert.AreEqual(GameState.Victory, gm.CurrentState);
                Assert.IsTrue(eventTriggered, "OnVictory event must be invoked");
                Assert.IsTrue(receivedSummary.isVictory);
                Assert.IsTrue(receivedSummary.bossDefeated);
                Assert.AreEqual(gm.LastRunSummary.finalScore, receivedSummary.finalScore);
            }
            finally
            {
                GameEvents.OnVictory -= handler;
            }
        }

        [Test]
        public void GameManager_GameOver_TransitionsStateAndRecordsStats()
        {
            var gmGo = Track(new GameObject("TestGameManager"));
            var gm = gmGo.AddComponent<GameManager>();

            RunSummary receivedSummary = default;
            bool eventTriggered = false;
            System.Action<RunSummary> handler = s => { receivedSummary = s; eventTriggered = true; };
            GameEvents.OnGameOver += handler;

            try
            {
                gm.StartGame();
                Assert.AreEqual(GameState.Playing, gm.CurrentState);

                gm.GameOver();

                Assert.AreEqual(GameState.GameOver, gm.CurrentState);
                Assert.IsTrue(eventTriggered, "OnGameOver event must be invoked");
                Assert.IsFalse(receivedSummary.isVictory);
                Assert.IsFalse(receivedSummary.bossDefeated);
            }
            finally
            {
                GameEvents.OnGameOver -= handler;
            }
        }

        [Test]
        public void HUDEndGameModal_Show_RendersStatsAndTogglesVisibility()
        {
            var modalGo = Track(new GameObject("TestEndGameModal"));
            var modal = modalGo.AddComponent<HUDEndGameModal>();

            Assert.IsFalse(modal.IsVisible, "Modal should be hidden on start");

            var summary = new RunSummary
            {
                isVictory = true,
                timeSurvived = 125f,
                gemsCollected = 25,
                asteroidsMined = 12,
                enemiesDestroyed = 8,
                bossDefeated = true,
                finalShipId = "fighter",
                finalScore = 2450
            };

            modal.Show(summary);

            Assert.IsTrue(modal.IsVisible, "Modal should become visible after Show()");
            Assert.IsTrue(modal.titleText.text.Contains("VICTORY"), "Title text should display VICTORY");
            Assert.IsTrue(modal.scoreText.text.Contains("2,450") || modal.scoreText.text.Contains("2450"), "Score text should format score");

            modal.SetVisible(false);
            Assert.IsFalse(modal.IsVisible, "Modal should hide when SetVisible(false) is called");
        }

        [Test]
        public void CanvasHUD_SetHUDVisible_DoesNotActivateEndGameModalPanel()
        {
            var hudGo = Track(new GameObject("TestHUD"));
            var hud = hudGo.AddComponent<CanvasHUD>();
            var modal = hudGo.AddComponent<HUDEndGameModal>();
            hud.endGameModal = modal;
            modal.SetVisible(false);

            Assert.IsFalse(modal.IsVisible, "Modal should initially be hidden");

            // Calling SetHUDVisible(true) must NOT show the end game modal
            hud.SetHUDVisible(true);
            Assert.IsFalse(modal.IsVisible, "EndGameModalPanel must NOT be activated by SetHUDVisible(true)");

            // If modal was shown explicitly, SetHUDVisible(false) properly hides it
            modal.SetVisible(true);
            Assert.IsTrue(modal.IsVisible);
            hud.SetHUDVisible(false);
            Assert.IsFalse(modal.IsVisible, "SetHUDVisible(false) should ensure EndGameModalPanel is hidden");
        }

        [Test]
        public void CIBuilder_ScenePath_MatchesActualGameScene()
        {
            string[] scenes = CIBuilder.GetScenes();
            Assert.IsNotNull(scenes);
            Assert.AreEqual(1, scenes.Length);
            Assert.AreEqual("Assets/_Project/Scenes/GameScene.unity", scenes[0]);

            var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenes[0]);
            Assert.IsNotNull(sceneAsset, "GameScene.unity must exist and be a valid SceneAsset");
        }

        [Test]
        public void PlayerInputReader_SingleTap_FiresSingleShot()
        {
            var go = Track(new GameObject("TestPlayer_Tap"));
            var health = go.AddComponent<ShipHealth>();
            health.maxEnergy = 80f;
            health.ResetMeters();

            var weapon = go.AddComponent<ShipWeapon>();
            weapon.fireRate = 0.22f;
            weapon.energyCost = 6f;

            var reader = go.AddComponent<PlayerInputReader>();
            reader.weapon = weapon;
            reader.health = health;

            // Frame 1: Tap press
            reader.inputFire = true;
            reader.ProcessFiring(0.016f); // 16ms
            Assert.AreEqual(74f, health.currentEnergy, 0.01f, "First tap must fire 1 shot deducting 6 energy");

            // Frame 2: Still held within the tap duration (16ms later, total 32ms < 220ms)
            reader.inputFire = true;
            reader.ProcessFiring(0.016f);
            Assert.AreEqual(74f, health.currentEnergy, 0.01f, "Within tap duration, no extra shot should fire");

            // Frame 3: Finger released
            reader.inputFire = false;
            reader.ProcessFiring(0.016f);
            Assert.AreEqual(74f, health.currentEnergy, 0.01f, "After release, energy remains same");
        }

        [Test]
        public void PlayerInputReader_HoldFire_AutoFiresAtFireRate()
        {
            var go = Track(new GameObject("TestPlayer_Hold"));
            var health = go.AddComponent<ShipHealth>();
            health.maxEnergy = 80f;
            health.ResetMeters();

            var weapon = go.AddComponent<ShipWeapon>();
            weapon.fireRate = 0.22f;
            weapon.energyCost = 6f;

            var reader = go.AddComponent<PlayerInputReader>();
            reader.weapon = weapon;
            reader.health = health;

            // Initial press (t = 0)
            reader.inputFire = true;
            reader.ProcessFiring(0.016f);
            Assert.AreEqual(74f, health.currentEnergy, 0.01f, "Initial shot fires immediately");

            // Hold for 0.10s (total held < 0.22s) -> no fire
            reader.ProcessFiring(0.10f);
            Assert.AreEqual(74f, health.currentEnergy, 0.01f, "No fire before fireRate repeat interval");

            // Advance time to surpass fireRate (another 0.12s, total 0.22s)
            // In EditMode Time.time is constant, but ProcessFiring respects repeatInterval
            // Now simulate next fire cycle
            reader.inputFire = false;
            reader.ResetInputState();
            Assert.IsFalse(reader.inputFire);
        }

        [Test]
        public void PlayerInputReader_ResetInputState_ClearsAllFlags()
        {
            var go = Track(new GameObject("TestPlayer_Reset"));
            var reader = go.AddComponent<PlayerInputReader>();
            reader.SetFire(true);
            reader.SetThrust(true);
            reader.SetBoost(true);
            reader.SetBrake(true);
            reader.inputFire = true;

            reader.ResetInputState();

            Assert.IsFalse(reader.inputFire, "inputFire must be false");
            Assert.IsFalse(reader.inputThrust, "inputThrust must be false");
            Assert.IsFalse(reader.inputBoost, "inputBoost must be false");
            Assert.IsFalse(reader.inputBrake, "inputBrake must be false");
        }

        [Test]
        public void ShipWeapon_DefaultEnergyCost_IsSustainable()
        {
            var go = Track(new GameObject("TestWeapon_Energy"));
            var weapon = go.AddComponent<ShipWeapon>();

            Assert.AreEqual(6f, weapon.energyCost, 0.01f, "Default energy cost should be 6f for sustainable combat");
        }
    }
}
