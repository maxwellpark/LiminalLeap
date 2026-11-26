using System;
using System.Collections.Generic;

// Banking and dying differ to the player, even at the same distance.
public enum RunOutcome
{
    Died,
    Completed,
    Banked,
    Abandoned,  // restarted on purpose, which is neither a death nor a result
}

// Per flag combination, so two variants can be compared instead of guessed at.
[Serializable]
public class VariantRecord
{
    public string Key;
    public int Runs;
    public int Banked;
    public float BestScore;
    public float TotalScore;
    public float TotalDistance;

    public float MeanScore => Runs > 0 ? TotalScore / Runs : 0f;
    public float MeanDistance => Runs > 0 ? TotalDistance / Runs : 0f;
    public float BankRate => Runs > 0 ? (float)Banked / Runs : 0f;

    public void Record(float score, float distance, RunOutcome outcome)
    {
        Runs++;
        TotalScore += score;
        TotalDistance += distance;

        if (score > BestScore)
        {
            BestScore = score;
        }

        if (outcome == RunOutcome.Banked)
        {
            Banked++;
        }
    }
}

// Best per day, separate from the all time best.
[Serializable]
public class DailyRecord
{
    public string Day;
    public int Runs;
    public float BestScore;
    public float BestDistance;

    public bool Record(float score, float distance)
    {
        Runs++;

        var improved = false;

        if (score > BestScore)
        {
            BestScore = score;
            improved = true;
        }

        if (distance > BestDistance)
        {
            BestDistance = distance;
            improved = true;
        }

        return improved;
    }
}

// Best per place in the story.
[Serializable]
public class PlaceRecord
{
    public string Id;
    public int Runs;
    public float BestScore;

    public bool Record(float score)
    {
        Runs++;
        if (score <= BestScore)
        {
            return false;
        }

        BestScore = score;
        return true;
    }
}

// Versioned so an old save can be migrated rather than binned.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 5;

    public int Version = CurrentVersion;
    public float HighScore;
    public float FurthestDistance;
    public int Runs;

    public GhostTrace Ghost = new();
    public List<VariantRecord> Variants = new();
    public List<DailyRecord> Dailies = new();

    public int StoryPlace;
    public int StoryFinishes;
    public List<PlaceRecord> PlaceBests = new();
    public List<PlaceMap> Maps = new();

    public static SaveData Fresh()
    {
        return new SaveData { Version = CurrentVersion };
    }

    // Written by a later build of the game. Left as it is, since this build would drop the fields
    // it doesn't know.
    public bool FromNewerVersion => Version > CurrentVersion;

    // Returns true if anything changed, so the caller knows to write it back.
    public bool Migrate()
    {
        EnsureFields();

        if (Version >= CurrentVersion)
        {
            return false;
        }

        // v2 ghost and variant stats, v3 daily bests, v4 story, v5 place maps: only fields to fill in.
        Version = CurrentVersion;
        return true;
    }

    public bool RecordRun(float score, float distance)
    {
        return RecordRun(score, distance, RunOutcome.Died, null);
    }

    public bool RecordRun(float score, float distance, RunOutcome outcome, string variantKey)
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

        if (!string.IsNullOrEmpty(variantKey))
        {
            Variant(variantKey).Record(score, distance, outcome);
        }

        return improved;
    }

    // Enough days for a streak to mean something, without a record kept for every day forever.
    public const int DailiesKept = 60;

    // Without adding one, so reading a day's best doesn't grow the save.
    public DailyRecord PeekDaily(string day)
    {
        EnsureFields();

        for (var i = 0; i < Dailies.Count; i++)
        {
            if (Dailies[i].Day == day)
            {
                return Dailies[i];
            }
        }

        return null;
    }

    public DailyRecord Daily(string day)
    {
        var found = PeekDaily(day);
        if (found != null)
        {
            return found;
        }

        var record = new DailyRecord { Day = day };
        Dailies.Add(record);
        TrimDailies(record);
        return record;
    }

    // Keys are yyyy-MM-dd, so the oldest sort first. Never the one just asked for.
    private void TrimDailies(DailyRecord keep)
    {
        if (Dailies.Count <= DailiesKept)
        {
            return;
        }

        Dailies.Sort((a, b) => string.CompareOrdinal(a.Day, b.Day));
        for (var i = 0; i < Dailies.Count && Dailies.Count > DailiesKept;)
        {
            if (Dailies[i] == keep)
            {
                i++;
                continue;
            }

            Dailies.RemoveAt(i);
        }
    }

    public PlaceMap Map(string id)
    {
        EnsureFields();

        for (var i = 0; i < Maps.Count; i++)
        {
            if (Maps[i].Id == id)
            {
                return Maps[i];
            }
        }

        var map = new PlaceMap { Id = id };
        Maps.Add(map);
        return map;
    }

    public PlaceRecord PlaceBest(string id)
    {
        EnsureFields();

        for (var i = 0; i < PlaceBests.Count; i++)
        {
            if (PlaceBests[i].Id == id)
            {
                return PlaceBests[i];
            }
        }

        var record = new PlaceRecord { Id = id };
        PlaceBests.Add(record);
        return record;
    }

    public VariantRecord Variant(string key)
    {
        EnsureFields();

        for (var i = 0; i < Variants.Count; i++)
        {
            if (Variants[i].Key == key)
            {
                return Variants[i];
            }
        }

        var record = new VariantRecord { Key = key };
        Variants.Add(record);
        return record;
    }

    // JsonUtility leaves missing objects null in older saves.
    private void EnsureFields()
    {
        Ghost ??= new GhostTrace();
        Variants ??= new List<VariantRecord>();
        Dailies ??= new List<DailyRecord>();
        PlaceBests ??= new List<PlaceRecord>();
        Maps ??= new List<PlaceMap>();
    }
}
