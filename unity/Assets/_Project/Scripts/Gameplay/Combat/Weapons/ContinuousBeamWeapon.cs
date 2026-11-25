using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    public class ContinuousBeamWeapon : MonoBehaviour, IWeapon
    {
        [Header("Beam Stats")]
        public float damagePerSecond = 75f;
        public float energyCostPerSecond = 22f;
        public float maxRange = 16f;
        public int weaponSlots = 1;
        public float fireRate = 0.05f; // Tick evaluation rate

        [Header("Target Lock-On")]
        [Range(0f, 90f)]
        [Tooltip("Angular half-cone radius in degrees around weapon aim direction to snap onto objectives.")]
        public float aimConeHalfAngle = 20f;

        [Min(0f)]
        [Tooltip("Close-range lock distance from ship/muzzle so enemies right in front of the ship still lock on even when too close for the 20-degree cone.")]
        public float closeRangeLockRadius = 3.5f;

        [Header("Visuals")]
        public Color beamColor = new Color(0.2f, 0.95f, 1f, 0.9f);
        public LineRenderer beamRenderer; // Primary line renderer (socket 0)

        private readonly List<LineRenderer> activeRenderers = new List<LineRenderer>();
        private readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[32];
        private readonly Collider2D[] targetOverlapBuffer = new Collider2D[128];
        private static readonly ContactFilter2D raycastFilter = new ContactFilter2D();
        private static Material sharedBeamMaterial;

        static ContinuousBeamWeapon()
        {
            raycastFilter.useTriggers = true;
            raycastFilter.useLayerMask = false;
        }

        public string WeaponName => "Beam Laser";
        public WeaponArchetype Archetype => WeaponArchetype.Beam;
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCostPerSecond;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }
        public float InaccuracyAngle => 0f;
        public bool IsFiring => isFiring;

        [Header("Physical Sockets")]
        public Vector2[] customSockets = null;
        public Vector2[] WeaponSockets => customSockets;
        public void SetSockets(Vector2[] sockets)
        {
            customSockets = sockets;
            EnsureBeamRenderers(sockets != null && sockets.Length > 0 ? sockets.Length : 1);
        }

        private float lastTickTime = -999f;
        private bool isFiring = false;

        private Transform cachedMuzzleTransform;
        private bool hasLookedUpMuzzle;
        private ShipWeapon cachedShipWeapon;
        private bool hasLookedUpShipWeapon;
        private PlayerProgression cachedProgression;
        private IShipInputProvider cachedInputProvider;
        private bool hasLookedUpInputProvider;
        private readonly Vector2[] singleMuzzleBuffer = new Vector2[1];
        private Vector2[] customMuzzleBuffer;

        private float pendingPopupDamage;
        private Vector2 pendingPopupPos;
        private bool hasPendingPopup;
        private float lastPopupEmitTime = -999f;
        private const float DamagePopupInterval = 0.22f;

        public void ResetCooldown() => lastTickTime = -999f;

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
            EnsureBeamRenderers(1);
        }

        private void EnsureBeamRenderers(int requiredCount)
        {
            if (requiredCount <= 0) requiredCount = 1;

            if (activeRenderers.Count == 0)
            {
                var existing = GetComponentsInChildren<LineRenderer>(true);
                foreach (var lr in existing)
                {
                    if (lr != null && lr.gameObject.name.StartsWith("BeamEffect"))
                    {
                        if (!activeRenderers.Contains(lr)) activeRenderers.Add(lr);
                    }
                }
            }

            if (sharedBeamMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) sharedBeamMaterial = new Material(shader);
            }

            while (activeRenderers.Count < requiredCount)
            {
                int index = activeRenderers.Count;
                var beamGo = new GameObject($"BeamEffect_{index}");
                beamGo.transform.SetParent(transform, false);
                var lr = beamGo.AddComponent<LineRenderer>();
                ConfigureLineRenderer(lr);
                activeRenderers.Add(lr);
            }

            if (beamRenderer == null && activeRenderers.Count > 0)
            {
                beamRenderer = activeRenderers[0];
            }
        }

        private void ConfigureLineRenderer(LineRenderer lr)
        {
            if (lr == null) return;
            lr.positionCount = 2;
            lr.startWidth = 0.18f;
            lr.endWidth = 0.12f;
            lr.useWorldSpace = true;
            if (sharedBeamMaterial != null) lr.sharedMaterial = sharedBeamMaterial;
            lr.startColor = beamColor;
            lr.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.4f);
            lr.sortingOrder = 6;
            lr.enabled = false;
        }

        public bool TryFire(ShipHealth health) => TryFire((IEnergyConsumer)health);

        public bool TryFire(IEnergyConsumer energySource = null)
        {
            float dt = lastTickTime > 0f ? (Time.time - lastTickTime) : Time.deltaTime;
            dt = Mathf.Clamp(dt, 0.01f, 0.1f);

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

            int beamCount = muzzles.Length;
            float totalEnergyNeeded = (energyCostPerSecond * beamCount) * dt;
            if (energySource != null && !energySource.ConsumeEnergy(totalEnergyNeeded))
            {
                StopFiring();
                return false;
            }

            lastTickTime = Time.time;
            isFiring = true;

            EnsureBeamRenderers(beamCount);

            if (!Application.isPlaying) Physics2D.SyncTransforms();

            Vector2 forward = ShipWeapon.ResolveIndependentAimDirection(
                transform, IsMountedOnSocket(), ref cachedProgression, ref cachedInputProvider, ref hasLookedUpInputProvider);
            if (forward.sqrMagnitude < 0.0001f) forward = Vector2.right;
            else forward.Normalize();

            float tickDamage = damagePerSecond * dt;
            Transform ownerRoot = transform.root;

            for (int i = 0; i < beamCount; i++)
            {
                Vector2 origin = muzzles[i];
                Vector2 beamDir;
                Vector2 lockedObjectivePoint;
                bool hasLockedObjective = TryFindLockedObjective(origin, forward, ownerRoot, out beamDir, out lockedObjectivePoint);

                Vector2 endPoint = origin + beamDir * maxRange;

                int hitCount = Physics2D.Raycast(origin, beamDir, raycastFilter, hitBuffer, maxRange);
                float closestDist = maxRange;
                RaycastHit2D closestHit = default;
                bool hasTarget = false;

                for (int h = 0; h < hitCount; h++)
                {
                    var hit = hitBuffer[h];
                    if (hit.collider == null || hit.collider.transform.root == ownerRoot) continue;

                    bool isTarget = hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Boss") || hit.collider.CompareTag("Asteroid");
                    if (isTarget && hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        closestHit = hit;
                        hasTarget = true;
                    }
                }

                if (hasTarget && closestHit.collider != null)
                {
                    Vector2 hitPoint = closestHit.distance > 0.05f
                        ? closestHit.point
                        : (hasLockedObjective ? lockedObjectivePoint : closestHit.point);
                    endPoint = hitPoint;

                    IDamageable damageable = ResolveDamageable(closestHit.collider);
                    if (damageable != null)
                    {
                        damageable.TakeDamage(tickDamage, hitPoint);
                        pendingPopupDamage += tickDamage;
                        pendingPopupPos = hitPoint;
                        hasPendingPopup = true;
                        if (Time.time - lastPopupEmitTime >= DamagePopupInterval)
                        {
                            FlushDamagePopup();
                        }
                    }
                }
                else if (hasLockedObjective)
                {
                    endPoint = lockedObjectivePoint;
                }

                var lr = activeRenderers[i];
                if (lr != null)
                {
                    lr.enabled = true;
                    lr.startWidth = 0.16f;
                    lr.endWidth = 0.10f;
                    lr.SetPosition(0, origin);
                    lr.SetPosition(1, endPoint);
                }
            }

            // Disable any extra renderers beyond active beam count
            for (int i = beamCount; i < activeRenderers.Count; i++)
            {
                if (activeRenderers[i] != null)
                {
                    activeRenderers[i].enabled = false;
                }
            }

            return true;
        }

        private static IDamageable ResolveDamageable(Collider2D col)
        {
            if (col == null) return null;
            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable == null) damageable = col.GetComponentInParent<IDamageable>();
            if (damageable == null && col.attachedRigidbody != null)
            {
                damageable = col.attachedRigidbody.GetComponent<IDamageable>();
            }
            return damageable;
        }

        private bool TryFindLockedObjective(Vector2 origin, Vector2 weaponDir, Transform ownerRoot, out Vector2 beamDir, out Vector2 objectivePoint)
        {
            beamDir = weaponDir;
            objectivePoint = origin + weaponDir * maxRange;
            if (aimConeHalfAngle <= 0f && closeRangeLockRadius <= 0f) return false;

            Vector2 shipPos = ownerRoot != null ? (Vector2)ownerRoot.position : (Vector2)transform.position;
            float scanRadius = maxRange + Vector2.Distance(shipPos, origin);
            int count = Physics2D.OverlapCircle(origin, scanRadius, raycastFilter, targetOverlapBuffer);
            int bestPriority = 0;
            float bestScore = float.MaxValue;
            bool found = false;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = targetOverlapBuffer[i];
                if (col == null || !col.enabled || !col.gameObject.activeInHierarchy) continue;
                if (col.transform.root == ownerRoot) continue;

                bool isEnemy = col.CompareTag("Enemy") || col.CompareTag("Boss");
                bool isAsteroid = !isEnemy && col.CompareTag("Asteroid");
                if (!isEnemy && !isAsteroid) continue;

                if (ResolveDamageable(col) == null) continue;

                Vector2 targetCenter = col.bounds.extents.sqrMagnitude > 0.0001f
                    ? (Vector2)col.bounds.center
                    : (Vector2)col.transform.TransformPoint(col.offset);
                Vector2 toTarget = targetCenter - origin;
                float distFromOrigin = toTarget.magnitude;
                if (distFromOrigin > maxRange) continue;

                Vector2 dir = distFromOrigin > 0.001f ? toTarget / distFromOrigin : weaponDir;
                float muzzleAngle = Vector2.Angle(weaponDir, dir);
                bool inMuzzleCone = muzzleAngle <= aimConeHalfAngle;

                Vector2 toFromShip = targetCenter - shipPos;
                float distFromShip = toFromShip.magnitude;
                bool inForwardHemisphere = distFromShip <= 0.05f || Vector2.Dot(toFromShip, weaponDir) > 0f;
                bool inShipCone = inForwardHemisphere && distFromShip > 0.001f && Vector2.Angle(weaponDir, toFromShip / distFromShip) <= aimConeHalfAngle;

                bool inCloseFrontLock = false;
                if (isEnemy && inForwardHemisphere && closeRangeLockRadius > 0f)
                {
                    float originEdgeDist = Vector2.Distance(origin, col.ClosestPoint(origin));
                    float shipEdgeDist = Vector2.Distance(shipPos, col.ClosestPoint(shipPos));
                    inCloseFrontLock = originEdgeDist <= closeRangeLockRadius || shipEdgeDist <= closeRangeLockRadius;
                }

                int priority;
                if (isEnemy)
                {
                    if (inMuzzleCone || inShipCone) priority = 3;
                    else if (inCloseFrontLock) priority = 2;
                    else continue;
                }
                else
                {
                    if (!inMuzzleCone) continue;
                    priority = 1;
                }

                if (priority < bestPriority) continue;

                float effectiveAngle = Mathf.Min(muzzleAngle, distFromShip > 0.001f ? Vector2.Angle(weaponDir, toFromShip / distFromShip) : muzzleAngle);
                float score = effectiveAngle + distFromOrigin * 0.5f;
                if (priority > bestPriority || score < bestScore)
                {
                    bestPriority = priority;
                    bestScore = score;
                    beamDir = dir;
                    objectivePoint = targetCenter;
                    found = true;
                }
            }

            return found;
        }

        private void FlushDamagePopup()
        {
            if (!hasPendingPopup || pendingPopupDamage <= 0.01f) return;
            GameEvents.TriggerDamageDealt(pendingPopupPos, pendingPopupDamage, isCrit: false);
            pendingPopupDamage = 0f;
            hasPendingPopup = false;
            lastPopupEmitTime = Time.time;
        }

        public void StopFiring()
        {
            FlushDamagePopup();
            isFiring = false;
            lastTickTime = -999f;
            for (int i = 0; i < activeRenderers.Count; i++)
            {
                if (activeRenderers[i] != null)
                {
                    activeRenderers[i].enabled = false;
                }
            }
            if (beamRenderer != null)
            {
                beamRenderer.enabled = false;
            }
        }

        private void OnDisable()
        {
            StopFiring();
        }

        private void OnDestroy()
        {
            StopFiring();
        }
    }
}
