using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ShipWeapon : MonoBehaviour
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
        public float energyCost = 12f;

        public bool isPlayer = false;

        private float lastFiredTime = -999f;
        private Rigidbody2D rb;

        private void Awake()
        {
            if (combatConfig == null) combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (combatConfig == null) combatConfig = ScriptableObject.CreateInstance<CombatConfig>();

            rb = GetComponent<Rigidbody2D>();
            if (CompareTag("Player") || gameObject.name == "Player")
            {
                isPlayer = true;
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

        public bool TryFire(ShipHealth health)
        {
            if (Time.time - lastFiredTime < fireRate) return false;
            
            if (health != null && !health.ConsumeEnergy(energyCost))
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
