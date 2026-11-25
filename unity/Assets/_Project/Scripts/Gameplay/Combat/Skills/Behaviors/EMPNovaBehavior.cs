using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay.Audio;
using StarStrike.Gameplay.FX;

namespace StarStrike.Gameplay
{
    public class EMPNovaBehavior : ISkillBehavior
    {
        private GameObject owner;
        private SkillDefinition definition;
        public float empRadius = 9.5f;
        public float empDamage = 30f;
        public float stunDuration = 3.0f;

        private static readonly Collider2D[] hitBuffer = new Collider2D[128];
        private static readonly ContactFilter2D hitFilter = new ContactFilter2D { useTriggers = true, useLayerMask = false };

        public void Initialize(GameObject ownerShip, SkillDefinition skillDef)
        {
            owner = ownerShip;
            definition = skillDef;
        }

        public bool CanExecute(GameObject ownerShip, SkillDefinition skillDef)
        {
            return ownerShip != null;
        }

        public void Execute(GameObject ownerShip, Vector2 origin, Vector2 direction)
        {
            if (ownerShip == null) return;

            // 1. Expanding Visual Shockwave Ring & Player Flash
            EMPShockwaveVisual.Spawn(origin, empRadius, 0.45f);

            var playerSr = ownerShip.GetComponent<SpriteRenderer>();
            if (playerSr != null && FXService.Instance != null)
            {
                FXService.Instance.FlashSprite(playerSr, Color.cyan, 0.25f);
            }

            // 2. Physics AOE overlap
            if (!Application.isPlaying) Physics2D.SyncTransforms();
            int hitCount = Physics2D.OverlapCircle(origin, empRadius, hitFilter, hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = hitBuffer[i];
                if (col == null || col.gameObject == ownerShip) continue;

                if (col.CompareTag("Enemy"))
                {
                    var health = col.GetComponent<ShipHealth>();
                    if (health != null && !health.isInvulnerable)
                    {
                        health.TakeDamage(empDamage);
                    }

                    var ai = col.GetComponent<EnemyAI>();
                    if (ai != null)
                    {
                        ai.ApplyEMPStun(stunDuration);
                    }

                    var sr = col.GetComponent<SpriteRenderer>();
                    if (sr != null && FXService.Instance != null)
                    {
                        FXService.Instance.FlashSprite(sr, Color.cyan, 0.35f);
                    }

                    GameEvents.TriggerDamageDealt(col.transform.position, empDamage, false);
                }
                else
                {
                    var asteroid = col.GetComponent<Asteroid>();
                    if (asteroid != null)
                    {
                        asteroid.TakeDamage(empDamage * 0.5f);
                        var sr = col.GetComponent<SpriteRenderer>();
                        if (sr != null && FXService.Instance != null)
                        {
                            FXService.Instance.FlashSprite(sr, Color.cyan, 0.2f);
                        }
                    }
                }
            }

            // 3. Audio & Camera Feedback
            if (FXService.Instance != null)
            {
                FXService.Instance.CameraShake(0.25f, 0.4f);
            }

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySound("explosion", 0.9f);
                AudioService.Instance.PlaySound("laser", 0.6f);
            }
        }

        public void Terminate()
        {
            owner = null;
            definition = null;
        }
    }
}
