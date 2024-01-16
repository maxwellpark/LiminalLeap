using TMPro;
using UnityEngine;
using EventType = Events.EventType;

public class UIManager : Singleton<UIManager>
{
    protected override EventType[] EventTypes => new[] { EventType.DataUpdated, };
    private GameManager gameManager;

    [SerializeField]
    private TMP_Text distanceText;

    [SerializeField]
    private TMP_Text highScoreText;

    private void Start()
    {
        gameManager = GameManager.GetInstance();
        highScoreText.gameObject.SetActive(gameManager.HighScore > 0);
    }

    private void Update()
    {
        distanceText.text = $"Distance covered: {PlayerMovement.DistanceCovered:F1}";
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
