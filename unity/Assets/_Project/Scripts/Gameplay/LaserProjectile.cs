using UnityEngine;

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
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Asteroid"))
            {
                Asteroid asteroid = collision.GetComponent<Asteroid>();
                if (asteroid != null)
                {
                    asteroid.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
            else if (isEnemy && collision.CompareTag("Player"))
            {
                ShipController player = collision.GetComponent<ShipController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
            else if (!isEnemy && collision.CompareTag("Enemy"))
            {
                EnemyAI enemy = collision.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}
