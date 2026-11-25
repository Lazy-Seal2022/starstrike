using StarStrike.Core;
using UnityEngine;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class Asteroid : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        public AsteroidConfig config;

        [Header("Prefabs")]
        public GameObject gemCrystalPrefab;
        public GameObject powerUpPrefab;
        public GameObject mediumChildPrefab;
        public GameObject smallChildPrefab;

        [Header("State")]
        public AsteroidTier tier = AsteroidTier.Large;
        public float currentHp;
        public float maxHp = 120f;
        public int gemDropCount = 8;

        private Rigidbody2D rb;
        private Collider2D cachedCol;
        private CircleCollider2D cachedCircleCol;
        private SpriteRenderer cachedSr;
        private static AsteroidConfig sharedConfig;
        private static bool hasSearchedFallbackPrefabs;
        private static GameObject cachedGemFallback;
        private static GameObject cachedPowerUpFallback;
        private static GameObject cachedMedFallback;
        private static GameObject cachedSmallFallback;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0.05f;
            rb.angularDamping = 0.05f;
            tag = "Asteroid";

            cachedCol = GetComponent<Collider2D>();
            cachedCircleCol = cachedCol as CircleCollider2D ?? GetComponent<CircleCollider2D>();
            cachedSr = GetComponent<SpriteRenderer>();

            var trackable = GetComponent<StarStrike.Gameplay.MinimapTrackable>();
            if (trackable == null) trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.SetType(StarStrike.Gameplay.TrackableType.Asteroid);

            SetupStats();

            if (config != null)
            {
                if (gemCrystalPrefab == null) gemCrystalPrefab = config.gemCrystalPrefab;
                if (powerUpPrefab == null) powerUpPrefab = config.powerUpPrefab;
                if (mediumChildPrefab == null) mediumChildPrefab = config.mediumPrefab;
                if (smallChildPrefab == null) smallChildPrefab = config.smallPrefab;
            }

            if (!hasSearchedFallbackPrefabs)
            {
                hasSearchedFallbackPrefabs = true;
                cachedGemFallback = Resources.Load<GameObject>("Prefabs/Environment/GemCrystal");
                cachedPowerUpFallback = Resources.Load<GameObject>("Prefabs/Environment/PowerUpItem");
                cachedMedFallback = Resources.Load<GameObject>("Prefabs/Environment/Asteroid_Medium");
                cachedSmallFallback = Resources.Load<GameObject>("Prefabs/Environment/Asteroid_Small");
            }

            if (gemCrystalPrefab == null) gemCrystalPrefab = cachedGemFallback;
            if (powerUpPrefab == null) powerUpPrefab = cachedPowerUpFallback;
            if (mediumChildPrefab == null) mediumChildPrefab = cachedMedFallback;
            if (smallChildPrefab == null) smallChildPrefab = cachedSmallFallback;
        }

        public void SetupStats()
        {
            if (config == null)
            {
                if (sharedConfig == null)
                {
                    sharedConfig = Resources.Load<AsteroidConfig>("Data/AsteroidConfig");
                    if (sharedConfig == null) sharedConfig = ScriptableObject.CreateInstance<AsteroidConfig>();
                }
                config = sharedConfig;
            }

            AsteroidTierData data = config.GetTierData(tier);
            maxHp = data.maxHp;
            gemDropCount = data.gemDropCount;
            float scale = data.visualScale;

            currentHp = maxHp;
            transform.localScale = Vector3.one * scale;

            if (cachedCircleCol == null) cachedCircleCol = GetComponent<CircleCollider2D>();
            if (cachedCircleCol == null) cachedCircleCol = gameObject.AddComponent<CircleCollider2D>();
            cachedCol = cachedCircleCol;
            cachedCircleCol.radius = data.colliderRadius;

            if (cachedSr == null) cachedSr = GetComponent<SpriteRenderer>();
            if (cachedSr == null) cachedSr = gameObject.AddComponent<SpriteRenderer>();
            if (cachedSr.sprite == null)
            {
                cachedSr.sprite = config != null && config.sprite != null ? config.sprite : ProceduralSpriteHelper.GetAsteroidSprite();
            }
            cachedSr.sortingOrder = 2;
        }

        public bool hasCustomInitialVelocity = false;

        private void OnEnable()
        {
            if (cachedCol == null) cachedCol = GetComponent<Collider2D>();
            if (cachedCol != null) cachedCol.enabled = true;
            if (rb != null) rb.simulated = true;

            if (!hasCustomInitialVelocity)
            {
                // Gentle initial spin & drift
                if (rb != null)
                {
                    rb.angularVelocity = Random.Range(-40f, 40f);
                    rb.linearVelocity = Random.insideUnitCircle * Random.Range(0.5f, 2.0f);
                }
            }
        }

        public void TakeDamage(float amount)
        {
            currentHp -= amount;

            if (StarStrike.Gameplay.FX.FXService.Instance != null)
            {
                if (cachedSr == null) cachedSr = GetComponent<SpriteRenderer>();
                StarStrike.Gameplay.FX.FXService.Instance.FlashSprite(cachedSr, Color.white);
            }

            if (currentHp <= 0f)
            {
                SplitAndDestroy();
            }
        }

        private void SplitAndDestroy()
        {
            if (cachedCol == null) cachedCol = GetComponent<Collider2D>();
            if (cachedCol != null) cachedCol.enabled = false;
            if (rb != null) rb.simulated = false;

            GameEvents.TriggerAsteroidDestroyed(transform.position, tier.ToString());

            // 1. Drop Crystals
            for (int i = 0; i < gemDropCount; i++)
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn("Gem", pos, Quaternion.identity, () => {
                        if (gemCrystalPrefab != null)
                        {
                            var inst = Instantiate(gemCrystalPrefab, pos, Quaternion.identity);
                            inst.name = "GemCrystal";
                            return inst;
                        }
                        GameObject obj = new GameObject("GemCrystal");
                        obj.AddComponent<GemCrystal>();
                        return obj;
                    });
                } else {
                    GameObject gemObj = null;
                    if (gemCrystalPrefab != null)
                    {
                        gemObj = Instantiate(gemCrystalPrefab, pos, Quaternion.identity);
                        gemObj.name = "GemCrystal";
                    }
                    else
                    {
                        gemObj = new GameObject("GemCrystal");
                        gemObj.transform.position = pos;
                        gemObj.AddComponent<GemCrystal>();
                    }
                }
            }
            
            // Rare chance to drop a powerup
            float puChance = config != null ? config.powerUpDropChance : 0.05f;
            if (Random.value < puChance)
            {
                Vector3 puPos = transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn("PowerUp", puPos, Quaternion.identity, () => {
                        if (powerUpPrefab != null)
                        {
                            var inst = Instantiate(powerUpPrefab, puPos, Quaternion.identity);
                            inst.name = "PowerUpItem";
                            return inst;
                        }
                        GameObject obj = new GameObject("PowerUpItem");
                        obj.AddComponent<PowerUpItem>();
                        return obj;
                    });
                } else {
                    GameObject puObj = null;
                    if (powerUpPrefab != null)
                    {
                        puObj = Instantiate(powerUpPrefab, puPos, Quaternion.identity);
                        puObj.name = "PowerUpItem";
                    }
                    else
                    {
                        puObj = new GameObject("PowerUpItem");
                        puObj.transform.position = puPos;
                        puObj.AddComponent<PowerUpItem>();
                    }
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
            GameObject prefabToUse = childTier == AsteroidTier.Medium ? mediumChildPrefab : smallChildPrefab;
            
            if (PoolManager.Instance != null) {
                child = PoolManager.Instance.Spawn(tag, pos, Quaternion.identity, () => {
                    if (prefabToUse != null)
                    {
                        var inst = Instantiate(prefabToUse, pos, Quaternion.identity);
                        inst.name = tag;
                        return inst;
                    }
                    GameObject obj = new GameObject(tag);
                    obj.AddComponent<Asteroid>();
                    return obj;
                });
            } else {
                if (prefabToUse != null)
                {
                    child = Instantiate(prefabToUse, pos, Quaternion.identity);
                    child.name = tag;
                }
                else
                {
                    child = new GameObject(tag);
                    child.transform.position = pos;
                }
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
