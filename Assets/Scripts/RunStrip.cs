using System;
using System.Collections.Generic;

// Ordered by how much a stretch shows it, so the louder one wins.
public enum StretchMark
{
    Quiet,
    LookedBack,
    NearMiss,
    Dodged,
}

// What happened in each 50 m of a run, for the share strip. Fixed size, as it's marked every frame.
public sealed class RunStrip
{
    public const float MetresPerStretch = 50f;
    public const int MaxStretches = 12;

    private readonly StretchMark[] marks = new StretchMark[MaxStretches];

    public IReadOnlyList<StretchMark> Marks => marks;

    public void Mark(float distance, StretchMark mark)
    {
        var i = (int)(Math.Max(0f, distance) / MetresPerStretch);
        if (i < marks.Length && mark > marks[i])
        {
            marks[i] = mark;
        }
    }

    public void Reset()
    {
        Array.Clear(marks, 0, marks.Length);
    }
}
