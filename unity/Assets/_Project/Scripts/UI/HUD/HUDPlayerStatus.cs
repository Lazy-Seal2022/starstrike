using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarStrike.Core;

namespace StarStrike.UI
{
    public class HUDPlayerStatus : MonoBehaviour
    {
        [Header("Configuration")]
        public UIThemeConfig themeConfig;

        [Header("Shield Bar")]
        public Image shieldFill;
        public Image shieldGhostFill;
        public Text shieldText;
        public TMP_Text shieldTmpText;

        [Header("Hull Bar")]
        public Image hullFill;
        public Image hullGhostFill;
        public Text hullText;
        public TMP_Text hullTmpText;

        [Header("Energy Bar")]
        public Image energyFill;
        public Text energyText;
        public TMP_Text energyTmpText;

        [Header("Afterburner Boost Bar")]
        public Image boostFill;

        [Header("Badges & Counters")]
        public Text shipNameText;
        public TMP_Text shipNameTmpText;
        public Text gemsText;
        public TMP_Text gemsTmpText;

        // Current internal targets
        private float targetShieldRatio = 1f;
        private float currentShieldRatio = 1f;
        private float ghostShieldRatio = 1f;
        private float lastDamageTime = -999f;

        private float targetHullRatio = 1f;
        private float currentHullRatio = 1f;

        private float targetEnergyRatio = 1f;
        private float currentEnergyRatio = 1f;

        private float targetBoostRatio = 1f;

        private void Awake()
        {
            if (themeConfig == null) themeConfig = Resources.Load<UIThemeConfig>("Data/UIThemeConfig");
            if (themeConfig == null) themeConfig = ScriptableObject.CreateInstance<UIThemeConfig>();

            ApplyThemeStyles();
        }

        private void OnEnable()
        {
            GameEvents.OnShieldChanged += HandleShieldChanged;
            GameEvents.OnHullChanged += HandleHullChanged;
            GameEvents.OnEnergyChanged += HandleEnergyChanged;
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnShieldChanged -= HandleShieldChanged;
            GameEvents.OnHullChanged -= HandleHullChanged;
            GameEvents.OnEnergyChanged -= HandleEnergyChanged;
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
        }

        public void ApplyThemeStyles()
        {
            if (themeConfig == null) return;

            if (shieldFill != null) shieldFill.color = themeConfig.shieldColor;
            if (shieldGhostFill != null) shieldGhostFill.color = themeConfig.shieldGhostColor;
            if (hullFill != null) hullFill.color = themeConfig.hullColor;
            if (hullGhostFill != null) hullGhostFill.color = themeConfig.shieldGhostColor;
            if (energyFill != null) energyFill.color = themeConfig.energyColor;
            if (boostFill != null) boostFill.color = themeConfig.boostColor;
            if (shieldText != null) shieldText.color = themeConfig.textPrimary;
            if (shieldTmpText != null) shieldTmpText.color = themeConfig.textPrimary;
            if (hullText != null) hullText.color = themeConfig.textPrimary;
            if (hullTmpText != null) hullTmpText.color = themeConfig.textPrimary;
            if (energyText != null) energyText.color = themeConfig.textPrimary;
            if (energyTmpText != null) energyTmpText.color = themeConfig.textPrimary;
            if (shipNameText != null) shipNameText.color = themeConfig.primaryAccent;
            if (shipNameTmpText != null) shipNameTmpText.color = themeConfig.primaryAccent;
            if (gemsText != null) gemsText.color = themeConfig.energyColor;
            if (gemsTmpText != null) gemsTmpText.color = themeConfig.energyColor;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float smoothSpeed = themeConfig != null ? themeConfig.barFillSmoothSpeed : 12f;

            // 1. Smooth Shield Fill
            currentShieldRatio = Mathf.MoveTowards(currentShieldRatio, targetShieldRatio, smoothSpeed * dt);
            if (shieldFill != null)
            {
                shieldFill.fillAmount = currentShieldRatio;
            }

            // 2. Lagging Damage Ghost Bar
            float delay = themeConfig != null ? themeConfig.ghostBarDelaySeconds : 0.35f;
            float drainSpeed = themeConfig != null ? themeConfig.ghostBarDrainSpeed : 3.5f;

            if (Time.time - lastDamageTime > delay)
            {
                ghostShieldRatio = Mathf.MoveTowards(ghostShieldRatio, targetShieldRatio, drainSpeed * dt);
                if (shieldGhostFill != null)
                {
                    shieldGhostFill.fillAmount = ghostShieldRatio;
                }
            }

            // 3. Smooth Hull Fill
            currentHullRatio = Mathf.MoveTowards(currentHullRatio, targetHullRatio, smoothSpeed * dt);
            if (hullFill != null)
            {
                hullFill.fillAmount = currentHullRatio;
            }

            // 4. Smooth Energy Fill
            currentEnergyRatio = Mathf.MoveTowards(currentEnergyRatio, targetEnergyRatio, smoothSpeed * dt);
            if (energyFill != null)
            {
                energyFill.fillAmount = currentEnergyRatio;
            }

            // 5. Boost Fill
            if (boostFill != null)
            {
                boostFill.fillAmount = Mathf.MoveTowards(boostFill.fillAmount, targetBoostRatio, smoothSpeed * dt);
            }

            // 6. Low Shield Danger Pulse
            if (targetShieldRatio < 0.25f && shieldText != null && themeConfig != null)
            {
                float pulse = Mathf.PingPong(Time.time * themeConfig.warningPulseFrequency, 1f);
                shieldText.color = Color.Lerp(themeConfig.dangerColor, themeConfig.textPrimary, pulse);
            }
        }

        public void HandleShieldChanged(float current, float max)
        {
            float newRatio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            // If taking damage, trigger ghost bar delay
            if (newRatio < targetShieldRatio)
            {
                lastDamageTime = Time.time;
            }
            else if (newRatio > targetShieldRatio)
            {
                // Instant snap for healing
                ghostShieldRatio = newRatio;
                if (shieldGhostFill != null) shieldGhostFill.fillAmount = ghostShieldRatio;
            }

            targetShieldRatio = newRatio;

            string shieldStr = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            if (shieldText != null) shieldText.text = shieldStr;
            if (shieldTmpText != null) shieldTmpText.text = shieldStr;
        }

        public void HandleHullChanged(float current, float max)
        {
            targetHullRatio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            string hullStr = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            if (hullText != null) hullText.text = hullStr;
            if (hullTmpText != null) hullTmpText.text = hullStr;
        }

        public void HandleEnergyChanged(float current, float max)
        {
            targetEnergyRatio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            string energyStr = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            if (energyText != null) energyText.text = energyStr;
            if (energyTmpText != null) energyTmpText.text = energyStr;
        }

        public void HandleGemCollected(int amount, int total)
        {
            string gemStr = $"{total} GEMS";
            if (gemsText != null) gemsText.text = gemStr;
            if (gemsTmpText != null) gemsTmpText.text = gemStr;
        }

        public void HandlePlayerEvolved(string shipId)
        {
            string shipStr = $"• {shipId.ToUpper()} •";
            if (shipNameText != null) shipNameText.text = shipStr;
            if (shipNameTmpText != null) shipNameTmpText.text = shipStr;
        }

        public void SetBoostRatio(float ratio)
        {
            targetBoostRatio = Mathf.Clamp01(ratio);
        }
    }
}
