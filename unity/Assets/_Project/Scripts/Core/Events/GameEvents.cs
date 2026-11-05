using System;
using UnityEngine;

namespace StarStrike.Core
{
    public static class GameEvents
    {
        public static event Action<float, float> OnShieldChanged; // current, max
        public static event Action<float, float> OnEnergyChanged; // current, max
        public static event Action<int, int> OnGemCollected;     // amount, total
        public static event Action<string> OnPlayerEvolved;       // shipId
        public static event Action<Vector2, string> OnAsteroidDestroyed; // pos, size

        public static void TriggerShieldChanged(float current, float max) => OnShieldChanged?.Invoke(current, max);
        public static void TriggerEnergyChanged(float current, float max) => OnEnergyChanged?.Invoke(current, max);
        public static void TriggerGemCollected(int amount, int total) => OnGemCollected?.Invoke(amount, total);
        public static void TriggerPlayerEvolved(string shipId) => OnPlayerEvolved?.Invoke(shipId);
        public static void TriggerAsteroidDestroyed(Vector2 pos, string size) => OnAsteroidDestroyed?.Invoke(pos, size);

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
    }
}
