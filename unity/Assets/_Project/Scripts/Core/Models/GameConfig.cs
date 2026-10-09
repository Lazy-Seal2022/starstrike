using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "StarStrike/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("World")]
        public float worldWidth = 4000f;
        public float worldHeight = 4000f;
        
        [Header("Physics")]
        public float baseFriction = 0.985f;
        public float maxVelocity = 500f;
        public float rotationSpeed = 0.08f;
        
        [Header("Mining")]
        public int asteroidCount = 80;
        public float gemMagnetRadius = 180f;
        public float gemMagnetSpeed = 380f;

        [Header("Progression Costs")]
        public int upgradeBaseCost = 10;
        public float upgradeCostMultiplier = 1.6f;
        
        [Header("Global Upgrade Definition")]
        public UpgradeDefinition upgradeDefinition;
    }
}
