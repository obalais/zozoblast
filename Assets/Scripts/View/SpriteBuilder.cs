using UnityEngine;

namespace Blockblast.View
{
    public static class SpriteBuilder
    {
        public static Sprite BuildFromPixels(int size, Color[] pixels, Vector4 slicedBorder)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.SetPixels(pixels);
            texture.Apply();

            Sprite sprite;
            if (slicedBorder == Vector4.zero)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size
                );
            }
            else
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size,
                    0,
                    SpriteMeshType.FullRect,
                    slicedBorder
                );
            }
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
