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

        // A test run in the same editor session leaves the flags isolated otherwise.
        Features.UseStorage();

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
        // Dying costs you the score, but only once there was a way to bank it. Without
        // exits, death is the only ending, so taking the score away would just be a bug.
        var kept = evt.Outcome != RunOutcome.Died || !Features.On(Feature.ExitDoors);
        var score = kept ? PlayerTrackMovement.Score : 0f;

        var improved = SaveStore.Data.RecordRun(
            score, evt.DistanceCovered, evt.Outcome, Features.VariantKey());

        SaveStore.Save();

        data.HighScore = SaveStore.Data.HighScore;

        if (improved)
        {
            ToastManager.GetInstance().Show($"New best  {data.HighScore:F0}");
        }

        EventService.Dispatch<OnDataUpdatedEvent>();
    }
}
