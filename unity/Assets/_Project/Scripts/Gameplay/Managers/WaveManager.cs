using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    /// <summary>
    /// Event-driven, data-driven wave manager that coordinates enemy escalation,
    /// dynamic hazard density, and boss summoning using zero-allocation object pools.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Configuration")]
        public WaveConfig waveConfig;

        [Header("World References")]
        public Transform playerTransform;
        public float worldWidth = 140f;
        public float worldHeight = 140f;

        [Header("State (Runtime Readonly)")]
        [SerializeField] private int currentWaveIndex = 0;
        [SerializeField] private bool isWaveActive = false;
        [SerializeField] private bool isIntermission = false;
        [SerializeField] private float waveTimer = 0f;
        [SerializeField] private float intermissionTimer = 0f;
        [SerializeField] private int loopCount = 0;

        public int CurrentWaveIndex => currentWaveIndex;
        public WaveDefinition CurrentWave => (waveConfig != null && waveConfig.waves != null && currentWaveIndex >= 0 && currentWaveIndex < waveConfig.waves.Length)
            ? waveConfig.waves[currentWaveIndex]
            : null;
        public bool IsWaveActive => isWaveActive;
        public bool IsIntermission => isIntermission;
        public float WaveTimeRemaining => CurrentWave != null && CurrentWave.durationSeconds > 0 ? Mathf.Max(0f, CurrentWave.durationSeconds - waveTimer) : 0f;
        public int LoopCount => loopCount;
        public float CurrentDifficultyScale => (waveConfig != null && loopCount > 0 && waveConfig.difficultyMultiplierPerLoop > 0f)
            ? Mathf.Pow(waveConfig.difficultyMultiplierPerLoop, loopCount)
            : 1f;

        private readonly Dictionary<string, float> spawnTimers = new Dictionary<string, float>();
        private readonly Dictionary<string, int> spawnedCountsByKey = new Dictionary<string, int>();
        private readonly Dictionary<string, List<GameObject>> activeEnemiesByKey = new Dictionary<string, List<GameObject>>();
        private readonly List<GameObject> activeWaveEnemies = new List<GameObject>();
        private bool bossSummonedForCurrentWave = false;
        private int lastBroadcastRemainingSec = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (waveConfig == null)
            {
                waveConfig = Resources.Load<WaveConfig>("Data/WaveConfig");
            }
        }

        private bool isSubscribed = false;

        private void SubscribeEvents()
        {
            if (isSubscribed) return;
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnEnemyDestroyed += HandleEnemyDestroyed;
            GameEvents.OnBossDefeated += HandleBossDefeated;
            isSubscribed = true;
        }

        private void UnsubscribeEvents()
        {
            if (!isSubscribed) return;
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnEnemyDestroyed -= HandleEnemyDestroyed;
            GameEvents.OnBossDefeated -= HandleBossDefeated;
            isSubscribed = false;
        }

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }

            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
            {
                StartWave(0);
            }
        }

        private void HandleGameStarted()
        {
            currentWaveIndex = 0;
            loopCount = 0;
            StartWave(0);
        }

        public void StartWave(int index)
        {
            SubscribeEvents();
            if (waveConfig == null || waveConfig.waves == null || waveConfig.waves.Length == 0) return;

            currentWaveIndex = Mathf.Clamp(index, 0, waveConfig.waves.Length - 1);
            var wave = CurrentWave;
            if (wave == null) return;

            isWaveActive = true;
            isIntermission = false;
            waveTimer = 0f;
            lastBroadcastRemainingSec = -1;
            bossSummonedForCurrentWave = false;
            spawnTimers.Clear();
            spawnedCountsByKey.Clear();
            activeEnemiesByKey.Clear();
            activeWaveEnemies.Clear();

            // Notify listeners & display floating announcement
            GameEvents.TriggerWaveStarted(wave.waveNumber, wave.waveName, wave.durationSeconds);

            // If wave has a boss, spawn it
            if (wave.bossPrefab != null)
            {
                SpawnWaveBoss(wave.bossPrefab);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            if (playerTransform == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
            }

            if (isIntermission)
            {
                intermissionTimer -= Time.deltaTime;
                if (intermissionTimer <= 0f)
                {
                    AdvanceToNextWave();
                }
                return;
            }

            if (!isWaveActive) return;

            var wave = CurrentWave;
            if (wave == null) return;

            waveTimer += Time.deltaTime;
            float remaining = WaveTimeRemaining;
            int remSec = Mathf.CeilToInt(remaining);
            if (remSec != lastBroadcastRemainingSec)
            {
                lastBroadcastRemainingSec = remSec;
                GameEvents.TriggerWaveTimeUpdated(remaining);
            }

            // Process enemy spawn entries
            if (wave.enemySpawns != null)
            {
                foreach (var entry in wave.enemySpawns)
                {
                    if (entry == null) continue;
                    ProcessSpawnEntry(entry);
                }
            }

            // Check wave duration completion (if not waiting on an undefeated boss)
            if (wave.durationSeconds > 0 && waveTimer >= wave.durationSeconds)
            {
                if (wave.bossPrefab == null || bossSummonedForCurrentWave == false)
                {
                    CompleteCurrentWave();
                }
            }
        }

        private void ProcessSpawnEntry(SpawnEntry entry)
        {
            string key = string.IsNullOrEmpty(entry.enemyId) ? "EnemyDrone" : entry.enemyId;
            if (!spawnTimers.ContainsKey(key))
            {
                spawnTimers[key] = 0f;
            }
            if (!spawnedCountsByKey.TryGetValue(key, out int totalSpawned))
            {
                totalSpawned = 0;
            }
            if (entry.totalToSpawn > 0 && totalSpawned >= entry.totalToSpawn)
            {
                return;
            }

            spawnTimers[key] += Time.deltaTime;

            if (spawnTimers[key] >= entry.spawnInterval)
            {
                spawnTimers[key] = 0f;

                int activeForKey = CleanActiveEnemyListForKey(key);
                if (entry.maxConcurrent <= 0 || activeForKey < entry.maxConcurrent)
                {
                    SpawnEnemy(entry, key);
                }
            }
        }

        private void SpawnEnemy(SpawnEntry entry, string key)
        {
            Vector2 spawnPos = GetOffscreenSpawnPosition();
            string tag = key;

            GameObject enemyObj = null;
            if (PoolManager.Instance != null)
            {
                enemyObj = PoolManager.Instance.Spawn(tag, spawnPos, Quaternion.identity, () =>
                {
                    if (entry.prefab != null)
                    {
                        var inst = Instantiate(entry.prefab, spawnPos, Quaternion.identity);
                        inst.name = tag;
                        return inst;
                    }
                    var fallback = new GameObject(tag);
                    fallback.AddComponent<EnemyAI>();
                    return fallback;
                });
            }
            else if (entry.prefab != null)
            {
                enemyObj = Instantiate(entry.prefab, spawnPos, Quaternion.identity);
                enemyObj.name = tag;
            }
            else
            {
                enemyObj = new GameObject(tag);
                enemyObj.transform.position = spawnPos;
                enemyObj.AddComponent<EnemyAI>();
            }

            if (enemyObj != null)
            {
                ConfigureEnemyAI(enemyObj, tag, CurrentDifficultyScale);
                if (!activeEnemiesByKey.TryGetValue(tag, out var keyList))
                {
                    keyList = new List<GameObject>();
                    activeEnemiesByKey[tag] = keyList;
                }
                keyList.Add(enemyObj);
                activeWaveEnemies.Add(enemyObj);
                spawnedCountsByKey.TryGetValue(tag, out int count);
                spawnedCountsByKey[tag] = count + 1;
            }
        }

        public static void ConfigureEnemyAI(GameObject enemyObj, string tagOrId)
        {
            ConfigureEnemyAI(enemyObj, tagOrId, 1f);
        }

        public static void ConfigureEnemyAI(GameObject enemyObj, string tagOrId, float difficultyScale)
        {
            if (enemyObj == null) return;
            var ai = enemyObj.GetComponent<EnemyAI>();
            if (ai == null) ai = enemyObj.AddComponent<EnemyAI>();

            string lower = (tagOrId ?? "").ToLowerInvariant();
            if (lower.Contains("kamikaze"))
            {
                ai.ConfigureArchetype(EnemyArchetype.Kamikaze, null, difficultyScale);
            }
            else if (lower.Contains("sniper"))
            {
                ai.ConfigureArchetype(EnemyArchetype.Sniper, null, difficultyScale);
            }
            else if (lower.Contains("support") || lower.Contains("aegis") || lower.Contains("shield"))
            {
                ai.ConfigureArchetype(EnemyArchetype.SupportShield, null, difficultyScale);
            }
            else if (lower.Contains("boss") || lower.Contains("dreadnought"))
            {
                ai.ConfigureArchetype(EnemyArchetype.Boss, null, difficultyScale);
            }
            else
            {
                ai.ConfigureArchetype(EnemyArchetype.Drone, null, difficultyScale);
            }
        }

        private void SpawnWaveBoss(GameObject bossPrefab)
        {
            if (bossSummonedForCurrentWave) return;
            bossSummonedForCurrentWave = true;

            float halfH = worldHeight * 0.5f;
            Vector2 pos = new Vector2(0f, halfH * 0.8f);

            GameObject bossObj;
            if (PoolManager.Instance != null)
            {
                bossObj = PoolManager.Instance.Spawn("Boss", pos, Quaternion.identity, () =>
                {
                    var inst = Instantiate(bossPrefab, pos, Quaternion.identity);
                    inst.name = "Boss";
                    return inst;
                });
            }
            else
            {
                bossObj = Instantiate(bossPrefab, pos, Quaternion.identity);
                bossObj.name = "Boss";
            }

            if (bossObj != null)
            {
                var bossBrain = bossObj.GetComponent<BossBrain>();
                if (bossBrain != null)
                {
                    bossBrain.ApplyDifficultyScale(CurrentDifficultyScale);
                }
            }
        }

        private void HandleEnemyDestroyed(string enemyId)
        {
            CleanActiveEnemyList();
            foreach (var kvp in activeEnemiesByKey)
            {
                PruneEnemyList(kvp.Value);
            }
        }

        private void HandleBossDefeated()
        {
            if (isWaveActive)
            {
                CompleteCurrentWave();
            }
        }

        private void CompleteCurrentWave()
        {
            var wave = CurrentWave;
            if (wave == null) return;

            isWaveActive = false;
            isIntermission = true;
            intermissionTimer = waveConfig != null ? waveConfig.intermissionSeconds : 4f;

            // Grant gem rewards
            var playerProg = PlayerProgression.Instance != null ? PlayerProgression.Instance : FindAnyObjectByType<PlayerProgression>();
            if (wave.clearGemReward > 0 && playerProg != null)
            {
                playerProg.AddGems(wave.clearGemReward);
            }

            GameEvents.TriggerWaveCompleted(wave.waveNumber, wave.clearGemReward);
        }

        private void AdvanceToNextWave()
        {
            int nextIndex = currentWaveIndex + 1;
            if (waveConfig != null && waveConfig.waves != null && nextIndex < waveConfig.waves.Length)
            {
                StartWave(nextIndex);
            }
            else if (waveConfig != null && waveConfig.loopWaves && waveConfig.waves != null && waveConfig.waves.Length > 0)
            {
                loopCount++;
                StartWave(0);
            }
            else
            {
                isWaveActive = false;
                isIntermission = false;
            }
        }

        private Vector2 GetOffscreenSpawnPosition()
        {
            Vector2 center = playerTransform != null ? (Vector2)playerTransform.position : Vector2.zero;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(24f, 32f);
            Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            float halfW = worldWidth * 0.5f - 4f;
            float halfH = worldHeight * 0.5f - 4f;
            candidate.x = Mathf.Clamp(candidate.x, -halfW, halfW);
            candidate.y = Mathf.Clamp(candidate.y, -halfH, halfH);

            return candidate;
        }

        private int CleanActiveEnemyListForKey(string key)
        {
            if (!activeEnemiesByKey.TryGetValue(key, out var list) || list == null) return 0;
            PruneEnemyList(list);
            return list.Count;
        }

        private void CleanActiveEnemyList()
        {
            PruneEnemyList(activeWaveEnemies);
        }

        private static void PruneEnemyList(List<GameObject> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null || !list[i].activeInHierarchy)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
