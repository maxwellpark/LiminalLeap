using UnityEngine;

// PlayerPrefs, since WebGL has no filesystem and prefs map to IndexedDB.
public static class SaveStore
{
    private const string Key = "liminalleap.save";
    private const string UnreadableKey = Key + ".unreadable";

    private static SaveData cached;
    private static bool fromNewerVersion;

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

        // A bad save costs the scores, not the ability to play, and its text is kept aside, as
        // the next run's save would otherwise write over it for good.
        if (data == null)
        {
            PlayerPrefs.SetString(UnreadableKey, json);
            PlayerPrefs.Save();
            return SaveData.Fresh();
        }

        fromNewerVersion = data.FromNewerVersion;
        if (fromNewerVersion)
        {
            Debug.LogWarning("Save is from a newer version of the game; playing on without saving.");
        }
        else if (data.Migrate())
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
        if (fromNewerVersion)
        {
            return;
        }

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
        fromNewerVersion = false;
    }

    public static void Wipe()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
        cached = null;
        fromNewerVersion = false;
    }
}
