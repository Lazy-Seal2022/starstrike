using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "StarStrike/Configs/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("World Dimensions")]
        public float worldWidth = 140f;
        public float worldHeight = 140f;
        public int asteroidCount = 50;
        public int initialDroneCount = 6;
        
        [Header("Legacy / Physics")]
        public float baseFriction = 0.985f;
        public float maxVelocity = 500f;
        public float rotationSpeed = 0.08f;
        
        [Header("Mining & Magnet")]
        public float gemMagnetRadius = 6.5f;
        public float gemMagnetSpeed = 14f;

        [Header("Progression Costs")]
        public int upgradeBaseCost = 10;
        public float upgradeCostMultiplier = 1.6f;
        
        [Header("Subsystem Config References")]
        public FlightConfig flightConfig;
        public CombatConfig combatConfig;
        public AsteroidConfig asteroidConfig;
        public BossConfig bossConfig;
        public LootConfig lootConfig;
        public UpgradeDefinition upgradeDefinition;
        public ShipDefinition defaultShip;
    }
}
