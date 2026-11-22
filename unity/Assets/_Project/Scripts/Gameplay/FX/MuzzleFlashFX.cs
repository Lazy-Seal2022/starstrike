using UnityEngine;
using StarStrike.Core;

namespace StarStrike.Gameplay.FX
{
    /// <summary>
    /// Snappy 4-frame additive plasma burst played at weapon muzzle sockets upon firing.
    /// Uses 4 rendered frames from Blender with rapid 0.08s playback and auto-pooling.
    /// </summary>
    public class MuzzleFlashFX : MonoBehaviour, IPoolable
    {
        private SpriteRenderer spriteRenderer;
        private float duration = 0.08f;
        private float elapsed = 0f;
        private Vector3 baseScale = Vector3.one;

        private static Material cachedMat;
        private static Sprite[] flashFrames;

        public static Sprite[] FlashFrames => flashFrames;

        public static void EnsureFramesLoaded()
        {
            if (flashFrames == null || flashFrames.Length == 0)
            {
                flashFrames = new Sprite[4];
                bool allFound = true;
                for (int i = 0; i < 4; i++)
                {
#if UNITY_EDITOR
                    var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/_Project/Art/Sprites/VFX/MuzzleFlash/MuzzleFlash_frame_{i}.png");
                    if (sp != null) flashFrames[i] = sp;
                    else allFound = false;
#endif
                }
                if (!allFound)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (flashFrames[i] == null)
                        {
                            flashFrames[i] = ProceduralSpriteHelper.GenerateLaserSprite(false);
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

            if (spriteRenderer.sprite == null && flashFrames != null && flashFrames.Length > 0)
            {
                spriteRenderer.sprite = flashFrames[0];
            }
            spriteRenderer.sortingOrder = 16; // Above ship and lasers

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

        public void Initialize(Color tint, float scale = 0.6f, float animDuration = 0.08f)
        {
            EnsureInitialized();
            duration = Mathf.Max(0.04f, animDuration);
            elapsed = 0f;
            baseScale = new Vector3(scale, scale, 1f);
            transform.localScale = baseScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = tint;
                if (flashFrames != null && flashFrames.Length > 0 && flashFrames[0] != null)
                {
                    spriteRenderer.sprite = flashFrames[0];
                }
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (flashFrames != null && flashFrames.Length > 0)
            {
                int frameIdx = Mathf.Clamp(Mathf.FloorToInt(t * flashFrames.Length), 0, flashFrames.Length - 1);
                if (spriteRenderer != null && flashFrames[frameIdx] != null)
                {
                    spriteRenderer.sprite = flashFrames[frameIdx];
                }
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
                if (flashFrames != null && flashFrames.Length > 0 && flashFrames[0] != null)
                {
                    spriteRenderer.sprite = flashFrames[0];
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
            var go = new GameObject("MuzzleFlashFX");
            var flash = go.AddComponent<MuzzleFlashFX>();
            flash.EnsureInitialized();
            return go;
        }
    }
}
