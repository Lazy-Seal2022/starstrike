using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ScattergunWeapon : MonoBehaviour, IWeapon
    {
        [Header("Configuration")]
        public CombatConfig combatConfig;

        [Header("Weapon Stats")]
        public float pelletDamage = 10f;
        public float pelletSpeed = 26f;
        public int basePellets = 5;
        public float spreadAngle = 32f;
        public float fireRate = 0.48f;
        public float energyCost = 14f;
        public int weaponSlots = 1;
        public float recoilForce = 3.2f;

        [Header("Prefabs")]
        public GameObject pelletPrefab;

        public string WeaponName => "Scattergun";
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => spreadAngle;

        private float lastFiredTime = -999f;
        public void ResetCooldown() => lastFiredTime = -999f;
        public void StopFiring() { }

        private Rigidbody2D rb;
        public bool isPlayer = false;

        private void Awake()
        {
            if (combatConfig == null) combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (combatConfig == null) combatConfig = ScriptableObject.CreateInstance<CombatConfig>();

            rb = GetComponent<Rigidbody2D>();
            if (CompareTag("Player") || gameObject.name == "Player")
            {
                isPlayer = true;
            }

            if (pelletPrefab == null && combatConfig != null && isPlayer && combatConfig.scattergunPelletPrefab != null)
            {
                pelletPrefab = combatConfig.scattergunPelletPrefab;
            }

            if (pelletPrefab == null)
            {
                pelletPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/PlayerScatterPellet");
            }

            if (pelletPrefab == null && combatConfig != null)
            {
                pelletPrefab = combatConfig.playerLaserPrefab;
            }

            if (pelletPrefab == null)
            {
                pelletPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/PlayerLaser");
            }
        }

        public bool TryFire(ShipHealth health) => TryFire((IEnergyConsumer)health);

        public bool TryFire(IEnergyConsumer energySource = null)
        {
            float minRate = combatConfig != null ? combatConfig.minFireRate : 0.10f;
            float effectiveFireRate = Mathf.Max(minRate, fireRate);
            if (Time.time - lastFiredTime < effectiveFireRate) return false;

            if (energySource != null && !energySource.ConsumeEnergy(energyCost))
            {
                return false;
            }

            lastFiredTime = Time.time;

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("laser", 0.6f);

            if (rb != null && recoilForce > 0f)
            {
                rb.AddForce(-transform.right * recoilForce, ForceMode2D.Impulse);
            }

            int totalPellets = basePellets + Mathf.Max(0, weaponSlots - 1) * 2;
            float startAngle = -spreadAngle * 0.5f;
            float step = totalPellets > 1 ? spreadAngle / (totalPellets - 1) : 0f;
            Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * 0.65f;

            for (int i = 0; i < totalPellets; i++)
            {
                float angle = startAngle + (step * i);
                Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, angle);

                GameObject pelletObj = null;
                string poolTag = isPlayer ? "PlayerScatterPellet" : "EnemyScatterPellet";

                if (PoolManager.Instance != null)
                {
                    pelletObj = PoolManager.Instance.Spawn(poolTag, nose, rot, () =>
                    {
                        if (pelletPrefab != null)
                        {
                            var inst = Instantiate(pelletPrefab, nose, rot);
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
                    if (pelletPrefab != null)
                    {
                        pelletObj = Instantiate(pelletPrefab, nose, rot);
                        pelletObj.name = poolTag;
                    }
                    else
                    {
                        pelletObj = new GameObject(poolTag);
                        pelletObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        pelletObj.transform.position = nose;
                        pelletObj.transform.rotation = rot;
                        pelletObj.AddComponent<LaserProjectile>();
                    }
                }

                LaserProjectile proj = pelletObj.GetComponent<LaserProjectile>();
                if (proj != null)
                {
                    proj.Initialize(pelletDamage, pelletSpeed, !isPlayer);
                    proj.lifetime = 0.85f; // Short-range shotgun falloff

                    var sr = pelletObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = ProceduralSpriteHelper.GetPelletSprite();
                    }
                }
            }

            return true;
        }
    }
}
