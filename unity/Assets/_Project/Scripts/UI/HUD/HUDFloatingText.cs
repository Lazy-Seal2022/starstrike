using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarStrike.Core;

namespace StarStrike.UI
{
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

        private void HandleSkillCast(SkillCastEvent evt)
        {
            if (evt.Slot == SkillSlotType.Passive1)
            {
                SpawnWorldNotice(evt.CastOrigin, "DEFLECTOR SURGE!", new Color(0.2f, 0.9f, 1f), 18f, true);
            }
            else if (evt.Slot == SkillSlotType.Active1)
            {
                SpawnWorldNotice(evt.CastOrigin, "SHOCKWAVE!", new Color(0.38f, 0.85f, 1f), 16f, false);
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

            GameObject textObj = new GameObject("WorldNotice", typeof(RectTransform));
            textObj.transform.SetParent(transform, false);

            var rt = textObj.GetComponent<RectTransform>();
            rt.position = screenPos;
            rt.sizeDelta = new Vector2(200f, 32f);

            var tmp = UIHelper.CreateTextMeshPro(textObj.transform, "Text", message, Mathf.RoundToInt(fontSize), isCrit ? FontStyles.Bold : FontStyles.Normal, color, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            StartCoroutine(AnimateWorldNotice(rt, tmp, isCrit));
        }

        public void SpawnFloatingNotice(string message, Color color)
        {
            if (!isActiveAndEnabled) return;

            GameObject textObj = new GameObject("FloatingNotice", typeof(RectTransform));
            textObj.transform.SetParent(transform, false);

            var rt = textObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(250f, 36f);
            rt.anchoredPosition = new Vector2(0f, 75f);

            var tmp = UIHelper.CreateTextMeshPro(textObj.transform, "Text", message, 18, FontStyles.Bold, color, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            StartCoroutine(AnimateNotice(rt, tmp));
        }

        private IEnumerator AnimateWorldNotice(RectTransform rt, TextMeshProUGUI tmp, bool isCrit)
        {
            float duration = isCrit ? 0.85f : 0.6f;
            float elapsed = 0f;
            Vector3 startPos = rt.position;
            Vector3 endPos = startPos + new Vector3(Random.Range(-15f, 15f), 45f, 0f);
            Color initialColor = tmp != null ? tmp.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (rt != null)
                {
                    rt.position = Vector3.Lerp(startPos, endPos, Ease.OutCubic(t));
                    float scale = t < 0.25f
                        ? Mathf.Lerp(1.0f, isCrit ? 1.4f : 1.25f, t / 0.25f)
                        : Mathf.Lerp(isCrit ? 1.4f : 1.25f, 1.0f, (t - 0.25f) / 0.75f);
                    rt.localScale = Vector3.one * scale;
                }
                if (tmp != null)
                {
                    float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                    tmp.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                }
                yield return null;
            }

            if (rt != null && rt.gameObject != null)
            {
                Destroy(rt.gameObject);
            }
        }

        private IEnumerator AnimateNotice(RectTransform rt, TextMeshProUGUI tmp)
        {
            float elapsed = 0f;
            float duration = 0.8f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, 40f);
            Color initialColor = tmp != null ? tmp.color : Color.white;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Ease.OutCubic(t));
                }
                if (tmp != null)
                {
                    tmp.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f - t);
                }
                yield return null;
            }

            if (rt != null && rt.gameObject != null)
            {
                Destroy(rt.gameObject);
            }
        }
    }
}
