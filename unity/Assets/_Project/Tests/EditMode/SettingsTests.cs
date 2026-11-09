using NUnit.Framework;
using UnityEngine;
using StarStrike.Platform.Storage;
using StarStrike.Gameplay.Audio;
using StarStrike.Gameplay;
using StarStrike.UI;

namespace StarStrike.Tests.EditMode
{
    public class SettingsTests
    {
        private GameObject trackObject;

        [TearDown]
        public void TearDown()
        {
            if (trackObject != null)
            {
                Object.DestroyImmediate(trackObject);
                trackObject = null;
            }
            StorageService.ResetSettingsToDefaults();
        }

        [Test]
        public void StorageService_SettingsDefaults_AreCorrect()
        {
            StorageService.ResetSettingsToDefaults();

            Assert.AreEqual(1.0f, StorageService.GetMasterVolume(), 0.01f);
            Assert.AreEqual(0.8f, StorageService.GetSfxVolume(), 0.01f);
            Assert.AreEqual(0.7f, StorageService.GetMusicVolume(), 0.01f);
            Assert.IsFalse(StorageService.GetAudioMuted());

            Assert.AreEqual(0, StorageService.GetControlAimMode());
            Assert.AreEqual(1.0f, StorageService.GetMouseSensitivity(), 0.01f);
            Assert.IsTrue(StorageService.GetAutoBrake());
            Assert.IsFalse(StorageService.GetShowMobileControls());
            Assert.IsFalse(StorageService.GetInvertTurn());
        }

        [Test]
        public void StorageService_SettingsPersistence_SavesAndRetrievesValues()
        {
            StorageService.SaveMasterVolume(0.45f);
            Assert.AreEqual(0.45f, StorageService.GetMasterVolume(), 0.01f);

            StorageService.SaveSfxVolume(0.6f);
            Assert.AreEqual(0.6f, StorageService.GetSfxVolume(), 0.01f);

            StorageService.SaveMusicVolume(0.3f);
            Assert.AreEqual(0.3f, StorageService.GetMusicVolume(), 0.01f);

            StorageService.SaveControlAimMode(1);
            Assert.AreEqual(1, StorageService.GetControlAimMode());

            StorageService.SaveAutoBrake(false);
            Assert.IsFalse(StorageService.GetAutoBrake());

            StorageService.SaveInvertTurn(true);
            Assert.IsTrue(StorageService.GetInvertTurn());

            StorageService.SaveShowMobileControls(true);
            Assert.IsTrue(StorageService.GetShowMobileControls());

            // Reset back
            StorageService.ResetSettingsToDefaults();
            Assert.AreEqual(1.0f, StorageService.GetMasterVolume(), 0.01f);
            Assert.AreEqual(0, StorageService.GetControlAimMode());
        }

        [Test]
        public void AudioService_VolumeControls_UpdateSettingsProperly()
        {
            trackObject = new GameObject("TestAudioService");
            var audioService = trackObject.AddComponent<AudioService>();

            audioService.SetMasterVolume(0.5f);
            Assert.AreEqual(0.5f, audioService.masterVolume, 0.01f);
            Assert.AreEqual(0.5f, StorageService.GetMasterVolume(), 0.01f);

            audioService.SetSfxVolume(0.75f);
            Assert.AreEqual(0.75f, audioService.sfxVolume, 0.01f);
            Assert.AreEqual(0.75f, StorageService.GetSfxVolume(), 0.01f);

            audioService.SetMusicVolume(0.4f);
            Assert.AreEqual(0.4f, audioService.musicVolume, 0.01f);
            Assert.AreEqual(0.4f, StorageService.GetMusicVolume(), 0.01f);

            audioService.SetMuted(true);
            Assert.IsTrue(audioService.isMuted);
            Assert.IsTrue(StorageService.GetAudioMuted());

            audioService.SetMuted(false);
            Assert.IsFalse(audioService.isMuted);
        }

        [Test]
        public void PlayerInputReader_ApplySettings_SynchronizesFromStorage()
        {
            trackObject = new GameObject("TestPlayerInput");
            trackObject.AddComponent<ShipMovement>();
            trackObject.AddComponent<ShipWeapon>();
            trackObject.AddComponent<ShipHealth>();
            var inputReader = trackObject.AddComponent<PlayerInputReader>();

            StorageService.SaveControlAimMode(1);
            StorageService.SaveAutoBrake(false);
            StorageService.SaveInvertTurn(true);

            inputReader.ApplySettings();

            Assert.AreEqual(1, inputReader.aimMode);
            Assert.IsFalse(inputReader.autoBrakeEnabled);
            Assert.IsTrue(inputReader.invertTurn);
        }

        [Test]
        public void SettingsUI_OpenCloseAndTabSwitching_WorksCorrectly()
        {
            trackObject = new GameObject("TestSettingsUI");
            var settings = trackObject.AddComponent<SettingsUI>();

            Assert.IsFalse(settings.IsVisible, "Settings should start hidden");

            settings.Open();
            Assert.IsTrue(settings.IsVisible, "Settings should be visible after Open()");

            settings.SwitchTab(SettingsUI.SettingsTab.Controls);
            // Controls tab active

            settings.SwitchTab(SettingsUI.SettingsTab.Audio);
            // Audio tab active

            // Test audio adjustment methods
            float initialMaster = StorageService.GetMasterVolume();
            settings.AdjustMasterVolume(-0.2f);
            Assert.Less(StorageService.GetMasterVolume(), initialMaster);

            // Test toggles
            int initialAim = StorageService.GetControlAimMode();
            settings.ToggleAimMode();
            Assert.AreNotEqual(initialAim, StorageService.GetControlAimMode());

            bool initialBrake = StorageService.GetAutoBrake();
            settings.ToggleAutoBrake();
            Assert.AreNotEqual(initialBrake, StorageService.GetAutoBrake());

            settings.Close();
            Assert.IsFalse(settings.IsVisible, "Settings should be hidden after Close()");
        }
    }
}
