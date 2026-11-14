using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class ShipSkillControllerTests
    {
        private GameObject shipGo;
        private ShipHealth health;
        private Rigidbody2D rb;
        private ShipSkillController skillController;

        [SetUp]
        public void SetUp()
        {
            shipGo = new GameObject("TestShip");
            rb = shipGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;

            health = shipGo.AddComponent<ShipHealth>();
            health.maxShield = 100f;
            health.maxEnergy = 80f;
            health.ResetMeters();

            skillController = shipGo.AddComponent<ShipSkillController>();
            skillController.InitializeDefaultSlots();
        }

        [TearDown]
        public void TearDown()
        {
            if (shipGo != null)
            {
                Object.DestroyImmediate(shipGo);
            }

            var shockwaves = Object.FindObjectsByType<ShockwaveProjectile>(FindObjectsSortMode.None);
            foreach (var s in shockwaves)
            {
                Object.DestroyImmediate(s.gameObject);
            }
        }

        [Test]
        public void HasEnergy_QueriesEnergyStateAccurately()
        {
            health.currentEnergy = 50f;
            Assert.IsTrue(health.HasEnergy(20f));
            Assert.IsTrue(health.HasEnergy(50f));
            Assert.IsFalse(health.HasEnergy(51f));
        }

        [Test]
        public void TryCastActiveSkill_WhenReady_DeductsEnergyAndStartsCooldown()
        {
            health.currentEnergy = 80f;
            var slot = skillController.GetSlot(SkillSlotType.Active1);
            Assert.IsNotNull(slot);
            Assert.AreEqual(0f, slot.currentCooldown);

            bool success = skillController.TryCastActiveSkill(SkillSlotType.Active1);

            Assert.IsTrue(success);
            Assert.AreEqual(60f, health.currentEnergy, 0.01f);
            Assert.Greater(slot.currentCooldown, 0f);
        }

        [Test]
        public void TryCastActiveSkill_WhenOnCooldown_FailsAndBroadcastsEvent()
        {
            health.currentEnergy = 80f;
            skillController.TryCastActiveSkill(SkillSlotType.Active1);

            SkillBlockedEvent blockedEvt = default;
            bool eventFired = false;
            GameEvents.OnSkillBlocked += evt =>
            {
                blockedEvt = evt;
                eventFired = true;
            };

            bool secondCast = skillController.TryCastActiveSkill(SkillSlotType.Active1);

            Assert.IsFalse(secondCast);
            Assert.IsTrue(eventFired);
            Assert.AreEqual(SkillBlockReason.OnCooldown, blockedEvt.Reason);
        }

        [Test]
        public void TryCastActiveSkill_WhenInsufficientEnergy_FailsAndBroadcastsEvent()
        {
            health.currentEnergy = 10f; // Needs 20f

            SkillBlockedEvent blockedEvt = default;
            bool eventFired = false;
            GameEvents.OnSkillBlocked += evt =>
            {
                blockedEvt = evt;
                eventFired = true;
            };

            bool success = skillController.TryCastActiveSkill(SkillSlotType.Active1);

            Assert.IsFalse(success);
            Assert.IsTrue(eventFired);
            Assert.AreEqual(SkillBlockReason.InsufficientEnergy, blockedEvt.Reason);
            Assert.AreEqual(10f, health.currentEnergy, 0.01f);
        }

        [Test]
        public void TryCastActiveSkill_AppliesReverseRecoilImpulse()
        {
            health.currentEnergy = 80f;
            shipGo.transform.rotation = Quaternion.identity; // right is (1, 0)
            rb.linearVelocity = Vector2.zero;

            bool success = skillController.TryCastActiveSkill(SkillSlotType.Active1);

            Assert.IsTrue(success);
            Assert.Less(rb.linearVelocity.x, 0f, "Recoil should push ship backwards along its negative heading");
        }

        [Test]
        public void ReactiveBarrier_WhenShieldDropsBelowThreshold_TriggersSurge()
        {
            health.currentShield = 100f;
            health.maxShield = 100f;

            bool castEventFired = false;
            GameEvents.OnSkillCast += evt =>
            {
                if (evt.Slot == SkillSlotType.Passive1)
                {
                    castEventFired = true;
                }
            };

            // Drop shield to 20% (threshold is <= 25%)
            health.TakeDamage(80f);

            Assert.IsTrue(castEventFired, "Reactive barrier should trigger deflector surge on critical shield threshold");
        }
    }
}
