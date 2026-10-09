using UnityEngine;
using StarStrike.Core;
using System;

namespace StarStrike.Gameplay
{
    public class ShipHealth : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        public float maxShield = 100f;
        public float shieldRegen = 8f;
        public float maxEnergy = 80f;
        public float energyRegen = 18f;

        [Header("Current State")]
        public float currentShield;
        public float currentEnergy;

        public bool isPlayer = false;

        public event Action<float, float> OnShieldChanged;
        public event Action<float, float> OnEnergyChanged;
        public event Action OnDied;

        private void Start()
        {
            ResetMeters();
        }

        public void ResetMeters()
        {
            currentShield = maxShield;
            currentEnergy = maxEnergy;
            NotifyChanges();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            bool changed = false;

            if (currentShield < maxShield)
            {
                currentShield = Mathf.Min(maxShield, currentShield + shieldRegen * dt);
                changed = true;
            }

            if (currentEnergy < maxEnergy)
            {
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegen * dt);
                changed = true;
            }

            if (changed)
            {
                NotifyChanges();
            }
        }

        public void TakeDamage(float amount)
        {
            currentShield -= amount;
            
            if (currentShield <= 0f)
            {
                currentShield = 0f;
                OnDied?.Invoke();
            }

            NotifyChanges();
        }

        public bool ConsumeEnergy(float amount)
        {
            if (currentEnergy >= amount)
            {
                currentEnergy -= amount;
                NotifyChanges();
                return true;
            }
            return false;
        }

        private void NotifyChanges()
        {
            OnShieldChanged?.Invoke(currentShield, maxShield);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);

            if (isPlayer)
            {
                GameEvents.TriggerShieldChanged(currentShield, maxShield);
                GameEvents.TriggerEnergyChanged(currentEnergy, maxEnergy);
            }
        }
    }
}
