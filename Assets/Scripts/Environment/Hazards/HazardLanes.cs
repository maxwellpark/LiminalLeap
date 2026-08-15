using System;
using System.Collections.Generic;

// A row spanning the track is an unwinnable run and nothing else would catch it.
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

    // Rows must be far enough apart that a jump over one lands before the next.
    public static int RequiredPieceGap(float maxSpeed, float jumpAirtime, float pieceLength, float marginUnits)
    {
        if (pieceLength <= 0f)
        {
            return 1;
        }

        var reach = Math.Max(0f, maxSpeed * jumpAirtime) + Math.Max(0f, marginUnits);
        return Math.Max(1, (int)Math.Ceiling(reach / pieceLength));
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
