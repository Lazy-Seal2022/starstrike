using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StarStrike.UI
{
    /// <summary>
    /// Slick Cyber-Vector visual theme engine inspired by Wipeout, F-Zero, and Geometry Wars.
    /// Provides procedural vector grid backgrounds, chamfered neon cards, holographic pedestals,
    /// and segmented LED vector telemetry bars with zero runtime GC allocations.
    /// </summary>
    public static class CyberVectorTheme
    {
        // ==========================================
        // CYBER-VECTOR COLOR PALETTE
        // ==========================================
        public static readonly Color VoidDeep = new Color(0.012f, 0.025f, 0.055f, 1.0f);        // #03060e
        public static readonly Color PanelFill = new Color(0.025f, 0.050f, 0.105f, 0.92f);      // #060d1b
        public static readonly Color PanelFillHigh = new Color(0.040f, 0.080f, 0.160f, 0.95f);  // #0a1429
        public static readonly Color NeonCyan = new Color(0.0f, 0.95f, 1.0f, 1.0f);             // #00f2ff
        public static readonly Color NeonMagenta = new Color(1.0f, 0.05f, 0.42f, 1.0f);          // #ff0d6b
        public static readonly Color VoltYellow = new Color(1.0f, 0.90f, 0.0f, 1.0f);            // #ffe600
        public static readonly Color LaserGreen = new Color(0.05f, 1.0f, 0.55f, 1.0f);           // #0dff8c
        public static readonly Color ElectricOrange = new Color(1.0f, 0.52f, 0.08f, 1.0f);       // #ff8514
        public static readonly Color GhostWhite = new Color(0.92f, 0.96f, 1.0f, 1.0f);           // #ebf5ff
        public static readonly Color DimMuted = new Color(0.32f, 0.42f, 0.58f, 1.0f);            // #526b94
        public static readonly Color BorderCyan = new Color(0.0f, 0.95f, 1.0f, 0.55f);
        public static readonly Color BorderMagenta = new Color(1.0f, 0.05f, 0.42f, 0.55f);
        public static readonly Color BorderVolt = new Color(1.0f, 0.90f, 0.0f, 0.55f);

        // Cached procedural sprites
        private static Sprite cachedGridSprite;
        private static Sprite cachedHoloPedestalSprite;
        private static Sprite cachedChamferCardSprite;
        private static Sprite cachedButtonSprite;
        private static Sprite cachedGlowBarSprite;
        private static Sprite cachedReticleSprite;

        /// <summary>
        /// Returns a procedural 256x256 vector grid sprite with subtle cyan coordinates and crosshairs.
        /// </summary>
        public static Sprite GetVectorGridSprite()
        {
            if (cachedGridSprite != null) return cachedGridSprite;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;

            Color baseBg = VoidDeep;
            Color gridLine = new Color(0.0f, 0.95f, 1.0f, 0.09f);
            Color gridSubLine = new Color(0.0f, 0.95f, 1.0f, 0.035f);
            Color dotPoint = new Color(0.0f, 0.95f, 1.0f, 0.35f);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = baseBg;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isMajorLine = (x % 64 == 0) || (y % 64 == 0);
                    bool isMinorLine = (x % 16 == 0) || (y % 16 == 0);
                    bool isIntersection = (x % 64 == 0) && (y % 64 == 0);

                    if (isIntersection)
                    {
                        pixels[y * size + x] = dotPoint;
                    }
                    else if (isMajorLine)
                    {
                        pixels[y * size + x] = gridLine;
                    }
                    else if (isMinorLine)
                    {
                        pixels[y * size + x] = gridSubLine;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            cachedGridSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return cachedGridSprite;
        }

        /// <summary>
        /// Returns a procedural 9-slice chamfered vector card with 45-degree corner bevels and glowing neon borders.
        /// </summary>
        public static Sprite GetChamferCardSprite(int size = 128, int chamfer = 16)
        {
            if (cachedChamferCardSprite != null) return cachedChamferCardSprite;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color fill = PanelFill;
            Color border = NeonCyan;
            Color borderInner = new Color(0.0f, 0.95f, 1.0f, 0.35f);

            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Check 4 chamfered corners
                    bool inCornerTL = (x < chamfer) && (y >= size - chamfer) && ((chamfer - x) + (y - (size - chamfer - 1)) > chamfer);
                    bool inCornerTR = (x >= size - chamfer) && (y >= size - chamfer) && ((x - (size - chamfer - 1)) + (y - (size - chamfer - 1)) > chamfer);
                    bool inCornerBL = (x < chamfer) && (y < chamfer) && ((chamfer - x) + (chamfer - y) > chamfer);
                    bool inCornerBR = (x >= size - chamfer) && (y < chamfer) && ((x - (size - chamfer - 1)) + (chamfer - y) > chamfer);

                    if (inCornerTL || inCornerTR || inCornerBL || inCornerBR)
                    {
                        pixels[y * size + x] = clear;
                        continue;
                    }

                    // Check border edges
                    bool isBorderEdge = (x <= 1 || x >= size - 2 || y <= 1 || y >= size - 2);
                    bool isChamferEdgeTL = (x < chamfer + 2) && (y >= size - chamfer - 2) && (Mathf.Abs((chamfer - x) + (y - (size - chamfer - 1)) - chamfer) <= 1);
                    bool isChamferEdgeTR = (x >= size - chamfer - 2) && (y >= size - chamfer - 2) && (Mathf.Abs((x - (size - chamfer - 1)) + (y - (size - chamfer - 1)) - chamfer) <= 1);
                    bool isChamferEdgeBL = (x < chamfer + 2) && (y < chamfer + 2) && (Mathf.Abs((chamfer - x) + (chamfer - y) - chamfer) <= 1);
                    bool isChamferEdgeBR = (x >= size - chamfer - 2) && (y < chamfer + 2) && (Mathf.Abs((x - (size - chamfer - 1)) + (chamfer - y) - chamfer) <= 1);

                    if (isBorderEdge || isChamferEdgeTL || isChamferEdgeTR || isChamferEdgeBL || isChamferEdgeBR)
                    {
                        pixels[y * size + x] = border;
                    }
                    else if (x <= 3 || x >= size - 4 || y <= 3 || y >= size - 4)
                    {
                        pixels[y * size + x] = borderInner;
                    }
                    else
                    {
                        pixels[y * size + x] = fill;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            // 9-slice border: left, bottom, right, top = chamfer + 2
            Vector4 borderSlice = new Vector4(chamfer + 2, chamfer + 2, chamfer + 2, chamfer + 2);
            cachedChamferCardSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, borderSlice);
            return cachedChamferCardSprite;
        }

        /// <summary>
        /// Returns a holographic vector pedestal sprite (concentric perspective vector ellipses with radar ticks)
        /// designed to sit directly underneath floating ship sprites.
        /// </summary>
        public static Sprite GetHoloPedestalSprite(int width = 256, int height = 96)
        {
            if (cachedHoloPedestalSprite != null) return cachedHoloPedestalSprite;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            float cx = width * 0.5f;
            float cy = height * 0.45f;
            float rxOuter = width * 0.44f;
            float ryOuter = height * 0.36f;
            float rxInner = width * 0.28f;
            float ryInner = height * 0.22f;

            Color outerRing = NeonCyan;
            Color innerRing = VoltYellow;
            Color glowCenter = new Color(0.0f, 0.95f, 1.0f, 0.25f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - cx);
                    float dy = (y - cy);

                    float distOuter = (dx * dx) / (rxOuter * rxOuter) + (dy * dy) / (ryOuter * ryOuter);
                    float distInner = (dx * dx) / (rxInner * rxInner) + (dy * dy) / (ryInner * ryInner);

                    // Outer glowing ring
                    if (Mathf.Abs(distOuter - 1.0f) < 0.07f)
                    {
                        float alpha = 1.0f - Mathf.Abs(distOuter - 1.0f) / 0.07f;
                        pixels[y * width + x] = new Color(outerRing.r, outerRing.g, outerRing.b, alpha * 0.85f);
                    }
                    // Inner ring
                    else if (Mathf.Abs(distInner - 1.0f) < 0.09f)
                    {
                        float alpha = 1.0f - Mathf.Abs(distInner - 1.0f) / 0.09f;
                        pixels[y * width + x] = new Color(innerRing.r, innerRing.g, innerRing.b, alpha * 0.70f);
                    }
                    // Center glow disc
                    else if (distInner < 0.85f)
                    {
                        float glow = 1.0f - Mathf.Sqrt(distInner);
                        pixels[y * width + x] = new Color(glowCenter.r, glowCenter.g, glowCenter.b, glow * 0.35f);
                    }

                    // Radial ticks at 30, 90, 150, 210, 270, 330 deg
                    if (distOuter >= 0.85f && distOuter <= 1.15f)
                    {
                        float angle = Mathf.Atan2(dy / ryOuter, dx / rxOuter) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        for (int t = 0; t < 360; t += 30)
                        {
                            if (Mathf.Abs(Mathf.DeltaAngle(angle, t)) < 1.4f)
                            {
                                pixels[y * width + x] = NeonCyan;
                                break;
                            }
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            cachedHoloPedestalSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            return cachedHoloPedestalSprite;
        }

        /// <summary>
        /// Returns a procedural chamfered button sprite with glowing vector borders.
        /// </summary>
        public static Sprite GetButtonSprite(int width = 96, int height = 48, int chamfer = 10)
        {
            if (cachedButtonSprite != null) return cachedButtonSprite;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color fill = PanelFillHigh;
            Color border = NeonCyan;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inCornerTL = (x < chamfer) && (y >= height - chamfer) && ((chamfer - x) + (y - (height - chamfer - 1)) > chamfer);
                    bool inCornerTR = (x >= width - chamfer) && (y >= height - chamfer) && ((x - (width - chamfer - 1)) + (y - (height - chamfer - 1)) > chamfer);
                    bool inCornerBL = (x < chamfer) && (y < chamfer) && ((chamfer - x) + (chamfer - y) > chamfer);
                    bool inCornerBR = (x >= width - chamfer) && (y < chamfer) && ((x - (width - chamfer - 1)) + (chamfer - y) > chamfer);

                    if (inCornerTL || inCornerTR || inCornerBL || inCornerBR)
                    {
                        pixels[y * width + x] = clear;
                        continue;
                    }

                    bool isBorder = (x <= 1 || x >= width - 2 || y <= 1 || y >= height - 2);
                    bool isChamferTL = (x < chamfer + 1) && (y >= height - chamfer - 1) && (Mathf.Abs((chamfer - x) + (y - (height - chamfer - 1)) - chamfer) <= 1);
                    bool isChamferTR = (x >= width - chamfer - 1) && (y >= height - chamfer - 1) && (Mathf.Abs((x - (width - chamfer - 1)) + (y - (height - chamfer - 1)) - chamfer) <= 1);
                    bool isChamferBL = (x < chamfer + 1) && (y < chamfer + 1) && (Mathf.Abs((chamfer - x) + (chamfer - y) - chamfer) <= 1);
                    bool isChamferBR = (x >= width - chamfer - 1) && (y < chamfer + 1) && (Mathf.Abs((x - (width - chamfer - 1)) + (chamfer - y) - chamfer) <= 1);

                    if (isBorder || isChamferTL || isChamferTR || isChamferBL || isChamferBR)
                    {
                        pixels[y * width + x] = border;
                    }
                    else
                    {
                        pixels[y * width + x] = fill;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            Vector4 borderSlice = new Vector4(chamfer + 2, chamfer + 2, chamfer + 2, chamfer + 2);
            cachedButtonSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, borderSlice);
            return cachedButtonSprite;
        }

        /// <summary>
        /// Returns a sleek horizontal tapered vector glow bar for headers and dividers.
        /// </summary>
        public static Sprite GetGlowBarSprite(int width = 128, int height = 8)
        {
            if (cachedGlowBarSprite != null) return cachedGlowBarSprite;

            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[width * height];
            float midY = height * 0.5f;

            for (int y = 0; y < height; y++)
            {
                float dy = 1.0f - Mathf.Abs((y - midY) / midY);
                for (int x = 0; x < width; x++)
                {
                    float dx = 1.0f - Mathf.Abs((x - width * 0.5f) / (width * 0.5f));
                    float intensity = dx * dy;
                    pixels[y * width + x] = new Color(NeonCyan.r, NeonCyan.g, NeonCyan.b, intensity * 0.9f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            cachedGlowBarSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            return cachedGlowBarSprite;
        }

        /// <summary>
        /// Returns a procedural tactical corner bracket targeting reticle sprite.
        /// </summary>
        public static Sprite GetTacticalReticleSprite(int size = 64)
        {
            if (cachedReticleSprite != null) return cachedReticleSprite;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color clear = new Color(0, 0, 0, 0);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Color reticleCol = Color.white;
            int bracketLen = size / 4; // 16px corner brackets
            int thickness = 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool topBracket = (y >= size - thickness) && (x < bracketLen || x >= size - bracketLen);
                    bool bottomBracket = (y < thickness) && (x < bracketLen || x >= size - bracketLen);
                    bool leftBracket = (x < thickness) && (y < bracketLen || y >= size - bracketLen);
                    bool rightBracket = (x >= size - thickness) && (y < bracketLen || y >= size - bracketLen);

                    if (topBracket || bottomBracket || leftBracket || rightBracket)
                    {
                        pixels[y * size + x] = reticleCol;
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            cachedReticleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return cachedReticleSprite;
        }

        // ==========================================
        // TELEMETRY & STAT FORMATTERS
        // ==========================================

        /// <summary>
        /// Formats numeric stats into high-tech segmented LED vector meters (e.g. [■■■■■■□□□□]).
        /// </summary>
        public static string FormatStatMeter(string label, float current, float max, string unit, Color activeColor, int totalSegments = 10)
        {
            int filled = Mathf.Clamp(Mathf.RoundToInt((current / Mathf.Max(1f, max)) * totalSegments), 0, totalSegments);
            int empty = totalSegments - filled;

            string colorHex = ColorUtility.ToHtmlStringRGB(activeColor);
            string filledStr = new string('■', filled);
            string emptyStr = new string('□', empty);

            return $"<color=#{colorHex}>{label,-15}</color> <color=#{colorHex}>{filledStr}</color><color=#122033>{emptyStr}</color>  <color=#ffffff><b>{current:F0}</b></color> <color=#4d698e>{unit}</color>";
        }

        /// <summary>
        /// Formats a custom meter with a direct readout string.
        /// </summary>
        public static string FormatMeterDirect(string label, int filled, int total, string readout, Color activeColor)
        {
            int empty = Mathf.Max(0, total - filled);
            string colorHex = ColorUtility.ToHtmlStringRGB(activeColor);
            string filledStr = new string('■', filled);
            string emptyStr = new string('□', empty);

            return $"<color=#{colorHex}>{label,-15}</color> <color=#{colorHex}>{filledStr}</color><color=#122033>{emptyStr}</color>  <color=#ffffff>{readout}</color>";
        }

        /// <summary>
        /// Applies high-contrast cyber-vector button styling with glowing transitions.
        /// </summary>
        public static void ApplyVectorButtonStyle(Button btn, Color accentColor, bool isPrimary = false)
        {
            if (btn == null) return;

            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock cb = btn.colors;

            Color baseFill = isPrimary ? new Color(accentColor.r * 0.25f, accentColor.g * 0.25f, accentColor.b * 0.25f, 0.90f) : PanelFillHigh;
            cb.normalColor = baseFill;
            cb.highlightedColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.85f);
            cb.pressedColor = Color.white;
            cb.selectedColor = new Color(accentColor.r * 0.40f, accentColor.g * 0.40f, accentColor.b * 0.40f, 0.95f);
            cb.disabledColor = new Color(0.1f, 0.15f, 0.22f, 0.5f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f; // Snappy cyber response

            btn.colors = cb;
        }
    }
}
