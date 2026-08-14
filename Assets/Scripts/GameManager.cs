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
        base.Awake(); // sets the singleton instance and runs the duplicate guard

        // Losing the asset reference shouldn't be an instant null ref, and a scene
        // built at runtime has no asset to point at.
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<GameData>();
        }

        data.ResetToDefaults();
    }

    protected override void OnDeath(OnDeathEvent evt)
    {
        if (evt.DistanceCovered > data.HighScore)
        {
            data.HighScore = evt.DistanceCovered;
            ToastManager.GetInstance().Show($"New high score  {data.HighScore:F0}m");
            EventService.Dispatch<OnDataUpdatedEvent>();
        }
    }
}
