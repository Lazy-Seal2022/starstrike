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
    }
}
