using Events;
using UnityEngine;
using EventType = Events.EventType;

public class GameManager : Singleton<GameManager>
{
    protected override EventType[] EventTypes => new[] { EventType.Death };

    [SerializeField]
    private GameData data;
    public float HighScore => data.HighScore;

    private static readonly EventService eventService = new();
    public static EventService EventService => eventService;

    protected override void Awake()
    {
        data.ResetToDefaults();
    }

    protected override void OnDeath(OnDeathEvent evt)
    {
        if (evt.DistanceCovered > data.HighScore)
        {
            data.HighScore = evt.DistanceCovered;
            Debug.Log("New high score: " + data.HighScore);
        }
    }
}
