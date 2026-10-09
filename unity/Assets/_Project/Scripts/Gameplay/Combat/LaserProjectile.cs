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
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.25f;
        }

        public void Initialize(float dmg, float spd, bool enemy)
        {
            damage = dmg;
            speed = spd;
            isEnemy = enemy;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetLaserSprite(isEnemy);
            sr.sortingOrder = 5;

            rb.linearVelocity = transform.right * speed;
            
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((isEnemy && collision.CompareTag("Player")) ||
                (!isEnemy && (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))) ||
                collision.CompareTag("Asteroid"))
            {
                IDamageable damageable = collision.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
                gameObject.SetActive(false);
            }
        }
    }
}
