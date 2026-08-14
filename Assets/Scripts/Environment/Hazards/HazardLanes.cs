using System;
using System.Collections.Generic;

// Whether a row of hazards leaves a gap the player can fit through. A procedurally
// placed row that spans the track is an unwinnable run, and nothing else would catch it.
public static class HazardLanes
{
    public struct Span
    {
        public float Min;
        public float Max;

        public Span(float centre, float halfWidth)
        {
            Min = centre - halfWidth;
            Max = centre + halfWidth;
        }
    }

    // trackHalfWidth is how far the player can strafe, playerHalfWidth its own width.
    public static bool HasGap(IReadOnlyList<Span> blocked, float trackHalfWidth, float playerHalfWidth)
    {
        var needed = playerHalfWidth * 2f;
        var sorted = new List<Span>(blocked);
        sorted.Sort((a, b) => a.Min.CompareTo(b.Min));

        var cursor = -trackHalfWidth;

        foreach (var span in sorted)
        {
            if (span.Max <= cursor)
            {
                continue; // fully behind the cursor, already covered
            }

            if (span.Min - cursor >= needed)
            {
                return true;
            }

            cursor = Math.Max(cursor, span.Max);
        }

        return trackHalfWidth - cursor >= needed;
    }

    // Widest gap available, so a generator can pick a lane that stays passable.
    public static float WidestGap(IReadOnlyList<Span> blocked, float trackHalfWidth)
    {
        var sorted = new List<Span>(blocked);
        sorted.Sort((a, b) => a.Min.CompareTo(b.Min));

        var cursor = -trackHalfWidth;
        var widest = 0f;

        foreach (var span in sorted)
        {
            if (span.Max <= cursor)
            {
                continue;
            }

            widest = Math.Max(widest, span.Min - cursor);
            cursor = Math.Max(cursor, span.Max);
        }

        return Math.Max(widest, trackHalfWidth - cursor);
    }
}
