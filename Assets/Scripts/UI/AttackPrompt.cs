using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The written half of the attack warning. Audio alone means the game is unplayable muted,
// and a cue you can only hear is a cue plenty of people never get.
//
// Deliberately never names the lane. It tells you something is coming and roughly when,
// which is exactly what the audio says. The lane stays in the mirror.
public class AttackPrompt : Singleton<AttackPrompt>
{
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float barWidth = 420f;

    private static readonly Color Charging = new(0.62f, 0.82f, 1f);
    private static readonly Color Locked = new(1f, 0.72f, 0.32f);

    private CanvasGroup group;
    private TextMeshProUGUI headline;
    private TextMeshProUGUI hint;
    private RectTransform fill;
    private Image fillImage;
    private Pursuer pursuer;
    private float shown;

    public override void Init()
    {
        Build();
    }

    private void Update()
    {
        if (group == null)
        {
            return;
        }

        pursuer = pursuer != null ? pursuer : Pursuer.Instance;
        var model = pursuer != null ? pursuer.Attack : null;
        var live = model != null && Features.On(Feature.PursuerAttacks) && model.InFlight;

        shown = Mathf.MoveTowards(shown, live ? 1f : 0f, fadeSpeed * Time.deltaTime);

        // Already looking, so stop shouting and just leave the timing bar.
        var looking = RearView.Instance != null && RearView.Instance.IsRaised;
        group.alpha = shown * (looking ? 0.45f : 1f);

        if (!live)
        {
            return;
        }

        var locked = model.Phase is AttackPhase.Locked or AttackPhase.Fire;
        var colour = locked ? Locked : Charging;

        headline.text = locked ? "INCOMING" : "BEHIND YOU";
        headline.color = colour;
        hint.text = looking ? "read the lane" : "hold SHIFT to look";

        // Drains toward the moment it lands, so timing is readable without the audio.
        var lead = Mathf.Max(0.01f, pursuer.AttackConfig.LeadTime - pursuer.AttackConfig.FireDuration);
        var remaining = Mathf.Clamp01(model.TimeUntilFire / lead);

        fill.sizeDelta = new Vector2(barWidth * remaining, fill.sizeDelta.y);
        fillImage.color = colour;
    }

    private void Build()
    {
        var canvas = RuntimeUi.CreateCanvas("AttackPromptCanvas", 95);

        var holder = new GameObject("AttackPrompt");
        holder.transform.SetParent(canvas.transform, false);
        group = holder.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        var rect = holder.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 150f);
        rect.sizeDelta = new Vector2(barWidth, 160f);

        var column = new RuntimeUi.Column(
            holder.transform, new Vector2(0.5f, 1f), TextAlignmentOptions.Center, 0f, 0f, barWidth);

        headline = column.Add("Headline", RuntimeUi.Headline, Charging, 0.22f, 10f);
        hint = column.Add("Hint", RuntimeUi.Caption, RuntimeUi.Muted);

        BuildBar(holder.transform);
    }

    private void BuildBar(Transform parent)
    {
        var track = new GameObject("BarTrack").AddComponent<Image>();
        track.transform.SetParent(parent, false);
        track.color = new Color(0.05f, 0.05f, 0.07f, 0.7f);
        track.raycastTarget = false;
        Place(track.rectTransform, barWidth, 8f);

        fillImage = new GameObject("BarFill").AddComponent<Image>();
        fillImage.transform.SetParent(parent, false);
        fillImage.color = Charging;
        fillImage.raycastTarget = false;

        fill = fillImage.rectTransform;
        Place(fill, barWidth, 8f);

        // Drains from the middle outward would read as decoration, so pin it left.
        fill.pivot = new Vector2(0f, 1f);
        fill.anchorMin = new Vector2(0.5f, 1f);
        fill.anchorMax = new Vector2(0.5f, 1f);
        fill.anchoredPosition = new Vector2(-barWidth * 0.5f, -132f);
    }

    private static void Place(RectTransform rect, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(0f, -132f);
    }
}
