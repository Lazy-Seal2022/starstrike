using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Persistent dynamic shield field component for ships.
    /// Manages:
    /// 1. The perimeter energy dome barrier surrounding the ship with idle breathing glow.
    /// 2. Glowing Shield Contour Line (LineRenderer) tracing the exact offset envelope in-game.
    /// 3. Dual-Hitbox System: Outer shield trigger envelope (hull outline + radial offset)
    ///    that captures incoming projectiles, flares the shield dome, and spawns impact ripples.
    /// 4. Automatically toggles the shield trigger collider and visual barrier off when shields
    ///    deplete (allowing projectiles to penetrate and only strike the tighter physical hull
    ///    collider), and re-enables it when shields regenerate.
    /// 5. Scene-view Gizmos drawing the hull and shield polygons.
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

        [Header("Shield Hitbox Tuning")]
        [Tooltip("Radial offset extending outward from the hull outline")]
        public float shieldHitboxOffset = 0.35f;

        [Header("Visual Contour")]
        public bool showShieldContour = true;
        public float contourLineWidth = 0.045f;

        private ShipHealth health;
        private SpriteRenderer domeRenderer;
        private GameObject domeObject;
        private Collider2D shieldCollider;
        private LineRenderer shieldLine;
        private Color currentShieldColor;
        private float currentDomeAlpha = 0f;

        public float ShieldRadius => shieldRadius;
        public Color ShieldColor => currentShieldColor;
        public float CurrentDomeAlpha => currentDomeAlpha;
        public Collider2D ShieldCollider => shieldCollider;
        public LineRenderer ShieldLine => shieldLine;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            EnsureInitialized();
            SubscribeEvents();
            UpdateShieldVisualState();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            if (domeRenderer != null)
            {
                domeRenderer.color = Color.clear;
            }
            if (shieldLine != null)
            {
                shieldLine.enabled = false;
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
                var poly = GetComponent<PolygonCollider2D>();
                if (poly != null && poly.pathCount > 0)
                {
                    Bounds b = poly.bounds;
                    shieldRadius = Mathf.Max(b.extents.x, b.extents.y) + shieldHitboxOffset;
                }
                else
                {
                    var col = GetComponent<CircleCollider2D>();
                    if (col != null && col.radius > 0.1f)
                    {
                        shieldRadius = col.radius + shieldHitboxOffset;
                    }
                    else
                    {
                        shieldRadius = Mathf.Max(0.8f, transform.localScale.x * 0.85f);
                    }
                }
            }

            // Setup perimeter energy dome child object
            if (domeObject == null)
            {
                Transform existing = transform.Find("ShieldPerimeterDome");
                if (existing != null)
                {
                    domeObject = existing.gameObject;
                }
                else
                {
                    domeObject = new GameObject("ShieldPerimeterDome");
                    domeObject.transform.SetParent(transform, false);
                    domeObject.transform.localPosition = Vector3.zero;
                    domeObject.transform.localRotation = Quaternion.identity;
                    domeObject.transform.localScale = Vector3.one;
                }
            }
            domeObject.transform.localScale = Vector3.one;
            domeObject.tag = gameObject.tag;

            // Visual dome renderer is on child DomeVisual under domeObject
            if (domeRenderer == null)
            {
                Transform visualTrans = domeObject.transform.Find("DomeVisual");
                GameObject visualObj = null;
                if (visualTrans != null)
                {
                    visualObj = visualTrans.gameObject;
                    domeRenderer = visualObj.GetComponent<SpriteRenderer>();
                }
                else
                {
                    var oldSr = domeObject.GetComponent<SpriteRenderer>();
                    if (oldSr != null)
                    {
                        DestroyImmediate(oldSr);
                    }
                    visualObj = new GameObject("DomeVisual");
                    visualObj.transform.SetParent(domeObject.transform, false);
                    visualObj.transform.localPosition = Vector3.zero;
                    visualObj.transform.localRotation = Quaternion.identity;
                    domeRenderer = visualObj.AddComponent<SpriteRenderer>();
                }
            }

            if (domeRenderer != null)
            {
                if (domeRenderer.sprite == null)
                {
                    domeRenderer.sprite = ProceduralSpriteHelper.GetShieldDomeSprite(128);
                }
                domeRenderer.sortingOrder = 11; // Layer 11: above ship hull (layer 10)
                domeRenderer.color = Color.clear;

                var shader = Shader.Find("Sprites/Default");
                if (shader != null && domeRenderer.sharedMaterial == null)
                {
                    domeRenderer.sharedMaterial = new Material(shader);
                }

                float domeDiameter = shieldRadius * 2.1f;
                domeRenderer.transform.localScale = new Vector3(domeDiameter, domeDiameter, 1f);
            }

            EnsureShieldLine();
            RebuildShieldCollider(shieldHitboxOffset);
        }

        private void EnsureShieldLine()
        {
            if (domeObject == null) return;
            if (shieldLine == null)
            {
                shieldLine = domeObject.GetComponent<LineRenderer>();
                if (shieldLine == null)
                {
                    shieldLine = domeObject.AddComponent<LineRenderer>();
                }
                shieldLine.useWorldSpace = false;
                shieldLine.startWidth = contourLineWidth;
                shieldLine.endWidth = contourLineWidth;
                shieldLine.sortingOrder = 12; // Layer 12: above dome (11) and hull (10)

                var shader = Shader.Find("Sprites/Default");
                if (shader != null && shieldLine.sharedMaterial == null)
                {
                    shieldLine.sharedMaterial = new Material(shader);
                }
                shieldLine.startColor = currentShieldColor;
                shieldLine.endColor = currentShieldColor;
            }
        }

        public void RebuildShieldCollider(float offset = -1f)
        {
            if (offset < 0f) offset = shieldHitboxOffset;
            if (domeObject == null) EnsureInitialized();
            if (domeObject == null) return;

            PolygonCollider2D hullPoly = GetComponent<PolygonCollider2D>();
            if (hullPoly != null && hullPoly.pathCount > 0)
            {
                PolygonCollider2D shieldPoly = domeObject.GetComponent<PolygonCollider2D>();
                if (shieldPoly == null)
                {
                    var oldCol = domeObject.GetComponent<Collider2D>();
                    if (oldCol != null) DestroyImmediate(oldCol);
                    shieldPoly = domeObject.AddComponent<PolygonCollider2D>();
                }
                shieldPoly.isTrigger = true;
                shieldPoly.pathCount = hullPoly.pathCount;

                for (int p = 0; p < hullPoly.pathCount; p++)
                {
                    Vector2[] hullPts = hullPoly.GetPath(p);
                    Vector2[] shieldPts = new Vector2[hullPts.Length];
                    for (int i = 0; i < hullPts.Length; i++)
                    {
                        Vector2 dir = hullPts[i].normalized;
                        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
                        shieldPts[i] = hullPts[i] + dir * offset;
                    }
                    shieldPoly.SetPath(p, shieldPts);
                }
                shieldCollider = shieldPoly;

                // Configure glowing shield contour line
                if (showShieldContour && shieldPoly.pathCount > 0)
                {
                    EnsureShieldLine();
                    Vector2[] pts = shieldPoly.GetPath(0);
                    if (shieldLine != null && pts != null && pts.Length > 0)
                    {
                        shieldLine.positionCount = pts.Length;
                        for (int i = 0; i < pts.Length; i++)
                        {
                            shieldLine.SetPosition(i, new Vector3(pts[i].x, pts[i].y, 0f));
                        }
                        shieldLine.loop = true;
                    }
                }
            }
            else
            {
                CircleCollider2D hullCircle = GetComponent<CircleCollider2D>();
                float r = hullCircle != null ? hullCircle.radius + offset : shieldRadius;

                CircleCollider2D shieldCircle = domeObject.GetComponent<CircleCollider2D>();
                if (shieldCircle == null)
                {
                    var oldCol = domeObject.GetComponent<Collider2D>();
                    if (oldCol != null) DestroyImmediate(oldCol);
                    shieldCircle = domeObject.AddComponent<CircleCollider2D>();
                }
                shieldCircle.isTrigger = true;
                shieldCircle.radius = r;
                shieldCollider = shieldCircle;

                // Circular contour line fallback
                if (showShieldContour)
                {
                    EnsureShieldLine();
                    if (shieldLine != null)
                    {
                        int segments = 36;
                        shieldLine.positionCount = segments;
                        for (int i = 0; i < segments; i++)
                        {
                            float rad = (i / (float)segments) * Mathf.PI * 2f;
                            shieldLine.SetPosition(i, new Vector3(Mathf.Cos(rad) * r, Mathf.Sin(rad) * r, 0f));
                        }
                        shieldLine.loop = true;
                    }
                }
            }

            UpdateShieldVisualState();
        }

        public void SubscribeEvents()
        {
            if (health != null)
            {
                health.OnShieldImpact -= HandleShieldImpact;
                health.OnShieldImpact += HandleShieldImpact;
                health.OnShieldChanged -= HandleShieldChanged;
                health.OnShieldChanged += HandleShieldChanged;
            }
        }

        public void UnsubscribeEvents()
        {
            if (health != null)
            {
                health.OnShieldImpact -= HandleShieldImpact;
                health.OnShieldChanged -= HandleShieldChanged;
            }
        }

        private void HandleShieldChanged(float current, float max)
        {
            UpdateShieldVisualState();
        }

        private void UpdateShieldVisualState()
        {
            bool hasShield = health == null || health.currentShield > 0.01f;

            if (shieldCollider != null)
            {
                shieldCollider.enabled = hasShield;
            }

            if (!hasShield)
            {
                currentDomeAlpha = 0f;
                if (domeRenderer != null)
                {
                    domeRenderer.color = Color.clear;
                }
                if (shieldLine != null)
                {
                    shieldLine.enabled = false;
                }
            }
            else
            {
                if (shieldLine != null && showShieldContour)
                {
                    shieldLine.enabled = true;
                }
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

            // 1. Flare the perimeter energy dome & contour
            currentDomeAlpha = 0.92f;

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
            bool hasShield = health == null || health.currentShield > 0.01f;

            if (hasShield)
            {
                // Smooth sine-wave breathing oscillation for the glowing shield contour
                float breathe = 0.42f + 0.18f * Mathf.Sin(Time.time * 3.2f);
                float lineAlpha = Mathf.Max(breathe, currentDomeAlpha);
                Color activeColor = new Color(currentShieldColor.r, currentShieldColor.g, currentShieldColor.b, lineAlpha);

                if (shieldLine != null && showShieldContour)
                {
                    shieldLine.enabled = true;
                    shieldLine.startColor = activeColor;
                    shieldLine.endColor = activeColor;
                    float width = contourLineWidth + currentDomeAlpha * 0.04f;
                    shieldLine.startWidth = width;
                    shieldLine.endWidth = width;
                }

                // Dome flare fade-out + gentle ambient shield glow
                if (currentDomeAlpha > 0f)
                {
                    currentDomeAlpha = Mathf.MoveTowards(currentDomeAlpha, 0f, domeFadeSpeed * Time.deltaTime);
                }

                if (domeRenderer != null)
                {
                    float domeAlpha = Mathf.Clamp01(0.08f + currentDomeAlpha * 0.75f);
                    domeRenderer.color = new Color(currentShieldColor.r, currentShieldColor.g, currentShieldColor.b, domeAlpha);
                }
            }
            else
            {
                if (shieldLine != null && shieldLine.enabled)
                {
                    shieldLine.enabled = false;
                }
                if (domeRenderer != null && domeRenderer.color.a > 0f)
                {
                    domeRenderer.color = Color.clear;
                }
            }
        }

        public void SetShieldRadius(float radius)
        {
            shieldRadius = radius;
            if (domeRenderer != null)
            {
                float domeDiameter = shieldRadius * 2.1f;
                domeRenderer.transform.localScale = new Vector3(domeDiameter, domeDiameter, 1f);
            }
            RebuildShieldCollider(shieldHitboxOffset);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw hull polygon in vibrant green
            var hullPoly = GetComponent<PolygonCollider2D>();
            if (hullPoly != null && hullPoly.pathCount > 0)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
                for (int p = 0; p < hullPoly.pathCount; p++)
                {
                    Vector2[] pts = hullPoly.GetPath(p);
                    for (int i = 0; i < pts.Length; i++)
                    {
                        Vector3 p1 = transform.TransformPoint(pts[i]);
                        Vector3 p2 = transform.TransformPoint(pts[(i + 1) % pts.Length]);
                        Gizmos.DrawLine(p1, p2);
                    }
                }
            }

            // Draw shield offset polygon in cyan / electric blue
            if (domeObject != null)
            {
                var shieldPoly = domeObject.GetComponent<PolygonCollider2D>();
                if (shieldPoly != null && shieldPoly.pathCount > 0)
                {
                    Gizmos.color = (health != null && health.currentShield <= 0f)
                        ? new Color(1f, 0.25f, 0.25f, 0.5f)
                        : new Color(0.2f, 0.9f, 1f, 0.95f);

                    for (int p = 0; p < shieldPoly.pathCount; p++)
                    {
                        Vector2[] pts = shieldPoly.GetPath(p);
                        for (int i = 0; i < pts.Length; i++)
                        {
                            Vector3 p1 = domeObject.transform.TransformPoint(pts[i]);
                            Vector3 p2 = domeObject.transform.TransformPoint(pts[(i + 1) % pts.Length]);
                            Gizmos.DrawLine(p1, p2);
                        }
                    }
                }
            }
        }
    }
}
