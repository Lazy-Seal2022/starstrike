using UnityEngine;

namespace StarStrike.Core
{
    public static class ProgressionMath
    {
        public static float CalculateStat(float baseValue, float multiplier, int level)
        {
            return baseValue * Mathf.Pow(multiplier, level);
        }

        public static int CalculateCost(int baseCost, float costMultiplier, int level)
        {
            return Mathf.FloorToInt(baseCost * Mathf.Pow(costMultiplier, level));
        }

        public static StatValues GetCurrentStats(ShipDefinition ship, UpgradeDefinition upgradeDef, UpgradeState state)
        {
            var baseStats = ship.baseStats;
            var mults = upgradeDef.multipliers;

            return new StatValues
            {
                shieldCapacity = CalculateStat(baseStats.shieldCapacity, mults.shieldCapacity, state.shieldCapacityLevel),
                shieldRegen = CalculateStat(baseStats.shieldRegen, mults.shieldRegen, state.shieldRegenLevel),
                energyCapacity = CalculateStat(baseStats.energyCapacity, mults.energyCapacity, state.energyCapacityLevel),
                energyRegen = CalculateStat(baseStats.energyRegen, mults.energyRegen, state.energyRegenLevel),
                pulseDamage = CalculateStat(baseStats.pulseDamage, mults.pulseDamage, state.pulseDamageLevel),
                pulseSpeed = CalculateStat(baseStats.pulseSpeed, mults.pulseSpeed, state.pulseSpeedLevel),
                speed = CalculateStat(baseStats.speed, mults.speed, state.speedLevel),
                agility = CalculateStat(baseStats.agility, mults.agility, state.agilityLevel)
            };
        }
    }
}
