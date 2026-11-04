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
            Initialize();
        }

        public void Initialize()
        {
            Instance = this;
            if (currentShip == null)
            {
                currentShip = Resources.Load<ShipDefinition>("Data/Ship_Scout");
            }
            if (currentShip == null || currentShip.evolvesTo == null || currentShip.evolvesTo.Length == 0)
            {
                currentShip = CreateDefaultShipTree();
            }

            if (gameConfig == null)
            {
                gameConfig = Resources.Load<GameConfig>("Data/GameConfig");
            }
            if (gameConfig == null)
            {
                gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            }

            if (health == null) health = GetComponent<ShipHealth>();
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (weapon == null) weapon = GetComponent<ShipWeapon>();

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            if (col != null && col.radius <= 0.01f)
            {
                col.radius = 0.5f;
            }
            col.isTrigger = false;

            if (shipRenderer == null)
            {
                shipRenderer = GetComponent<SpriteRenderer>();
                if (shipRenderer == null) shipRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            shipRenderer.sortingOrder = 10;

            baseShip = currentShip;
            ApplyShipVisuals();
        }

        private void Start()
        {
            if (health == null) health = GetComponent<ShipHealth>();
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (weapon == null) weapon = GetComponent<ShipWeapon>();

            ApplyCurrentStats();
            ApplyShipVisuals();
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

            ApplyShipVisuals();

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

        public static ShipDefinition CreateDefaultShipTree()
        {
            var scout = ScriptableObject.CreateInstance<ShipDefinition>();
            scout.id = "scout";
            scout.shipName = "Delta Scout";
            scout.tier = 1;
            scout.maxUpgradeLevel = 4;
            scout.weaponSlots = 1;
            scout.baseStats = new StatValues { shieldCapacity = 100, shieldRegen = 8, energyCapacity = 80, energyRegen = 18, pulseDamage = 18, pulseSpeed = 600, speed = 260, agility = 4.5f };

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            fighter.id = "fighter";
            fighter.shipName = "Ares Fighter";
            fighter.tier = 2;
            fighter.maxUpgradeLevel = 5;
            fighter.weaponSlots = 2;
            fighter.baseStats = new StatValues { shieldCapacity = 150, shieldRegen = 12, energyCapacity = 120, energyRegen = 24, pulseDamage = 28, pulseSpeed = 680, speed = 310, agility = 5.0f };

            var miner = ScriptableObject.CreateInstance<ShipDefinition>();
            miner.id = "miner";
            miner.shipName = "Goliath Miner";
            miner.tier = 2;
            miner.maxUpgradeLevel = 5;
            miner.weaponSlots = 2;
            miner.baseStats = new StatValues { shieldCapacity = 220, shieldRegen = 16, energyCapacity = 150, energyRegen = 20, pulseDamage = 40, pulseSpeed = 520, speed = 210, agility = 3.5f };

            var interceptor = ScriptableObject.CreateInstance<ShipDefinition>();
            interceptor.id = "interceptor";
            interceptor.shipName = "Viper Interceptor";
            interceptor.tier = 3;
            interceptor.maxUpgradeLevel = 6;
            interceptor.weaponSlots = 2;
            interceptor.baseStats = new StatValues { shieldCapacity = 180, shieldRegen = 15, energyCapacity = 140, energyRegen = 30, pulseDamage = 35, pulseSpeed = 800, speed = 400, agility = 6.5f };

            var juggernaut = ScriptableObject.CreateInstance<ShipDefinition>();
            juggernaut.id = "juggernaut";
            juggernaut.shipName = "Titan Juggernaut";
            juggernaut.tier = 3;
            juggernaut.maxUpgradeLevel = 6;
            juggernaut.weaponSlots = 3;
            juggernaut.baseStats = new StatValues { shieldCapacity = 400, shieldRegen = 25, energyCapacity = 200, energyRegen = 25, pulseDamage = 75, pulseSpeed = 480, speed = 180, agility = 2.5f };

            scout.evolvesTo = new ShipDefinition[] { fighter, miner };
            fighter.evolvesTo = new ShipDefinition[] { interceptor };
            miner.evolvesTo = new ShipDefinition[] { juggernaut };
            interceptor.evolvesTo = new ShipDefinition[0];
            juggernaut.evolvesTo = new ShipDefinition[0];

            return scout;
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
            if (Instance == this) Instance = null;
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
            GameEvents.TriggerGemCollected(amount, gems);
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

            int baseCost = gameConfig != null ? gameConfig.upgradeBaseCost : 10;
            float multiplier = gameConfig != null ? gameConfig.upgradeCostMultiplier : 1.5f;
            return ProgressionMath.CalculateCost(baseCost, multiplier, lvl);
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
            ApplyShipVisuals();

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
                float spd = currentStats.speed > 35f ? currentStats.speed / 26f : currentStats.speed;
                if (spd <= 0f) spd = 10f;
                movement.maxSpeed = spd;
                movement.acceleration = spd * 1.8f;

                float agl = currentStats.agility > 20f ? currentStats.agility / 30f : currentStats.agility;
                if (agl <= 0f) agl = 6f;
                movement.agility = agl;
            }

            if (weapon != null)
            {
                weapon.pulseDamage = currentStats.pulseDamage > 0 ? currentStats.pulseDamage : 20f;
                float pSpd = currentStats.pulseSpeed > 35f ? currentStats.pulseSpeed / 26f : currentStats.pulseSpeed;
                if (pSpd <= 0f) pSpd = 22f;
                weapon.pulseSpeed = pSpd;
                weapon.weaponSlots = currentShip.weaponSlots > 0 ? currentShip.weaponSlots : 1;
            }
        }

        public void ApplyShipVisuals()
        {
            if (shipRenderer == null)
            {
                shipRenderer = GetComponent<SpriteRenderer>();
                if (shipRenderer == null) shipRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            shipRenderer.sortingOrder = 10;
            string shipId = currentShip != null && !string.IsNullOrEmpty(currentShip.id) ? currentShip.id : "scout";
            shipRenderer.sprite = ProceduralSpriteHelper.GetShipSprite(shipId);

            if (shipId == "fighter")
                transform.localScale = Vector3.one * 1.25f;
            else if (shipId == "miner")
                transform.localScale = Vector3.one * 1.35f;
            else if (shipId == "interceptor")
                transform.localScale = Vector3.one * 1.2f;
            else if (shipId == "juggernaut")
                transform.localScale = Vector3.one * 1.6f;
            else
                transform.localScale = Vector3.one * 1.0f;
        }
    }
}
