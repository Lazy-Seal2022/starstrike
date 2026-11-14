using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ReactiveBarrierPassiveBehavior : ISkillBehavior
    {
        private GameObject owner;
        private SkillDefinition definition;
        private ShipHealth shipHealth;
        private float internalCooldownRemaining = 0f;
        private float pulseRadius = 8f;
        private float pushForce = 16f;

        public float RemainingCooldown => internalCooldownRemaining;

        public void Initialize(GameObject ownerShip, SkillDefinition skillDef)
        {
            owner = ownerShip;
            definition = skillDef;
            if (owner != null)
            {
                shipHealth = owner.GetComponent<ShipHealth>();
                if (shipHealth != null)
                {
                    shipHealth.OnShieldChanged += HandleShieldChanged;
                }
            }
        }

        public bool CanExecute(GameObject ownerShip, SkillDefinition skillDef)
        {
            return internalCooldownRemaining <= 0f;
        }

        public void UpdateCooldown(float dt)
        {
            if (internalCooldownRemaining > 0f)
            {
                internalCooldownRemaining -= dt;
            }
        }

        private void HandleShieldChanged(float current, float max)
        {
            if (max <= 0f) return;
            float ratio = current / max;
            if (ratio <= 0.25f && CanExecute(owner, definition))
            {
                Execute(owner, owner != null ? (Vector2)owner.transform.position : Vector2.zero, Vector2.zero);
            }
        }

        public void Execute(GameObject ownerShip, Vector2 origin, Vector2 direction)
        {
            if (ownerShip == null) return;
            float cd = definition != null ? definition.cooldownDuration : 25f;
            internalCooldownRemaining = cd;

            Vector2 center = origin != Vector2.zero ? origin : (Vector2)ownerShip.transform.position;

            // 1. Purge enemy projectiles and repel hostiles in radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, pulseRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.gameObject == ownerShip) continue;

                LaserProjectile laser = hit.GetComponent<LaserProjectile>();
                if (laser != null && laser.isEnemy)
                {
                    laser.gameObject.SetActive(false);
                    continue;
                }

                bool isEnemy = hit.CompareTag("Enemy") || (hit.attachedRigidbody != null && hit.attachedRigidbody.CompareTag("Enemy"));
                bool isAsteroid = hit.CompareTag("Asteroid") || (hit.attachedRigidbody != null && hit.attachedRigidbody.CompareTag("Asteroid"));

                if (isEnemy || isAsteroid)
                {
                    Rigidbody2D targetRb = hit.attachedRigidbody != null ? hit.attachedRigidbody : hit.GetComponent<Rigidbody2D>();
                    if (targetRb != null)
                    {
                        Vector2 repulseDir = (targetRb.position - center).normalized;
                        if (repulseDir.sqrMagnitude < 0.01f) repulseDir = Random.insideUnitCircle.normalized;
                        targetRb.AddForce(repulseDir * pushForce, ForceMode2D.Impulse);
                    }
                }
            }

            // 2. Announce trigger via Screen Shake and SFX
            if (FX.FXService.Instance != null)
            {
                FX.FXService.Instance.CameraShake(0.25f, 0.35f);
            }

            if (Audio.AudioService.Instance != null)
            {
                Audio.AudioService.Instance.PlaySound("boss_explosion", 0.9f);
            }

            GameEvents.TriggerSkillCast(new SkillCastEvent(SkillSlotType.Passive1, definition, center, Vector2.zero));
        }

        public void Terminate()
        {
            if (shipHealth != null)
            {
                shipHealth.OnShieldChanged -= HandleShieldChanged;
            }
            owner = null;
            definition = null;
            shipHealth = null;
        }
    }
}
