using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class WeaponPickupItem : MonoBehaviour, IPoolable
    {
        public enum WeaponPickupType { Scattergun, Missiles, Beam, Pulse, Gatling }

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

            string weaponId = "pulse";
            switch (pickupType)
            {
                case WeaponPickupType.Scattergun:
                    weaponId = "scattergun";
                    break;
                case WeaponPickupType.Missiles:
                    weaponId = "missile";
                    break;
                case WeaponPickupType.Beam:
                    weaponId = "beam";
                    break;
                case WeaponPickupType.Gatling:
                    weaponId = "gatling";
                    break;
                case WeaponPickupType.Pulse:
                default:
                    weaponId = "pulse";
                    break;
            }

            sr.sprite = ProceduralSpriteHelper.GetWeaponSprite(weaponId);
            sr.color = Color.white;
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
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
            if (collision.CompareTag("Player") || (collision.attachedRigidbody != null && collision.attachedRigidbody.CompareTag("Player")) || collision.gameObject.name == "Player")
            {
                GameObject playerObj = collision.attachedRigidbody != null ? collision.attachedRigidbody.gameObject : collision.gameObject;
                ApplyToPlayer(playerObj);
                gameObject.SetActive(false);
            }
        }

        public void ApplyToPlayer(GameObject player)
        {
            var controller = player.GetComponent<ShipWeaponController>();
            if (controller != null)
            {
                int targetIndex = pickupType switch
                {
                    WeaponPickupType.Scattergun => 1,
                    WeaponPickupType.Missiles => 2,
                    WeaponPickupType.Beam => 3,
                    WeaponPickupType.Gatling => 4,
                    WeaponPickupType.Pulse => 0,
                    _ => 0
                };

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
