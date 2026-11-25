using System;
using UnityEngine;

namespace StarStrike.Core
{
    [Serializable]
    public class SpawnEntry
    {
        public string enemyId = "drone";
        public GameObject prefab;
        public float spawnInterval = 4f;
        public int maxConcurrent = 8;
        public int totalToSpawn = -1; // -1 for unlimited
    }

    [CreateAssetMenu(fileName = "WaveDefinition", menuName = "StarStrike/Configs/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        public int waveNumber = 1;
        public string waveName = "Incursion";
        public float durationSeconds = 45f;
        public int targetAsteroidCount = 40;
        public SpawnEntry[] enemySpawns;
        public GameObject bossPrefab;
        public int clearGemReward = 25;
    }
}
