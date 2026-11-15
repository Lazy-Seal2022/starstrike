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
        public float masterVolume = 1.0f;
        public float sfxVolume = 0.8f;
        public float musicVolume = 0.7f;

        // Display & Visuals
        public float screenShakeIntensity = 1.0f;
        public bool isFullscreen = true;
        public int vSyncCount = 1;
        public int resolutionIndex = 0; // 0 = native / auto

        // Control settings
        public int controlAimMode = 0; // 0 = Mouse / Direct Aim, 1 = Keyboard Turn (Classic)
        public float mouseSensitivity = 1.0f;
        public bool autoBrakeEnabled = true;
        public bool showMobileControls = false;
        public bool invertTurn = false;
        public float cameraZoom = 18f;
        public string inputBindingOverrides = "";

        public string lastShipId = "scout";
        public string selectedSkill = "forward_shockwave";
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
            try
            {
                if (File.Exists(SavePath))
                {
                    string json = File.ReadAllText(SavePath);
                    currentProfile = JsonUtility.FromJson<PlayerProfile>(json);
                    if (currentProfile == null) currentProfile = new PlayerProfile();
                    else EnsureDefaultSettings(currentProfile);
                }
                else
                {
                    currentProfile = new PlayerProfile();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load player profile from {SavePath}: {ex.Message}. Using default profile.");
                currentProfile = new PlayerProfile();
            }
        }

        private static void EnsureDefaultSettings(PlayerProfile profile)
        {
            if (profile.masterVolume <= 0.001f && profile.sfxVolume <= 0.001f && profile.musicVolume <= 0.001f && !profile.isAudioMuted)
            {
                profile.masterVolume = 1.0f;
                profile.sfxVolume = 0.8f;
                profile.musicVolume = 0.7f;
                profile.mouseSensitivity = 1.0f;
                profile.autoBrakeEnabled = true;
            }
            if (profile.cameraZoom <= 0.001f)
            {
                profile.cameraZoom = 18f;
            }
            if (profile.screenShakeIntensity <= 0.001f && profile.masterVolume > 0f)
            {
                profile.screenShakeIntensity = 1.0f;
            }
        }

        public static void SaveProfile()
        {
            if (currentProfile == null) return;
            try
            {
                string json = JsonUtility.ToJson(currentProfile, true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save player profile to {SavePath}: {ex.Message}");
            }
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

        public static float GetMasterVolume() => Profile.masterVolume;
        public static void SaveMasterVolume(float vol)
        {
            Profile.masterVolume = Mathf.Clamp01(vol);
            SaveProfile();
        }

        public static float GetSfxVolume() => Profile.sfxVolume;
        public static void SaveSfxVolume(float vol)
        {
            Profile.sfxVolume = Mathf.Clamp01(vol);
            SaveProfile();
        }

        public static float GetMusicVolume() => Profile.musicVolume;
        public static void SaveMusicVolume(float vol)
        {
            Profile.musicVolume = Mathf.Clamp01(vol);
            SaveProfile();
        }

        public static int GetControlAimMode() => Profile.controlAimMode;
        public static void SaveControlAimMode(int mode)
        {
            Profile.controlAimMode = mode;
            SaveProfile();
        }

        public static float GetMouseSensitivity() => Profile.mouseSensitivity;
        public static void SaveMouseSensitivity(float sens)
        {
            Profile.mouseSensitivity = Mathf.Clamp(sens, 0.2f, 3.0f);
            SaveProfile();
        }

        public static bool GetAutoBrake() => Profile.autoBrakeEnabled;
        public static void SaveAutoBrake(bool enabled)
        {
            Profile.autoBrakeEnabled = enabled;
            SaveProfile();
        }

        public static bool GetShowMobileControls() => Profile.showMobileControls;
        public static void SaveShowMobileControls(bool show)
        {
            Profile.showMobileControls = show;
            SaveProfile();
        }

        public static bool GetInvertTurn() => Profile.invertTurn;
        public static void SaveInvertTurn(bool invert)
        {
            Profile.invertTurn = invert;
            SaveProfile();
        }

        public static float GetCameraZoom() => Profile.cameraZoom > 0f ? Profile.cameraZoom : 18f;
        public static void SaveCameraZoom(float zoom)
        {
            Profile.cameraZoom = Mathf.Clamp(zoom, 10f, 32f);
            SaveProfile();
        }

        public static float GetScreenShakeIntensity() => Profile.screenShakeIntensity;
        public static void SaveScreenShakeIntensity(float intensity)
        {
            Profile.screenShakeIntensity = Mathf.Clamp01(intensity);
            SaveProfile();
        }

        public static string GetInputBindingOverrides() => Profile.inputBindingOverrides ?? "";
        public static void SaveInputBindingOverrides(string overridesJson)
        {
            Profile.inputBindingOverrides = overridesJson ?? "";
            SaveProfile();
        }

        public static bool GetFullscreen() => Profile.isFullscreen;
        public static void SaveFullscreen(bool fullscreen)
        {
            Profile.isFullscreen = fullscreen;
            SaveProfile();
        }

        public static int GetVSync() => Profile.vSyncCount;
        public static void SaveVSync(int vsync)
        {
            Profile.vSyncCount = Mathf.Clamp(vsync, 0, 2);
            SaveProfile();
        }

        public static int GetResolutionIndex() => Profile.resolutionIndex;
        public static void SaveResolutionIndex(int idx)
        {
            Profile.resolutionIndex = idx;
            SaveProfile();
        }

        public static void ResetSettingsToDefaults()
        {
            Profile.isAudioMuted = false;
            Profile.masterVolume = 1.0f;
            Profile.sfxVolume = 0.8f;
            Profile.musicVolume = 0.7f;
            Profile.screenShakeIntensity = 1.0f;
            Profile.inputBindingOverrides = "";
            Profile.isFullscreen = true;
            Profile.vSyncCount = 1;
            Profile.resolutionIndex = 0;
            Profile.controlAimMode = 0;
            Profile.mouseSensitivity = 1.0f;
            Profile.autoBrakeEnabled = true;
            Profile.showMobileControls = false;
            Profile.invertTurn = false;
            Profile.cameraZoom = 18f;
            SaveProfile();
        }

        public static void SaveLastShipEvolved(string shipId)
        {
            Profile.lastShipId = shipId;
            SaveProfile();
        }

        public static string GetLastShipEvolved() => Profile.lastShipId;

        public static string GetSelectedSkill() => string.IsNullOrEmpty(Profile.selectedSkill) ? "forward_shockwave" : Profile.selectedSkill;
        public static void SaveSelectedSkill(string skillId)
        {
            Profile.selectedSkill = string.IsNullOrEmpty(skillId) ? "forward_shockwave" : skillId;
            SaveProfile();
        }
        
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
