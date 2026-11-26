// One source for the keys, so the title screen can't teach a stale set.
public static class Controls
{
    public const string Steer = "A D";
    public const string Jump = "SPACE";
    public const string LookBack = "SHIFT";
    public const string LookBackAlso = "Q";
    public const string Leave = "E";
    public const string Restart = "R";
    public const string Pause = "P";
    public const string Daily = "ENTER";
    public const string ReducedFlashing = "F";
    public const string ToTitle = "T";
    public const string Mute = "M";

    public static string Summary =>
        $"{Steer}  steer     {Jump}  jump     {LookBack} or {LookBackAlso}  look back     "
        + $"{Leave}  leave     hold {Restart}  restart     {Pause}  pause";

    // Before any run, since the lights flicker and drop out from the start.
    public static string Flashing =>
        $"The lights flicker. {Pause} then {ReducedFlashing} turns the flashing down.";

    // Floor signage takes them one per stripe, in the order you meet them.
    public static readonly string[] FloorLines =
    {
        $"{Steer}    STEER",
        $"{Jump}    JUMP",
        $"{LookBack}    LOOK BACK",
        $"{Leave}    LEAVE",
        $"HOLD {Restart}    RESTART",
    };
}
