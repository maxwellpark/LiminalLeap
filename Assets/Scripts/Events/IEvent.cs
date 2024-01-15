namespace Events
{
    public interface IEvent
    {
        EventType Type { get; }
    }

    public enum EventType
    {
        Death,
    }
}
