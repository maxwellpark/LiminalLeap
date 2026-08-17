using System;
using System.Collections.Generic;

// Your last run, stored as the time you reached each distance mark. A pace curve rather
// than a path, so it still replays when the track ahead generates differently.
[Serializable]
public class GhostTrace
{
    public const float Spacing = 2f;
    public const int MaxSamples = 4000;

    public float[] Times = Array.Empty<float>();

    public bool HasData => Times != null && Times.Length > 1;

    public float Duration => HasData ? Times[Times.Length - 1] : 0f;

    public float TotalDistance => HasData ? (Times.Length - 1) * Spacing : 0f;

    // Stops at the end of the recording: the ghost died there, so outliving your last
    // attempt is how you shake it off.
    public float DistanceAt(float time)
    {
        if (!HasData || time <= Times[0])
        {
            return 0f;
        }

        if (time >= Times[Times.Length - 1])
        {
            return TotalDistance;
        }

        var lo = 0;
        var hi = Times.Length - 1;

        while (hi - lo > 1)
        {
            var mid = (lo + hi) / 2;
            if (Times[mid] <= time)
            {
                lo = mid;
            }
            else
            {
                hi = mid;
            }
        }

        var span = Times[hi] - Times[lo];
        var t = span > 0f ? (time - Times[lo]) / span : 0f;
        return (lo + t) * Spacing;
    }
}

// Samples on distance rather than time, so the trace is the same length whatever the framerate.
public class GhostRecorder
{
    private readonly List<float> times = new() { 0f };
    private int next = 1;

    public int Count => times.Count;

    public void Sample(float time, float distance)
    {
        while (next * GhostTrace.Spacing <= distance && times.Count < GhostTrace.MaxSamples)
        {
            times.Add(time);
            next++;
        }
    }

    public GhostTrace Build()
    {
        return new GhostTrace { Times = times.ToArray() };
    }

    public void Reset()
    {
        times.Clear();
        times.Add(0f);
        next = 1;
    }
}
