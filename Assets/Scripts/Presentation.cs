using UnityEngine;

// Story plays as horror, the daily as an arcade runner. The HUD, toasts and summary ask here.
public static class Presentation
{
    public static bool Arcade => !RunMode.Story;

    // A sign's two readings: the one ahead, which can lie, and the mirror's, which can't.
    public static int ForwardOnlyLayer => LayerMask.NameToLayer("ForwardOnly");
    public static int MirrorOnlyLayer => LayerMask.NameToLayer("MirrorOnly");
}
