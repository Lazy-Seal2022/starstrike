using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "StarStrike/Configs/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        public WaveDefinition[] waves;
        public float intermissionSeconds = 4f;
        public bool loopWaves = true;
        public float difficultyMultiplierPerLoop = 1.25f;
    }
}
