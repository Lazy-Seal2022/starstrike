using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.Network
{
    public class ArenaScoreboardUI : MonoBehaviour
    {
        public static ArenaScoreboardUI Instance { get; private set; }

        private GameObject scoreboardContainer;
        private Transform rowsParent;
        private Text killfeedText;
        private GameObject victoryModal;
        private Text victoryWinnerText;
        private readonly List<string> recentKills = new List<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildUI();
            SetVisible(false);
        }

        private void OnEnable()
        {
            NetworkArenaManager.OnKillFeed += HandleKillFeed;
            NetworkArenaManager.OnMatchOver += HandleMatchOver;
            NetworkArenaManager.OnScoreboardUpdated += RefreshLeaderboard;
        }

        private void OnDisable()
        {
            NetworkArenaManager.OnKillFeed -= HandleKillFeed;
            NetworkArenaManager.OnMatchOver -= HandleMatchOver;
            NetworkArenaManager.OnScoreboardUpdated -= RefreshLeaderboard;
        }

        public void SetVisible(bool visible)
        {
            if (scoreboardContainer != null)
            {
                scoreboardContainer.SetActive(visible);
                if (visible)
                {
                    if (victoryModal != null) victoryModal.SetActive(false);
                    RefreshLeaderboard();
                }
            }
        }

        private void Update()
        {
            // Toggle scoreboard details with Tab key
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (scoreboardContainer != null && GameManager.Instance != null && GameManager.Instance.CurrentMode == GameMode.PvPArena)
                {
                    scoreboardContainer.SetActive(!scoreboardContainer.activeSelf);
                    if (scoreboardContainer.activeSelf) RefreshLeaderboard();
                }
            }
        }

        private void HandleKillFeed(string killer, string victim)
        {
            string line = $"<color=#00e5ff>[{killer}]</color> blasted <color=#ff5252>[{victim}]</color>";
            recentKills.Insert(0, line);
            if (recentKills.Count > 4) recentKills.RemoveAt(recentKills.Count - 1);

            if (killfeedText != null)
            {
                killfeedText.text = string.Join("\n", recentKills);
            }
        }

        private void HandleMatchOver(string winner)
        {
            if (victoryModal != null)
            {
                victoryModal.SetActive(true);
                if (victoryWinnerText != null)
                {
                    victoryWinnerText.text = $"ARENA CHAMPION\n<color=#ffcc00>{winner}</color>\nREACHED 10 KILLS!";
                }
            }
        }

        public void RefreshLeaderboard()
        {
            if (rowsParent == null || NetworkArenaManager.Instance == null || NetworkArenaManager.Instance.Scoreboard == null) return;

            // Clear rows
            foreach (Transform child in rowsParent)
            {
                Destroy(child.gameObject);
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Convert scoreboard to list and sort by kills/score descending
            List<ArenaScoreEntry> sorted = new List<ArenaScoreEntry>();
            for (int i = 0; i < NetworkArenaManager.Instance.Scoreboard.Count; i++)
            {
                sorted.Add(NetworkArenaManager.Instance.Scoreboard[i]);
            }
            sorted.Sort((a, b) => b.Score.CompareTo(a.Score));

            for (int i = 0; i < sorted.Count; i++)
            {
                var entry = sorted[i];
                string rowText = $"#{i + 1}  {entry.PlayerName,-12}  {entry.ShipId.ToString().ToUpper(),-10}  K: {entry.Kills,-3}  D: {entry.Deaths,-3}  Score: {entry.Score}";
                CreateText(rowsParent, $"Row_{i}", rowText, 14, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(1, 1), new Vector2(10, -(i * 24 + 20)), new Vector2(-10, -(i * 24)), defaultFont);
            }
        }

        private void BuildUI()
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            scoreboardContainer = new GameObject("ArenaScoreboardContainer", typeof(RectTransform));
            scoreboardContainer.transform.SetParent(transform, false);
            RectTransform rt = scoreboardContainer.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 1. Killfeed (Top Left)
            GameObject kfObj = CreateText(scoreboardContainer.transform, "KillfeedText", "", 16, FontStyle.Bold, Color.white, TextAnchor.UpperLeft, new Vector2(0.02f, 0.75f), new Vector2(0.40f, 0.98f), Vector2.zero, Vector2.zero, defaultFont);
            killfeedText = kfObj.GetComponent<Text>();

            // 2. Leaderboard Card (Top Right)
            GameObject boardCard = new GameObject("LeaderboardCard", typeof(RectTransform), typeof(Image));
            boardCard.transform.SetParent(scoreboardContainer.transform, false);
            RectTransform bcRt = boardCard.GetComponent<RectTransform>();
            bcRt.anchorMin = new Vector2(0.60f, 0.65f);
            bcRt.anchorMax = new Vector2(0.98f, 0.98f);
            bcRt.offsetMin = Vector2.zero;
            bcRt.offsetMax = Vector2.zero;
            boardCard.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.16f, 0.85f);

            // Leaderboard Header
            CreateText(boardCard.transform, "Header", "PVP ARENA LEADERBOARD [TAB]", 14, FontStyle.Bold, Color.yellow, TextAnchor.MiddleCenter, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -28), Vector2.zero, defaultFont);

            // Rows Container
            GameObject rowsObj = new GameObject("RowsContainer", typeof(RectTransform));
            rowsObj.transform.SetParent(boardCard.transform, false);
            RectTransform rcRt = rowsObj.GetComponent<RectTransform>();
            rcRt.anchorMin = Vector2.zero;
            rcRt.anchorMax = new Vector2(1, 1);
            rcRt.offsetMin = new Vector2(5, 5);
            rcRt.offsetMax = new Vector2(-5, -30);
            rowsParent = rowsObj.transform;

            // 3. Victory Modal (Center)
            BuildVictoryModal(defaultFont);
        }

        private void BuildVictoryModal(Font font)
        {
            victoryModal = new GameObject("ArenaVictoryModal", typeof(RectTransform), typeof(Image));
            victoryModal.transform.SetParent(scoreboardContainer.transform, false);
            RectTransform vmRt = victoryModal.GetComponent<RectTransform>();
            vmRt.anchorMin = new Vector2(0.5f, 0.5f);
            vmRt.anchorMax = new Vector2(0.5f, 0.5f);
            vmRt.anchoredPosition = Vector2.zero;
            vmRt.sizeDelta = new Vector2(480, 320);
            victoryModal.GetComponent<Image>().color = new Color(0.04f, 0.07f, 0.14f, 0.96f);

            // Title & Winner Text
            GameObject winObj = CreateText(victoryModal.transform, "WinnerText", "ARENA CHAMPION\nWINNER!\nREACHED 10 KILLS!", 24, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.65f), new Vector2(-220, -60), new Vector2(220, 60), font);
            victoryWinnerText = winObj.GetComponent<Text>();

            // Return to Menu Button
            GameObject retBtnObj = new GameObject("ReturnBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            retBtnObj.transform.SetParent(victoryModal.transform, false);
            RectTransform rbRt = retBtnObj.GetComponent<RectTransform>();
            rbRt.anchorMin = new Vector2(0.5f, 0.20f);
            rbRt.anchorMax = new Vector2(0.5f, 0.20f);
            rbRt.anchoredPosition = Vector2.zero;
            rbRt.sizeDelta = new Vector2(260, 50);

            Image rbImg = retBtnObj.GetComponent<Image>();
            rbImg.color = new Color(0.18f, 0.65f, 0.35f);

            Button btn = retBtnObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (NetworkArenaManager.Instance != null)
                {
                    NetworkArenaManager.Instance.DisconnectAndReturnToMenu();
                }
                SetVisible(false);
            });

            CreateText(retBtnObj.transform, "BtnText", "RETURN TO HANGAR", 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, font);

            victoryModal.SetActive(false);
        }

        private GameObject CreateText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Font font)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Text txt = obj.GetComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.font = font;
            return obj;
        }
    }
}
