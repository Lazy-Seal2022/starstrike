using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Procedural juice and physical secondary animations for ships:
    /// - Weapon recoil compression and spring recovery.
    /// - Impact damage squash and angular micro-jitter.
    /// - Turn banking / lean (pseudo-3D roll).
    /// </summary>
    [ExecuteAlways]
    public class ShipProceduralAnimator : MonoBehaviour
    {
        [Header("Recoil Punch Settings")]
        [Tooltip("Compression along forward firing axis (X)")]
        public float recoilKickX = -0.14f;
        [Tooltip("Expansion along lateral axis (Y)")]
        public float recoilExpansionY = 0.08f;
        [Tooltip("Spring recovery speed back to resting scale")]
        public float recoilRecoverySpeed = 22f;

        [Header("Damage Squash & Jitter Settings")]
        [Tooltip("Lateral deformation upon receiving damage")]
        public float damageSquashX = 0.12f;
        public float damageSquashY = -0.16f;
        [Tooltip("Angular micro-jitter intensity in degrees")]
        public float damageJitterAngle = 4.0f;
        public float damageRecoverySpeed = 16f;

        [Header("Bank Lean Settings")]
        [Tooltip("Max profile narrowing factor during sharp turns")]
        public float maxBankLean = 0.14f;
        public float bankSmoothing = 12f;

        private Vector3 baseScale = Vector3.one;
        private float currentRecoilX = 0f;
        private float currentRecoilY = 0f;
        private float currentDamageX = 0f;
        private float currentDamageY = 0f;
        private float currentJitter = 0f;
        private float currentBankFactor = 0f;

        private ShipWeapon weapon;
        private ShipHealth health;
        private ShipMovement movement;
        private Rigidbody2D rb;

        public Vector3 BaseScale => baseScale;
        public float CurrentRecoilX => currentRecoilX;
        public float CurrentDamageX => currentDamageX;
        public float CurrentBankFactor => currentBankFactor;

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
        }

        public void EnsureInitialized()
        {
            if (weapon == null) weapon = GetComponent<ShipWeapon>();
            if (health == null) health = GetComponent<ShipHealth>();
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (rb == null) rb = GetComponent<Rigidbody2D>();
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
            float scaleMultiplier = 0.6f + 0.4f * intensity;
            currentDamageX = damageSquashX * scaleMultiplier;
            currentDamageY = damageSquashY * scaleMultiplier;
            currentJitter = (Random.value > 0.5f ? 1f : -1f) * damageJitterAngle * scaleMultiplier;
        }

        private void HandleFired() => TriggerRecoil();
        private void HandleDamaged(float amount) => TriggerDamage(amount);

        public void SimulateStep(float dt)
        {
            if (dt <= 0f) return;

            // 1. Recover weapon recoil
            currentRecoilX = Mathf.MoveTowards(currentRecoilX, 0f, recoilRecoverySpeed * dt);
            currentRecoilY = Mathf.MoveTowards(currentRecoilY, 0f, recoilRecoverySpeed * dt);

            // 2. Recover damage squash & jitter
            currentDamageX = Mathf.MoveTowards(currentDamageX, 0f, damageRecoverySpeed * dt);
            currentDamageY = Mathf.MoveTowards(currentDamageY, 0f, damageRecoverySpeed * dt);
            currentJitter = Mathf.MoveTowards(currentJitter, 0f, damageRecoverySpeed * 1.8f * dt);

            // 3. Smooth turn banking
            float targetBank = 0f;
            if (movement != null)
            {
                targetBank = Mathf.Clamp(movement.TurnInput, -1f, 1f) * maxBankLean;
            }
            currentBankFactor = Mathf.Lerp(currentBankFactor, targetBank, dt * bankSmoothing);

            // Calculate composite procedural deformation
            float modX = 1f + currentRecoilX + currentDamageX;
            float modY = 1f + currentRecoilY + currentDamageY - Mathf.Abs(currentBankFactor);

            // Apply procedural scale
            transform.localScale = new Vector3(baseScale.x * modX, baseScale.y * modY, baseScale.z);

            // Apply micro-jitter to rotation during hit shudder
            if (Mathf.Abs(currentJitter) > 0.01f && rb != null)
            {
                float jitterAngle = currentJitter * Mathf.Sin((Application.isPlaying ? Time.time : 1f) * 55f);
                transform.rotation = Quaternion.Euler(0f, 0f, rb.rotation + jitterAngle);
            }
        }

        private void Update()
        {
            SimulateStep(Application.isPlaying ? Time.deltaTime : 0.016f);
        }
    }
}
