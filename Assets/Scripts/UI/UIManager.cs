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
        highScoreText.gameObject.SetActive(gameManager.HighScore > 0);
    }

    private void Update()
    {
        // SetText with args formats into TMP's own buffer, string interpolation allocates every frame
        distanceText.SetText("Distance covered: {0:1}", PlayerTrackMovement.DistanceCovered);
        speedText.SetText("Speed: {0:1}", PlayerTrackMovement.CurrentSpeed);
    }

    protected override void OnDataUpdated()
    {
        if (gameManager.HighScore > 0)
        {
            highScoreText.gameObject.SetActive(true);
            highScoreText.text = $"High score: {gameManager.HighScore:F1}";
        }
    }
}
