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

    // Replaces rather than queues: at speed the queue dropped over half of them.
    public void Show(string message)
    {
        if (label != null)
        {
            label.text = message;
        }

        elapsed = 0f;
        showing = true;
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

    // RunSummary reports the run now, so a lingering near miss toast would just sit over it.
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
