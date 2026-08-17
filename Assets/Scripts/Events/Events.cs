namespace Events
{
    public class OnDeathEvent : IEvent
    {
        public EventType Type => EventType.Death;
        public float DistanceCovered { get; }
        public RunOutcome Outcome { get; }

        // Kept so existing subscribers don't all have to learn about outcomes at once.
        public bool Completed => Outcome != RunOutcome.Died;

        public OnDeathEvent(float distance, bool completed = false)
            : this(distance, completed ? RunOutcome.Completed : RunOutcome.Died)
        {
        }

        public OnDeathEvent(float distance, RunOutcome outcome)
        {
            DistanceCovered = distance;
            Outcome = outcome;
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

    // Raised when the beam misses. Score and later Flow hang off this, never off the mirror.
    public class OnAttackDodgedEvent : IEvent
    {
        public EventType Type => EventType.AttackDodged;
        public AttackLane Lane { get; }
        public float PlayerLane { get; }

        public OnAttackDodgedEvent(AttackLane lane, float playerLane)
        {
            Lane = lane;
            PlayerLane = playerLane;
        }
    }

    public class OnAttackHitEvent : IEvent
    {
        public EventType Type => EventType.AttackHit;
        public AttackLane Lane { get; }

        public OnAttackHitEvent(AttackLane lane)
        {
            Lane = lane;
        }
    }
}
