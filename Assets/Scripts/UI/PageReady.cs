#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Tells the page the game is up and which run started, for web-check and scripts/web-run.js.
public static class PageReady
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PageReadyMark();
    [DllImport("__Internal")] private static extern void PageRunMark(string mode, string place);
    [DllImport("__Internal")] private static extern void PageTitleMark();
    [DllImport("__Internal")] private static extern void PageLapMark();
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

    // At the start of every run, and again after a death: "story", "daily" or "free", and the
    // place's id or "none". A lap isn't a run.
    public static void Run(string mode, string place)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageRunMark(mode, place);
#endif
    }

    // On the title, so a check can tell a run that ended there from one still going.
    public static void Title()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageTitleMark();
#endif
    }

    // Each lap of a place, back at its start after a call at an exit.
    public static void Lap()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageLapMark();
#endif
    }
}
