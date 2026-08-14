using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Managers build their own UI when nothing is wired in the scene, so generated
// scenes and headless runs work without a hand-authored canvas.
public static class RuntimeUi
{
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
