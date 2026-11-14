using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class PolymorphicArchitectureTests
    {
        private GameObject testGo;

        [SetUp]
        public void SetUp()
        {
            testGo = new GameObject("TestRoot_Polymorphic");
        }

        [TearDown]
        public void TearDown()
        {
            if (testGo != null) Object.DestroyImmediate(testGo);
        }

        [Test]
        public void IWeapon_PolymorphicTryFire_ConsumesEnergyFromIEnergyConsumer()
        {
            var health = testGo.AddComponent<ShipHealth>();
            health.maxEnergy = 80f;
            health.ResetMeters();

            var weapon = testGo.AddComponent<ShipWeapon>();
            weapon.energyCost = 6f;
            weapon.fireRate = 0.22f;

            IWeapon weaponInterface = weapon;
            IEnergyConsumer consumer = health;

            Assert.AreEqual(0.22f, weaponInterface.FireRate);
            Assert.AreEqual(6f, weaponInterface.EnergyCost);
            Assert.AreEqual(80f, consumer.CurrentEnergy);

            bool fired = weaponInterface.TryFire(consumer);
            Assert.IsTrue(fired, "IWeapon.TryFire must succeed with valid IEnergyConsumer");
            Assert.AreEqual(74f, consumer.CurrentEnergy, 0.01f);
        }

        [Test]
        public void IPowerUpEffect_AppliesPolymorphically_WithoutEnumConditionals()
        {
            var health = testGo.AddComponent<ShipHealth>();
            health.maxShield = 100f;
            health.ResetMeters();

            var weapon = testGo.AddComponent<ShipWeapon>();
            weapon.weaponSlots = 1;
            weapon.fireRate = 0.22f;

            var config = ScriptableObject.CreateInstance<LootConfig>();
            config.shieldOverchargeBonus = 50f;
            config.weaponOverdriveFireRateMultiplier = 0.8f;
            config.minFireRate = 0.10f;

            IPowerUpEffect shieldEffect = new ShieldOverchargeEffect();
            IPowerUpEffect weaponEffect = new WeaponOverdriveEffect();

            // 1. Apply shield effect
            shieldEffect.Apply(testGo, config);
            Assert.AreEqual(150f, health.maxShield, "ShieldOverchargeEffect should increase shield capacity");
            Assert.AreEqual(150f, health.currentShield);

            // 2. Apply weapon effect
            weaponEffect.Apply(testGo, config);
            Assert.AreEqual(2, weapon.WeaponSlots, "WeaponOverdriveEffect should increment weapon slots");
            Assert.AreEqual(0.176f, weapon.FireRate, 0.001f, "WeaponOverdriveEffect should apply multiplier");
        }

        [Test]
        public void IPoolable_NotifiesSpawnAndDespawnLifecycles()
        {
            var poolManagerGo = new GameObject("TestPoolManager");
            poolManagerGo.transform.SetParent(testGo.transform);
            var pool = poolManagerGo.AddComponent<PoolManager>();
            pool.Initialize();

            var laser = pool.Spawn("TestLaser", Vector3.zero, Quaternion.identity, () =>
            {
                var go = new GameObject("Laser");
                go.AddComponent<LaserProjectile>();
                return go;
            });

            Assert.IsNotNull(laser);
            Assert.IsTrue(laser.activeInHierarchy);
            Assert.IsNotNull(laser.GetComponent<IPoolable>(), "LaserProjectile must implement IPoolable");

            // Despawn through PoolManager
            pool.Despawn(laser);
            Assert.IsFalse(laser.activeInHierarchy, "Despawned object should be deactivated");
        }

        [Test]
        public void IShipInputProvider_FeedsShipMovementPolymorphically()
        {
            var movement = testGo.AddComponent<ShipMovement>();
            var inputReader = testGo.AddComponent<PlayerInputReader>();

            inputReader.inputThrust = true;
            inputReader.inputBoost = true;
            inputReader.inputTurn = -1f;
            inputReader.inputAimDirection = new Vector2(0f, 1f);

            IShipInputProvider provider = inputReader;

            movement.ApplyInput(provider);

            Assert.IsTrue(movement.IsThrusting, "ShipMovement.IsThrusting should match provider");
            Assert.IsTrue(movement.IsBoosting, "ShipMovement.IsBoosting should match provider");
            Assert.AreEqual(-1f, movement.TurnInput, "ShipMovement.TurnInput should match provider");
            Assert.AreEqual(new Vector2(0f, 1f), movement.AimDirection, "ShipMovement.AimDirection should match provider");
        }

        [Test]
        public void IProjectile_ExposesConsistentContracts_AcrossDifferentProjectiles()
        {
            var laserGo = new GameObject("Laser");
            laserGo.transform.SetParent(testGo.transform);
            var laser = laserGo.AddComponent<LaserProjectile>();
            laser.damage = 18f;
            laser.speed = 25f;

            var shockGo = new GameObject("Shockwave");
            shockGo.transform.SetParent(testGo.transform);
            var shock = shockGo.AddComponent<ShockwaveProjectile>();
            shock.damage = 40f;
            shock.speed = 22f;

            IProjectile p1 = laser;
            IProjectile p2 = shock;

            Assert.AreEqual(18f, p1.Damage);
            Assert.AreEqual(25f, p1.Speed);
            Assert.AreEqual(40f, p2.Damage);
            Assert.AreEqual(22f, p2.Speed);
        }
    }
}
