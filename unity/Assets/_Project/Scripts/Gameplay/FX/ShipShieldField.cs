using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    public enum ShieldHitboxShape
    {
        Round,          // Smooth round protective bubble (snug ellipse enclosing the ship, not too fat)
        Circle,         // Uniform circular bubble enclosing the ship
        HullOutline     // Exact hull polygon offset
    }

    /// <summary>
    /// Persistent dynamic shield field component for ships (Battlevoid: Harbinger style).
    /// Manages:
    /// 1. Outer shield trigger envelope (smooth round protective bubble enclosing the ship snugly).
    /// 2. Invisible when idle (clean ship, no obtrusive permanent rings or lines).
    /// 3. On shield hit: momentary flash of the round shield bubble + localized
    ///    impact ripple at the hit point on the shield surface.
    /// 4. When shields deplete: shield trigger collider turns off, incoming fire penetrates
    ///    directly to the tighter physical hull polygon.
    /// 5. When shields regenerate: trigger collider automatically re-arms.
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
        public float domeFadeSpeed = 5.5f;

        [Header("Shield Hitbox Tuning")]
        public ShieldHitboxShape shieldShape = ShieldHitboxShape.Round;

        [Tooltip("Radial offset extending outward from the hull outline (snug, not too fat)")]
        public float shieldHitboxOffset = 0.20f;

        [Header("Visual Contour")]
        public bool showShieldContour = true;
        public float contourLineWidth = 0.045f;

        private ShipHealth health;
        private GameObject domeObject;
        private Collider2D shieldCollider;
        private LineRenderer shieldLine;
        private Color currentShieldColor;
        private float currentDomeAlpha = 0f;

        private static Material cachedShieldMat;

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

            // Clean up any legacy circular DomeVisual if present
            Transform legacyDome = domeObject.transform.Find("DomeVisual");
            if (legacyDome != null)
            {
                if (Application.isPlaying) Destroy(legacyDome.gameObject);
                else DestroyImmediate(legacyDome.gameObject);
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
                shieldLine.sortingOrder = 12; // Above hull (layer 10)

                if (cachedShieldMat == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        cachedShieldMat = new Material(shader);
                    }
                }
                if (cachedShieldMat != null)
                {
                    shieldLine.sharedMaterial = cachedShieldMat;
                }

                shieldLine.startColor = currentShieldColor;
                shieldLine.endColor = currentShieldColor;
                shieldLine.enabled = false; // Invisible when idle (Battlevoid: Harbinger style)
            }
        }

        public void RebuildShieldCollider(float offset = -1f)
        {
            if (offset < 0f) offset = shieldHitboxOffset;
            if (domeObject == null) EnsureInitialized();
            if (domeObject == null) return;

            var hullPoly = GetComponent<PolygonCollider2D>();

            // 1. Determine ship center and extents in local space
            Vector2 center = Vector2.zero;
            float extX = 1.0f;
            float extY = 1.0f;

            if (hullPoly != null && hullPoly.pathCount > 0)
            {
                Vector2[] pts = hullPoly.GetPath(0);
                if (pts.Length > 0)
                {
                    float minX = pts[0].x, maxX = pts[0].x;
                    float minY = pts[0].y, maxY = pts[0].y;
                    for (int i = 1; i < pts.Length; i++)
                    {
                        if (pts[i].x < minX) minX = pts[i].x;
                        if (pts[i].x > maxX) maxX = pts[i].x;
                        if (pts[i].y < minY) minY = pts[i].y;
                        if (pts[i].y > maxY) maxY = pts[i].y;
                    }
                    center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
                    extX = (maxX - minX) * 0.5f;
                    extY = (maxY - minY) * 0.5f;
                }
            }
            else
            {
                var circle = GetComponent<CircleCollider2D>();
                if (circle != null)
                {
                    center = circle.offset;
                    extX = circle.radius;
                    extY = circle.radius;
                }
                else
                {
                    var sr = GetComponent<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        Bounds b = sr.sprite.bounds;
                        center = b.center;
                        extX = b.extents.x;
                        extY = b.extents.y;
                    }
                }
            }

            extX = Mathf.Max(0.2f, extX);
            extY = Mathf.Max(0.2f, extY);

            if (shieldShape == ShieldHitboxShape.HullOutline && hullPoly != null && hullPoly.pathCount > 0)
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
                        shieldLine.enabled = false;
                    }
                }
            }
            else
            {
                // Round shield bubble (snug ellipse or circle, covered on the ship but not too fat!)
                float rx, ry;
                if (shieldShape == ShieldHitboxShape.Circle)
                {
                    float r = Mathf.Max(extX, extY) + offset;
                    rx = r;
                    ry = r;
                }
                else // ShieldHitboxShape.Round (default)
                {
                    rx = extX + offset;
                    ry = extY + offset;
                }

                shieldRadius = Mathf.Max(rx, ry);

                int segments = 32;
                Vector2[] roundPts = new Vector2[segments];
                for (int i = 0; i < segments; i++)
                {
                    float angle = (i / (float)segments) * Mathf.PI * 2f;
                    roundPts[i] = center + new Vector2(Mathf.Cos(angle) * rx, Mathf.Sin(angle) * ry);
                }

                PolygonCollider2D shieldPoly = domeObject.GetComponent<PolygonCollider2D>();
                if (shieldPoly == null)
                {
                    var oldCol = domeObject.GetComponent<Collider2D>();
                    if (oldCol != null) DestroyImmediate(oldCol);
                    shieldPoly = domeObject.AddComponent<PolygonCollider2D>();
                }
                shieldPoly.isTrigger = true;
                shieldPoly.pathCount = 1;
                shieldPoly.SetPath(0, roundPts);
                shieldCollider = shieldPoly;

                if (showShieldContour)
                {
                    EnsureShieldLine();
                    if (shieldLine != null)
                    {
                        shieldLine.positionCount = segments;
                        for (int i = 0; i < segments; i++)
                        {
                            shieldLine.SetPosition(i, new Vector3(roundPts[i].x, roundPts[i].y, 0f));
                        }
                        shieldLine.loop = true;
                        shieldLine.enabled = false; // Invisible when idle
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
                if (shieldLine != null)
                {
                    shieldLine.enabled = false;
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
                return;
            }

            // 1. Flash the exact shield silhouette contour line (NO circular dome!)
            currentDomeAlpha = 0.90f;
            if (shieldLine != null && showShieldContour)
            {
                shieldLine.enabled = true;
                Color lineColor = new Color(currentShieldColor.r, currentShieldColor.g, currentShieldColor.b, currentDomeAlpha);
                shieldLine.startColor = lineColor;
                shieldLine.endColor = lineColor;
                float width = contourLineWidth + 0.025f;
                shieldLine.startWidth = width;
                shieldLine.endWidth = width;
            }

            // 2. Calculate impact position on the shield envelope boundary
            Vector2 shipPos = transform.position;
            Vector2 dir = (hitPoint - shipPos).normalized;
            if (dir == Vector2.zero) dir = Vector2.up;

            Vector2 surfacePoint = shipPos + dir * shieldRadius;
            if (shieldCollider is PolygonCollider2D polyCol && polyCol.pathCount > 0)
            {
                Vector2 localDir = transform.InverseTransformDirection(dir);
                float localAngle = Mathf.Atan2(localDir.y, localDir.x);
                Bounds sBounds = shieldCollider.bounds;
                float rx = sBounds.extents.x / Mathf.Max(0.001f, transform.lossyScale.x);
                float ry = sBounds.extents.y / Mathf.Max(0.001f, transform.lossyScale.y);
                Vector2 localSurface = new Vector2(Mathf.Cos(localAngle) * rx, Mathf.Sin(localAngle) * ry);
                surfacePoint = transform.TransformPoint(localSurface);
            }

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rippleRotation = Quaternion.Euler(0f, 0f, angle - 90f);

            // 3. Spawn localized kinetic impact ripple at the contact point (tight, crisp spark/ripple)
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
                        ripple.Initialize(currentShieldColor, 0.15f, 0.45f, 0.16f);
                    }
                }
            }

            // 4. Subtle audio feedback
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("laser", 0.25f, 0.15f);
        }

        private void Update()
        {
            // Battlevoid: Harbinger style: Shield is invisible when idle.
            // On hit, the exact silhouette flashes and quickly fades back to 0.
            if (currentDomeAlpha > 0f)
            {
                currentDomeAlpha = Mathf.MoveTowards(currentDomeAlpha, 0f, domeFadeSpeed * Time.deltaTime);

                if (shieldLine != null && showShieldContour)
                {
                    Color lineColor = new Color(currentShieldColor.r, currentShieldColor.g, currentShieldColor.b, currentDomeAlpha);
                    shieldLine.startColor = lineColor;
                    shieldLine.endColor = lineColor;
                    float width = contourLineWidth + currentDomeAlpha * 0.025f;
                    shieldLine.startWidth = width;
                    shieldLine.endWidth = width;
                }

                if (currentDomeAlpha <= 0f)
                {
                    if (shieldLine != null) shieldLine.enabled = false;
                }
            }
        }

        public void SetShieldRadius(float radius)
        {
            shieldRadius = radius;
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
