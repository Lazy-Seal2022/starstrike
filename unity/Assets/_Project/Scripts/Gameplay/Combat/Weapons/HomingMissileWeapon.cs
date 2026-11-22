using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class HomingMissileWeapon : MonoBehaviour, IWeapon
    {
        [Header("Weapon Stats")]
        public float missileDamage = 45f;
        public float missileSpeed = 16f;
        public float fireRate = 0.70f;
        public float energyCost = 16f;
        public int weaponSlots = 1;
        public float recoilForce = 2.0f;

        [Header("Prefabs")]
        public GameObject missilePrefab;

        public string WeaponName => "Homing Missiles";
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => 0f;

        private float lastFiredTime = -999f;
        public void ResetCooldown() => lastFiredTime = -999f;
        public void StopFiring() { }

        private Rigidbody2D rb;
        public bool isPlayer = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (CompareTag("Player") || gameObject.name == "Player")
            {
                isPlayer = true;
            }

            if (missilePrefab == null)
            {
                var combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
                if (combatConfig != null && combatConfig.missilePrefab != null)
                {
                    missilePrefab = combatConfig.missilePrefab;
                }
            }

            if (missilePrefab == null)
            {
                missilePrefab = Resources.Load<GameObject>("Prefabs/Projectiles/PlayerMissile");
            }
        }

        public bool TryFire(ShipHealth health) => TryFire((IEnergyConsumer)health);

        public bool TryFire(IEnergyConsumer energySource = null)
        {
            if (Time.time - lastFiredTime < fireRate) return false;

            if (energySource != null && !energySource.ConsumeEnergy(energyCost))
            {
                return false;
            }

            lastFiredTime = Time.time;

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("laser", 0.5f);

            if (rb != null && recoilForce > 0f)
            {
                rb.AddForce(-transform.right * recoilForce, ForceMode2D.Impulse);
            }

            float spread = 0.4f;
            float fwd = 0.7f;
            float[] offsets = ShipWeapon.CalculateMuzzleOffsets(weaponSlots, spread);

            foreach (float offset in offsets)
            {
                Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * fwd + (Vector2)transform.up * offset;
                string poolTag = isPlayer ? "PlayerMissile" : "EnemyMissile";

                GameObject missileObj = null;
                if (PoolManager.Instance != null)
                {
                    missileObj = PoolManager.Instance.Spawn(poolTag, nose, transform.rotation, () =>
                    {
                        if (missilePrefab != null)
                        {
                            var inst = Instantiate(missilePrefab, nose, transform.rotation);
                            inst.name = poolTag;
                            return inst;
                        }
                        GameObject obj = new GameObject(poolTag);
                        obj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        obj.AddComponent<MissileProjectile>();
                        return obj;
                    });
                }
                else
                {
                    if (missilePrefab != null)
                    {
                        missileObj = Instantiate(missilePrefab, nose, transform.rotation);
                        missileObj.name = poolTag;
                    }
                    else
                    {
                        missileObj = new GameObject(poolTag);
                        missileObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        missileObj.transform.position = nose;
                        missileObj.transform.rotation = transform.rotation;
                        missileObj.AddComponent<MissileProjectile>();
                    }
                }

                MissileProjectile proj = missileObj.GetComponent<MissileProjectile>();
                if (proj != null)
                {
                    proj.Initialize(missileDamage, missileSpeed, !isPlayer);
                }
            }

            return true;
        }
    }
}
