using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "FlightConfig", menuName = "StarStrike/Configs/Flight Config")]
    public class FlightConfig : ScriptableObject
    {
        [Header("Default Physics")]
        public float defaultMaxSpeed = 10f;
        public float defaultAcceleration = 18f;
        public float defaultAgility = 6f; // rad/sec
        public float linearDamping = 0.5f;
        public float angularDamping = 1.0f;
        public float brakeDecelerationFactor = 0.4f;

        [Header("Afterburner Boost")]
        public float boostMultiplier = 1.8f;
        public float boostMaxSpeedMultiplier = 1.4f;
        public float boostEnergyDrain = 25f; // Energy units / second
        public float minEnergyToBoost = 5f;
    }
}
