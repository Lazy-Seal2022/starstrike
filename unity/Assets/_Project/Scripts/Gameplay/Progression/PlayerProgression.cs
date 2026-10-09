using UnityEngine;
using StarStrike.Core;
using System.Collections;

namespace StarStrike.Gameplay
{
    public class PlayerProgression : MonoBehaviour
    {
        public static PlayerProgression Instance { get; private set; }

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

        private ShipDefinition baseShip;

        private void Awake()
        {
            Instance = this;
            baseShip = currentShip;
        }

        private void Start()
        {
            ApplyCurrentStats();
            if (health != null) health.OnDied += HandleDeath;

            // Add Minimap Trackable
            var trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.Type = StarStrike.Gameplay.TrackableType.Player;
        }

        public void ResetForNewRun()
        {
            gems = 0;
            upgradeState.Reset();
            
            string selectedShipId = StarStrike.Platform.Storage.StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(selectedShipId)) selectedShipId = "scout";

            ShipDefinition selectedDef = FindShipById(baseShip, selectedShipId);
            if (selectedDef != null)
            {
                currentShip = selectedDef;
            }
            
            ApplyCurrentStats();
            if (health != null) health.ResetMeters();

            if (shipRenderer != null && currentShip.sprite != null)
            {
                shipRenderer.sprite = currentShip.sprite;
                shipRenderer.color = currentShip.color;
                shipRenderer.transform.localScale = Vector3.one * currentShip.scale;
            }

            transform.position = Vector3.zero;
            if (movement != null)
            {
                var rb = movement.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
            
            if (shipRenderer != null) shipRenderer.enabled = true;
            if (movement != null) movement.enabled = true;
            if (weapon != null) weapon.enabled = true;

            GameEvents.TriggerPlayerEvolved(currentShip.id);
            GameEvents.TriggerGemCollected(0, gems);
        }

        private ShipDefinition FindShipById(ShipDefinition root, string id, System.Collections.Generic.HashSet<string> visited = null)
        {
            if (visited == null) visited = new System.Collections.Generic.HashSet<string>();
            if (root == null || visited.Contains(root.id)) return null;
            visited.Add(root.id);

            if (root.id == id) return root;
            if (root.evolvesTo != null)
            {
                foreach (var child in root.evolvesTo)
                {
                    var found = FindShipById(child, id, visited);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDied -= HandleDeath;
        }

        private void HandleDeath()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            if (shipRenderer != null) shipRenderer.enabled = false;
            if (movement != null) movement.enabled = false;
            if (weapon != null) weapon.enabled = false;
            
            // TODO: Spawn explosion particles here
            if (PoolManager.Instance != null) {
                // PoolManager.Instance.Spawn("Explosion", transform.position, Quaternion.identity, null);
            }

            yield return new WaitForSeconds(1.5f);
            
            if (shipRenderer != null) shipRenderer.enabled = true;
            if (movement != null) movement.enabled = true;
            if (weapon != null) weapon.enabled = true;
            
            transform.position = Vector3.zero;
            if (movement != null) {
                var rb = movement.GetComponent<Rigidbody2D>();
                if (rb != null) {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }
            health.ResetMeters();
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

        public void EvolveToId(string targetId)
        {
            if (currentShip == null || currentShip.evolvesTo == null) return;
            foreach (var ship in currentShip.evolvesTo)
            {
                if (ship.id == targetId)
                {
                    Evolve(ship);
                    return;
                }
            }
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
        }
    }
}
