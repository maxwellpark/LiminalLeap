using System;

public enum SignKind
{
    Clear,
    Jump,
    Strafe,
    ExitAhead,
}

// What the signage says, kept apart from painting it so the lying can be tested.
public static class SignText
{
    public static readonly SignKind[] All = (SignKind[])Enum.GetValues(typeof(SignKind));

    public static string Label(SignKind kind)
    {
        return kind switch
        {
            SignKind.Clear => "CLEAR AHEAD",
            SignKind.Jump => "JUMP AHEAD",
            SignKind.Strafe => "OBSTRUCTION  KEEP LEFT",
            SignKind.ExitAhead => "EXIT  400m",
            _ => string.Empty,
        };
    }

    // Mostly honest, or the lies stop meaning anything and the signs become wallpaper.
    // roll is 0..1; anything at or above lieChance tells the truth.
    public static SignKind Choose(SignKind truth, float roll, float lieChance)
    {
        if (lieChance <= 0f || roll >= lieChance)
        {
            return truth;
        }

        // Always lands on a different kind, so a lie is never accidentally the truth.
        var span = Math.Max(0.0001f, lieChance);
        var offset = 1 + (int)(roll / span * (All.Length - 1));
        offset = Math.Clamp(offset, 1, All.Length - 1);

        return All[((int)truth + offset) % All.Length];
    }

    public static bool IsLie(SignKind truth, SignKind shown)
    {
        return truth != shown;
    }
}
