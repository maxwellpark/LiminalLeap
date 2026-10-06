using Events;
using TMPro;
using UnityEngine;
using EventType = Events.EventType;

// Transient one-liners. Self-provisioning like the other managers.
public class ToastManager : Singleton<ToastManager>
{
    protected override EventType[] EventTypes => new[] { EventType.Death };

    [SerializeField] private TMP_Text label;
    [SerializeField] private float holdSeconds = 0.9f;
    [SerializeField] private float fadeSeconds = 0.35f;

    private float elapsed;
    private bool showing;

    public override void Init()
    {
        if (label == null)
        {
            label = BuildLabel();
        }

        SetAlpha(0f);
    }

    public string LastMessage { get; private set; }

    // Replaces rather than queues, which dropped most at speed.
    public void Show(string message)
    {
        LastMessage = message;

        if (label != null)
        {
            label.text = message;
        }

        elapsed = 0f;
        showing = true;
    }

    // Score, combos and rewards. Nothing in the story.
    public void ShowArcade(string message)
    {
        if (Presentation.Arcade)
        {
            Show(message);
        }
    }

    public void Clear()
    {
        showing = false;
        SetAlpha(0f);
    }

    private void Update()
    {
        if (label == null)
        {
            return;
        }

        if (!showing)
        {
            return;
        }

        elapsed += Time.deltaTime;

        var total = holdSeconds + fadeSeconds;
        if (elapsed >= total)
        {
            SetAlpha(0f);
            showing = false;
            return;
        }

        SetAlpha(elapsed <= holdSeconds ? 1f : 1f - (elapsed - holdSeconds) / fadeSeconds);
    }

    // The summary reports the run, so clear any toast.
    protected override void OnDeath(OnDeathEvent evt)
    {
        Clear();
    }

    private void SetAlpha(float a)
    {
        if (label != null)
        {
            var c = label.color;
            label.color = new Color(c.r, c.g, c.b, a);
        }
    }

    private static TMP_Text BuildLabel()
    {
        var canvas = RuntimeUi.CreateCanvas("ToastCanvas", 100);
        return RuntimeUi.CreateText(
            canvas.transform, "ToastText", new Vector2(0.5f, 0.72f), Vector2.zero,
            new Vector2(900f, 120f), 42f, TextAlignmentOptions.Center);
    }
}
