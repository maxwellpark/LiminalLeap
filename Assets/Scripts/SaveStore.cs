using UnityEngine;

// PlayerPrefs rather than a file: WebGL has no plain filesystem, and prefs map to
// IndexedDB there, so the same code works in the browser build.
public static class SaveStore
{
    private const string Key = "liminalleap.save";

    private static SaveData cached;

    public static SaveData Data => cached ??= Load();

    public static SaveData Load()
    {
        var json = PlayerPrefs.GetString(Key, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return SaveData.Fresh();
        }

        SaveData data = null;
        try
        {
            data = JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Save unreadable, starting fresh: " + e.Message);
        }

        // A corrupt or truncated save should cost you your scores, not the ability to play.
        if (data == null)
        {
            return SaveData.Fresh();
        }

        if (data.Migrate())
        {
            Save(data);
        }

        return data;
    }

    public static void Save(SaveData data)
    {
        if (data == null)
        {
            return;
        }

        cached = data;
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static void Save()
    {
        Save(Data);
    }

    // Tests need a clean slate without touching the player's real save.
    public static void ClearCache()
    {
        cached = null;
    }

    public static void Wipe()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        cached = null;
    }
}
