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

        private float halfW;
        private float halfH;
        private LineRenderer lineRenderer;

        private void Start()
        {
            if (config != null)
            {
                halfW = config.worldWidth * 0.5f;
                halfH = config.worldHeight * 0.5f;
            }
            else
            {
                // Fallback
                halfW = 2000f;
                halfH = 2000f;
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

        private void LateUpdate()
        {
            if (playerTransform != null && Camera.main != null)
            {
                Vector3 targetPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 8f * Time.deltaTime);
            }
        }

        private void SpawnAsteroidField()
        {
            int count = config != null ? config.asteroidCount : 50;
            
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW * 0.9f, halfW * 0.9f), Random.Range(-halfH * 0.9f, halfH * 0.9f));
                if (pos.magnitude < 10f) pos = pos.normalized * 12f;

                string tag = "Asteroid_Large";
                float r = Random.value;
                if (r < 0.35f) tag = "Asteroid_Large";
                else if (r < 0.75f) tag = "Asteroid_Medium";
                else tag = "Asteroid_Small";

                if (PoolManager.Instance != null) {
                    PoolManager.Instance.Spawn(tag, pos, Quaternion.identity, () => {
                        GameObject obj = new GameObject(tag);
                        obj.AddComponent<Asteroid>();
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
        }

        private void SpawnEnemyDrones()
        {
            int count = config != null ? 12 : 6;
            
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW * 0.8f, halfW * 0.8f), Random.Range(-halfH * 0.8f, halfH * 0.8f));
                if (pos.magnitude < 15f) pos = pos.normalized * 20f;

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
            float w = config != null ? config.worldWidth : 4000f;
            float h = config != null ? config.worldHeight : 4000f;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(w, h, 0f));
        }
    }
}
