using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Procedural juice and physical secondary animations for ships:
    /// - Weapon recoil compression and smooth spring recovery.
    /// - Impact damage squash, hit flash tint, and angular shudder jitter.
    /// Runs in LateUpdate to ensure deformations are rendered directly to screen
    /// without being overwritten by physics or movement scripts.
    /// </summary>
    [ExecuteAlways]
    public class ShipProceduralAnimator : MonoBehaviour
    {
        [Header("Recoil Punch Settings")]
        [Tooltip("Compression along forward firing axis (X)")]
        public float recoilKickX = -0.28f;
        [Tooltip("Expansion along lateral axis (Y)")]
        public float recoilExpansionY = 0.16f;
        [Tooltip("Spring recovery speed back to resting scale")]
        public float recoilRecoverySpeed = 14f;

        [Header("Damage Squash & Jitter Settings")]
        [Tooltip("Lateral deformation upon receiving damage")]
        public float damageSquashX = 0.25f;
        public float damageSquashY = -0.22f;
        [Tooltip("Angular micro-jitter intensity in degrees")]
        public float damageJitterAngle = 9.0f;
        public float damageRecoverySpeed = 10f;

        [Header("Hit Flash Settings")]
        public bool enableHitFlash = true;
        public Color damageFlashColor = new Color(1f, 0.28f, 0.28f, 1f);
        public float flashDuration = 0.12f;

        private Vector3 baseScale = Vector3.one;
        private float currentRecoilX = 0f;
        private float currentRecoilY = 0f;
        private float currentDamageX = 0f;
        private float currentDamageY = 0f;
        private float currentJitter = 0f;
        private float currentFlashTimer = 0f;

        private ShipWeapon weapon;
        private ShipHealth health;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Color baseColor = Color.white;

        public Vector3 BaseScale => baseScale;
        public float CurrentRecoilX => currentRecoilX;
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
            if (weapon == null) weapon = GetComponent<ShipWeapon>();
            if (health == null) health = GetComponent<ShipHealth>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && baseColor == Color.white && spriteRenderer.color != Color.white)
            {
                baseColor = spriteRenderer.color;
            }

            if (baseScale == Vector3.one && transform.localScale != Vector3.one)
            {
                baseScale = transform.localScale;
            }
        }

        public void SubscribeEvents()
        {
            if (weapon != null)
            {
                weapon.OnFired -= HandleFired;
                weapon.OnFired += HandleFired;
            }
            if (health != null)
            {
                health.OnDamaged -= HandleDamaged;
                health.OnDamaged += HandleDamaged;
            }
        }

        public void UnsubscribeEvents()
        {
            if (weapon != null) weapon.OnFired -= HandleFired;
            if (health != null) health.OnDamaged -= HandleDamaged;
        }

        public void SetBaseScale(Vector3 scale)
        {
            baseScale = scale;
        }

        public void TriggerRecoil()
        {
            currentRecoilX = recoilKickX;
            currentRecoilY = recoilExpansionY;
        }

        public void TriggerDamage(float amount = 1f)
        {
            float intensity = Mathf.Clamp01(amount / 25f);
            float scaleMultiplier = 0.65f + 0.35f * intensity;
            currentDamageX = damageSquashX * scaleMultiplier;
            currentDamageY = damageSquashY * scaleMultiplier;
            currentJitter = (Random.value > 0.5f ? 1f : -1f) * damageJitterAngle * scaleMultiplier;

            if (enableHitFlash && spriteRenderer != null)
            {
                currentFlashTimer = flashDuration;
                spriteRenderer.color = damageFlashColor;
            }
        }

        private void HandleFired() => TriggerRecoil();
        private void HandleDamaged(float amount) => TriggerDamage(amount);

        public void SimulateStep(float dt)
        {
            if (dt <= 0f) return;

            // 1. Elastic exponential recovery for weapon recoil (smooth spring over ~0.18s)
            currentRecoilX = Mathf.Lerp(currentRecoilX, 0f, recoilRecoverySpeed * dt);
            currentRecoilY = Mathf.Lerp(currentRecoilY, 0f, recoilRecoverySpeed * dt);
            if (Mathf.Abs(currentRecoilX) < 0.005f) currentRecoilX = 0f;
            if (Mathf.Abs(currentRecoilY) < 0.005f) currentRecoilY = 0f;

            // 2. Elastic exponential recovery for damage squash & jitter (smooth over ~0.22s)
            currentDamageX = Mathf.Lerp(currentDamageX, 0f, damageRecoverySpeed * dt);
            currentDamageY = Mathf.Lerp(currentDamageY, 0f, damageRecoverySpeed * dt);
            currentJitter = Mathf.Lerp(currentJitter, 0f, damageRecoverySpeed * 1.5f * dt);
            if (Mathf.Abs(currentDamageX) < 0.005f) currentDamageX = 0f;
            if (Mathf.Abs(currentDamageY) < 0.005f) currentDamageY = 0f;
            if (Mathf.Abs(currentJitter) < 0.05f) currentJitter = 0f;

            // 3. Hit flash decay
            if (currentFlashTimer > 0f)
            {
                currentFlashTimer -= dt;
                if (currentFlashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = baseColor;
                }
            }

            // Calculate composite procedural deformation
            float modX = 1f + currentRecoilX + currentDamageX;
            float modY = 1f + currentRecoilY + currentDamageY;

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
            currentRecoilX = 0f;
            currentRecoilY = 0f;
            currentDamageX = 0f;
            currentDamageY = 0f;
            currentJitter = 0f;
            currentFlashTimer = 0f;
            transform.localScale = baseScale;
            if (spriteRenderer != null) spriteRenderer.color = baseColor;
        }

        private void LateUpdate()
        {
            SimulateStep(Application.isPlaying ? Time.deltaTime : 0.016f);
        }
    }
}
