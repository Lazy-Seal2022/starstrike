using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class ConfigurationTests
    {
        private GameObject testGo;

        [TearDown]
        public void TearDown()
        {
            if (testGo != null)
            {
                Object.DestroyImmediate(testGo);
            }
        }

        [Test]
        public void GameConfig_LoadsFromResources_WithValidSubConfigs()
        {
            var config = Resources.Load<GameConfig>("Data/GameConfig");
            Assert.IsNotNull(config, "GameConfig asset must exist in Resources/Data/GameConfig");
            Assert.IsNotNull(config.flightConfig, "FlightConfig must be attached to GameConfig");
            Assert.IsNotNull(config.combatConfig, "CombatConfig must be attached to GameConfig");
            Assert.IsNotNull(config.asteroidConfig, "AsteroidConfig must be attached to GameConfig");
            Assert.IsNotNull(config.bossConfig, "BossConfig must be attached to GameConfig");
            Assert.IsNotNull(config.lootConfig, "LootConfig must be attached to GameConfig");
            Assert.IsNotNull(config.upgradeDefinition, "UpgradeDefinition must be attached to GameConfig");
            Assert.IsNotNull(config.defaultShip, "DefaultShip must be attached to GameConfig");
        }

        [Test]
        public void FlightConfig_DefaultValues_AreSensible()
        {
            var config = Resources.Load<FlightConfig>("Data/FlightConfig");
            if (config == null) config = ScriptableObject.CreateInstance<FlightConfig>();

            Assert.Greater(config.defaultMaxSpeed, 0f);
            Assert.Greater(config.defaultAcceleration, 0f);
            Assert.Greater(config.defaultAgility, 0f);
            Assert.Greater(config.boostMultiplier, 1.0f);
            Assert.Greater(config.boostMaxSpeedMultiplier, 1.0f);
            Assert.Greater(config.boostEnergyDrain, 0f);
        }

        [Test]
        public void CombatConfig_DefaultValues_AreSensible()
        {
            var config = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (config == null) config = ScriptableObject.CreateInstance<CombatConfig>();

            Assert.Greater(config.defaultPulseDamage, 0f);
            Assert.Greater(config.defaultPulseSpeed, 0f);
            Assert.Greater(config.projectileLifetime, 0f);
            Assert.Greater(config.laserColliderRadius, 0f);
            Assert.Greater(config.muzzleForwardOffset, 0f);
        }

        [Test]
        public void AsteroidConfig_TierRetrieval_ReturnsExpectedStats()
        {
            var config = Resources.Load<AsteroidConfig>("Data/AsteroidConfig");
            if (config == null) config = ScriptableObject.CreateInstance<AsteroidConfig>();

            var large = config.GetTierData(AsteroidTier.Large);
            Assert.AreEqual(120f, large.maxHp);
            Assert.AreEqual(8, large.gemDropCount);

            var medium = config.GetTierData(AsteroidTier.Medium);
            Assert.AreEqual(50f, medium.maxHp);
            Assert.AreEqual(4, medium.gemDropCount);

            var small = config.GetTierData(AsteroidTier.Small);
            Assert.AreEqual(20f, small.maxHp);
            Assert.AreEqual(1, small.gemDropCount);
        }

        [Test]
        public void BossConfig_PhaseThresholds_AreDescending()
        {
            var config = Resources.Load<BossConfig>("Data/BossConfig");
            if (config == null) config = ScriptableObject.CreateInstance<BossConfig>();

            Assert.Greater(config.phase1Threshold, config.phase2Threshold);
            Assert.Greater(config.phase2Threshold, config.phase3Threshold);
            Assert.Greater(config.phase3Threshold, 0f);
            Assert.Greater(config.maxShield, 0f);
        }

        [Test]
        public void LootConfig_MagnetAndPowerUps_AreSensible()
        {
            var config = Resources.Load<LootConfig>("Data/LootConfig");
            if (config == null) config = ScriptableObject.CreateInstance<LootConfig>();

            Assert.Greater(config.magnetRadius, 0f);
            Assert.Greater(config.magnetSpeed, 0f);
            Assert.Greater(config.shieldOverchargeBonus, 0f);
            Assert.Greater(config.weaponOverdriveDamageMultiplier, 1.0f);
        }

        [Test]
        public void ShipMovement_FallbackAutoResolves_WhenConfigNotAssigned()
        {
            testGo = new GameObject("TestShip");
            var movement = testGo.AddComponent<ShipMovement>();
            movement.InitializePhysics();
            
            // Movement InitializePhysics handles fallback
            Assert.IsNotNull(movement.flightConfig, "ShipMovement must automatically resolve flightConfig");
        }

        [Test]
        public void Asteroid_FallbackAutoResolves_WhenConfigNotAssigned()
        {
            testGo = new GameObject("TestAsteroid");
            var asteroid = testGo.AddComponent<Asteroid>();
            asteroid.SetupStats();

            Assert.IsNotNull(asteroid.config, "Asteroid must automatically resolve AsteroidConfig");
        }
    }
}
