using System;
using System.Collections.Generic;

// Which lanes are fair game. Postponing is always allowed.
public static class PursuerSafety
{
    public static int AllowedLanes(
        IReadOnlyList<HazardLanes.Span> blocked,
        float laneSpacing,
        float playerHalfWidth)
    {
        var open = 0;
        var count = 0;

        for (var i = 0; i < 3; i++)
        {
            var centre = (i - 1) * laneSpacing;
            if (HazardLanes.Overlaps(new HazardLanes.Span(centre, playerHalfWidth), blocked, 0f))
            {
                continue;
            }

            open |= 1 << i;
            count++;
        }

        // Taking the last open lane leaves no legal response, so wait for a cleaner stretch.
        return count >= 2 ? open : 0;
    }

    // Centred on where the beam lands, which is over 50 units out at full speed.
    public static void ResolveWindow(
        float speed,
        float secondsUntilResolve,
        float margin,
        out float near,
        out float far)
    {
        var landing = Math.Max(0f, speed) * Math.Max(0f, secondsUntilResolve);
        var spread = Math.Max(0f, margin);

        near = Math.Max(0f, landing - spread);
        far = landing + spread;
    }

    public static bool LaneAllowed(int mask, AttackLane lane)
    {
        return (mask & (1 << (int)lane)) != 0;
    }
}
