using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    /// <summary>
    /// High-RPM rotary autocannon / Gatling gun with continuous fire spool-up and dynamic angular spread bloom.
    /// Features rapid alternating socket fire, punchy kinetic recoil, and high-velocity kinetic tracer projectiles.
    /// </summary>
    public class GatlingWeapon : MonoBehaviour, IWeapon
    {
        [Header("Configuration")]
        public CombatConfig combatConfig;

        [Header("Gatling Stats")]
        public float bulletDamage = 8.5f;
        public float bulletSpeed = 34f;
        public float baseFireRate = 0.10f; // ~10 rounds/sec initial
        public float maxFireRate = 0.055f; // ~18.2 rounds/sec at peak RPM
        public float energyCost = 1.6f;
        public int weaponSlots = 1;
        public float recoilForce = 0.35f;

        [Header("Accuracy & Spread Bloom")]
        [Tooltip("Starting inaccuracy angle in degrees when cold")]
        public float baseInaccuracyAngle = 3.2f;
        [Tooltip("Maximum inaccuracy angle in degrees when fully spooled up")]
        public float maxInaccuracyAngle = 7.5f;
        [Tooltip("Seconds of continuous firing required to reach peak RPM and maximum spread bloom")]
        public float spoolUpTime = 0.65f;
        [Tooltip("Seconds before spool begins decaying when fire is stopped")]
        public float spoolDecayTime = 0.4f;

        [Header("Prefabs")]
        public GameObject tracerPrefab;

        public string WeaponName => "Gatling Gun";
        public float FireRate { get => CurrentFireRate; set => baseFireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => CurrentInaccuracyAngle;

        public float SpoolProgress => spoolProgress;
        public float CurrentFireRate => Mathf.Lerp(baseFireRate, maxFireRate, spoolProgress);
        public float CurrentInaccuracyAngle => Mathf.Lerp(baseInaccuracyAngle, maxInaccuracyAngle, spoolProgress);

        private float lastFiredTime = -999f;
        private float spoolProgress = 0f;
        private float lastFireRequestTime = -999f;
        private int currentSocketIndex = 0;
        private Rigidbody2D rb;
        public bool isPlayer = false;

        public void ResetCooldown() => lastFiredTime = -999f;
        public void StopFiring() { }

        private void Awake()
        {
            if (combatConfig == null) combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (combatConfig == null) combatConfig = ScriptableObject.CreateInstance<CombatConfig>();

            rb = GetComponent<Rigidbody2D>();
            if (CompareTag("Player") || gameObject.name == "Player")
            {
                isPlayer = true;
            }

            if (tracerPrefab == null && combatConfig != null)
            {
                tracerPrefab = isPlayer ? combatConfig.playerLaserPrefab : combatConfig.enemyLaserPrefab;
            }

            if (tracerPrefab == null)
            {
                string path = isPlayer ? "Prefabs/Projectiles/PlayerLaser" : "Prefabs/Projectiles/EnemyLaser";
                tracerPrefab = Resources.Load<GameObject>(path);
            }
        }

        private void Update()
        {
            // Smoothly decay spool when trigger has been released
            if (Time.time - lastFireRequestTime > spoolDecayTime && spoolProgress > 0f)
            {
                spoolProgress = Mathf.MoveTowards(spoolProgress, 0f, Time.deltaTime * 2.2f);
            }
        }

        public bool TryFire(ShipHealth health) => TryFire((IEnergyConsumer)health);

        public bool TryFire(IEnergyConsumer energySource = null)
        {
            if (!enabled || !gameObject.activeInHierarchy) return false;

            float effectiveFireRate = CurrentFireRate;
            if (Time.time - lastFiredTime < effectiveFireRate) return false;

            if (energySource != null && !energySource.ConsumeEnergy(energyCost))
            {
                return false;
            }

            lastFiredTime = Time.time;
            lastFireRequestTime = Time.time;

            // Spool up with continuous firing
            spoolProgress = Mathf.Clamp01(spoolProgress + (effectiveFireRate / spoolUpTime));

            // Rapid mechanical audio
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound(isPlayer ? "laser" : "enemy_laser", 0.35f);

            // Light rapid micro-recoil impulse
            if (rb != null && recoilForce > 0f)
            {
                rb.AddForce(-transform.right * recoilForce, ForceMode2D.Impulse);
            }

            // Muzzle positioning: supports ShipWeapon hardpoints or fallback forward nose
            Vector2[] muzzlePositions = GetMuzzleWorldPositions();

            // Alternate or cycle hardpoint sockets
            Vector2 firingMuzzle = muzzlePositions.Length > 0 ? muzzlePositions[currentSocketIndex % muzzlePositions.Length] : (Vector2)transform.position + (Vector2)transform.right * 0.7f;
            currentSocketIndex = (currentSocketIndex + 1) % Mathf.Max(1, muzzlePositions.Length);

            // Apply dynamic inaccuracy spread bloom
            float currentSpread = CurrentInaccuracyAngle;
            float spreadOffset = Random.Range(-currentSpread, currentSpread);
            Quaternion bulletRotation = transform.rotation * Quaternion.Euler(0f, 0f, spreadOffset);

            string poolTag = isPlayer ? "PlayerGatlingBullet" : "EnemyGatlingBullet";
            GameObject bulletObj = null;

            if (PoolManager.Instance != null)
            {
                bulletObj = PoolManager.Instance.Spawn(poolTag, firingMuzzle, bulletRotation, () =>
                {
                    if (tracerPrefab != null)
                    {
                        var inst = Instantiate(tracerPrefab, firingMuzzle, bulletRotation);
                        inst.name = poolTag;
                        return inst;
                    }
                    GameObject go = new GameObject(poolTag);
                    go.tag = isPlayer ? "Laser" : "EnemyLaser";
                    go.AddComponent<LaserProjectile>();
                    return go;
                });
            }
            else
            {
                if (tracerPrefab != null)
                {
                    bulletObj = Instantiate(tracerPrefab, firingMuzzle, bulletRotation);
                    bulletObj.name = poolTag;
                }
                else
                {
                    bulletObj = new GameObject(poolTag);
                    bulletObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                    bulletObj.transform.position = firingMuzzle;
                    bulletObj.transform.rotation = bulletRotation;
                    bulletObj.AddComponent<LaserProjectile>();
                }
            }

            bulletObj.transform.position = firingMuzzle;
            bulletObj.transform.rotation = bulletRotation;

            LaserProjectile proj = bulletObj.GetComponent<LaserProjectile>();
            if (proj != null)
            {
                proj.Initialize(bulletDamage, bulletSpeed, !isPlayer);

                // Give gatling tracers a punchy golden-amber kinetic tint & shape
                var sr = bulletObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = isPlayer ? new Color(1f, 0.85f, 0.25f, 1f) : new Color(1f, 0.45f, 0.15f, 1f);
                    sr.sprite = ProceduralSpriteHelper.GetGatlingTracerSprite();
                }
            }

            return true;
        }

        public Vector2[] GetMuzzleWorldPositions()
        {
            var shipWp = GetComponent<ShipWeapon>();
            if (shipWp != null)
            {
                return shipWp.GetMuzzleWorldPositions();
            }

            Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * 0.7f;
            return new Vector2[] { nose };
        }
    }
}
