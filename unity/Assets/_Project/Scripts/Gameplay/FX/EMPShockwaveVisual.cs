using UnityEngine;
using StarStrike.Gameplay;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Expanding radial electromagnetic shockwave visual effect.
    /// Spawns a glowing neon-cyan shockwave ring and energy pulse that expands outward
    /// to visual indicate the EMP Nova radius.
    /// </summary>
    public class EMPShockwaveVisual : MonoBehaviour
    {
        private LineRenderer ringLine;
        private SpriteRenderer glowDisk;
        private float targetRadius = 9.5f;
        private float duration = 0.45f;
        private float elapsed = 0f;
        private const int SegmentCount = 48;
        private static Material sharedRingMaterial;

        public static void Spawn(Vector3 position, float radius = 9.5f, float expandDuration = 0.45f)
        {
            if (!Application.isPlaying) return;

            var go = new GameObject("EMP_Nova_Shockwave");
            go.transform.position = position;
            var visual = go.AddComponent<EMPShockwaveVisual>();
            visual.Initialize(radius, expandDuration);
        }

        public void Initialize(float radius, float expandDuration)
        {
            targetRadius = radius;
            duration = Mathf.Max(0.1f, expandDuration);
            elapsed = 0f;

            // 1. Setup glowing perimeter ring
            ringLine = gameObject.AddComponent<LineRenderer>();
            ringLine.useWorldSpace = false;
            ringLine.loop = true;
            ringLine.positionCount = SegmentCount;
            ringLine.startWidth = 0.28f;
            ringLine.endWidth = 0.28f;
            ringLine.sortingOrder = 9;

            if (sharedRingMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) sharedRingMaterial = new Material(shader);
            }
            if (sharedRingMaterial != null)
            {
                ringLine.sharedMaterial = sharedRingMaterial;
            }

            Color ringCol = new Color(0.35f, 0.95f, 1f, 0.95f);
            ringLine.startColor = ringCol;
            ringLine.endColor = ringCol;

            // Pre-calculate unit circle points (radius = 1.0f)
            float angleStep = 360f / SegmentCount;
            for (int i = 0; i < SegmentCount; i++)
            {
                float rad = i * angleStep * Mathf.Deg2Rad;
                ringLine.SetPosition(i, new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f));
            }

            // 2. Setup inner radiant energy pulse disk
            var diskObj = new GameObject("InnerGlow");
            diskObj.transform.SetParent(transform, false);
            glowDisk = diskObj.AddComponent<SpriteRenderer>();
            glowDisk.sprite = ProceduralSpriteHelper.GetCircleSprite(128);
            glowDisk.color = new Color(0.2f, 0.85f, 1f, 0.35f);
            glowDisk.sortingOrder = 8;

            // Set initial scale
            transform.localScale = Vector3.one * 0.2f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Explosive cubic ease-out for rapid initial burst that smoothly settles
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float currentRadius = Mathf.Lerp(0.3f, targetRadius, eased);

            transform.localScale = new Vector3(currentRadius, currentRadius, 1f);

            // Scale inner disk to follow inside the shockwave ring
            if (glowDisk != null)
            {
                float diskScale = (currentRadius / 0.64f); // normalized to 128px circle sprite size
                glowDisk.transform.localScale = Vector3.one * diskScale * 0.1f;
            }

            // Fade out as it expands
            float alpha = 1f - (t * t);
            if (ringLine != null)
            {
                Color c = new Color(0.35f, 0.95f, 1f, alpha * 0.95f);
                ringLine.startColor = c;
                ringLine.endColor = c;
                ringLine.startWidth = Mathf.Lerp(0.32f, 0.08f, t);
                ringLine.endWidth = ringLine.startWidth;
            }

            if (glowDisk != null)
            {
                glowDisk.color = new Color(0.2f, 0.85f, 1f, alpha * 0.32f);
            }

            if (elapsed >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
