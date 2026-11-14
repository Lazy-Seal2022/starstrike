using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class ShockwaveProjectile : MonoBehaviour
    {
        public float damage = 40f;
        public float speed = 22f;
        public float lifetime = 1.5f;
        public float knockbackForce = 12f;
        public int maxHits = 3;
        public float energyRefundOnMultiHit = 15f;

        private Rigidbody2D rb;
        private CircleCollider2D col;
        private GameObject ownerShip;
        private ShipHealth ownerHealth;
        private int hitCount = 0;
        private bool refunded = false;
        private float spawnTime;
        private readonly HashSet<int> hitEntityIds = new HashSet<int>();

        private void Awake()
        {
            col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = 0.8f;
            }

            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void Initialize(GameObject owner, float dmg, float spd, float knockback)
        {
            ownerShip = owner;
            if (owner != null)
            {
                ownerHealth = owner.GetComponent<ShipHealth>();
            }

            damage = dmg;
            speed = spd;
            knockbackForce = knockback;
            hitCount = 0;
            refunded = false;
            hitEntityIds.Clear();
            spawnTime = Time.time;

            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed;
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = gameObject.AddComponent<SpriteRenderer>();
            }
            sr.sprite = ProceduralSpriteHelper.GetLaserSprite(false);
            sr.color = new Color(0.2f, 0.9f, 1f, 0.9f);
            sr.sortingOrder = 6;
            transform.localScale = new Vector3(1.8f, 2.5f, 1f);
        }

        private void Update()
        {
            if (Time.time - spawnTime > lifetime)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other == null || other.gameObject == ownerShip) return;

            int targetId = other.attachedRigidbody != null ? other.attachedRigidbody.GetInstanceID() : other.GetInstanceID();
            if (hitEntityIds.Contains(targetId)) return;

            bool isEnemy = other.CompareTag("Enemy") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Enemy"));
            bool isBoss = other.CompareTag("Boss") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Boss"));
            bool isAsteroid = other.CompareTag("Asteroid") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Asteroid"));

            if (isEnemy || isBoss || isAsteroid)
            {
                hitEntityIds.Add(targetId);
                hitCount++;

                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable == null && other.attachedRigidbody != null)
                {
                    damageable = other.attachedRigidbody.GetComponent<IDamageable>();
                }

                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    GameEvents.TriggerDamageDealt(transform.position, damage, isCrit: true);
                }

                Rigidbody2D targetRb = other.attachedRigidbody != null ? other.attachedRigidbody : other.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    Vector2 pushDir = (targetRb.position - (Vector2)transform.position).normalized;
                    if (pushDir.sqrMagnitude < 0.01f) pushDir = transform.right;
                    targetRb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
                }

                // Multi-target hit energy refund
                if (hitCount >= 2 && !refunded && ownerHealth != null)
                {
                    refunded = true;
                    ownerHealth.currentEnergy = Mathf.Min(ownerHealth.maxEnergy, ownerHealth.currentEnergy + energyRefundOnMultiHit);
                    GameEvents.TriggerEnergyChanged(ownerHealth.currentEnergy, ownerHealth.maxEnergy);
                }

                if (hitCount >= maxHits)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
