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

        [Header("Accuracy & Spread")]
        [Tooltip("Random angular inaccuracy / spread applied to each fired bullet (in degrees)")]
        public float inaccuracyAngle = 2.5f;

        [Header("Physical Sockets")]
        [Tooltip("Custom weapon hardpoint positions in local coordinates (X = forward, Y = lateral)")]
        public Vector2[] weaponSockets = null;

        public Vector2[] WeaponSockets => weaponSockets;
        public void SetSockets(Vector2[] sockets) => weaponSockets = sockets;

        public string WeaponName => "Pulse Laser";
        public WeaponArchetype Archetype => WeaponArchetype.Projectile;
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => inaccuracyAngle;
        public void StopFiring() { }

        public bool isPlayer = false;

        public event System.Action OnFired;

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
                if (inaccuracyAngle <= 0f && combatConfig.bulletInaccuracyAngle > 0f)
                {
                    inaccuracyAngle = combatConfig.bulletInaccuracyAngle;
                }
            }

            float minRate = combatConfig != null ? combatConfig.minFireRate : 0.10f;
            if (fireRate < minRate) fireRate = minRate;

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = GetComponentInParent<Rigidbody2D>();

            if (CompareTag("Player") || gameObject.name == "Player" ||
                (transform.root != null && (transform.root.CompareTag("Player") || transform.root.name == "Player")) ||
                (transform.parent != null && (transform.parent.CompareTag("Player") || transform.parent.name == "Player")))
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

        private bool IsMountedOnSocket()
        {
            if (transform.parent != null && (transform.parent.name.StartsWith("SocketMount_") || transform.name.StartsWith("Mounted_") || transform.name.StartsWith("Weapon_")))
            {
                return true;
            }
            return false;
        }

        public Vector2[] GetActiveSocketLocalPositions()
        {
            if (IsMountedOnSocket())
            {
                var muzzleTransform = transform.Find("MuzzlePoint");
                if (muzzleTransform != null)
                {
                    return new Vector2[] { (Vector2)muzzleTransform.localPosition };
                }
                if (weaponSlots == 2)
                {
                    return new Vector2[] { new Vector2(0.5f, 0.15f), new Vector2(0.5f, -0.15f) };
                }
                if (weaponSlots >= 3)
                {
                    return new Vector2[] { new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0f), new Vector2(0.5f, -0.15f) };
                }
                return new Vector2[] { new Vector2(0.5f, 0f) };
            }

            if (weaponSockets == null || weaponSockets.Length == 0)
            {
                var prog = GetComponent<PlayerProgression>();
                if (prog != null && prog.currentShip != null && prog.currentShip.weaponSockets != null && prog.currentShip.weaponSockets.Length > 0)
                {
                    weaponSockets = prog.currentShip.weaponSockets;
                }
            }

            if (weaponSockets != null && weaponSockets.Length > 0)
            {
                if (weaponSockets.Length == 1)
                {
                    if (weaponSlots <= 1)
                    {
                        return new Vector2[] { weaponSockets[0] };
                    }
                    if (weaponSlots == 2)
                    {
                        return new Vector2[]
                        {
                            weaponSockets[0] + new Vector2(0f, 0.35f),
                            weaponSockets[0] - new Vector2(0f, 0.35f)
                        };
                    }
                    // 3 or more slots
                    return new Vector2[]
                    {
                        weaponSockets[0] + new Vector2(0f, 0.35f),
                        weaponSockets[0],
                        weaponSockets[0] - new Vector2(0f, 0.35f)
                    };
                }
                else if (weaponSockets.Length == 2)
                {
                    if (weaponSlots <= 1)
                    {
                        return new Vector2[] { (weaponSockets[0] + weaponSockets[1]) * 0.5f };
                    }
                    if (weaponSlots == 2)
                    {
                        return new Vector2[] { weaponSockets[0], weaponSockets[1] };
                    }
                    // 3 or more slots
                    return new Vector2[]
                    {
                        weaponSockets[0],
                        (weaponSockets[0] + weaponSockets[1]) * 0.5f,
                        weaponSockets[1]
                    };
                }
                else
                {
                    // 3 or more physical sockets (e.g. index 0=center, index 1=port, index 2=starboard)
                    if (weaponSlots <= 1)
                    {
                        return new Vector2[] { weaponSockets[0] };
                    }
                    if (weaponSlots == 2)
                    {
                        return new Vector2[] { weaponSockets[1], weaponSockets[2] };
                    }
                    // 3 slots: center + port + starboard
                    return new Vector2[] { weaponSockets[0], weaponSockets[1], weaponSockets[2] };
                }
            }

            // Fallback: use combatConfig parameters with CalculateMuzzleOffsets
            float spread = combatConfig != null ? combatConfig.dualWeaponSpread : 0.3f;
            float fwd = combatConfig != null ? combatConfig.muzzleForwardOffset : 1.55f;
            float[] offsets = CalculateMuzzleOffsets(weaponSlots, spread);
            Vector2[] fallback = new Vector2[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                fallback[i] = new Vector2(fwd, offsets[i]);
            }
            return fallback;
        }

        public Vector2[] GetMuzzleWorldPositions()
        {
            Vector2[] locals = GetActiveSocketLocalPositions();
            Vector2[] worlds = new Vector2[locals.Length];
            for (int i = 0; i < locals.Length; i++)
            {
                worlds[i] = (Vector2)transform.TransformPoint(locals[i]);
            }
            return worlds;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2[] muzzles = GetMuzzleWorldPositions();
            if (muzzles != null)
            {
                foreach (var pos in muzzles)
                {
                    Gizmos.DrawWireSphere(pos, 0.12f);
                    Gizmos.DrawRay(pos, transform.right * 0.4f);
                }
            }
        }
#endif

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

            // Recoil mechanism removed: zero shock-back impulse on weapon fire

            Vector2[] muzzlePositions = GetMuzzleWorldPositions();

            Quaternion baseRotation = transform.rotation;
            if (!IsMountedOnSocket())
            {
                var prog = GetComponent<PlayerProgression>();
                bool canAim = (prog != null && prog.currentShip != null && (prog.currentShip.canAimIndependently || prog.currentShip.id == "cruiser" || prog.currentShip.id == "battleship"));
                if (canAim)
                {
                    var inputProvider = GetComponent<IShipInputProvider>();
                    Vector2 aimDir = inputProvider != null ? inputProvider.AimDirection : Vector2.zero;
                    if (aimDir.sqrMagnitude < 0.01f && PlayerInputReader.Instance != null)
                    {
                        aimDir = PlayerInputReader.Instance.AimDirection;
                    }
                    if (aimDir.sqrMagnitude > 0.01f)
                    {
                        baseRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg);
                    }
                }
            }

            foreach (Vector2 nose in muzzlePositions)
            {
                float spreadOffset = inaccuracyAngle > 0f ? Random.Range(-inaccuracyAngle, inaccuracyAngle) : 0f;
                Quaternion bulletRotation = baseRotation * Quaternion.Euler(0f, 0f, spreadOffset);

                string poolTag = isPlayer ? "PlayerLaser" : "EnemyLaser";
                
                GameObject laserObj = null;
                if (PoolManager.Instance != null)
                {
                    laserObj = PoolManager.Instance.Spawn(poolTag, nose, bulletRotation, () => 
                    {
                        if (laserPrefab != null)
                        {
                            var inst = Instantiate(laserPrefab, nose, bulletRotation);
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
                        laserObj = Instantiate(laserPrefab, nose, bulletRotation);
                        laserObj.name = poolTag;
                    }
                    else
                    {
                        laserObj = new GameObject(poolTag);
                        laserObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        laserObj.transform.position = nose;
                        laserObj.transform.rotation = bulletRotation;
                        laserObj.AddComponent<LaserProjectile>();
                    }
                }

                laserObj.transform.position = nose;
                laserObj.transform.rotation = bulletRotation;

                LaserProjectile proj = laserObj.GetComponent<LaserProjectile>();
                proj.Initialize(pulseDamage, pulseSpeed, !isPlayer);
            }

            OnFired?.Invoke();

            return true;
        }
    }
}
