using NUnit.Framework;
using UnityEngine;
using StarStrike.Platform.Storage;

namespace StarStrike.Tests.EditMode
{
    public class StorageTests
    {
        [Test]
        public void PlayerProfile_DefaultValues_AreCorrect()
        {
            var profile = new PlayerProfile();

            Assert.AreEqual(0, profile.highScore);
            Assert.AreEqual(0, profile.totalMetaGems);
            Assert.IsFalse(profile.isAudioMuted);
            Assert.AreEqual("scout", profile.lastShipId);
            Assert.Contains("scout", profile.unlockedShips);
        }

        [Test]
        public void PlayerProfile_JsonSerialization_RoundTripsSuccessfully()
        {
            var profile = new PlayerProfile
            {
                highScore = 1500,
                totalMetaGems = 250,
                isAudioMuted = true,
                lastShipId = "fighter"
            };
            profile.unlockedShips.Add("fighter");

            string json = JsonUtility.ToJson(profile);
            var deserialized = JsonUtility.FromJson<PlayerProfile>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1500, deserialized.highScore);
            Assert.AreEqual(250, deserialized.totalMetaGems);
            Assert.IsTrue(deserialized.isAudioMuted);
            Assert.AreEqual("fighter", deserialized.lastShipId);
            Assert.IsTrue(deserialized.unlockedShips.Contains("fighter"));
        }

        [Test]
        public void AddMetaGems_IncrementsProfileGems()
        {
            int initialGems = StorageService.Profile.totalMetaGems;
            StorageService.AddMetaGems(100);

            Assert.AreEqual(initialGems + 100, StorageService.Profile.totalMetaGems);
        }

        [Test]
        public void UnlockShip_AddsShipIdIdempotently()
        {
            StorageService.UnlockShip("miner");
            Assert.IsTrue(StorageService.Profile.unlockedShips.Contains("miner"));

            int countBefore = StorageService.Profile.unlockedShips.Count;
            StorageService.UnlockShip("miner");
            Assert.AreEqual(countBefore, StorageService.Profile.unlockedShips.Count, "Unlocking already unlocked ship should not create duplicates");
        }

        [Test]
        public void SaveHighScore_OnlyUpdatesWhenHigher()
        {
            StorageService.Profile.highScore = 100;

            StorageService.SaveHighScore(50);
            Assert.AreEqual(100, StorageService.GetHighScore());

            StorageService.SaveHighScore(200);
            Assert.AreEqual(200, StorageService.GetHighScore());
        }
    }
}
