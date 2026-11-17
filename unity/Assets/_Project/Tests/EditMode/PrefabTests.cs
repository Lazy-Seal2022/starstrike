using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;
using StarStrike.Core;

namespace StarStrike.Tests.EditMode
{
    public class PrefabTests
    {
        [Test]
        public void ProjectilePrefabs_PlayerAndEnemyLasers_ExistWithExpectedComponents()
        {
            var playerLaser = TestAssetHelper.LoadPrefab("Projectiles/PlayerLaser");
            Assert.IsNotNull(playerLaser, "PlayerLaser prefab must exist in Prefabs/Projectiles");
            Assert.AreEqual("Laser", playerLaser.tag);
            Assert.IsNotNull(playerLaser.GetComponent<LaserProjectile>());
            Assert.IsNotNull(playerLaser.GetComponent<Rigidbody2D>());
            Assert.IsTrue(playerLaser.GetComponent<CircleCollider2D>().isTrigger);
            Assert.IsNotNull(playerLaser.GetComponent<TrailRenderer>());

            var enemyLaser = TestAssetHelper.LoadPrefab("Projectiles/EnemyLaser");
            Assert.IsNotNull(enemyLaser, "EnemyLaser prefab must exist in Prefabs/Projectiles");
            Assert.AreEqual("EnemyLaser", enemyLaser.tag);
            var proj = enemyLaser.GetComponent<LaserProjectile>();
            Assert.IsNotNull(proj);
            Assert.IsTrue(proj.isEnemy);
        }

        [Test]
        public void AsteroidPrefabs_AllThreeTiers_ExistWithExpectedComponents()
        {
            var large = TestAssetHelper.LoadPrefab("Environment/Asteroid_Large");
            var medium = TestAssetHelper.LoadPrefab("Environment/Asteroid_Medium");
            var small = TestAssetHelper.LoadPrefab("Environment/Asteroid_Small");

            Assert.IsNotNull(large, "Asteroid_Large prefab must exist");
            Assert.IsNotNull(medium, "Asteroid_Medium prefab must exist");
            Assert.IsNotNull(small, "Asteroid_Small prefab must exist");

            Assert.AreEqual("Asteroid", large.tag);
            Assert.AreEqual("Asteroid", medium.tag);
            Assert.AreEqual("Asteroid", small.tag);

            Assert.AreEqual(AsteroidTier.Large, large.GetComponent<Asteroid>().tier);
            Assert.AreEqual(AsteroidTier.Medium, medium.GetComponent<Asteroid>().tier);
            Assert.AreEqual(AsteroidTier.Small, small.GetComponent<Asteroid>().tier);
        }

        [Test]
        public void LootPrefabs_GemAndPowerUp_ExistWithTriggers()
        {
            var gem = TestAssetHelper.LoadPrefab("Environment/GemCrystal");
            Assert.IsNotNull(gem, "GemCrystal prefab must exist");
            Assert.AreEqual("Gem", gem.tag);
            Assert.IsNotNull(gem.GetComponent<GemCrystal>());
            Assert.IsTrue(gem.GetComponent<CircleCollider2D>().isTrigger);

            var powerUp = TestAssetHelper.LoadPrefab("Environment/PowerUpItem");
            Assert.IsNotNull(powerUp, "PowerUpItem prefab must exist");
            Assert.AreEqual("PowerUp", powerUp.tag);
            Assert.IsNotNull(powerUp.GetComponent<PowerUpItem>());
            Assert.IsTrue(powerUp.GetComponent<CircleCollider2D>().isTrigger);
        }

        [Test]
        public void EnemyPrefabs_DroneAndBoss_ExistWithAIAndCombatComponents()
        {
            var drone = TestAssetHelper.LoadPrefab("Enemies/EnemyDrone");
            Assert.IsNotNull(drone, "EnemyDrone prefab must exist");
            Assert.AreEqual("Enemy", drone.tag);
            Assert.IsNotNull(drone.GetComponent<EnemyAI>());
            Assert.IsNotNull(drone.GetComponent<ShipHealth>());
            Assert.IsNotNull(drone.GetComponent<ShipWeapon>());

            var boss = TestAssetHelper.LoadPrefab("Enemies/BossDreadnought");
            Assert.IsNotNull(boss, "BossDreadnought prefab must exist");
            Assert.AreEqual("Boss", boss.tag);
            Assert.IsNotNull(boss.GetComponent<BossBrain>());
            Assert.AreEqual(2500f, boss.GetComponent<ShipHealth>().maxShield);
        }

        [Test]
        public void ShipPrefabs_AllSixShips_ExistWithMuzzlesAndProgression()
        {
            string[] ships = { "Ship_Scout", "Ship_Fighter", "Ship_Miner", "Ship_Interceptor", "Ship_Juggernaut", "Ship_Carrier" };

            foreach (var shipName in ships)
            {
                var shipPrefab = TestAssetHelper.LoadPrefab($"Ships/{shipName}");
                Assert.IsNotNull(shipPrefab, $"{shipName} prefab must exist in Prefabs/Ships");
                Assert.AreEqual("Player", shipPrefab.tag);
                Assert.IsNotNull(shipPrefab.GetComponent<ShipMovement>(), $"{shipName} missing ShipMovement");
                Assert.IsNotNull(shipPrefab.GetComponent<ShipHealth>(), $"{shipName} missing ShipHealth");
                Assert.IsNotNull(shipPrefab.GetComponent<ShipWeapon>(), $"{shipName} missing ShipWeapon");
                Assert.IsNotNull(shipPrefab.GetComponent<PlayerProgression>(), $"{shipName} missing PlayerProgression");

                var leftMuzzle = shipPrefab.transform.Find("LeftMuzzle");
                var rightMuzzle = shipPrefab.transform.Find("RightMuzzle");
                Assert.IsNotNull(leftMuzzle, $"{shipName} missing LeftMuzzle child transform");
                Assert.IsNotNull(rightMuzzle, $"{shipName} missing RightMuzzle child transform");
            }
        }
    }
}
