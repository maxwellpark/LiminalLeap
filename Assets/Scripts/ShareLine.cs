using System;
using System.Globalization;
using System.Text;

// A daily as a few lines to paste: enough to compare, nothing that spoils the corridor.
public static class ShareLine
{
    public const string Link = "maxwellpark.itch.io/liminal-leap";
    public const float MetresPerBlock = 50f;
    public const int MaxBlocks = 12;

    public const string Block = "\u2B1B";      // square, one per stretch of the run
    public const string Door = "\U0001F6AA";   // door, for getting out
    public const string Eye = "\U0001F441";    // eye, for getting caught

    // Kept is false when dying cost the score, so the line never claims it.
    public static string For(string day, string place, RunOutcome outcome, float distance, float score, bool kept)
    {
        var metres = Math.Max(0f, distance).ToString("N0", CultureInfo.InvariantCulture);
        var got = outcome == RunOutcome.Died ? $"caught at {metres} m" : $"out at {metres} m";

        var result = new StringBuilder();
        if (!string.IsNullOrEmpty(place))
        {
            result.Append(place).Append(" · ");
        }

        result.Append(got);
        if (kept && score > 0f)
        {
            result.Append(" · ").Append(score.ToString("N0", CultureInfo.InvariantCulture));
        }

        return $"Liminal Leap daily {day}\n{Strip(outcome, distance)}\n{result}\n{Link}";
    }

    // Rounded up, so any distance at all shows as a block.
    public static string Strip(RunOutcome outcome, float distance)
    {
        var blocks = (int)Math.Ceiling(Math.Max(0f, distance) / MetresPerBlock);
        var strip = new StringBuilder();

        for (var i = 0; i < Math.Min(blocks, MaxBlocks); i++)
        {
            strip.Append(Block);
        }

        if (blocks > MaxBlocks)
        {
            strip.Append('+');
        }

        return strip.Append(outcome == RunOutcome.Died ? Eye : Door).ToString();
    }
}
