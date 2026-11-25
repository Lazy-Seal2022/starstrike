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

        [Header("Contact Shield Flare Tuning")]
        [Tooltip("Max half-angle cap in degrees of the contact arc (fallback limit)")]
        public float contactArcHalfAngle = 35f;

        [Tooltip("Target physical arc length (in world units) of the localized contact flare")]
        public float contactArcPhysicalLength = 1.4f;

        [Tooltip("Width of the contact barrier arc at the impact epicenter")]
        public float contactArcWidth = 0.08f;

        private struct ContactArcSlot
        {
            public LineRenderer line;
            public float alpha;
        }

        private const int MAX_CONTACT_ARCS = 4;
        private ContactArcSlot[] contactArcs;
        private int currentArcIndex = 0;
        private bool hasActiveContactArc = false;

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
            if (contactArcs != null)
            {
                for (int i = 0; i < contactArcs.Length; i++)
                {
                    contactArcs[i].alpha = 0f;
                    if (contactArcs[i].line != null) contactArcs[i].line.enabled = false;
                }
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
                ConfigureLineRenderer(shieldLine);
            }

            if (contactArcs == null || contactArcs.Length < MAX_CONTACT_ARCS)
            {
                contactArcs = new ContactArcSlot[MAX_CONTACT_ARCS];
                contactArcs[0].line = shieldLine;
                contactArcs[0].alpha = 0f;

                for (int i = 1; i < MAX_CONTACT_ARCS; i++)
                {
                    string arcName = $"ContactArc_{i}";
                    Transform child = domeObject.transform.Find(arcName);
                    GameObject arcObj = child != null ? child.gameObject : null;
                    if (arcObj == null)
                    {
                        arcObj = new GameObject(arcName);
                        arcObj.transform.SetParent(domeObject.transform, false);
                        arcObj.transform.localPosition = Vector3.zero;
                        arcObj.transform.localRotation = Quaternion.identity;
                        arcObj.transform.localScale = Vector3.one;
                    }
                    LineRenderer lr = arcObj.GetComponent<LineRenderer>();
                    if (lr == null) lr = arcObj.AddComponent<LineRenderer>();
                    ConfigureLineRenderer(lr);
                    contactArcs[i].line = lr;
                    contactArcs[i].alpha = 0f;
                }
            }
        }

        private static AnimationCurve cachedContactWidthCurve;
        private static readonly Gradient cachedArcGradient = new Gradient();
        private static readonly GradientColorKey[] cachedColorKeys = new GradientColorKey[2];
        private static readonly GradientAlphaKey[] cachedAlphaKeys = new GradientAlphaKey[3];
        private static readonly Vector3[] cachedArcPositions = new Vector3[13];

        private static AnimationCurve GetContactWidthCurve()
        {
            if (cachedContactWidthCurve == null)
            {
                cachedContactWidthCurve = new AnimationCurve();
                cachedContactWidthCurve.AddKey(0f, 0f);
                cachedContactWidthCurve.AddKey(0.5f, 1f);
                cachedContactWidthCurve.AddKey(1f, 0f);
            }
            return cachedContactWidthCurve;
        }

        private void ApplyContactArcGradient(LineRenderer lr, float alpha)
        {
            if (lr == null) return;
            cachedColorKeys[0] = new GradientColorKey(currentShieldColor, 0f);
            cachedColorKeys[1] = new GradientColorKey(currentShieldColor, 1f);
            cachedAlphaKeys[0] = new GradientAlphaKey(0f, 0f);
            cachedAlphaKeys[1] = new GradientAlphaKey(alpha, 0.5f);
            cachedAlphaKeys[2] = new GradientAlphaKey(0f, 1f);
            cachedArcGradient.SetKeys(cachedColorKeys, cachedAlphaKeys);
            lr.colorGradient = cachedArcGradient;
        }

        private void ConfigureLineRenderer(LineRenderer lr)
        {
            if (lr == null) return;
            lr.useWorldSpace = false;
            lr.loop = false;
            lr.startWidth = contourLineWidth;
            lr.endWidth = contourLineWidth;
            lr.widthCurve = GetContactWidthCurve();
            lr.widthMultiplier = contactArcWidth > 0.01f ? contactArcWidth : (contourLineWidth + 0.035f);
            lr.sortingOrder = 12; // Above hull (layer 10)

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
                lr.sharedMaterial = cachedShieldMat;
            }

            lr.startColor = currentShieldColor;
            lr.endColor = currentShieldColor;
            lr.enabled = false; // Invisible when idle (Battlevoid: Harbinger style)
        }

        private Vector2 shieldCenter = Vector2.zero;
        private float shieldRadiusX = 1f;
        private float shieldRadiusY = 1f;
        private float shieldPExp = 2.4f;
        private static readonly System.Collections.Generic.List<Vector2> candidatePtsBuffer = new System.Collections.Generic.List<Vector2>(128);

        public void RebuildShieldCollider(float offset = -1f)
        {
            if (offset < 0f) offset = shieldHitboxOffset;
            if (domeObject == null) EnsureInitialized();
            if (domeObject == null) return;

            var hullPoly = GetComponent<PolygonCollider2D>();

            // 1. Collect all boundary points (hull outline vertices, sprite bounds, weapon/thruster sockets)
            // to guarantee 100% enclosure of the visible ship sprite.
            candidatePtsBuffer.Clear();

            if (hullPoly != null && hullPoly.pathCount > 0)
            {
                for (int p = 0; p < hullPoly.pathCount; p++)
                {
                    Vector2[] pPts = hullPoly.GetPath(p);
                    if (pPts != null && pPts.Length > 0)
                    {
                        candidatePtsBuffer.AddRange(pPts);
                    }
                }
            }

            var weapon = GetComponent<ShipWeapon>();
            if (weapon != null)
            {
                var sockets = weapon.GetActiveSocketLocalPositions();
                if (sockets != null) candidatePtsBuffer.AddRange(sockets);
            }

            var prog = GetComponent<StarStrike.Gameplay.PlayerProgression>();
            if (prog != null && prog.currentShip != null)
            {
                if (prog.currentShip.weaponSockets != null)
                    candidatePtsBuffer.AddRange(prog.currentShip.weaponSockets);
                if (prog.currentShip.thrusterSockets != null)
                    candidatePtsBuffer.AddRange(prog.currentShip.thrusterSockets);
            }

            var sr = GetComponent<SpriteRenderer>();
            if (candidatePtsBuffer.Count == 0)
            {
                var circle = GetComponent<CircleCollider2D>();
                if (circle != null && circle.radius > 0.05f)
                {
                    candidatePtsBuffer.Add(circle.offset + new Vector2(circle.radius, 0f));
                    candidatePtsBuffer.Add(circle.offset + new Vector2(-circle.radius, 0f));
                    candidatePtsBuffer.Add(circle.offset + new Vector2(0f, circle.radius));
                    candidatePtsBuffer.Add(circle.offset + new Vector2(0f, -circle.radius));
                }
                else if (sr != null && sr.sprite != null)
                {
                    Bounds b = sr.sprite.bounds;
                    candidatePtsBuffer.Add(new Vector2(b.min.x, b.min.y));
                    candidatePtsBuffer.Add(new Vector2(b.min.x, b.max.y));
                    candidatePtsBuffer.Add(new Vector2(b.max.x, b.min.y));
                    candidatePtsBuffer.Add(new Vector2(b.max.x, b.max.y));
                }
            }

            // 2. Determine geometric center and base extents
            Vector2 center = Vector2.zero;
            float extX = 1.0f;
            float extY = 1.0f;

            if (candidatePtsBuffer.Count > 0)
            {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                for (int i = 0; i < candidatePtsBuffer.Count; i++)
                {
                    Vector2 pt = candidatePtsBuffer[i];
                    if (pt.x < minX) minX = pt.x;
                    if (pt.x > maxX) maxX = pt.x;
                    if (pt.y < minY) minY = pt.y;
                    if (pt.y > maxY) maxY = pt.y;
                }
                center = new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
                extX = (maxX - minX) * 0.5f;
                extY = (maxY - minY) * 0.5f;
            }

            extX = Mathf.Max(0.2f, extX);
            extY = Mathf.Max(0.2f, extY);
            shieldCenter = center;

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
                }
            }
            else
            {
                // Round shield bubble (smooth continuous envelope enclosing 100% of the sprite without being bloated)
                float pExp = 2.4f;
                shieldPExp = pExp;
                float maxNormDist = 1.0f;
                float maxRadDist = 0.5f;

                for (int i = 0; i < candidatePtsBuffer.Count; i++)
                {
                    float dx = candidatePtsBuffer[i].x - center.x;
                    float dy = candidatePtsBuffer[i].y - center.y;
                    float normD = Mathf.Pow(Mathf.Pow(Mathf.Abs(dx) / extX, pExp) + Mathf.Pow(Mathf.Abs(dy) / extY, pExp), 1.0f / pExp);
                    if (normD > maxNormDist) maxNormDist = normD;

                    float rad = Mathf.Sqrt(dx * dx + dy * dy);
                    if (rad > maxRadDist) maxRadDist = rad;
                }

                float rx, ry;
                if (shieldShape == ShieldHitboxShape.Circle)
                {
                    float r = maxRadDist + offset;
                    rx = r;
                    ry = r;
                }
                else // ShieldHitboxShape.Round (default)
                {
                    rx = extX * maxNormDist + offset;
                    ry = extY * maxNormDist + offset;
                }

                shieldRadiusX = rx;
                shieldRadiusY = ry;
                shieldRadius = Mathf.Max(rx, ry);

                int segments = 32;
                Vector2[] roundPts = new Vector2[segments];
                for (int i = 0; i < segments; i++)
                {
                    float angle = (i / (float)segments) * Mathf.PI * 2f;
                    float cosA = Mathf.Cos(angle);
                    float sinA = Mathf.Sin(angle);

                    float xVal, yVal;
                    if (shieldShape == ShieldHitboxShape.Circle)
                    {
                        xVal = cosA * rx;
                        yVal = sinA * ry;
                    }
                    else
                    {
                        xVal = Mathf.Sign(cosA) * Mathf.Pow(Mathf.Abs(cosA), 2.0f / pExp) * rx;
                        yVal = Mathf.Sign(sinA) * Mathf.Pow(Mathf.Abs(sinA), 2.0f / pExp) * ry;
                    }

                    roundPts[i] = center + new Vector2(xVal, yVal);
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
                if (contactArcs != null)
                {
                    for (int i = 0; i < contactArcs.Length; i++)
                    {
                        contactArcs[i].alpha = 0f;
                        if (contactArcs[i].line != null) contactArcs[i].line.enabled = false;
                    }
                }
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

            // 1. Calculate impact position and angle on the shield envelope boundary
            Vector2 shipPos = transform.position;
            Vector2 dir = (hitPoint - shipPos).normalized;
            if (dir == Vector2.zero) dir = transform.right;

            Vector2 localDir = transform.InverseTransformDirection(dir);
            float localAngle = Mathf.Atan2(localDir.y, localDir.x);

            float cosHit = Mathf.Cos(localAngle);
            float sinHit = Mathf.Sin(localAngle);
            float lx, ly;
            if (shieldShape == ShieldHitboxShape.Circle)
            {
                lx = cosHit * shieldRadius;
                ly = sinHit * shieldRadius;
            }
            else if (shieldShape == ShieldHitboxShape.HullOutline)
            {
                lx = cosHit * shieldRadius;
                ly = sinHit * shieldRadius;
            }
            else // ShieldHitboxShape.Round (default)
            {
                lx = Mathf.Sign(cosHit) * Mathf.Pow(Mathf.Abs(cosHit), 2.0f / shieldPExp) * shieldRadiusX;
                ly = Mathf.Sign(sinHit) * Mathf.Pow(Mathf.Abs(sinHit), 2.0f / shieldPExp) * shieldRadiusY;
            }
            Vector2 surfacePoint = transform.TransformPoint(shieldCenter + new Vector2(lx, ly));

            // 2. Localized contact barrier flare: ONLY the contact area lights up!
            // Dynamically calculate angular span from target physical arc length:
            // delta_theta = length / tanSpeed, ensuring uniform compact contact flare on all ships
            // regardless of whether hit is on the long flank or the narrow nose.
            EnsureShieldLine();
            if (showShieldContour && contactArcs != null && contactArcs.Length > 0)
            {
                int arcSegments = 12; // 13 smooth points along the contact curvature
                float targetPhysicalLength = contactArcPhysicalLength > 0.2f ? contactArcPhysicalLength : 1.4f;
                float halfPhysicalLength = targetPhysicalLength * 0.5f;

                // Tangential speed along the ellipse/superellipse boundary at localAngle:
                // v(theta) = sqrt((Rx * sin(theta))^2 + (Ry * cos(theta))^2) * superellipseCorrection
                float tanSpeed;
                if (shieldShape == ShieldHitboxShape.Circle || shieldRadiusX <= 0.01f || shieldRadiusY <= 0.01f)
                {
                    tanSpeed = Mathf.Max(0.5f, shieldRadius);
                }
                else
                {
                    tanSpeed = Mathf.Sqrt(
                        Mathf.Pow(shieldRadiusX * sinHit, 2f) +
                        Mathf.Pow(shieldRadiusY * cosHit, 2f)
                    ) * 1.15f;
                }

                float maxHalfAngle = (contactArcHalfAngle > 5f ? contactArcHalfAngle : 35f) * Mathf.Deg2Rad;
                float halfAngle = Mathf.Clamp(halfPhysicalLength / Mathf.Max(0.5f, tanSpeed), 5f * Mathf.Deg2Rad, maxHalfAngle);

                for (int i = 0; i <= arcSegments; i++)
                {
                    float frac = i / (float)arcSegments;
                    float a = localAngle + Mathf.Lerp(-halfAngle, halfAngle, frac);
                    float ca = Mathf.Cos(a);
                    float sa = Mathf.Sin(a);
                    float ax, ay;
                    if (shieldShape == ShieldHitboxShape.Circle)
                    {
                        ax = ca * shieldRadius;
                        ay = sa * shieldRadius;
                    }
                    else if (shieldShape == ShieldHitboxShape.HullOutline)
                    {
                        ax = ca * shieldRadius;
                        ay = sa * shieldRadius;
                    }
                    else
                    {
                        ax = Mathf.Sign(ca) * Mathf.Pow(Mathf.Abs(ca), 2.0f / shieldPExp) * shieldRadiusX;
                        ay = Mathf.Sign(sa) * Mathf.Pow(Mathf.Abs(sa), 2.0f / shieldPExp) * shieldRadiusY;
                    }
                    cachedArcPositions[i] = shieldCenter + new Vector2(ax, ay);
                }

                int slot = currentArcIndex;
                currentArcIndex = (currentArcIndex + 1) % MAX_CONTACT_ARCS;

                LineRenderer targetLine = contactArcs[slot].line != null ? contactArcs[slot].line : shieldLine;
                if (targetLine != null)
                {
                    targetLine.loop = false;
                    targetLine.positionCount = arcSegments + 1;
                    targetLine.SetPositions(cachedArcPositions);

                    float peakWidth = contactArcWidth > 0.01f ? contactArcWidth : (contourLineWidth + 0.035f);
                    targetLine.widthCurve = GetContactWidthCurve();
                    targetLine.widthMultiplier = peakWidth;

                    ApplyContactArcGradient(targetLine, 0.95f);
                    targetLine.enabled = true;

                    contactArcs[slot].alpha = 0.95f;
                    hasActiveContactArc = true;
                }
            }

            currentDomeAlpha = 0.95f;

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
                        ripple.Initialize(currentShieldColor, 0.15f, 0.50f, 0.18f);
                    }
                }
            }

            // 4. Subtle audio feedback
            StarStrike.Gameplay.Audio.AudioService.Instance?.PlaySound("shield_hit", 0.25f, 0.15f);
        }

        private void Update()
        {
            if (hasActiveContactArc && contactArcs != null)
            {
                float maxAlpha = 0f;
                bool anyActive = false;
                for (int i = 0; i < contactArcs.Length; i++)
                {
                    if (contactArcs[i].alpha > 0f)
                    {
                        contactArcs[i].alpha = Mathf.MoveTowards(contactArcs[i].alpha, 0f, domeFadeSpeed * Time.deltaTime);
                        if (contactArcs[i].line != null)
                        {
                            if (contactArcs[i].alpha > 0f)
                            {
                                ApplyContactArcGradient(contactArcs[i].line, contactArcs[i].alpha);
                            }
                            else
                            {
                                contactArcs[i].line.enabled = false;
                            }
                        }
                    }
                    if (contactArcs[i].alpha > 0f) anyActive = true;
                    if (contactArcs[i].alpha > maxAlpha) maxAlpha = contactArcs[i].alpha;
                }
                currentDomeAlpha = maxAlpha;
                hasActiveContactArc = anyActive;
            }
            else if (currentDomeAlpha > 0f)
            {
                currentDomeAlpha = Mathf.MoveTowards(currentDomeAlpha, 0f, domeFadeSpeed * Time.deltaTime);
                if (currentDomeAlpha <= 0f && shieldLine != null)
                {
                    shieldLine.enabled = false;
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
