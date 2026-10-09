using UnityEngine;

namespace StarStrike.Platform.Storage
{
    public static class StorageService
    {
        private const string KEY_HIGH_SCORE = "StarStrike_HighScore";
        private const string KEY_MUTE_AUDIO = "StarStrike_MuteAudio";
        private const string KEY_LAST_SHIP = "StarStrike_LastShip";

        public static void SaveHighScore(int score)
        {
            if (score > GetHighScore())
            {
                PlayerPrefs.SetInt(KEY_HIGH_SCORE, score);
                PlayerPrefs.Save();
            }
        }

        public static int GetHighScore()
        {
            return PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);
        }

        public static void SaveAudioMuted(bool isMuted)
        {
            PlayerPrefs.SetInt(KEY_MUTE_AUDIO, isMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool GetAudioMuted()
        {
            return PlayerPrefs.GetInt(KEY_MUTE_AUDIO, 0) == 1;
        }

        public static void SaveLastShipEvolved(string shipId)
        {
            PlayerPrefs.SetString(KEY_LAST_SHIP, shipId);
            PlayerPrefs.Save();
        }

        public static string GetLastShipEvolved()
        {
            return PlayerPrefs.GetString(KEY_LAST_SHIP, "");
        }
    }
}
