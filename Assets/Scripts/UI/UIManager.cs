using TMPro;
using UnityEngine;
using EventType = Events.EventType;

public class UIManager : Singleton<UIManager>
{
    protected override EventType[] EventTypes => new[] { EventType.DataUpdated, };
    private GameManager gameManager;

    [SerializeField] private TMP_Text distanceText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text multiplierText;

    // Score ticks up every frame, so pop on milestones rather than on every change.
    [SerializeField] private float scorePopEvery = 250f;
    [SerializeField] private float multiplierPopStep = 0.15f;

    private UiPop scorePop;
    private UiPop multiplierPop;
    private UiPop highScorePop;
    private float nextScorePop;
    private float lastMultiplier = 1f;

    private void Start()
    {
        gameManager = GameManager.GetInstance();

        // Prefab ships these unassigned, so anything but MovementTestScene builds its own.
        if (distanceText == null || speedText == null || highScoreText == null
            || scoreText == null || multiplierText == null)
        {
            BuildHud();
        }

        highScoreText.gameObject.SetActive(gameManager.HighScore > 0);
        nextScorePop = scorePopEvery;
    }

    private void Update()
    {
        if (distanceText == null || speedText == null)
        {
            return;
        }

        // SetText with args formats into TMP's own buffer, string interpolation allocates every frame
        distanceText.SetText("{0:1} m", PlayerTrackMovement.DistanceCovered);
        speedText.SetText("{0:1} u/s", PlayerTrackMovement.CurrentSpeed);

        if (scoreText != null)
        {
            scoreText.SetText("{0:0}", PlayerTrackMovement.Score);
            TrackScoreMilestones();
        }

        if (multiplierText != null)
        {
            multiplierText.SetText("x{0:2}", PlayerTrackMovement.Multiplier);
            TrackMultiplierJumps();
        }
    }

    private void TrackScoreMilestones()
    {
        var score = PlayerTrackMovement.Score;

        if (score < nextScorePop - scorePopEvery)
        {
            nextScorePop = scorePopEvery; // the run reset under us
            return;
        }

        if (score >= nextScorePop)
        {
            nextScorePop += scorePopEvery;
            scorePop?.Punch();
        }
    }

    private void TrackMultiplierJumps()
    {
        var multiplier = PlayerTrackMovement.Multiplier;

        // Only the discrete near-miss jumps, not the smooth climb with speed.
        if (multiplier > lastMultiplier + multiplierPopStep)
        {
            multiplierPop?.Punch();
        }

        lastMultiplier = multiplier;
    }

    protected override void OnDataUpdated()
    {
        if (highScoreText != null && gameManager != null && gameManager.HighScore > 0)
        {
            highScoreText.gameObject.SetActive(true);
            highScoreText.SetText("BEST {0:0}", gameManager.HighScore);
            highScorePop?.Punch();
        }
    }

    private void BuildHud()
    {
        var canvas = RuntimeUi.CreateCanvas("HudCanvas", 90);
        var topLeft = new Vector2(0f, 1f);
        var topRight = new Vector2(1f, 1f);
        var row = new Vector2(520f, 60f);

        speedText ??= RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Speed", topLeft, new Vector2(40f, -36f), row, RuntimeUi.Headline, TextAlignmentOptions.TopLeft),
            RuntimeUi.Ink);

        distanceText ??= RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Distance", topLeft, new Vector2(40f, -100f), row, RuntimeUi.Body, TextAlignmentOptions.TopLeft),
            RuntimeUi.Muted);

        scoreText ??= RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Score", topRight, new Vector2(-40f, -36f), row, RuntimeUi.Display, TextAlignmentOptions.TopRight),
            RuntimeUi.Ink, 0.24f, 2f);

        multiplierText ??= RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "Multiplier", topRight, new Vector2(-40f, -146f), row, RuntimeUi.Headline, TextAlignmentOptions.TopRight),
            RuntimeUi.Accent);

        highScoreText ??= RuntimeUi.Style(
            RuntimeUi.CreateText(canvas.transform, "HighScore", topRight, new Vector2(-40f, -206f), row, RuntimeUi.Caption, TextAlignmentOptions.TopRight),
            RuntimeUi.Muted);

        scorePop = RuntimeUi.AddPop((TextMeshProUGUI)scoreText);
        multiplierPop = RuntimeUi.AddPop((TextMeshProUGUI)multiplierText);
        highScorePop = RuntimeUi.AddPop((TextMeshProUGUI)highScoreText);
    }
}
