using System;
using System.Collections.Generic;

// Banking a run and dying on one are not the same thing to the player, even when the
// distance matches, so the outcome travels with the result.
public enum RunOutcome
{
    Died,
    Completed,
    Banked,
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

// Best per day, so a daily run has something to beat that is not your all time best.
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

// Plain serialisable state. Versioned from the start so an old save can be migrated
// rather than binned when fields change.
[Serializable]
public class SaveData
{
    public const int CurrentVersion = 3;

    public int Version = CurrentVersion;
    public float HighScore;
    public float FurthestDistance;
    public int Runs;

    public GhostTrace Ghost = new();
    public List<VariantRecord> Variants = new();
    public List<DailyRecord> Dailies = new();

    public static SaveData Fresh()
    {
        return new SaveData { Version = CurrentVersion };
    }

    // Returns true if anything changed, so the caller knows to write it back.
    public bool Migrate()
    {
        EnsureFields();

        if (Version == CurrentVersion)
        {
            return false;
        }

        // v2 added the ghost trace and per variant stats, v3 the per day bests. All start
        // empty, so there is nothing to move across, only fields to make sure exist.
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

    public DailyRecord Daily(string day)
    {
        EnsureFields();

        for (var i = 0; i < Dailies.Count; i++)
        {
            if (Dailies[i].Day == day)
            {
                return Dailies[i];
            }
        }

        var record = new DailyRecord { Day = day };
        Dailies.Add(record);
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

    // JsonUtility leaves absent objects null, so a save written before these existed comes
    // back with holes in it.
    private void EnsureFields()
    {
        Ghost ??= new GhostTrace();
        Variants ??= new List<VariantRecord>();
        Dailies ??= new List<DailyRecord>();
    }
}
