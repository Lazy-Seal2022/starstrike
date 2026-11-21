using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Procedural juice and physical secondary animations for ships:
    /// - Impact damage squash and angular shudder micro-jitter.
    /// Runs in LateUpdate to ensure deformations are rendered directly to screen
    /// without being overwritten by physics or movement scripts.
    /// </summary>
    [ExecuteAlways]
    public class ShipProceduralAnimator : MonoBehaviour
    {
        [Header("Damage Squash & Jitter Settings")]
        [Tooltip("Lateral deformation upon receiving damage")]
        public float damageSquashX = 0.25f;
        public float damageSquashY = -0.22f;
        [Tooltip("Angular micro-jitter intensity in degrees")]
        public float damageJitterAngle = 9.0f;
        public float damageRecoverySpeed = 10f;

        private Vector3 baseScale = Vector3.one;
        private float currentDamageX = 0f;
        private float currentDamageY = 0f;
        private float currentJitter = 0f;

        private ShipHealth health;
        private Rigidbody2D rb;

        public Vector3 BaseScale => baseScale;
        public float CurrentDamageX => currentDamageX;
        public float CurrentJitter => currentJitter;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            ResetVisuals();
        }

        public void EnsureInitialized()
        {
            if (health == null) health = GetComponent<ShipHealth>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();

            if (baseScale == Vector3.one && transform.localScale != Vector3.one)
            {
                baseScale = transform.localScale;
            }
        }

        public void SubscribeEvents()
        {
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
                health.OnDamaged += HandleDamaged;
            }
        }

        public void UnsubscribeEvents()
        {
            if (health != null) health.OnDamaged -= HandleDamaged;
        }

        public void SetBaseScale(Vector3 scale)
        {
            baseScale = scale;
        }

        public void TriggerDamage(float amount = 1f)
        {
            float intensity = Mathf.Clamp01(amount / 25f);
            float scaleMultiplier = 0.65f + 0.35f * intensity;
            currentDamageX = damageSquashX * scaleMultiplier;
            currentDamageY = damageSquashY * scaleMultiplier;
            currentJitter = (Random.value > 0.5f ? 1f : -1f) * damageJitterAngle * scaleMultiplier;
        }

        private void HandleDamaged(float amount) => TriggerDamage(amount);

        public void SimulateStep(float dt)
        {
            if (dt <= 0f) return;

            // Elastic exponential recovery for damage squash & jitter (smooth over ~0.22s)
            currentDamageX = Mathf.Lerp(currentDamageX, 0f, damageRecoverySpeed * dt);
            currentDamageY = Mathf.Lerp(currentDamageY, 0f, damageRecoverySpeed * dt);
            currentJitter = Mathf.Lerp(currentJitter, 0f, damageRecoverySpeed * 1.5f * dt);
            if (Mathf.Abs(currentDamageX) < 0.005f) currentDamageX = 0f;
            if (Mathf.Abs(currentDamageY) < 0.005f) currentDamageY = 0f;
            if (Mathf.Abs(currentJitter) < 0.05f) currentJitter = 0f;

            // Calculate composite procedural deformation
            float modX = 1f + currentDamageX;
            float modY = 1f + currentDamageY;

            // Apply procedural scale
            transform.localScale = new Vector3(baseScale.x * modX, baseScale.y * modY, baseScale.z);

            // Apply micro-jitter to rotation during hit shudder
            if (Mathf.Abs(currentJitter) > 0.01f)
            {
                float jitterAngle = currentJitter * Mathf.Sin((Application.isPlaying ? Time.time : 1f) * 50f);
                float baseRot = rb != null ? rb.rotation : transform.eulerAngles.z;
                transform.rotation = Quaternion.Euler(0f, 0f, baseRot + jitterAngle);
            }
        }

        public void ResetVisuals()
        {
            currentDamageX = 0f;
            currentDamageY = 0f;
            currentJitter = 0f;
            transform.localScale = baseScale;
        }

        private void LateUpdate()
        {
            SimulateStep(Application.isPlaying ? Time.deltaTime : 0.016f);
        }
    }
}
