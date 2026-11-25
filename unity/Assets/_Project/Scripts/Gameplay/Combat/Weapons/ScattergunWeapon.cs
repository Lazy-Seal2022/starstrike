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
        public float recoilForce = 0f;

        [Header("Prefabs")]
        public GameObject pelletPrefab;

        public string WeaponName => "Scattergun";
        public WeaponArchetype Archetype => WeaponArchetype.Projectile;
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCost;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => spreadAngle;

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
            if (combatConfig == null) combatConfig = Resources.Load<CombatConfig>("Data/CombatConfig");
            if (combatConfig == null) combatConfig = ScriptableObject.CreateInstance<CombatConfig>();

            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = GetComponentInParent<Rigidbody2D>();

            if (CompareTag("Player") || gameObject.name == "Player" ||
                (transform.root != null && (transform.root.CompareTag("Player") || transform.root.name == "Player")) ||
                (transform.parent != null && (transform.parent.CompareTag("Player") || transform.parent.name == "Player")))
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

            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("scattergun", 0.6f);

            // Recoil mechanism removed: zero shock-back impulse on weapon fire

            int totalPellets = basePellets + Mathf.Max(0, weaponSlots - 1) * 2;
            float startAngle = -spreadAngle * 0.5f;
            float step = totalPellets > 1 ? spreadAngle / (totalPellets - 1) : 0f;
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
                singleMuzzleBuffer[0] = (Vector2)transform.position + (Vector2)transform.right * 1.55f;
                muzzles = singleMuzzleBuffer;
            }

            Quaternion baseRotation = ShipWeapon.ResolveIndependentAimRotation(
                transform, IsMountedOnSocket(), ref cachedProgression, ref cachedInputProvider, ref hasLookedUpInputProvider);

            foreach (Vector2 nose in muzzles)
            {
                for (int i = 0; i < totalPellets; i++)
                {
                    float angle = startAngle + (step * i);
                    Quaternion rot = baseRotation * Quaternion.Euler(0, 0, angle);

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
            }

            return true;
        }
    }
}
