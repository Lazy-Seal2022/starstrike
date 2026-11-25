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
        public void ContinuousBeamWeapon_LocksWithin20DegCone_AimsAtObjective_AndPrioritizesEnemiesOverAsteroids()
        {
            var beamWeapon = playerGo.AddComponent<ContinuousBeamWeapon>();
            beamWeapon.damagePerSecond = 100f;
            beamWeapon.energyCostPerSecond = 10f;
            beamWeapon.maxRange = 20f;
            beamWeapon.aimConeHalfAngle = 20f;

            // Asteroid dead ahead (0 degrees, closer)
            var asteroidGo = new GameObject("BlockingConeAsteroid");
            asteroidGo.tag = "Asteroid";
            asteroidGo.transform.position = new Vector3(4f, 0f, 0f);
            var astCol = asteroidGo.AddComponent<CircleCollider2D>();
            astCol.radius = 0.6f;
            var astComp = asteroidGo.AddComponent<Asteroid>();

            // Enemy inside 20-degree cone (at ~14 degrees off right axis)
            float radIn = 14f * Mathf.Deg2Rad;
            Vector3 enemyInPos = new Vector3(Mathf.Cos(radIn), Mathf.Sin(radIn), 0f) * 8f;
            var enemyInGo = new GameObject("ConeEnemy");
            enemyInGo.tag = "Enemy";
            enemyInGo.transform.position = enemyInPos;
            var enemyInHealth = enemyInGo.AddComponent<ShipHealth>();
            enemyInHealth.maxShield = 100f;
            enemyInHealth.currentShield = 100f;
            enemyInHealth.maxHull = 100f;
            enemyInHealth.currentHull = 100f;
            enemyInGo.AddComponent<CircleCollider2D>().radius = 0.6f;

            // Enemy outside 20-degree cone (~35 degrees)
            float radOut = 35f * Mathf.Deg2Rad;
            Vector3 enemyOutPos = new Vector3(Mathf.Cos(radOut), Mathf.Sin(radOut), 0f) * 6f;
            var enemyOutGo = new GameObject("OutOfConeEnemy");
            enemyOutGo.tag = "Enemy";
            enemyOutGo.transform.position = enemyOutPos;
            var enemyOutHealth = enemyOutGo.AddComponent<ShipHealth>();
            enemyOutHealth.maxShield = 100f;
            enemyOutHealth.currentShield = 100f;
            enemyOutHealth.maxHull = 100f;
            enemyOutHealth.currentHull = 100f;
            enemyOutGo.AddComponent<CircleCollider2D>().radius = 0.6f;

            try
            {
                Physics2D.SyncTransforms();

                bool fired = beamWeapon.TryFire((IEnergyConsumer)health);
                Assert.IsTrue(fired);

                Assert.Less(enemyInHealth.currentShield, 100f, "Beam must lock onto and damage the in-cone enemy despite asteroid dead ahead");
                Assert.AreEqual(100f, enemyOutHealth.currentShield, 0.01f, "Enemy outside 20 deg cone must remain undamaged");
                Assert.IsNotNull(beamWeapon.beamRenderer);
                Vector3 endPos = beamWeapon.beamRenderer.GetPosition(1);
                Vector3 expectedDir = (enemyInPos - beamWeapon.beamRenderer.GetPosition(0)).normalized;
                Vector3 actualDir = (endPos - beamWeapon.beamRenderer.GetPosition(0)).normalized;
                Assert.Less(Vector2.Angle(expectedDir, actualDir), 2f, "Beam endpoint direction must aim directly at locked objective");
            }
            finally
            {
                beamWeapon.StopFiring();
                Object.DestroyImmediate(asteroidGo);
                Object.DestroyImmediate(enemyInGo);
                Object.DestroyImmediate(enemyOutGo);
            }
        }

        [Test]
        public void ContinuousBeamWeapon_LocksCloseForwardEnemy_EvenWhenOutside20DegMuzzleCone()
        {
            var beamWeapon = playerGo.AddComponent<ContinuousBeamWeapon>();
            beamWeapon.damagePerSecond = 100f;
            beamWeapon.energyCostPerSecond = 10f;
            beamWeapon.maxRange = 20f;
            beamWeapon.aimConeHalfAngle = 20f;
            beamWeapon.closeRangeLockRadius = 3.5f;

            // Enemy is right in front/side of the ship (55 deg off ship center, behind default muzzle x=1.55)
            Vector3 closeForwardPos = new Vector3(1.25f, 1.80f, 0f);
            var closeEnemyGo = new GameObject("CloseForwardEnemy");
            closeEnemyGo.tag = "Enemy";
            closeEnemyGo.transform.position = closeForwardPos;
            var closeHealth = closeEnemyGo.AddComponent<ShipHealth>();
            closeHealth.maxShield = 100f;
            closeHealth.currentShield = 100f;
            closeHealth.maxHull = 100f;
            closeHealth.currentHull = 100f;
            closeEnemyGo.AddComponent<CircleCollider2D>().radius = 0.6f;

            // Enemy behind the ship must NOT be shot
            var rearEnemyGo = new GameObject("RearEnemy");
            rearEnemyGo.tag = "Enemy";
            rearEnemyGo.transform.position = new Vector3(-1.5f, 0.5f, 0f);
            var rearHealth = rearEnemyGo.AddComponent<ShipHealth>();
            rearHealth.maxShield = 100f;
            rearHealth.currentShield = 100f;
            rearHealth.maxHull = 100f;
            rearHealth.currentHull = 100f;
            rearEnemyGo.AddComponent<CircleCollider2D>().radius = 0.6f;

            try
            {
                Physics2D.SyncTransforms();

                bool fired = beamWeapon.TryFire((IEnergyConsumer)health);
                Assert.IsTrue(fired);

                Assert.Less(closeHealth.currentShield, 100f, "Close enemy in front of ship must be targeted even outside 20 deg cone");
                Assert.AreEqual(100f, rearHealth.currentShield, 0.01f, "Enemy behind ship must not be targeted");
            }
            finally
            {
                beamWeapon.StopFiring();
                Object.DestroyImmediate(closeEnemyGo);
                Object.DestroyImmediate(rearEnemyGo);
            }
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

        [Test]
        public void ShipWeaponController_MultiSocketLoadout_EquipsAndFiresDedicatedHardpoints()
        {
            StarStrike.Platform.Storage.StorageService.SaveSocketWeapon("fighter", 0, "pulse");
            StarStrike.Platform.Storage.StorageService.SaveSocketWeapon("fighter", 1, "gatling");

            var controller = playerGo.AddComponent<ShipWeaponController>();
            controller.InitializeWeapons();
            controller.EquipFromStorage("fighter");

            Assert.IsTrue(controller.HasMultiSocketLoadout, "Controller must report active multi-socket loadout");
            Assert.AreEqual(2, controller.MountedHardpoints.Count, "Fighter must mount exactly 2 hardpoint sockets");

            Assert.AreEqual("pulse", controller.MountedHardpoints[0].weaponId);
            Assert.AreEqual("gatling", controller.MountedHardpoints[1].weaponId);

            Assert.AreEqual(new Vector2(0.35f, 1.25f), controller.MountedHardpoints[0].localPosition);
            Assert.AreEqual(new Vector2(0.35f, -1.25f), controller.MountedHardpoints[1].localPosition);

            // Primary weapon matches socket 0
            Assert.AreEqual("Pulse Laser", controller.CurrentWeapon.WeaponName);

            // Firing sockets coordinates independent weapon discharge
            bool fired = controller.TryFireSockets(health);
            Assert.IsTrue(fired, "Multi-socket firing must succeed via TryFireSockets");

            // Fastest fire rate is governed by Gatling Gun (~0.10s cold down to 0.055s spooled)
            float fastestRate = controller.GetFastestFireRate();
            Assert.LessOrEqual(fastestRate, 0.12f, "Fastest fire rate must reflect the rotary autocannon");
        }
    }
}
