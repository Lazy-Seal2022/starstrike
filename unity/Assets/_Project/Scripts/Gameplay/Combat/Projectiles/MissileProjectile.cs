using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class MissileProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        [Header("Stats")]
        public float damage = 45f;
        public float speed = 18f;
        public float turnSpeed = 300f;
        public float blastRadius = 2.4f;
        public float lifetime = 4.0f;
        public bool isEnemy = false;

        public float Damage => damage;
        public float Speed => speed;
        public bool IsEnemy => isEnemy;

        private Rigidbody2D rb;
        private Transform currentTarget;
        private float spawnTime;
        private float nextTargetSearchTime = 0f;

        private void Awake()
        {
            SetupPhysics();
        }

        public void SetupPhysics()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = 0.4f;
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetMissileSprite();
            sr.sortingOrder = 5;
        }

        public void Initialize(float dmg, float spd, bool enemy)
        {
            damage = dmg;
            speed = spd;
            isEnemy = enemy;

            if (rb == null) SetupPhysics();

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = ProceduralSpriteHelper.GetMissileSprite();
                sr.color = enemy ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.7f, 0.2f);
            }

            currentTarget = null;
            nextTargetSearchTime = 0f;
            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed;
            }
        }

        private void OnEnable()
        {
            OnSpawned();
        }

        public void OnSpawned()
        {
            spawnTime = Time.time;
            nextTargetSearchTime = 0f;
        }

        public void OnDespawned()
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            currentTarget = null;
        }

        private void Update()
        {
            if (Time.time - spawnTime > lifetime)
            {
                Explode();
                return;
            }

            // Target search periodically
            if (Time.time >= nextTargetSearchTime)
            {
                nextTargetSearchTime = Time.time + 0.25f;
                if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
                {
                    currentTarget = FindNearestTarget();
                }
            }

            // Steering towards target
            if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            {
                Vector2 toTarget = ((Vector2)currentTarget.position - (Vector2)transform.position).normalized;
                float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, newAngle);
            }

            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed;
            }
        }

        private Transform FindNearestTarget()
        {
            float searchRadius = 25f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, searchRadius);
            Transform bestTarget = null;
            float closestDistSqr = float.MaxValue;

            foreach (var hit in hits)
            {
                if (hit == null || !hit.gameObject.activeInHierarchy) continue;

                bool isValid = false;
                if (!isEnemy)
                {
                    isValid = hit.CompareTag("Enemy") || hit.CompareTag("Boss") || hit.CompareTag("Asteroid");
                }
                else
                {
                    isValid = hit.CompareTag("Player");
                }

                if (isValid)
                {
                    float distSqr = ((Vector2)hit.transform.position - (Vector2)transform.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                        bestTarget = hit.transform;
                    }
                }
            }

            return bestTarget;
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            bool isEnemyTarget = collision.CompareTag("Enemy") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Enemy"));
            bool isBossTarget = collision.CompareTag("Boss") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Boss"));
            bool isPlayerTarget = collision.CompareTag("Player") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Player")) ||
                                  collision.GetComponent<StarStrike.Gameplay.Carrier.CarrierEscortDrone>() != null || (collision.attachedRigidbody != null && collision.attachedRigidbody.GetComponent<StarStrike.Gameplay.Carrier.CarrierEscortDrone>() != null);
            bool isAsteroidTarget = collision.CompareTag("Asteroid") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Asteroid"));

            bool hitTarget = false;
            if (isEnemy)
            {
                hitTarget = isPlayerTarget || isAsteroidTarget;
            }
            else
            {
                hitTarget = isEnemyTarget || isBossTarget || isAsteroidTarget;
            }

            if (hitTarget)
            {
                Explode();
            }
        }

        private void Explode()
        {
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("explosion", 0.7f);

            // Area of Effect damage
            Collider2D[] victims = Physics2D.OverlapCircleAll(transform.position, blastRadius);
            foreach (var victim in victims)
            {
                if (victim == null || !victim.gameObject.activeInHierarchy) continue;

                bool validVictim = false;
                if (!isEnemy)
                {
                    validVictim = victim.CompareTag("Enemy") || victim.CompareTag("Boss") || victim.CompareTag("Asteroid");
                }
                else
                {
                    validVictim = victim.CompareTag("Player") || victim.CompareTag("Asteroid") || victim.GetComponent<StarStrike.Gameplay.Carrier.CarrierEscortDrone>() != null;
                }

                if (validVictim)
                {
                    IDamageable damageable = victim.GetComponent<IDamageable>();
                    if (damageable == null) damageable = victim.GetComponentInParent<IDamageable>();
                    if (damageable == null && victim.attachedRigidbody != null) damageable = victim.attachedRigidbody.GetComponent<IDamageable>();

                    if (damageable != null)
                    {
                        float dist = Vector2.Distance(transform.position, victim.transform.position);
                        float falloff = Mathf.Clamp01(1f - (dist / blastRadius) * 0.4f);
                        float finalDmg = damage * falloff;
                        damageable.TakeDamage(finalDmg, transform.position);
                        GameEvents.TriggerDamageDealt(victim.transform.position, finalDmg, isCrit: Random.value < 0.25f);
                    }
                }
            }

            gameObject.SetActive(false);
        }
    }
}
