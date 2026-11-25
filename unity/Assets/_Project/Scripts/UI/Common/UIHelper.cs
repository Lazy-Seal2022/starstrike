using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StarStrike.UI
{
    public static class UIHelper
    {
        public const float DefaultTextScale = 1.18f; // ~18% enlargement for optimal readability across all UI
        public static float TextSizeScale = DefaultTextScale;

        public static int ScaleFontSize(int baseFontSize)
        {
            if (baseFontSize <= 0) return baseFontSize;
            int scaled = Mathf.RoundToInt(baseFontSize * TextSizeScale);
            return Mathf.Max(scaled, baseFontSize + 1);
        }

        public static float ScaleFontSize(float baseFontSize)
        {
            if (baseFontSize <= 0f) return baseFontSize;
            float scaled = baseFontSize * TextSizeScale;
            return Mathf.Max(scaled, baseFontSize + 1f);
        }

        public static TextMeshProUGUI CreateTextMeshPro(Transform parent, string name, string text, int fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool autoScale = true)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = autoScale ? ScaleFontSize(fontSize) : fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false; // Labels do not block clicks

            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null) tmp.font = defaultFont;

            return tmp;
        }

        public static TextMeshProUGUI CreateTextMeshProExplicit(Transform parent, string name, string text, int fontSize, FontStyles style, Color color, TextAlignmentOptions alignment, Vector2 anchor, Vector2 pos, Vector2 size, bool autoScale = false)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = autoScale ? ScaleFontSize(fontSize) : fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;

            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null) tmp.font = defaultFont;

            return tmp;
        }

        public static TextAlignmentOptions ConvertTextAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

        public static GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font = null)
        {
            FontStyles style = (fontStyle == FontStyle.Bold) ? FontStyles.Bold : FontStyles.Normal;
            var tmp = CreateTextMeshPro(parent, name, text, fontSize, style, color, ConvertTextAnchor(alignment), anchorMin, anchorMax, offsetMin, offsetMax);
            return tmp.gameObject;
        }

        public static GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Font font, int fontSize = 16, bool isPrimary = false)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = btnObj.GetComponent<Image>();
            img.sprite = CyberVectorTheme.GetButtonSprite();
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                StarStrike.Gameplay.Audio.AudioService.Instance?.PlayUISound("click");
            });
            CyberVectorTheme.ApplyVectorButtonStyle(btn, bgColor, isPrimary);

            CreateText(btnObj.transform, "Text", label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, font);
            return btnObj;
        }
    }
}
