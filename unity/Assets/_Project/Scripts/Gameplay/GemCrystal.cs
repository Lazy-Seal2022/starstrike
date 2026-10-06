using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class GemCrystal : MonoBehaviour
    {
        public int gemValue = 1;
        public float magnetRadius = 6.5f;
        public float magnetSpeed = 14f;

        private Rigidbody2D rb;
        private Transform playerTarget;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.2f;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            tag = "Gem";

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetGemSprite();
            sr.sortingOrder = 3;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;

            // Small initial ejection impulse
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rb.linearVelocity = randomDir * Random.Range(1.5f, 4f);
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                return;
            }

            float dist = Vector2.Distance(transform.position, playerTarget.position);
            if (dist <= magnetRadius)
            {
                Vector2 dir = ((Vector2)playerTarget.position - (Vector2)transform.position).normalized;
                float pull = Mathf.Lerp(magnetSpeed * 1.5f, magnetSpeed * 0.5f, dist / magnetRadius);
                rb.linearVelocity = dir * pull;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                ShipController player = collision.GetComponent<ShipController>();
                if (player != null)
                {
                    player.AddGems(gemValue);
                    Destroy(gameObject);
                }
            }
        }
    }
}
