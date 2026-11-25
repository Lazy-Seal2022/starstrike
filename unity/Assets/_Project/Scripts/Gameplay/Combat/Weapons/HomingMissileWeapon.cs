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
        public float recoilForce = 0f;

        [Header("Prefabs")]
        public GameObject missilePrefab;

        public string WeaponName => "Homing Missiles";
        public WeaponArchetype Archetype => WeaponArchetype.Traceable;
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => 0f;

        [Header("Physical Sockets")]
        public Vector2[] customSockets = null;
        public Vector2[] WeaponSockets => customSockets;
        public void SetSockets(Vector2[] sockets) => customSockets = sockets;

        private float lastFiredTime = -999f;
        public void ResetCooldown() => lastFiredTime = -999f;
        public void StopFiring() { }

        private Rigidbody2D rb;
        public bool isPlayer = false;
        private Transform cachedMuzzleTransform;
        private bool hasLookedUpMuzzle;
        private ShipWeapon cachedShipWeapon;
        private bool hasLookedUpShipWeapon;
        private PlayerProgression cachedProgression;
        private IShipInputProvider cachedInputProvider;
        private bool hasLookedUpInputProvider;
        private readonly Vector2[] singleMuzzleBuffer = new Vector2[1];
        private Vector2[] customMuzzleBuffer;
        private Vector2[] fallbackMuzzleBuffer;

        private bool IsMountedOnSocket()
        {
            if (transform.parent != null && (transform.parent.name.StartsWith("SocketMount_") || transform.name.StartsWith("Mounted_") || transform.name.StartsWith("Weapon_")))
            {
                return true;
            }
            return false;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = GetComponentInParent<Rigidbody2D>();

            if (CompareTag("Player") || gameObject.name == "Player" ||
                (transform.root != null && (transform.root.CompareTag("Player") || transform.root.name == "Player")) ||
                (transform.parent != null && (transform.parent.CompareTag("Player") || transform.parent.name == "Player")))
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

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("missile_launch", 0.5f);

            // Recoil mechanism removed: zero shock-back impulse on weapon fire

            Vector2[] muzzles = null;
            if (IsMountedOnSocket())
            {
                if (!hasLookedUpMuzzle)
                {
                    cachedMuzzleTransform = transform.Find("MuzzlePoint");
                    hasLookedUpMuzzle = true;
                }
                Vector2 localMuzzle = cachedMuzzleTransform != null ? (Vector2)cachedMuzzleTransform.localPosition : new Vector2(0.5f, 0f);
                singleMuzzleBuffer[0] = (Vector2)transform.TransformPoint(localMuzzle);
                muzzles = singleMuzzleBuffer;
            }
            else if (customSockets != null && customSockets.Length > 0)
            {
                if (customMuzzleBuffer == null || customMuzzleBuffer.Length != customSockets.Length)
                {
                    customMuzzleBuffer = new Vector2[customSockets.Length];
                }
                for (int i = 0; i < customSockets.Length; i++)
                {
                    customMuzzleBuffer[i] = (Vector2)transform.TransformPoint(customSockets[i]);
                }
                muzzles = customMuzzleBuffer;
            }
            else
            {
                if (!hasLookedUpShipWeapon)
                {
                    cachedShipWeapon = GetComponent<ShipWeapon>();
                    hasLookedUpShipWeapon = true;
                }
                muzzles = cachedShipWeapon != null ? cachedShipWeapon.GetMuzzleWorldPositions() : null;
            }
            if (muzzles == null || muzzles.Length == 0)
            {
                float spread = 0.4f;
                float fwd = 1.55f;
                float[] offsets = ShipWeapon.CalculateMuzzleOffsets(weaponSlots, spread);
                if (fallbackMuzzleBuffer == null || fallbackMuzzleBuffer.Length != offsets.Length)
                {
                    fallbackMuzzleBuffer = new Vector2[offsets.Length];
                }
                for (int i = 0; i < offsets.Length; i++)
                {
                    fallbackMuzzleBuffer[i] = (Vector2)transform.TransformPoint(new Vector2(fwd, offsets[i]));
                }
                muzzles = fallbackMuzzleBuffer;
            }

            Quaternion baseRotation = ShipWeapon.ResolveIndependentAimRotation(
                transform, IsMountedOnSocket(), ref cachedProgression, ref cachedInputProvider, ref hasLookedUpInputProvider);

            foreach (Vector2 nose in muzzles)
            {
                string poolTag = isPlayer ? "PlayerMissile" : "EnemyMissile";

                GameObject missileObj = null;
                if (PoolManager.Instance != null)
                {
                    missileObj = PoolManager.Instance.Spawn(poolTag, nose, baseRotation, () =>
                    {
                        if (missilePrefab != null)
                        {
                            var inst = Instantiate(missilePrefab, nose, baseRotation);
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
                        missileObj = Instantiate(missilePrefab, nose, baseRotation);
                        missileObj.name = poolTag;
                    }
                    else
                    {
                        missileObj = new GameObject(poolTag);
                        missileObj.tag = isPlayer ? "Laser" : "EnemyLaser";
                        missileObj.transform.position = nose;
                        missileObj.transform.rotation = baseRotation;
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
