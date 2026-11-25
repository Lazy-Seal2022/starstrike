using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarStrike.Core;
using StarStrike.UI;
using StarStrike.Gameplay.Carrier;

namespace StarStrike.UI.HUD
{
    /// <summary>
    /// Tactical HUD overlay providing visual flight deck feedback, squadron readiness,
    /// foundry fabrication meters, and stance commands for Carrier captains.
    /// </summary>
    public class CarrierHUDOverlay : MonoBehaviour
    {
        private CarrierHangarController carrier;
        private GameObject panelRoot;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI squadronText;
        private TextMeshProUGUI stanceText;
        private TextMeshProUGUI foundryText;

        private float nextFindTime;
        private int lastActive = -1;
        private int lastCap = -1;
        private DroneCommandStance lastStance = (DroneCommandStance)(-1);
        private int lastBuildPct = -1;
        private bool lastIsBuilding;
        private bool hasFoundryState;
        private readonly StringBuilder squadronSb = new StringBuilder(48);

        private void OnEnable()
        {
            GameEvents.OnGameStarted += HandleRosterChanged;
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= HandleRosterChanged;
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
        }

        private void Start()
        {
            FindCarrier();
            BuildUI();
        }

        private void HandleRosterChanged()
        {
            carrier = null;
            nextFindTime = 0f;
            FindCarrier();
        }

        private void HandlePlayerEvolved(string _)
        {
            HandleRosterChanged();
        }

        private void Update()
        {
            if (carrier == null)
            {
                if (Time.unscaledTime >= nextFindTime)
                {
                    nextFindTime = Time.unscaledTime + 1.0f;
                    FindCarrier();
                }

                if (carrier == null)
                {
                    if (panelRoot != null && panelRoot.activeSelf) panelRoot.SetActive(false);
                    return;
                }
            }

            if (panelRoot == null)
            {
                BuildUI();
            }

            if (panelRoot != null && !panelRoot.activeSelf)
            {
                panelRoot.SetActive(true);
            }

            UpdateDisplay();
        }

        private void FindCarrier()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                carrier = player.GetComponent<CarrierHangarController>();
            }
            if (carrier == null && nextFindTime <= 0f)
            {
                carrier = FindAnyObjectByType<CarrierHangarController>();
            }
        }

        private void BuildUI()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindAnyObjectByType<Canvas>();
            }
            if (canvas == null) return;

            // Panel Root
            panelRoot = new GameObject("Carrier_FlightDeck_Panel");
            panelRoot.transform.SetParent(canvas.transform, false);

            var rect = panelRoot.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 85f);
            rect.sizeDelta = new Vector2(340f, 75f);

            var bg = panelRoot.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);

            // Title Header
            titleText = UIHelper.CreateTextMeshPro(
                panelRoot.transform,
                "Carrier_Header",
                "VANGUARD CARRIER // FLIGHT OPS",
                12,
                FontStyles.Bold,
                new Color(0.2f, 0.85f, 1f),
                TextAlignmentOptions.Left,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(12f, -14f),
                new Vector2(-12f, 14f));

            // Squadron Readout
            squadronText = UIHelper.CreateTextMeshPro(
                panelRoot.transform,
                "Carrier_Squadron",
                "SQUADRON: 4/4 ACTIVE",
                13,
                FontStyles.Normal,
                Color.white,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0.5f),
                new Vector2(0.65f, 0.5f),
                new Vector2(12f, 2f),
                new Vector2(0f, 18f));

            // Stance Command Readout
            stanceText = UIHelper.CreateTextMeshPro(
                panelRoot.transform,
                "Carrier_Stance",
                "[F] STANCE: DEFENSIVE ESCORT",
                12,
                FontStyles.Bold,
                new Color(0.25f, 1f, 0.5f),
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(12f, 12f),
                new Vector2(-12f, 26f));

            // Foundry Status Readout
            foundryText = UIHelper.CreateTextMeshPro(
                panelRoot.transform,
                "Carrier_Foundry",
                "FOUNDRY: READY",
                11,
                FontStyles.Normal,
                new Color(1f, 0.85f, 0.2f),
                TextAlignmentOptions.Right,
                new Vector2(0.6f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(0f, 2f),
                new Vector2(-12f, 18f));
        }

        private void UpdateDisplay()
        {
            if (carrier == null) return;

            // Squadron count
            int active = carrier.ActiveDroneCount;
            int cap = carrier.DroneCapacity;
            if (squadronText != null && (active != lastActive || cap != lastCap))
            {
                lastActive = active;
                lastCap = cap;
                squadronSb.Clear();
                squadronSb.Append("SQUADRON: ");
                for (int i = 0; i < cap; i++)
                {
                    squadronSb.Append(i < active ? "■ " : "□ ");
                }
                squadronSb.Append('(').Append(active).Append('/').Append(cap).Append(')');
                squadronText.text = squadronSb.ToString();
            }

            // Stance display
            var stance = carrier.CurrentStance;
            if (stanceText != null && stance != lastStance)
            {
                lastStance = stance;
                switch (stance)
                {
                    case DroneCommandStance.DefensiveEscort:
                        stanceText.text = "[F] STANCE: DEFENSIVE ESCORT";
                        stanceText.color = new Color(0.25f, 1f, 0.5f);
                        break;
                    case DroneCommandStance.OffensiveStrike:
                        stanceText.text = "[F] STANCE: OFFENSIVE STRIKE";
                        stanceText.color = new Color(1f, 0.35f, 0.25f);
                        break;
                    case DroneCommandStance.DockAndRepair:
                        stanceText.text = "[F] STANCE: DOCK & REPAIR";
                        stanceText.color = new Color(0.3f, 0.8f, 1f);
                        break;
                }
            }

            // Foundry fabrication display
            if (foundryText != null)
            {
                bool isBuilding = carrier.IsBuilding;
                int pct = isBuilding ? Mathf.RoundToInt(carrier.BuildProgress * 100f) : -1;
                if (!hasFoundryState || isBuilding != lastIsBuilding || pct != lastBuildPct)
                {
                    hasFoundryState = true;
                    lastIsBuilding = isBuilding;
                    lastBuildPct = pct;
                    if (isBuilding)
                    {
                        foundryText.text = $"FOUNDRY: {pct}%";
                        foundryText.color = new Color(1f, 0.85f, 0.2f);
                    }
                    else
                    {
                        foundryText.text = "FOUNDRY: READY";
                        foundryText.color = new Color(0.4f, 1f, 0.6f);
                    }
                }
            }
        }
    }
}
