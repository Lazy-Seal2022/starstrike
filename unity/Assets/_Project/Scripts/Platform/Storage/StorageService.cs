using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace StarStrike.Platform.Storage
{
    [System.Serializable]
    public class PlayerProfile
    {
        public int highScore = 0;
        public int totalMetaGems = 0;
        public bool isAudioMuted = false;
        public string lastShipId = "scout";
        public List<string> unlockedShips = new List<string> { "scout" };
        public List<string> achievements = new List<string>();
        
        [System.Serializable]
        public struct StatEntry { public string key; public int value; }
        public List<StatEntry> stats = new List<StatEntry>();
    }

    public static class StorageService
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "profile.json");
        private static PlayerProfile currentProfile;

        public static PlayerProfile Profile
        {
            get
            {
                if (currentProfile == null)
                {
                    LoadProfile();
                }
                return currentProfile;
            }
        }

        public static void LoadProfile()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                currentProfile = JsonUtility.FromJson<PlayerProfile>(json);
                if (currentProfile == null) currentProfile = new PlayerProfile();
            }
            else
            {
                currentProfile = new PlayerProfile();
            }
        }

        public static void SaveProfile()
        {
            if (currentProfile == null) return;
            string json = JsonUtility.ToJson(currentProfile, true);
            File.WriteAllText(SavePath, json);
        }

        public static void SaveHighScore(int score)
        {
            if (score > Profile.highScore)
            {
                Profile.highScore = score;
                SaveProfile();
            }
        }

        public static int GetHighScore() => Profile.highScore;

        public static void SaveAudioMuted(bool isMuted)
        {
            Profile.isAudioMuted = isMuted;
            SaveProfile();
        }

        public static bool GetAudioMuted() => Profile.isAudioMuted;

        public static void SaveLastShipEvolved(string shipId)
        {
            Profile.lastShipId = shipId;
            SaveProfile();
        }

        public static string GetLastShipEvolved() => Profile.lastShipId;
        
        public static void AddMetaGems(int amount)
        {
            Profile.totalMetaGems += amount;
            SaveProfile();
        }

        public static void UnlockShip(string shipId)
        {
            if (!Profile.unlockedShips.Contains(shipId))
            {
                Profile.unlockedShips.Add(shipId);
                SaveProfile();
            }
        }
        
        public static void IncrementStat(string statName, int amount = 1)
        {
            int index = Profile.stats.FindIndex(s => s.key == statName);
            if (index >= 0)
            {
                var entry = Profile.stats[index];
                entry.value += amount;
                Profile.stats[index] = entry;
            }
            else
            {
                Profile.stats.Add(new PlayerProfile.StatEntry { key = statName, value = amount });
            }
            SaveProfile();
        }

        public static void UnlockAchievement(string achievementId)
        {
            if (!Profile.achievements.Contains(achievementId))
            {
                Profile.achievements.Add(achievementId);
                Debug.Log("Achievement Unlocked: " + achievementId);
                StarStrike.Core.GameEvents.TriggerAchievementUnlocked(achievementId);
                SaveProfile();
            }
        }
    }
}
