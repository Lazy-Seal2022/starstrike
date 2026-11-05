using UnityEngine;

namespace StarStrike.Core
{
    public enum AsteroidTier
    {
        Large,
        Medium,
        Small
    }

    [System.Serializable]
    public class AsteroidTierData
    {
        public float maxHp = 120f;
        public int gemDropCount = 8;
        public float visualScale = 2.2f;
        public float colliderRadius = 0.45f;
    }

    [CreateAssetMenu(fileName = "AsteroidConfig", menuName = "StarStrike/Configs/Asteroid Config")]
    public class AsteroidConfig : ScriptableObject
    {
        [Header("Tiers")]
        public AsteroidTierData largeTier = new AsteroidTierData { maxHp = 120f, gemDropCount = 8, visualScale = 2.2f, colliderRadius = 0.45f };
        public AsteroidTierData mediumTier = new AsteroidTierData { maxHp = 50f, gemDropCount = 4, visualScale = 1.3f, colliderRadius = 0.45f };
        public AsteroidTierData smallTier = new AsteroidTierData { maxHp = 20f, gemDropCount = 1, visualScale = 0.75f, colliderRadius = 0.45f };

        [Header("Fracturing & Drops")]
        [Range(0f, 1f)]
        public float powerUpDropChance = 0.05f;
        public int fractureSplitCount = 2;
        public float minChildSpeed = 2.0f;
        public float maxChildSpeed = 4.5f;

        public AsteroidTierData GetTierData(AsteroidTier tier)
        {
            switch (tier)
            {
                case AsteroidTier.Large:
                    return largeTier;
                case AsteroidTier.Medium:
                    return mediumTier;
                case AsteroidTier.Small:
                default:
                    return smallTier;
            }
        }
    }
}
