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
