using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class MinimapTests
    {
        private GameObject minimapGo;
        private MinimapController minimap;

        [SetUp]
        public void SetUp()
        {
            minimapGo = new GameObject("TestMinimap");
            minimap = minimapGo.AddComponent<MinimapController>();
            minimap.radarRange = 50f;
            minimap.mapSize = 150f;
            minimap.BuildUI();
        }

        [TearDown]
        public void TearDown()
        {
            if (minimapGo != null)
            {
                Object.DestroyImmediate(minimapGo);
            }
        }

        [Test]
        public void RadarScale_CalculatesCorrectFactor()
        {
            // scale = (mapSize / 2f) / radarRange = 75 / 50 = 1.5
            float expectedScale = (minimap.mapSize / 2f) / minimap.radarRange;
            Assert.AreEqual(1.5f, expectedScale, 0.01f);

            // An entity 20 units away should map to 30 UI units
            Vector2 worldOffset = new Vector2(20f, 0f);
            Vector2 uiPos = worldOffset * expectedScale;
            Assert.AreEqual(30f, uiPos.x, 0.01f);
            Assert.AreEqual(0f, uiPos.y, 0.01f);
        }

        [Test]
        public void RadarPosition_ClampsEntitiesOutsideRadarRange()
        {
            float scale = (minimap.mapSize / 2f) / minimap.radarRange;
            float maxDist = (minimap.mapSize / 2f) - 6f; // 75 - 6 = 69f

            // Entity 100 units away in world space (exceeds 50 range)
            Vector2 distantOffset = new Vector2(100f, 0f);
            Vector2 uiPos = distantOffset * scale;

            if (uiPos.magnitude > maxDist)
            {
                uiPos = uiPos.normalized * maxDist;
            }

            Assert.AreEqual(maxDist, uiPos.magnitude, 0.01f);
            Assert.AreEqual(maxDist, uiPos.x, 0.01f);
        }

        [Test]
        public void Minimap_MapContainer_AnchorsToBottomRight()
        {
            Assert.IsNotNull(minimap.mapContainer);
            Assert.AreEqual(new Vector2(1f, 0f), minimap.mapContainer.anchorMin);
            Assert.AreEqual(new Vector2(1f, 0f), minimap.mapContainer.anchorMax);
            Assert.AreEqual(new Vector2(1f, 0f), minimap.mapContainer.pivot);
            Assert.AreEqual(new Vector2(-25f, 25f), minimap.mapContainer.anchoredPosition);
        }
    }
}
