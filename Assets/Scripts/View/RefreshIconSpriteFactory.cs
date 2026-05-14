using UnityEngine;

namespace Blockblast.View
{
    public static class RefreshIconSpriteFactory
    {
        private const int TextureSize = 128;

        private static Sprite cachedRefreshIcon;

        public static Sprite GetRefreshIcon()
        {
            if (cachedRefreshIcon != null) return cachedRefreshIcon;

            var pixels = new Color[TextureSize * TextureSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(1f, 1f, 1f, 0f);

            float center = TextureSize * 0.5f;
            float outerRadius = TextureSize * 0.38f;
            float innerRadius = TextureSize * 0.27f;
            float ringThickness = outerRadius - innerRadius;
            float midRadius = (outerRadius + innerRadius) * 0.5f;

            const float arcStartClockDeg = 35f;
            const float arcEndClockDeg = 330f;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float dx = (x + 0.5f) - center;
                    float dy = (y + 0.5f) - center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    if (distance < innerRadius - 1.5f || distance > outerRadius + 1.5f) continue;

                    float clockDeg = 90f - Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (clockDeg < 0f) clockDeg += 360f;
                    if (clockDeg >= 360f) clockDeg -= 360f;

                    if (clockDeg < arcStartClockDeg || clockDeg > arcEndClockDeg) continue;

                    float alpha = 1f;
                    if (distance < innerRadius) alpha *= Mathf.Clamp01(distance - (innerRadius - 1.5f));
                    if (distance > outerRadius) alpha *= Mathf.Clamp01((outerRadius + 1.5f) - distance);

                    pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            float arrowClockRad = arcStartClockDeg * Mathf.Deg2Rad;
            Vector2 arrowAnchor = new Vector2(
                center + midRadius * Mathf.Sin(arrowClockRad),
                center + midRadius * Mathf.Cos(arrowClockRad)
            );
            Vector2 radialOut = new Vector2(Mathf.Sin(arrowClockRad), Mathf.Cos(arrowClockRad));
            Vector2 tangentCcw = new Vector2(-Mathf.Cos(arrowClockRad), Mathf.Sin(arrowClockRad));

            float arrowReach = ringThickness * 1.6f;
            float arrowHalfWidth = ringThickness * 1.25f;

            Vector2 tip = arrowAnchor + tangentCcw * arrowReach;
            Vector2 baseInner = arrowAnchor - radialOut * arrowHalfWidth;
            Vector2 baseOuter = arrowAnchor + radialOut * arrowHalfWidth;

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (PointInTriangle(p, tip, baseInner, baseOuter))
                    {
                        pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, 1f);
                    }
                }
            }

            cachedRefreshIcon = SpriteBuilder.BuildFromPixels(TextureSize, pixels, Vector4.zero);
            return cachedRefreshIcon;
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNegative = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPositive = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNegative && hasPositive);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }
    }
}
