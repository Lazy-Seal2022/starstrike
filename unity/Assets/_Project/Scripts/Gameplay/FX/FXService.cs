using System.Collections;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    public class FXService : MonoBehaviour
    {
        private static FXService instance;
        public static FXService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<FXService>();
                    if (instance == null)
                    {
                        var go = new GameObject("FXService");
                        instance = go.AddComponent<FXService>();
                    }
                }
                return instance;
            }
        }

        public static Vector3 CameraOffset { get; private set; }
        public static bool IsHitStopActive { get; private set; }

        public float ShakeIntensity { get; set; } = 1.0f;

        public static void ResetStaticState()
        {
            CameraOffset = Vector3.zero;
            IsHitStopActive = false;
        }

        private Coroutine activeShakeCoroutine;
        private Coroutine activeHitStopCoroutine;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            ShakeIntensity = StarStrike.Platform.Storage.StorageService.GetScreenShakeIntensity();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            CameraOffset = Vector3.zero;
        }

        private void OnEnable()
        {
            GameEvents.OnAsteroidDestroyed += SpawnExplosion;
            GameEvents.OnPlayerDamaged += TriggerPlayerDamageShake;
        }

        private void OnDisable()
        {
            GameEvents.OnAsteroidDestroyed -= SpawnExplosion;
            GameEvents.OnPlayerDamaged -= TriggerPlayerDamageShake;
            CameraOffset = Vector3.zero;
        }

        public void CameraShake(float duration, float magnitude)
        {
            CameraShake(duration, magnitude, 24f);
        }

        public void CameraShake(float duration, float magnitude, float frequency)
        {
            if (!isActiveAndEnabled) return;
            if (activeShakeCoroutine != null) StopCoroutine(activeShakeCoroutine);
            activeShakeCoroutine = StartCoroutine(DampedShakeRoutine(duration, magnitude, frequency));
        }

        public void CameraShakeImpulse(Vector2 direction, float force, float duration = 0.2f)
        {
            if (!isActiveAndEnabled) return;
            if (activeShakeCoroutine != null) StopCoroutine(activeShakeCoroutine);
            activeShakeCoroutine = StartCoroutine(ImpulseShakeRoutine(direction.normalized, force, duration));
        }

        public void TriggerPlayerDamageShake(float damageRatio)
        {
            float mag = Mathf.Clamp(damageRatio * 0.75f, 0.15f, 0.55f);
            CameraShake(0.25f, mag, 28f);
        }

        public void ApplyHitStop(float durationSeconds = 0.05f, float targetTimeScale = 0.05f)
        {
            if (!isActiveAndEnabled) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
            if (activeHitStopCoroutine != null) StopCoroutine(activeHitStopCoroutine);
            activeHitStopCoroutine = StartCoroutine(HitStopRoutine(durationSeconds, targetTimeScale));
        }

        private IEnumerator DampedShakeRoutine(float duration, float magnitude, float frequency)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                // Smooth quadratic falloff curve so shake fades out naturally
                float damping = (1f - progress) * (1f - progress);
                float wave = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f);

                float pX = Mathf.PerlinNoise(elapsed * 12f, 0f) * 2f - 1f;
                float pY = Mathf.PerlinNoise(0f, elapsed * 12f) * 2f - 1f;

                float x = (wave * 0.6f + pX * 0.4f) * magnitude * damping;
                float y = (wave * 0.6f + pY * 0.4f) * magnitude * damping;

                CameraOffset = new Vector3(x, y, 0f) * ShakeIntensity;
                yield return null;
            }
            CameraOffset = Vector3.zero;
            activeShakeCoroutine = null;
        }

        private IEnumerator ImpulseShakeRoutine(Vector2 dir, float force, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float decay = Ease.OutQuad(1f - progress);
                CameraOffset = (Vector3)(dir * force * decay * ShakeIntensity);
                yield return null;
            }
            CameraOffset = Vector3.zero;
            activeShakeCoroutine = null;
        }

        private IEnumerator HitStopRoutine(float durationSeconds, float targetTimeScale)
        {
            IsHitStopActive = true;
            float prevScale = Time.timeScale;
            if (prevScale > 0.01f)
            {
                Time.timeScale = targetTimeScale;
            }

            yield return new WaitForSecondsRealtime(durationSeconds);

            if (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing)
            {
                Time.timeScale = 1f;
            }
            IsHitStopActive = false;
            activeHitStopCoroutine = null;
        }

        public void FlashSprite(SpriteRenderer sr, Color flashColor, float duration = 0.05f)
        {
            if (sr != null && isActiveAndEnabled)
                StartCoroutine(FlashRoutine(sr, flashColor, duration));
        }

        private IEnumerator FlashRoutine(SpriteRenderer sr, Color flashColor, float duration)
        {
            Color original = sr.color;
            sr.color = flashColor;
            yield return new WaitForSecondsRealtime(duration);
            if (sr != null) sr.color = original;
        }

        private void SpawnExplosion(Vector2 pos, string size)
        {
            if (size == "Large")
            {
                CameraShake(0.35f, 0.45f, 26f);
                ApplyHitStop(0.06f, 0.05f);
            }
            else if (size == "Medium")
            {
                CameraShake(0.22f, 0.25f, 24f);
                ApplyHitStop(0.03f, 0.1f);
            }
            else
            {
                CameraShake(0.12f, 0.12f, 20f);
            }

            if (PoolManager.Instance != null)
            {
                var expObj = PoolManager.Instance.Spawn("ExplosionFX", pos, Quaternion.identity, () => {
                    return CreateExplosionPrefab();
                });
                if (expObj != null)
                {
                    var ps = expObj.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        var main = ps.main;
                        if (size == "Large")
                        {
                            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 14f);
                            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 1.1f);
                        }
                        else if (size == "Medium")
                        {
                            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 9f);
                            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.8f);
                        }
                        else
                        {
                            main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 6f);
                            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.5f);
                        }
                        ps.Clear();
                        ps.Play();
                    }
                }
            }
        }

        private GameObject CreateExplosionPrefab()
        {
            GameObject obj = new GameObject("ExplosionFX");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
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
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            
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
