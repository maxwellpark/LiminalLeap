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
}
