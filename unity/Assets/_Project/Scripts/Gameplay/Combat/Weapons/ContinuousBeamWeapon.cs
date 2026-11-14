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

        [Header("Visuals")]
        public Color beamColor = new Color(0.2f, 0.95f, 1f, 0.9f);
        public LineRenderer beamRenderer;

        public string WeaponName => "Beam Laser";
        public float FireRate { get => fireRate; set => fireRate = value; }
        public float EnergyCost => energyCostPerSecond;
        public int WeaponSlots { get => weaponSlots; set => weaponSlots = value; }

        private float lastTickTime = -999f;
        private bool isFiring = false;

        public void ResetCooldown() => lastTickTime = -999f;

        private void Awake()
        {
            SetupBeamRenderer();
        }

        private void SetupBeamRenderer()
        {
            if (beamRenderer == null)
            {
                var beamGo = new GameObject("BeamEffect");
                beamGo.transform.SetParent(transform, false);
                beamRenderer = beamGo.AddComponent<LineRenderer>();
            }

            beamRenderer.positionCount = 2;
            beamRenderer.startWidth = 0.18f;
            beamRenderer.endWidth = 0.12f;
            beamRenderer.useWorldSpace = true;
            beamRenderer.material = new Material(Shader.Find("Sprites/Default"));
            beamRenderer.startColor = beamColor;
            beamRenderer.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.4f);
            beamRenderer.sortingOrder = 6;
            beamRenderer.enabled = false;
        }

        public bool TryFire(ShipHealth health) => TryFire((IEnergyConsumer)health);

        public bool TryFire(IEnergyConsumer energySource = null)
        {
            float dt = lastTickTime > 0f ? (Time.time - lastTickTime) : Time.deltaTime;
            dt = Mathf.Clamp(dt, 0.01f, 0.1f);

            float energyNeeded = energyCostPerSecond * dt;
            if (energySource != null && !energySource.ConsumeEnergy(energyNeeded))
            {
                StopFiring();
                return false;
            }

            lastTickTime = Time.time;
            isFiring = true;

            if (beamRenderer == null) SetupBeamRenderer();
            beamRenderer.enabled = true;
            beamRenderer.startWidth = 0.15f * Mathf.Max(1, weaponSlots);
            beamRenderer.endWidth = 0.10f * Mathf.Max(1, weaponSlots);

            Vector2 nose = (Vector2)transform.position + (Vector2)transform.right * 0.65f;
            Vector2 forward = transform.right;

            RaycastHit2D[] hits = Physics2D.RaycastAll(nose, forward, maxRange);
            Vector2 endPoint = nose + forward * maxRange;

            foreach (var hit in hits)
            {
                if (hit.collider == null || hit.collider.gameObject == gameObject) continue;

                bool isTarget = hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Boss") || hit.collider.CompareTag("Asteroid");
                if (isTarget)
                {
                    endPoint = hit.point;

                    IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                    if (damageable == null) damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable == null && hit.collider.attachedRigidbody != null) damageable = hit.collider.attachedRigidbody.GetComponent<IDamageable>();

                    if (damageable != null)
                    {
                        float tickDamage = damagePerSecond * dt;
                        damageable.TakeDamage(tickDamage);
                        GameEvents.TriggerDamageDealt(hit.point, tickDamage, isCrit: false);
                    }
                    break;
                }
            }

            beamRenderer.SetPosition(0, nose);
            beamRenderer.SetPosition(1, endPoint);

            return true;
        }

        public void StopFiring()
        {
            isFiring = false;
            lastTickTime = -999f;
            if (beamRenderer != null)
            {
                beamRenderer.enabled = false;
            }
        }

        private void OnDisable()
        {
            StopFiring();
        }
    }
}
