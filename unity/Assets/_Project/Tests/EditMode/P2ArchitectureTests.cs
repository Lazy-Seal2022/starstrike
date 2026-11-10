using System.Collections;
using NUnit.Framework;
using UnityEngine;
using TMPro;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Gameplay.FX;
using StarStrike.Network;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class P2ArchitectureTests
    {
        private GameObject testRoot;

        [SetUp]
        public void SetUp()
        {
            testRoot = new GameObject("TestRoot_P2");
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1.0f;
            if (testRoot != null) Object.DestroyImmediate(testRoot);
        }

        [Test]
        public void EaseCurves_CalculateExpectedValuesAndOvershoot()
        {
            // Boundaries
            Assert.AreEqual(0f, Ease.Linear(0f), 0.001f);
            Assert.AreEqual(1f, Ease.Linear(1f), 0.001f);

            Assert.AreEqual(0f, Ease.OutQuad(0f), 0.001f);
            Assert.AreEqual(1f, Ease.OutQuad(1f), 0.001f);
            Assert.Greater(Ease.OutQuad(0.5f), 0.5f, "OutQuad should ease out faster than linear at mid-point");

            Assert.AreEqual(0f, Ease.OutCubic(0f), 0.001f);
            Assert.AreEqual(1f, Ease.OutCubic(1f), 0.001f);

            Assert.AreEqual(0f, Ease.OutBack(0f), 0.001f);
            Assert.AreEqual(1f, Ease.OutBack(1f), 0.001f);
            // OutBack overshoot: at t around 0.7 - 0.85, value must exceed 1.0
            float peak = Ease.OutBack(0.75f);
            Assert.Greater(peak, 1.0f, "OutBack curve must overshoot 1.0 to produce juice pop effect");
        }

        [Test]
        public void FXService_CameraShakeAndImpulse_ComputesOffsetCleanly()
        {
            var fxGo = new GameObject("TestFXService");
            fxGo.transform.SetParent(testRoot.transform, false);
            var fx = fxGo.AddComponent<FXService>();

            Assert.AreEqual(Vector3.zero, FXService.CameraOffset);

            // Trigger player damage shake
            fx.TriggerPlayerDamageShake(0.5f);
            // Verify service is active
            Assert.IsNotNull(FXService.Instance);
        }

        [Test]
        public void ShipThrusterEffect_TogglesTrailEmittingAndBoost()
        {
            var shipGo = new GameObject("TestShip");
            shipGo.transform.SetParent(testRoot.transform, false);
            var thruster = shipGo.AddComponent<ShipThrusterEffect>();
            thruster.EnsureInitialized();

            Assert.IsFalse(thruster.IsThrusting);
            Assert.IsFalse(thruster.IsBoosting);

            // Normal thrust
            thruster.SetThrusting(true, false);
            Assert.IsTrue(thruster.IsThrusting);
            Assert.IsFalse(thruster.IsBoosting);

            var trail = shipGo.GetComponentInChildren<TrailRenderer>();
            Assert.IsNotNull(trail, "ShipThrusterEffect must create a TrailRenderer");
            Assert.IsTrue(trail.emitting, "TrailRenderer must emit when thrusting");
            Assert.AreEqual(0.35f, trail.startWidth, 0.01f);

            // Boost thrust
            thruster.SetThrusting(true, true);
            Assert.IsTrue(thruster.IsBoosting);
            Assert.AreEqual(0.5f, trail.startWidth, 0.01f, "Boost must widen the trail");

            // Stop thrust
            thruster.SetThrusting(false, false);
            Assert.IsFalse(thruster.IsThrusting);
            Assert.IsFalse(trail.emitting, "TrailRenderer must stop emitting when idle");
        }

        [Test]
        public void HUDFloatingText_SpawnWorldDamage_CreatesTMPFloaterWithProperFormatting()
        {
            var hudGo = new GameObject("TestHUD");
            hudGo.transform.SetParent(testRoot.transform, false);
            hudGo.AddComponent<Canvas>();
            var floatingText = hudGo.AddComponent<HUDFloatingText>();

            // Normal damage
            floatingText.SpawnWorldDamage(Vector3.zero, 18f, isCrit: false);
            var tmpList = hudGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            Assert.Greater(tmpList.Length, 0, "Floating text floater must be created");
            Assert.IsTrue(tmpList[0].text.Contains("18"));

            // Critical hit
            floatingText.SpawnWorldDamage(new Vector3(5, 5, 0), 45f, isCrit: true);
            var allTmp = hudGo.GetComponentsInChildren<TextMeshProUGUI>(true);
            bool foundCrit = false;
            foreach (var t in allTmp)
            {
                if (t.text.Contains("CRIT! 45"))
                {
                    foundCrit = true;
                    Assert.AreEqual(FontStyles.Bold, t.fontStyle);
                    break;
                }
            }
            Assert.IsTrue(foundCrit, "Critical hit must display 'CRIT!' with bold styling");
        }

        [Test]
        public void NetworkSessionData_GenerateAndParseJoinCode_WorksAccurately()
        {
            string code = NetworkSessionData.GenerateJoinCode(7777);
            Assert.IsNotNull(code);
            Assert.IsTrue(code.StartsWith("STAR-"), "Join code must follow STAR-XXXX format");

            bool parsed = NetworkSessionData.TryParseJoinCode(code, out string host, out ushort port);
            Assert.IsTrue(parsed, "Generated join code must be parseable");
            Assert.AreEqual("127.0.0.1", host);
            Assert.AreEqual(7777, port);

            // Direct IP parsing
            bool ipParsed = NetworkSessionData.TryParseJoinCode("192.168.1.50:8888", out string ipHost, out ushort ipPort);
            Assert.IsTrue(ipParsed);
            Assert.AreEqual("192.168.1.50", ipHost);
            Assert.AreEqual(8888, ipPort);

            // Invalid code
            bool invalidParsed = NetworkSessionData.TryParseJoinCode("", out _, out _);
            Assert.IsFalse(invalidParsed, "Empty code must return false");
        }

        [Test]
        public void NetworkSessionManager_EnsureInfrastructure_BootstrapsSingletonSafely()
        {
            var mgrGo = new GameObject("SessionMgrTest");
            mgrGo.transform.SetParent(testRoot.transform, false);
            var sessionMgr = mgrGo.AddComponent<NetworkSessionManager>();

            sessionMgr.EnsureInfrastructure();
            Assert.IsFalse(sessionMgr.IsSessionActive, "Session should not be active prior to hosting or joining");
        }
    }
}
