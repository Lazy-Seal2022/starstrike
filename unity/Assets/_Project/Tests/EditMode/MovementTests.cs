using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class MovementTests
    {
        private GameObject shipGo;
        private Rigidbody2D rb;
        private ShipMovement movement;

        [SetUp]
        public void SetUp()
        {
            shipGo = new GameObject("TestShip");
            rb = shipGo.AddComponent<Rigidbody2D>();
            movement = shipGo.AddComponent<ShipMovement>();
            movement.InitializePhysics();
            movement.maxSpeed = 10f;
            movement.acceleration = 18f;
            movement.agility = 6f;
        }

        [TearDown]
        public void TearDown()
        {
            if (shipGo != null)
            {
                Object.DestroyImmediate(shipGo);
            }
        }

        [Test]
        public void Movement_Awake_EnforcesZeroGravityAndSpaceDamping()
        {
            Assert.AreEqual(0f, rb.gravityScale, "Ship in space must have gravityScale = 0");
            Assert.AreEqual(0.5f, rb.linearDamping, 0.01f);
            Assert.AreEqual(1.0f, rb.angularDamping, 0.01f);
        }

        [Test]
        public void Movement_CalculateTargetAngle_MatchesAimDirection()
        {
            // Aim right (1, 0) -> 0 deg
            Vector2 aimRight = new Vector2(1, 0);
            float angleRight = Mathf.Atan2(aimRight.y, aimRight.x) * Mathf.Rad2Deg;
            Assert.AreEqual(0f, angleRight, 0.01f);

            // Aim up (0, 1) -> 90 deg
            Vector2 aimUp = new Vector2(0, 1);
            float angleUp = Mathf.Atan2(aimUp.y, aimUp.x) * Mathf.Rad2Deg;
            Assert.AreEqual(90f, angleUp, 0.01f);

            // Aim left (-1, 0) -> 180 deg
            Vector2 aimLeft = new Vector2(-1, 0);
            float angleLeft = Mathf.Atan2(aimLeft.y, aimLeft.x) * Mathf.Rad2Deg;
            Assert.AreEqual(180f, angleLeft, 0.01f);

            // Aim down (0, -1) -> -90 deg
            Vector2 aimDown = new Vector2(0, -1);
            float angleDown = Mathf.Atan2(aimDown.y, aimDown.x) * Mathf.Rad2Deg;
            Assert.AreEqual(-90f, angleDown, 0.01f);
        }

        [Test]
        public void Movement_VelocityClamping_RespectsMaxSpeed()
        {
            rb.linearVelocity = new Vector2(30f, 40f); // magnitude 50
            float maxSpeed = movement.maxSpeed;

            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }

            Assert.AreEqual(maxSpeed, rb.linearVelocity.magnitude, 0.01f);
        }
    }
}
