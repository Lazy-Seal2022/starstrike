using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay
{
    [RequireComponent(typeof(ShipMovement), typeof(ShipHealth), typeof(ShipWeapon))]
    public class EnemyAI : MonoBehaviour
    {
        public enum AIState { Patrol, Chase, Attack }

        public EnemyDefinition definition;

        public AIState currentState = AIState.Patrol;
        public float sightRadius = 16f;
        public float attackRadius = 10f;
        public float patrolSpeedMult = 0.5f;

        private ShipMovement movement;
        private ShipHealth health;
        private ShipWeapon weapon;
        private Transform playerTarget;
        
        private Vector2 patrolCenter;
        private float patrolAngle;
        private float patrolRadius = 20f;

        private void Awake()
        {
            movement = GetComponent<ShipMovement>();
            health = GetComponent<ShipHealth>();
            weapon = GetComponent<ShipWeapon>();
            
            tag = "Enemy";

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteHelper.GetShipSprite("drone");
            sr.sortingOrder = 4;

            health.OnDied += Die;
        }

        private void OnEnable()
        {
            if (definition != null)
            {
                health.maxShield = definition.maxShield;
                health.shieldRegen = definition.shieldRegen;
                movement.maxSpeed = definition.speed;
                movement.acceleration = definition.speed * 2f;
                movement.agility = definition.agility;
                weapon.pulseDamage = definition.pulseDamage;
                weapon.pulseSpeed = definition.pulseSpeed;
                weapon.fireRate = definition.fireRate;
                sightRadius = definition.sightRadius;
                attackRadius = definition.attackRadius;
            }
            else
            {
                health.maxShield = 60f;
                health.shieldRegen = 0f;
                movement.maxSpeed = 6f;
                movement.acceleration = 12f;
                movement.agility = 5f;
                weapon.pulseDamage = 14f;
                weapon.pulseSpeed = 18f;
                weapon.fireRate = 1.1f;
            }

            health.maxEnergy = 9999f;
            health.energyRegen = 9999f;
            health.ResetMeters();

            weapon.energyCost = 0f;
            weapon.isPlayer = false;

            patrolCenter = transform.position;
            patrolAngle = Random.Range(0f, 360f);
            currentState = AIState.Patrol;
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
                RunPatrol();
                return;
            }

            Vector2 toPlayer = (Vector2)playerTarget.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;

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

        private void Die()
        {
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

            gameObject.SetActive(false);
        }
    }
}
