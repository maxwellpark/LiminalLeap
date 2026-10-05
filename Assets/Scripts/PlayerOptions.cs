using UnityEngine;

// Player choices, kept apart from Features. Cached, as MoodLighting reads one every frame.
public static class PlayerOptions
{
    private const string Prefix = "liminalleap.option.";

    private static int reducedFlashing = -1;
    private static int muted = -1;

    public static bool ReducedFlashing
    {
        get => Read(ref reducedFlashing, "reducedFlashing");
        set => Write(ref reducedFlashing, "reducedFlashing", value);
    }

    public static bool Muted
    {
        get => Read(ref muted, "muted");
        set
        {
            Write(ref muted, "muted", value);
            ApplyVolume();
        }
    }

    public static void ApplyVolume()
    {
        AudioListener.volume = Muted ? 0f : 1f;
    }

    private static bool Read(ref int cache, string key)
    {
        if (cache < 0)
        {
            cache = PlayerPrefs.GetInt(Prefix + key, 0);
        }

        return cache == 1;
    }

    private static void Write(ref int cache, string key, bool value)
    {
        cache = value ? 1 : 0;
        PlayerPrefs.SetInt(Prefix + key, cache);
        PlayerPrefs.Save();
    }
}
