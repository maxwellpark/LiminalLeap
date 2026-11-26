#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Tells the page the game is up and which run started, for web-check and scripts/web-run.js.
public static class PageReady
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PageReadyMark();
    [DllImport("__Internal")] private static extern void PageRunMark(string mode, string place);
    [DllImport("__Internal")] private static extern void PageTitleMark(int dailyRuns);
    [DllImport("__Internal")] private static extern void PageLapMark();
    [DllImport("__Internal")] private static extern void PageOptionsMark(int reduced, int muted);
    [DllImport("__Internal")] private static extern int PageAudioRunning();
    [DllImport("__Internal")] private static extern void PageMarkSting();
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

    // At the start of every run, and again after each death, restart or way out of a place:
    // "story", "daily" or "free", and the place's id or "none". A lap isn't a run.
    public static void Run(string mode, string place)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageRunMark(mode, place);
#endif
    }

    // On the title, so a check can tell a run that ended there from one still going, and with
    // today's daily runs as the save has them, so a check can see the save survive a reload.
    public static void Title(int dailyRuns)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageTitleMark(dailyRuns);
#endif
    }

    // Whether a sound played now is heard from its start. Always, outside a browser.
    public static bool AudioRunning
    {
        get
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return PageAudioRunning() != 0;
#else
            return true;
#endif
        }
    }

    public static void StingPlayed()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageMarkSting();
#endif
    }

    // The player's options as the title shows them, so a check can see them set and kept.
    public static void Options(bool reducedFlashing, bool muted)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PageOptionsMark(reducedFlashing ? 1 : 0, muted ? 1 : 0);
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
