// One source for what the keys are. The title screen had its own copy serialised into the
// scene, so adding a control changed the code and left the screen still telling players the
// old set. Derive it instead.
public static class Controls
{
    public const string Steer = "A D";
    public const string Jump = "SPACE";
    public const string LookBack = "SHIFT";
    public const string Leave = "E";
    public const string Restart = "R";

    public static string Summary =>
        $"{Steer}  steer     {Jump}  jump     {LookBack}  look back     {Leave}  leave";

    // Floor signage takes them one per stripe, in the order you meet them.
    public static readonly string[] FloorLines =
    {
        $"{Steer}    STEER",
        $"{Jump}    JUMP",
        $"{LookBack}    LOOK BACK",
        $"{Leave}    LEAVE",
        $"{Restart}    RESTART",
    };
}
