using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class WeaponPickupItem : MonoBehaviour, IPoolable
    {
        public enum WeaponPickupType { Scattergun, Missiles, Beam, Pulse }

        public WeaponPickupType pickupType = WeaponPickupType.Scattergun;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 1.0f;
            }

            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = 0.6f;
            }

            tag = "PowerUp";

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (sr.sprite == null)
            {
                sr.sprite = ProceduralSpriteHelper.GetPowerUpSprite();
            }
            sr.sortingOrder = 3;

            UpdateVisualColor();
        }

        private void UpdateVisualColor()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            switch (pickupType)
            {
                case WeaponPickupType.Scattergun:
                    sr.color = new Color(1f, 0.5f, 0.1f); // Orange
                    break;
                case WeaponPickupType.Missiles:
                    sr.color = new Color(0.9f, 0.2f, 0.2f); // Red
                    break;
                case WeaponPickupType.Beam:
                    sr.color = new Color(0.2f, 0.9f, 1f); // Cyan
                    break;
                case WeaponPickupType.Pulse:
                default:
                    sr.color = new Color(0.2f, 1f, 0.4f); // Green
                    break;
            }
        }

        private void OnEnable()
        {
            OnSpawned();
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            UpdateVisualColor();
        }

        public void OnDespawned()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.gameObject.name == "Player")
            {
                ApplyToPlayer(collision.gameObject);
                gameObject.SetActive(false);
            }
        }

        public void ApplyToPlayer(GameObject player)
        {
            var controller = player.GetComponent<ShipWeaponController>();
            if (controller != null)
            {
                int targetIndex = (int)pickupType;
                if (targetIndex >= 0 && targetIndex < controller.WeaponCount)
                {
                    controller.SwitchWeapon(targetIndex);
                    // Upgrade weapon slot on pickup
                    controller.CurrentWeapon.WeaponSlots = Mathf.Min(controller.CurrentWeapon.WeaponSlots + 1, 3);
                }
            }

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("upgrade", 1.0f);
        }
    }
}
