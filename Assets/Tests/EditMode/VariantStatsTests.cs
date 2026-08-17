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
    public void MigratingAnOldSaveFillsInTheNewFields()
    {
        var save = SaveData.Fresh();
        save.Version = 1;
        save.Ghost = null;
        save.Variants = null;
        save.HighScore = 4321f;

        Assert.IsTrue(save.Migrate());
        Assert.AreEqual(SaveData.CurrentVersion, save.Version);
        Assert.IsNotNull(save.Ghost, "a null ghost would throw on the next run");
        Assert.IsNotNull(save.Variants);
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
