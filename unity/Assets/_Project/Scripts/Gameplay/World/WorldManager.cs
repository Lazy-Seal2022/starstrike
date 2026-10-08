using UnityEngine;

namespace StarStrike.Gameplay
{
    public class WorldManager : MonoBehaviour
    {
        [Header("World Boundaries")]
        public float worldWidth = 140f;
        public float worldHeight = 140f;

        [Header("Spawning")]
        public int initialAsteroidCount = 50;
        public int initialDroneCount = 6;

        public Transform playerTransform;

        private void Start()
        {
            // Find player
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            SpawnAsteroidField();
            SpawnEnemyDrones();
            SetupCamera();
        }

        private void SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 10f;
                cam.backgroundColor = new Color(0.015f, 0.03f, 0.07f); // Deep space dark blue
            }
        }

        private void LateUpdate()
        {
            // Smooth Camera Follow
            if (playerTransform != null && Camera.main != null)
            {
                Vector3 targetPos = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
                Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 8f * Time.deltaTime);
            }
        }

        private void SpawnAsteroidField()
        {
            float halfW = worldWidth * 0.45f;
            float halfH = worldHeight * 0.45f;

            for (int i = 0; i < initialAsteroidCount; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
                // Avoid spawning right on player
                if (pos.magnitude < 6f) pos = pos.normalized * 8f;

                GameObject astObj = new GameObject($"Asteroid_{i}");
                astObj.transform.position = pos;
                Asteroid ast = astObj.AddComponent<Asteroid>();

                float r = Random.value;
                if (r < 0.35f) ast.tier = AsteroidTier.Large;
                else if (r < 0.75f) ast.tier = AsteroidTier.Medium;
                else ast.tier = AsteroidTier.Small;

                ast.SetupStats();
            }
        }

        private void SpawnEnemyDrones()
        {
            float halfW = worldWidth * 0.4f;
            float halfH = worldHeight * 0.4f;

            for (int i = 0; i < initialDroneCount; i++)
            {
                Vector2 pos = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
                if (pos.magnitude < 12f) pos = pos.normalized * 15f;

                GameObject droneObj = new GameObject($"EnemyDrone_{i}");
                droneObj.transform.position = pos;
                droneObj.AddComponent<EnemyAI>();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(worldWidth, worldHeight, 0f));
        }
    }
}
