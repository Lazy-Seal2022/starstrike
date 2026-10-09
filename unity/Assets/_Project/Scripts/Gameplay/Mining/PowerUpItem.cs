using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class PowerUpItem : MonoBehaviour
    {
        public enum PowerUpType { ShieldOvercharge, WeaponOverdrive }

        public PowerUpType type = PowerUpType.ShieldOvercharge;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.0f;

            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
            tag = "PowerUp";

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetAsteroidSprite(); // Can replace with specific powerup sprite
            sr.sortingOrder = 3;
            
            var trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.Type = TrackableType.Gem;
        }

        private void OnEnable()
        {
            type = Random.value > 0.5f ? PowerUpType.ShieldOvercharge : PowerUpType.WeaponOverdrive;
            var sr = GetComponent<SpriteRenderer>();
            sr.color = type == PowerUpType.ShieldOvercharge ? Color.blue : Color.red;

            var rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = Random.insideUnitCircle * 2f;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                ApplyPowerUp(collision.gameObject);
                gameObject.SetActive(false);
            }
        }

        private void ApplyPowerUp(GameObject player)
        {
            if (type == PowerUpType.ShieldOvercharge)
            {
                var health = player.GetComponent<ShipHealth>();
                if (health != null)
                {
                    health.maxShield += 100f; // Temporary boost? For now just instant heal + max capacity boost
                    health.currentShield = health.maxShield;
                }
            }
            else if (type == PowerUpType.WeaponOverdrive)
            {
                var weapon = player.GetComponent<ShipWeapon>();
                if (weapon != null)
                {
                    weapon.weaponSlots = Mathf.Min(weapon.weaponSlots + 1, 3);
                    weapon.fireRate *= 0.8f; // Faster fire rate
                }
            }

            // Could also trigger a sound
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("upgrade", 1.0f);
        }
    }
}
