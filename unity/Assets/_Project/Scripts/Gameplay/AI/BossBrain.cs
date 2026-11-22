using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(ShipMovement), typeof(ShipHealth), typeof(ShipWeapon))]
    public class BossBrain : MonoBehaviour
    {
        public enum BossPhase { Approaching, SweepingLasers, BulletHell, Enraged }

        [Header("Configuration")]
        public BossConfig config;

        public BossPhase currentPhase = BossPhase.Approaching;
        
        [Header("Stats")]
        public float sightRadius = 40f;
        public float attackRadius = 15f;

        [Header("Game Flow Options")]
        public bool triggerVictoryOnDefeat = false;

        private ShipMovement movement;
        private ShipHealth health;
        private ShipWeapon weapon;
        private Transform playerTarget;
        
        private float phaseTimer = 0f;
        private float bulletHellAngle = 0f;
        private float bulletHellWindupTimer = 0f;
        private const float BULLET_HELL_WINDUP = 0.8f;

        private LineRenderer telegraphLine;
        private static Material cachedTelegraphMat;

        public LineRenderer TelegraphLine => telegraphLine;

        private void Awake()
        {
            if (config == null) config = Resources.Load<BossConfig>("Data/BossConfig");
            if (config == null) config = ScriptableObject.CreateInstance<BossConfig>();

            movement = GetComponent<ShipMovement>();
            health = GetComponent<ShipHealth>();
            weapon = GetComponent<ShipWeapon>();
            
            tag = "Enemy"; // Or "Boss"

            EnsureTelegraphInitialized();

            Collider2D col = GetComponent<Collider2D>();
            if (col == null) col = gameObject.AddComponent<PolygonCollider2D>();
            col.isTrigger = false;
            if (col is CircleCollider2D circleCol)
            {
                circleCol.radius = config.colliderRadius;
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            if (config != null && config.sprite != null)
            {
                sr.sprite = config.sprite;
            }
            else if (sr.sprite == null)
            {
                sr.sprite = ProceduralSpriteHelper.GetShipSprite("boss");
            }
            sr.sortingOrder = 5;
            sr.color = new Color(1f, 0.4f, 0.4f);
            transform.localScale = Vector3.one * config.visualScale;

            health.OnDied += Die;

            var trackable = GetComponent<StarStrike.Gameplay.MinimapTrackable>();
            if (trackable == null) trackable = gameObject.AddComponent<StarStrike.Gameplay.MinimapTrackable>();
            trackable.SetType(TrackableType.Enemy);
        }

        private void OnEnable()
        {
            if (config == null) config = Resources.Load<BossConfig>("Data/BossConfig");
            if (config == null) config = ScriptableObject.CreateInstance<BossConfig>();

            // Massive stats for boss from config
            health.maxShield = config.maxShield;
            health.shieldRegen = config.shieldRegen;
            health.maxHull = config.maxHull > 0 ? config.maxHull : 1500f;
            movement.maxSpeed = config.maxSpeed;
            movement.acceleration = config.acceleration;
            movement.agility = config.agility;
            weapon.pulseDamage = config.pulseDamage;
            weapon.pulseSpeed = config.pulseSpeed;
            weapon.fireRate = config.fireRate;

            health.maxEnergy = 9999f;
            health.energyRegen = 9999f;
            health.ResetMeters();

            weapon.energyCost = 0f;
            weapon.isPlayer = false;
            weapon.weaponSlots = config.weaponSlots;

            currentPhase = BossPhase.Approaching;
            phaseTimer = 0f;
            bulletHellWindupTimer = 0f;
            if (telegraphLine != null) telegraphLine.enabled = false;
        }

        private void OnDisable()
        {
            if (telegraphLine != null) telegraphLine.enabled = false;
        }

        public void EnsureTelegraphInitialized()
        {
            if (telegraphLine == null)
            {
                Transform child = transform.Find("TelegraphRay");
                GameObject rayObj = child != null ? child.gameObject : null;
                if (rayObj == null)
                {
                    rayObj = new GameObject("TelegraphRay");
                    rayObj.transform.SetParent(transform, false);
                    rayObj.transform.localPosition = Vector3.zero;
                    rayObj.transform.localRotation = Quaternion.identity;
                }
                telegraphLine = rayObj.GetComponent<LineRenderer>();
                if (telegraphLine == null) telegraphLine = rayObj.AddComponent<LineRenderer>();

                telegraphLine.useWorldSpace = true;
                telegraphLine.positionCount = 2;
                telegraphLine.startWidth = 0.08f;
                telegraphLine.endWidth = 0.04f;
                telegraphLine.sortingOrder = 7;

                if (cachedTelegraphMat == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null) cachedTelegraphMat = new Material(shader);
                }
                if (cachedTelegraphMat != null) telegraphLine.sharedMaterial = cachedTelegraphMat;
                telegraphLine.enabled = false;
            }
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTarget = p.transform;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDied -= Die;
        }

        private void Update()
        {
            if (playerTarget == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTarget = p.transform;
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;

            // Phase Transitions based on Health
            float p1 = config != null ? config.phase1Threshold : 0.75f;
            float p2 = config != null ? config.phase2Threshold : 0.50f;
            float p3 = config != null ? config.phase3Threshold : 0.25f;

            float totalHp = health.currentShield + health.currentHull;
            float maxHp = health.maxShield + health.maxHull;
            float hpPercent = maxHp > 0f ? totalHp / maxHp : 1f;
            if (hpPercent < p3) currentPhase = BossPhase.Enraged;
            else if (hpPercent < p2 && currentPhase != BossPhase.BulletHell) 
            {
                if (currentPhase != BossPhase.SweepingLasers) currentPhase = BossPhase.BulletHell; 
                // Cycle between BulletHell and Sweeping
            }

            phaseTimer += Time.deltaTime;
            
            if (currentPhase != BossPhase.Enraged && currentPhase != BossPhase.Approaching && phaseTimer > 10f)
            {
                phaseTimer = 0f;
                currentPhase = currentPhase == BossPhase.BulletHell ? BossPhase.SweepingLasers : BossPhase.BulletHell;
                bulletHellWindupTimer = 0f;
                if (telegraphLine != null) telegraphLine.enabled = false;
            }
            
            if (currentPhase == BossPhase.Approaching && dist < attackRadius)
            {
                currentPhase = BossPhase.SweepingLasers;
                phaseTimer = 0f;
                bulletHellWindupTimer = 0f;
                if (telegraphLine != null) telegraphLine.enabled = false;
            }

            switch (currentPhase)
            {
                case BossPhase.Approaching:
                    RunApproach(toPlayer);
                    break;
                case BossPhase.SweepingLasers:
                    RunSweepingLasers(toPlayer);
                    break;
                case BossPhase.BulletHell:
                    RunBulletHell();
                    break;
                case BossPhase.Enraged:
                    RunEnraged(toPlayer);
                    break;
            }
        }

        private void RunApproach(Vector2 toPlayer)
        {
            if (telegraphLine != null) telegraphLine.enabled = false;
            movement.AimDirection = toPlayer.normalized;
            movement.IsThrusting = true;
        }

        private void RunSweepingLasers(Vector2 toPlayer)
        {
            // Move slowly towards player, constantly sweeping aim
            movement.IsThrusting = toPlayer.magnitude > attackRadius * 0.8f;
            
            float sweepAngle = Mathf.Sin(Time.time * 2f) * 45f;
            Vector2 aimDir = Quaternion.Euler(0, 0, sweepAngle) * toPlayer.normalized;
            movement.AimDirection = aimDir;

            Vector2 forward = transform.right;
            float alignment = Vector2.Dot(forward, aimDir);

            // Enemy Telegraph (Feature #4): Project warning targeting laser along sweep path
            if (telegraphLine != null)
            {
                telegraphLine.enabled = true;
                Vector3 origin = transform.position;
                Vector3 target = origin + (Vector3)(forward * attackRadius * 1.25f);
                telegraphLine.SetPosition(0, origin);
                telegraphLine.SetPosition(1, target);

                bool isFiringReady = alignment > 0.95f;
                float pulse = Mathf.PingPong(Time.time * 8f, 1f);
                float width = isFiringReady ? 0.22f : Mathf.Lerp(0.05f, 0.12f, pulse);
                telegraphLine.startWidth = width;
                telegraphLine.endWidth = width * 0.4f;

                Color warningColor = isFiringReady
                    ? new Color(1f, 0.95f, 0.3f, 0.95f)
                    : new Color(1f, 0.25f, 0.15f, Mathf.Lerp(0.4f, 0.85f, pulse));
                telegraphLine.startColor = warningColor;
                telegraphLine.endColor = new Color(warningColor.r, warningColor.g, warningColor.b, 0.05f);
            }

            if (alignment > 0.95f)
            {
                weapon.fireRate = 0.4f;
                weapon.TryFire(health);
            }
        }

        private void RunBulletHell()
        {
            movement.IsThrusting = false; // Stay put
            
            // Feature #4: Windup charge-up telegraph giving player reaction time
            if (bulletHellWindupTimer < BULLET_HELL_WINDUP)
            {
                bulletHellWindupTimer += Time.deltaTime;
                float progress = bulletHellWindupTimer / BULLET_HELL_WINDUP;

                if (telegraphLine != null)
                {
                    telegraphLine.enabled = true;
                    bulletHellAngle += Time.deltaTime * 180f; // Fast warning sweep
                    Vector3 forward = new Vector3(Mathf.Cos(bulletHellAngle * Mathf.Deg2Rad), Mathf.Sin(bulletHellAngle * Mathf.Deg2Rad), 0f);
                    Vector3 origin = transform.position;
                    telegraphLine.SetPosition(0, origin - forward * 7f);
                    telegraphLine.SetPosition(1, origin + forward * 7f);

                    float w = Mathf.Lerp(0.04f, 0.20f, progress);
                    telegraphLine.startWidth = w;
                    telegraphLine.endWidth = w;
                    Color c = Color.Lerp(new Color(1f, 0.5f, 0.1f, 0.4f), new Color(1f, 0.15f, 0.15f, 0.95f), progress);
                    telegraphLine.startColor = c;
                    telegraphLine.endColor = c;
                }
                return;
            }

            if (telegraphLine != null) telegraphLine.enabled = false;

            bulletHellAngle += Time.deltaTime * 60f;
            movement.AimDirection = new Vector2(Mathf.Cos(bulletHellAngle * Mathf.Deg2Rad), Mathf.Sin(bulletHellAngle * Mathf.Deg2Rad));
            
            weapon.fireRate = 0.15f; // Rapid fire
            weapon.weaponSlots = 2; // dual spiral
            weapon.TryFire(health);
        }

        private void RunEnraged(Vector2 toPlayer)
        {
            movement.maxSpeed = 8f;
            movement.acceleration = 16f;
            movement.IsThrusting = toPlayer.magnitude > attackRadius * 0.5f;
            movement.AimDirection = toPlayer.normalized;

            // Feature #4: Intense crimson lock-on telegraph ray
            if (telegraphLine != null)
            {
                telegraphLine.enabled = true;
                Vector3 origin = transform.position;
                Vector3 target = origin + (Vector3)(transform.right * attackRadius);
                telegraphLine.SetPosition(0, origin);
                telegraphLine.SetPosition(1, target);

                float pulse = Mathf.PingPong(Time.time * 14f, 1f);
                float w = Mathf.Lerp(0.08f, 0.25f, pulse);
                telegraphLine.startWidth = w;
                telegraphLine.endWidth = 0.05f;
                Color col = new Color(1f, 0.1f, 0.15f, Mathf.Lerp(0.6f, 1f, pulse));
                telegraphLine.startColor = col;
                telegraphLine.endColor = new Color(1f, 0.1f, 0.15f, 0.05f);
            }

            weapon.fireRate = 0.2f;
            weapon.weaponSlots = 3;
            Vector2 forward = transform.right;
            if (Vector2.Dot(forward, toPlayer.normalized) > 0.8f)
            {
                weapon.TryFire(health);
            }
        }

        private void Die()
        {
            if (telegraphLine != null) telegraphLine.enabled = false;

            // Drop a ton of gems
            for (int i = 0; i < 30; i++)
            {
                Vector3 pos = transform.position + (Vector3)(Random.insideUnitCircle * 2f);
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Spawn("Gem", pos, Quaternion.identity, () => {
                        GameObject obj = new GameObject("GemCrystal");
                        obj.AddComponent<GemCrystal>();
                        return obj;
                    });
                }
            }

            // Big explosion
            GameEvents.TriggerAsteroidDestroyed(transform.position, "Large");
            GameEvents.TriggerAsteroidDestroyed(transform.position + Vector3.right, "Large");
            GameEvents.TriggerAsteroidDestroyed(transform.position + Vector3.left, "Large");
            
            GameEvents.TriggerBossDefeated();
            if (triggerVictoryOnDefeat && GameManager.Instance != null)
            {
                GameManager.Instance.Victory();
            }

            gameObject.SetActive(false);
        }
    }
}
