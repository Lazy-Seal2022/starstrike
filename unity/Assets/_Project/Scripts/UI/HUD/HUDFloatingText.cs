using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using StarStrike.Core;

namespace StarStrike.UI
{
    /// <summary>
    /// Displays floating combat damage numbers and resource pickup alerts.
    /// Uses an internal O(1) text object pool and an isolated nested Sub-Canvas
    /// to eliminate GC allocations and prevent dirtying the root HUD canvas.
    /// </summary>
    public class HUDFloatingText : MonoBehaviour
    {
        private static HUDFloatingText instance;
        public static HUDFloatingText Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<HUDFloatingText>();
                }
                return instance;
            }
        }

        [Header("Configuration")]
        public UIThemeConfig themeConfig;

        private class PooledText
        {
            public GameObject gameObject;
            public RectTransform rectTransform;
            public TextMeshProUGUI textMesh;
        }

        private readonly Stack<PooledText> pool = new Stack<PooledText>();
        private Canvas subCanvas;
        private GameObject textContainer;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (themeConfig == null) themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            if (themeConfig == null) themeConfig = ScriptableObject.CreateInstance<UIThemeConfig>();

            EnsureSubCanvas();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        private void OnEnable()
        {
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnDamageDealt += HandleDamageDealt;
            GameEvents.OnSkillCast += HandleSkillCast;
        }

        private void OnDisable()
        {
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnDamageDealt -= HandleDamageDealt;
            GameEvents.OnSkillCast -= HandleSkillCast;
        }

        private void EnsureSubCanvas()
        {
            if (subCanvas == null)
            {
                if (textContainer == null)
                {
                    textContainer = new GameObject("FloatingTextContainer", typeof(RectTransform));
                    textContainer.transform.SetParent(transform, false);
                    var containerRt = textContainer.GetComponent<RectTransform>();
                    containerRt.anchorMin = Vector2.zero;
                    containerRt.anchorMax = Vector2.one;
                    containerRt.offsetMin = Vector2.zero;
                    containerRt.offsetMax = Vector2.zero;
                }

                subCanvas = textContainer.GetComponent<Canvas>();
                if (subCanvas == null)
                {
                    subCanvas = textContainer.AddComponent<Canvas>();
                }

                Canvas parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null && parentCanvas != subCanvas)
                {
                    subCanvas.overrideSorting = true;
                    subCanvas.sortingLayerID = parentCanvas.sortingLayerID;
                    subCanvas.sortingOrder = parentCanvas.sortingOrder + 25;
                }

                var cg = textContainer.GetComponent<CanvasGroup>();
                if (cg == null) cg = textContainer.AddComponent<CanvasGroup>();
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }

        private PooledText GetOrCreateItem()
        {
            EnsureSubCanvas();

            while (pool.Count > 0)
            {
                var candidate = pool.Pop();
                if (candidate != null && candidate.gameObject != null)
                {
                    candidate.gameObject.SetActive(true);
                    return candidate;
                }
            }

            Transform parentTransform = textContainer != null ? textContainer.transform : transform;
            GameObject textObj = new GameObject("PooledNotice", typeof(RectTransform));
            textObj.transform.SetParent(parentTransform, false);

            var rt = textObj.GetComponent<RectTransform>();
            var tmp = UIHelper.CreateTextMeshPro(textObj.transform, "Text", "", 16, FontStyles.Normal, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            return new PooledText
            {
                gameObject = textObj,
                rectTransform = rt,
                textMesh = tmp
            };
        }

        private void RecycleItem(PooledText item)
        {
            if (item != null && item.gameObject != null)
            {
                item.gameObject.SetActive(false);
                pool.Push(item);
            }
        }

        private void HandleSkillCast(SkillCastEvent evt)
        {
            if (evt.Slot == SkillSlotType.Passive1)
            {
                SpawnWorldNotice(evt.CastOrigin, "DEFLECTOR SURGE!", new Color(0.2f, 0.9f, 1f), 18f, true);
            }
            else if (evt.Slot == SkillSlotType.Active1)
            {
                string skillLabel = "SKILL!";
                if (evt.Definition != null && !string.IsNullOrEmpty(evt.Definition.displayName))
                {
                    skillLabel = evt.Definition.displayName.ToUpper() + "!";
                }
                SpawnWorldNotice(evt.CastOrigin, skillLabel, new Color(0.38f, 0.85f, 1f), 16f, false);
            }
        }

        private void HandleGemCollected(int amount, int total)
        {
            Color col = themeConfig != null ? themeConfig.energyColor : Color.yellow;
            SpawnFloatingNotice($"+{amount} GEMS", col);
        }

        private void HandleDamageDealt(Vector3 worldPos, float damage, bool isCrit)
        {
            SpawnWorldDamage(worldPos, damage, isCrit);
        }

        public void SpawnWorldDamage(Vector3 worldPos, float damage, bool isCrit = false)
        {
            if (!isActiveAndEnabled) return;

            Color col = isCrit
                ? new Color(1f, 0.85f, 0.2f, 1f) // Bright gold for crits
                : (damage >= 20f ? new Color(1f, 0.45f, 0.25f, 1f) : Color.white);

            string label = isCrit ? $"CRIT! {Mathf.RoundToInt(damage)}" : $"{Mathf.RoundToInt(damage)}";
            float fontSize = isCrit ? 22f : 16f;
            SpawnWorldNotice(worldPos, label, col, fontSize, isCrit);
        }

        public void SpawnWorldNotice(Vector3 worldPos, string message, Color color, float fontSize = 16f, bool isCrit = false)
        {
            if (!isActiveAndEnabled) return;

            Vector2 screenPos = Vector2.zero;
            if (Camera.main != null)
            {
                screenPos = Camera.main.WorldToScreenPoint(worldPos);
            }

            var item = GetOrCreateItem();
            item.rectTransform.position = screenPos;
            item.rectTransform.sizeDelta = new Vector2(200f, 32f);
            item.rectTransform.localScale = Vector3.one;

            if (item.textMesh != null)
            {
                item.textMesh.text = message;
                item.textMesh.fontSize = Mathf.RoundToInt(fontSize);
                item.textMesh.fontStyle = isCrit ? FontStyles.Bold : FontStyles.Normal;
                item.textMesh.color = color;
            }

            StartCoroutine(AnimateWorldNotice(item, isCrit));
        }

        public void SpawnFloatingNotice(string message, Color color)
        {
            if (!isActiveAndEnabled) return;

            var item = GetOrCreateItem();
            item.rectTransform.sizeDelta = new Vector2(250f, 36f);
            item.rectTransform.anchoredPosition = new Vector2(0f, 75f);
            item.rectTransform.localScale = Vector3.one;

            if (item.textMesh != null)
            {
                item.textMesh.text = message;
                item.textMesh.fontSize = 18;
                item.textMesh.fontStyle = FontStyles.Bold;
                item.textMesh.color = color;
            }

            StartCoroutine(AnimateNotice(item));
        }

        private IEnumerator AnimateWorldNotice(PooledText item, bool isCrit)
        {
            float duration = isCrit ? 0.85f : 0.6f;
            float elapsed = 0f;
            Vector3 startPos = item.rectTransform.position;
            Vector3 endPos = startPos + new Vector3(Random.Range(-15f, 15f), 45f, 0f);
            Color initialColor = item.textMesh != null ? item.textMesh.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (item.rectTransform != null)
                {
                    item.rectTransform.position = Vector3.Lerp(startPos, endPos, Ease.OutCubic(t));
                    float scale = t < 0.25f
                        ? Mathf.Lerp(1.0f, isCrit ? 1.4f : 1.25f, t / 0.25f)
                        : Mathf.Lerp(isCrit ? 1.4f : 1.25f, 1.0f, (t - 0.25f) / 0.75f);
                    item.rectTransform.localScale = Vector3.one * scale;
                }
                if (item.textMesh != null)
                {
                    float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                    item.textMesh.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                }
                yield return null;
            }

            RecycleItem(item);
        }

        private IEnumerator AnimateNotice(PooledText item)
        {
            float elapsed = 0f;
            float duration = 0.8f;
            Vector2 startPos = item.rectTransform.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, 40f);
            Color initialColor = item.textMesh != null ? item.textMesh.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (item.rectTransform != null)
                {
                    item.rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, Ease.OutCubic(t));
                }
                if (item.textMesh != null)
                {
                    item.textMesh.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f - t);
                }
                yield return null;
            }

            RecycleItem(item);
        }
    }
}
