using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay.Audio;
using StarStrike.Gameplay.FX;

namespace StarStrike.Gameplay
{
    public class BlinkTeleportBehavior : ISkillBehavior
    {
        private GameObject owner;
        private SkillDefinition definition;
        private Rigidbody2D shipRb;
        private ShipHealth shipHealth;

        public float teleportDistance = 8.5f;
        public float invulnerabilityDuration = 0.35f;
        public float arenaLimit = 65f;

        public void Initialize(GameObject ownerShip, SkillDefinition skillDef)
        {
            owner = ownerShip;
            definition = skillDef;
            if (owner != null)
            {
                shipRb = owner.GetComponent<Rigidbody2D>();
                shipHealth = owner.GetComponent<ShipHealth>();
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
            if (shipHealth == null) shipHealth = ownerShip.GetComponent<ShipHealth>();

            Vector2 dir = direction.sqrMagnitude > 0.001f ? direction.normalized : (Vector2)ownerShip.transform.right;

            Vector2 newPos = origin + dir * teleportDistance;
            newPos.x = Mathf.Clamp(newPos.x, -arenaLimit, arenaLimit);
            newPos.y = Mathf.Clamp(newPos.y, -arenaLimit, arenaLimit);

            // Update position
            if (shipRb != null)
            {
                shipRb.position = newPos;
            }
            ownerShip.transform.position = newPos;

            // Flash sprite
            if (FXService.Instance != null)
            {
                var sr = ownerShip.GetComponent<SpriteRenderer>();
                if (sr != null) FXService.Instance.FlashSprite(sr, Color.cyan);
                FXService.Instance.CameraShake(0.12f, 0.2f);
            }

            // Audio warp
            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlaySound("laser_fire", 1.6f);
            }

            // Temporary invulnerability frames
            if (shipHealth != null)
            {
                shipHealth.isInvulnerable = true;
                var runner = ownerShip.GetComponent<MonoBehaviour>();
                if (runner != null && runner.gameObject.activeInHierarchy && Application.isPlaying)
                {
                    runner.StartCoroutine(BlinkIFramesRoutine(shipHealth, invulnerabilityDuration));
                }
            }
        }

        private static System.Collections.IEnumerator BlinkIFramesRoutine(ShipHealth health, float duration)
        {
            if (health == null) yield break;
            yield return new WaitForSeconds(duration);
            if (health != null) health.isInvulnerable = false;
        }

        public void Terminate()
        {
            owner = null;
            definition = null;
            shipRb = null;
            shipHealth = null;
        }
    }
}
