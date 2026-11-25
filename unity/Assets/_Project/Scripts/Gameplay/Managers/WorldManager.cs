using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class WorldManager : MonoBehaviour
    {
        [Header("References")]
        public GameConfig config;
        public BossConfig bossConfig;
        public Transform playerTransform;

        [Header("Prefabs")]
        public GameObject asteroidLargePrefab;
        public GameObject asteroidMediumPrefab;
        public GameObject asteroidSmallPrefab;
        public GameObject enemyDronePrefab;
        public GameObject bossPrefab;

        [Header("World Dimensions")]
        public float worldWidth = 140f;
        public float worldHeight = 140f;
        public int initialAsteroidCount = 50;
        public int initialDroneCount = 6;

        private float halfW;
        private float halfH;
        private LineRenderer lineRenderer;
        private Camera cachedMainCamera;

        private void Awake()
        {
            EnsureInitialized();

            if (StarStrike.Gameplay.Audio.AudioService.Instance == null)
            {
                new GameObject("AudioService").AddComponent<StarStrike.Gameplay.Audio.AudioService>();
            }
            if (StarStrike.Gameplay.FX.FXService.Instance == null)
            {
                new GameObject("FXService").AddComponent<StarStrike.Gameplay.FX.FXService>();
            }
            if (FindAnyObjectByType<AchievementManager>() == null)
            {
                new GameObject("AchievementManager").AddComponent<AchievementManager>();
            }
            if (FindAnyObjectByType<ParallaxBackground>() == null)
            {
                new GameObject("ParallaxBackground").AddComponent<ParallaxBackground>();
            }
            if (FindAnyObjectByType<StarStrike.Gameplay.FX.PostProcessingSetup>() == null)
            {
                new GameObject("PostProcessingSetup").AddComponent<StarStrike.Gameplay.FX.PostProcessingSetup>();
            }
            if (FindAnyObjectByType<GameManager>() == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
            }
        }

        public void EnsureInitialized()
        {
            if (config == null)
            {
                config = Resources.Load<GameConfig>("Data/GameConfig");
            }

            if (bossConfig == null && config != null)
            {
                bossConfig = config.bossConfig;
            }
            if (bossConfig == null)
            {
                bossConfig = Resources.Load<BossConfig>("Data/BossConfig");
            }
            if (bossConfig == null)
            {
                bossConfig = ScriptableObject.CreateInstance<BossConfig>();
            }

            if (asteroidLargePrefab == null && config?.asteroidConfig != null) asteroidLargePrefab = config.asteroidConfig.largePrefab;
            if (asteroidMediumPrefab == null && config?.asteroidConfig != null) asteroidMediumPrefab = config.asteroidConfig.mediumPrefab;
            if (asteroidSmallPrefab == null && config?.asteroidConfig != null) asteroidSmallPrefab = config.asteroidConfig.smallPrefab;
            if (enemyDronePrefab == null && config != null) enemyDronePrefab = config.enemyDronePrefab;
            if (bossPrefab == null && config != null) bossPrefab = config.bossPrefab;

            if (asteroidLargePrefab == null) asteroidLargePrefab = Resources.Load<GameObject>("Prefabs/Environment/Asteroid_Large");
            if (asteroidMediumPrefab == null) asteroidMediumPrefab = Resources.Load<GameObject>("Prefabs/Environment/Asteroid_Medium");
            if (asteroidSmallPrefab == null) asteroidSmallPrefab = Resources.Load<GameObject>("Prefabs/Environment/Asteroid_Small");
            if (enemyDronePrefab == null) enemyDronePrefab = Resources.Load<GameObject>("Prefabs/Enemies/EnemyDrone");
            if (bossPrefab == null) bossPrefab = Resources.Load<GameObject>("Prefabs/Enemies/BossDreadnought");

            float w = (config != null && config.worldWidth > 0 && config.worldWidth <= 500f) ? config.worldWidth : (worldWidth > 0 ? worldWidth : 140f);
            float h = (config != null && config.worldHeight > 0 && config.worldHeight <= 500f) ? config.worldHeight : (worldHeight > 0 ? worldHeight : 140f);
            worldWidth = w;
            worldHeight = h;
            halfW = w * 0.5f;
            halfH = h * 0.5f;
        }

        private void Start()
        {
            EnsureInitialized();

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            SetupCamera();
            DrawBorder();
            PrewarmCorePools();
            
            SpawnAsteroidField();
            if (GameManager.Instance == null || GameManager.Instance.CurrentMode == GameMode.PvE)
            {
                SpawnEnemyDrones();
            }
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.backgroundColor = new Color(0.015f, 0.03f, 0.07f);

                var camCtrl = cam.GetComponent<CameraController>();
                if (camCtrl == null) camCtrl = cam.gameObject.AddComponent<CameraController>();
                if (playerTransform != null) camCtrl.SetTarget(playerTransform);
            }
        }

        private void DrawBorder()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 5;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = new Color(0f, 1f, 1f, 0.3f);
            lineRenderer.endColor = new Color(0f, 1f, 1f, 0.3f);

            Vector3[] positions = new Vector3[]
            {
                new Vector3(-halfW, -halfH, 0),
                new Vector3(halfW, -halfH, 0),
                new Vector3(halfW, halfH, 0),
                new Vector3(-halfW, halfH, 0),
                new Vector3(-halfW, -halfH, 0)
            };
            lineRenderer.SetPositions(positions);

            EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
            edge.points = new Vector2[]
            {
                new Vector2(-halfW, -halfH),
                new Vector2(halfW, -halfH),
                new Vector2(halfW, halfH),
                new Vector2(-halfW, halfH),
                new Vector2(-halfW, -halfH)
            };
        }

        private float gameTimer = 0f;
        private bool bossSpawned = false;

        public void ResetWorld()
        {
            EnsureInitialized();
            gameTimer = 0f;
            bossSpawned = false;

            // 1. Clean up ALL existing asteroids
            var existingAsteroids = FindObjectsByType<StarStrike.Gameplay.Asteroid>(FindObjectsSortMode.None);
            foreach (var a in existingAsteroids)
            {
                if (a != null && a.gameObject != null)
                {
                    if (PoolManager.Instance != null) PoolManager.Instance.Despawn(a.gameObject);
                    else Destroy(a.gameObject);
                }
            }

            // 2. Clean up ALL existing gems
            var existingGems = FindObjectsByType<StarStrike.Gameplay.GemCrystal>(FindObjectsSortMode.None);
            foreach (var g in existingGems)
            {
                if (g != null && g.gameObject != null)
                {
                    if (PoolManager.Instance != null) PoolManager.Instance.Despawn(g.gameObject);
                    else Destroy(g.gameObject);
                }
            }

            // 3. Clean up ALL existing powerups
            var existingPowerups = FindObjectsByType<StarStrike.Gameplay.PowerUpItem>(FindObjectsSortMode.None);
            foreach (var p in existingPowerups)
            {
                if (p != null && p.gameObject != null)
                {
                    if (PoolManager.Instance != null) PoolManager.Instance.Despawn(p.gameObject);
                    else Destroy(p.gameObject);
                }
            }

            // 4. Clean up ALL existing enemies & bosses
            var existingEnemies = FindObjectsByType<StarStrike.Gameplay.EnemyAI>(FindObjectsSortMode.None);
            foreach (var e in existingEnemies)
            {
                if (e != null && e.gameObject != null)
                {
                    if (PoolManager.Instance != null) PoolManager.Instance.Despawn(e.gameObject);
                    else Destroy(e.gameObject);
                }
            }
            var existingBosses = FindObjectsByType<StarStrike.Gameplay.BossBrain>(FindObjectsSortMode.None);
            foreach (var b in existingBosses)
            {
                if (b != null && b.gameObject != null)
                {
                    if (PoolManager.Instance != null) PoolManager.Instance.Despawn(b.gameObject);
                    else Destroy(b.gameObject);
                }
            }

            // 5. Spawn fresh field
            SpawnAsteroidField();
            if (GameManager.Instance == null || GameManager.Instance.CurrentMode == GameMode.PvE)
            {
                SpawnEnemyDrones();
            }
        }

        private void LateUpdate()
        {
            if (CameraController.Instance == null && playerTransform != null)
            {
                if (cachedMainCamera == null || !cachedMainCamera.isActiveAndEnabled)
                {
                    cachedMainCamera = Camera.main;
                }
                if (cachedMainCamera != null)
                {
                    Vector3 targetPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
                    cachedMainCamera.transform.position = Vector3.Lerp(cachedMainCamera.transform.position, targetPos, 8f * Time.deltaTime);

                    if (StarStrike.Gameplay.FX.FXService.Instance != null)
                    {
                        cachedMainCamera.transform.position += StarStrike.Gameplay.FX.FXService.CameraOffset;
                    }
                }
            }

            if (!bossSpawned && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing && GameManager.Instance.CurrentMode == GameMode.PvE)
            {
                gameTimer += Time.deltaTime;
                float delay = bossConfig != null ? bossConfig.spawnDelaySeconds : 60f;
                if (gameTimer > delay)
                {
                    SpawnBoss();
                    bossSpawned = true;
                }
            }
        }

        private void SpawnBoss()
        {
            float yRatio = bossConfig != null ? bossConfig.spawnYRatio : 0.8f;
            Vector2 pos = new Vector2(0, halfH * yRatio);
            
            if (PoolManager.Instance != null) {
                PoolManager.Instance.Spawn("Boss", pos, Quaternion.identity, () => {
                    if (bossPrefab != null)
                    {
                        var inst = Instantiate(bossPrefab, pos, Quaternion.identity);
                        inst.name = "Boss";
                        var brain = inst.GetComponent<BossBrain>();
                        if (brain != null) brain.config = bossConfig;
                        return inst;
                    }
                    GameObject obj = new GameObject("Boss");
                    BossBrain b = obj.AddComponent<BossBrain>();
                    b.config = bossConfig;
                    return obj;
                });
            } else {
                GameObject bossObj = null;
                if (bossPrefab != null)
                {
                    bossObj = Instantiate(bossPrefab, pos, Quaternion.identity);
                    bossObj.name = "Boss";
                }
                else
                {
                    bossObj = new GameObject("Boss");
                    bossObj.transform.position = pos;
                }
                BossBrain b = bossObj.GetComponent<BossBrain>();
                if (b == null) b = bossObj.AddComponent<BossBrain>();
                b.config = bossConfig;
            }
            
            // Optionally notify UI
            Debug.Log("BOSS SPAWNED!");
        }

        private void SpawnAsteroidField()
        {
            EnsureInitialized();
            if (halfW < 20f) halfW = 70f;
            if (halfH < 20f) halfH = 70f;

            int count = config != null ? config.asteroidCount : initialAsteroidCount;
            if (count <= 0) count = 50;
            
            // Guarantee 6-8 asteroids right around player in direct camera view
            int immediateCount = Mathf.Min(8, count);
            for (int i = 0; i < immediateCount; i++)
            {
                float angle = (i / (float)immediateCount) * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
                float dist = Random.Range(7.5f, 14.5f);
                Vector2 pos = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
                SpawnSingleAsteroid(pos);
            }

            // Spawn the remaining asteroids throughout the world
            for (int i = immediateCount; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW * 0.9f, halfW * 0.9f), Random.Range(-halfH * 0.9f, halfH * 0.9f));
                if (pos.sqrMagnitude < 36f)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(8f, 18f);
                }
                SpawnSingleAsteroid(pos);
            }
        }

        private void PrewarmCorePools()
        {
            if (!Application.isPlaying || PoolManager.Instance == null) return;

            var combatConfig = config != null && config.combatConfig != null
                ? config.combatConfig
                : Resources.Load<CombatConfig>("Data/CombatConfig");
            var asteroidConfig = config != null && config.asteroidConfig != null
                ? config.asteroidConfig
                : Resources.Load<AsteroidConfig>("Data/AsteroidConfig");

            PoolManager.Instance.Prewarm("ExplosionFX", StarStrike.Gameplay.FX.FXService.CreateExplosionPrefab, 12);
            PoolManager.Instance.Prewarm("Gem", () =>
            {
                GameObject gemPrefab = asteroidConfig != null ? asteroidConfig.gemCrystalPrefab : null;
                GameObject obj = gemPrefab != null ? Instantiate(gemPrefab) : new GameObject("GemCrystal");
                obj.name = "GemCrystal";
                if (obj.GetComponent<GemCrystal>() == null) obj.AddComponent<GemCrystal>();
                return obj;
            }, 24);
            PoolManager.Instance.Prewarm("PlayerLaser", () => CreatePooledLaser("PlayerLaser", false, combatConfig), 24);
            PoolManager.Instance.Prewarm("EnemyLaser", () => CreatePooledLaser("EnemyLaser", true, combatConfig), 16);
            PoolManager.Instance.Prewarm("Asteroid_Large", () => CreateAsteroidObject("Asteroid_Large", Vector2.zero, asteroidLargePrefab), 12);
            PoolManager.Instance.Prewarm("Asteroid_Medium", () => CreateAsteroidObject("Asteroid_Medium", Vector2.zero, asteroidMediumPrefab), 16);
            PoolManager.Instance.Prewarm("Asteroid_Small", () => CreateAsteroidObject("Asteroid_Small", Vector2.zero, asteroidSmallPrefab), 16);
        }

        private static GameObject CreatePooledLaser(string tag, bool isEnemy, CombatConfig combatConfig)
        {
            GameObject prefab = combatConfig != null
                ? (isEnemy ? combatConfig.enemyLaserPrefab : combatConfig.playerLaserPrefab)
                : null;
            GameObject obj = prefab != null ? Instantiate(prefab) : new GameObject(tag);
            obj.name = tag;
            obj.tag = isEnemy ? "EnemyLaser" : "Laser";
            if (obj.GetComponent<LaserProjectile>() == null) obj.AddComponent<LaserProjectile>();
            return obj;
        }

        private GameObject CreateAsteroidObject(string poolTag, Vector2 pos, GameObject prefabToUse)
        {
            GameObject obj = prefabToUse != null
                ? Instantiate(prefabToUse, pos, Quaternion.identity)
                : new GameObject(poolTag);
            obj.name = poolTag;
            obj.transform.position = pos;

            Asteroid ast = obj.GetComponent<Asteroid>();
            if (ast == null) ast = obj.AddComponent<Asteroid>();
            if (poolTag.Contains("Large")) ast.tier = AsteroidTier.Large;
            else if (poolTag.Contains("Medium")) ast.tier = AsteroidTier.Medium;
            else ast.tier = AsteroidTier.Small;
            ast.SetupStats();
            return obj;
        }

        private void SpawnSingleAsteroid(Vector2 pos)
        {
            string tag = "Asteroid_Large";
            float r = Random.value;
            GameObject prefabToUse = asteroidLargePrefab;
            if (r < 0.35f)
            {
                tag = "Asteroid_Large";
                prefabToUse = asteroidLargePrefab;
            }
            else if (r < 0.75f)
            {
                tag = "Asteroid_Medium";
                prefabToUse = asteroidMediumPrefab;
            }
            else
            {
                tag = "Asteroid_Small";
                prefabToUse = asteroidSmallPrefab;
            }

            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Spawn(tag, pos, Quaternion.identity, () => CreateAsteroidObject(tag, pos, prefabToUse));
            }
            else
            {
                CreateAsteroidObject(tag, pos, prefabToUse);
            }
        }

        private void SpawnEnemyDrones()
        {
            EnsureInitialized();
            if (halfW < 20f) halfW = 70f;
            if (halfH < 20f) halfH = 70f;

            int count = config != null ? 12 : initialDroneCount;
            if (count <= 0) count = 6;
            
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW * 0.8f, halfW * 0.8f), Random.Range(-halfH * 0.8f, halfH * 0.8f));
                if (pos.sqrMagnitude < 225f)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(18f, 32f);
                }

                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn("EnemyDrone", pos, Quaternion.identity, () => {
                        if (enemyDronePrefab != null)
                        {
                            var inst = Instantiate(enemyDronePrefab, pos, Quaternion.identity);
                            inst.name = "EnemyDrone";
                            return inst;
                        }
                        GameObject obj = new GameObject("EnemyDrone");
                        obj.AddComponent<EnemyAI>();
                        return obj;
                    });
                } else {
                    GameObject droneObj = null;
                    if (enemyDronePrefab != null)
                    {
                        droneObj = Instantiate(enemyDronePrefab, pos, Quaternion.identity);
                        droneObj.name = "EnemyDrone";
                    }
                    else
                    {
                        droneObj = new GameObject("EnemyDrone");
                        droneObj.transform.position = pos;
                        droneObj.AddComponent<EnemyAI>();
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            float w = halfW > 0 ? halfW * 2f : worldWidth;
            float h = halfH > 0 ? halfH * 2f : worldHeight;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(w, h, 0f));
        }
    }
}
