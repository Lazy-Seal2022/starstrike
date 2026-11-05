using StarStrike.Core;
using UnityEngine;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class Asteroid : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        public AsteroidConfig config;

        [Header("State")]
        public AsteroidTier tier = AsteroidTier.Large;
        public float currentHp;
        public float maxHp = 120f;
        public int gemDropCount = 8;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;
            tag = "Asteroid";
            
            var trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.Type = StarStrike.Gameplay.TrackableType.Asteroid;

            SetupStats();
        }

        public void SetupStats()
        {
            if (config == null) config = Resources.Load<AsteroidConfig>("Data/AsteroidConfig");
            if (config == null) config = ScriptableObject.CreateInstance<AsteroidConfig>();

            AsteroidTierData data = config.GetTierData(tier);
            maxHp = data.maxHp;
            gemDropCount = data.gemDropCount;
            float scale = data.visualScale;

            currentHp = maxHp;
            transform.localScale = Vector3.one * scale;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = data.colliderRadius;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetAsteroidSprite();
            sr.sortingOrder = 2;
        }

        public bool hasCustomInitialVelocity = false;

        private void OnEnable()
        {
            if (!hasCustomInitialVelocity)
            {
                // Gentle initial spin & drift
                rb.angularVelocity = Random.Range(-40f, 40f);
                rb.linearVelocity = Random.insideUnitCircle * Random.Range(0.5f, 2.0f);
            }
        }

        public void TakeDamage(float amount)
        {
            currentHp -= amount;
            
            if (StarStrike.Gameplay.FX.FXService.Instance != null)
            {
                StarStrike.Gameplay.FX.FXService.Instance.FlashSprite(GetComponent<SpriteRenderer>(), Color.white);
            }

            if (currentHp <= 0f)
            {
                SplitAndDestroy();
            }
        }

        private void SplitAndDestroy()
        {
            GameEvents.TriggerAsteroidDestroyed(transform.position, tier.ToString());

            // 1. Drop Crystals
            for (int i = 0; i < gemDropCount; i++)
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn("Gem", pos, Quaternion.identity, () => {
                        GameObject obj = new GameObject("GemCrystal");
                        obj.AddComponent<GemCrystal>();
                        return obj;
                    });
                } else {
                    GameObject gemObj = new GameObject("GemCrystal");
                    gemObj.transform.position = pos;
                    gemObj.AddComponent<GemCrystal>();
                }
            }
            
            // Rare chance to drop a powerup
            float puChance = config != null ? config.powerUpDropChance : 0.05f;
            if (Random.value < puChance)
            {
                Vector3 puPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn("PowerUp", puPos, Quaternion.identity, () => {
                        GameObject obj = new GameObject("PowerUpItem");
                        obj.AddComponent<PowerUpItem>();
                        return obj;
                    });
                } else {
                    GameObject puObj = new GameObject("PowerUpItem");
                    puObj.transform.position = puPos;
                    puObj.AddComponent<PowerUpItem>();
                }
            }

            // 2. Fracture into smaller asteroids
            int splitCount = config != null ? config.fractureSplitCount : 2;
            if (tier == AsteroidTier.Large)
            {
                for (int i = 0; i < splitCount; i++) SpawnChildAsteroid(AsteroidTier.Medium);
            }
            else if (tier == AsteroidTier.Medium)
            {
                for (int i = 0; i < splitCount; i++) SpawnChildAsteroid(AsteroidTier.Small);
            }

            gameObject.SetActive(false);
        }

        private void SpawnChildAsteroid(AsteroidTier childTier)
        {
            Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.4f);
            string tag = $"Asteroid_{childTier}";
            GameObject child = null;
            
            if (PoolManager.Instance != null) {
                child = PoolManager.Instance.Spawn(tag, pos, Quaternion.identity, () => {
                    GameObject obj = new GameObject(tag);
                    obj.AddComponent<Asteroid>();
                    return obj;
                });
            } else {
                child = new GameObject(tag);
                child.transform.position = pos;
            }

            Asteroid ast = child.GetComponent<Asteroid>();
            if (ast == null) ast = child.AddComponent<Asteroid>();
            ast.tier = childTier;
            ast.config = config;
            ast.SetupStats();
            ast.hasCustomInitialVelocity = true;

            float minSpd = config != null ? config.minChildSpeed : 2.0f;
            float maxSpd = config != null ? config.maxChildSpeed : 4.5f;

            Rigidbody2D cRb = child.GetComponent<Rigidbody2D>();
            cRb.angularVelocity = Random.Range(-60f, 60f);
            cRb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(minSpd, maxSpd);
        }
    }
}
