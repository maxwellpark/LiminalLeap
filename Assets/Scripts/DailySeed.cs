using System;

// Everyone runs the same corridor on a given date.
public static class DailySeed
{
    // UTC, so the day rolls over at the same moment for everyone.
    public static string KeyFor(DateTime date)
    {
        return date.ToUniversalTime().ToString("yyyy-MM-dd");
    }

    public static int For(DateTime date)
    {
        unchecked
        {
            var days = (uint)(date.ToUniversalTime().Date - new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Days;

            // Hashed, or consecutive days lay out similar corridors.
            var h = days * 2654435761u;
            h ^= h >> 15;
            h *= 0x85ebca6b;
            h ^= h >> 13;

            return (int)(h & 0x7FFFFFFF);
        }
    }

    // The key and the seed from one reading of the clock, so a start across midnight can't play
    // one day's corridor under the next day's name.
    public static (string Key, int Seed) Pick(DateTime now)
    {
        return (KeyFor(now), For(now));
    }

    public static string TodayKey()
    {
        return KeyFor(DateTime.UtcNow);
    }

    public static int Today()
    {
        return For(DateTime.UtcNow);
    }
}
