using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class EnemyAI : MonoBehaviour
    {
        public float maxHp = 60f;
        public float currentHp;
        public float sightRadius = 16f;
        public float attackRadius = 10f;
        public float fireCooldown = 1.1f;
        public float moveSpeed = 6f;
        public float turnSpeed = 5f;

        private Rigidbody2D rb;
        private Transform playerTarget;
        private float lastFireTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.8f;
            tag = "Enemy";

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.radius = 0.5f;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite("drone");
            sr.sortingOrder = 4;

            currentHp = maxHp;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;

            if (dist <= sightRadius)
            {
                // Face the player
                float targetAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * 60f * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, angle);

                // Move forward if too far
                if (dist > attackRadius * 0.6f)
                {
                    rb.linearVelocity = transform.right * moveSpeed;
                }

                // Shoot laser
                if (dist <= attackRadius && Time.time >= lastFireTime + fireCooldown)
                {
                    FireLaser();
                }
            }
        }

        private void FireLaser()
        {
            lastFireTime = Time.time;
            GameObject laserObj = new GameObject("EnemyLaser");
            laserObj.tag = "Laser";
            laserObj.transform.position = transform.position + transform.right * 0.6f;
            laserObj.transform.rotation = transform.rotation;

            LaserProjectile proj = laserObj.AddComponent<LaserProjectile>();
            proj.Initialize(14f, 18f, true);
        }

        public void TakeDamage(float amount)
        {
            currentHp -= amount;
            if (currentHp <= 0f)
            {
                Die();
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
