using StarStrike.Core;
using UnityEngine;

namespace StarStrike.Gameplay
{
    public enum AsteroidTier { Large, Medium, Small }

    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class Asteroid : MonoBehaviour, IDamageable
    {
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
            float scale = 1.6f;
            if (tier == AsteroidTier.Large)
            {
                maxHp = 120f;
                gemDropCount = 8;
                scale = 2.2f;
            }
            else if (tier == AsteroidTier.Medium)
            {
                maxHp = 50f;
                gemDropCount = 4;
                scale = 1.3f;
            }
            else // Small
            {
                maxHp = 20f;
                gemDropCount = 1;
                scale = 0.75f;
            }

            currentHp = maxHp;
            transform.localScale = Vector3.one * scale;

            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.radius = 0.45f;

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
            
            StarStrike.Gameplay.FX.FXService.Instance?.FlashSprite(GetComponent<SpriteRenderer>(), Color.white);

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
            if (Random.value < 0.05f) // 5% chance
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
            if (tier == AsteroidTier.Large)
            {
                SpawnChildAsteroid(AsteroidTier.Medium);
                SpawnChildAsteroid(AsteroidTier.Medium);
            }
            else if (tier == AsteroidTier.Medium)
            {
                SpawnChildAsteroid(AsteroidTier.Small);
                SpawnChildAsteroid(AsteroidTier.Small);
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
            ast.SetupStats();
            ast.hasCustomInitialVelocity = true;

            Rigidbody2D cRb = child.GetComponent<Rigidbody2D>();
            cRb.angularVelocity = Random.Range(-60f, 60f);
            cRb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(2f, 4.5f);
        }
    }
}
