using NUnit.Framework;
using UnityEngine;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class CameraZoomTests
    {
        private GameObject cameraGo;
        private GameObject targetGo;
        private GameObject settingsGo;

        [TearDown]
        public void TearDown()
        {
            if (cameraGo != null)
            {
                Object.DestroyImmediate(cameraGo);
                cameraGo = null;
            }
            if (targetGo != null)
            {
                Object.DestroyImmediate(targetGo);
                targetGo = null;
            }
            if (settingsGo != null)
            {
                Object.DestroyImmediate(settingsGo);
                settingsGo = null;
            }
            StorageService.ResetSettingsToDefaults();
        }

        [Test]
        public void CameraController_InitializesWithBroadDefaultZoom()
        {
            StorageService.ResetSettingsToDefaults();

            cameraGo = new GameObject("TestCam");
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f; // initial small size

            var controller = cameraGo.AddComponent<CameraController>();
            controller.EnsureInitialized();

            Assert.AreEqual(18f, controller.GetBaseZoom(), 0.01f);
            Assert.AreEqual(18f, cam.orthographicSize, 0.01f);
            Assert.IsTrue(cam.orthographic);
        }

        [Test]
        public void CameraController_SetBaseZoom_ClampsToBoundsAndPersists()
        {
            StorageService.ResetSettingsToDefaults();

            cameraGo = new GameObject("TestCam");
            var cam = cameraGo.AddComponent<Camera>();
            var controller = cameraGo.AddComponent<CameraController>();
            controller.EnsureInitialized();

            // Upper bound clamp
            controller.SetBaseZoom(50f);
            Assert.AreEqual(30f, controller.GetBaseZoom(), 0.01f);
            Assert.AreEqual(30f, StorageService.GetCameraZoom(), 0.01f);

            // Lower bound clamp
            controller.SetBaseZoom(5f);
            Assert.AreEqual(12f, controller.GetBaseZoom(), 0.01f);
            Assert.AreEqual(12f, StorageService.GetCameraZoom(), 0.01f);

            // In-range value
            controller.SetBaseZoom(20f);
            Assert.AreEqual(20f, controller.GetBaseZoom(), 0.01f);
            Assert.AreEqual(20f, StorageService.GetCameraZoom(), 0.01f);
        }

        [Test]
        public void CameraController_AdjustZoom_AdjustsBaseZoom()
        {
            StorageService.ResetSettingsToDefaults();

            cameraGo = new GameObject("TestCam");
            cameraGo.AddComponent<Camera>();
            var controller = cameraGo.AddComponent<CameraController>();
            controller.EnsureInitialized();

            controller.SetBaseZoom(18f);
            controller.AdjustZoom(3f);
            Assert.AreEqual(21f, controller.GetBaseZoom(), 0.01f);

            controller.AdjustZoom(-5f);
            Assert.AreEqual(16f, controller.GetBaseZoom(), 0.01f);
        }

        [Test]
        public void SettingsUI_CycleZoom_CyclesThroughZoomPresets()
        {
            StorageService.ResetSettingsToDefaults();

            cameraGo = new GameObject("TestCam");
            cameraGo.AddComponent<Camera>();
            var camCtrl = cameraGo.AddComponent<CameraController>();
            camCtrl.EnsureInitialized();

            settingsGo = new GameObject("SettingsUI");
            var settings = settingsGo.AddComponent<SettingsUI>();

            // Initial default is 18f
            Assert.AreEqual(18f, StorageService.GetCameraZoom(), 0.01f);

            // Cycle 1: 18 -> 22
            settings.CycleZoom();
            Assert.AreEqual(22f, StorageService.GetCameraZoom(), 0.01f);
            Assert.AreEqual(22f, camCtrl.GetBaseZoom(), 0.01f);

            // Cycle 2: 22 -> 26
            settings.CycleZoom();
            Assert.AreEqual(26f, StorageService.GetCameraZoom(), 0.01f);
            Assert.AreEqual(26f, camCtrl.GetBaseZoom(), 0.01f);

            // Cycle 3: 26 -> 14
            settings.CycleZoom();
            Assert.AreEqual(14f, StorageService.GetCameraZoom(), 0.01f);
            Assert.AreEqual(14f, camCtrl.GetBaseZoom(), 0.01f);

            // Cycle 4: 14 -> 18
            settings.CycleZoom();
            Assert.AreEqual(18f, StorageService.GetCameraZoom(), 0.01f);
            Assert.AreEqual(18f, camCtrl.GetBaseZoom(), 0.01f);
        }

        [Test]
        public void SettingsUI_ResetDefaults_RestoresBroadCameraZoom()
        {
            StorageService.SaveCameraZoom(28f);

            cameraGo = new GameObject("TestCam");
            cameraGo.AddComponent<Camera>();
            var camCtrl = cameraGo.AddComponent<CameraController>();
            camCtrl.EnsureInitialized();
            camCtrl.SetBaseZoom(28f);

            settingsGo = new GameObject("SettingsUI");
            var settings = settingsGo.AddComponent<SettingsUI>();

            settings.ResetDefaults();
            Assert.AreEqual(18f, StorageService.GetCameraZoom(), 0.01f);
            Assert.AreEqual(18f, camCtrl.GetBaseZoom(), 0.01f);
        }
    }
}
