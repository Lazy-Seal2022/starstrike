using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class PlayerProgression : MonoBehaviour
    {
        public static PlayerProgression Instance { get; private set; }
        private void Awake() { Instance = this; }
        [Header("Configuration")]
        public ShipDefinition currentShip;
        public GameConfig gameConfig;
        
        [Header("State")]
        public UpgradeState upgradeState = new UpgradeState();
        public int gems = 0;

        [Header("Component References")]
        public ShipHealth health;
        public ShipMovement movement;
        public ShipWeapon weapon;
        public SpriteRenderer shipRenderer;

        private void Start()
        {
            ApplyCurrentStats();
        }

        public void AddGems(int amount)
        {
            gems += amount;
            GameEvents.TriggerGemCollected(0, gems);
        }

        public bool UpgradeStat(string statName)
        {
            int cost = GetUpgradeCost(statName);
            if (gems < cost) return false;

            gems -= cost;

            if (statName == "shieldCap") upgradeState.shieldCapacityLevel++;
            else if (statName == "shieldRegen") upgradeState.shieldRegenLevel++;
            else if (statName == "energyCap") upgradeState.energyCapacityLevel++;
            else if (statName == "energyRegen") upgradeState.energyRegenLevel++;
            else if (statName == "damage") upgradeState.pulseDamageLevel++;
            else if (statName == "pulseSpeed") upgradeState.pulseSpeedLevel++;
            else if (statName == "speed") upgradeState.speedLevel++;
            else if (statName == "agility") upgradeState.agilityLevel++;

            GameEvents.TriggerGemCollected(0, gems);
            ApplyCurrentStats();
            
            // Keep current health proportional or just let it regenerate? The JS baseline keeps it proportional, 
            // but for now we just let it regen from current value as before.
            
            return true;
        }

        public int GetUpgradeCost(string statName)
        {
            int lvl = 0;
            if (statName == "shieldCap") lvl = upgradeState.shieldCapacityLevel;
            else if (statName == "shieldRegen") lvl = upgradeState.shieldRegenLevel;
            else if (statName == "energyCap") lvl = upgradeState.energyCapacityLevel;
            else if (statName == "energyRegen") lvl = upgradeState.energyRegenLevel;
            else if (statName == "damage") lvl = upgradeState.pulseDamageLevel;
            else if (statName == "pulseSpeed") lvl = upgradeState.pulseSpeedLevel;
            else if (statName == "speed") lvl = upgradeState.speedLevel;
            else if (statName == "agility") lvl = upgradeState.agilityLevel;

            return ProgressionMath.CalculateCost(gameConfig.upgradeBaseCost, gameConfig.upgradeCostMultiplier, lvl);
        }

        public bool CanEvolve()
        {
            return currentShip != null && currentShip.evolvesTo != null && currentShip.evolvesTo.Length > 0 && upgradeState.AreAllStatsMaxed(currentShip.maxUpgradeLevel);
        }

        public void Evolve(ShipDefinition targetShip)
        {
            currentShip = targetShip;
            upgradeState.Reset();
            ApplyCurrentStats();
            health.ResetMeters();

            if (shipRenderer != null && currentShip.sprite != null)
            {
                shipRenderer.sprite = currentShip.sprite;
                shipRenderer.color = currentShip.color;
                shipRenderer.transform.localScale = Vector3.one * currentShip.scale;
            }

            GameEvents.TriggerPlayerEvolved(currentShip.id);
        }

        private void ApplyCurrentStats()
        {
            if (currentShip == null || gameConfig == null) return;

            StatValues currentStats = ProgressionMath.GetCurrentStats(currentShip, gameConfig.upgradeDefinition, upgradeState);

            if (health != null)
            {
                health.maxShield = currentStats.shieldCapacity;
                health.shieldRegen = currentStats.shieldRegen;
                health.maxEnergy = currentStats.energyCapacity;
                health.energyRegen = currentStats.energyRegen;
            }

            if (movement != null)
            {
                // In Phase 1 JS we saw speed 260. We might need a physics conversion factor here later.
                // For now, if currentStats.speed is huge, we divide it to match old Unity scale (e.g. 260 -> 10 = divide by 26)
                // Let's just pass it directly and adjust Physics later if needed.
                movement.maxSpeed = currentStats.speed / 26f;
                movement.acceleration = currentStats.speed / 26f * 1.8f; 
                movement.agility = currentStats.agility;
            }

            if (weapon != null)
            {
                weapon.pulseDamage = currentStats.pulseDamage;
                weapon.pulseSpeed = currentStats.pulseSpeed / 26f;
                weapon.weaponSlots = currentShip.weaponSlots;
            }
