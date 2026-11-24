#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Tells the web page the game is up, for shipwright's web-check --ready-marker.
public static class PageReady
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PageReadyMark();
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
}
