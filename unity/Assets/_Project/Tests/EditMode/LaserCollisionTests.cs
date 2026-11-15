using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class LaserCollisionTests
    {
        [Test]
        public void PlayerLaser_OnTriggerEnter2D_DamagesEnemyAndDeactivates()
        {
            GameObject enemyGo = new GameObject("TestEnemy");
            enemyGo.tag = "Enemy";
            enemyGo.AddComponent<Rigidbody2D>();
            CircleCollider2D enemyCol = enemyGo.AddComponent<CircleCollider2D>();
            enemyCol.radius = 0.7f;
            ShipHealth enemyHealth = enemyGo.AddComponent<ShipHealth>();
            enemyHealth.maxShield = 100f;
            enemyHealth.ResetMeters();

            GameObject laserGo = new GameObject("PlayerLaser");
            laserGo.tag = "Laser";
            laserGo.AddComponent<Rigidbody2D>();
            laserGo.AddComponent<CircleCollider2D>();
            LaserProjectile laser = laserGo.AddComponent<LaserProjectile>();
            laser.Initialize(25f, 20f, enemy: false);

            laser.OnTriggerEnter2D(enemyCol);

            Assert.AreEqual(75f, enemyHealth.currentShield, 0.01f, "Laser damage must be deducted from enemy shield");
            Assert.IsFalse(laserGo.activeSelf, "Laser must deactivate upon hitting enemy");

            Object.DestroyImmediate(enemyGo);
            Object.DestroyImmediate(laserGo);
        }

        [Test]
        public void EnemyLaser_OnTriggerEnter2D_DamagesPlayerAndDeactivates()
        {
            GameObject playerTarget = new GameObject("TestPlayerTarget");
            playerTarget.tag = "Player";
            playerTarget.AddComponent<Rigidbody2D>();
            CircleCollider2D playerCol = playerTarget.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.5f;
            ShipHealth targetHealth = playerTarget.AddComponent<ShipHealth>();
            targetHealth.maxShield = 100f;
            targetHealth.ResetMeters();

            GameObject laserGo = new GameObject("EnemyLaser");
            laserGo.tag = "EnemyLaser";
            laserGo.AddComponent<Rigidbody2D>();
            laserGo.AddComponent<CircleCollider2D>();
            LaserProjectile laser = laserGo.AddComponent<LaserProjectile>();
            laser.Initialize(15f, 18f, enemy: true);

            laser.OnTriggerEnter2D(playerCol);

            Assert.AreEqual(85f, targetHealth.currentShield, 0.01f, "Enemy laser must deduct shield from player");
            Assert.IsFalse(laserGo.activeSelf, "Enemy laser must deactivate upon hitting player");

            Object.DestroyImmediate(playerTarget);
            Object.DestroyImmediate(laserGo);
        }

        [Test]
        public void PlayerLaser_DoesNotDamageFriendlyPlayer()
        {
            GameObject friendlyPlayer = new GameObject("FriendlyPlayer");
            friendlyPlayer.tag = "Player";
            friendlyPlayer.AddComponent<Rigidbody2D>();
            CircleCollider2D playerCol = friendlyPlayer.AddComponent<CircleCollider2D>();
            ShipHealth pHealth = friendlyPlayer.AddComponent<ShipHealth>();
            pHealth.maxShield = 100f;
            pHealth.ResetMeters();

            GameObject laserGo = new GameObject("PlayerLaser");
            laserGo.tag = "Laser";
            LaserProjectile laser = laserGo.AddComponent<LaserProjectile>();
            laser.Initialize(25f, 20f, enemy: false);

            laser.OnTriggerEnter2D(playerCol);

            Assert.AreEqual(100f, pHealth.currentShield, "Player laser must never damage player");
            Assert.IsTrue(laserGo.activeSelf, "Laser must not be consumed by hitting friendly player");

            Object.DestroyImmediate(friendlyPlayer);
            Object.DestroyImmediate(laserGo);
        }

        [Test]
        public void ProjectileOrientation_WhenAimedVertically_IsVerticalAlongFlightPath()
        {
            GameObject laserUp = new GameObject("LaserUp");
            laserUp.transform.rotation = Quaternion.Euler(0, 0, 90f);
            var laser = laserUp.AddComponent<LaserProjectile>();
            laser.Initialize(20f, 25f, enemy: false);

            var rb = laserUp.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb);
            Assert.Greater(rb.linearVelocity.y, 20f, "Bullet aimed vertically must move along +Y");
            Assert.AreEqual(0f, rb.linearVelocity.x, 0.01f);

            var sr = laserUp.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(sr);
            Assert.Greater(sr.bounds.size.y, sr.bounds.size.x, "Bullet flying vertically must be visually oriented vertically");

            Object.DestroyImmediate(laserUp);
        }

        [Test]
        public void ProjectileOrientation_WhenAimedHorizontally_IsHorizontalAlongFlightPath()
        {
            GameObject laserRight = new GameObject("LaserRight");
            laserRight.transform.rotation = Quaternion.Euler(0, 0, 0f);
            var laser = laserRight.AddComponent<LaserProjectile>();
            laser.Initialize(20f, 25f, enemy: false);

            var rb = laserRight.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb);
            Assert.Greater(rb.linearVelocity.x, 20f, "Bullet aimed horizontally must move along +X");
            Assert.AreEqual(0f, rb.linearVelocity.y, 0.01f);

            var sr = laserRight.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(sr);
            Assert.Greater(sr.bounds.size.x, sr.bounds.size.y, "Bullet flying horizontally must be visually oriented horizontally");

            Object.DestroyImmediate(laserRight);
        }
    }
}
