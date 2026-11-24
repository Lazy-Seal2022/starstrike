using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay.Audio;
using StarStrike.Platform.Storage;
using System.Collections;

namespace StarStrike.Gameplay
{
    public class PlayerProgression : MonoBehaviour
    {
        private static PlayerProgression _instance;
        public static PlayerProgression Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<PlayerProgression>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

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
        private Material emissiveMaterial;
        private Material defaultSpriteMaterial;

        private void Awake()
        {
            Initialize();
        }

        public static ShipDefinition GetShipDefinitionById(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "scout";
            id = id.ToLowerInvariant();

            string assetName = "Ship_Scout";
            switch (id)
            {
                case "scout": assetName = "Ship_Scout"; break;
                case "fighter": assetName = "Ship_Fighter"; break;
                case "miner": assetName = "Ship_Miner"; break;
                case "interceptor": assetName = "Ship_Interceptor"; break;
                case "juggernaut": assetName = "Ship_Juggernaut"; break;
                case "carrier":
                case "vanguard": assetName = "Ship_Carrier"; break;
                case "phantom": assetName = "Ship_Phantom"; break;
                case "frigate": assetName = "Ship_Frigate"; break;
                case "destroyer": assetName = "Ship_Destroyer"; break;
                case "cruiser": assetName = "Ship_Cruiser"; break;
                case "battleship": assetName = "Ship_Battleship"; break;
                default:
                    string cap = char.ToUpper(id[0]) + id.Substring(1);
                    assetName = $"Ship_{cap}";
                    break;
            }

            var def = Resources.Load<ShipDefinition>($"Data/{assetName}");
            if (def == null && assetName == "Ship_Phantom")
            {
                def = Resources.Load<ShipDefinition>("Data/Ship_Interceptor");
            }
            if (def == null)
            {
                def = Resources.Load<ShipDefinition>("Data/Ship_Scout");
            }
            if (def != null && def.sprite == null)
            {
                def.sprite = ProceduralSpriteHelper.GetShipSprite(id);
            }
            return def;
        }

        public void Initialize()
        {
            Instance = this;
            if (baseShip == null)
            {
                baseShip = Resources.Load<ShipDefinition>("Data/Ship_Scout");
                if (baseShip == null)
                {
                    baseShip = CreateDefaultShipTree();
                }
            }

            if (currentShip == null)
            {
                string chosenShipId = StorageService.GetLastShipEvolved();
                if (string.IsNullOrEmpty(chosenShipId)) chosenShipId = "scout";
                currentShip = GetShipDefinitionById(chosenShipId) ?? baseShip;
            }
            if (currentShip == null)
            {
                currentShip = baseShip;
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

            PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
            if (poly == null)
            {
                var circleCol = GetComponent<CircleCollider2D>();
                if (circleCol != null && Application.isPlaying)
                {
                    Destroy(circleCol);
                }
                poly = gameObject.AddComponent<PolygonCollider2D>();
            }
            poly.isTrigger = false;

            if (shipRenderer == null)
            {
                shipRenderer = GetComponent<SpriteRenderer>();
                if (shipRenderer == null) shipRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            shipRenderer.sortingOrder = 10;

            ApplyShipVisuals();
        }

        private void Start()
        {
            if (health == null) health = GetComponent<ShipHealth>();
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (weapon == null) weapon = GetComponent<ShipWeapon>();

            ApplyCurrentStats();
            ApplyShipVisuals();
            var weaponCtrl = GetComponent<ShipWeaponController>();
            if (weaponCtrl != null)
            {
                weaponCtrl.EquipFromStorage(currentShip != null ? currentShip.id : "scout");
            }
            if (health != null) health.OnDied += HandleDeath;

            // Add Minimap Trackable
            var trackable = GetComponent<StarStrike.Gameplay.MinimapTrackable>();
            if (trackable == null) trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.SetType(StarStrike.Gameplay.TrackableType.Player);
        }

        public void ResetForNewRun()
        {
            gems = 0;
            upgradeState.Reset();
            
            string selectedShipId = StarStrike.Platform.Storage.StorageService.GetLastShipEvolved();
            if (string.IsNullOrEmpty(selectedShipId)) selectedShipId = "scout";

            ShipDefinition selectedDef = GetShipDefinitionById(selectedShipId);
            if (selectedDef == null)
            {
                selectedDef = FindShipById(baseShip, selectedShipId);
            }
            if (selectedDef != null)
            {
                currentShip = selectedDef;
            }
            else if (currentShip == null)
            {
                currentShip = baseShip;
            }
            
            ApplyCurrentStats();
            if (health != null) health.ResetMeters();

            ApplyShipVisuals();

            var weaponCtrl = GetComponent<ShipWeaponController>();
            if (weaponCtrl != null)
            {
                weaponCtrl.EquipFromStorage(currentShip != null ? currentShip.id : "scout");
            }

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            if (movement != null)
            {
                var rb = movement.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = Vector2.zero;
                    rb.rotation = 90f;
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
            scout.hitboxRadius = 0.55f;
            scout.weaponSockets = new Vector2[] { new Vector2(1.55f, 0f) };
            scout.thrusterSockets = new Vector2[] { new Vector2(-1.55f, 0f) };
            scout.baseStats = new StatValues { hullCapacity = 80, shieldCapacity = 100, shieldRegen = 8, energyCapacity = 80, energyRegen = 18, pulseDamage = 18, pulseSpeed = 600, speed = 260, agility = 4.5f };

            var fighter = ScriptableObject.CreateInstance<ShipDefinition>();
            fighter.id = "fighter";
            fighter.shipName = "Ares Fighter";
            fighter.tier = 2;
            fighter.maxUpgradeLevel = 5;
            fighter.weaponSlots = 2;
            fighter.hitboxRadius = 0.55f;
            fighter.weaponSockets = new Vector2[] { new Vector2(0.35f, 1.25f), new Vector2(0.35f, -1.25f) };
            fighter.thrusterSockets = new Vector2[] { new Vector2(-1.5f, 0.5f), new Vector2(-1.5f, -0.5f) };
            fighter.baseStats = new StatValues { hullCapacity = 120, shieldCapacity = 150, shieldRegen = 12, energyCapacity = 120, energyRegen = 24, pulseDamage = 28, pulseSpeed = 680, speed = 310, agility = 5.0f };

            var miner = ScriptableObject.CreateInstance<ShipDefinition>();
            miner.id = "miner";
            miner.shipName = "Goliath Miner";
            miner.tier = 2;
            miner.maxUpgradeLevel = 5;
            miner.weaponSlots = 2;
            miner.hitboxRadius = 0.65f;
            miner.weaponSockets = new Vector2[] { new Vector2(1.25f, 0.85f), new Vector2(1.25f, -0.85f) };
            miner.thrusterSockets = new Vector2[] { new Vector2(-1.4f, 0.4f), new Vector2(-1.4f, -0.4f) };
            miner.baseStats = new StatValues { hullCapacity = 180, shieldCapacity = 220, shieldRegen = 16, energyCapacity = 150, energyRegen = 20, pulseDamage = 40, pulseSpeed = 520, speed = 210, agility = 3.5f };

            var interceptor = ScriptableObject.CreateInstance<ShipDefinition>();
            interceptor.id = "interceptor";
            interceptor.shipName = "Viper Interceptor";
            interceptor.tier = 3;
            interceptor.maxUpgradeLevel = 6;
            interceptor.weaponSlots = 2;
            interceptor.hitboxRadius = 0.50f;
            interceptor.weaponSockets = new Vector2[] { new Vector2(0.45f, 1.15f), new Vector2(0.45f, -1.15f) };
            interceptor.thrusterSockets = new Vector2[] { new Vector2(-1.5f, 0f) };
            interceptor.baseStats = new StatValues { hullCapacity = 100, shieldCapacity = 180, shieldRegen = 15, energyCapacity = 140, energyRegen = 30, pulseDamage = 35, pulseSpeed = 800, speed = 400, agility = 6.5f };

            var juggernaut = ScriptableObject.CreateInstance<ShipDefinition>();
            juggernaut.id = "juggernaut";
            juggernaut.shipName = "Titan Juggernaut";
            juggernaut.tier = 3;
            juggernaut.maxUpgradeLevel = 6;
            juggernaut.weaponSlots = 3;
            juggernaut.hitboxRadius = 0.85f;
            juggernaut.shieldHitboxOffset = 0.30f;
            juggernaut.weaponSockets = new Vector2[] {
                new Vector2(2.75f, 0f),      // Central spinal cannon
                new Vector2(0.70f, 1.50f),   // Port sponson turret
                new Vector2(0.70f, -1.50f)   // Starboard sponson turret
            };
            juggernaut.thrusterSockets = new Vector2[] {
                new Vector2(-2.75f, 0.85f),
                new Vector2(-2.75f, 0.30f),
                new Vector2(-2.75f, -0.30f),
                new Vector2(-2.75f, -0.85f)
            };
            juggernaut.baseStats = new StatValues { hullCapacity = 300, shieldCapacity = 400, shieldRegen = 25, energyCapacity = 200, energyRegen = 25, pulseDamage = 75, pulseSpeed = 480, speed = 180, agility = 2.5f };

            var phantom = ScriptableObject.CreateInstance<ShipDefinition>();
            phantom.id = "phantom";
            phantom.shipName = "Shadow Phantom";
            phantom.tier = 3;
            phantom.maxUpgradeLevel = 6;
            phantom.weaponSlots = 2;
            phantom.hitboxRadius = 0.50f;
            phantom.weaponSockets = new Vector2[] { new Vector2(0.50f, 0.80f), new Vector2(0.50f, -0.80f) };
            phantom.thrusterSockets = new Vector2[] { new Vector2(-1.4f, 0f) };
            phantom.baseStats = new StatValues { hullCapacity = 110, shieldCapacity = 160, shieldRegen = 16, energyCapacity = 160, energyRegen = 35, pulseDamage = 45, pulseSpeed = 850, speed = 420, agility = 7.0f };

            var vanguard = ScriptableObject.CreateInstance<ShipDefinition>();
            vanguard.id = "vanguard";
            vanguard.shipName = "Vanguard Carrier";
            vanguard.tier = 3;
            vanguard.shipClass = ShipClass.Carrier;
            vanguard.maxUpgradeLevel = 6;
            vanguard.weaponSlots = 3;
            vanguard.hitboxRadius = 0.80f;
            vanguard.shieldHitboxOffset = 0.30f;
            vanguard.canAimIndependently = true;
            vanguard.turretTraverseSpeed = 130f;
            vanguard.droneHangarCapacity = 4;
            vanguard.defaultDroneType = "interceptor_drone";
            vanguard.collisionArmorReduction = 0.25f;
            vanguard.cameraOrthoSize = 13f;
            vanguard.hullMass = 6f;
            vanguard.weaponSockets = new Vector2[] {
                new Vector2(2.50f, 0f),
                new Vector2(2.75f, 1.12f),
                new Vector2(2.75f, -1.12f)
            };
            vanguard.thrusterSockets = new Vector2[] {
                new Vector2(-2.80f, 1.0f),
                new Vector2(-2.80f, -1.0f),
                new Vector2(-2.90f, 0f)
            };
            vanguard.baseStats = new StatValues { hullCapacity = 400, shieldCapacity = 280, shieldRegen = 18, energyCapacity = 220, energyRegen = 25, pulseDamage = 30, pulseSpeed = 560, speed = 160, agility = 2.6f };

            scout.evolvesTo = new ShipDefinition[] { fighter, miner };
            fighter.evolvesTo = new ShipDefinition[] { interceptor, phantom };
            miner.evolvesTo = new ShipDefinition[] { juggernaut, vanguard };
            interceptor.evolvesTo = new ShipDefinition[0];
            juggernaut.evolvesTo = new ShipDefinition[0];
            phantom.evolvesTo = new ShipDefinition[0];
            vanguard.evolvesTo = new ShipDefinition[0];

            scout.sprite = Resources.Load<ShipDefinition>("Data/Ship_Scout")?.sprite;
            fighter.sprite = Resources.Load<ShipDefinition>("Data/Ship_Fighter")?.sprite;
            miner.sprite = Resources.Load<ShipDefinition>("Data/Ship_Miner")?.sprite;
            interceptor.sprite = Resources.Load<ShipDefinition>("Data/Ship_Interceptor")?.sprite;
            juggernaut.sprite = Resources.Load<ShipDefinition>("Data/Ship_Juggernaut")?.sprite;
            vanguard.sprite = Resources.Load<ShipDefinition>("Data/Ship_Carrier")?.sprite;
            phantom.sprite = interceptor.sprite;

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

        [Header("Game Flow Options")]
        public bool triggerGameOverOnDeath = false;

        private void HandleDeath()
        {
            if (triggerGameOverOnDeath && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                if (shipRenderer != null) shipRenderer.enabled = false;
                if (movement != null) movement.enabled = false;
                if (weapon != null) weapon.enabled = false;
                GameManager.Instance.GameOver();
            }
            else
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            if (shipRenderer != null) shipRenderer.enabled = false;
            if (movement != null) movement.enabled = false;
            if (weapon != null) weapon.enabled = false;
            
            GameEvents.TriggerAsteroidDestroyed(transform.position, "Large");
            AudioService.Instance?.PlaySound("explosion", 0.8f);

            yield return new WaitForSeconds(1.5f);
            
            if (shipRenderer != null) shipRenderer.enabled = true;
            if (movement != null) movement.enabled = true;
            if (weapon != null) weapon.enabled = true;
            
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            if (movement != null) {
                var rb = movement.GetComponent<Rigidbody2D>();
                if (rb != null) {
                    rb.position = Vector2.zero;
                    rb.rotation = 90f;
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

        public int GetMaxLevel()
        {
            return currentShip != null ? currentShip.maxUpgradeLevel : 5;
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

            var weaponCtrl = GetComponent<ShipWeaponController>();
            if (weaponCtrl != null)
            {
                weaponCtrl.EquipFromStorage(currentShip != null ? currentShip.id : "scout");
            }

            GameEvents.TriggerPlayerEvolved(currentShip.id);
        }

        public void EvolveTo(ShipDefinition targetShip)
        {
            Evolve(targetShip);
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

            // Calculate installed equipment module stat bonuses
            float bonusShield = 0f;
            float bonusShieldRegen = 0f;
            float bonusHull = 0f;
            float bonusEnergy = 0f;
            float bonusEnergyRegen = 0f;
            float bonusSpeed = 0f;
            float bonusAgility = 0f;
            float damageMultiplier = 1.0f;
            float bonusArmorReduction = 0f;

            string shipId = !string.IsNullOrEmpty(currentShip.id) ? currentShip.id : "scout";
            var equippedModules = StorageService.GetAllEquippedModules(shipId);
            foreach (var kvp in equippedModules)
            {
                var mod = EquipmentModuleCatalog.GetModule(kvp.Value);
                if (mod != null)
                {
                    bonusShield += mod.shieldCapacityBonus;
                    bonusShieldRegen += mod.shieldRegenBonus;
                    bonusHull += mod.hullCapacityBonus;
                    bonusEnergy += mod.energyCapacityBonus;
                    bonusEnergyRegen += mod.energyRegenBonus;
                    bonusSpeed += mod.speedBonus;
                    bonusAgility += mod.agilityBonus;
                    damageMultiplier *= mod.damageMultiplier;
                    bonusArmorReduction += mod.collisionArmorBonus;
                }
            }

            if (health != null)
            {
                health.maxShield = currentStats.shieldCapacity + bonusShield;
                health.shieldRegen = currentStats.shieldRegen + bonusShieldRegen;
                health.maxHull = (currentStats.hullCapacity > 0 ? currentStats.hullCapacity : 80f) + bonusHull;
                health.maxEnergy = currentStats.energyCapacity + bonusEnergy;
                health.energyRegen = currentStats.energyRegen + bonusEnergyRegen;
                health.armorDamageReduction = Mathf.Clamp(currentShip.collisionArmorReduction + bonusArmorReduction, 0f, 0.75f);
            }

            if (movement != null)
            {
                float spd = currentStats.speed > 35f ? currentStats.speed / 26f : currentStats.speed;
                if (spd <= 0f) spd = 10f;
                movement.maxSpeed = spd + (bonusSpeed > 0f ? bonusSpeed / 26f : 0f);
                movement.acceleration = movement.maxSpeed * 1.8f;

                float agl = currentStats.agility > 20f ? currentStats.agility / 30f : currentStats.agility;
                if (agl <= 0f) agl = 6f;
                movement.agility = agl + (bonusAgility > 0f ? bonusAgility / 30f : 0f);

                bool isIndependentAimShip = currentShip != null && (currentShip.canAimIndependently || currentShip.id == "battleship" || currentShip.id == "cruiser" || currentShip.id == "carrier" || currentShip.id == "vanguard" || currentShip.shipClass == ShipClass.Battleship || currentShip.shipClass == ShipClass.Cruiser || currentShip.shipClass == ShipClass.Carrier);
                movement.steerTowardsAim = !isIndependentAimShip;
            }

            if (weapon != null)
            {
                float baseDmg = currentStats.pulseDamage > 0 ? currentStats.pulseDamage : 20f;
                weapon.pulseDamage = baseDmg * damageMultiplier;
                float pSpd = currentStats.pulseSpeed > 35f ? currentStats.pulseSpeed / 26f : currentStats.pulseSpeed;
                if (pSpd <= 0f) pSpd = 22f;
                weapon.pulseSpeed = pSpd;
                weapon.weaponSlots = currentShip.weaponSlots > 0 ? currentShip.weaponSlots : 1;
                weapon.fireRate = weapon.combatConfig != null ? weapon.combatConfig.defaultFireRate : 0.22f;
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
            Sprite targetSprite = currentShip != null ? currentShip.sprite : null;
            if (targetSprite == null)
            {
                var def = GetShipDefinitionById(shipId);
                if (def != null) targetSprite = def.sprite;
            }
            if (targetSprite == null)
            {
                targetSprite = ProceduralSpriteHelper.GetShipSprite(shipId);
            }

            if (targetSprite != null)
            {
                shipRenderer.sprite = targetSprite;
                if (currentShip != null && currentShip.sprite == null)
                {
                    currentShip.sprite = targetSprite;
                }
            }

            // Emissive glow setup (Feature #6): Apply emissive shader if ship definition has emissive texture
            Texture2D emissiveTex = currentShip != null ? currentShip.emissiveTexture : null;
            if (emissiveTex == null)
            {
                var def = GetShipDefinitionById(shipId);
                if (def != null) emissiveTex = def.emissiveTexture;
            }

            if (emissiveTex != null && shipRenderer != null)
            {
                var shader = Shader.Find("StarStrike/Sprite-Emissive");
                if (shader != null)
                {
                    if (emissiveMaterial == null || emissiveMaterial.shader != shader)
                    {
                        emissiveMaterial = new Material(shader);
                    }
                    emissiveMaterial.SetTexture("_EmissiveTex", emissiveTex);
                    Color emCol = currentShip != null && currentShip.emissiveColor.a > 0.01f ? currentShip.emissiveColor : new Color(0.25f, 0.9f, 1f, 1f);
                    float emInt = currentShip != null && currentShip.emissiveIntensity > 0.01f ? currentShip.emissiveIntensity : 1.8f;
                    emissiveMaterial.SetColor("_EmissiveColor", emCol);
                    emissiveMaterial.SetFloat("_EmissiveIntensity", emInt);
                    shipRenderer.sharedMaterial = emissiveMaterial;
                }
            }
            else if (shipRenderer != null)
            {
                var defaultShader = Shader.Find("Sprites/Default");
                if (defaultShader != null)
                {
                    if (defaultSpriteMaterial == null) defaultSpriteMaterial = new Material(defaultShader);
                    shipRenderer.sharedMaterial = defaultSpriteMaterial;
                }
            }

            if (shipId == "fighter")
                transform.localScale = Vector3.one * 1.25f;
            else if (shipId == "miner")
                transform.localScale = Vector3.one * 1.35f;
            else if (shipId == "interceptor")
                transform.localScale = Vector3.one * 1.2f;
            else if (shipId == "juggernaut")
                transform.localScale = Vector3.one * 1.6f;
            else if (shipId == "vanguard" || shipId == "carrier")
                transform.localScale = Vector3.one * 1.5f;
            else if (shipId == "frigate")
                transform.localScale = Vector3.one * 1.3f;
            else if (shipId == "destroyer")
                transform.localScale = Vector3.one * 1.45f;
            else if (shipId == "cruiser")
                transform.localScale = Vector3.one * 1.6f;
            else if (shipId == "battleship")
                transform.localScale = Vector3.one * 1.85f;
            else if (shipId == "phantom")
                transform.localScale = Vector3.one * 1.15f;
            else
                transform.localScale = Vector3.one * 1.0f;

            // Carrier Hangar & Drone Squadron integration
            bool isCarrier = currentShip != null && 
                (currentShip.id == "carrier" || currentShip.id == "vanguard" || currentShip.shipClass == ShipClass.Carrier || currentShip.droneHangarCapacity > 0);
            var hangar = GetComponent<StarStrike.Gameplay.Carrier.CarrierHangarController>();
            if (isCarrier)
            {
                if (hangar == null) hangar = gameObject.AddComponent<StarStrike.Gameplay.Carrier.CarrierHangarController>();
                hangar.enabled = true;
                hangar.droneCapacity = currentShip.droneHangarCapacity > 0 ? currentShip.droneHangarCapacity : 4;
                if (hangar.ActiveDroneCount == 0)
                {
                    hangar.SpawnInitialSquadron();
                }
            }
            else if (hangar != null)
            {
                hangar.ClearSquadron();
                hangar.enabled = false;
            }

            if (weapon == null) weapon = GetComponent<ShipWeapon>();
            if (weapon != null && currentShip != null)
            {
                weapon.weaponSlots = currentShip.weaponSlots > 0 ? currentShip.weaponSlots : 1;
                if (currentShip.weaponSockets != null && currentShip.weaponSockets.Length > 0)
                {
                    weapon.SetSockets(currentShip.weaponSockets);
                }
            }

            PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
            if (poly != null && shipRenderer != null && shipRenderer.sprite != null)
            {
                UpdatePolygonFromSprite(poly, shipRenderer.sprite);
            }

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null && currentShip != null && currentShip.hitboxRadius > 0.05f)
            {
                col.radius = currentShip.hitboxRadius;
            }

            var shieldField = GetComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            if (shieldField != null)
            {
                if (currentShip != null && currentShip.shieldHitboxOffset > 0.05f)
                {
                    shieldField.shieldHitboxOffset = currentShip.shieldHitboxOffset;
                }
                else if (shipId == "juggernaut" || shipId == "vanguard" || shipId == "carrier")
                {
                    shieldField.shieldHitboxOffset = 0.30f;
                }
                else
                {
                    shieldField.shieldHitboxOffset = 0.20f;
                }
                shieldField.RebuildShieldCollider();
            }
        }

        private void Update()
        {
            // Feature #6: Emissive idle breathing glow
            if (emissiveMaterial != null && shipRenderer != null && shipRenderer.sharedMaterial == emissiveMaterial)
            {
                float baseInt = currentShip != null && currentShip.emissiveIntensity > 0.01f ? currentShip.emissiveIntensity : 1.8f;
                float breathe = 1f + 0.20f * Mathf.Sin(Time.time * 2.8f);
                emissiveMaterial.SetFloat("_EmissiveIntensity", baseInt * breathe);
            }
        }

        private static readonly System.Collections.Generic.List<Vector2> shapePointsBuffer = new System.Collections.Generic.List<Vector2>(64);

        public static void UpdatePolygonFromSprite(PolygonCollider2D poly, Sprite sprite)
        {
            if (poly == null || sprite == null) return;
            int shapeCount = sprite.GetPhysicsShapeCount();
            if (shapeCount > 0)
            {
                poly.pathCount = shapeCount;
                for (int i = 0; i < shapeCount; i++)
                {
                    shapePointsBuffer.Clear();
                    sprite.GetPhysicsShape(i, shapePointsBuffer);
                    poly.SetPath(i, shapePointsBuffer);
                }
            }
            else
            {
                Bounds b = sprite.bounds;
                Vector2[] fallback = new Vector2[]
                {
                    new Vector2(b.max.x, 0f),
                    new Vector2(b.min.x * 0.5f, b.max.y),
                    new Vector2(b.min.x, b.max.y * 0.7f),
                    new Vector2(b.min.x, b.min.y * 0.7f),
                    new Vector2(b.min.x * 0.5f, b.min.y)
                };
                poly.pathCount = 1;
                poly.SetPath(0, fallback);
            }
        }
    }
}
