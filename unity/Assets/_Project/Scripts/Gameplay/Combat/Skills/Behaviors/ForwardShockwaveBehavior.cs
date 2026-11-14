using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ForwardShockwaveBehavior : ISkillBehavior
    {
        private GameObject owner;
        private SkillDefinition definition;
        private Rigidbody2D shipRb;
        private float recoilImpulse = 7.5f;

        public void Initialize(GameObject ownerShip, SkillDefinition skillDef)
        {
            owner = ownerShip;
            definition = skillDef;
            if (owner != null)
            {
                shipRb = owner.GetComponent<Rigidbody2D>();
            }
        }

        public bool CanExecute(GameObject ownerShip, SkillDefinition skillDef)
        {
            return ownerShip != null;
        }

        public void Execute(GameObject ownerShip, Vector2 origin, Vector2 direction)
        {
            if (ownerShip == null) return;
            if (shipRb == null) shipRb = ownerShip.GetComponent<Rigidbody2D>();

            Vector2 castDir = direction.sqrMagnitude > 0.001f ? direction.normalized : (Vector2)ownerShip.transform.right;

            // 1. Apply Newtonian Reaction Recoil to the ship
            if (shipRb != null)
            {
                shipRb.AddForce(-castDir * recoilImpulse, ForceMode2D.Impulse);
            }

            // 2. Spawn Shockwave Projectile
            float spawnOffset = 1.2f;
            CircleCollider2D ownerCol = ownerShip.GetComponent<CircleCollider2D>();
            if (ownerCol != null)
            {
                spawnOffset = ownerCol.radius + 0.5f;
            }

            Vector2 spawnPos = (Vector2)ownerShip.transform.position + castDir * spawnOffset;
            float angle = Mathf.Atan2(castDir.y, castDir.x) * Mathf.Rad2Deg;
            Quaternion spawnRot = Quaternion.Euler(0f, 0f, angle);

            GameObject shockwaveObj = null;
            if (definition != null && definition.effectPrefab != null)
            {
                shockwaveObj = Object.Instantiate(definition.effectPrefab, spawnPos, spawnRot);
            }
            else
            {
                shockwaveObj = new GameObject("ShockwaveWave");
                shockwaveObj.transform.position = spawnPos;
                shockwaveObj.transform.rotation = spawnRot;
                shockwaveObj.AddComponent<ShockwaveProjectile>();
            }

            ShockwaveProjectile proj = shockwaveObj.GetComponent<ShockwaveProjectile>();
            if (proj == null) proj = shockwaveObj.AddComponent<ShockwaveProjectile>();
            proj.Initialize(ownerShip, 40f, 24f, 14f);

            // 3. Audio & Visual Feedback
            if (Audio.AudioService.Instance != null)
            {
                Audio.AudioService.Instance.PlaySound("laser_fire", 1.2f);
            }

            if (FX.FXService.Instance != null)
            {
                FX.FXService.Instance.CameraShake(0.18f, 0.25f);
            }
        }

        public void Terminate()
        {
            owner = null;
            definition = null;
            shipRb = null;
        }
    }
}
