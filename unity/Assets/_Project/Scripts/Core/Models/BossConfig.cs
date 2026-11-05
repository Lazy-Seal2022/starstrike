using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "StarStrike/Configs/Boss Config")]
    public class BossConfig : ScriptableObject
    {
        [Header("Spawn Settings")]
        public float spawnDelaySeconds = 60f;
        public float spawnYRatio = 0.8f;

        [Header("Boss Stats")]
        public float maxShield = 2500f;
        public float shieldRegen = 10f;
        public float maxSpeed = 4f;
        public float acceleration = 8f;
        public float agility = 3f;
        public float pulseDamage = 30f;
        public float pulseSpeed = 25f;
        public float fireRate = 0.5f;
        public int weaponSlots = 3;
        public float visualScale = 3f;
        public float colliderRadius = 0.8f;

        [Header("Phase Thresholds (HP Ratio)")]
        [Range(0f, 1f)] public float phase1Threshold = 0.75f; // Sweeping Lasers
        [Range(0f, 1f)] public float phase2Threshold = 0.50f; // Bullet Hell
        [Range(0f, 1f)] public float phase3Threshold = 0.25f; // Enraged
    }
}
