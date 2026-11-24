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
            health.maxHull = 50f;
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
            Assert.AreEqual(0f, health.currentHull);
            Assert.IsTrue(diedTriggered);
        }

        [Test]
        public void TakeDamage_ShieldAbsorbsFirst_SpillsOverToHull()
        {
            bool diedTriggered = false;
            health.OnDied += () => diedTriggered = true;

            // 100 shield, 50 hull
            health.TakeDamage(60f);
            Assert.AreEqual(40f, health.currentShield, 0.01f);
            Assert.AreEqual(50f, health.currentHull, 0.01f);
            Assert.IsFalse(diedTriggered);

            // 40 shield left, 70 damage -> 40 to shield, 30 to hull
            health.TakeDamage(70f);
            Assert.AreEqual(0f, health.currentShield);
            Assert.AreEqual(20f, health.currentHull, 0.01f);
            Assert.IsFalse(diedTriggered);

            // Deplete remaining hull
            health.TakeDamage(20f);
            Assert.AreEqual(0f, health.currentHull);
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

        [Test]
        public void Weapon_TryFire_AppliesInaccuracySpreadAngle()
        {
            var testObj = new GameObject("TestShip_Spread");
            var testWp = testObj.AddComponent<ShipWeapon>();
            testWp.inaccuracyAngle = 4.0f;
            testWp.fireRate = 0.01f;
            testWp.energyCost = 0f;

            float minAngle = float.MaxValue;
            float maxAngle = float.MinValue;

            for (int i = 0; i < 25; i++)
            {
                testWp.ResetCooldown();
                var before = new System.Collections.Generic.HashSet<LaserProjectile>(
                    Object.FindObjectsByType<LaserProjectile>(FindObjectsInactive.Include, FindObjectsSortMode.None));

                testWp.TryFire(null);

                var after = Object.FindObjectsByType<LaserProjectile>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var l in after)
                {
                    if (!before.Contains(l))
                    {
                        float angle = l.transform.eulerAngles.z;
                        if (angle > 180f) angle -= 360f;
                        if (angle < minAngle) minAngle = angle;
                        if (angle > maxAngle) maxAngle = angle;
                    }
                }
            }

            Object.DestroyImmediate(testObj);

            Assert.LessOrEqual(maxAngle, 4.05f, "Bullet angle must not exceed +inaccuracyAngle");
            Assert.GreaterOrEqual(minAngle, -4.05f, "Bullet angle must not be less than -inaccuracyAngle");
            Assert.Greater(maxAngle - minAngle, 0.5f, "Bullets must show angular spread across multiple shots");
        }

        [Test]
        public void RegenerateMeters_RestoresShieldAndEnergyOverTime()
        {
            health.shieldRegen = 10f;
            health.energyRegen = 20f;
            health.TakeDamage(50f); // Shield at 50 / 100
            health.ConsumeEnergy(40f); // Energy at 40 / 80

            health.RegenerateMeters(2.0f); // 2 seconds

            Assert.AreEqual(70f, health.currentShield, 0.01f);
            Assert.AreEqual(80f, health.currentEnergy, 0.01f); // capped at max (80)
        }

        [Test]
        public void ConsumeEnergy_TotalDepletion_TriggersEnergyLockout()
        {
            health.ConsumeEnergy(80f);
            Assert.AreEqual(0f, health.currentEnergy);
            Assert.IsTrue(health.IsEnergyLockedOut);
        }

        [Test]
        public void EnergyLockout_PreventsWeaponFiringAndConsumingEnergy()
        {
            health.energyRegen = 20f;
            health.ConsumeEnergy(80f);
            Assert.IsTrue(health.IsEnergyLockedOut);

            // Regenerate 10 energy (threshold is 30% of 80 = 24)
            health.RegenerateMeters(0.5f); // 20 regen * 0.5 = 10 energy
            Assert.AreEqual(10f, health.currentEnergy, 0.01f);
            Assert.IsTrue(health.IsEnergyLockedOut);

            Assert.IsFalse(health.HasEnergy(5f));
            Assert.IsFalse(health.ConsumeEnergy(5f));
            Assert.IsFalse(weapon.TryFire(health));
        }

        [Test]
        public void EnergyLockout_RecoversWhenThresholdReached()
        {
            health.energyRegen = 20f;
            health.ConsumeEnergy(80f);
            Assert.IsTrue(health.IsEnergyLockedOut);

            // Regenerate to 30 energy (>= 24 threshold)
            health.RegenerateMeters(1.5f); // 20 regen * 1.5 = 30 energy
            Assert.AreEqual(30f, health.currentEnergy, 0.01f);
            Assert.IsFalse(health.IsEnergyLockedOut);

            Assert.IsTrue(health.HasEnergy(12f));
            bool fired = weapon.TryFire(health);
            Assert.IsTrue(fired);
            Assert.AreEqual(18f, health.currentEnergy, 0.01f);
        }

        [Test]
        public void EnergyLockout_ConfigurableFixedThreshold()
        {
            health.energyRegen = 20f;
            health.SetEnergyRecoveryThreshold(50f);
            Assert.AreEqual(50f, health.EnergyRecoveryThreshold);

            health.ConsumeEnergy(80f);
            Assert.IsTrue(health.IsEnergyLockedOut);

            // Regenerate to 40 energy (< 50)
            health.RegenerateMeters(2.0f);
            Assert.AreEqual(40f, health.currentEnergy, 0.01f);
            Assert.IsTrue(health.IsEnergyLockedOut);

            // Regenerate to 60 energy (>= 50)
            health.RegenerateMeters(1.0f);
            Assert.AreEqual(60f, health.currentEnergy, 0.01f);
            Assert.IsFalse(health.IsEnergyLockedOut);
        }
    }
}
