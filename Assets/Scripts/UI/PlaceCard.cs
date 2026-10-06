using TMPro;
using UnityEngine;

// The place's name and a line as you arrive. Unscaled, so it still plays out over a pause.
public class PlaceCard : Singleton<PlaceCard>
{
    [SerializeField] private float fadeIn = 0.6f;
    [SerializeField] private float hold = 2.6f;
    [SerializeField] private float fadeOut = 1.2f;

    private CanvasGroup group;
    private TextMeshProUGUI heading;
    private TextMeshProUGUI arrival;
    private float shownAt = -999f;

    public override void Init()
    {
        Build();
    }

    public void Show(Place place)
    {
        if (place == null || heading == null)
        {
            return;
        }

        heading.text = place.Name.ToUpperInvariant();
        arrival.text = place.Arrival;
        shownAt = Time.unscaledTime;
    }

    private void Update()
    {
        if (group == null)
        {
            return;
        }

        var t = Time.unscaledTime - shownAt;
        group.alpha = t < fadeIn
            ? Mathf.Clamp01(t / fadeIn)
            : Mathf.Clamp01(1f - (t - fadeIn - hold) / fadeOut);
    }

    private void Build()
    {
        // Above the toasts, under the fade and the summary.
        var canvas = RuntimeUi.CreateCanvas("PlaceCardCanvas", 105);

        var holder = new GameObject("PlaceCard");
        holder.transform.SetParent(canvas.transform, false);
        group = holder.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        var rect = holder.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.62f);
        rect.anchorMax = new Vector2(0.5f, 0.62f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1400f, 200f);

        var column = new RuntimeUi.Column(
            holder.transform, new Vector2(0.5f, 1f), TextAlignmentOptions.Center, 0f, 0f, 1400f, 10f);

        heading = column.Add("Heading", RuntimeUi.Headline, RuntimeUi.Ink, 0.22f, 10f);
        arrival = column.Add("Arrival", RuntimeUi.Body, RuntimeUi.Muted);
    }
}
