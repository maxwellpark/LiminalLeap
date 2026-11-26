using System;
using System.Collections.Generic;

public enum AnomalyKind
{
    None,
    WrongLight,      // a stretch of strip lights the wrong colour
    HangingPillars,  // pillars that stop short of the floor
    LowLights,       // strip lights hung at head height
    WrongCount,      // the floor count skips, when only a fade can change it
}

public enum LapCall
{
    Onward,   // right to run past: the next lap carries on
    Through,  // right to leave, but not enough yet: back to the start, count kept
    Leave,    // right to leave, and enough: out of the place
    Wrong,    // the wrong call: back to the start, count lost
}

// Something wrong this lap: run past the exit. Nothing wrong: take it.
public sealed class LapRule
{
    public const int StretchPieces = 4;

    private static readonly AnomalyKind[] Kinds =
    {
        AnomalyKind.WrongLight,
        AnomalyKind.HangingPillars,
        AnomalyKind.LowLights,
        AnomalyKind.WrongCount,
    };

    public int Seed { get; }
    public int Required { get; }
    public float Chance { get; }

    public int Lap { get; private set; }        // the lap the player is in
    public int CleanLap { get; private set; }   // first lap after a start or a wrong call
    public int Streak { get; private set; }     // right calls in a row

    private readonly List<bool> history = new();

    // Every call made in the place, right or wrong, for the share line.
    public IReadOnlyList<bool> History => history;

    public LapRule(int seed, int required, float chance)
    {
        Seed = seed;
        Required = Math.Max(1, required);
        Chance = Math.Max(0f, Math.Min(1f, chance));
    }

    // The first lap after a start is always clean, so there's a baseline to tell wrong from.
    public bool HasAnomaly(int lap)
    {
        return lap != CleanLap && Hash01(Seed, lap, 1) < Chance;
    }

    public AnomalyKind KindFor(int lap)
    {
        return HasAnomaly(lap) ? Kinds[(int)(Hash01(Seed, lap, 2) * Kinds.Length) % Kinds.Length] : AnomalyKind.None;
    }

    // Away from both ends, so it isn't in the lead-in and there's time to decide after it.
    public int StretchStart(int lap, int lapPieces)
    {
        var first = lapPieces / 4;
        var span = Math.Max(1, lapPieces / 2 - StretchPieces);
        return first + (int)(Hash01(Seed, lap, 3) * span) % span;
    }

    public bool InStretch(int lap, int pieceInLap, int lapPieces)
    {
        var kind = KindFor(lap);
        if (kind == AnomalyKind.None || kind == AnomalyKind.WrongCount)
        {
            return false;
        }

        var start = StretchStart(lap, lapPieces);
        return pieceInLap >= start && pieceInLap < start + StretchPieces;
    }

    // As if the calls before it are right, since a wrong one fades the track away anyway.
    public int CountShown(int lap)
    {
        var expected = Streak + Math.Max(0, lap - Lap);
        return KindFor(lap) == AnomalyKind.WrongCount ? expected + 2 : expected;
    }

    // Painted under the rules, so the count on the floor has something to count towards.
    public static string GoalLine(int required)
    {
        return required > 1 ? $"{required} RIGHT IN A ROW    GET OUT" : "RIGHT ONCE    GET OUT";
    }

    // Dying doesn't call, so the same lap comes round again with the same answer.
    public LapCall Call(bool tookExit)
    {
        var right = tookExit != HasAnomaly(Lap);
        history.Add(right);
        if (!right)
        {
            Streak = 0;
            Lap++;
            CleanLap = Lap;
            return LapCall.Wrong;
        }

        Streak++;
        if (tookExit && Streak >= Required)
        {
            return LapCall.Leave;
        }

        Lap++;
        return tookExit ? LapCall.Through : LapCall.Onward;
    }

    private static float Hash01(int seed, int lap, int salt)
    {
        unchecked
        {
            var h = (uint)(seed * 73856093) ^ (uint)(lap * 19349663) ^ (uint)(salt * 83492791);
            h ^= h >> 13;
            h *= 0x85ebca6b;
            h ^= h >> 16;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
