using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
    private TextMeshProUGUI strip;
    private TextMeshProUGUI copyLabel;
    private Image copyBack;
    private Rect copyTarget;
    private bool copyShown;

    private static readonly Vector3[] Corners = new Vector3[4];
    private static readonly Color CopyRest = new(1f, 1f, 1f, 0.08f);
    private static readonly Color CopyHover = new(0.45f, 0.86f, 0.95f, 0.3f);

    private const string RunAgain = "press SPACE to run again";

    private float visibleAt = -1f;
    private float alpha;

    // PlayerTrackMovement holds the reset until this clears, so the run waits for you.
    public bool WaitingForInput { get; private set; }

    // A daily's result to paste, or null when there's nothing to compare against.
    public string Share { get; private set; }
    public bool Copied { get; private set; }

    // Screen pixels, so a test can click it and the page can copy on it.
    public Rect CopyButtonRect { get; private set; }

    public override void Init()
    {
        Build();
    }

    // Not for restarts. The story only stops to mark getting out of a place with laps.
    protected override void OnDeath(OnDeathEvent evt)
    {
        if (evt.Outcome == RunOutcome.Abandoned)
        {
            return;
        }

        if (Presentation.Arcade)
        {
            Show(evt);
            return;
        }

        var laps = StoryLaps.Current;
        var place = Places.ForRun();
        if (evt.Outcome == RunOutcome.Banked && laps != null && place != null)
        {
            ShowPlace(place, laps.History);
        }
    }

    private void ShowPlace(Place place, System.Collections.Generic.IReadOnlyList<bool> calls)
    {
        if (headline == null)
        {
            return;
        }

        var right = 0;
        for (var i = 0; i < calls.Count; i++)
        {
            right += calls[i] ? 1 : 0;
        }

        headline.text = $"OUT OF {place.Short.ToUpperInvariant()}";
        headline.color = RuntimeUi.Accent;
        score.text = string.Empty;
        detail.text = $"{calls.Count} laps     {right} right     {calls.Count - right} wrong";
        strip.text = ShareLine.PlacePreview(calls);
        best.text = string.Empty;

        Share = ShareLine.ForPlace(place.Short, calls);
        Copied = false;
        ShareClipboard.Arm(Share);
        copyLabel.text = "COPY RESULT";
        copyTarget = default;
        prompt.text = "press SPACE to go on";

        visibleAt = -1f;
        WaitingForInput = true;
    }

    public void Dismiss()
    {
        WaitingForInput = false;
        Share = null;
        ShareClipboard.Disarm();
    }

    // Or the page would still copy a finished run's line on a click there.
    protected override void OnDestroy()
    {
        base.OnDestroy();
        ShareClipboard.Disarm();
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
        ShowCopy(ready && Share != null);

        if (!ready)
        {
            return;
        }

        var source = InputRouter.Source;
        if (Share != null)
        {
            UpdateCopy(source);
        }

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

        Share = daily
            ? ShareLine.For(RunMode.Day, Places.ForRun()?.Short, evt.Outcome, evt.DistanceCovered, run, kept,
                PlayerTrackMovement.Strip.Marks)
            : null;
        Copied = false;

        if (Share != null)
        {
            ShareClipboard.Arm(Share);
        }

        prompt.text = RunAgain;
        strip.text = Share != null ? ShareLine.Preview(evt.DistanceCovered, PlayerTrackMovement.Strip.Marks) : string.Empty;
        copyLabel.text = "COPY RESULT";
        copyTarget = default;

        visibleAt = -1f;
        WaitingForInput = true;
    }

    private void UpdateCopy(IInputSource source)
    {
        copyLabel.rectTransform.GetWorldCorners(Corners);
        CopyButtonRect = Rect.MinMaxRect(Corners[0].x, Corners[0].y, Corners[2].x, Corners[2].y);
        if (CopyButtonRect != copyTarget)
        {
            copyTarget = CopyButtonRect;
            ShareClipboard.Target(copyTarget);
        }

        var over = source != null && CopyButtonRect.Contains(source.Pointer);
        copyBack.color = over ? CopyHover : CopyRest;

        if (over && source.PointerPressed)
        {
            ShareClipboard.Copy();
        }

        if (ShareClipboard.TakeCopied())
        {
            Copied = true;
            copyLabel.text = "COPIED";
        }
    }

    // On change only, as SetActive isn't free every frame.
    private void ShowCopy(bool show)
    {
        if (show == copyShown)
        {
            return;
        }

        copyShown = show;
        copyLabel.gameObject.SetActive(show);
        copyBack.gameObject.SetActive(show);
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
        strip = column.Add("Strip", RuntimeUi.Body, RuntimeUi.Muted, 0f, 6f);
        strip.richText = true;
        column.Space(16f);
        best = column.Add("Best", RuntimeUi.Body, RuntimeUi.Muted);
        column.Space(20f);
        copyLabel = column.Add("Copy", RuntimeUi.Caption, RuntimeUi.Ink, 0.1f, 6f);
        BuildCopyBack(holder.transform);
        column.Space(28f);
        prompt = column.Add("Prompt", RuntimeUi.Caption, RuntimeUi.Muted);

        copyShown = true;
        ShowCopy(false);
    }

    // A button's width, and drawn behind the label rather than over it.
    private void BuildCopyBack(Transform parent)
    {
        var label = copyLabel.rectTransform;
        label.sizeDelta = new Vector2(300f, label.sizeDelta.y + 16f);

        var go = new GameObject("CopyBack");
        go.transform.SetParent(parent, false);
        go.transform.SetSiblingIndex(label.GetSiblingIndex());

        copyBack = go.AddComponent<Image>();
        copyBack.color = CopyRest;
        copyBack.raycastTarget = false;

        var back = copyBack.rectTransform;
        back.anchorMin = label.anchorMin;
        back.anchorMax = label.anchorMax;
        back.pivot = label.pivot;
        back.sizeDelta = label.sizeDelta;
        back.anchoredPosition = label.anchoredPosition;
    }
}
