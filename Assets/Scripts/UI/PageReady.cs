#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Tells the page the game is up and which run started, for web-check and scripts/web-run.js.
public static class PageReady
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PageReadyMark();
    [DllImport("__Internal")] private static extern void PageRunMark(string mode, string place);
#endif

    public static bool Marked { get; private set; }

    public static void Mark()
    {
        if (Marked)
        {
            return;
        }

        Marked = true;
#if UNITY_WEBGL && !UNITY_EDITOR
        PageReadyMark();
#endif
    }

    // At the start of every run: "story", "daily" or "free", and the place's id or "none".
    public static void Run(string mode, string place)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageRunMark(mode, place);
#endif
    }
}
