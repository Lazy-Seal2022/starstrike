using UnityEngine;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Gameplay.Carrier
{
    /// <summary>
    /// Autonomous carrier-deployed escort strike drone that defends the carrier or executes strike sorties.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CarrierEscortDrone : MonoBehaviour, IDamageable
    {
        [Header("Identity & Mother Ship")]
        public CarrierHangarController carrier;
        public int droneIndex = 0;

        [Header("Defensive Systems")]
        public float maxHull = 60f;
        public float currentHull = 60f;
        public float maxShield = 80f;
        public float currentShield = 80f;
        public float shieldRegen = 16f;
        public float shieldRegenDelay = 2.5f;

        [Header("Flight Kinematics")]
        public float maxSpeed = 15f;
        public float acceleration = 24f;
        public float rotationSpeed = 380f;
        public float orbitDistance = 4.0f;

        [Header("Combat & Armament")]
        public float attackRange = 9.5f;
        public float pulseDamage = 16f;
        public float pulseSpeed = 26f;
        public float fireRate = 0.35f;

        [Header("Current State")]
        public bool isDocked = false;
        public Transform currentTarget;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D col;
        private float lastFireTime = -999f;
        private float lastDamageTime = -999f;
        private float nextTargetSearchTime = 0f;
        private Vector2 velocity;
        private static GameObject cachedDroneLaserPrefab;
        private static readonly Collider2D[] overlapBuffer = new Collider2D[64];
        private static readonly ContactFilter2D overlapFilter = new ContactFilter2D { useTriggers = true, useLayerMask = false };

        public float HullPercentage => maxHull > 0f ? Mathf.Clamp01(currentHull / maxHull) : 0f;
        public float ShieldPercentage => maxShield > 0f ? Mathf.Clamp01(currentShield / maxShield) : 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 1.2f;
                rb.angularDamping = 3f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }

            col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = ProceduralSpriteHelper.GetEscortDroneSprite();
            }
            spriteRenderer.sortingOrder = 4;
            transform.localScale = new Vector3(0.65f, 0.65f, 1f);

            gameObject.tag = "Untagged";
        }

        public void Initialize(CarrierHangarController motherCarrier, int index)
        {
            carrier = motherCarrier;
            droneIndex = index;
            currentHull = maxHull;
            currentShield = maxShield;
            isDocked = false;
        }

        private void Update()
        {
            if (carrier == null)
            {
                Destroy(gameObject);
                return;
            }

            // Shield regeneration
            if (Time.time - lastDamageTime >= shieldRegenDelay && currentShield < maxShield)
            {
                currentShield = Mathf.Min(maxShield, currentShield + shieldRegen * Time.deltaTime);
            }

            if (isDocked)
            {
                HandleDockedState();
                return;
            }

            DroneCommandStance stance = carrier.CurrentStance;
            switch (stance)
            {
                case DroneCommandStance.DefensiveEscort:
                    HandleDefensiveEscort();
                    break;
                case DroneCommandStance.OffensiveStrike:
                    HandleOffensiveStrike();
                    break;
                case DroneCommandStance.DockAndRepair:
                    HandleDockingFlight();
                    break;
            }
        }

        private void HandleDockedState()
        {
            // Rapid dock repair
            currentShield = Mathf.Min(maxShield, currentShield + 50f * Time.deltaTime);
            currentHull = Mathf.Min(maxHull, currentHull + 30f * Time.deltaTime);

            // Follow carrier launch bay
            Vector2 bayPos = carrier.GetLaunchBayWorldPosition(droneIndex % 2);
            transform.position = bayPos;
            transform.rotation = carrier.transform.rotation;

            // Undock if carrier switched stance and shields are restored
            if (carrier.CurrentStance != DroneCommandStance.DockAndRepair && currentShield >= maxShield * 0.9f)
            {
                isDocked = false;
                if (spriteRenderer != null) spriteRenderer.enabled = true;
                if (col != null) col.enabled = true;
            }
        }

        private void HandleDefensiveEscort()
        {
            // Calculate orbital escort position around carrier
            float baseAngle = (droneIndex * (360f / Mathf.Max(1, carrier.droneCapacity))) + (Time.time * 28f);
            Vector2 orbitOffset = new Vector2(Mathf.Cos(baseAngle * Mathf.Deg2Rad), Mathf.Sin(baseAngle * Mathf.Deg2Rad)) * orbitDistance;
            Vector2 targetPos = (Vector2)carrier.transform.position + orbitOffset;

            // Steer towards formation position
            MoveTowards(targetPos);

            // Scan for nearby threats to intercept
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || Time.time >= nextTargetSearchTime)
            {
                nextTargetSearchTime = Time.time + 0.15f;
                currentTarget = FindBestTarget(transform.position, attackRange);
            }
            if (currentTarget != null)
            {
                AimAt(currentTarget.position);
                TryFireAt(currentTarget.position);
            }
            else
            {
                // Align flight heading with carrier
                AimAt(transform.position + (Vector3)carrier.transform.right);
            }
        }

        private void HandleOffensiveStrike()
        {
            Vector2 strikeTarget = carrier.CurrentAimPosition;

            // Prioritize enemies near the target or within strike range
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || Time.time >= nextTargetSearchTime)
            {
                nextTargetSearchTime = Time.time + 0.15f;
                currentTarget = FindBestTarget(strikeTarget, 7.5f);
                if (currentTarget == null)
                {
                    currentTarget = FindBestTarget(transform.position, attackRange * 1.5f);
                }
            }

            Vector2 moveTarget = (currentTarget != null) ? (Vector2)currentTarget.position : strikeTarget;

            // Circle around strike target at combat distance
            float strafeAngle = (droneIndex * 90f) + (Time.time * 45f);
            Vector2 strafeOffset = new Vector2(Mathf.Cos(strafeAngle * Mathf.Deg2Rad), Mathf.Sin(strafeAngle * Mathf.Deg2Rad)) * 3.2f;
            Vector2 desiredPos = moveTarget + strafeOffset;

            MoveTowards(desiredPos);

            if (currentTarget != null)
            {
                AimAt(currentTarget.position);
                TryFireAt(currentTarget.position);
            }
            else
            {
                AimAt(strikeTarget);
            }
        }

        private void HandleDockingFlight()
        {
            Vector2 bayPos = carrier.GetLaunchBayWorldPosition(droneIndex % 2);
            MoveTowards(bayPos);
            AimAt(bayPos);

            if (Vector2.Distance(transform.position, bayPos) < 0.85f)
            {
                isDocked = true;
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                if (col != null) col.enabled = false;
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }

        private void MoveTowards(Vector2 targetPos)
        {
            Vector2 currentPos = transform.position;
            Vector2 diff = targetPos - currentPos;
            Vector2 desiredVelocity = diff.normalized * Mathf.Min(maxSpeed, diff.magnitude * 4f);

            velocity = Vector2.MoveTowards(velocity, desiredVelocity, acceleration * Time.deltaTime);
            if (rb != null)
            {
                rb.linearVelocity = velocity;
            }
            else
            {
                transform.position = (Vector2)transform.position + velocity * Time.deltaTime;
            }
        }

        private void AimAt(Vector2 targetPos)
        {
            Vector2 dir = targetPos - (Vector2)transform.position;
            if (dir.sqrMagnitude < 0.001f) return;

            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        private void TryFireAt(Vector2 targetPos)
        {
            if (Time.time - lastFireTime < fireRate) return;
            lastFireTime = Time.time;

            Vector2 fireDir = (targetPos - (Vector2)transform.position).normalized;
            Vector3 muzzlePos = transform.position + (Vector3)fireDir * 0.45f;
            Quaternion rot = Quaternion.Euler(0f, 0f, Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg);

            if (cachedDroneLaserPrefab == null)
            {
                cachedDroneLaserPrefab = Resources.Load<GameObject>("Prefabs/Projectiles/PlayerLaser");
            }

            // Spawn pooled cyan player laser projectile
            GameObject laserObj;
            if (PoolManager.Instance != null)
            {
                laserObj = PoolManager.Instance.Spawn("PlayerLaser", muzzlePos, rot, () =>
                {
                    if (cachedDroneLaserPrefab != null)
                    {
                        var inst = Instantiate(cachedDroneLaserPrefab, muzzlePos, rot);
                        inst.name = "PlayerLaser";
                        return inst;
                    }
                    var go = new GameObject("PlayerLaser");
                    go.tag = "Laser";
                    go.AddComponent<LaserProjectile>();
                    return go;
                });
            }
            else if (cachedDroneLaserPrefab != null)
            {
                laserObj = Instantiate(cachedDroneLaserPrefab, muzzlePos, rot);
                laserObj.name = "PlayerLaser";
            }
            else
            {
                laserObj = new GameObject("EscortDrone_Laser");
                laserObj.transform.position = muzzlePos;
                laserObj.transform.rotation = rot;
                laserObj.tag = "Laser";
                laserObj.AddComponent<LaserProjectile>();
            }

            laserObj.transform.position = muzzlePos;
            laserObj.transform.rotation = rot;
            var laser = laserObj.GetComponent<LaserProjectile>();
            if (laser != null)
            {
                laser.Initialize(pulseDamage, pulseSpeed, false);
            }
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, (Vector2)transform.position);
        }

        public void TakeDamage(float damage, Vector2 hitPoint)
        {
            lastDamageTime = Time.time;
            if (currentShield > 0f)
            {
                float shieldDmg = Mathf.Min(currentShield, damage);
                currentShield -= shieldDmg;
                damage -= shieldDmg;
            }

            if (damage > 0f)
            {
                currentHull -= damage;
            }

            if (currentHull <= 0f)
            {
                OnDroneDestroyed();
            }
        }

        private void OnDroneDestroyed()
        {
            if (carrier != null)
            {
                carrier.NotifyDroneDestroyed(this);
            }
            Destroy(gameObject);
        }

        private Transform FindBestTarget(Vector2 searchOrigin, float radius)
        {
            int hitCount = Physics2D.OverlapCircle(searchOrigin, radius, overlapFilter, overlapBuffer);
            Transform best = null;
            float closestSqrDist = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = overlapBuffer[i];
                if (hit == null || hit.gameObject == gameObject) continue;
                if (hit.CompareTag("Player") || hit.CompareTag("Laser")) continue;

                bool isEnemy = hit.CompareTag("Enemy") || hit.CompareTag("Asteroid") || hit.CompareTag("Boss") ||
                               hit.name.Contains("Drone") || hit.name.Contains("Asteroid");

                if (isEnemy)
                {
                    float sqrDist = ((Vector2)hit.transform.position - searchOrigin).sqrMagnitude;
                    if (sqrDist < closestSqrDist)
                    {
                        closestSqrDist = sqrDist;
                        best = hit.transform;
                    }
                }
            }

            return best;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision == null || isDocked) return;
            if (collision.CompareTag("EnemyLaser"))
            {
                var proj = collision.GetComponent<IProjectile>();
                float dmg = proj != null ? proj.Damage : 15f;
                TakeDamage(dmg);
                collision.gameObject.SetActive(false);
            }
            else if (collision.CompareTag("Asteroid"))
            {
                TakeDamage(25f);
            }
            else if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
            {
                TakeDamage(30f);
            }
        }
    }
}