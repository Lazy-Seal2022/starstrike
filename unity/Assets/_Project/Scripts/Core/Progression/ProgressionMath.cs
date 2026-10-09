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
            var baseStats = ship != null ? ship.baseStats : default;
            var mults = upgradeDef != null ? upgradeDef.multipliers : new StatValues
            {
                shieldCapacity = 1.15f,
                shieldRegen = 1.15f,
                energyCapacity = 1.15f,
                energyRegen = 1.15f,
                pulseDamage = 1.15f,
                pulseSpeed = 1.15f,
                speed = 1.10f,
                agility = 1.10f
            };
            if (state == null) state = new UpgradeState();

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
