using System.Collections.Generic;
using Events;
using TMPro;
using UnityEngine;
using EventType = Events.EventType;

// Transient one-line messages. Self-provisioning like AudioManager, so generated
// scenes and MovementTestScene both get toasts without being edited.
public class ToastManager : Singleton<ToastManager>
{
    protected override EventType[] EventTypes => new[] { EventType.Death };

    [SerializeField] private TMP_Text label;
    [SerializeField] private float holdSeconds = 1.6f;
    [SerializeField] private float fadeSeconds = 0.45f;
    [SerializeField] private int maxQueued = 3;

    private readonly Queue<string> pending = new();
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

    public void Show(string message)
    {
        // A backlog of stale toasts is worse than dropping some.
        if (pending.Count >= maxQueued)
        {
            pending.Dequeue();
        }

        pending.Enqueue(message);
    }

    private void Update()
    {
        if (label == null)
        {
            return;
        }

        if (!showing)
        {
            if (pending.Count == 0)
            {
                return;
            }

            label.text = pending.Dequeue();
            elapsed = 0f;
            showing = true;
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

    protected override void OnDeath(OnDeathEvent evt)
    {
        Show($"Run ended  {evt.DistanceCovered:F0}m");
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
