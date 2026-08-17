using System.Collections.Generic;

// Decides which lanes are fair game. Postponing an attack is always allowed, inventing an
// exception once the beam is already committed is not.
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

    public static bool LaneAllowed(int mask, AttackLane lane)
    {
        return (mask & (1 << (int)lane)) != 0;
    }
}
