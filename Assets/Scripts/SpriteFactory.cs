using UnityEngine;

namespace TrickyParcels
{
    // Generates one shared plain white square sprite at runtime, so the whole
    // prototype can ship with zero imported art (tinted per tile/package type instead).
    public static class SpriteFactory
    {
        private static Sprite _square;

        public static Sprite Square()
        {
            if (_square != null) return _square;

            var tex = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            _square = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _square;
        }
    }
}
