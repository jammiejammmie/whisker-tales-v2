using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Shared visual constants and small UI factory helpers for Whisker screens.
    /// Warm sunset palette per spec; no neon, no pure white, no hard shadows.
    /// </summary>
    public static class WhiskerTheme
    {
        public const string FontPath = "NanumMyeongjo-Regular";

        public static readonly Color Cream      = new Color(0.97f, 0.93f, 0.85f, 1f);
        public static readonly Color WarmBrown  = new Color(0.45f, 0.31f, 0.22f, 1f);
        public static readonly Color SoftOrange = new Color(0.96f, 0.74f, 0.50f, 1f);
        public static readonly Color DeepBrown  = new Color(0.27f, 0.18f, 0.13f, 1f);
        public static readonly Color SunsetPink = new Color(0.95f, 0.66f, 0.56f, 1f);
        public static readonly Color PanelTint  = new Color(0.99f, 0.93f, 0.82f, 0.92f);
        public static readonly Color BoardPaper = new Color(0.96f, 0.90f, 0.76f, 0.95f);
        public static readonly Color NavBg      = new Color(0.22f, 0.15f, 0.10f, 0.88f);
        public static readonly Color SleepDim   = new Color(0.02f, 0.02f, 0.04f, 0.80f);
        public static readonly Color HeartPink  = new Color(0.93f, 0.45f, 0.55f, 1f);
        public static readonly Color AffinityBg = new Color(0.30f, 0.18f, 0.16f, 0.6f);

        private static TMP_FontAsset _cachedFont;

        public static TMP_FontAsset Font
        {
            get
            {
                if (_cachedFont != null) return _cachedFont;
                var ttf = Resources.Load<Font>(FontPath);
                if (ttf == null) ttf = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _cachedFont = TMP_FontAsset.CreateFontAsset(ttf);
                _cachedFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                return _cachedFont;
            }
        }

        public static GameObject MakePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return go;
        }

        public static GameObject MakeImage(Transform parent, string name, Color color, Sprite sprite = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = false;
            return go;
        }

        public static TextMeshProUGUI MakeText(Transform parent, string name, string text,
            int size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static GameObject MakeCircle(Transform parent, string name, float diameter, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(diameter, diameter);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.sprite = CircleSprite;
            img.raycastTarget = false;
            return go;
        }

        private static Sprite _circleSprite;
        public static Sprite CircleSprite
        {
            get
            {
                if (_circleSprite != null) return _circleSprite;
                const int size = 128;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var px = new Color[size * size];
                float r = size * 0.5f - 1f;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - size * 0.5f + 0.5f;
                        float dy = y - size * 0.5f + 0.5f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = Mathf.Clamp01(r - d);
                        px[y * size + x] = new Color(1f, 1f, 1f, a);
                    }
                }
                tex.SetPixels(px);
                tex.Apply();
                tex.wrapMode = TextureWrapMode.Clamp;
                _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
                _circleSprite.name = "WhiskerCircle";
                return _circleSprite;
            }
        }

        private static Sprite _roundedSprite;
        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite != null) return _roundedSprite;
                const int size = 128;
                const int radius = 24;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var px = new Color[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float a = 1f;
                        int dx = 0, dy = 0;
                        if (x < radius) dx = radius - x;
                        else if (x >= size - radius) dx = x - (size - radius - 1);
                        if (y < radius) dy = radius - y;
                        else if (y >= size - radius) dy = y - (size - radius - 1);
                        if (dx > 0 && dy > 0)
                        {
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            a = Mathf.Clamp01(radius - d);
                        }
                        px[y * size + x] = new Color(1f, 1f, 1f, a);
                    }
                }
                tex.SetPixels(px);
                tex.Apply();
                _roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
                _roundedSprite.name = "WhiskerRounded";
                return _roundedSprite;
            }
        }

        public static void Anchor(RectTransform rt, Vector2 min, Vector2 max,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
