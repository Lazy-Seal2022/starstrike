using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Tests.EditMode
{
    public class WaveSystemTests
    {
        private List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in createdObjects)
            {
                if (go != null) Object.DestroyImmediate(go);
            }
            createdObjects.Clear();
        }

        private GameObject Track(GameObject go)
        {
            createdObjects.Add(go);
            return go;
        }

        [Test]
        public void WaveConfig_InitializesWithValidSequences()
        {
            var config = ScriptableObject.CreateInstance<WaveConfig>();
            var wave1 = ScriptableObject.CreateInstance<WaveDefinition>();
            wave1.waveNumber = 1;
            wave1.waveName = "Recon";
            wave1.durationSeconds = 30f;
            wave1.clearGemReward = 20;

            var wave2 = ScriptableObject.CreateInstance<WaveDefinition>();
            wave2.waveNumber = 2;
            wave2.waveName = "Assault";
            wave2.durationSeconds = 45f;
            wave2.clearGemReward = 50;

            config.waves = new[] { wave1, wave2 };
            config.intermissionSeconds = 3f;
            config.loopWaves = true;

            Assert.AreEqual(2, config.waves.Length);
            Assert.AreEqual("Recon", config.waves[0].waveName);
            Assert.AreEqual(50, config.waves[1].clearGemReward);
            Assert.IsTrue(config.loopWaves);
        }

        [Test]
        public void WaveManager_StartWave_TriggersWaveStartedEvent()
        {
            var go = Track(new GameObject("TestWaveManager"));
            var manager = go.AddComponent<WaveManager>();

            var config = ScriptableObject.CreateInstance<WaveConfig>();
            var wave1 = ScriptableObject.CreateInstance<WaveDefinition>();
            wave1.waveNumber = 1;
            wave1.waveName = "Sector Patrol";
            wave1.durationSeconds = 25f;
            config.waves = new[] { wave1 };
            manager.waveConfig = config;

            bool eventFired = false;
            int receivedWave = 0;
            string receivedName = "";

            GameEvents.OnWaveStarted += (num, name, dur) =>
            {
                eventFired = true;
                receivedWave = num;
                receivedName = name;
            };

            manager.StartWave(0);

            Assert.IsTrue(eventFired, "OnWaveStarted event must fire when wave begins");
            Assert.AreEqual(1, receivedWave);
            Assert.AreEqual("Sector Patrol", receivedName);
            Assert.IsTrue(manager.IsWaveActive);
            Assert.AreEqual(wave1, manager.CurrentWave);
        }

        [Test]
        public void WaveManager_HandleBossDefeated_CompletesWaveAndGrantsReward()
        {
            var go = Track(new GameObject("TestWaveManager"));
            var manager = go.AddComponent<WaveManager>();

            var playerGo = Track(new GameObject("TestPlayer"));
            var progression = playerGo.AddComponent<PlayerProgression>();
            progression.gems = 0;

            var config = ScriptableObject.CreateInstance<WaveConfig>();
            var bossWave = ScriptableObject.CreateInstance<WaveDefinition>();
            bossWave.waveNumber = 3;
            bossWave.waveName = "Boss Encounter";
            bossWave.durationSeconds = 60f;
            bossWave.clearGemReward = 100;
            config.waves = new[] { bossWave };
            manager.waveConfig = config;

            bool waveCompletedFired = false;
            int completedWaveNum = 0;
            int grantedGems = 0;

            GameEvents.OnWaveCompleted += (num, gems) =>
            {
                waveCompletedFired = true;
                completedWaveNum = num;
                grantedGems = gems;
            };

            manager.StartWave(0);
            GameEvents.TriggerBossDefeated();

            Assert.IsTrue(waveCompletedFired, "OnWaveCompleted event must fire when boss is defeated");
            Assert.AreEqual(3, completedWaveNum);
            Assert.AreEqual(100, grantedGems);
            Assert.AreEqual(100, progression.gems, "PlayerProgression must be awarded clear gem rewards");
            Assert.IsFalse(manager.IsWaveActive, "Wave should become inactive upon completion");
            Assert.IsTrue(manager.IsIntermission, "Manager should transition into intermission state");
        }

        [Test]
        public void WaveResources_ContainAllEnemyArchetypes()
        {
            var config = Resources.Load<WaveConfig>("Data/WaveConfig");
            Assert.IsNotNull(config, "WaveConfig must load from Resources/Data/WaveConfig");
            Assert.IsNotNull(config.waves);
            Assert.GreaterOrEqual(config.waves.Length, 3);

            var ids = new HashSet<string>();
            foreach (var wave in config.waves)
            {
                if (wave == null || wave.enemySpawns == null) continue;
                foreach (var entry in wave.enemySpawns)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.enemyId))
                    {
                        ids.Add(entry.enemyId);
                    }
                }
            }

            Assert.IsTrue(ids.Contains("EnemyDrone"), "Waves must include EnemyDrone");
            Assert.IsTrue(ids.Contains("Kamikaze"), "Waves must include Kamikaze");
            Assert.IsTrue(ids.Contains("Sniper"), "Waves must include Sniper");
            Assert.IsTrue(ids.Contains("SupportShield"), "Waves must include SupportShield");
        }

        [Test]
        public void WaveManager_ConfigureEnemyAI_ScalesStatsWithDifficultyMultiplier()
        {
            var go = Track(new GameObject("ScaledSniper"));
            WaveManager.ConfigureEnemyAI(go, "Sniper", 1.5f);

            var ai = go.GetComponent<EnemyAI>();
            var health = go.GetComponent<ShipHealth>();
            Assert.IsNotNull(ai);
            Assert.IsNotNull(health);
            Assert.AreEqual(EnemyArchetype.Sniper, ai.Archetype);
            Assert.AreEqual(75f, health.maxShield, 0.01f, "Sniper base shield (50) should scale by 1.5x");
            Assert.AreEqual(52.5f, health.maxHull, 0.01f, "Sniper base hull (35) should scale by 1.5x");
        }
    }
}
