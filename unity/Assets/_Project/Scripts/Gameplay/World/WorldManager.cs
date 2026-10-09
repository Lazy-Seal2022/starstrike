using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class WorldManager : MonoBehaviour
    {
        [Header("References")]
        public GameConfig config;
        public Transform playerTransform;

        [Header("World Dimensions")]
        public float worldWidth = 140f;
        public float worldHeight = 140f;
        public int initialAsteroidCount = 50;
        public int initialDroneCount = 6;

        private float halfW;
        private float halfH;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            if (StarStrike.Gameplay.Audio.AudioService.Instance == null)
            {
                new GameObject("AudioService").AddComponent<StarStrike.Gameplay.Audio.AudioService>();
            }
            if (StarStrike.Gameplay.FX.FXService.Instance == null)
            {
                new GameObject("FXService").AddComponent<StarStrike.Gameplay.FX.FXService>();
            }
            if (FindObjectOfType<AchievementManager>() == null)
            {
                new GameObject("AchievementManager").AddComponent<AchievementManager>();
            }
            if (FindObjectOfType<ParallaxBackground>() == null)
            {
                new GameObject("ParallaxBackground").AddComponent<ParallaxBackground>();
            }
            if (FindObjectOfType<StarStrike.Gameplay.FX.PostProcessingSetup>() == null)
            {
                new GameObject("PostProcessingSetup").AddComponent<StarStrike.Gameplay.FX.PostProcessingSetup>();
            }
            if (FindObjectOfType<GameManager>() == null)
            {
                new GameObject("GameManager").AddComponent<GameManager>();
            }
        }

        private void Start()
        {
            if (config == null)
            {
                config = Resources.Load<GameConfig>("Data/GameConfig");
            }

            if (config != null)
            {
                float w = (config.worldWidth > 0 && config.worldWidth <= 500f) ? config.worldWidth : worldWidth;
                float h = (config.worldHeight > 0 && config.worldHeight <= 500f) ? config.worldHeight : worldHeight;
                halfW = w * 0.5f;
                halfH = h * 0.5f;
            }
            else
            {
                halfW = worldWidth * 0.5f;
                halfH = worldHeight * 0.5f;
            }

            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            SetupCamera();
            DrawBorder();
            
            SpawnAsteroidField();
            SpawnEnemyDrones();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 10f;
                cam.backgroundColor = new Color(0.015f, 0.03f, 0.07f);
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
            gameTimer = 0f;
            bossSpawned = false;

            var existingAsteroids = FindObjectsOfType<StarStrike.Gameplay.Asteroid>();
            if (existingAsteroids.Length < 15)
            {
                foreach (var a in existingAsteroids)
                {
                    Destroy(a.gameObject);
                }
                SpawnAsteroidField();
            }
        }

        private void LateUpdate()
        {
            if (playerTransform != null && Camera.main != null)
            {
                Vector3 targetPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 8f * Time.deltaTime);
                
                if (StarStrike.Gameplay.FX.FXService.Instance != null)
                {
                    Camera.main.transform.position += StarStrike.Gameplay.FX.FXService.CameraOffset;
                }
            }

            if (!bossSpawned && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                gameTimer += Time.deltaTime;
                if (gameTimer > 60f) // Spawn boss after 60 seconds
                {
                    SpawnBoss();
                    bossSpawned = true;
                }
            }
        }

        private void SpawnBoss()
        {
            Vector2 pos = new Vector2(0, halfH * 0.8f);
            
            if (PoolManager.Instance != null) {
                PoolManager.Instance.Spawn("Boss", pos, Quaternion.identity, () => {
                    GameObject obj = new GameObject("Boss");
                    obj.AddComponent<BossBrain>();
                    return obj;
                });
            } else {
                GameObject bossObj = new GameObject("Boss");
                bossObj.transform.position = pos;
                bossObj.AddComponent<BossBrain>();
            }
            
            // Optionally notify UI
            Debug.Log("BOSS SPAWNED!");
        }

        private void SpawnAsteroidField()
        {
            int count = config != null ? config.asteroidCount : initialAsteroidCount;
            if (count <= 0) count = 50;
            
            // Guarantee 6-8 asteroids right around player in direct camera view
            int immediateCount = Mathf.Min(8, count);
            for (int i = 0; i < immediateCount; i++)
            {
                float angle = (i / (float)immediateCount) * Mathf.PI * 2f + Random.Range(-0.25f, 0.25f);
                float dist = Random.Range(6.5f, 13.5f);
                Vector2 pos = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
                SpawnSingleAsteroid(pos);
            }

            // Spawn the remaining asteroids throughout the world
            for (int i = immediateCount; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW * 0.9f, halfW * 0.9f), Random.Range(-halfH * 0.9f, halfH * 0.9f));
                if (pos.magnitude < 6f) pos = pos.normalized * 8f;
                SpawnSingleAsteroid(pos);
            }
        }

        private void SpawnSingleAsteroid(Vector2 pos)
        {
            string tag = "Asteroid_Large";
            float r = Random.value;
            if (r < 0.35f) tag = "Asteroid_Large";
            else if (r < 0.75f) tag = "Asteroid_Medium";
            else tag = "Asteroid_Small";

            if (PoolManager.Instance != null) {
                PoolManager.Instance.Spawn(tag, pos, Quaternion.identity, () => {
                    GameObject obj = new GameObject(tag);
                    Asteroid ast = obj.AddComponent<Asteroid>();
                    if (tag.Contains("Large")) ast.tier = AsteroidTier.Large;
                    else if (tag.Contains("Medium")) ast.tier = AsteroidTier.Medium;
                    else ast.tier = AsteroidTier.Small;
                    ast.SetupStats();
                    return obj;
                });
            } else {
                GameObject astObj = new GameObject(tag);
                astObj.transform.position = pos;
                Asteroid ast = astObj.AddComponent<Asteroid>();
                if (tag.Contains("Large")) ast.tier = AsteroidTier.Large;
                else if (tag.Contains("Medium")) ast.tier = AsteroidTier.Medium;
                else ast.tier = AsteroidTier.Small;
                ast.SetupStats();
            }
        }

        private void SpawnEnemyDrones()
        {
            int count = config != null ? 12 : initialDroneCount;
            if (count <= 0) count = 6;
            
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW * 0.8f, halfW * 0.8f), Random.Range(-halfH * 0.8f, halfH * 0.8f));
                if (pos.magnitude < 15f) pos = pos.normalized * 18f;

                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn("EnemyDrone", pos, Quaternion.identity, () => {
                        GameObject obj = new GameObject("EnemyDrone");
                        obj.AddComponent<EnemyAI>();
                        return obj;
                    });
                } else {
                    GameObject droneObj = new GameObject("EnemyDrone");
                    droneObj.transform.position = pos;
                    droneObj.AddComponent<EnemyAI>();
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
