using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public enum AsteroidTier { Large, Medium, Small }

    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class Asteroid : MonoBehaviour
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

        private void Start()
        {
            // Gentle initial spin & drift
            rb.angularVelocity = Random.Range(-40f, 40f);
            rb.linearVelocity = Random.insideUnitCircle * Random.Range(0.5f, 2.0f);
        }

        public void TakeDamage(float amount)
        {
            currentHp -= amount;
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
                GameObject gemObj = new GameObject("GemCrystal");
                gemObj.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                gemObj.AddComponent<GemCrystal>();
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

            Destroy(gameObject);
        }

        private void SpawnChildAsteroid(AsteroidTier childTier)
        {
            GameObject child = new GameObject($"Asteroid_{childTier}");
            child.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 0.4f);
            Asteroid ast = child.AddComponent<Asteroid>();
            ast.tier = childTier;
            ast.SetupStats();

            Rigidbody2D cRb = child.GetComponent<Rigidbody2D>();
            cRb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(2f, 4.5f);
        }
    }
}
