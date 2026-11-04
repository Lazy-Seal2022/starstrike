using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class CombatTests
    {
        private GameObject playerGo;
        private ShipHealth health;
        private ShipWeapon weapon;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("TestPlayer");
            playerGo.AddComponent<Rigidbody2D>();
            health = playerGo.AddComponent<ShipHealth>();
            health.maxShield = 100f;
            health.maxEnergy = 80f;
            health.ResetMeters();

            weapon = playerGo.AddComponent<ShipWeapon>();
            weapon.pulseDamage = 20f;
            weapon.pulseSpeed = 22f;
            weapon.fireRate = 0.2f;
            weapon.energyCost = 12f;
        }

        [TearDown]
        public void TearDown()
        {
            if (playerGo != null)
            {
                Object.DestroyImmediate(playerGo);
            }

            var lasers = Object.FindObjectsByType<LaserProjectile>(FindObjectsSortMode.None);
            foreach (var l in lasers)
            {
                Object.DestroyImmediate(l.gameObject);
            }
        }

        [Test]
        public void TakeDamage_ReducesShield()
        {
            health.TakeDamage(30f);
            Assert.AreEqual(70f, health.currentShield, 0.01f);
        }

        [Test]
        public void TakeDamage_CapsAtZeroAndTriggersOnDied()
        {
            bool diedTriggered = false;
            health.OnDied += () => diedTriggered = true;

            health.TakeDamage(150f);

            Assert.AreEqual(0f, health.currentShield);
            Assert.IsTrue(diedTriggered);
        }

        [Test]
        public void ConsumeEnergy_SucceedsWhenSufficient()
        {
            bool consumed = health.ConsumeEnergy(12f);
            Assert.IsTrue(consumed);
            Assert.AreEqual(68f, health.currentEnergy, 0.01f);
        }

        [Test]
        public void ConsumeEnergy_FailsWhenInsufficient()
        {
            health.ConsumeEnergy(75f); // 5 left
            bool consumed = health.ConsumeEnergy(12f);

            Assert.IsFalse(consumed);
            Assert.AreEqual(5f, health.currentEnergy, 0.01f);
        }

        [Test]
        public void ResetMeters_RestoresFullShieldAndEnergy()
        {
            health.TakeDamage(50f);
            health.ConsumeEnergy(40f);

            health.ResetMeters();

            Assert.AreEqual(health.maxShield, health.currentShield);
            Assert.AreEqual(health.maxEnergy, health.currentEnergy);
        }

        [Test]
        public void Weapon_TryFire_FailsWhenInsufficientEnergy()
        {
            health.ConsumeEnergy(75f); // leaves 5, cost is 12
            bool fired = weapon.TryFire(health);

            Assert.IsFalse(fired);
        }

        [Test]
        public void Weapon_TryFire_DeductsEnergyWhenSuccessful()
        {
            float energyBefore = health.currentEnergy;
            bool fired = weapon.TryFire(health);

            Assert.IsTrue(fired);
            Assert.AreEqual(energyBefore - weapon.energyCost, health.currentEnergy, 0.01f);
        }
    }
}
