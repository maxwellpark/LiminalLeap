using System;

// Where exits go. A place's sits at its end, and a missed one comes round again shortly.
// An endless run spaces them further apart each time.
public static class ExitSchedule
{
    public static int First(int placeLength, int endlessFirst)
    {
        return placeLength > 0 ? placeLength : endlessFirst;
    }

    public static int Next(int spawned, int exitsSpawned, int placeLength, int endlessFirst, float growth, int retryGap)
    {
        return spawned + (placeLength > 0
            ? retryGap
            : (int)Math.Round(endlessFirst * Math.Pow(growth, exitsSpawned)));
    }
}
