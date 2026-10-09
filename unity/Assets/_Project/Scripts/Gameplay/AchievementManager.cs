using UnityEngine;
using StarStrike.Core;
using StarStrike.Platform.Storage;

namespace StarStrike.Gameplay
{
    public class AchievementManager : MonoBehaviour
    {
        private void OnEnable()
        {
            GameEvents.OnAsteroidDestroyed += OnAsteroidDestroyed;
            GameEvents.OnGemCollected += OnGemCollected;
            GameEvents.OnPlayerEvolved += OnPlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnAsteroidDestroyed -= OnAsteroidDestroyed;
            GameEvents.OnGemCollected -= OnGemCollected;
            GameEvents.OnPlayerEvolved -= OnPlayerEvolved;
        }

        private void OnAsteroidDestroyed(Vector2 pos, string size)
        {
            StorageService.IncrementStat("asteroids_destroyed");
            
            // Example achievement check
            var profile = StorageService.Profile;
            var index = profile.stats.FindIndex(s => s.key == "asteroids_destroyed");
            if (index >= 0 && profile.stats[index].value >= 100)
            {
                StorageService.UnlockAchievement("ASTEROID_SLAYER");
            }
        }

        private void OnGemCollected(int amount, int total)
        {
            StorageService.AddMetaGems(amount);
            
            if (StorageService.Profile.totalMetaGems >= 1000)
            {
                StorageService.UnlockAchievement("GEM_HOARDER");
            }
        }

        private void OnPlayerEvolved(string shipId)
        {
            if (shipId == "juggernaut")
            {
                StorageService.UnlockAchievement("TITAN_COMMANDER");
            }
            else if (shipId == "interceptor")
            {
                StorageService.UnlockAchievement("SPEED_DEMON");
            }
        }
    }
}
