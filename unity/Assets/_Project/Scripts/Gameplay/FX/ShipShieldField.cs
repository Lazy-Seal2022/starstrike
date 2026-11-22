using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Persistent dynamic shield field component for ships.
    /// Manages:
    /// 1. The perimeter energy dome barrier surrounding the ship.
    /// 2. Reacting to shield impacts by flaring the barrier and spawning localized hexagonal ripples.
    /// 3. Deactivating shield visuals when the shield is depleted.
    /// </summary>
    [DisallowMultipleComponent]
    public class ShipShieldField : MonoBehaviour
    {
        [Header("Shield Visual Tuning")]
        public Color playerShieldColor = new Color(0.2f, 0.9f, 1f, 0.95f);
        public Color enemyShieldColor = new Color(1f, 0.65f, 0.18f, 0.95f);
        public Color bossShieldColor = new Color(1f, 0.22f, 0.35f, 0.95f);

        [Tooltip("Effective radius of the shield energy envelope")]
        public float shieldRadius = 1.0f;
        public bool autoDetectRadius = true;
        public float domeFadeSpeed = 4.2f;

        private ShipHealth health;
        private SpriteRenderer domeRenderer;
        private GameObject domeObject;
        private Color currentShieldColor;
        private float currentDomeAlpha = 0f;

        public float ShieldRadius => shieldRadius;
        public Color ShieldColor => currentShieldColor;
        public float CurrentDomeAlpha => currentDomeAlpha;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            if (domeRenderer != null)
            {
                domeRenderer.color = Color.clear;
            }
        }

        public void EnsureInitialized()
        {
            if (health == null) health = GetComponent<ShipHealth>();

            // Determine shield color profile
            if (CompareTag("Player") || (health != null && health.isPlayer))
            {
                currentShieldColor = playerShieldColor;
            }
            else if (CompareTag("Boss"))
            {
                currentShieldColor = bossShieldColor;
            }
            else
            {
                currentShieldColor = enemyShieldColor;
            }

            // Auto-detect shield envelope radius from ship physical collider
            if (autoDetectRadius)
            {
                var col = GetComponent<CircleCollider2D>();
                if (col != null && col.radius > 0.1f)
                {
                    shieldRadius = col.radius * 1.35f;
                }
                else
                {
                    shieldRadius = Mathf.Max(0.8f, transform.localScale.x * 0.85f);
                }
            }

            // Setup perimeter energy dome child object
            if (domeObject == null)
            {
                Transform existing = transform.Find("ShieldPerimeterDome");
                if (existing != null)
                {
                    domeObject = existing.gameObject;
                    domeRenderer = domeObject.GetComponent<SpriteRenderer>();
                }
                else
                {
                    domeObject = new GameObject("ShieldPerimeterDome");
                    domeObject.transform.SetParent(transform, false);
                    domeObject.transform.localPosition = Vector3.zero;
                    domeRenderer = domeObject.AddComponent<SpriteRenderer>();
                }
            }

            if (domeRenderer != null)
            {
                if (domeRenderer.sprite == null)
                {
                    domeRenderer.sprite = ProceduralSpriteHelper.GetShieldDomeSprite(128);
                }
                domeRenderer.sortingOrder = 5; // Layer 5: above ship hull (layer 4)
                domeRenderer.color = Color.clear;

                var shader = Shader.Find("Sprites/Default");
                if (shader != null && domeRenderer.material == null)
                {
                    domeRenderer.material = new Material(shader);
                }

                // Size dome according to shield radius (normalized to 128px sprite size = ~1.28 world units)
                float domeDiameter = shieldRadius * 2.1f;
                domeObject.transform.localScale = new Vector3(domeDiameter, domeDiameter, 1f);
            }
        }

        public void SubscribeEvents()
        {
            if (health != null)
            {
                health.OnShieldImpact -= HandleShieldImpact;
                health.OnShieldImpact += HandleShieldImpact;
            }
        }

        public void UnsubscribeEvents()
        {
            if (health != null)
            {
                health.OnShieldImpact -= HandleShieldImpact;
            }
        }

        public void TriggerShieldHit(Vector2 hitPoint, float damage = 20f)
        {
            HandleShieldImpact(hitPoint, damage);
        }

        private void HandleShieldImpact(Vector2 hitPoint, float damage)
        {
            if (health != null && health.currentShield <= 0f)
            {
                // Shield is depleted; damage penetrates straight to hull, no shield ripple
                return;
            }

            // 1. Flare the perimeter energy dome
            currentDomeAlpha = 0.88f;

            // 2. Calculate impact position on the shield envelope boundary
            Vector2 shipPos = transform.position;
            Vector2 dir = (hitPoint - shipPos).normalized;
            if (dir == Vector2.zero) dir = Vector2.up;

            Vector2 surfacePoint = shipPos + dir * shieldRadius;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rippleRotation = Quaternion.Euler(0f, 0f, angle - 90f);

            // 3. Spawn localized hexagonal impact ripple from PoolManager
            float startR = shieldRadius * 0.35f;
            float endR = shieldRadius * 1.15f;

            if (PoolManager.Instance != null)
            {
                GameObject rippleObj = PoolManager.Instance.Spawn("ShieldImpactRipple", surfacePoint, rippleRotation, () => {
                    var go = new GameObject("ShieldImpactRipple");
                    go.AddComponent<ShieldImpactRipple>();
                    return go;
                });

                if (rippleObj != null)
                {
                    var ripple = rippleObj.GetComponent<ShieldImpactRipple>();
                    if (ripple != null)
                    {
                        ripple.Initialize(currentShieldColor, startR, endR, 0.22f);
                    }
                }
            }

            // 4. Subtle audio feedback
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("laser", 0.25f, 0.15f);
        }

        private void Update()
        {
            // Exponential fade-out for the perimeter energy dome
            if (currentDomeAlpha > 0f)
            {
                currentDomeAlpha = Mathf.MoveTowards(currentDomeAlpha, 0f, domeFadeSpeed * Time.deltaTime);
                if (domeRenderer != null)
                {
                    domeRenderer.color = new Color(currentShieldColor.r, currentShieldColor.g, currentShieldColor.b, currentDomeAlpha);
                }
            }
        }

        public void SetShieldRadius(float radius)
        {
            shieldRadius = radius;
            if (domeObject != null)
            {
                float domeDiameter = shieldRadius * 2.1f;
                domeObject.transform.localScale = new Vector3(domeDiameter, domeDiameter, 1f);
            }
        }
    }
}
