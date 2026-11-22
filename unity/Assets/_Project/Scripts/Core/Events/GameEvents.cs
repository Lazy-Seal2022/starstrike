using System;
using UnityEngine;

namespace StarStrike.Core
{
    public static class GameEvents
    {
        public static event Action<float, float> OnShieldChanged; // current, max
        public static event Action<float, float> OnHullChanged;   // current, max
        public static event Action<float, float> OnEnergyChanged; // current, max
        public static event Action<float> OnPlayerDamaged;        // damageRatio (0..1)
        public static event Action<int, int> OnGemCollected;     // amount, total
        public static event Action<string> OnPlayerEvolved;       // shipId
        public static event Action<Vector2, string> OnAsteroidDestroyed; // pos, size
        public static event Action<Vector3, float, bool> OnDamageDealt; // pos, damage, isCrit

        public static void TriggerShieldChanged(float current, float max) => OnShieldChanged?.Invoke(current, max);
        public static void TriggerHullChanged(float current, float max) => OnHullChanged?.Invoke(current, max);
        public static void TriggerEnergyChanged(float current, float max) => OnEnergyChanged?.Invoke(current, max);
        public static void TriggerPlayerDamaged(float damageRatio) => OnPlayerDamaged?.Invoke(damageRatio);
        public static void TriggerGemCollected(int amount, int total) => OnGemCollected?.Invoke(amount, total);
        public static void TriggerPlayerEvolved(string shipId) => OnPlayerEvolved?.Invoke(shipId);
        public static void TriggerAsteroidDestroyed(Vector2 pos, string size) => OnAsteroidDestroyed?.Invoke(pos, size);
        public static void TriggerDamageDealt(Vector3 pos, float damage, bool isCrit = false) => OnDamageDealt?.Invoke(pos, damage, isCrit);

        public static event Action<string> OnEnemyDestroyed; // enemyId or enemyType
        public static void TriggerEnemyDestroyed(string enemyId) => OnEnemyDestroyed?.Invoke(enemyId);

        public static event Action OnBossDefeated;
        public static void TriggerBossDefeated() => OnBossDefeated?.Invoke();

        public static event Action<RunSummary> OnGameOver;
        public static void TriggerGameOver(RunSummary summary) => OnGameOver?.Invoke(summary);

        public static event Action<RunSummary> OnVictory;
        public static void TriggerVictory(RunSummary summary) => OnVictory?.Invoke(summary);
        
        public static event Action<string> OnAchievementUnlocked;
        public static void TriggerAchievementUnlocked(string id) => OnAchievementUnlocked?.Invoke(id);

        public static event Action OnGameStarted;
        public static void TriggerGameStarted() => OnGameStarted?.Invoke();

        public static event Action<SkillCastEvent> OnSkillCast;
        public static event Action<SkillCooldownEvent> OnSkillCooldownChanged;
        public static event Action<SkillBlockedEvent> OnSkillBlocked;

        public static void TriggerSkillCast(SkillCastEvent evt) => OnSkillCast?.Invoke(evt);
        public static void TriggerSkillCooldownChanged(SkillCooldownEvent evt) => OnSkillCooldownChanged?.Invoke(evt);
        public static void TriggerSkillBlocked(SkillBlockedEvent evt) => OnSkillBlocked?.Invoke(evt);

        public static event Action<string, int> OnWeaponSwapped;
        public static void TriggerWeaponSwapped(string weaponName, int weaponIndex) => OnWeaponSwapped?.Invoke(weaponName, weaponIndex);

        // Wave & Spawning Events
        public static event Action<int, string, float> OnWaveStarted; // waveNumber, waveName, duration
        public static event Action<int, int> OnWaveCompleted;         // waveNumber, rewardGems
        public static event Action<float> OnWaveTimeUpdated;          // remainingSeconds

        public static void TriggerWaveStarted(int waveNumber, string waveName, float duration) => OnWaveStarted?.Invoke(waveNumber, waveName, duration);
        public static void TriggerWaveCompleted(int waveNumber, int rewardGems) => OnWaveCompleted?.Invoke(waveNumber, rewardGems);
        public static void TriggerWaveTimeUpdated(float remainingSeconds) => OnWaveTimeUpdated?.Invoke(remainingSeconds);

        // Hazard & Environmental Events
        public static event Action<string, float> OnHazardWarning; // hazardName, duration
        public static event Action<string> OnHazardTriggered;     // hazardName
        public static event Action<string> OnHazardEnded;         // hazardName
        public static event Action<float> OnRadarJitterChanged;   // jitterAmount

        public static void TriggerHazardWarning(string hazardName, float duration) => OnHazardWarning?.Invoke(hazardName, duration);
        public static void TriggerHazardTriggered(string hazardName) => OnHazardTriggered?.Invoke(hazardName);
        public static void TriggerHazardEnded(string hazardName) => OnHazardEnded?.Invoke(hazardName);
        public static void TriggerRadarJitterChanged(float jitter) => OnRadarJitterChanged?.Invoke(jitter);
    }
}
