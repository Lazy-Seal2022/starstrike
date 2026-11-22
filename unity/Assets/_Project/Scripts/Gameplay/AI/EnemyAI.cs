using System.Collections.Generic;
using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(ShipMovement), typeof(ShipHealth), typeof(ShipWeapon))]
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState { Patrol, Chase, Attack, KamikazeCharge, SniperAim, SupportEscort }

        public EnemyDefinition definition;

        public AIState currentState = AIState.Patrol;
        public float sightRadius = 16f;
        public float attackRadius = 10f;
        public float patrolSpeedMult = 0.5f;

        public EnemyArchetype Archetype => definition != null ? definition.archetype : EnemyArchetype.Drone;

        private ShipMovement movement;
        private ShipHealth health;
        private ShipWeapon weapon;
        private Transform playerTarget;
        private SpriteRenderer spriteRenderer;
        private LineRenderer sniperAimLine;
        
        private Vector2 patrolCenter;
        private float patrolAngle;
        private float patrolRadius = 20f;

        // Sniper specific state
        private float sniperChargeTimer = 0f;
        private float sniperCooldownTimer = 0f;

        // Support Shield specific state
        private Transform protectedAllyTarget;
        private readonly List<ShipHealth> currentlyShieldedAllies = new List<ShipHealth>();
        private static readonly Collider2D[] overlapBuffer = new Collider2D[128];

        private void Awake()
        {
            movement = GetComponent<ShipMovement>();
            health = GetComponent<ShipHealth>();
            weapon = GetComponent<ShipWeapon>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            
            tag = "Enemy";

            Collider2D col = GetComponent<Collider2D>();
            if (col == null) col = gameObject.AddComponent<PolygonCollider2D>();
            col.isTrigger = false;
            if (col is CircleCollider2D circleCol)
            {
                circleCol.radius = 0.7f;
            }

            if (definition == null)
            {
                definition = Resources.Load<EnemyDefinition>("Data/Enemy_Drone");
                if (definition == null)
                {
                    ConfigureArchetype(EnemyArchetype.Drone);
                }
            }

            var shieldField = GetComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            if (shieldField == null) shieldField = gameObject.AddComponent<StarStrike.Gameplay.FX.ShipShieldField>();

            ApplyVisuals();

            var trackable = GetComponent<MinimapTrackable>();
            if (trackable == null) trackable = gameObject.AddComponent<MinimapTrackable>();
            trackable.SetType(TrackableType.Enemy);

            SetupSniperLine();

            health.OnDied += Die;
        }

        private void SetupSniperLine()
        {
            sniperAimLine = GetComponent<LineRenderer>();
            if (sniperAimLine == null)
            {
                sniperAimLine = gameObject.AddComponent<LineRenderer>();
            }
            sniperAimLine.positionCount = 2;
            sniperAimLine.startWidth = 0.05f;
            sniperAimLine.endWidth = 0.02f;
            sniperAimLine.material = new Material(Shader.Find("Sprites/Default"));
            sniperAimLine.startColor = new Color(0.8f, 0.2f, 1f, 0.2f);
            sniperAimLine.endColor = new Color(1f, 0.2f, 0.2f, 0.6f);
            sniperAimLine.sortingOrder = 6;
            sniperAimLine.enabled = false;
        }

        public void ConfigureArchetype(EnemyArchetype archetype, EnemyDefinition customDef = null)
        {
            if (movement == null) movement = GetComponent<ShipMovement>();
            if (health == null) health = GetComponent<ShipHealth>();
            if (weapon == null) weapon = GetComponent<ShipWeapon>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (health != null)
            {
                health.OnDied -= Die;
                health.OnDied += Die;
            }

            if (customDef != null)
            {
                definition = customDef;
            }
            else
            {
                definition = ScriptableObject.CreateInstance<EnemyDefinition>();
                definition.archetype = archetype;

                switch (archetype)
                {
                    case EnemyArchetype.Kamikaze:
                        definition.id = "kamikaze";
                        definition.enemyName = "Crimson Dart";
                        definition.maxShield = 40f;
                        definition.maxHull = 25f;
                        definition.speed = 10f;
                        definition.agility = 7f;
                        definition.sightRadius = 22f;
                        definition.attackRadius = 2f;
                        definition.pulseDamage = 0f;
                        definition.kamikazeBlastRadius = 4.5f;
                        definition.kamikazeBlastDamage = 45f;
                        definition.tintColor = new Color(0.95f, 0.2f, 0.25f);
                        definition.visualScale = 1.15f;
                        break;

                    case EnemyArchetype.Sniper:
                        definition.id = "sniper";
                        definition.enemyName = "Void Sniper";
                        definition.maxShield = 50f;
                        definition.maxHull = 35f;
                        definition.speed = 4.5f;
                        definition.agility = 4f;
                        definition.sightRadius = 26f;
                        definition.attackRadius = 18f;
                        definition.pulseDamage = 35f;
                        definition.pulseSpeed = 36f;
                        definition.fireRate = 2.4f;
                        definition.sniperChargeTime = 1.2f;
                        definition.tintColor = new Color(0.75f, 0.3f, 1f);
                        definition.visualScale = 1.3f;
                        break;

                    case EnemyArchetype.SupportShield:
                        definition.id = "support_shield";
                        definition.enemyName = "Aegis Frigate";
                        definition.maxShield = 120f;
                        definition.shieldRegen = 4f;
                        definition.maxHull = 80f;
                        definition.speed = 5f;
                        definition.agility = 4.5f;
                        definition.sightRadius = 20f;
                        definition.attackRadius = 12f;
                        definition.pulseDamage = 12f;
                        definition.pulseSpeed = 16f;
                        definition.fireRate = 1.6f;
                        definition.supportShieldRadius = 5.5f;
                        definition.tintColor = new Color(1f, 0.85f, 0.2f);
                        definition.visualScale = 1.45f;
                        break;

                    case EnemyArchetype.Boss:
                        definition.id = "boss";
                        definition.enemyName = "Dreadnought Flagship";
                        definition.maxShield = 800f;
                        definition.shieldRegen = 10f;
                        definition.maxHull = 500f;
                        definition.speed = 3.5f;
                        definition.agility = 2.5f;
                        definition.sightRadius = 30f;
                        definition.attackRadius = 16f;
                        definition.pulseDamage = 25f;
                        definition.pulseSpeed = 22f;
                        definition.fireRate = 0.8f;
                        definition.tintColor = new Color(1f, 0.3f, 0.3f);
                        definition.visualScale = 3.0f;
                        break;

                    default: // Drone
                        definition.id = "drone";
                        definition.enemyName = "Hostile Drone";
                        definition.maxShield = 60f;
                        definition.maxHull = 40f;
                        definition.shieldRegen = 0f;
                        definition.speed = 6f;
                        definition.agility = 5f;
                        definition.sightRadius = 16f;
                        definition.attackRadius = 10f;
                        definition.pulseDamage = 14f;
                        definition.pulseSpeed = 18f;
                        definition.fireRate = 1.1f;
                        definition.tintColor = Color.white;
                        definition.visualScale = 1.0f;
                        break;
                }
            }

            ApplyVisuals();
            ApplyStats();
        }

        public void ApplyVisuals()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            string spriteType = "drone";
            switch (Archetype)
            {
                case EnemyArchetype.Kamikaze: spriteType = "kamikaze"; break;
                case EnemyArchetype.Sniper: spriteType = "sniper"; break;
                case EnemyArchetype.SupportShield: spriteType = "support"; break;
                case EnemyArchetype.Boss: spriteType = "boss"; break;
                default: spriteType = "drone"; break;
            }

            if (definition != null && definition.sprite != null)
            {
                spriteRenderer.sprite = definition.sprite;
            }
            else
            {
                spriteRenderer.sprite = ProceduralSpriteHelper.GetShipSprite(spriteType);
            }

            if (definition != null && definition.tintColor != Color.white)
            {
                spriteRenderer.color = definition.tintColor;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }

            spriteRenderer.sortingOrder = 4;

            float scale = (definition != null && definition.visualScale > 0f) ? definition.visualScale : 1.0f;
            transform.localScale = Vector3.one * scale;

            var poly = GetComponent<PolygonCollider2D>();
            if (poly != null && spriteRenderer.sprite != null)
            {
                PlayerProgression.UpdatePolygonFromSprite(poly, spriteRenderer.sprite);
            }

            var shield = GetComponent<StarStrike.Gameplay.FX.ShipShieldField>();
            if (shield != null)
            {
                shield.EnsureInitialized();
                shield.RebuildShieldCollider();
            }
        }

        private void ApplyStats()
        {
            if (definition == null) return;

            if (health != null)
            {
                health.maxShield = definition.maxShield;
                health.shieldRegen = definition.shieldRegen;
                health.maxHull = definition.maxHull > 0 ? definition.maxHull : 40f;
                health.maxEnergy = 9999f;
                health.energyRegen = 9999f;
                health.ResetMeters();
            }

            if (movement != null)
            {
                movement.maxSpeed = definition.speed;
                movement.acceleration = definition.speed * 2f;
                movement.agility = definition.agility;
            }

            if (weapon != null)
            {
                weapon.pulseDamage = definition.pulseDamage;
                weapon.pulseSpeed = definition.pulseSpeed;
                weapon.fireRate = definition.fireRate;
                weapon.energyCost = 0f;
                weapon.isPlayer = false;
            }

            sightRadius = definition.sightRadius;
            attackRadius = definition.attackRadius;
        }

        private void OnEnable()
        {
            ApplyStats();
            ApplyVisuals();

            patrolCenter = transform.position;
            patrolAngle = Random.Range(0f, 360f);
            currentState = AIState.Patrol;
            sniperChargeTimer = 0f;
            sniperCooldownTimer = 0f;
            if (sniperAimLine != null) sniperAimLine.enabled = false;
            ReleaseShieldedAllies();
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        private void OnDisable()
        {
            IsStunned = false;
            if (sniperAimLine != null) sniperAimLine.enabled = false;
            ReleaseShieldedAllies();
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDied -= Die;
            ReleaseShieldedAllies();
        }

        private void Update()
        {
            if (IsStunned)
            {
                if (movement != null)
                {
                    movement.IsThrusting = false;
                    movement.IsBraking = true;
                }
                return;
            }

            if (playerTarget == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                RunPatrol();
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;

            switch (Archetype)
            {
                case EnemyArchetype.Kamikaze:
                    UpdateKamikaze(toPlayer, dist);
                    break;

                case EnemyArchetype.Sniper:
                    UpdateSniper(toPlayer, dist);
                    break;

                case EnemyArchetype.SupportShield:
                    UpdateSupportShield(toPlayer, dist);
                    break;

                default: // Drone & Boss
                    UpdateStandardAI(toPlayer, dist);
                    break;
            }
        }

        #region Standard Drone AI
        private void UpdateStandardAI(Vector2 toPlayer, float dist)
        {
            if (dist <= attackRadius)
            {
                currentState = AIState.Attack;
            }
            else if (dist <= sightRadius)
            {
                currentState = AIState.Chase;
            }
            else
            {
                currentState = AIState.Patrol;
            }

            switch (currentState)
            {
                case AIState.Patrol:
                    RunPatrol();
                    break;
                case AIState.Chase:
                    RunChase(toPlayer);
                    break;
                case AIState.Attack:
                    RunAttack(toPlayer);
                    break;
            }
        }

        private void RunPatrol()
        {
            patrolAngle += Time.deltaTime * 0.2f;
            Vector2 targetPos = patrolCenter + new Vector2(Mathf.Cos(patrolAngle), Mathf.Sin(patrolAngle)) * patrolRadius;
            Vector2 toTarget = targetPos - (Vector2)transform.position;
            
            movement.AimDirection = toTarget.normalized;
            movement.IsThrusting = true;
            movement.IsBraking = false; 
        }

        private void RunChase(Vector2 toPlayer)
        {
            movement.AimDirection = toPlayer.normalized;
            movement.IsThrusting = true;
        }

        private void RunAttack(Vector2 toPlayer)
        {
            movement.AimDirection = toPlayer.normalized;
            movement.IsThrusting = toPlayer.magnitude > attackRadius * 0.5f;

            Vector2 forward = transform.right;
            if (Vector2.Dot(forward, toPlayer.normalized) > 0.9f)
            {
                weapon.TryFire(health);
            }
        }
        #endregion

        #region Kamikaze Archetype
        private void UpdateKamikaze(Vector2 toPlayer, float dist)
        {
            if (dist <= sightRadius)
            {
                currentState = AIState.KamikazeCharge;
                movement.AimDirection = toPlayer.normalized;
                movement.IsThrusting = true;
                
                // Boost speed during charge
                movement.maxSpeed = definition != null ? definition.speed * 1.5f : 12f;
                movement.acceleration = definition != null ? definition.speed * 3.5f : 30f;

                if (dist <= 1.2f)
                {
                    DetonateKamikaze();
                }
            }
            else
            {
                currentState = AIState.Patrol;
                RunPatrol();
            }
        }

        public void DetonateKamikaze()
        {
            // Radial blast explosion
            float blastRadius = definition != null ? definition.kamikazeBlastRadius : 4.5f;
            float blastDmg = definition != null ? definition.kamikazeBlastDamage : 45f;

            Physics2D.SyncTransforms();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null || col.gameObject == gameObject) continue;

                // Player or asteroid damage
                if (col.CompareTag("Player") || col.GetComponent<ShipHealth>() != null)
                {
                    var sh = col.GetComponent<ShipHealth>();
                    if (sh != null && !sh.isInvulnerable)
                    {
                        sh.TakeDamage(blastDmg);
                    }
                }
                else if (col.GetComponent<Asteroid>() != null)
                {
                    col.GetComponent<Asteroid>().TakeDamage(blastDmg);
                }
            }

            if (FX.FXService.Instance != null)
            {
                FX.FXService.Instance.CameraShake(0.28f, 0.35f);
            }
            if (Audio.AudioService.Instance != null)
            {
                Audio.AudioService.Instance.PlaySound("explosion", 1.2f);
            }

            Die();
        }
        #endregion

        #region Sniper Archetype
        private void UpdateSniper(Vector2 toPlayer, float dist)
        {
            if (sniperCooldownTimer > 0f)
            {
                sniperCooldownTimer -= Time.deltaTime;
            }

            if (dist < 8.0f)
            {
                // Kite away if player rushes in
                currentState = AIState.Chase;
                movement.AimDirection = -toPlayer.normalized;
                movement.IsThrusting = true;
                if (sniperAimLine != null) sniperAimLine.enabled = false;
                sniperChargeTimer = 0f;
            }
            else if (dist <= sightRadius)
            {
                // Hold standoff and charge beam
                currentState = AIState.SniperAim;
                movement.AimDirection = toPlayer.normalized;
                movement.IsThrusting = dist > attackRadius;

                if (sniperCooldownTimer <= 0f)
                {
                    sniperChargeTimer += Time.deltaTime;
                    float chargeDuration = definition != null ? definition.sniperChargeTime : 1.2f;

                    if (sniperAimLine != null)
                    {
                        sniperAimLine.enabled = true;
                        Vector3 origin = transform.position;
                        Vector3 target = playerTarget != null ? playerTarget.position : origin + (Vector3)(toPlayer.normalized * 20f);
                        sniperAimLine.SetPosition(0, origin);
                        sniperAimLine.SetPosition(1, target);

                        float progress = Mathf.Clamp01(sniperChargeTimer / chargeDuration);
                        sniperAimLine.startWidth = Mathf.Lerp(0.04f, 0.16f, progress);
                        sniperAimLine.endWidth = Mathf.Lerp(0.02f, 0.08f, progress);
                        sniperAimLine.startColor = Color.Lerp(new Color(0.8f, 0.2f, 1f, 0.3f), Color.white, progress);
                    }

                    if (sniperChargeTimer >= chargeDuration)
                    {
                        FireSniperShot(toPlayer);
                    }
                }
                else
                {
                    if (sniperAimLine != null) sniperAimLine.enabled = false;
                }
            }
            else
            {
                currentState = AIState.Patrol;
                if (sniperAimLine != null) sniperAimLine.enabled = false;
                sniperChargeTimer = 0f;
                RunPatrol();
            }
        }

        private void FireSniperShot(Vector2 toPlayer)
        {
            sniperChargeTimer = 0f;
            sniperCooldownTimer = definition != null ? definition.fireRate : 2.4f;

            if (sniperAimLine != null) sniperAimLine.enabled = false;

            // Fire high velocity pulse
            if (weapon != null)
            {
                weapon.pulseSpeed = definition != null ? definition.pulseSpeed : 36f;
                weapon.pulseDamage = definition != null ? definition.pulseDamage : 35f;
                weapon.TryFire(health);
            }

            if (Audio.AudioService.Instance != null)
            {
                Audio.AudioService.Instance.PlaySound("laser_fire", 1.4f);
            }
            if (FX.FXService.Instance != null)
            {
                FX.FXService.Instance.CameraShake(0.12f, 0.15f);
            }
        }
        #endregion

        #region Support Shield Archetype
        private void UpdateSupportShield(Vector2 toPlayer, float dist)
        {
            currentState = AIState.SupportEscort;

            // Find nearest ally enemy
            FindNearestAlly();

            if (protectedAllyTarget != null)
            {
                // Orbit around the protected ally
                Vector2 toAlly = (Vector2)protectedAllyTarget.position - (Vector2)transform.position;
                float allyDist = toAlly.magnitude;

                if (allyDist > 4.5f)
                {
                    movement.AimDirection = toAlly.normalized;
                    movement.IsThrusting = true;
                }
                else
                {
                    Vector2 orbitTangent = new Vector2(-toAlly.y, toAlly.x).normalized;
                    movement.AimDirection = orbitTangent;
                    movement.IsThrusting = true;
                }
            }
            else
            {
                // Fallback: stay near player's perimeter
                movement.AimDirection = toPlayer.normalized;
                movement.IsThrusting = dist > 14f;
            }

            // Project Invulnerability Aura to nearby allies
            ApplySupportShieldAura();
        }

        private void FindNearestAlly()
        {
            if (protectedAllyTarget != null && protectedAllyTarget.gameObject.activeInHierarchy)
            {
                return;
            }

            float closestSqrDist = float.MaxValue;
            Transform closest = null;

            Physics2D.SyncTransforms();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 25f);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null || col.gameObject == gameObject) continue;

                if (col.CompareTag("Enemy") && col.GetComponent<EnemyAI>() != null)
                {
                    float sqr = ((Vector2)col.transform.position - (Vector2)transform.position).sqrMagnitude;
                    if (sqr < closestSqrDist)
                    {
                        closestSqrDist = sqr;
                        closest = col.transform;
                    }
                }
            }

            protectedAllyTarget = closest;
        }

        private void ApplySupportShieldAura()
        {
            float shieldRadius = definition != null ? definition.supportShieldRadius : 5.5f;
            Physics2D.SyncTransforms();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, shieldRadius);

            var insideThisFrame = new HashSet<ShipHealth>();

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D col = hits[i];
                if (col == null || col.gameObject == gameObject) continue;

                if (col.CompareTag("Enemy"))
                {
                    var sh = col.GetComponent<ShipHealth>();
                    if (sh != null)
                    {
                        insideThisFrame.Add(sh);
                        sh.isInvulnerable = true;
                        if (!currentlyShieldedAllies.Contains(sh))
                        {
                            currentlyShieldedAllies.Add(sh);
                        }
                    }
                }
            }

            // Unshield allies that exited the radius
            for (int i = currentlyShieldedAllies.Count - 1; i >= 0; i--)
            {
                var sh = currentlyShieldedAllies[i];
                if (sh == null || !insideThisFrame.Contains(sh))
                {
                    if (sh != null) sh.isInvulnerable = false;
                    currentlyShieldedAllies.RemoveAt(i);
                }
            }
        }

        private void ReleaseShieldedAllies()
        {
            for (int i = 0; i < currentlyShieldedAllies.Count; i++)
            {
                if (currentlyShieldedAllies[i] != null)
                {
                    currentlyShieldedAllies[i].isInvulnerable = false;
                }
            }
            currentlyShieldedAllies.Clear();
        }
        #endregion

        public bool IsStunned { get; private set; }

        public void ApplyEMPStun(float duration)
        {
            if (!gameObject.activeInHierarchy || !enabled) return;
            StartCoroutine(EMPStunRoutine(duration));
        }

        private System.Collections.IEnumerator EMPStunRoutine(float duration)
        {
            IsStunned = true;
            if (movement != null)
            {
                movement.IsThrusting = false;
                movement.IsBraking = true;
            }
            if (weapon != null) { weapon.enabled = false; }
            if (sniperAimLine != null) { sniperAimLine.enabled = false; }
            sniperChargeTimer = 0f;

            Color origColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (spriteRenderer != null)
                {
                    // Electric blue flicker while EMP stunned
                    float flicker = (Mathf.Sin(elapsed * 24f) + 1f) * 0.5f;
                    spriteRenderer.color = Color.Lerp(new Color(0.2f, 0.8f, 1f, 1f), Color.white, flicker * 0.5f);
                }
                yield return null;
            }

            IsStunned = false;
            if (weapon != null) { weapon.enabled = true; }
            if (spriteRenderer != null) { spriteRenderer.color = origColor; }
        }

        public void Die()
        {
            ReleaseShieldedAllies();

            if (Archetype == EnemyArchetype.Kamikaze)
            {
                // Kamikaze death blast
                float blastRadius = definition != null ? definition.kamikazeBlastRadius : 4.5f;
                float blastDmg = definition != null ? definition.kamikazeBlastDamage : 45f;

                Physics2D.SyncTransforms();
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
                for (int i = 0; i < hits.Length; i++)
                {
                    Collider2D col = hits[i];
                    if (col == null || col.gameObject == gameObject) continue;

                    var sh = col.GetComponent<ShipHealth>();
                    if (sh != null && !sh.isInvulnerable)
                    {
                        sh.TakeDamage(blastDmg);
                    }
                    else if (col.GetComponent<Asteroid>() != null)
                    {
                        col.GetComponent<Asteroid>().TakeDamage(blastDmg);
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 0.4f);
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Spawn("Gem", pos, Quaternion.identity, () => {
                        GameObject obj = new GameObject("GemCrystal");
                        obj.AddComponent<GemCrystal>();
                        return obj;
                    });
                }
                else
                {
                    GameObject gemObj = new GameObject("GemCrystal");
                    gemObj.transform.position = pos;
                    gemObj.AddComponent<GemCrystal>();
                }
            }

            GameEvents.TriggerEnemyDestroyed(definition != null ? definition.id : "drone");
            gameObject.SetActive(false);
        }
    }
}
