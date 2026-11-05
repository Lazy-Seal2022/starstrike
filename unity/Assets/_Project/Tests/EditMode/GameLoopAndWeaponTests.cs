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
        public void CIBuilder_ScenePath_MatchesActualGameScene()
        {
            string[] scenes = CIBuilder.GetScenes();
            Assert.IsNotNull(scenes);
            Assert.AreEqual(1, scenes.Length);
            Assert.AreEqual("Assets/_Project/Scenes/GameScene.unity", scenes[0]);

            var sceneAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenes[0]);
            Assert.IsNotNull(sceneAsset, "GameScene.unity must exist and be a valid SceneAsset");
        }
    }
}
