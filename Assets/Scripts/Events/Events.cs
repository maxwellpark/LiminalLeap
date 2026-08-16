namespace Events
{
    public class OnDeathEvent : IEvent
    {
        public EventType Type => EventType.Death;
        public float DistanceCovered { get; }
        public bool Completed { get; }

        public OnDeathEvent(float distance, bool completed = false)
        {
            DistanceCovered = distance;
            Completed = completed;
        }
    }

    public class OnSpawnEvent : IEvent
    {
        public EventType Type => EventType.Spawn;
    }

    public class OnDataUpdatedEvent : IEvent
    {
        public EventType Type => EventType.DataUpdated;
    }
}
