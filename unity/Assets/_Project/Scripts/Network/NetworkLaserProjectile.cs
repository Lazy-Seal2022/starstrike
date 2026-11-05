using UnityEngine;
using Unity.Netcode;
using StarStrike.Gameplay;

namespace StarStrike.Network
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(NetworkObject))]
    public class NetworkLaserProjectile : NetworkBehaviour
    {
        [Header("Stats")]
        public float damage = 22f;
        public float speed = 28f;
        public float lifetime = 2.5f;

        public ulong AttackerClientId { get; set; }

        private Rigidbody2D rb;
        private CircleCollider2D col;
        private SpriteRenderer sr;
        private float spawnTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = 0.2f;
            }

            sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetLaserSprite(false);
            sr.sortingOrder = 6;
            sr.color = new Color(0.2f, 0.95f, 1f, 1f);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            spawnTime = Time.time;

            if (rb != null)
            {
                rb.linearVelocity = transform.right * speed;
            }
        }

        private void Update()
        {
            if (!IsServer) return;

            if (Time.time - spawnTime > lifetime)
            {
                if (NetworkObject != null && NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn(true);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;

            var ship = other.GetComponent<NetworkPlayerShip>();
            if (ship == null && other.attachedRigidbody != null)
            {
                ship = other.attachedRigidbody.GetComponent<NetworkPlayerShip>();
            }

            if (ship != null && ship.OwnerClientId != AttackerClientId && !ship.NetworkIsDead.Value)
            {
                ship.ApplyNetworkDamage(damage, AttackerClientId);

                if (NetworkObject != null && NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn(true);
                }
                return;
            }

            var asteroid = other.GetComponent<Asteroid>();
            if (asteroid == null && other.attachedRigidbody != null)
            {
                asteroid = other.attachedRigidbody.GetComponent<Asteroid>();
            }
            if (asteroid != null)
            {
                asteroid.TakeDamage(damage);
                if (NetworkObject != null && NetworkObject.IsSpawned)
                {
                    NetworkObject.Despawn(true);
                }
            }
        }
    }
}
