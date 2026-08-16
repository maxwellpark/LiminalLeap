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

        // A lost asset ref shouldn't be an instant null ref, and runtime scenes have none.
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<GameData>();
        }

        // Was ResetToDefaults, which wiped the high score every launch and made the
        // BEST readout decorative. The save is the source of truth now.
        data.HighScore = SaveStore.Data.HighScore;
    }

    protected override void OnDeath(OnDeathEvent evt)
    {
        var improved = SaveStore.Data.RecordRun(PlayerTrackMovement.Score, evt.DistanceCovered);
        SaveStore.Save();

        data.HighScore = SaveStore.Data.HighScore;

        if (improved)
        {
            ToastManager.GetInstance().Show($"New best  {data.HighScore:F0}");
        }

        EventService.Dispatch<OnDataUpdatedEvent>();
    }
}
