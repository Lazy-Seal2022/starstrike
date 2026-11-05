using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class MiningEconomyTests
    {
        private GameObject asteroidGo;

        [TearDown]
        public void TearDown()
        {
            if (asteroidGo != null)
            {
                Object.DestroyImmediate(asteroidGo);
            }
        }

        [Test]
        public void AsteroidTier_Large_HasExpectedStats()
        {
            asteroidGo = new GameObject("LargeAsteroid");
            var ast = asteroidGo.AddComponent<Asteroid>();
            ast.tier = AsteroidTier.Large;
            ast.SetupStats();

            Assert.AreEqual(120f, ast.maxHp);
            Assert.AreEqual(120f, ast.currentHp);
            Assert.AreEqual(8, ast.gemDropCount);
            Assert.AreEqual(2.2f, asteroidGo.transform.localScale.x, 0.01f);
        }

        [Test]
        public void AsteroidTier_Medium_HasExpectedStats()
        {
            asteroidGo = new GameObject("MediumAsteroid");
            var ast = asteroidGo.AddComponent<Asteroid>();
            ast.tier = AsteroidTier.Medium;
            ast.SetupStats();

            Assert.AreEqual(50f, ast.maxHp);
            Assert.AreEqual(50f, ast.currentHp);
            Assert.AreEqual(4, ast.gemDropCount);
            Assert.AreEqual(1.3f, asteroidGo.transform.localScale.x, 0.01f);
        }

        [Test]
        public void AsteroidTier_Small_HasExpectedStats()
        {
            asteroidGo = new GameObject("SmallAsteroid");
            var ast = asteroidGo.AddComponent<Asteroid>();
            ast.tier = AsteroidTier.Small;
            ast.SetupStats();

            Assert.AreEqual(20f, ast.maxHp);
            Assert.AreEqual(20f, ast.currentHp);
            Assert.AreEqual(1, ast.gemDropCount);
            Assert.AreEqual(0.75f, asteroidGo.transform.localScale.x, 0.01f);
        }

        [Test]
        public void Asteroid_TakeDamage_ReducesCurrentHp()
        {
            asteroidGo = new GameObject("AsteroidDmgTest");
            var ast = asteroidGo.AddComponent<Asteroid>();
            ast.tier = AsteroidTier.Large;
            ast.SetupStats();

            ast.TakeDamage(45f);

            Assert.AreEqual(75f, ast.currentHp, 0.01f);
        }
    }
}
