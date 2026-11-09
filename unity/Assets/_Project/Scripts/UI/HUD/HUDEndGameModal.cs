using UnityEngine;
using UnityEngine.UI;
using StarStrike.Core;
using StarStrike.Gameplay;
using StarStrike.Platform.Storage;

namespace StarStrike.UI
{
    public class HUDEndGameModal : MonoBehaviour
    {
        [Header("Configuration")]
        public UIThemeConfig themeConfig;

        [Header("UI Roots")]
        public GameObject modalRoot;
        public Text titleText;
        public Text statsSummaryText;
        public Text scoreText;
        public Button playAgainButton;
        public Button returnHangarButton;

        public bool IsVisible => modalRoot != null && modalRoot.activeSelf;

        private void Awake()
        {
            if (themeConfig == null)
            {
                themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            }
            if (modalRoot == null)
            {
                BuildDefaultModal();
            }
            SetVisible(false);
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnVictory += HandleVictory;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnVictory -= HandleVictory;
        }

        private void HandleGameOver(RunSummary summary)
        {
            Show(summary);
        }

        private void HandleVictory(RunSummary summary)
        {
            Show(summary);
        }

        public void Show(RunSummary summary)
        {
            if (modalRoot == null)
            {
                BuildDefaultModal();
            }

            int mins = (int)(summary.timeSurvived / 60f);
            int secs = (int)(summary.timeSurvived % 60f);
            string timeFormatted = $"{mins:00}:{secs:00}";

            bool isHigh = summary.finalScore >= StorageService.GetHighScore() && summary.finalScore > 0;

            if (titleText != null)
            {
                if (summary.isVictory)
                {
                    titleText.text = "★ VICTORY ACHIEVED ★";
                    titleText.color = themeConfig != null ? themeConfig.textHighlight : Color.cyan;
                }
                else
                {
                    titleText.text = "✖ VESSEL DESTROYED ✖";
                    titleText.color = themeConfig != null ? themeConfig.dangerColor : Color.red;
                }
            }

            if (statsSummaryText != null)
            {
                statsSummaryText.text = $"TIME SURVIVED: <color=#FFFFFF><b>{timeFormatted}</b></color>\n" +
                                        $"GEMS EXTRACTED: <color=#4ADE80><b>{summary.gemsCollected} ◈</b></color>\n" +
                                        $"ASTEROIDS MINED: <color=#94A3B8><b>{summary.asteroidsMined}</b></color>\n" +
                                        $"HOSTILES PURGED: <color=#F87171><b>{summary.enemiesDestroyed}</b></color>\n" +
                                        $"FLAGSHIP TIER: <color=#38BDF8><b>{summary.finalShipId.ToUpper()}</b></color>";
            }

            if (scoreText != null)
            {
                string hiBadge = isHigh ? " <color=#FBBF24>[NEW BEST!]</color>" : "";
                scoreText.text = $"FINAL SCORE: {summary.finalScore:N0} PTS{hiBadge}";
            }

            SetVisible(true);
        }

        public void SetVisible(bool visible)
        {
            if (modalRoot == null)
            {
                BuildDefaultModal();
            }
            if (modalRoot != null)
            {
                modalRoot.SetActive(visible);
            }
        }

        public void OnClickPlayAgain()
        {
            SetVisible(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        public void OnClickReturnToHangar()
        {
            SetVisible(false);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Menu);
            }
        }

        private void BuildDefaultModal()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            modalRoot = new GameObject("EndGameModalPanel", typeof(RectTransform), typeof(Image));
            modalRoot.transform.SetParent(transform, false);

            var rt = modalRoot.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(520f, 420f);

            var bg = modalRoot.GetComponent<Image>();
            bg.color = themeConfig != null ? themeConfig.panelBackground : new Color(0.05f, 0.08f, 0.14f, 0.94f);

            // Title
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(modalRoot.transform, false);
            var titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -24f);
            titleRt.sizeDelta = new Vector2(0f, 44f);

            titleText = titleObj.GetComponent<Text>();
            titleText.font = defaultFont;
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "MISSION OVER";

            // Stats
            GameObject statsObj = new GameObject("StatsText", typeof(RectTransform), typeof(Text));
            statsObj.transform.SetParent(modalRoot.transform, false);
            var statsRt = statsObj.GetComponent<RectTransform>();
            statsRt.anchorMin = new Vector2(0f, 0.35f);
            statsRt.anchorMax = new Vector2(1f, 0.85f);
            statsRt.offsetMin = new Vector2(36f, 0f);
            statsRt.offsetMax = new Vector2(-36f, 0f);

            statsSummaryText = statsObj.GetComponent<Text>();
            statsSummaryText.font = defaultFont;
            statsSummaryText.fontSize = 15;
            statsSummaryText.lineSpacing = 1.35f;
            statsSummaryText.alignment = TextAnchor.MiddleLeft;
            statsSummaryText.color = themeConfig != null ? themeConfig.textMuted : Color.gray;
            statsSummaryText.supportRichText = true;

            // Score
            GameObject scoreObj = new GameObject("ScoreText", typeof(RectTransform), typeof(Text));
            scoreObj.transform.SetParent(modalRoot.transform, false);
            var scoreRt = scoreObj.GetComponent<RectTransform>();
            scoreRt.anchorMin = new Vector2(0f, 0.22f);
            scoreRt.anchorMax = new Vector2(1f, 0.35f);
            scoreRt.offsetMin = new Vector2(36f, 0f);
            scoreRt.offsetMax = new Vector2(-36f, 0f);

            scoreText = scoreObj.GetComponent<Text>();
            scoreText.font = defaultFont;
            scoreText.fontSize = 17;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = themeConfig != null ? themeConfig.textPrimary : Color.white;
            scoreText.supportRichText = true;

            // Buttons Container
            GameObject btnContainer = new GameObject("Buttons", typeof(RectTransform));
            btnContainer.transform.SetParent(modalRoot.transform, false);
            var btnContainerRt = btnContainer.GetComponent<RectTransform>();
            btnContainerRt.anchorMin = new Vector2(0f, 0f);
            btnContainerRt.anchorMax = new Vector2(1f, 0.22f);
            btnContainerRt.offsetMin = new Vector2(24f, 16f);
            btnContainerRt.offsetMax = new Vector2(-24f, 0f);

            playAgainButton = CreateButton(btnContainer.transform, "BtnPlayAgain", "▶ PLAY AGAIN", new Vector2(0f, 0f), new Vector2(0.48f, 1f));
            playAgainButton.onClick.AddListener(OnClickPlayAgain);

            returnHangarButton = CreateButton(btnContainer.transform, "BtnHangar", "⌂ HANGAR", new Vector2(0.52f, 0f), new Vector2(1f, 1f));
            returnHangarButton.onClick.AddListener(OnClickReturnToHangar);
        }

        private Button CreateButton(Transform parent, string name, string labelText, Vector2 anchorMin, Vector2 anchorMax)
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = btnObj.GetComponent<Image>();
            img.color = themeConfig != null ? themeConfig.buttonNormal : new Color(0.12f, 0.22f, 0.35f, 1f);

            var btn = btnObj.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = themeConfig != null ? themeConfig.buttonHover : new Color(0.2f, 0.4f, 0.6f, 1f);
            colors.pressedColor = themeConfig != null ? themeConfig.buttonPressed : new Color(0.1f, 0.5f, 0.8f, 1f);
            btn.colors = colors;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(btnObj.transform, false);
            var textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var t = textObj.GetComponent<Text>();
            t.font = defaultFont;
            t.fontSize = 14;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.text = labelText;
            t.color = Color.white;

            return btn;
        }
    }
}
