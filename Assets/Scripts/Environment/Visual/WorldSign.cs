using TMPro;
using UnityEngine;

// Text painted into the world rather than over it. Liminal spaces are all institutional
// signage, so the controls can be floor markings instead of a tutorial overlay.
public static class WorldSign
{
    public static TextMeshPro Floor(Transform parent, string text, Vector3 localPosition, float size, Color colour)
    {
        var go = new GameObject("Sign_" + text);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        // Flat on the deck, reading away from the player as they run onto it.
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        var label = go.AddComponent<TextMeshPro>();
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = colour;
        label.characterSpacing = 12f;
        label.enableWordWrapping = false;

        var rect = label.rectTransform;
        rect.sizeDelta = new Vector2(14f, 3f);

        return label;
    }
}
