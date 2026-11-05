using NUnit.Framework;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Network;

namespace StarStrike.Tests.EditMode
{
    [TestFixture]
    public class PvPArenaTests
    {
        private GameObject testObject;

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        [Test]
        public void GameMode_EnumValues_ExistAndDefaultIsPvE()
        {
            Assert.AreEqual(0, (int)GameMode.PvE);
            Assert.AreEqual(1, (int)GameMode.PvPArena);

            testObject = new GameObject("GameManager_Test");
            var gm = testObject.AddComponent<GameManager>();

            Assert.AreEqual(GameMode.PvE, gm.CurrentMode);

            gm.SetGameMode(GameMode.PvPArena);
            Assert.AreEqual(GameMode.PvPArena, gm.CurrentMode);
        }

        [Test]
        public void ArenaScoreEntry_FieldsAndEquality_WorkCorrectly()
        {
            var entry1 = new ArenaScoreEntry
            {
                ClientId = 1,
                PlayerName = new FixedString32Bytes("Viper"),
                ShipId = new FixedString32Bytes("interceptor"),
                Kills = 5,
                Deaths = 2,
                Score = 500
            };

            var entry2 = new ArenaScoreEntry
            {
                ClientId = 1,
                PlayerName = new FixedString32Bytes("ViperUpdated"),
                ShipId = new FixedString32Bytes("interceptor"),
                Kills = 6,
                Deaths = 2,
                Score = 600
            };

            var entry3 = new ArenaScoreEntry
            {
                ClientId = 2,
                PlayerName = new FixedString32Bytes("Ace"),
                ShipId = new FixedString32Bytes("fighter"),
                Kills = 1,
                Deaths = 4,
                Score = 100
            };

            Assert.AreEqual(entry1, entry2, "Entries with the same ClientId should be equal.");
            Assert.AreNotEqual(entry1, entry3, "Entries with different ClientId should not be equal.");
            Assert.AreEqual(entry1.GetHashCode(), entry2.GetHashCode());
        }

        [Test]
        public void NetworkArenaManager_SpawnMath_DistributesEquallyAroundRadius()
        {
            testObject = new GameObject("ArenaManager_Test");
            var arenaMgr = testObject.AddComponent<NetworkArenaManager>();

            float radius = 35f;
            int totalSlots = 8;

            Vector2 p0 = arenaMgr.GetArenaSpawnPoint(0, totalSlots, radius);
            Vector2 p2 = arenaMgr.GetArenaSpawnPoint(2, totalSlots, radius);
            Vector2 p4 = arenaMgr.GetArenaSpawnPoint(4, totalSlots, radius);

            // Radius distance check
            Assert.AreEqual(radius, p0.magnitude, 0.01f);
            Assert.AreEqual(radius, p2.magnitude, 0.01f);
            Assert.AreEqual(radius, p4.magnitude, 0.01f);

            // Angle check: slot 0 is (r, 0), slot 2 is 90 deg (0, r), slot 4 is 180 deg (-r, 0)
            Assert.AreEqual(radius, p0.x, 0.05f);
            Assert.AreEqual(0f, p0.y, 0.05f);

            Assert.AreEqual(0f, p2.x, 0.05f);
            Assert.AreEqual(radius, p2.y, 0.05f);

            Assert.AreEqual(-radius, p4.x, 0.05f);
            Assert.AreEqual(0f, p4.y, 0.05f);

            // No two slots overlap
            Assert.Greater(Vector2.Distance(p0, p2), 1f);
            Assert.Greater(Vector2.Distance(p2, p4), 1f);
        }

        [Test]
        public void NetworkPlayerShip_ComponentSetup_HasRequiredPhysicsAndRenderer()
        {
            testObject = new GameObject("NetworkShip_Test");
            var ship = testObject.AddComponent<NetworkPlayerShip>();

            var rb = testObject.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb);
            Assert.AreEqual(0f, rb.gravityScale);
            Assert.AreEqual(0.5f, rb.linearDamping);

            var col = testObject.GetComponent<CircleCollider2D>();
            Assert.IsNotNull(col);
            Assert.IsFalse(col.isTrigger);

            var sr = testObject.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(sr);
            Assert.AreEqual(10, sr.sortingOrder);

            var netObj = testObject.GetComponent<NetworkObject>();
            Assert.IsNotNull(netObj);
        }

        [Test]
        public void NetworkLaserProjectile_ComponentSetup_HasTriggerAndContinuousPhysics()
        {
            testObject = new GameObject("NetworkLaser_Test");
            var laser = testObject.AddComponent<NetworkLaserProjectile>();

            var rb = testObject.GetComponent<Rigidbody2D>();
            Assert.IsNotNull(rb);
            Assert.AreEqual(0f, rb.gravityScale);
            Assert.AreEqual(CollisionDetectionMode2D.Continuous, rb.collisionDetectionMode);

            var col = testObject.GetComponent<CircleCollider2D>();
            Assert.IsNotNull(col);
            Assert.IsTrue(col.isTrigger);

            var netObj = testObject.GetComponent<NetworkObject>();
            Assert.IsNotNull(netObj);
        }
    }
}
