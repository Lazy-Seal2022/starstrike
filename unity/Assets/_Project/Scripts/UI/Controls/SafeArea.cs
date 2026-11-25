using UnityEngine;

namespace StarStrike.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);
        private int lastScreenWidth;
        private int lastScreenHeight;
        private float nextPollTime;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void Update()
        {
            bool resized = Screen.width != lastScreenWidth || Screen.height != lastScreenHeight;
            if (!resized && Time.unscaledTime < nextPollTime) return;

            nextPollTime = Time.unscaledTime + 0.25f;
            if (resized || Screen.safeArea != lastSafeArea)
            {
                ApplySafeArea();
            }
        }

        void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
