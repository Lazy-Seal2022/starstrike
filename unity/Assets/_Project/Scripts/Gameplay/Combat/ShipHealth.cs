using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay.Audio;
using System;

namespace StarStrike.Gameplay
{
    public class ShipHealth : MonoBehaviour, IDamageable, IEnergyConsumer
    {
        [Header("Stats")]
        public float maxShield = 100f;
        public float shieldRegen = 8f;
        public float maxHull = 80f;
        public float maxEnergy = 80f;
        public float energyRegen = 18f;

        [Header("Energy Capacitor Lockout & Overdraw")]
        public bool isEnergyLockedOut = false;
        [Tooltip("Ratio of max energy required to clear lockout (default 0.30 = 30%)")]
        public float energyRecoveryThresholdRatio = 0.30f;
        [Tooltip("If > 0, overrides ratio with an exact energy value to clear lockout")]
        public float fixedEnergyRecoveryThreshold = 0f;
        [Tooltip("If true, allows a final 'Last Gasp' shot with partial energy, draining the capacitor to 0 and tripping lockout")]
        public bool allowOverdraw = true;

        [Header("Defensive Armor Modifiers")]
        [Range(0f, 0.75f)]
        public float armorDamageReduction = 0f;

        [Header("Current State")]
        public float currentShield;
        public float currentHull;
        public float currentEnergy;

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public bool IsEnergyLockedOut => isEnergyLockedOut;
        public float EnergyRecoveryThreshold => GetEnergyRecoveryThreshold();
        public bool AllowOverdraw => allowOverdraw;
        public float CurrentHull => currentHull;
        public float MaxHull => maxHull;
        public float CurrentShield => currentShield;
        public float MaxShield => maxShield;

        public bool isPlayer = false;
        public bool isInvulnerable = false;
        public bool flashRedOnHullHit = false;

        public bool IsPlayerShip => isPlayer || CompareTag("Player") || gameObject.name == "Player" || GetComponent<PlayerProgression>() != null;

        public event Action<float, float> OnShieldChanged;
        public event Action<float, float> OnHullChanged;
        public event Action<float, float> OnEnergyChanged;
        public event Action<bool, float, float> OnEnergyLockoutChanged;
        public event Action OnCapacitorOverdrawn;
        public event Action<float> OnDamaged;
        public event Action<Vector2, float> OnShieldImpact;
        public event Action OnDied;

        private Vector2 pendingHitPoint = Vector2.zero;

        private void Awake()
        {
            if (CompareTag("Player") || gameObject.name == "Player" || GetComponent<PlayerProgression>() != null)
            {
                isPlayer = true;
            }
        }

        private SpriteRenderer cachedSr;

        private SpriteRenderer GetCachedSpriteRenderer()
        {
            if (cachedSr == null) cachedSr = GetComponent<SpriteRenderer>();
            return cachedSr;
        }

        private void Start()
        {
            ResetMeters();
        }

        public void ResetMeters()
        {
            bool wasLockedOut = isEnergyLockedOut;
            isEnergyLockedOut = false;
            currentShield = maxShield;
            currentHull = maxHull;
            currentEnergy = maxEnergy;
            NotifyChanges();

            if (wasLockedOut)
            {
                float threshold = GetEnergyRecoveryThreshold();
                OnEnergyLockoutChanged?.Invoke(false, currentEnergy, threshold);
                if (isPlayer)
                {
                    GameEvents.TriggerEnergyLockoutChanged(false, currentEnergy, threshold);
                }
            }
        }

        private void Update()
        {
            RegenerateMeters(Time.deltaTime);
        }

        public void RegenerateMeters(float dt)
        {
            bool shieldChanged = false;
            bool energyChanged = false;

            if (currentShield < maxShield)
            {
                currentShield = Mathf.Min(maxShield, currentShield + shieldRegen * dt);
                shieldChanged = true;
            }

            if (currentEnergy < maxEnergy)
            {
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegen * dt);
                energyChanged = true;
            }

            if (isEnergyLockedOut)
            {
                float threshold = GetEnergyRecoveryThreshold();
                if (currentEnergy >= threshold)
                {
                    TriggerEnergyLockout(false);
                    energyChanged = true;
                }
            }

            if (shieldChanged) NotifyShieldChanged();
            if (energyChanged) NotifyEnergyChanged();
        }

        public void TakeDamage(float amount, Vector2 hitPoint)
        {
            pendingHitPoint = hitPoint;
            TakeDamage(amount);
        }

        public void TakeDamage(float amount)
        {
            if (isInvulnerable || amount <= 0f) return;

            float previousShield = currentShield;
            float shieldDamage = Mathf.Min(currentShield, amount);
            currentShield -= shieldDamage;
            float leftoverDamage = amount - shieldDamage;

            if (leftoverDamage > 0f)
            {
                if (armorDamageReduction > 0f)
                {
                    leftoverDamage *= (1f - Mathf.Clamp(armorDamageReduction, 0f, 0.75f));
                }
                currentHull = Mathf.Max(0f, currentHull - leftoverDamage);
            }
            
            OnDamaged?.Invoke(amount);

            if (previousShield > 0f && shieldDamage > 0f)
            {
                Vector2 hitPos = pendingHitPoint != Vector2.zero ? pendingHitPoint : (Vector2)transform.position;
                OnShieldImpact?.Invoke(hitPos, shieldDamage);
            }
            else if (leftoverDamage > 0f)
            {
                // Direct Hull Hit (player ship does not flash red on hull hit)
                if (!IsPlayerShip && flashRedOnHullHit)
                {
                    var sr = GetCachedSpriteRenderer();
                    if (sr != null && StarStrike.Gameplay.FX.FXService.Instance != null)
                    {
                        StarStrike.Gameplay.FX.FXService.Instance.FlashSprite(sr, new Color(1f, 0.35f, 0.35f, 1f));
                    }
                }
            }
            pendingHitPoint = Vector2.zero;
            
            if (StarStrike.Gameplay.FX.FXService.Instance != null && isPlayer)
            {
                float totalPool = maxShield + maxHull;
                float ratio = totalPool > 0f ? amount / totalPool : 0.2f;
                StarStrike.Core.GameEvents.TriggerPlayerDamaged(ratio);
            }

            if (currentHull <= 0f)
            {
                currentHull = 0f;
                OnDied?.Invoke();
            }

            NotifyChanges();
        }

        public void RepairHull(float amount)
        {
            if (amount <= 0f || currentHull >= maxHull) return;
            currentHull = Mathf.Min(maxHull, currentHull + amount);
            NotifyHullChanged();
        }

        public float GetEnergyRecoveryThreshold()
        {
            if (fixedEnergyRecoveryThreshold > 0f)
            {
                return Mathf.Clamp(fixedEnergyRecoveryThreshold, 1f, maxEnergy);
            }
            return maxEnergy * Mathf.Clamp01(energyRecoveryThresholdRatio);
        }

        public void SetEnergyRecoveryThreshold(float exactAmount)
        {
            fixedEnergyRecoveryThreshold = Mathf.Clamp(exactAmount, 1f, maxEnergy);
            if (isEnergyLockedOut && currentEnergy >= fixedEnergyRecoveryThreshold)
            {
                TriggerEnergyLockout(false);
            }
        }

        public void SetEnergyRecoveryRatio(float ratio)
        {
            fixedEnergyRecoveryThreshold = 0f;
            energyRecoveryThresholdRatio = Mathf.Clamp01(ratio);
            if (isEnergyLockedOut && currentEnergy >= GetEnergyRecoveryThreshold())
            {
                TriggerEnergyLockout(false);
            }
        }

        public void TriggerEnergyLockout(bool locked)
        {
            if (isEnergyLockedOut == locked) return;

            isEnergyLockedOut = locked;
            float threshold = GetEnergyRecoveryThreshold();

            OnEnergyLockoutChanged?.Invoke(isEnergyLockedOut, currentEnergy, threshold);

            if (isPlayer)
            {
                GameEvents.TriggerEnergyLockoutChanged(isEnergyLockedOut, currentEnergy, threshold);

                if (locked)
                {
                    AudioService.Instance?.PlaySound("power_down", 0.85f);
                    var sr = GetCachedSpriteRenderer();
                    if (sr != null && StarStrike.Gameplay.FX.FXService.Instance != null)
                    {
                        StarStrike.Gameplay.FX.FXService.Instance.FlashSprite(sr, new Color(1f, 0.45f, 0.1f, 1f));
                    }
                }
                else
                {
                    AudioService.Instance?.PlaySound("power_up", 0.85f);
                    var sr = GetCachedSpriteRenderer();
                    if (sr != null && StarStrike.Gameplay.FX.FXService.Instance != null)
                    {
                        StarStrike.Gameplay.FX.FXService.Instance.FlashSprite(sr, new Color(0.2f, 0.85f, 1f, 1f));
                    }
                }
            }
        }

        public bool HasEnergy(float amount)
        {
            if (isEnergyLockedOut) return false;
            return currentEnergy >= amount;
        }

        public bool ConsumeEnergy(float amount)
        {
            return ConsumeEnergy(amount, allowOverdraw);
        }

        public bool ConsumeEnergy(float amount, bool canOverdraw)
        {
            if (isEnergyLockedOut) return false;

            if (currentEnergy >= amount)
            {
                currentEnergy -= amount;
                if (currentEnergy <= 0.001f)
                {
                    currentEnergy = 0f;
                    TriggerEnergyLockout(true);
                }
                NotifyEnergyChanged();
                return true;
            }

            // Option 2 (Last Gasp Overdraw):
            // If there is any remaining capacitor energy, allow this final shot, drain capacitor to 0, and enter lockout
            if (canOverdraw && currentEnergy > 0.001f)
            {
                currentEnergy = 0f;
                TriggerCapacitorOverdrawn();
                TriggerEnergyLockout(true);
                NotifyEnergyChanged();
                return true;
            }

            // Option 1 (Dry Fire / Brownout):
            // Attempted to draw energy with zero/insufficient energy without overdraw -> trip circuit breaker into lockout
            currentEnergy = 0f;
            TriggerEnergyLockout(true);
            NotifyEnergyChanged();
            return false;
        }

        private void TriggerCapacitorOverdrawn()
        {
            OnCapacitorOverdrawn?.Invoke();
            if (isPlayer)
            {
                GameEvents.TriggerCapacitorOverdrawn();
            }
        }

        private void NotifyShieldChanged()
        {
            OnShieldChanged?.Invoke(currentShield, maxShield);
            if (isPlayer) GameEvents.TriggerShieldChanged(currentShield, maxShield);
        }

        private void NotifyHullChanged()
        {
            OnHullChanged?.Invoke(currentHull, maxHull);
            if (isPlayer) GameEvents.TriggerHullChanged(currentHull, maxHull);
        }

        private void NotifyEnergyChanged()
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            if (isPlayer) GameEvents.TriggerEnergyChanged(currentEnergy, maxEnergy);
        }

        private void NotifyChanges()
        {
            NotifyShieldChanged();
            NotifyHullChanged();
            NotifyEnergyChanged();
        }
    }
}
