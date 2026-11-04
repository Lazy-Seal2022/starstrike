using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class PowerUpTests
    {
        private GameObject playerGo;
        private GameObject powerUpGo;
        private ShipHealth health;
        private ShipWeapon weapon;
        private PowerUpItem powerUp;

        [SetUp]
        public void SetUp()
        {
            playerGo = new GameObject("TestPlayer");
            playerGo.AddComponent<Rigidbody2D>();
            health = playerGo.AddComponent<ShipHealth>();
            health.maxShield = 100f;
            health.ResetMeters();

            weapon = playerGo.AddComponent<ShipWeapon>();
            weapon.weaponSlots = 1;
            weapon.fireRate = 0.2f;

            powerUpGo = new GameObject("TestPowerUp");
            powerUp = powerUpGo.AddComponent<PowerUpItem>();
        }

        [TearDown]
        public void TearDown()
        {
            if (playerGo != null) Object.DestroyImmediate(playerGo);
            if (powerUpGo != null) Object.DestroyImmediate(powerUpGo);
        }

        [Test]
        public void ApplyPowerUp_ShieldOvercharge_IncreasesMaxShieldAndFullyHeals()
        {
            health.TakeDamage(60f); // 40 shield left
            Assert.AreEqual(40f, health.currentShield);

            powerUp.type = PowerUpItem.PowerUpType.ShieldOvercharge;
            powerUp.ApplyPowerUp(playerGo);

            Assert.AreEqual(200f, health.maxShield, "Shield capacity should increase by 100");
            Assert.AreEqual(200f, health.currentShield, "Shield should be fully restored to new capacity");
        }

        [Test]
        public void ApplyPowerUp_WeaponOverdrive_IncreasesSlotsAndFireRate()
        {
            powerUp.type = PowerUpItem.PowerUpType.WeaponOverdrive;
            powerUp.ApplyPowerUp(playerGo);

            Assert.AreEqual(2, weapon.weaponSlots, "Weapon slots should increment");
            Assert.AreEqual(0.16f, weapon.fireRate, 0.001f, "Fire rate cooldown should reduce by 20%");
        }
    }
}
