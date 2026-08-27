using NUnit.Framework;

public class VariantStatsTests
{
    [Test]
    public void RecordingWithoutAKeyKeepsTheOldBehaviour()
    {
        var save = SaveData.Fresh();
        save.RecordRun(100f, 50f);

        Assert.AreEqual(1, save.Runs);
        Assert.IsEmpty(save.Variants, "no key means no bucket");
    }

    [Test]
    public void EachVariantGetsItsOwnBucket()
    {
        var save = SaveData.Fresh();
        save.RecordRun(100f, 50f, RunOutcome.Died, "1010");
        save.RecordRun(200f, 60f, RunOutcome.Died, "0101");

        Assert.AreEqual(2, save.Variants.Count);
    }

    [Test]
    public void TheSameVariantAccumulates()
    {
        var save = SaveData.Fresh();
        save.RecordRun(100f, 50f, RunOutcome.Died, "1010");
        save.RecordRun(300f, 70f, RunOutcome.Died, "1010");

        var record = save.Variant("1010");

        Assert.AreEqual(1, save.Variants.Count);
        Assert.AreEqual(2, record.Runs);
        Assert.AreEqual(300f, record.BestScore);
        Assert.AreEqual(200f, record.MeanScore, 0.001f);
        Assert.AreEqual(60f, record.MeanDistance, 0.001f);
    }

    [Test]
    public void BankRateCountsOnlyBankedRuns()
    {
        var save = SaveData.Fresh();
        save.RecordRun(10f, 10f, RunOutcome.Banked, "x");
        save.RecordRun(10f, 10f, RunOutcome.Died, "x");
        save.RecordRun(10f, 10f, RunOutcome.Died, "x");
        save.RecordRun(10f, 10f, RunOutcome.Completed, "x");

        var record = save.Variant("x");

        Assert.AreEqual(1, record.Banked, "completing is not the same as choosing to leave");
        Assert.AreEqual(0.25f, record.BankRate, 0.001f);
    }

    [Test]
    public void AnEmptyBucketReportsZeroRatherThanDividingByNothing()
    {
        var record = SaveData.Fresh().Variant("fresh");

        Assert.AreEqual(0f, record.MeanScore);
        Assert.AreEqual(0f, record.MeanDistance);
        Assert.AreEqual(0f, record.BankRate);
    }

    [Test]
    public void EachDayGetsItsOwnBest()
    {
        var save = SaveData.Fresh();
        save.Daily("2026-08-27").Record(500f, 200f);
        save.Daily("2026-08-28").Record(100f, 50f);

        Assert.AreEqual(2, save.Dailies.Count);
        Assert.AreEqual(500f, save.Daily("2026-08-27").BestScore);
        Assert.AreEqual(100f, save.Daily("2026-08-28").BestScore);
    }

    [Test]
    public void ADayKeepsItsBestRatherThanItsLast()
    {
        var day = SaveData.Fresh().Daily("2026-08-27");

        Assert.IsTrue(day.Record(500f, 200f));
        Assert.IsFalse(day.Record(100f, 50f), "a worse run is not an improvement");
        Assert.AreEqual(500f, day.BestScore);
        Assert.AreEqual(200f, day.BestDistance);
        Assert.AreEqual(2, day.Runs, "every attempt still counts");
    }

    [Test]
    public void DistanceAndScoreImproveIndependentlyWithinADay()
    {
        var day = SaveData.Fresh().Daily("2026-08-27");
        day.Record(500f, 10f);

        Assert.IsTrue(day.Record(1f, 900f), "beating distance alone still counts");
        Assert.AreEqual(500f, day.BestScore);
        Assert.AreEqual(900f, day.BestDistance);
    }

    [Test]
    public void MigratingAnOldSaveFillsInTheNewFields()
    {
        var save = SaveData.Fresh();
        save.Version = 1;
        save.Ghost = null;
        save.Variants = null;
        save.Dailies = null;
        save.HighScore = 4321f;

        Assert.IsTrue(save.Migrate());
        Assert.AreEqual(SaveData.CurrentVersion, save.Version);
        Assert.IsNotNull(save.Ghost, "a null ghost would throw on the next run");
        Assert.IsNotNull(save.Variants);
        Assert.IsNotNull(save.Dailies);
        Assert.AreEqual(4321f, save.HighScore, "upgrading must not cost the player their score");
    }

    [Test]
    public void MigrateAtTheCurrentVersionStillRepairsNulls()
    {
        var save = SaveData.Fresh();
        save.Ghost = null;

        save.Migrate();
        Assert.IsNotNull(save.Ghost);
    }
}
