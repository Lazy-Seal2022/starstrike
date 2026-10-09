using UnityEngine;

namespace StarStrike.Gameplay
{
    public class ParallaxBackground : MonoBehaviour
    {
        public int layerCount = 3;
        public float parallaxFactor = 0.5f;

        private Transform cameraTransform;
        private Vector3[] startPositions;
        private GameObject[] layers;

        private void Start()
        {
            cameraTransform = Camera.main.transform;
            
            layers = new GameObject[layerCount];
            startPositions = new Vector3[layerCount];

            for (int i = 0; i < layerCount; i++)
            {
                layers[i] = new GameObject($"ParallaxLayer_{i}");
                layers[i].transform.SetParent(transform);
                
                // Set farther layers deeper in Z and smaller parallax factor
                float zOffset = 10f + (i * 5f);
                layers[i].transform.localPosition = new Vector3(0, 0, zOffset);
                startPositions[i] = layers[i].transform.position;

                // Add some star sprites
                for (int j = 0; j < 60; j++)
                {
                    GameObject star = new GameObject("Star");
                    star.transform.SetParent(layers[i].transform);
                    star.transform.localPosition = new Vector3(Random.Range(-70f, 70f), Random.Range(-70f, 70f), 0);
                    
                    var sr = star.AddComponent<SpriteRenderer>();
                    sr.sprite = ProceduralSpriteHelper.GetStarSprite();
                    sr.color = new Color(1f, 1f, 1f, Random.Range(0.3f, 0.9f) - (i * 0.1f));
                    float size = Random.Range(0.15f, 0.45f);
                    star.transform.localScale = new Vector3(size, size, 1);
                }
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null) return;

            for (int i = 0; i < layerCount; i++)
            {
                float factor = parallaxFactor / (i + 1);
                Vector3 offset = new Vector3(
                    cameraTransform.position.x * factor,
                    cameraTransform.position.y * factor,
                    0
                );
                layers[i].transform.position = startPositions[i] + offset;
            }
        }
    }
}
