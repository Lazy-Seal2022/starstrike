using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Handcrafted multi-frame animated sprite explosion replacing generic particle bursts.
    /// Uses 8 rendered frames of punchy arcade explosion with expanding shockwave lobes and ember decay.
    /// Fully pooled via PoolManager for zero runtime GC allocations.
    /// </summary>
    public class AnimatedSpriteExplosion : MonoBehaviour, IPoolable
    {
        private SpriteRenderer spriteRenderer;
        private float duration = 0.35f;
        private float elapsed = 0f;
        private Vector3 baseScale = Vector3.one;

        private static Material cachedMat;
        private static Sprite[] explosionFrames;

        public static Sprite[] ExplosionFrames => explosionFrames;

        public static void EnsureFramesLoaded()
        {
            if (explosionFrames == null || explosionFrames.Length == 0)
            {
                explosionFrames = new Sprite[8];
                bool allFound = true;
                for (int i = 0; i < 8; i++)
                {
#if UNITY_EDITOR
                    var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/VFX/Explosion/Explosion_frame_{i}.png");
                    if (sp != null) explosionFrames[i] = sp;
                    else allFound = false;
#endif
                }
                if (!allFound)
                {
                    // Procedural fallback circles of decreasing size and shifting color
                    for (int i = 0; i < 8; i++)
                    {
                        if (explosionFrames[i] == null)
                        {
                            explosionFrames[i] = ProceduralSpriteHelper.GenerateLaserSprite(false);
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            EnsureFramesLoaded();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null && explosionFrames != null && explosionFrames.Length > 0)
            {
                spriteRenderer.sprite = explosionFrames[0];
            }
            spriteRenderer.sortingOrder = 15; // Above hull (10) and shield dome (14)

            if (cachedMat == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) cachedMat = new Material(shader);
            }
            if (cachedMat != null)
            {
                spriteRenderer.sharedMaterial = cachedMat;
            }
        }

        public void Initialize(float scale = 1.2f, float animDuration = 0.35f)
        {
            EnsureInitialized();
            duration = Mathf.Max(0.08f, animDuration);
            elapsed = 0f;
            baseScale = new Vector3(scale, scale, 1f);
            transform.localScale = baseScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                if (explosionFrames != null && explosionFrames.Length > 0 && explosionFrames[0] != null)
                {
                    spriteRenderer.sprite = explosionFrames[0];
                }
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (explosionFrames != null && explosionFrames.Length > 0)
            {
                int frameIdx = Mathf.Clamp(Mathf.FloorToInt(t * explosionFrames.Length), 0, explosionFrames.Length - 1);
                if (spriteRenderer != null && explosionFrames[frameIdx] != null)
                {
                    spriteRenderer.sprite = explosionFrames[frameIdx];
                }
            }

            // Punchy scale pop: expands quickly, then subtly settles
            float scaleMul = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
            transform.localScale = baseScale * scaleMul;

            // Fade out in last 25% of life
            if (t > 0.75f && spriteRenderer != null)
            {
                float fade = (1f - t) / 0.25f;
                spriteRenderer.color = new Color(1f, 1f, 1f, fade);
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
            transform.localScale = baseScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                if (explosionFrames != null && explosionFrames.Length > 0 && explosionFrames[0] != null)
                {
                    spriteRenderer.sprite = explosionFrames[0];
                }
            }
        }

        public void OnDespawned()
        {
            elapsed = 0f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.clear;
            }
        }

        public static GameObject CreatePrefab()
        {
            var go = new GameObject("AnimatedExplosionFX");
            var anim = go.AddComponent<AnimatedSpriteExplosion>();
            anim.EnsureInitialized();
            return go;
        }
    }
}
