using Events;
using TMPro;
using UnityEngine;
using EventType = Events.EventType;

// Score, best and one key to go again. Sorts above the fade.
public class RunSummary : Singleton<RunSummary>
{
    protected override EventType[] EventTypes => new[] { EventType.Death };

    [SerializeField] private float minimumSeconds = 0.6f; // once fully shown, so a panic jump can't skip it
    [SerializeField] private float fadeSpeed = 6f;

    private CanvasGroup group;
    private TextMeshProUGUI headline;
    private TextMeshProUGUI score;
    private TextMeshProUGUI detail;
    private TextMeshProUGUI best;
    private TextMeshProUGUI prompt;

    private float visibleAt = -1f;
    private float alpha;

    // PlayerTrackMovement holds the reset until this clears, so the run waits for you.
    public bool WaitingForInput { get; private set; }

    public override void Init()
    {
        Build();
    }

    // Not for restarts, which go straight back in, nor in the story, which just carries on.
    protected override void OnDeath(OnDeathEvent evt)
    {
        if (evt.Outcome != RunOutcome.Abandoned && Presentation.Arcade)
        {
            Show(evt);
        }
    }

    public void Dismiss()
    {
        WaitingForInput = false;
    }

    private void Update()
    {
        if (group == null)
        {
            return;
        }

        alpha = Mathf.MoveTowards(alpha, WaitingForInput ? 1f : 0f, fadeSpeed * Time.unscaledDeltaTime);
        group.alpha = alpha;

        // From fully visible, or a panic jump skips it.
        if (WaitingForInput && alpha >= 1f && visibleAt < 0f)
        {
            visibleAt = Time.unscaledTime;
        }

        var ready = WaitingForInput && visibleAt >= 0f && Time.unscaledTime - visibleAt >= minimumSeconds;

        // Only shown once a press will count.
        prompt.alpha = ready ? 1f : 0f;

        if (!ready)
        {
            return;
        }

        var source = InputRouter.Source;
        if (source != null && (source.JumpPressed || source.RestartPressed))
        {
            Dismiss();
        }
    }

    private void Show(OnDeathEvent evt)
    {
        if (headline == null)
        {
            return;
        }

        var run = PlayerTrackMovement.Score;

        // Dying only costs the score when there was a way to bank it.
        var banking = Features.On(Feature.ExitDoors);
        var kept = evt.Outcome != RunOutcome.Died || !banking;

        headline.text = evt.Outcome switch
        {
            RunOutcome.Banked => "BANKED",
            RunOutcome.Completed => "TRACK COMPLETE",
            _ => "CAUGHT",
        };

        headline.color = kept ? RuntimeUi.Accent : new Color(1f, 0.45f, 0.4f);
        score.text = $"{(kept ? run : 0f):N0}";
        score.color = kept ? RuntimeUi.Ink : new Color(0.55f, 0.57f, 0.62f);

        // Say what walking past the exit cost.
        detail.text = kept
            ? $"{evt.DistanceCovered:N0} m"
            : $"{evt.DistanceCovered:N0} m     lost {run:N0}";

        // A daily is measured against today's best.
        var daily = RunMode.Daily && !string.IsNullOrEmpty(RunMode.Day);
        var record = daily ? SaveStore.Data.Daily(RunMode.Day).BestScore : SaveStore.Data.HighScore;
        var beaten = kept && run > 0f && run >= record;

        headline.text += daily ? "   DAILY" : string.Empty;
        best.text = beaten
            ? (daily ? "BEST TODAY" : "NEW BEST")
            : $"{(daily ? "today" : "best")}  {record:N0}";
        best.color = beaten ? RuntimeUi.Accent : RuntimeUi.Muted;

        prompt.text = "press SPACE to run again";

        visibleAt = -1f;
        WaitingForInput = true;
    }

    private void Build()
    {
        // Above the fade at 200, or the panel would sit under the black wipe.
        var canvas = RuntimeUi.CreateCanvas("RunSummaryCanvas", 220);

        var holder = new GameObject("RunSummary");
        holder.transform.SetParent(canvas.transform, false);
        group = holder.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        var rect = holder.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 520f);

        var column = new RuntimeUi.Column(
            holder.transform, new Vector2(0.5f, 1f), TextAlignmentOptions.Center, 0f, 0f, 900f);

        headline = column.Add("Headline", RuntimeUi.Headline, RuntimeUi.Accent, 0.22f, 12f);
        score = column.Add("Score", RuntimeUi.Display, RuntimeUi.Ink, 0.2f, 2f);
        detail = column.Add("Detail", RuntimeUi.Body, RuntimeUi.Muted);
        column.Space(24f);
        best = column.Add("Best", RuntimeUi.Body, RuntimeUi.Muted);
        column.Space(28f);
        prompt = column.Add("Prompt", RuntimeUi.Caption, RuntimeUi.Muted);
    }
}
