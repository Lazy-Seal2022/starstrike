using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ShipWeapon : MonoBehaviour, IWeapon
    {
        [Header("Configuration")]
        public CombatConfig combatConfig;

        [Header("Prefabs")]
        public GameObject laserPrefab;

        [Header("Stats")]
        public float pulseDamage = 18f;
        public float pulseSpeed = 22f;
        public int weaponSlots = 1;
        public float fireRate = 0.22f;
        public float energyCost = 6f;

        public string WeaponName => "Pulse Laser";
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public void StopFiring() { }

        public bool isPlayer = false;

        private float lastFiredTime = -999f;
        public void ResetCooldown() => lastFiredTime = -999f;
        private Rigidbody2D rb;

        private void Awake()
        {
            if (combatConfig == null) combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (combatConfig == null) combatConfig = ScriptableObject.CreateInstance<CombatConfig>();

            if (combatConfig != null)
            {
                if (energyCost == 12f || energyCost <= 0f) energyCost = combatConfig.defaultEnergyCost;
                if (fireRate <= 0f) fireRate = combatConfig.defaultFireRate;
            }

            float minRate = combatConfig != null ? combatConfig.minFireRate : 0.10f;
            if (fireRate < minRate) fireRate = minRate;

            rb = GetComponent<Rigidbody2D>();
            if (CompareTag("Player") || gameObject.name == "Player")
            {
                isPlayer = true;
            }

            if (laserPrefab == null && combatConfig != null)
            {
                laserPrefab = isPlayer ? combatConfig.playerLaserPrefab : combatConfig.enemyLaserPrefab;
            }

            if (laserPrefab == null)
            {
                string prefabPath = isPlayer ? "Prefabs/Projectiles/PlayerLaser" : "Prefabs/Projectiles/EnemyLaser";
                laserPrefab = Resources.Load<GameObject>(prefabPath);
            }
        }

        public static float[] CalculateMuzzleOffsets(int slots, float spread)
        {
            if (slots >= 3)
            {
                return new float[] { -spread, 0f, spread };
            }
            if (slots == 2)
            {
                return new float[] { -spread, spread };
            }
            return new float[] { 0f };
        }

        public bool TryFire(ShipHealth health) => TryFire((IEnergyConsumer)health);

        public bool TryFire(IEnergyConsumer energySource = null)
        {
            if (!enabled || !gameObject.activeInHierarchy) return false;
            float minRate = combatConfig != null ? combatConfig.minFireRate : 0.10f;
            float effectiveFireRate = Mathf.Max(minRate, fireRate);
            if (Time.time - lastFiredTime < effectiveFireRate) return false;
            
            if (energySource != null && !energySource.ConsumeEnergy(energyCost))
            {
                return false;
            }

            lastFiredTime = Time.time;
            
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound(isPlayer ? "laser" : "enemy_laser", 0.4f);

            float recoil = combatConfig != null ? combatConfig.recoilForce : 1.5f;
            if (rb != null && recoil > 0f)
            {
                rb.AddForce(-transform.right * recoil, ForceMode2D.Impulse);
            }

            float spread = combatConfig != null ? combatConfig.dualWeaponSpread : 0.3f;
            float fwd = combatConfig != null ? combatConfig.muzzleForwardOffset : 0.65f;
            float[] offsets = CalculateMuzzleOffsets(weaponSlots, spread);

            foreach (float offset in offsets)
            {
                Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * fwd + (Vector2)transform.up * offset;
                
                string poolTag = isPlayer ? "PlayerLaser" : "EnemyLaser";
                
                GameObject laserObj = null;
                if (PoolManager.Instance != null)
                {
                    laserObj = PoolManager.Instance.Spawn(poolTag, nose, transform.rotation, () => 
                    {
                        if (laserPrefab != null)
                        {
                            var inst = Instantiate(laserPrefab, nose, transform.rotation);
                            inst.name = poolTag;
                            return inst;
                        }
                        GameObject obj = new GameObject(poolTag);
                        obj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        obj.AddComponent<LaserProjectile>();
                        return obj;
                    });
                }
                else
                {
                    // Fallback if no pool
                    if (laserPrefab != null)
                    {
                        laserObj = Instantiate(laserPrefab, nose, transform.rotation);
                        laserObj.name = poolTag;
                    }
                    else
                    {
                        laserObj = new GameObject(poolTag);
                        laserObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        laserObj.transform.position = nose;
                        laserObj.transform.rotation = transform.rotation;
                        laserObj.AddComponent<LaserProjectile>();
                    }
                }

                LaserProjectile proj = laserObj.GetComponent<LaserProjectile>();
                proj.Initialize(pulseDamage, pulseSpeed, !isPlayer);
            }

            return true;
        }
    }
}
