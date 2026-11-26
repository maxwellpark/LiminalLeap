using System;

// Static, so it survives the load into the game scene.
public static class RunMode
{
    private static readonly Random Rolls = new();

    public static bool Daily { get; private set; }
    public static bool Story { get; private set; }
    public static int Seed { get; private set; }

    public static string Day { get; private set; } = string.Empty;

    // Unset when a scene is opened directly, so it keeps its own seed.
    public static bool FreshEachRun { get; private set; }

    // A new start, so no lap carries over from the last time a place was run.
    public static void ChooseStory()
    {
        StoryLaps.Clear();
        Story = true;
        Daily = false;
        FreshEachRun = true;
        Day = string.Empty;
    }

    public static void ChooseDaily()
    {
        StoryLaps.Clear();
        Story = false;
        Daily = true;
        FreshEachRun = false;
        (Day, Seed) = DailySeed.Pick(DateTime.UtcNow);
    }

    public static void ChooseFree()
    {
        StoryLaps.Clear();
        Story = false;
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
