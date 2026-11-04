using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class LaserProjectile : MonoBehaviour
    {
        public float damage = 18f;
        public bool isEnemy = false;
        public float speed = 25f;
        public float lifetime = 3f;

        private Rigidbody2D rb;

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
                col.radius = 0.35f;
            }
        }

        public void Initialize(float dmg, float spd, bool enemy)
        {
            damage = dmg;
            speed = spd;
            isEnemy = enemy;

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
                if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
                SetupPhysics();
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetLaserSprite(isEnemy);
            sr.sortingOrder = 5;

            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed;
            }
        }

        private float spawnTime;
        private void OnEnable()
        {
            spawnTime = Time.time;
        }
        private void Update()
        {
            if (Time.time - spawnTime > lifetime) {
                gameObject.SetActive(false);
            }
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            bool isEnemyTarget = collision.CompareTag("Enemy") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Enemy"));
            bool isBossTarget = collision.CompareTag("Boss") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Boss"));
            bool isPlayerTarget = collision.CompareTag("Player") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Player"));
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
                IDamageable damageable = collision.GetComponent<IDamageable>();
                if (damageable == null)
                {
                    damageable = collision.GetComponentInParent<IDamageable>();
                }
                if (damageable == null && collision.attachedRigidbody != null)
                {
                    damageable = collision.attachedRigidbody.GetComponent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
                gameObject.SetActive(false);
            }
        }
    }
}
