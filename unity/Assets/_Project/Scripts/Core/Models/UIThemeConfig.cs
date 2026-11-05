using UnityEngine;

namespace StarStrike.Core
{
    [CreateAssetMenu(fileName = "UIThemeConfig", menuName = "StarStrike/Configs/UI Theme Config")]
    public class UIThemeConfig : ScriptableObject
    {
        [Header("Primary Brand & Accents")]
        public Color primaryAccent = new Color(0.2f, 0.9f, 1f, 1f);     // Electric Cyan
        public Color secondaryAccent = new Color(0.6f, 0.4f, 1f, 1f);   // Neon Violet

        [Header("Player Vitals")]
        public Color shieldColor = new Color(0.25f, 0.75f, 1f, 1f);      // Cyan / Electric Blue
        public Color shieldGhostColor = new Color(1f, 0.35f, 0.25f, 0.9f); // Lagging damage ghost bar
        public Color energyColor = new Color(1f, 0.85f, 0.2f, 1f);       // Gold / Yellow
        public Color boostColor = new Color(0.1f, 1f, 0.75f, 1f);        // Bright Mint / Teal
        public Color dangerColor = new Color(1f, 0.25f, 0.25f, 1f);       // Critical Red

        [Header("Panels & Backgrounds")]
        public Color panelBackground = new Color(0.05f, 0.07f, 0.11f, 0.88f); // Translucent Dark Slate
        public Color panelBorder = new Color(0.22f, 0.45f, 0.65f, 0.7f);       // Metallic Blue Border
        public Color buttonNormal = new Color(0.1f, 0.18f, 0.28f, 0.9f);
        public Color buttonHover = new Color(0.18f, 0.35f, 0.55f, 1f);
        public Color buttonPressed = new Color(0.08f, 0.5f, 0.75f, 1f);
        public Color buttonDisabled = new Color(0.08f, 0.1f, 0.14f, 0.5f);

        [Header("Typography")]
        public Color textPrimary = Color.white;
        public Color textMuted = new Color(0.68f, 0.74f, 0.82f, 1f);
        public Color textHighlight = new Color(0.2f, 0.95f, 1f, 1f);

        [Header("Animation & Easing Parameters")]
        public float barFillSmoothSpeed = 12f;
        public float ghostBarDelaySeconds = 0.35f;
        public float ghostBarDrainSpeed = 3.5f;
        public float warningPulseFrequency = 3f;
        public float drawerSlideSpeed = 10f;
    }
}
