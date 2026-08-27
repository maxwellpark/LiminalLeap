using System;

// Everyone runs the same corridor on a given date. The generator is already deterministic,
// so a shared seed is nearly free, and it gives a reason to come back tomorrow without
// touching what the game is.
public static class DailySeed
{
    // UTC, so the day rolls over at the same moment for everyone rather than making the
    // "same" daily run depend on which timezone you opened it in.
    public static string KeyFor(DateTime date)
    {
        return date.ToUniversalTime().ToString("yyyy-MM-dd");
    }

    public static int For(DateTime date)
    {
        unchecked
        {
            var days = (uint)(date.ToUniversalTime().Date - new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Days;

            // Hashed rather than used raw: consecutive ordinals produce visibly similar
            // corridors, and "today looks like yesterday" defeats the point.
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

// Which run you asked for. Static because it has to survive the load into the game scene.
public static class RunMode
{
    public static bool Daily { get; private set; }
    public static int Seed { get; private set; }

    public static string Day { get; private set; } = string.Empty;

    public static void ChooseDaily()
    {
        Daily = true;
        Seed = DailySeed.Today();
        Day = DailySeed.TodayKey();
    }

    public static void ChooseFree()
    {
        Daily = false;
        Day = string.Empty;
    }
}
