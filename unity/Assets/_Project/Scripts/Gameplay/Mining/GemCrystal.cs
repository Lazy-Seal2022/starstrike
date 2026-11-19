using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class GemCrystal : MonoBehaviour, IPoolable
    {
        [Header("Configuration")]
        public LootConfig lootConfig;

        [Header("Stats")]
        public int gemValue = 1;
        public float magnetRadius = 6.5f;
        public float magnetSpeed = 14f;

        private Rigidbody2D rb;
        private Transform playerTarget;

        private void Awake()
        {
            if (lootConfig == null) lootConfig = Resources.Load<LootConfig>("Data/LootConfig");
            if (lootConfig == null) lootConfig = ScriptableObject.CreateInstance<LootConfig>();

            gemValue = lootConfig.defaultGemValue;
            magnetRadius = lootConfig.magnetRadius;
            magnetSpeed = lootConfig.magnetSpeed;

            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.2f;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = lootConfig.gemColliderRadius;
            tag = "Gem";

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (sr.sprite == null)
            {
                sr.sprite = lootConfig != null && lootConfig.gemSprite != null ? lootConfig.gemSprite : ProceduralSpriteHelper.GetGemSprite();
            }
            sr.sortingOrder = 3;

            var trackable = GetComponent<StarStrike.Gameplay.MinimapTrackable>();
            if (trackable == null) trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.SetType(StarStrike.Gameplay.TrackableType.Gem);
        }

        private float spawnTime;
        public float lifetime = 35f;
        private float spinSpeed = 45f;
        private float pulsePhase = 0f;
        private Vector3 baseScale = Vector3.one;

        private void OnEnable()
        {
            OnSpawned();

            // Small initial ejection impulse
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float minSpd = lootConfig != null ? lootConfig.minEjectionSpeed : 1.5f;
            float maxSpd = lootConfig != null ? lootConfig.maxEjectionSpeed : 4.0f;
            if (rb != null) rb.linearVelocity = randomDir * Random.Range(minSpd, maxSpd);
        }

        public void OnSpawned()
        {
            spawnTime = Time.time;
            spinSpeed = Random.Range(-75f, 75f);
            if (Mathf.Abs(spinSpeed) < 25f) spinSpeed = 35f * Mathf.Sign(spinSpeed != 0 ? spinSpeed : 1f);
            pulsePhase = Random.Range(0f, Mathf.PI * 2f);
            if (transform.localScale.sqrMagnitude > 0.01f) baseScale = transform.localScale;

            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        public void OnDespawned()
        {
            playerTarget = null;
            transform.localScale = baseScale;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void SimulatePulse(float time, float dt)
        {
            transform.Rotate(0f, 0f, spinSpeed * dt);
            float pulse = 1f + 0.14f * Mathf.Sin(time * 5.5f + pulsePhase);
            transform.localScale = baseScale * pulse;
        }

        private void Update()
        {
            if (Time.time - spawnTime > lifetime)
            {
                if (PoolManager.Instance != null) PoolManager.Instance.Despawn(gameObject);
                else gameObject.SetActive(false);
                return;
            }

            // Procedural tumble and energetic scale pulsing
            SimulatePulse(Time.time, Time.deltaTime);

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
                PlayerProgression player = collision.GetComponent<PlayerProgression>();
                if (player != null)
                {
                    player.AddGems(gemValue);
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
