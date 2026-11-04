using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class GameStateTests
    {
        private GameObject gmGo;
        private GameManager gameManager;

        [SetUp]
        public void SetUp()
        {
            gmGo = new GameObject("TestGameManager");
            gameManager = gmGo.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (gmGo != null)
            {
                Object.DestroyImmediate(gmGo);
            }
            Time.timeScale = 1f;
        }

        [Test]
        public void ChangeState_Paused_SetsTimeScaleToZero()
        {
            gameManager.ChangeState(GameState.Paused);

            Assert.AreEqual(GameState.Paused, gameManager.CurrentState);
            Assert.AreEqual(0f, Time.timeScale, "Time.timeScale must be 0 when Paused");
        }

        [Test]
        public void ChangeState_Playing_SetsTimeScaleToOne()
        {
            gameManager.ChangeState(GameState.Paused);
            gameManager.ChangeState(GameState.Playing);

            Assert.AreEqual(GameState.Playing, gameManager.CurrentState);
            Assert.AreEqual(1f, Time.timeScale, "Time.timeScale must be 1 when Playing");
        }
    }
}
