using System;

// Pure distance rules for the thing behind you, so the pursuit can be tested without a scene.
public static class PursuitModel
{
    public struct Settings
    {
        public float StartDistance;
        public float CloseRate;     // units/sec gained while unobserved
        public float RecoverRate;   // units/sec lost while watched
        public float MaxDistance;
        public float SpeedRelief;   // extra distance per second at full player speed
        public float LungeWithin;   // closes harder inside this distance
        public float LungeMultiplier;
    }

    // Flat pursuit makes the endgame as tame as the opening. Past a threshold it commits.
    public static float CloseRateAt(float distance, Settings s)
    {
        var rate = Math.Max(0f, s.CloseRate);

        if (s.LungeWithin > 0f && distance < s.LungeWithin)
        {
            rate *= Math.Max(1f, s.LungeMultiplier);
        }

        return rate;
    }

    // Watching it holds it off, running fast buys a little room, ignoring it costs you.
    public static float Step(float distance, float dt, bool observed, float speedFraction, Settings s)
    {
        if (dt <= 0f)
        {
            return distance;
        }

        var relief = Math.Max(0f, speedFraction) * Math.Max(0f, s.SpeedRelief) * dt;

        distance += observed
            ? Math.Max(0f, s.RecoverRate) * dt + relief
            : relief - CloseRateAt(distance, s) * dt;

        var max = s.MaxDistance <= 0f ? float.MaxValue : s.MaxDistance;
        return Math.Min(Math.Max(distance, 0f), max);
    }

    // 0 when far, 1 when it reaches you. Drives dread, not just death.
    public static float Proximity(float distance, Settings s)
    {
        if (s.StartDistance <= 0f)
        {
            return 0f;
        }

        var t = 1f - distance / s.StartDistance;
        return t < 0f ? 0f : t > 1f ? 1f : t;
    }

    public static bool Caught(float distance)
    {
        return distance <= 0f;
    }
}
