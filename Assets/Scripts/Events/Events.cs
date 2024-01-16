namespace Events
{
    public class OnDeathEvent : IEvent
    {
        public EventType Type => EventType.Death;
        public float DistanceCovered { get; }
        public OnDeathEvent(float distance)
        {
            DistanceCovered = distance;
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
