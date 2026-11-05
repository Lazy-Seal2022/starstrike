using UnityEngine;
using UnityEngine.UI;
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

        [Header("Energy Bar")]
        public Image energyFill;
        public Text energyText;

        [Header("Afterburner Boost Bar")]
        public Image boostFill;

        [Header("Badges & Counters")]
        public Text shipNameText;
        public Text gemsText;

        // Current internal targets
        private float targetShieldRatio = 1f;
        private float currentShieldRatio = 1f;
        private float ghostShieldRatio = 1f;
        private float lastDamageTime = -999f;

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
            GameEvents.OnEnergyChanged += HandleEnergyChanged;
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
        }

        private void OnDisable()
        {
            GameEvents.OnShieldChanged -= HandleShieldChanged;
            GameEvents.OnEnergyChanged -= HandleEnergyChanged;
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
        }

        public void ApplyThemeStyles()
        {
            if (themeConfig == null) return;

            if (shieldFill != null) shieldFill.color = themeConfig.shieldColor;
            if (shieldGhostFill != null) shieldGhostFill.color = themeConfig.shieldGhostColor;
            if (energyFill != null) energyFill.color = themeConfig.energyColor;
            if (boostFill != null) boostFill.color = themeConfig.boostColor;
            if (shieldText != null) shieldText.color = themeConfig.textPrimary;
            if (energyText != null) energyText.color = themeConfig.textPrimary;
            if (shipNameText != null) shipNameText.color = themeConfig.primaryAccent;
            if (gemsText != null) gemsText.color = themeConfig.energyColor;
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

            // 3. Smooth Energy Fill
            currentEnergyRatio = Mathf.MoveTowards(currentEnergyRatio, targetEnergyRatio, smoothSpeed * dt);
            if (energyFill != null)
            {
                energyFill.fillAmount = currentEnergyRatio;
            }

            // 4. Boost Fill
            if (boostFill != null)
            {
                boostFill.fillAmount = Mathf.MoveTowards(boostFill.fillAmount, targetBoostRatio, smoothSpeed * dt);
            }

            // 5. Low Shield Danger Pulse
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

            if (shieldText != null)
            {
                shieldText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        public void HandleEnergyChanged(float current, float max)
        {
            targetEnergyRatio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            if (energyText != null)
            {
                energyText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        public void HandleGemCollected(int amount, int total)
        {
            if (gemsText != null)
            {
                gemsText.text = $"◈ {total}";
            }
        }

        public void HandlePlayerEvolved(string shipId)
        {
            if (shipNameText != null)
            {
                shipNameText.text = $"• {shipId.ToUpper()} •";
            }
        }

        public void SetBoostRatio(float ratio)
        {
            targetBoostRatio = Mathf.Clamp01(ratio);
        }
    }
}
