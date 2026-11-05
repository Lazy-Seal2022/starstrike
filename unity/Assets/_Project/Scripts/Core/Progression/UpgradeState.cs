using System;

namespace StarStrike.Core
{
    [Serializable]
    public class UpgradeState
    {
        public int shieldCapacityLevel;
        public int shieldRegenLevel;
        public int energyCapacityLevel;
        public int energyRegenLevel;
        public int pulseDamageLevel;
        public int pulseSpeedLevel;
        public int speedLevel;
        public int agilityLevel;

        public int GetLevel(string statName)
        {
            switch (statName)
            {
                case "shieldCap": return shieldCapacityLevel;
                case "shieldRegen": return shieldRegenLevel;
                case "energyCap": return energyCapacityLevel;
                case "energyRegen": return energyRegenLevel;
                case "damage": return pulseDamageLevel;
                case "pulseSpeed": return pulseSpeedLevel;
                case "speed": return speedLevel;
                case "agility": return agilityLevel;
                default: return 0;
            }
        }

        public void Reset()
        {
            shieldCapacityLevel = 0;
            shieldRegenLevel = 0;
            energyCapacityLevel = 0;
            energyRegenLevel = 0;
            pulseDamageLevel = 0;
            pulseSpeedLevel = 0;
            speedLevel = 0;
            agilityLevel = 0;
        }

        public bool AreAllStatsMaxed(int maxLevel)
        {
            return shieldCapacityLevel >= maxLevel &&
                   shieldRegenLevel >= maxLevel &&
                   energyCapacityLevel >= maxLevel &&
                   energyRegenLevel >= maxLevel &&
                   pulseDamageLevel >= maxLevel &&
                   pulseSpeedLevel >= maxLevel &&
                   speedLevel >= maxLevel &&
                   agilityLevel >= maxLevel;
        }
    }
}
