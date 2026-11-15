using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    [TestFixture]
    public class Phase2TacticalTests
    {
        private GameObject playerGo;
        private GameObject enemyGo;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("Player");
            playerGo.tag = "Player";
            playerGo.AddComponent<CircleCollider2D>();
            var pHealth = playerGo.AddComponent<ShipHealth>();
            pHealth.maxShield = 100f;
            pHealth.ResetMeters();
            playerGo.AddComponent<ShipMovement>();
            playerGo.AddComponent<ShipWeapon>();

            enemyGo = new GameObject("Enemy");
            enemyGo.tag = "Enemy";
            enemyGo.AddComponent<CircleCollider2D>();
            var eHealth = enemyGo.AddComponent<ShipHealth>();
            eHealth.maxShield = 60f;
            eHealth.ResetMeters();
            enemyGo.AddComponent<ShipMovement>();
            enemyGo.AddComponent<ShipWeapon>();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (enemyGo != null) Object.DestroyImmediate(enemyGo);
        }

        [Test]
        public void Kamikaze_Configuration_And_Detonation_Radius_Damage()
        {
            var ai = enemyGo.AddComponent<EnemyAI>();
            ai.ConfigureArchetype(EnemyArchetype.Kamikaze);

            Assert.AreEqual(EnemyArchetype.Kamikaze, ai.Archetype);
            Assert.AreEqual(4.5f, ai.definition.kamikazeBlastRadius);
            Assert.AreEqual(45f, ai.definition.kamikazeBlastDamage);

            // Target inside radius (distance = 2.0)
            var insideTarget = new GameObject("InsideTarget");
            insideTarget.transform.position = new Vector3(2f, 0f, 0f);
            var inCol = insideTarget.AddComponent<CircleCollider2D>();
            inCol.radius = 0.5f;
            var inHealth = insideTarget.AddComponent<ShipHealth>();
            inHealth.maxShield = 100f;
            inHealth.ResetMeters();

            // Target outside radius (distance = 8.0)
            var outsideTarget = new GameObject("OutsideTarget");
            outsideTarget.transform.position = new Vector3(8f, 0f, 0f);
            var outCol = outsideTarget.AddComponent<CircleCollider2D>();
            outCol.radius = 0.5f;
            var outHealth = outsideTarget.AddComponent<ShipHealth>();
            outHealth.maxShield = 100f;
            outHealth.ResetMeters();

            Physics2D.SyncTransforms();

            // Trigger death blast
            var enemyHealth = enemyGo.GetComponent<ShipHealth>();
            enemyHealth.TakeDamage(100f);

            Assert.Less(inHealth.currentShield, 100f, "Target inside blast radius should take damage");
            Assert.AreEqual(100f, outHealth.currentShield, 0.01f, "Target outside blast radius should be unharmed");

            Object.DestroyImmediate(insideTarget);
            Object.DestroyImmediate(outsideTarget);
        }

        [Test]
        public void Sniper_Configuration_And_Aim_Stats()
        {
            var ai = enemyGo.AddComponent<EnemyAI>();
            ai.ConfigureArchetype(EnemyArchetype.Sniper);

            Assert.AreEqual(EnemyArchetype.Sniper, ai.Archetype);
            Assert.AreEqual(26f, ai.definition.sightRadius);
            Assert.AreEqual(18f, ai.definition.attackRadius);
            Assert.AreEqual(1.2f, ai.definition.sniperChargeTime);
            Assert.AreEqual(35f, ai.definition.pulseDamage);
            Assert.AreEqual(36f, ai.definition.pulseSpeed);
        }

        [Test]
        public void SupportShield_Configuration_And_Ally_Invulnerability()
        {
            var supportAI = enemyGo.AddComponent<EnemyAI>();
            supportAI.ConfigureArchetype(EnemyArchetype.SupportShield);

            Assert.AreEqual(EnemyArchetype.SupportShield, supportAI.Archetype);
            Assert.AreEqual(5.5f, supportAI.definition.supportShieldRadius);

            // Ally enemy inside aura (distance = 2.0)
            var allyGo = new GameObject("AllyEnemy");
            allyGo.tag = "Enemy";
            allyGo.transform.position = new Vector3(2f, 0f, 0f);
            var allyCol = allyGo.AddComponent<CircleCollider2D>();
            allyCol.radius = 0.5f;
            var allyHealth = allyGo.AddComponent<ShipHealth>();
            allyHealth.maxShield = 80f;
            allyHealth.ResetMeters();
            var allyAI = allyGo.AddComponent<EnemyAI>();
            allyAI.ConfigureArchetype(EnemyArchetype.Drone);

            Physics2D.SyncTransforms();

            // Call aura method via reflection
            var auraMethod = typeof(EnemyAI).GetMethod("ApplySupportShieldAura", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(auraMethod);
            auraMethod.Invoke(supportAI, null);

            Assert.IsTrue(allyHealth.isInvulnerable, "Ally within Support Shield aura must be invulnerable");

            float initialShield = allyHealth.currentShield;
            // Attempt damage while invulnerable
            allyHealth.TakeDamage(30f);
            Assert.AreEqual(initialShield, allyHealth.currentShield, "Invulnerable ally must take zero shield damage");

            // Destroy Support Shield frigate; verify ally is no longer invulnerable
            var releaseMethod = typeof(EnemyAI).GetMethod("ReleaseShieldedAllies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(releaseMethod);
            releaseMethod.Invoke(supportAI, null);

            Assert.IsFalse(allyHealth.isInvulnerable, "Ally must lose invulnerability once support ship is destroyed/released");

            Object.DestroyImmediate(allyGo);
        }

        [Test]
        public void SectorHazardController_Biome_Switching_And_Drift_Damping()
        {
            var hazardGo = new GameObject("HazardCtrl");
            var hazard = hazardGo.AddComponent<SectorHazardController>();

            hazard.SetSector(SectorHazardController.SectorBiome.AsteroidBelt);
            Assert.AreEqual(SectorHazardController.SectorBiome.AsteroidBelt, hazard.CurrentBiome);

            hazard.SetSector(SectorHazardController.SectorBiome.IonNebula);
            Assert.AreEqual(SectorHazardController.SectorBiome.IonNebula, hazard.CurrentBiome);

            hazard.SetSector(SectorHazardController.SectorBiome.SolarFlare);
            Assert.AreEqual(SectorHazardController.SectorBiome.SolarFlare, hazard.CurrentBiome);

            var movement = playerGo.GetComponent<ShipMovement>();
            Assert.IsTrue(movement.autoBrakeEnabled);

            // Simulate Ion Storm surge disabling autobrake
            movement.autoBrakeEnabled = false;
            Assert.IsFalse(movement.autoBrakeEnabled);

            hazard.ResetAllHazards();
            Object.DestroyImmediate(hazardGo);
        }

        [Test]
        public void SectorHazardController_SolarFlare_Asteroid_Occlusion()
        {
            var hazardGo = new GameObject("HazardCtrl");
            var hazard = hazardGo.AddComponent<SectorHazardController>();

            Vector2 flareDir = hazard.flareDirection.normalized;

            // In open space, position is not sheltered
            Assert.IsFalse(hazard.IsPositionSheltered(Vector2.zero));

            // Place an asteroid directly between flare source and origin
            var astGo = new GameObject("ShelterAsteroid");
            astGo.transform.position = (Vector3)(-flareDir * 6f);
            var col = astGo.AddComponent<CircleCollider2D>();
            col.radius = 2.0f;
            astGo.AddComponent<Asteroid>();

            Physics2D.SyncTransforms();

            Assert.IsTrue(hazard.IsPositionSheltered(Vector2.zero), "Position behind asteroid must be sheltered from incoming solar flare");

            Object.DestroyImmediate(astGo);
            Object.DestroyImmediate(hazardGo);
        }

        [Test]
        public void AlternateSkills_BlinkTeleport_Execution_And_IFrames()
        {
            var rb = playerGo.GetComponent<Rigidbody2D>();
            var health = playerGo.GetComponent<ShipHealth>();

            var blink = new BlinkTeleportBehavior();
            blink.Initialize(playerGo, null);

            Vector2 initialPos = playerGo.transform.position;
            blink.Execute(playerGo, initialPos, Vector2.right);

            Assert.Greater(playerGo.transform.position.x, 7.5f, "Blink should teleport vessel forward ~8.5 units");
            Assert.IsTrue(health.isInvulnerable, "Blink must grant temporary i-frames");

            blink.Terminate();
        }

        [Test]
        public void AlternateSkills_EMPNova_Damage_And_Stun()
        {
            var emp = new EMPNovaBehavior();
            emp.Initialize(playerGo, null);

            var enemyAI = enemyGo.AddComponent<EnemyAI>();
            var enemyHealth = enemyGo.GetComponent<ShipHealth>();
            enemyGo.transform.position = new Vector3(4f, 0f, 0f);

            Physics2D.SyncTransforms();

            emp.Execute(playerGo, playerGo.transform.position, Vector2.zero);

            Assert.Less(enemyHealth.currentShield, 60f, "EMP Nova must deal radial damage to enemies within 9.5 units");

            emp.Terminate();
        }

        [Test]
        public void PlayableHulls_Vanguard_And_Phantom_Trees_Configured()
        {
            var scout = PlayerProgression.CreateDefaultShipTree();
            Assert.IsNotNull(scout);
            Assert.AreEqual("scout", scout.id);
            Assert.AreEqual(2, scout.evolvesTo.Length);

            var fighter = scout.evolvesTo[0];
            Assert.AreEqual("fighter", fighter.id);
            Assert.AreEqual(2, fighter.evolvesTo.Length);

            bool foundPhantom = false;
            foreach (var ship in fighter.evolvesTo)
            {
                if (ship.id == "phantom")
                {
                    foundPhantom = true;
                    Assert.AreEqual("Shadow Phantom", ship.shipName);
                    Assert.AreEqual(3, ship.tier);
                    Assert.AreEqual(420, ship.baseStats.speed);
                }
            }
            Assert.IsTrue(foundPhantom, "Fighter must evolve to Shadow Phantom");

            var miner = scout.evolvesTo[1];
            Assert.AreEqual("miner", miner.id);
            Assert.AreEqual(2, miner.evolvesTo.Length);

            bool foundVanguard = false;
            foreach (var ship in miner.evolvesTo)
            {
                if (ship.id == "vanguard")
                {
                    foundVanguard = true;
                    Assert.AreEqual("Vanguard Carrier", ship.shipName);
                    Assert.AreEqual(3, ship.tier);
                    Assert.AreEqual(320, ship.baseStats.shieldCapacity);
                }
            }
            Assert.IsTrue(foundVanguard, "Miner must evolve to Vanguard Carrier");
        }
    }
}
