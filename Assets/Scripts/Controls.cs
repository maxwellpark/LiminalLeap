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

    // Before any run, since the lights flicker and drop out from the start; the title takes the
    // same keys as the pause screen, so the flashing can be turned down before it starts.
    public static string Flashing(bool reduced, bool muted) =>
        $"The lights flicker.     {ReducedFlashing}  reduced flashing  {(reduced ? "on" : "off")}     "
        + $"{Mute}  sound  {(muted ? "off" : "on")}";

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
