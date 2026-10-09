using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(ShipMovement), typeof(ShipHealth), typeof(ShipWeapon))]
    public class BossBrain : MonoBehaviour
    {
        public enum BossPhase { Approaching, SweepingLasers, BulletHell, Enraged }

        public BossPhase currentPhase = BossPhase.Approaching;
        
        [Header("Stats")]
        public float sightRadius = 40f;
        public float attackRadius = 15f;

        private ShipMovement movement;
        private ShipHealth health;
        private ShipWeapon weapon;
        private Transform playerTarget;
        
        private float phaseTimer = 0f;
        private float bulletHellAngle = 0f;

        private void Awake()
        {
            movement = GetComponent<ShipMovement>();
            health = GetComponent<ShipHealth>();
            weapon = GetComponent<ShipWeapon>();
            
            tag = "Enemy"; // Or "Boss"

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite("boss");
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.4f, 0.4f);
            transform.localScale = Vector3.one * 3f;

            health.OnDied += Die;

            var trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.Type = TrackableType.Enemy; // Boss can use Enemy dot or a new Boss dot
        }

        private void OnEnable()
        {
            // Massive stats for boss
            health.maxShield = 2500f;
            health.shieldRegen = 10f;
            movement.maxSpeed = 4f;
            movement.acceleration = 8f;
            movement.agility = 3f;
            weapon.pulseDamage = 30f;
            weapon.pulseSpeed = 25f;
            weapon.fireRate = 0.5f;

            health.maxEnergy = 9999f;
            health.energyRegen = 9999f;
            health.ResetMeters();

            weapon.energyCost = 0f;
            weapon.isPlayer = false;
            weapon.weaponSlots = 3;

            currentPhase = BossPhase.Approaching;
            phaseTimer = 0f;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDied -= Die;
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;

            // Phase Transitions based on Health
            float hpPercent = health.currentShield / health.maxShield;
            if (hpPercent < 0.3f) currentPhase = BossPhase.Enraged;
            else if (hpPercent < 0.6f && currentPhase != BossPhase.BulletHell) 
            {
                if (currentPhase != BossPhase.SweepingLasers) currentPhase = BossPhase.BulletHell; 
                // Cycle between BulletHell and Sweeping
            }

            phaseTimer += Time.deltaTime;
            
            if (currentPhase != BossPhase.Enraged && currentPhase != BossPhase.Approaching && phaseTimer > 10f)
            {
                phaseTimer = 0f;
                currentPhase = currentPhase == BossPhase.BulletHell ? BossPhase.SweepingLasers : BossPhase.BulletHell;
            }
            
            if (currentPhase == BossPhase.Approaching && dist < attackRadius)
            {
                currentPhase = BossPhase.SweepingLasers;
                phaseTimer = 0f;
            }

            switch (currentPhase)
            {
                case BossPhase.Approaching:
                    RunApproach(toPlayer);
                    break;
                case BossPhase.SweepingLasers:
                    RunSweepingLasers(toPlayer);
                    break;
                case BossPhase.BulletHell:
                    RunBulletHell();
                    break;
                case BossPhase.Enraged:
                    RunEnraged(toPlayer);
                    break;
            }
        }

        private void RunApproach(Vector2 toPlayer)
        {
            movement.AimDirection = toPlayer.normalized;
            movement.IsThrusting = true;
        }

        private void RunSweepingLasers(Vector2 toPlayer)
        {
            // Move slowly towards player, constantly sweeping aim
            movement.IsThrusting = toPlayer.magnitude > attackRadius * 0.8f;
            
            float sweepAngle = Mathf.Sin(Time.time * 2f) * 45f;
            Vector2 aimDir = Quaternion.Euler(0, 0, sweepAngle) * toPlayer.normalized;
            movement.AimDirection = aimDir;

            Vector2 forward = transform.right;
            if (Vector2.Dot(forward, aimDir) > 0.95f)
            {
                weapon.fireRate = 0.4f;
                weapon.TryFire(health);
            }
        }

        private void RunBulletHell()
        {
            movement.IsThrusting = false; // Stay put
            
            bulletHellAngle += Time.deltaTime * 60f;
            movement.AimDirection = new Vector2(Mathf.Cos(bulletHellAngle * Mathf.Deg2Rad), Mathf.Sin(bulletHellAngle * Mathf.Deg2Rad));
            
            weapon.fireRate = 0.15f; // Rapid fire
            weapon.weaponSlots = 2; // dual spiral
            weapon.TryFire(health);
        }

        private void RunEnraged(Vector2 toPlayer)
        {
            movement.maxSpeed = 8f;
            movement.acceleration = 16f;
            movement.IsThrusting = toPlayer.magnitude > attackRadius * 0.5f;
            movement.AimDirection = toPlayer.normalized;

            weapon.fireRate = 0.2f;
            weapon.weaponSlots = 3;
            Vector2 forward = transform.right;
            if (Vector2.Dot(forward, toPlayer.normalized) > 0.8f)
            {
                weapon.TryFire(health);
            }
        }

        private void Die()
        {
            // Drop a ton of gems
            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 2f);
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Spawn("Gem", pos, Quaternion.identity, () => {
                        GameObject obj = new GameObject("GemCrystal");
                        obj.AddComponent<GemCrystal>();
                        return obj;
                    });
                }
            }

            // Big explosion
            GameEvents.TriggerAsteroidDestroyed(transform.position, "Large");
            GameEvents.TriggerAsteroidDestroyed(transform.position + Vector3.right, "Large");
            GameEvents.TriggerAsteroidDestroyed(transform.position + Vector3.left, "Large");
            
            // TODO: Trigger Victory Event

            gameObject.SetActive(false);
        }
    }
}
