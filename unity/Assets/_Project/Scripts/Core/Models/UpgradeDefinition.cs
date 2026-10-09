using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "New Upgrade Definition", menuName = "StarStrike/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Tooltip("The multiplier applied per level. (e.g. 1.25 for 25% increase)")]
        public StatValues multipliers = new StatValues
        {
            shieldCapacity = 1.25f,
            shieldRegen = 1.3f,
            energyCapacity = 1.2f,
            energyRegen = 1.35f,
            pulseDamage = 1.25f,
            pulseSpeed = 1.15f,
            speed = 1.12f,
            agility = 1.15f
        };
    }
}
