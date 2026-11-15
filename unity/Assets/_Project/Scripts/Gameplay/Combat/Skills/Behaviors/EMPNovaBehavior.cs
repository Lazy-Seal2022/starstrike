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

            Physics2D.SyncTransforms();
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin, empRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
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
                }
                else if (col.GetComponent<Asteroid>() != null)
                {
                    col.GetComponent<Asteroid>().TakeDamage(empDamage * 0.5f);
                }
            }

            if (FXService.Instance != null)
            {
                FXService.Instance.CameraShake(0.22f, 0.35f);
            }

            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySound("explosion", 0.9f);
            }
        }

        public void Terminate()
        {
            owner = null;
            definition = null;
        }
    }
}
