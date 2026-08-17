using System;

// What changed on this tick. Presentation and audio react to these, they don't poll state.
public struct AttackTick
{
    public bool Started;         // warning began, player can hear it
    public bool TelegraphReady;  // lane chosen, mirror can show it
    public bool Locked;          // target frozen, last chance to move
    public bool Fired;
    public bool Dodged;
    public bool Hit;
    public bool Aborted;         // no fair lane left, so it never fired
}

// Pure state machine for the thing behind you taking a swing. Deliberately knows nothing
// about the mirror: the mirror reads this, it can never change it.
public class PursuerAttackModel
{
    public const int AllLanes = 0b111;

    private readonly PursuerAttackConfig config;
    private readonly Random rng;

    private float phaseTime;
    private float idleWait;

    public AttackPhase Phase { get; private set; } = AttackPhase.Idle;
    public AttackLane TargetLane { get; private set; } = AttackLane.Centre;
    public float PhaseTime => phaseTime;

    // The lane is only readable once it has been chosen, which is the point of the mirror.
    public bool TargetVisible =>
        Phase is AttackPhase.Telegraph or AttackPhase.Locked or AttackPhase.Fire;

    // An attack is committed to a layout from the warning onward, so anything that rearranges
    // the track has to hold off until this is clear.
    public bool InFlight =>
        Phase is not AttackPhase.Idle and not AttackPhase.Cooldown;

    public PursuerAttackModel(PursuerAttackConfig config, int seed)
    {
        this.config = config ?? new PursuerAttackConfig();
        rng = new Random(seed);
        idleWait = NextInterval();
    }

    public void Reset()
    {
        Phase = AttackPhase.Idle;
        TargetLane = AttackLane.Centre;
        phaseTime = 0f;
        idleWait = NextInterval();
    }

    // allowedLanes is a bitmask of lanes it is fair to target. Zero means wait.
    public AttackTick Tick(float dt, float playerLane, int allowedLanes)
    {
        var result = default(AttackTick);

        if (dt <= 0f)
        {
            return result;
        }

        phaseTime += dt;

        switch (Phase)
        {
            case AttackPhase.Idle:
                // Holding here rather than firing blind is the whole fairness story.
                if (phaseTime >= idleWait && allowedLanes != 0)
                {
                    Enter(AttackPhase.Warning);
                    result.Started = true;
                }

                break;

            case AttackPhase.Warning:
                if (phaseTime >= config.WarningDuration)
                {
                    var lane = PickLane(allowedLanes);
                    if (lane < 0)
                    {
                        Enter(AttackPhase.Cooldown);
                        result.Aborted = true;
                        break;
                    }

                    TargetLane = (AttackLane)lane;
                    Enter(AttackPhase.Telegraph);
                    result.TelegraphReady = true;
                }

                break;

            case AttackPhase.Telegraph:
                if (phaseTime >= config.TelegraphDuration)
                {
                    Enter(AttackPhase.Locked);
                    result.Locked = true;
                }

                break;

            case AttackPhase.Locked:
                if (phaseTime >= config.LockDuration)
                {
                    Enter(AttackPhase.Fire);
                    result.Fired = true;
                }

                break;

            case AttackPhase.Fire:
                if (phaseTime >= config.FireDuration)
                {
                    var hit = Threatens(playerLane);
                    Enter(AttackPhase.Resolve);
                    result.Hit = hit;
                    result.Dodged = !hit;
                }

                break;

            case AttackPhase.Resolve:
                Enter(AttackPhase.Cooldown);
                break;

            case AttackPhase.Cooldown:
                if (phaseTime >= config.CooldownDuration)
                {
                    Enter(AttackPhase.Idle);
                    idleWait = NextInterval();
                }

                break;
        }

        return result;
    }

    public float LaneCentre(AttackLane lane)
    {
        return ((int)lane - 1) * config.LaneSpacing;
    }

    public bool Threatens(float playerLane)
    {
        var half = Math.Max(0f, config.LaneHalfWidth - config.DodgeTolerance);
        return Math.Abs(playerLane - LaneCentre(TargetLane)) < half;
    }

    // For the debug overlay. Zero outside the run up, since there is nothing pending.
    public float TimeUntilFire =>
        Phase switch
        {
            AttackPhase.Warning => config.WarningDuration - phaseTime + config.TelegraphDuration + config.LockDuration,
            AttackPhase.Telegraph => config.TelegraphDuration - phaseTime + config.LockDuration,
            AttackPhase.Locked => config.LockDuration - phaseTime,
            _ => 0f,
        };

    // Skips straight to a fired attack on the given lane, for F9 and for tests.
    public void ForceAttack(AttackLane lane)
    {
        TargetLane = lane;
        Enter(AttackPhase.Telegraph);
    }

    private void Enter(AttackPhase phase)
    {
        Phase = phase;
        phaseTime = 0f;
    }

    private float NextInterval()
    {
        var min = Math.Max(0f, config.MinAttackInterval);
        var max = Math.Max(min, config.MaxAttackInterval);
        return min + (float)rng.NextDouble() * (max - min);
    }

    private int PickLane(int allowed)
    {
        var total = 0f;
        for (var i = 0; i < 3; i++)
        {
            if ((allowed & (1 << i)) != 0)
            {
                total += Weight(i);
            }
        }

        if (total <= 0f)
        {
            return -1;
        }

        var roll = (float)rng.NextDouble() * total;
        for (var i = 0; i < 3; i++)
        {
            if ((allowed & (1 << i)) == 0)
            {
                continue;
            }

            roll -= Weight(i);
            if (roll <= 0f)
            {
                return i;
            }
        }

        // Float drift only, so hand back the last allowed lane rather than nothing.
        for (var i = 2; i >= 0; i--)
        {
            if ((allowed & (1 << i)) != 0)
            {
                return i;
            }
        }

        return -1;
    }

    private float Weight(int lane)
    {
        var weights = config.LaneWeights;
        if (weights == null || lane >= weights.Length)
        {
            return 1f;
        }

        return Math.Max(0f, weights[lane]);
    }
}
