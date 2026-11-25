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
        private Transform cachedMuzzleTransform;
        private bool hasLookedUpMuzzle;
        private PlayerProgression cachedProgression;
        private IShipInputProvider cachedInputProvider;
        private bool hasLookedUpInputProvider;

        private readonly Vector2[] localBuf1 = new Vector2[1];
        private readonly Vector2[] localBuf2 = new Vector2[2];
        private readonly Vector2[] localBuf3 = new Vector2[3];
        private readonly Vector2[] worldBuf1 = new Vector2[1];
        private readonly Vector2[] worldBuf2 = new Vector2[2];
        private readonly Vector2[] worldBuf3 = new Vector2[3];
        private Vector2[] worldBufDynamic;

        public static bool CanShipAimIndependently(PlayerProgression prog)
        {
            if (prog == null || prog.currentShip == null) return false;
            var ship = prog.currentShip;
            return ship.canAimIndependently ||
                   ship.id == "cruiser" || ship.id == "battleship" ||
                   ship.id == "carrier" || ship.id == "vanguard" ||
                   ship.shipClass == ShipClass.Carrier ||
                   ship.shipClass == ShipClass.Cruiser ||
                   ship.shipClass == ShipClass.Battleship;
        }

        public static Vector2 ResolveIndependentAimDirection(
            Transform weaponTransform,
            bool isMounted,
            ref PlayerProgression cachedProg,
            ref IShipInputProvider cachedInput,
            ref bool hasLookedUpInput)
        {
            if (!isMounted)
            {
                if (cachedProg == null) cachedProg = weaponTransform.GetComponent<PlayerProgression>();
                if (CanShipAimIndependently(cachedProg))
                {
                    if (!hasLookedUpInput)
                    {
                        cachedInput = weaponTransform.GetComponent<IShipInputProvider>();
                        hasLookedUpInput = true;
                    }
                    Vector2 aimDir = cachedInput != null ? cachedInput.AimDirection : Vector2.zero;
                    if (aimDir.sqrMagnitude < 0.01f && PlayerInputReader.Instance != null)
                    {
                        aimDir = PlayerInputReader.Instance.AimDirection;
                    }
                    if (aimDir.sqrMagnitude > 0.01f)
                    {
                        return aimDir.normalized;
                    }
                }
            }
            return weaponTransform.right;
        }

        public static Quaternion ResolveIndependentAimRotation(
            Transform weaponTransform,
            bool isMounted,
            ref PlayerProgression cachedProg,
            ref IShipInputProvider cachedInput,
            ref bool hasLookedUpInput)
        {
            if (!isMounted)
            {
                if (cachedProg == null) cachedProg = weaponTransform.GetComponent<PlayerProgression>();
                if (CanShipAimIndependently(cachedProg))
                {
                    if (!hasLookedUpInput)
                    {
                        cachedInput = weaponTransform.GetComponent<IShipInputProvider>();
                        hasLookedUpInput = true;
                    }
                    Vector2 aimDir = cachedInput != null ? cachedInput.AimDirection : Vector2.zero;
                    if (aimDir.sqrMagnitude < 0.01f && PlayerInputReader.Instance != null)
                    {
                        aimDir = PlayerInputReader.Instance.AimDirection;
                    }
                    if (aimDir.sqrMagnitude > 0.01f)
                    {
                        return Quaternion.Euler(0f, 0f, Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg);
                    }
                }
            }
            return weaponTransform.rotation;
        }

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
                if (!hasLookedUpMuzzle)
                {
                    cachedMuzzleTransform = transform.Find("MuzzlePoint");
                    hasLookedUpMuzzle = true;
                }
                if (cachedMuzzleTransform != null)
                {
                    localBuf1[0] = (Vector2)cachedMuzzleTransform.localPosition;
                    return localBuf1;
                }
                if (weaponSlots == 2)
                {
                    localBuf2[0] = new Vector2(0.5f, 0.15f);
                    localBuf2[1] = new Vector2(0.5f, -0.15f);
                    return localBuf2;
                }
                if (weaponSlots >= 3)
                {
                    localBuf3[0] = new Vector2(0.5f, 0.15f);
                    localBuf3[1] = new Vector2(0.5f, 0f);
                    localBuf3[2] = new Vector2(0.5f, -0.15f);
                    return localBuf3;
                }
                localBuf1[0] = new Vector2(0.5f, 0f);
                return localBuf1;
            }

            if (weaponSockets == null || weaponSockets.Length == 0)
            {
                if (cachedProgression == null) cachedProgression = GetComponent<PlayerProgression>();
                if (cachedProgression != null && cachedProgression.currentShip != null && cachedProgression.currentShip.weaponSockets != null && cachedProgression.currentShip.weaponSockets.Length > 0)
                {
                    weaponSockets = cachedProgression.currentShip.weaponSockets;
                }
            }

            if (weaponSockets != null && weaponSockets.Length > 0)
            {
                if (weaponSockets.Length == 1)
                {
                    if (weaponSlots <= 1)
                    {
                        localBuf1[0] = weaponSockets[0];
                        return localBuf1;
                    }
                    if (weaponSlots == 2)
                    {
                        localBuf2[0] = weaponSockets[0] + new Vector2(0f, 0.35f);
                        localBuf2[1] = weaponSockets[0] - new Vector2(0f, 0.35f);
                        return localBuf2;
                    }
                    // 3 or more slots
                    localBuf3[0] = weaponSockets[0] + new Vector2(0f, 0.35f);
                    localBuf3[1] = weaponSockets[0];
                    localBuf3[2] = weaponSockets[0] - new Vector2(0f, 0.35f);
                    return localBuf3;
                }
                else if (weaponSockets.Length == 2)
                {
                    if (weaponSlots <= 1)
                    {
                        localBuf1[0] = (weaponSockets[0] + weaponSockets[1]) * 0.5f;
                        return localBuf1;
                    }
                    if (weaponSlots == 2)
                    {
                        localBuf2[0] = weaponSockets[0];
                        localBuf2[1] = weaponSockets[1];
                        return localBuf2;
                    }
                    // 3 or more slots
                    localBuf3[0] = weaponSockets[0];
                    localBuf3[1] = (weaponSockets[0] + weaponSockets[1]) * 0.5f;
                    localBuf3[2] = weaponSockets[1];
                    return localBuf3;
                }
                else
                {
                    // 3 or more physical sockets (e.g. index 0=center, index 1=port, index 2=starboard)
                    if (weaponSlots <= 1)
                    {
                        localBuf1[0] = weaponSockets[0];
                        return localBuf1;
                    }
                    if (weaponSlots == 2)
                    {
                        localBuf2[0] = weaponSockets[1];
                        localBuf2[1] = weaponSockets[2];
                        return localBuf2;
                    }
                    // 3 slots: center + port + starboard
                    localBuf3[0] = weaponSockets[0];
                    localBuf3[1] = weaponSockets[1];
                    localBuf3[2] = weaponSockets[2];
                    return localBuf3;
                }
            }

            // Fallback: use combatConfig parameters with CalculateMuzzleOffsets
            float spread = combatConfig != null ? combatConfig.dualWeaponSpread : 0.3f;
            float fwd = combatConfig != null ? combatConfig.muzzleForwardOffset : 1.55f;
            if (weaponSlots >= 3)
            {
                localBuf3[0] = new Vector2(fwd, -spread);
                localBuf3[1] = new Vector2(fwd, 0f);
                localBuf3[2] = new Vector2(fwd, spread);
                return localBuf3;
            }
            if (weaponSlots == 2)
            {
                localBuf2[0] = new Vector2(fwd, -spread);
                localBuf2[1] = new Vector2(fwd, spread);
                return localBuf2;
            }
            localBuf1[0] = new Vector2(fwd, 0f);
            return localBuf1;
        }

        public Vector2[] GetMuzzleWorldPositions()
        {
            Vector2[] locals = GetActiveSocketLocalPositions();
            Vector2[] worlds;
            if (locals.Length == 1) worlds = worldBuf1;
            else if (locals.Length == 2) worlds = worldBuf2;
            else if (locals.Length == 3) worlds = worldBuf3;
            else
            {
                if (worldBufDynamic == null || worldBufDynamic.Length != locals.Length)
                {
                    worldBufDynamic = new Vector2[locals.Length];
                }
                worlds = worldBufDynamic;
            }

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

            Quaternion baseRotation = ResolveIndependentAimRotation(
                transform, IsMountedOnSocket(), ref cachedProgression, ref cachedInputProvider, ref hasLookedUpInputProvider);

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
