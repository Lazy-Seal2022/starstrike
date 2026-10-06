using System;
using UnityEngine;
using UnityEngine.InputSystem;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class ShipController : MonoBehaviour
    {
        [Header("Ship Stats")]
        public ShipStats stats;
        public float currentShield;
        public float currentEnergy;
        public int gems = 0;

        [Header("Input State (Multiplatform)")]
        public bool inputThrust;
        public bool inputBrake;
        public bool inputBoost;
        public bool inputFire;
        public Vector2 inputAimDirection;
        public bool hasDirectAim = false;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private float lastFiredTime;

        // Upgrade Levels (0 to 5)
        public int lvlShieldCap = 0;
        public int lvlShieldRegen = 0;
        public int lvlEnergyCap = 0;
        public int lvlEnergyRegen = 0;
        public int lvlDamage = 0;
        public int lvlPulseSpeed = 0;
        public int lvlSpeed = 0;
        public int lvlAgility = 0;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.5f; // Newtonian space drag
            rb.angularDamping = 1.0f;
            tag = "Player";

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.radius = 0.45f;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 10;

            if (stats == null)
            {
                stats = ShipStats.CreateScout();
            }

            ApplyShipVisuals();
            ResetMeters();
        }

        private void Start()
        {
            GameEvents.TriggerShieldChanged(currentShield, stats.shieldCapacity);
            GameEvents.TriggerEnergyChanged(currentEnergy, stats.energyCapacity);
            GameEvents.TriggerGemCollected(0, gems);
        }

        private void ResetMeters()
        {
            currentShield = stats.shieldCapacity;
            currentEnergy = stats.energyCapacity;
        }

        public void ApplyShipVisuals()
        {
            spriteRenderer.sprite = ProceduralSpriteHelper.GetShipSprite(stats.shipId);
            if (stats.shipId == "fighter")
                transform.localScale = Vector3.one * 1.25f;
            else if (stats.shipId == "miner")
                transform.localScale = Vector3.one * 1.35f;
            else
                transform.localScale = Vector3.one * 1.0f;
        }

        private void Update()
        {
            ReadInputs();
            RegenerateMeters();

            if (inputFire)
            {
                TryFire();
            }
        }

        private void FixedUpdate()
        {
            HandleSteering();
            HandleThrust();
        }

        private void ReadInputs()
        {
            // Keyboard (New Input System)
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            Gamepad pad = Gamepad.current;

            bool keyW = kb != null && (kb.wKey.isPressed || kb.upArrowKey.isPressed);
            bool keyS = kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed);
            bool keyShift = kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed);
            bool keySpace = kb != null && kb.spaceKey.isPressed;

            bool mouseLeft = mouse != null && mouse.leftButton.isPressed;
            bool mouseRight = mouse != null && mouse.rightButton.isPressed;

            // Gamepad
            bool padThrust = pad != null && (pad.leftStick.y.ReadValue() > 0.2f || pad.rightTrigger.isPressed);
            bool padBrake = pad != null && pad.leftStick.y.ReadValue() < -0.2f;
            bool padBoost = pad != null && pad.buttonSouth.isPressed;
            bool padFire = pad != null && (pad.rightShoulder.isPressed || pad.buttonWest.isPressed);

            inputThrust = inputThrust || keyW || padThrust;
            inputBrake = inputBrake || keyS || padBrake;
            inputBoost = inputBoost || keyShift || mouseRight || padBoost;
            inputFire = inputFire || keySpace || mouseLeft || padFire;

            // Mouse Aim
            if (!hasDirectAim && mouse != null && Camera.main != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
                Vector2 diff = (Vector2)mouseWorld - (Vector2)transform.position;
                if (diff.sqrMagnitude > 0.05f)
                {
                    inputAimDirection = diff.normalized;
                }
            }
            else if (pad != null && pad.leftStick.ReadValue().sqrMagnitude > 0.1f)
            {
                inputAimDirection = pad.leftStick.ReadValue().normalized;
            }
        }

        private void HandleSteering()
        {
            if (inputAimDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(inputAimDirection.y, inputAimDirection.x) * Mathf.Rad2Deg;
                float currentAngle = rb.rotation;
                float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, stats.agility * 60f * Time.fixedDeltaTime);
                rb.MoveRotation(angle);
            }
        }

        private void HandleThrust()
        {
            float dt = Time.fixedDeltaTime;
            float accel = stats.acceleration;

            if (inputBoost && currentEnergy > 5f)
            {
                accel *= 1.8f;
                currentEnergy = Mathf.Max(0f, currentEnergy - 25f * dt);
                GameEvents.TriggerEnergyChanged(currentEnergy, stats.energyCapacity);
            }

            if (inputThrust)
            {
                Vector2 thrustDir = transform.right;
                rb.AddForce(thrustDir * accel * rb.mass);

                // Cap max velocity
                if (rb.linearVelocity.magnitude > stats.speed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * stats.speed;
                }
            }
            else if (inputBrake)
            {
                rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, accel * 0.4f * dt);
            }

            // Reset touch / frame triggers
            inputThrust = false;
            inputBrake = false;
            inputBoost = false;
            inputFire = false;
            hasDirectAim = false;
        }

        private void RegenerateMeters()
        {
            float dt = Time.deltaTime;

            if (currentShield < stats.shieldCapacity)
            {
                currentShield = Mathf.Min(stats.shieldCapacity, currentShield + stats.shieldRegen * dt);
                GameEvents.TriggerShieldChanged(currentShield, stats.shieldCapacity);
            }

            if (currentEnergy < stats.energyCapacity)
            {
                currentEnergy = Mathf.Min(stats.energyCapacity, currentEnergy + stats.energyRegen * dt);
                GameEvents.TriggerEnergyChanged(currentEnergy, stats.energyCapacity);
            }
        }

        public void TryFire()
        {
            float cost = 12f;
            float fireRate = 0.22f;

            if (Time.time - lastFiredTime < fireRate) return;
            if (currentEnergy < cost) return;

            currentEnergy -= cost;
            lastFiredTime = Time.time;
            GameEvents.TriggerEnergyChanged(currentEnergy, stats.energyCapacity);

            // Recoil
            rb.AddForce(-transform.right * 1.5f, ForceMode2D.Impulse);

            // Spawn Projectiles: single or dual barrel
            float[] offsets = stats.weaponSlots == 2 ? new float[] { -0.3f, 0.3f } : new float[] { 0f };

            foreach (float offset in offsets)
            {
                Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * 0.65f + (Vector2)transform.up * offset;
                GameObject laserObj = new GameObject("PlayerLaser");
                laserObj.tag = "Laser";
                laserObj.transform.position = nose;
                laserObj.transform.rotation = transform.rotation;

                LaserProjectile proj = laserObj.AddComponent<LaserProjectile>();
                proj.Initialize(stats.pulseDamage, stats.pulseSpeed, false);
            }
        }

        public void TakeDamage(float amount)
        {
            currentShield -= amount;
            GameEvents.TriggerShieldChanged(Mathf.Max(0f, currentShield), stats.shieldCapacity);

            if (currentShield <= 0f)
            {
                // Respawn / soft reset
                currentShield = stats.shieldCapacity;
                transform.position = Vector3.zero;
                rb.linearVelocity = Vector2.zero;
                GameEvents.TriggerShieldChanged(currentShield, stats.shieldCapacity);
            }
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

            if (statName == "shieldCap") { lvlShieldCap++; stats.shieldCapacity *= 1.25f; currentShield = stats.shieldCapacity; }
            else if (statName == "shieldRegen") { lvlShieldRegen++; stats.shieldRegen *= 1.30f; }
            else if (statName == "energyCap") { lvlEnergyCap++; stats.energyCapacity *= 1.20f; currentEnergy = stats.energyCapacity; }
            else if (statName == "energyRegen") { lvlEnergyRegen++; stats.energyRegen *= 1.35f; }
            else if (statName == "damage") { lvlDamage++; stats.pulseDamage *= 1.25f; }
            else if (statName == "pulseSpeed") { lvlPulseSpeed++; stats.pulseSpeed *= 1.15f; }
            else if (statName == "speed") { lvlSpeed++; stats.speed *= 1.12f; stats.acceleration *= 1.12f; }
            else if (statName == "agility") { lvlAgility++; stats.agility *= 1.15f; }

            GameEvents.TriggerGemCollected(0, gems);
            GameEvents.TriggerShieldChanged(currentShield, stats.shieldCapacity);
            GameEvents.TriggerEnergyChanged(currentEnergy, stats.energyCapacity);
            return true;
        }

        public int GetUpgradeCost(string statName)
        {
            int lvl = 0;
            if (statName == "shieldCap") lvl = lvlShieldCap;
            else if (statName == "shieldRegen") lvl = lvlShieldRegen;
            else if (statName == "energyCap") lvl = lvlEnergyCap;
            else if (statName == "energyRegen") lvl = lvlEnergyRegen;
            else if (statName == "damage") lvl = lvlDamage;
            else if (statName == "pulseSpeed") lvl = lvlPulseSpeed;
            else if (statName == "speed") lvl = lvlSpeed;
            else if (statName == "agility") lvl = lvlAgility;

            return Mathf.RoundToInt(10f * Mathf.Pow(1.5f, lvl));
        }

        public bool CanEvolve()
        {
            return stats.tier == 1 && (lvlShieldCap >= 3 || lvlDamage >= 3 || lvlSpeed >= 3);
        }

        public void Evolve(string targetShipId)
        {
            if (targetShipId == "fighter")
                stats = ShipStats.CreateFighter();
            else if (targetShipId == "miner")
                stats = ShipStats.CreateMiner();

            ResetMeters();
            ApplyShipVisuals();
            GameEvents.TriggerPlayerEvolved(stats.shipId);
            GameEvents.TriggerShieldChanged(currentShield, stats.shieldCapacity);
            GameEvents.TriggerEnergyChanged(currentEnergy, stats.energyCapacity);
        }
    }
}
