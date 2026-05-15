using System;
using Blockblast.View;
using UnityEngine;

namespace Blockblast.Controller
{
    public static class ThemeManager
    {
        public static readonly int[] Thresholds = { 0, 150, 350, 750, 1700, 3500 };

        public static readonly Theme[] Themes =
        {
            new Theme(
                name: "Vibrant",
                colorFilter: original => original,
                backgroundColor: new Color(0.05f, 0.07f, 0.12f, 1f),
                gridBackgroundColor: new Color(0.10f, 0.12f, 0.18f, 1f),
                emptyCellColor: new Color(0.18f, 0.20f, 0.28f, 1f),
                scoreTextColor: new Color(0.95f, 0.95f, 1f, 1f),
                celebrationAccentColor: new Color(1f, 1f, 1f, 1f)
            ),
            new Theme(
                name: "Pastel",
                colorFilter: original => Color.Lerp(original, Color.white, 0.30f),
                backgroundColor: new Color(0.13f, 0.10f, 0.18f, 1f),
                gridBackgroundColor: new Color(0.20f, 0.16f, 0.26f, 1f),
                emptyCellColor: new Color(0.32f, 0.26f, 0.40f, 1f),
                scoreTextColor: new Color(0.96f, 0.92f, 0.98f, 1f),
                celebrationAccentColor: new Color(0.95f, 0.75f, 0.95f, 1f)
            ),
            new Theme(
                name: "Or",
                colorFilter: ToGold,
                backgroundColor: new Color(0.20f, 0.08f, 0.04f, 1f),
                gridBackgroundColor: new Color(0.30f, 0.16f, 0.08f, 1f),
                emptyCellColor: new Color(0.40f, 0.24f, 0.14f, 1f),
                scoreTextColor: new Color(1.00f, 0.85f, 0.30f, 1f),
                celebrationAccentColor: new Color(1.00f, 0.85f, 0.30f, 1f)
            ),
            new Theme(
                name: "Néon",
                colorFilter: ToNeon,
                backgroundColor: new Color(0.02f, 0.02f, 0.05f, 1f),
                gridBackgroundColor: new Color(0.06f, 0.04f, 0.10f, 1f),
                emptyCellColor: new Color(0.10f, 0.08f, 0.18f, 1f),
                scoreTextColor: new Color(0.40f, 1.00f, 1.00f, 1f),
                celebrationAccentColor: new Color(1.00f, 0.30f, 0.95f, 1f)
            ),
            new Theme(
                name: "Argent",
                colorFilter: ToMono,
                backgroundColor: new Color(0.05f, 0.07f, 0.14f, 1f),
                gridBackgroundColor: new Color(0.10f, 0.14f, 0.22f, 1f),
                emptyCellColor: new Color(0.22f, 0.26f, 0.34f, 1f),
                scoreTextColor: new Color(0.92f, 0.95f, 1.00f, 1f),
                celebrationAccentColor: new Color(0.85f, 0.92f, 1.00f, 1f)
            ),
            new Theme(
                name: "Arc-en-ciel",
                colorFilter: ToRainbow,
                backgroundColor: new Color(0.02f, 0.02f, 0.04f, 1f),
                gridBackgroundColor: new Color(0.06f, 0.06f, 0.12f, 1f),
                emptyCellColor: new Color(0.14f, 0.14f, 0.22f, 1f),
                scoreTextColor: new Color(1.00f, 1.00f, 1.00f, 1f),
                celebrationAccentColor: new Color(1.00f, 0.50f, 0.85f, 1f)
            ),
        };

        public static Theme GetForLevel(int level)
        {
            int safeLevel = Mathf.Clamp(level, 0, Themes.Length - 1);
            return Themes[safeLevel];
        }

        public static int LevelForScore(int score)
        {
            int reachedLevel = 0;
            for (int i = 0; i < Thresholds.Length; i++)
            {
                if (score >= Thresholds[i]) reachedLevel = i;
                else break;
            }
            return reachedLevel;
        }

        private static Color ToGold(Color original)
        {
            float luminance = original.r * 0.299f + original.g * 0.587f + original.b * 0.114f;
            return new Color(
                Mathf.Clamp01(luminance * 1.15f + 0.20f),
                Mathf.Clamp01(luminance * 0.95f + 0.10f),
                Mathf.Clamp01(luminance * 0.30f),
                original.a
            );
        }

        private static Color ToNeon(Color original)
        {
            Color.RGBToHSV(original, out float h, out float s, out float v);
            float boostedS = Mathf.Clamp01(Mathf.Max(s, 0.5f) * 1.3f);
            float boostedV = Mathf.Clamp01(Mathf.Max(v, 0.7f) * 1.2f);
            Color rgb = Color.HSVToRGB(h, boostedS, boostedV);
            rgb.a = original.a;
            return rgb;
        }

        private static Color ToMono(Color original)
        {
            float luminance = original.r * 0.299f + original.g * 0.587f + original.b * 0.114f;
            float boosted = Mathf.Clamp01(luminance * 1.3f + 0.20f);
            return new Color(boosted * 0.95f, boosted * 0.97f, boosted, original.a);
        }

        private static Color ToRainbow(Color original)
        {
            Color.RGBToHSV(original, out float h, out float s, out float v);
            float rotatedH = (h + 0.5f) % 1f;
            float boostedS = Mathf.Clamp01(s * 1.2f);
            Color rgb = Color.HSVToRGB(rotatedH, boostedS, v);
            rgb.a = original.a;
            return rgb;
        }
    }
}
