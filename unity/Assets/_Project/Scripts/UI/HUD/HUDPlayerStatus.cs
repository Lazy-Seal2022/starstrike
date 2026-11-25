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

        private bool isEnergyLockedOut = false;
        private float energyRecoveryTarget = 0f;
        private float cachedMaxEnergy = 80f;

        private string cachedShipId = "SCOUT";
        private string cachedWeaponName = "Pulse Laser";

        private int lastShieldCurInt = int.MinValue;
        private int lastShieldMaxInt = int.MinValue;
        private int lastHullCurInt = int.MinValue;
        private int lastHullMaxInt = int.MinValue;
        private int lastEnergyCurInt = int.MinValue;
        private int lastEnergyMaxInt = int.MinValue;
        private int lastEnergyRecInt = int.MinValue;
        private bool lastEnergyLockStr = false;
        private bool hasEnergyStrInit = false;
        private bool wasDangerPulsing = false;

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
            GameEvents.OnEnergyLockoutChanged += HandleEnergyLockoutChanged;
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnPlayerEvolved += HandlePlayerEvolved;
            GameEvents.OnWeaponSwapped += HandleWeaponSwapped;
        }

        private void OnDisable()
        {
            GameEvents.OnShieldChanged -= HandleShieldChanged;
            GameEvents.OnHullChanged -= HandleHullChanged;
            GameEvents.OnEnergyChanged -= HandleEnergyChanged;
            GameEvents.OnEnergyLockoutChanged -= HandleEnergyLockoutChanged;
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnPlayerEvolved -= HandlePlayerEvolved;
            GameEvents.OnWeaponSwapped -= HandleWeaponSwapped;
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
            if (!Mathf.Approximately(currentShieldRatio, targetShieldRatio))
            {
                currentShieldRatio = Mathf.MoveTowards(currentShieldRatio, targetShieldRatio, smoothSpeed * dt);
                if (shieldFill != null) shieldFill.fillAmount = currentShieldRatio;
            }

            // 2. Lagging Damage Ghost Bar
            float delay = themeConfig != null ? themeConfig.ghostBarDelaySeconds : 0.35f;
            float drainSpeed = themeConfig != null ? themeConfig.ghostBarDrainSpeed : 3.5f;

            if (!Mathf.Approximately(ghostShieldRatio, targetShieldRatio) && Time.time - lastDamageTime > delay)
            {
                ghostShieldRatio = Mathf.MoveTowards(ghostShieldRatio, targetShieldRatio, drainSpeed * dt);
                if (shieldGhostFill != null) shieldGhostFill.fillAmount = ghostShieldRatio;
            }

            // 3. Smooth Hull Fill
            if (!Mathf.Approximately(currentHullRatio, targetHullRatio))
            {
                currentHullRatio = Mathf.MoveTowards(currentHullRatio, targetHullRatio, smoothSpeed * dt);
                if (hullFill != null) hullFill.fillAmount = currentHullRatio;
            }

            // 4. Smooth Energy Fill
            if (!Mathf.Approximately(currentEnergyRatio, targetEnergyRatio))
            {
                currentEnergyRatio = Mathf.MoveTowards(currentEnergyRatio, targetEnergyRatio, smoothSpeed * dt);
                if (energyFill != null) energyFill.fillAmount = currentEnergyRatio;
            }

            // 5. Boost Fill
            if (boostFill != null && !Mathf.Approximately(boostFill.fillAmount, targetBoostRatio))
            {
                boostFill.fillAmount = Mathf.MoveTowards(boostFill.fillAmount, targetBoostRatio, smoothSpeed * dt);
            }

            // 6. Low Shield Danger Pulse
            if (shieldText != null && themeConfig != null)
            {
                if (targetShieldRatio < 0.25f)
                {
                    wasDangerPulsing = true;
                    float pulse = Mathf.PingPong(Time.time * themeConfig.warningPulseFrequency, 1f);
                    shieldText.color = Color.Lerp(themeConfig.dangerColor, themeConfig.textPrimary, pulse);
                }
                else if (wasDangerPulsing)
                {
                    wasDangerPulsing = false;
                    shieldText.color = themeConfig.textPrimary;
                }
            }

            // 7. Energy Capacitor Lockout Pulse
            if (isEnergyLockedOut && energyFill != null)
            {
                float pulse = Mathf.PingPong(Time.time * 4f, 1f);
                energyFill.color = Color.Lerp(new Color(1f, 0.35f, 0.05f), new Color(1f, 0.75f, 0.2f), pulse);
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

            int curInt = Mathf.CeilToInt(current);
            int maxInt = Mathf.CeilToInt(max);
            if (curInt != lastShieldCurInt || maxInt != lastShieldMaxInt)
            {
                lastShieldCurInt = curInt;
                lastShieldMaxInt = maxInt;
                string shieldStr = $"{curInt} / {maxInt}";
                if (shieldText != null) shieldText.text = shieldStr;
                if (shieldTmpText != null) shieldTmpText.text = shieldStr;
            }
        }

        public void HandleHullChanged(float current, float max)
        {
            targetHullRatio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            int curInt = Mathf.CeilToInt(current);
            int maxInt = Mathf.CeilToInt(max);
            if (curInt != lastHullCurInt || maxInt != lastHullMaxInt)
            {
                lastHullCurInt = curInt;
                lastHullMaxInt = maxInt;
                string hullStr = $"{curInt} / {maxInt}";
                if (hullText != null) hullText.text = hullStr;
                if (hullTmpText != null) hullTmpText.text = hullStr;
            }
        }

        public void HandleEnergyChanged(float current, float max)
        {
            cachedMaxEnergy = max;
            targetEnergyRatio = max > 0 ? Mathf.Clamp01(current / max) : 0f;

            int curInt = Mathf.CeilToInt(current);
            int maxInt = Mathf.CeilToInt(max);
            int recInt = Mathf.CeilToInt(energyRecoveryTarget);
            if (!hasEnergyStrInit || lastEnergyLockStr != isEnergyLockedOut || curInt != lastEnergyCurInt ||
                (isEnergyLockedOut ? recInt != lastEnergyRecInt : maxInt != lastEnergyMaxInt))
            {
                hasEnergyStrInit = true;
                lastEnergyLockStr = isEnergyLockedOut;
                lastEnergyCurInt = curInt;
                lastEnergyMaxInt = maxInt;
                lastEnergyRecInt = recInt;

                string energyStr = isEnergyLockedOut
                    ? $"REBOOT [{curInt}/{recInt}]"
                    : $"{curInt} / {maxInt}";
                if (energyText != null) energyText.text = energyStr;
                if (energyTmpText != null) energyTmpText.text = energyStr;
            }
        }

        public void HandleEnergyLockoutChanged(bool isLocked, float current, float target)
        {
            isEnergyLockedOut = isLocked;
            energyRecoveryTarget = target;
            if (!isLocked && themeConfig != null && energyFill != null)
            {
                energyFill.color = themeConfig.energyColor;
            }
            HandleEnergyChanged(current, cachedMaxEnergy);
        }

        public void HandleGemCollected(int amount, int total)
        {
            string gemStr = $"{total} GEMS";
            if (gemsText != null) gemsText.text = gemStr;
            if (gemsTmpText != null) gemsTmpText.text = gemStr;
        }

        public void HandlePlayerEvolved(string shipId)
        {
            if (!string.IsNullOrEmpty(shipId)) cachedShipId = shipId.ToUpper();
            UpdateShipBadge();
        }

        public void HandleWeaponSwapped(string weaponName, int weaponIndex)
        {
            if (!string.IsNullOrEmpty(weaponName)) cachedWeaponName = weaponName;
            UpdateShipBadge();
        }

        private void UpdateShipBadge()
        {
            string shipStr = $"• {cachedShipId} [{cachedWeaponName}] •";
            if (shipNameText != null) shipNameText.text = shipStr;
            if (shipNameTmpText != null) shipNameTmpText.text = shipStr;
        }

        public void SetBoostRatio(float ratio)
        {
            targetBoostRatio = Mathf.Clamp01(ratio);
        }
    }
}
