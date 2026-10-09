using UnityEngine;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipMovement : MonoBehaviour
    {
        [Header("Stats")]
        public float maxSpeed = 10f;
        public float acceleration = 18f;
        public float agility = 6f; // rad/sec

        private Rigidbody2D rb;

        // Input state
        public Vector2 AimDirection { get; set; }
        public bool IsThrusting { get; set; }
        public bool IsBraking { get; set; }
        
        private ParticleSystem thrustFX;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 0.5f;
                rb.angularDamping = 1.0f;
            }
        }

        private void Start()
        {
            if (StarStrike.Gameplay.FX.FXService.Instance != null)
            {
                GameObject fxObj = StarStrike.Gameplay.FX.FXService.Instance.CreateThrustFX();
                fxObj.transform.SetParent(transform, false);
                fxObj.transform.localPosition = new Vector3(-0.5f, 0, 0); // Position behind the ship
                thrustFX = fxObj.GetComponent<ParticleSystem>();
                thrustFX.Stop();
            }
        }

        private void FixedUpdate()
        {
            HandleSteering();
            HandleThrust();
        }

        private void HandleSteering()
        {
            if (AimDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                float currentAngle = rb.rotation;
                float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, agility * 60f * Time.fixedDeltaTime);
                rb.MoveRotation(angle);
            }
        }

        private void HandleThrust()
        {
            float dt = Time.fixedDeltaTime;

            if (IsThrusting)
            {
                if (thrustFX != null && !thrustFX.isPlaying) thrustFX.Play();
                
                Vector2 thrustDir = transform.right;
                rb.AddForce(thrustDir * acceleration * rb.mass);

                if (rb.linearVelocity.magnitude > maxSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                }
            }
            else
            {
                if (thrustFX != null && thrustFX.isPlaying) thrustFX.Stop();

                if (IsBraking)
                {
                    rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, acceleration * 0.4f * dt);
                }
            }
        }
    }
}
