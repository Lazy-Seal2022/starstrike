using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarStrike.Core;

namespace StarStrike.UI
{
    public class AchievementUI : MonoBehaviour
    {
        private GameObject toastContainer;
        private Text titleText;
        private Text descText;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            BuildUI();
            GameEvents.OnAchievementUnlocked += ShowToast;
        }

        private void OnDestroy()
        {
            GameEvents.OnAchievementUnlocked -= ShowToast;
        }

        private void BuildUI()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 200; // Above HUD
                
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }

            toastContainer = new GameObject("ToastContainer", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            toastContainer.transform.SetParent(transform, false);
            
            RectTransform rt = toastContainer.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, 50); // Start off-screen (above)
            rt.sizeDelta = new Vector2(400, 100);

            Image img = toastContainer.GetComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.2f, 0.9f);

            canvasGroup = toastContainer.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(toastContainer.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0, 0.5f);
            titleRt.anchorMax = new Vector2(1, 1);
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(-20, -10);
            titleText = titleObj.GetComponent<Text>();
            titleText.text = "ACHIEVEMENT UNLOCKED";
            titleText.font = font;
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.yellow;
            titleText.alignment = TextAnchor.MiddleCenter;

            GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(Text));
            descObj.transform.SetParent(toastContainer.transform, false);
            RectTransform descRt = descObj.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0, 0);
            descRt.anchorMax = new Vector2(1, 0.5f);
            descRt.offsetMin = new Vector2(20, 10);
            descRt.offsetMax = new Vector2(-20, 0);
            descText = descObj.GetComponent<Text>();
            descText.text = "";
            descText.font = font;
            descText.fontSize = 18;
            descText.color = Color.white;
            descText.alignment = TextAnchor.MiddleCenter;
        }

        private void ShowToast(string id)
        {
            StopAllCoroutines();
            StartCoroutine(ToastRoutine(id));
        }

        private IEnumerator ToastRoutine(string id)
        {
            descText.text = id.Replace("_", " ");
            
            RectTransform rt = toastContainer.GetComponent<RectTransform>();
            float elapsed = 0f;
            float duration = 0.5f;

            // Slide in
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * (2f - t); // ease out
                rt.anchoredPosition = new Vector2(0, Mathf.Lerp(50, -50, t));
                canvasGroup.alpha = t;
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            // Slide out
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t; // ease in
                rt.anchoredPosition = new Vector2(0, Mathf.Lerp(-50, 50, t));
                canvasGroup.alpha = 1f - t;
                yield return null;
            }

            rt.anchoredPosition = new Vector2(0, 50);
            canvasGroup.alpha = 0f;
        }
    }
}
