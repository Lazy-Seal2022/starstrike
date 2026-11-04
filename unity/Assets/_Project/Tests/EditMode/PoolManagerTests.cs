using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class PoolManagerTests
    {
        private GameObject poolGo;
        private PoolManager pool;

        [SetUp]
        public void SetUp()
        {
            poolGo = new GameObject("TestPoolManager");
            pool = poolGo.AddComponent<PoolManager>();
            pool.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            if (poolGo != null)
            {
                Object.DestroyImmediate(poolGo);
            }
        }

        [Test]
        public void Spawn_InstantiatesNewObjectWhenPoolEmpty()
        {
            int factoryCalls = 0;
            GameObject spawned = pool.Spawn("TestTag", Vector3.zero, Quaternion.identity, () =>
            {
                factoryCalls++;
                return new GameObject("PooledObject_1");
            });

            Assert.IsNotNull(spawned);
            Assert.AreEqual(1, factoryCalls);
            Assert.IsTrue(spawned.activeInHierarchy);
        }

        [Test]
        public void Spawn_ReusesDeactivatedObjectInsteadOfCreatingNew()
        {
            int factoryCalls = 0;
            GameObject first = pool.Spawn("LaserPool", Vector3.zero, Quaternion.identity, () =>
            {
                factoryCalls++;
                return new GameObject("LaserObj");
            });

            first.SetActive(false);

            GameObject second = pool.Spawn("LaserPool", new Vector3(10, 0, 0), Quaternion.identity, () =>
            {
                factoryCalls++;
                return new GameObject("LaserObj_New");
            });

            Assert.AreSame(first, second, "PoolManager must reuse the deactivated GameObject");
            Assert.AreEqual(1, factoryCalls, "Factory should not be invoked when an inactive pooled object is available");
            Assert.IsTrue(second.activeInHierarchy);
            Assert.AreEqual(new Vector3(10, 0, 0), second.transform.position);
        }
    }
}
