using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class WeaponSystemPolymorphicTests
    {
        private GameObject playerGo;
        private ShipHealth health;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("Player_TestWeaponSystems");
            playerGo.tag = "Player";
            health = playerGo.AddComponent<ShipHealth>();
            health.maxEnergy = 100f;
            health.maxShield = 100f;
            health.ResetMeters();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
        }

        [Test]
        public void ScattergunWeapon_FiresPolymorphically_ConsumesEnergyAndHasCooldown()
        {
            var scattergun = playerGo.AddComponent<ScattergunWeapon>();
            scattergun.energyCost = 14f;
            scattergun.fireRate = 0.45f;
            scattergun.weaponSlots = 1;

            IWeapon weapon = scattergun;
            IEnergyConsumer consumer = health;

            Assert.AreEqual("Scattergun", weapon.WeaponName);
            Assert.AreEqual(14f, weapon.EnergyCost);
            Assert.AreEqual(100f, consumer.CurrentEnergy);

            bool fired = weapon.TryFire(consumer);
            Assert.IsTrue(fired, "Scattergun must successfully fire via IWeapon");
            Assert.AreEqual(86f, consumer.CurrentEnergy, 0.01f);

            // Immediate second fire must fail due to cooldown
            bool firedTooQuickly = weapon.TryFire(consumer);
            Assert.IsFalse(firedTooQuickly, "Scattergun must respect fireRate cooldown");
        }

        [Test]
        public void HomingMissileWeapon_FiresPolymorphically_AndConsumesEnergy()
        {
            var missileWeapon = playerGo.AddComponent<HomingMissileWeapon>();
            missileWeapon.energyCost = 16f;
            missileWeapon.fireRate = 0.70f;
            missileWeapon.weaponSlots = 1;

            IWeapon weapon = missileWeapon;
            IEnergyConsumer consumer = health;

            Assert.AreEqual("Homing Missiles", weapon.WeaponName);
            Assert.AreEqual(16f, weapon.EnergyCost);

            bool fired = weapon.TryFire(consumer);
            Assert.IsTrue(fired, "HomingMissileWeapon must fire via IWeapon");
            Assert.AreEqual(84f, consumer.CurrentEnergy, 0.01f);
        }

        [Test]
        public void ContinuousBeamWeapon_FiresAndDealsContinuousDamage_AndStops()
        {
            var beamWeapon = playerGo.AddComponent<ContinuousBeamWeapon>();
            beamWeapon.damagePerSecond = 80f;
            beamWeapon.energyCostPerSecond = 20f;
            beamWeapon.maxRange = 20f;

            IWeapon weapon = beamWeapon;
            IEnergyConsumer consumer = health;

            Assert.AreEqual("Beam Laser", weapon.WeaponName);

            // Create target dummy in front of ship
            var dummyGo = new GameObject("TargetEnemy");
            dummyGo.tag = "Enemy";
            dummyGo.transform.position = playerGo.transform.position + Vector3.right * 5f;
            var dummyHealth = dummyGo.AddComponent<ShipHealth>();
            dummyHealth.maxShield = 100f;
            dummyHealth.currentShield = 100f;
            var dummyCollider = dummyGo.AddComponent<CircleCollider2D>();
            dummyCollider.radius = 1f;

            bool fired = weapon.TryFire(consumer);
            Assert.IsTrue(fired, "ContinuousBeamWeapon must fire when energy is sufficient");
            Assert.IsTrue(consumer.CurrentEnergy < 100f, "ContinuousBeamWeapon must consume energy");

            weapon.StopFiring();
            if (beamWeapon.beamRenderer != null)
            {
                Assert.IsFalse(beamWeapon.beamRenderer.enabled, "StopFiring must disable the beam renderer");
            }

            Object.DestroyImmediate(dummyGo);
        }

        [Test]
        public void ShipWeaponController_ManagesAllWeapons_AndSwapsViaIndex()
        {
            var controller = playerGo.AddComponent<ShipWeaponController>();
            controller.InitializeWeapons();

            Assert.GreaterOrEqual(controller.WeaponCount, 5, "Player ship must initialize with all 5 weapon types");

            // 0: Pulse Laser, 1: Scattergun, 2: Homing Missiles, 3: Beam Laser, 4: Gatling Gun
            Assert.AreEqual("Pulse Laser", controller.CurrentWeapon.WeaponName);

            controller.SwitchWeapon(1);
            Assert.AreEqual("Scattergun", controller.CurrentWeapon.WeaponName);

            controller.SwitchWeapon(2);
            Assert.AreEqual("Homing Missiles", controller.CurrentWeapon.WeaponName);

            controller.SwitchWeapon(3);
            Assert.AreEqual("Beam Laser", controller.CurrentWeapon.WeaponName);

            controller.SwitchWeapon(4);
            Assert.AreEqual("Gatling Gun", controller.CurrentWeapon.WeaponName);

            // Cycling forward and backward
            controller.NextWeapon();
            Assert.AreEqual("Pulse Laser", controller.CurrentWeapon.WeaponName);

            controller.PreviousWeapon();
            Assert.AreEqual("Gatling Gun", controller.CurrentWeapon.WeaponName);
        }

        [Test]
        public void GatlingWeapon_FiresPolymorphically_SpoolsUpAndBloomsSpread()
        {
            var gatling = playerGo.AddComponent<GatlingWeapon>();
            gatling.energyCost = 2f;
            gatling.baseFireRate = 0.08f;
            gatling.maxFireRate = 0.05f;
            gatling.baseInaccuracyAngle = 3.2f;
            gatling.maxInaccuracyAngle = 7.5f;

            IWeapon weapon = gatling;
            IEnergyConsumer consumer = health;

            Assert.AreEqual("Gatling Gun", weapon.WeaponName);
            Assert.AreEqual(2f, weapon.EnergyCost);
            Assert.AreEqual(3.2f, weapon.InaccuracyAngle, 0.01f, "Initial cold spread should be baseInaccuracyAngle");

            bool fired = weapon.TryFire(consumer);
            Assert.IsTrue(fired, "GatlingWeapon must fire via IWeapon");
            Assert.Greater(gatling.SpoolProgress, 0f, "Firing must increase spool progress");
            Assert.Greater(weapon.InaccuracyAngle, 3.2f, "Continuous firing must bloom spread angle");
            Assert.LessOrEqual(weapon.InaccuracyAngle, 7.55f, "Spread must be clamped to maxInaccuracyAngle");
        }

        [Test]
        public void WeaponPickupItem_AppliesToPlayerShip_SwitchesAndIncrementsSlots()
        {
            var controller = playerGo.AddComponent<ShipWeaponController>();
            controller.InitializeWeapons();

            var pickupGo = new GameObject("TestPickup");
            var pickup = pickupGo.AddComponent<WeaponPickupItem>();
            pickup.pickupType = WeaponPickupItem.WeaponPickupType.Scattergun;

            pickup.ApplyToPlayer(playerGo);

            Assert.AreEqual("Scattergun", controller.CurrentWeapon.WeaponName, "Pickup must switch active weapon to Scattergun");
            Assert.GreaterOrEqual(controller.CurrentWeapon.WeaponSlots, 2, "Pickup must increment weapon slots");

            Object.DestroyImmediate(pickupGo);
        }

        [Test]
        public void UserWeaponLoadout_EquipsAndPersistsPerShip()
        {
            // Save equipped weapons per ship in storage
            StarStrike.Platform.Storage.StorageService.SaveEquippedWeapon("scout", "gatling");
            StarStrike.Platform.Storage.StorageService.SaveEquippedWeapon("fighter", "scattergun");
            StarStrike.Platform.Storage.StorageService.SaveEquippedWeapon("juggernaut", "beam");

            Assert.AreEqual("gatling", StarStrike.Platform.Storage.StorageService.GetEquippedWeapon("scout"));
            Assert.AreEqual("scattergun", StarStrike.Platform.Storage.StorageService.GetEquippedWeapon("fighter"));
            Assert.AreEqual("beam", StarStrike.Platform.Storage.StorageService.GetEquippedWeapon("juggernaut"));

            var controller = playerGo.AddComponent<ShipWeaponController>();
            controller.InitializeWeapons();

            // Equip for scout
            controller.EquipFromStorage("scout");
            Assert.AreEqual("Gatling Gun", controller.CurrentWeapon.WeaponName);

            // Equip for fighter
            controller.EquipFromStorage("fighter");
            Assert.AreEqual("Scattergun", controller.CurrentWeapon.WeaponName);

            // Equip for juggernaut
            controller.EquipFromStorage("juggernaut");
            Assert.AreEqual("Beam Laser", controller.CurrentWeapon.WeaponName);
        }
    }
}
