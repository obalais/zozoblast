using UnityEngine;

namespace Blockblast.View
{
    public static class CellSpriteFactory
    {
        private const int TextureSize = 64;
        private const int CornerRadius = 12;

        private static Sprite cachedRoundedSquare;

        public static Sprite GetRoundedSquare()
        {
            if (cachedRoundedSquare != null) return cachedRoundedSquare;

            var pixels = new Color[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float alpha = ComputeRoundedAlpha(x, y);
                    pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            cachedRoundedSquare = SpriteBuilder.BuildFromPixels(
                TextureSize,
                pixels,
                new Vector4(CornerRadius, CornerRadius, CornerRadius, CornerRadius)
            );
            return cachedRoundedSquare;
        }

        private static float ComputeRoundedAlpha(int pixelX, int pixelY)
        {
            float centeredX = pixelX + 0.5f;
            float centeredY = pixelY + 0.5f;

            float distanceToNearestXEdge = Mathf.Min(centeredX, TextureSize - centeredX);
            float distanceToNearestYEdge = Mathf.Min(centeredY, TextureSize - centeredY);

            if (distanceToNearestXEdge >= CornerRadius || distanceToNearestYEdge >= CornerRadius)
            {
                return 1f;
            }

            float distFromInnerCornerX = CornerRadius - distanceToNearestXEdge;
            float distFromInnerCornerY = CornerRadius - distanceToNearestYEdge;
            float distanceToCornerCenter = Mathf.Sqrt(
                distFromInnerCornerX * distFromInnerCornerX +
                distFromInnerCornerY * distFromInnerCornerY
            );

            return Mathf.Clamp01(CornerRadius - distanceToCornerCenter + 0.5f);
        }
    }
}
