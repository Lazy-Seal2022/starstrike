using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "StarStrike/Configs/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Music Tracks")]
        public AudioClip ambientMusic;

        [Header("Sound Effects")]
        public AudioClip laser;
        public AudioClip enemyLaser;
        public AudioClip gem;
        public AudioClip explosion;
        public AudioClip upgrade;
        public AudioClip click;

        public AudioClip GetClip(string id)
        {
            switch (id)
            {
                case "ambient_music": return ambientMusic;
                case "laser":
                case "laser_fire": return laser;
                case "enemy_laser": return enemyLaser;
                case "gem": return gem;
                case "explosion": return explosion;
                case "upgrade":
                case "upgrade_acquired": return upgrade;
                case "click": return click;
                default: return null;
            }
        }
    }
}
