using UnityEngine;

namespace StarStrike.Gameplay
{
    public class ParallaxBackground : MonoBehaviour
    {
        public int layerCount = 3;
        public float parallaxFactor = 0.5f;
        public bool disablePixelBackground = false;

        private Transform cameraTransform;
        private Vector3[] startPositions;
        private GameObject[] layers;
        private static Material sharedStarMaterial;

        private void Start()
        {
            if (disablePixelBackground) return;

            cameraTransform = Camera.main != null ? Camera.main.transform : null;
            if (cameraTransform == null) return;

            if (sharedStarMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    sharedStarMaterial = new Material(shader);
                    Sprite starSprite = ProceduralSpriteHelper.GetStarSprite();
                    if (starSprite != null && starSprite.texture != null)
                    {
                        sharedStarMaterial.mainTexture = starSprite.texture;
                    }
                }
            }

            layers = new GameObject[layerCount];
            startPositions = new Vector3[layerCount];
            const int starsPerLayer = 60;
            var particles = new ParticleSystem.Particle[starsPerLayer];

            for (int i = 0; i < layerCount; i++)
            {
                layers[i] = new GameObject($"ParallaxLayer_{i}");
                layers[i].transform.SetParent(transform);

                // Set farther layers deeper in Z and smaller parallax factor
                float zOffset = 10f + (i * 5f);
                layers[i].transform.localPosition = new Vector3(0, 0, zOffset);
                startPositions[i] = layers[i].transform.position;

                var ps = layers[i].AddComponent<ParticleSystem>();
                var main = ps.main;
                main.playOnAwake = false;
                main.loop = false;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.maxParticles = starsPerLayer;
                main.startLifetime = float.MaxValue;
                main.startSpeed = 0f;

                var emission = ps.emission;
                emission.enabled = false;
                var shape = ps.shape;
                shape.enabled = false;

                var psr = ps.GetComponent<ParticleSystemRenderer>();
                psr.renderMode = ParticleSystemRenderMode.Billboard;
                psr.sortingOrder = -10 - i;
                if (sharedStarMaterial != null) psr.sharedMaterial = sharedStarMaterial;

                for (int j = 0; j < starsPerLayer; j++)
                {
                    float alpha = Mathf.Clamp01(Random.Range(0.3f, 0.9f) - (i * 0.1f));
                    float size = Random.Range(0.15f, 0.45f);
                    particles[j] = new ParticleSystem.Particle
                    {
                        position = new Vector3(Random.Range(-70f, 70f), Random.Range(-70f, 70f), 0f),
                        startColor = new Color(1f, 1f, 1f, alpha),
                        startSize = size,
                        remainingLifetime = 1000000f,
                        startLifetime = 1000000f
                    };
                }
                ps.SetParticles(particles, starsPerLayer);
                ps.Pause();
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
