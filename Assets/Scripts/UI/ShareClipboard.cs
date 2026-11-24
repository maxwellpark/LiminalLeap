using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// Browsers only copy inside the key event, which Unity reads a frame late, so the page copies.
public static class ShareClipboard
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ShareClipboardArm(string text);
    [DllImport("__Internal")] private static extern void ShareClipboardDisarm();
    [DllImport("__Internal")] private static extern int ShareClipboardTakeCopied();
#endif

    private static string armed;
    private static bool copied;

    public static bool Armed => armed != null;

    public static void Arm(string text)
    {
        armed = text;
        copied = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        ShareClipboardArm(text);
#endif
    }

    public static void Disarm()
    {
        armed = null;
        copied = false;
#if UNITY_WEBGL && !UNITY_EDITOR
        ShareClipboardDisarm();
#endif
    }

    // A no-op on WebGL, where the page has already copied in its own key handler.
    public static void Copy()
    {
        if (armed == null)
        {
            return;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        GUIUtility.systemCopyBuffer = armed;
        copied = true;
#endif
    }

    // True once per copy, however it happened.
    public static bool TakeCopied()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        copied |= ShareClipboardTakeCopied() == 1;
#endif
        var result = copied;
        copied = false;
        return result;
    }
}
