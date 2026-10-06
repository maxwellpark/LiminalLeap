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

        // Runtime scenes have no asset to reference.
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<GameData>();
        }

        // The save is the source of truth for the best score.
        data.HighScore = SaveStore.Data.HighScore;
    }

    protected override void OnDeath(OnDeathEvent evt)
    {
        // Restarts aren't results, so they stay out of the stats.
        if (evt.Outcome == RunOutcome.Abandoned)
        {
            return;
        }

        // Dying only costs the score when there was a way to bank it.
        var kept = evt.Outcome != RunOutcome.Died || !Features.On(Feature.ExitDoors);
        var score = kept ? PlayerTrackMovement.Score : 0f;

        SaveStore.Data.RecordRun(
            score, evt.DistanceCovered, evt.Outcome, Features.VariantKey());

        if (RunMode.Daily && !string.IsNullOrEmpty(RunMode.Day))
        {
            SaveStore.Data.Daily(RunMode.Day).Record(score, evt.DistanceCovered);
        }

        SaveStore.Save();

        data.HighScore = SaveStore.Data.HighScore;

        EventService.Dispatch<OnDataUpdatedEvent>();
    }
}
