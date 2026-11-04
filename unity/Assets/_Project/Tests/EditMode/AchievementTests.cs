using NUnit.Framework;
using UnityEngine;
using StarStrike.Platform.Storage;

namespace StarStrike.Tests.EditMode
{
    public class AchievementTests
    {
        [SetUp]
        public void SetUp()
        {
            StorageService.LoadProfile();
            StorageService.Profile.achievements.Clear();
            StorageService.Profile.stats.Clear();
            StorageService.Profile.totalMetaGems = 0;
        }

        [TearDown]
        public void TearDown()
        {
            StorageService.Profile.achievements.Clear();
            StorageService.Profile.stats.Clear();
            StorageService.Profile.totalMetaGems = 0;
        }

        [Test]
        public void IncrementStat_AddsNewStatAndIncrementsExisting()
        {
            StorageService.IncrementStat("asteroids_destroyed", 5);
            var entry = StorageService.Profile.stats.Find(s => s.key == "asteroids_destroyed");
            Assert.IsNotNull(entry);
            Assert.AreEqual(5, entry.value);

            StorageService.IncrementStat("asteroids_destroyed", 10);
            entry = StorageService.Profile.stats.Find(s => s.key == "asteroids_destroyed");
            Assert.AreEqual(15, entry.value);
        }

        [Test]
        public void UnlockAchievement_AddsAchievementAndIsIdempotent()
        {
            Assert.IsFalse(StorageService.Profile.achievements.Contains("SPEED_DEMON"));

            StorageService.UnlockAchievement("SPEED_DEMON");
            Assert.IsTrue(StorageService.Profile.achievements.Contains("SPEED_DEMON"));
            Assert.AreEqual(1, StorageService.Profile.achievements.Count);

            // Re-unlocking must not duplicate
            StorageService.UnlockAchievement("SPEED_DEMON");
            Assert.AreEqual(1, StorageService.Profile.achievements.Count);
        }
    }
}
