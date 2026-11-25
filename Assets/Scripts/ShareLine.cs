using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// A daily as a few lines to paste: enough to compare, nothing that spoils the corridor.
public static class ShareLine
{
    public const string Link = "maxwellpark.itch.io/liminal-leap";
    public const float MetresPerBlock = RunStrip.MetresPerStretch;
    public const int MaxBlocks = RunStrip.MaxStretches;

    public const string Block = "\u2B1B";        // a quiet stretch
    public const string LookedBack = "\U0001F7E6"; // blue: the mirror was up
    public const string NearMiss = "\U0001F7E8";   // yellow: skimmed a hazard
    public const string Dodged = "\U0001F7E5";     // red: an attack missed
    public const string Door = "\U0001F6AA";     // got out
    public const string Eye = "\U0001F441";      // got caught
    public const string Dot = " \u00B7 ";
    public const string RightCall = "\U0001F7E9";  // green: a lap called right
    public const string WrongCall = "\U0001F7E5";  // red: a lap called wrong

    // Kept is false when dying cost the score, so the line never claims it.
    public static string For(string day, string place, RunOutcome outcome, float distance, float score, bool kept,
        IReadOnlyList<StretchMark> marks = null)
    {
        var metres = Math.Max(0f, distance).ToString("N0", CultureInfo.InvariantCulture);
        var got = outcome == RunOutcome.Died ? $"caught at {metres} m" : $"out at {metres} m";

        var result = new StringBuilder();
        if (!string.IsNullOrEmpty(place))
        {
            result.Append(place).Append(Dot);
        }

        result.Append(got);
        if (kept && score > 0f)
        {
            result.Append(Dot).Append(score.ToString("N0", CultureInfo.InvariantCulture));
        }

        return $"Liminal Leap daily {day}\n{Strip(outcome, distance, marks)}\n{result}\n{Link}";
    }

    // Rounded up, so any distance at all shows as a block.
    public static string Strip(RunOutcome outcome, float distance, IReadOnlyList<StretchMark> marks = null)
    {
        var blocks = (int)Math.Ceiling(Math.Max(0f, distance) / MetresPerBlock);
        var strip = new StringBuilder();

        for (var i = 0; i < Math.Min(blocks, MaxBlocks); i++)
        {
            strip.Append(Glyph(marks != null && i < marks.Count ? marks[i] : StretchMark.Quiet));
        }

        if (blocks > MaxBlocks)
        {
            strip.Append('+');
        }

        return strip.Append(outcome == RunOutcome.Died ? Eye : Door).ToString();
    }

    // Getting out of a story place: how the calls went, never what was wrong.
    public static string ForPlace(string place, IReadOnlyList<bool> calls)
    {
        var right = 0;
        var strip = new StringBuilder();
        for (var i = 0; i < calls.Count; i++)
        {
            right += calls[i] ? 1 : 0;
            if (i < MaxBlocks)
            {
                strip.Append(calls[i] ? RightCall : WrongCall);
            }
        }

        if (calls.Count > MaxBlocks)
        {
            strip.Append('+');
        }

        strip.Append(Door);
        var laps = calls.Count == 1 ? "1 lap" : $"{calls.Count} laps";
        return $"Liminal Leap{Dot}{place}\n{strip}\nout after {laps}{Dot}{right} right, {calls.Count - right} wrong\n{Link}";
    }

    public static string PlacePreview(IReadOnlyList<bool> calls)
    {
        var preview = new StringBuilder();
        for (var i = 0; i < Math.Min(calls.Count, MaxBlocks); i++)
        {
            preview.Append("<color=").Append(calls[i] ? "#57AB5A" : Colour(StretchMark.Dodged)).Append(">\u25A1</color>");
        }

        if (calls.Count > MaxBlocks)
        {
            preview.Append('+');
        }

        return preview.ToString();
    }

    // The summary's version: the one square the game font has, coloured to match the copied line.
    public static string Preview(float distance, IReadOnlyList<StretchMark> marks = null)
    {
        var blocks = (int)Math.Ceiling(Math.Max(0f, distance) / MetresPerBlock);
        var preview = new StringBuilder();

        for (var i = 0; i < Math.Min(blocks, MaxBlocks); i++)
        {
            var mark = marks != null && i < marks.Count ? marks[i] : StretchMark.Quiet;
            preview.Append("<color=").Append(Colour(mark)).Append(">\u25A1</color>");
        }

        if (blocks > MaxBlocks)
        {
            preview.Append('+');
        }

        return preview.ToString();
    }

    public static string Colour(StretchMark mark)
    {
        return mark switch
        {
            StretchMark.LookedBack => "#4FA3E0",
            StretchMark.NearMiss => "#E3B341",
            StretchMark.Dodged => "#E5534B",
            _ => "#9EA6B3",
        };
    }

    public static string Glyph(StretchMark mark)
    {
        return mark switch
        {
            StretchMark.LookedBack => LookedBack,
            StretchMark.NearMiss => NearMiss,
            StretchMark.Dodged => Dodged,
            _ => Block,
        };
    }
}
