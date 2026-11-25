using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipMovement : MonoBehaviour
    {
        [Header("Configuration")]
        public FlightConfig flightConfig;

        [Header("Stats")]
        public float maxSpeed = 10f;
        public float acceleration = 18f;
        public float agility = 6f; // rad/sec
        public float boostMultiplier = 1.8f;
        public float boostMaxSpeedMultiplier = 1.4f;
        public float boostEnergyDrain = 25f; // Energy units / second

        private Rigidbody2D rb;
        private ShipHealth health;
        private PlayerProgression cachedProgression;
        private ShipWeaponController cachedWeaponCtrl;

        [Header("Aim & Steering")]
        public bool steerTowardsAim = true;

        public bool CanAimIndependently
        {
            get
            {
                if (cachedProgression == null) cachedProgression = GetComponent<PlayerProgression>();
                if (ShipWeapon.CanShipAimIndependently(cachedProgression)) return true;

                if (cachedWeaponCtrl == null) cachedWeaponCtrl = GetComponent<ShipWeaponController>();
                return cachedWeaponCtrl != null &&
                       (cachedWeaponCtrl.CanAimIndependently || ShipWeapon.CanShipIdAimIndependently(cachedWeaponCtrl.currentShipId));
            }
        }

        public bool ShouldSteerTowardsAim => steerTowardsAim && !CanAimIndependently;

        // Input state
        public Vector2 AimDirection { get; set; }
        public float TurnInput { get; set; }
        public bool IsThrusting { get; set; }
        public bool IsBoosting { get; set; }
        public bool IsBraking { get; set; }
        public bool autoBrakeEnabled = true;

        public void ApplyInput(IShipInputProvider input)
        {
            if (input == null) return;
            AimDirection = input.AimDirection;
            TurnInput = input.TurnInput;
            IsThrusting = input.IsThrusting;
            IsBoosting = input.IsBoosting;
            IsBraking = input.IsBraking;
        }
        
        private ParticleSystem thrustFX;
        private StarStrike.Gameplay.FX.ShipThrusterEffect thrusterEffect;

        private void Awake()
        {
            InitializePhysics();
            health = GetComponent<ShipHealth>();
        }

        public void InitializePhysics()
        {
            if (flightConfig == null) flightConfig = Resources.Load<FlightConfig>("Data/FlightConfig");
            if (flightConfig == null) flightConfig = ScriptableObject.CreateInstance<FlightConfig>();

            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = flightConfig.linearDamping;
                rb.angularDamping = flightConfig.angularDamping;
            }

            boostMultiplier = flightConfig.boostMultiplier;
            boostMaxSpeedMultiplier = flightConfig.boostMaxSpeedMultiplier;
            boostEnergyDrain = flightConfig.boostEnergyDrain;

            Collider2D col = GetComponent<Collider2D>();
            if (col == null) col = gameObject.AddComponent<PolygonCollider2D>();
            if (col is CircleCollider2D circleCol && circleCol.radius <= 0.01f)
            {
                circleCol.radius = 0.6f;
            }
        }

        private void Start()
        {
            thrusterEffect = GetComponent<StarStrike.Gameplay.FX.ShipThrusterEffect>();
            if (thrusterEffect == null)
            {
                thrusterEffect = gameObject.AddComponent<StarStrike.Gameplay.FX.ShipThrusterEffect>();
            }

            var shieldField = GetComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            if (shieldField == null)
            {
                shieldField = gameObject.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            }
        }

        private void FixedUpdate()
        {
            HandleSteering();
            HandleThrust();
        }

        public void HandleSteering()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (rb == null) return;

            float dt = Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 0.02f;

            if (Mathf.Abs(TurnInput) > 0.01f)
            {
                float deltaAngle = TurnInput * agility * 45f * dt;
                rb.rotation += deltaAngle;
                rb.MoveRotation(rb.rotation);
            }
            else if (ShouldSteerTowardsAim && AimDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                float currentAngle = rb.rotation;
                float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, agility * 60f * dt);
                rb.rotation = angle;
                rb.MoveRotation(angle);
            }
        }

        private void HandleThrust()
        {
            float dt = Time.fixedDeltaTime;

            if (health == null) health = GetComponent<ShipHealth>();
            float minBoostEnergy = flightConfig != null ? flightConfig.minEnergyToBoost : 5f;
            float brakeFactor = flightConfig != null ? flightConfig.brakeDecelerationFactor : 0.4f;

            bool canBoost = IsBoosting && (health == null || health.currentEnergy > minBoostEnergy);
            bool isApplyingThrust = IsThrusting || canBoost;

            if (thrusterEffect != null)
            {
                thrusterEffect.SetThrusting(isApplyingThrust, canBoost);
            }

            if (isApplyingThrust)
            {
                float currentAccel = acceleration;
                float currentMaxSpeed = maxSpeed;

                if (canBoost)
                {
                    if (health == null || health.ConsumeEnergy(boostEnergyDrain * dt))
                    {
                        currentAccel *= boostMultiplier;
                        currentMaxSpeed *= boostMaxSpeedMultiplier;
                    }
                }

                Vector2 thrustDir = transform.right;
                rb.AddForce(thrustDir * currentAccel * rb.mass);

                if (rb.linearVelocity.magnitude > currentMaxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
                }
            }
            else
            {
                if (IsBraking && autoBrakeEnabled)
                {
                    rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, acceleration * brakeFactor * dt);
                }
            }

            if (rb != null && flightConfig != null)
            {
                rb.linearDamping = autoBrakeEnabled ? flightConfig.linearDamping : 0f;
            }
        }
    }
}
