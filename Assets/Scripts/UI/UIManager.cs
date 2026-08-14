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

    private void Start()
    {
        gameManager = GameManager.GetInstance();

        // The prefab ships with these unassigned; the authored labels only exist in
        // MovementTestScene, so anything else needs a HUD building at runtime.
        if (distanceText == null || speedText == null || highScoreText == null)
        {
            BuildHud();
        }

        highScoreText.gameObject.SetActive(gameManager.HighScore > 0);
    }

    private void Update()
    {
        if (distanceText == null || speedText == null)
        {
            return;
        }

        // SetText with args formats into TMP's own buffer, string interpolation allocates every frame
        distanceText.SetText("Distance covered: {0:1}", PlayerTrackMovement.DistanceCovered);
        speedText.SetText("Speed: {0:1}", PlayerTrackMovement.CurrentSpeed);
    }

    protected override void OnDataUpdated()
    {
        if (highScoreText != null && gameManager != null && gameManager.HighScore > 0)
        {
            highScoreText.gameObject.SetActive(true);
            highScoreText.SetText("High score: {0:1}", gameManager.HighScore);
        }
    }

    private void BuildHud()
    {
        var canvas = RuntimeUi.CreateCanvas("HudCanvas", 90);
        var topLeft = new Vector2(0f, 1f);
        var size = new Vector2(700f, 60f);

        distanceText ??= RuntimeUi.CreateText(canvas.transform, "Distance", topLeft, new Vector2(30f, -30f), size, 34f, TextAlignmentOptions.TopLeft);
        speedText ??= RuntimeUi.CreateText(canvas.transform, "Speed", topLeft, new Vector2(30f, -80f), size, 34f, TextAlignmentOptions.TopLeft);
        highScoreText ??= RuntimeUi.CreateText(canvas.transform, "HighScore", topLeft, new Vector2(30f, -130f), size, 34f, TextAlignmentOptions.TopLeft);
    }
}
