using System;

// Plain serialisable state. Versioned from the start so an old save can be migrated
// rather than binned when fields change.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 1;

    public int Version = CurrentVersion;
    public float HighScore;
    public float FurthestDistance;
    public int Runs;

    public static SaveData Fresh()
    {
        return new SaveData { Version = CurrentVersion };
    }

    // Returns true if anything changed, so the caller knows to write it back.
    public bool Migrate()
    {
        if (Version == CurrentVersion)
        {
            return false;
        }

        // Nothing to move yet; later versions add their steps here.
        Version = CurrentVersion;
        return true;
    }

    public bool RecordRun(float score, float distance)
    {
        Runs++;

        var improved = false;

        if (score > HighScore)
        {
            HighScore = score;
            improved = true;
        }

        if (distance > FurthestDistance)
        {
            FurthestDistance = distance;
            improved = true;
        }

        return improved;
    }
}
