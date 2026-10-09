using UnityEngine;

namespace StarStrike.Gameplay
{
    public class ShipWeapon : MonoBehaviour
    {
        [Header("Stats")]
        public float pulseDamage = 18f;
        public float pulseSpeed = 22f;
        public int weaponSlots = 1;
        public float fireRate = 0.22f;
        public float energyCost = 12f;

        public bool isPlayer = false;

        private float lastFiredTime;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public bool TryFire(ShipHealth health)
        {
            if (Time.time - lastFiredTime < fireRate) return false;
            
            if (health != null && !health.ConsumeEnergy(energyCost))
            {
                return false;
            }

            lastFiredTime = Time.time;

            if (rb != null)
            {
                rb.AddForce(-transform.right * 1.5f, ForceMode2D.Impulse);
            }

            float[] offsets = weaponSlots == 2 ? new float[] { -0.3f, 0.3f } : new float[] { 0f };

            foreach (float offset in offsets)
            {
                Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * 0.65f + (Vector2)transform.up * offset;
                
                string poolTag = isPlayer ? "PlayerLaser" : "EnemyLaser";
                
                GameObject laserObj = null;
                if (PoolManager.Instance != null)
                {
                    laserObj = PoolManager.Instance.Spawn(poolTag, nose, transform.rotation, () => 
                    {
                        GameObject obj = new GameObject(poolTag);
                        obj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        obj.AddComponent<LaserProjectile>();
                        return obj;
                    });
                }
                else
                {
                    // Fallback if no pool
                    laserObj = new GameObject(poolTag);
                    laserObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                    laserObj.transform.position = nose;
                    laserObj.transform.rotation = transform.rotation;
                    laserObj.AddComponent<LaserProjectile>();
                }

                LaserProjectile proj = laserObj.GetComponent<LaserProjectile>();
                proj.Initialize(pulseDamage, pulseSpeed, !isPlayer);
            }

            return true;
        }
    }
}
