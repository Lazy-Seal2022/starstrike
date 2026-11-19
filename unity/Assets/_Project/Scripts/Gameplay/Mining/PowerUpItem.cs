using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class PowerUpItem : MonoBehaviour, IPoolable
    {
        public enum PowerUpType { ShieldOvercharge, WeaponOverdrive }

        [Header("Configuration")]
        public LootConfig lootConfig;

        public PowerUpType type = PowerUpType.ShieldOvercharge;

        private static readonly IPowerUpEffect ShieldEffect = new ShieldOverchargeEffect();
        private static readonly IPowerUpEffect WeaponEffect = new WeaponOverdriveEffect();

        public IPowerUpEffect CurrentEffect => type == PowerUpType.ShieldOvercharge ? ShieldEffect : WeaponEffect;

        private void Awake()
        {
            if (lootConfig == null) lootConfig = Resources.Load<LootConfig>("Data/LootConfig");
            if (lootConfig == null) lootConfig = ScriptableObject.CreateInstance<LootConfig>();

            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.0f;

            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;
            tag = "PowerUp";

            var sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (sr.sprite == null)
            {
                sr.sprite = lootConfig != null && lootConfig.powerUpSprite != null ? lootConfig.powerUpSprite : ProceduralSpriteHelper.GetPowerUpSprite();
            }
            sr.sortingOrder = 3;
            
            var trackable = GetComponent<StarStrike.Gameplay.MinimapTrackable>();
            if (trackable == null) trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.SetType(TrackableType.Gem);
        }

        private void OnEnable()
        {
            OnSpawned();
        }

        public void OnSpawned()
        {
            type = Random.value > 0.5f ? PowerUpType.ShieldOvercharge : PowerUpType.WeaponOverdrive;
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = CurrentEffect.EffectColor;

            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Random.insideUnitCircle * 2f;
        }

        public void OnDespawned()
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            // Procedural cosmic float & breathing pulse
            transform.Rotate(0f, 0f, 32f * Time.deltaTime);
            float pulse = 1f + 0.12f * Mathf.Sin(Time.time * 4f);
            transform.localScale = Vector3.one * pulse;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                ApplyPowerUp(collision.gameObject);
                gameObject.SetActive(false);
            }
        }

        public void ApplyPowerUp(GameObject player)
        {
            CurrentEffect.Apply(player, lootConfig);
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("upgrade", 1.0f);
        }
    }

    public class ShieldOverchargeEffect : IPowerUpEffect
    {
        public string Name => "Shield Overcharge";
        public Color EffectColor => Color.blue;

        public void Apply(GameObject target, LootConfig config)
        {
            var health = target.GetComponent<ShipHealth>();
            if (health != null)
            {
                float bonus = config != null ? config.shieldOverchargeBonus : 100f;
                health.maxShield = Mathf.Min(500f, health.maxShield + bonus);
                health.currentShield = health.maxShield;
            }
        }
    }

    public class WeaponOverdriveEffect : IPowerUpEffect
    {
        public string Name => "Weapon Overdrive";
        public Color EffectColor => Color.red;

        public void Apply(GameObject target, LootConfig config)
        {
            var weapon = target.GetComponent<ShipWeapon>();
            if (weapon != null)
            {
                weapon.WeaponSlots = Mathf.Min(weapon.WeaponSlots + 1, 3);
                float fireMult = config != null ? config.weaponOverdriveFireRateMultiplier : 0.8f;
                float minRate = config != null ? config.minFireRate : (weapon.combatConfig != null ? weapon.combatConfig.minFireRate : 0.10f);
                weapon.FireRate = Mathf.Max(minRate, weapon.FireRate * fireMult);
            }
        }
    }
}
