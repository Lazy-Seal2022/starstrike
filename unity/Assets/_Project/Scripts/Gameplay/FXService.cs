using System.Collections;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    public class FXService : MonoBehaviour
    {
        public static FXService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnAsteroidDestroyed += SpawnExplosion;
        }

        private void OnDisable()
        {
            GameEvents.OnAsteroidDestroyed -= SpawnExplosion;
        }

        public void CameraShake(float duration, float magnitude)
        {
            StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            Transform camTransform = Camera.main.transform;
            Vector3 originalPos = camTransform.localPosition; // assumes no parent or parent is moving
            // In our game, camera follows player via LateUpdate, so localPosition isn't great.
            // A better way is to add an offset per frame in LateUpdate, but for a quick shake,
            // we'll just offset it directly and let LateUpdate override it if necessary, or we add a shakeOffset.
            // Let's implement a global shake offset.
            
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                
                CameraOffset = new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            CameraOffset = Vector3.zero;
        }

        public static Vector3 CameraOffset { get; private set; }

        public void FlashSprite(SpriteRenderer sr, Color flashColor, float duration = 0.05f)
        {
            if (sr != null)
                StartCoroutine(FlashRoutine(sr, flashColor, duration));
        }

        private IEnumerator FlashRoutine(SpriteRenderer sr, Color flashColor, float duration)
        {
            Color original = sr.color;
            sr.color = flashColor;
            yield return new WaitForSeconds(duration);
            if (sr != null) sr.color = original;
        }

        private void SpawnExplosion(Vector2 pos, string size)
        {
            CameraShake(0.2f, size == "Large" ? 0.4f : (size == "Medium" ? 0.2f : 0.1f));
            
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Spawn("ExplosionFX", pos, Quaternion.identity, () => {
                    return CreateExplosionPrefab();
                });
            }
        }

        private GameObject CreateExplosionPrefab()
        {
            GameObject obj = new GameObject("ExplosionFX");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.4f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
            main.startColor = new ParticleSystem.MinMaxGradient(Color.yellow, Color.red);
            main.stopAction = ParticleSystemStopAction.Disable;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            return obj;
        }
        
        public GameObject CreateThrustFX()
        {
            GameObject obj = new GameObject("ThrustFX");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 0.2f;
            main.startSpeed = 3f;
            main.startSize = 0.3f;
            main.startColor = new Color(0.3f, 0.8f, 1f, 0.8f); // Cyan
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 20;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.1f;
            
            obj.transform.localRotation = Quaternion.Euler(0, -90, 0); // Pointing backwards

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            return obj;
        }
    }
}
