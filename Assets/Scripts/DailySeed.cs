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

    public static string TodayKey()
    {
        return KeyFor(DateTime.UtcNow);
    }

    public static int Today()
    {
        return For(DateTime.UtcNow);
    }
}

// Static, so it survives the load into the game scene.
public static class RunMode
{
    private static readonly Random Rolls = new();

    public static bool Daily { get; private set; }
    public static int Seed { get; private set; }

    public static string Day { get; private set; } = string.Empty;

    // Unset when a scene is opened directly, so it keeps its own seed.
    public static bool FreshEachRun { get; private set; }

    public static void ChooseDaily()
    {
        Daily = true;
        FreshEachRun = false;
        Seed = DailySeed.Today();
        Day = DailySeed.TodayKey();
    }

    public static void ChooseFree()
    {
        Daily = false;
        FreshEachRun = true;
        Day = string.Empty;
    }

    // Dailies derive everything from the date. Free runs roll a new seed every time.
    public static int SeedFor(int authored, int salt)
    {
        if (Daily)
        {
            return Seed ^ salt;
        }

        return FreshEachRun ? Rolls.Next() : authored;
    }
}
