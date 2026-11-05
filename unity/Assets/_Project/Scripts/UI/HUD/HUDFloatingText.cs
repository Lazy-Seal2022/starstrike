using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;

namespace StarStrike.UI
{
    public class HUDFloatingText : MonoBehaviour
    {
        public static HUDFloatingText Instance { get; private set; }

        [Header("Configuration")]
        public UIThemeConfig themeConfig;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (themeConfig == null) themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            if (themeConfig == null) themeConfig = ScriptableObject.CreateInstance<UIThemeConfig>();
        }

        private void OnEnable()
        {
            GameEvents.OnGemCollected += HandleGemCollected;
        }

        private void OnDisable()
        {
            GameEvents.OnGemCollected -= HandleGemCollected;
        }

        private void HandleGemCollected(int amount, int total)
        {
            // Spawn float notification at screen center
            SpawnFloatingNotice($"+{amount} ◈", themeConfig != null ? themeConfig.energyColor : Color.yellow);
        }

        public void SpawnFloatingNotice(string message, Color color)
        {
            GameObject textObj = new GameObject("FloatingNotice");
            textObj.transform.SetParent(transform, false);

            var rt = textObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200f, 30f);
            rt.anchoredPosition = new Vector2(0f, 60f);

            var txt = textObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = message;
            txt.fontSize = 18;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = color;

            StartCoroutine(AnimateNotice(rt, txt));
        }

        private System.Collections.IEnumerator AnimateNotice(RectTransform rt, Text txt)
        {
            float elapsed = 0f;
            float duration = 0.8f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, 40f);
            Color initialColor = txt.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (rt != null)
                {
                    rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                }
                if (txt != null)
                {
                    txt.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f - t);
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
