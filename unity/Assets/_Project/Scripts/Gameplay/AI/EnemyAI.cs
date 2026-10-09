using UnityEngine;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(ShipMovement), typeof(ShipHealth), typeof(ShipWeapon))]
    public class EnemyAI : MonoBehaviour
    {
        public float sightRadius = 16f;
        public float attackRadius = 10f;

        private ShipMovement movement;
        private ShipHealth health;
        private ShipWeapon weapon;
        private Transform playerTarget;

        private void Awake()
        {
            movement = GetComponent<ShipMovement>();
            health = GetComponent<ShipHealth>();
            weapon = GetComponent<ShipWeapon>();
            
            tag = "Enemy";

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite("drone");
            sr.sortingOrder = 4;

            health.OnDied += Die;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
            
            // Setup stats for drone
            health.maxShield = 60f;
            health.shieldRegen = 0f;
            health.maxEnergy = 9999f;
            health.energyRegen = 9999f;
            health.ResetMeters();

            movement.maxSpeed = 6f;
            movement.acceleration = 12f;
            movement.agility = 5f;

            weapon.pulseDamage = 14f;
            weapon.pulseSpeed = 18f;
            weapon.fireRate = 1.1f;
            weapon.energyCost = 0f;
            weapon.isPlayer = false;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDied -= Die;
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                
                movement.AimDirection = Vector2.zero;
                movement.IsThrusting = false;
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;

            if (dist <= sightRadius)
            {
                movement.AimDirection = toPlayer.normalized;

                // Move forward if too far
                movement.IsThrusting = dist > attackRadius * 0.6f;

                // Shoot laser
                if (dist <= attackRadius)
                {
                    Vector2 forward = transform.right;
                    // Only fire if roughly facing the player
                    if (Vector2.Dot(forward, toPlayer.normalized) > 0.9f)
                    {
                        weapon.TryFire(health);
                    }
                }
            }
            else
            {
                movement.AimDirection = Vector2.zero;
                movement.IsThrusting = false;
            }
        }

        private void Die()
        {
            // Drop gems
            for (int i = 0; i < 6; i++)
            {
                GameObject gemObj = new GameObject("GemCrystal");
                gemObj.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 0.4f);
                gemObj.AddComponent<GemCrystal>();
            }

            Destroy(gameObject);
        }
    }
}
