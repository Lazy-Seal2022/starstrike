using NUnit.Framework;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Tests.EditMode
{
    public class ProgressionTests
    {
        [Test]
        public void CalculateCost_MatchesJSFormula()
        {
            // JS formula: Math.floor(baseCost * Math.pow(costMultiplier, level))
            int baseCost = 10;
            float costMultiplier = 1.6f;

            Assert.AreEqual(10, ProgressionMath.CalculateCost(baseCost, costMultiplier, 0));
            Assert.AreEqual(16, ProgressionMath.CalculateCost(baseCost, costMultiplier, 1)); // 10 * 1.6 = 16
            Assert.AreEqual(25, ProgressionMath.CalculateCost(baseCost, costMultiplier, 2)); // 10 * 2.56 = 25.6 -> 25
            Assert.AreEqual(40, ProgressionMath.CalculateCost(baseCost, costMultiplier, 3)); // 10 * 4.096 = 40.96 -> 40
        }

        [Test]
        public void CalculateStat_MatchesJSFormula()
        {
            // JS formula: base * Math.pow(multiplier, level)
            float baseStat = 100f;
            float multiplier = 1.25f;

            Assert.AreEqual(100f, ProgressionMath.CalculateStat(baseStat, multiplier, 0), 0.01f);
            Assert.AreEqual(125f, ProgressionMath.CalculateStat(baseStat, multiplier, 1), 0.01f);
            Assert.AreEqual(156.25f, ProgressionMath.CalculateStat(baseStat, multiplier, 2), 0.01f);
        }

        [Test]
        public void UpgradeState_EvolutionRule_RequiresAllMaxed()
        {
            var state = new UpgradeState();
            int maxLevel = 4;

            // Start at 0
            Assert.IsFalse(state.AreAllStatsMaxed(maxLevel));

            // Max almost everything
            state.shieldCapacityLevel = maxLevel;
            state.shieldRegenLevel = maxLevel;
            state.energyCapacityLevel = maxLevel;
            state.energyRegenLevel = maxLevel;
            state.pulseDamageLevel = maxLevel;
            state.pulseSpeedLevel = maxLevel;
            state.speedLevel = maxLevel;
            
            // Still false because agility is 0
            Assert.IsFalse(state.AreAllStatsMaxed(maxLevel));

            // Max agility
            state.agilityLevel = maxLevel;
            Assert.IsTrue(state.AreAllStatsMaxed(maxLevel));
        }
    }
}
