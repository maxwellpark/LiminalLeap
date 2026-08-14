// Single place gameplay reads input from. Swap Source in a test, call Reset after.
public static class InputRouter
{
    private static IInputSource source;

    public static IInputSource Source
    {
        get => source ??= new KeyboardInput();
        set => source = value;
    }

    public static void Reset()
    {
        source = new KeyboardInput();
    }
}
