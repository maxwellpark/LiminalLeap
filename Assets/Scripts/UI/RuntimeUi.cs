using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Managers build their own UI when nothing is wired, so generated scenes work.
public static class RuntimeUi
{
    // One scale rather than numbers picked per call site, which is why sizes looked arbitrary.
    public const float Display = 96f;
    public const float Headline = 52f;
    public const float Body = 32f;
    public const float Caption = 24f;

    public static readonly Color Ink = new(0.92f, 0.93f, 0.95f, 1f);
    public static readonly Color Muted = new(0.62f, 0.65f, 0.7f, 1f);
    public static readonly Color Accent = new(0.45f, 0.86f, 0.95f, 1f);

    // Outline and a touch of tracking, or light text vanishes against a bright surface.
    public static TextMeshProUGUI Style(TextMeshProUGUI text, Color colour, float outline = 0.18f, float tracking = 4f)
    {
        text.color = colour;
        text.characterSpacing = tracking;
        text.fontStyle = FontStyles.Normal;
        text.outlineWidth = outline;
        text.outlineColor = new Color32(0, 0, 0, 190);
        return text;
    }

    public static UiPop AddPop(TextMeshProUGUI text)
    {
        return text.gameObject.AddComponent<UiPop>();
    }

    // Stacks rows by their own height. Hand-placed offsets silently overlapped the moment
    // a font size changed, which is exactly what happened when score went to 96pt.
    public class Column
    {
        private const float LineFactor = 1.3f;

        private readonly Transform parent;
        private readonly Vector2 anchor;
        private readonly TextAlignmentOptions alignment;
        private readonly float x;
        private readonly float width;
        private readonly float gap;
        private float y;

        public Column(Transform parent, Vector2 anchor, TextAlignmentOptions alignment,
            float x, float top, float width, float gap = 8f)
        {
            this.parent = parent;
            this.anchor = anchor;
            this.alignment = alignment;
            this.x = x;
            this.width = width;
            this.gap = gap;
            y = top;
        }

        public TextMeshProUGUI Add(string name, float size, Color colour, float outline = 0.18f, float tracking = 4f)
        {
            var height = size * LineFactor;
            var text = CreateText(parent, name, anchor, new Vector2(x, y), new Vector2(width, height), size, alignment);
            y -= height + gap;
            return Style(text, colour, outline, tracking);
        }

        public void Space(float pixels)
        {
            y -= pixels;
        }
    }

    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        return canvas;
    }

    public static Image CreateFullScreenImage(Transform parent, string name, Color colour)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var image = go.AddComponent<Image>();
        image.color = colour;
        image.raycastTarget = false;

        var rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return image;
    }

    public static TextMeshProUGUI CreateText(
        Transform parent, string name, Vector2 anchor, Vector2 offset, Vector2 size,
        float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.alignment = alignment;
        text.fontSize = fontSize;
        text.raycastTarget = false;

        var rect = text.rectTransform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = offset;

        return text;
    }
}
