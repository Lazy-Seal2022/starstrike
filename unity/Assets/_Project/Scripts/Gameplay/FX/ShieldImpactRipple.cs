using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Visual representation of an energetic hexagonal shield impact ripple.
    /// Expands rapidly and dissipates at the contact point on the ship's shield barrier.
    /// Managed via PoolManager for zero runtime memory allocations.
    /// </summary>
    public class ShieldImpactRipple : MonoBehaviour, IPoolable
    {
        private SpriteRenderer spriteRenderer;
        private float duration = 0.22f;
        private float elapsed = 0f;
        private float startScale = 0.4f;
        private float endScale = 1.25f;
        private Color baseColor = new Color(0.25f, 0.95f, 1f, 1f);

        private static Material cachedRippleMat;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = ProceduralSpriteHelper.GetShieldRippleSprite(64);
            }
            spriteRenderer.sortingOrder = 14; // Above hull (layer 10)

            if (cachedRippleMat == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) cachedRippleMat = new Material(shader);
            }
            if (cachedRippleMat != null)
            {
                spriteRenderer.sharedMaterial = cachedRippleMat;
            }
        }

        public void Initialize(Color color, float startRadius = 0.15f, float endRadius = 0.45f, float animDuration = 0.16f)
        {
            EnsureInitialized();
            baseColor = color;
            startScale = startRadius;
            endScale = endRadius;
            duration = Mathf.Max(0.05f, animDuration);
            elapsed = 0f;

            transform.localScale = Vector3.one * startScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Explosive cubic ease-out: expands very quickly, then settles
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float currentScale = Mathf.Lerp(startScale, endScale, eased);
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // Quadratic alpha decay
            float alpha = (1f - t) * (1f - t);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * alpha);
            }

            if (elapsed >= duration)
            {
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Despawn(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void OnSpawned()
        {
            elapsed = 0f;
            transform.localScale = Vector3.one * startScale;
        }

        public void OnDespawned()
        {
            elapsed = 0f;
            transform.localScale = Vector3.one * startScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.clear;
            }
        }
    }
}
