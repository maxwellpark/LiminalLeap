using NUnit.Framework;

public class SaveDataTests
{
    [Test]
    public void FreshSaveIsAtTheCurrentVersion()
    {
        Assert.AreEqual(SaveData.CurrentVersion, SaveData.Fresh().Version);
    }

    [Test]
    public void FreshSaveHasNoScores()
    {
        var s = SaveData.Fresh();
        Assert.AreEqual(0f, s.HighScore);
        Assert.AreEqual(0f, s.FurthestDistance);
        Assert.AreEqual(0, s.Runs);
    }

    [Test]
    public void RecordingKeepsTheBestScore()
    {
        var s = SaveData.Fresh();
        s.RecordRun(100f, 50f);
        s.RecordRun(40f, 20f);
        Assert.AreEqual(100f, s.HighScore);
    }

    [Test]
    public void DistanceAndScoreImproveIndependently()
    {
        var s = SaveData.Fresh();
        s.RecordRun(100f, 10f);
        s.RecordRun(20f, 500f);

        Assert.AreEqual(100f, s.HighScore, "a long slow run should not lower the best score");
        Assert.AreEqual(500f, s.FurthestDistance);
    }

    [Test]
    public void RunsCountEveryAttempt()
    {
        var s = SaveData.Fresh();
        s.RecordRun(10f, 10f);
        s.RecordRun(1f, 1f);
        s.RecordRun(1f, 1f);
        Assert.AreEqual(3, s.Runs);
    }

    [Test]
    public void ImprovementIsOnlyReportedWhenSomethingBeatsTheBest()
    {
        var s = SaveData.Fresh();
        Assert.IsTrue(s.RecordRun(100f, 50f), "first run is always an improvement");
        Assert.IsFalse(s.RecordRun(10f, 5f), "a worse run should not report an improvement");
        Assert.IsTrue(s.RecordRun(10f, 900f), "beating distance alone still counts");
    }

    [Test]
    public void MigrateDoesNothingAtTheCurrentVersion()
    {
        Assert.IsFalse(SaveData.Fresh().Migrate());
    }

    [Test]
    public void MigrateBringsAnOldSaveForward()
    {
        var s = SaveData.Fresh();
        s.Version = 0;
        Assert.IsTrue(s.Migrate());
        Assert.AreEqual(SaveData.CurrentVersion, s.Version);
    }

    [Test]
    public void MigrateKeepsScoresWhenUpgrading()
    {
        var s = SaveData.Fresh();
        s.Version = 0;
        s.HighScore = 1234f;
        s.Migrate();
        Assert.AreEqual(1234f, s.HighScore, "upgrading a save must not cost the player their score");
    }

    // A save from before the maps loads with no list at all, which must not throw.
    [Test]
    public void AnOldSaveGainsAnEmptyMap()
    {
        var s = SaveData.Fresh();
        s.Version = 4;
        s.Maps = null;

        Assert.IsTrue(s.Migrate());
        Assert.IsNotNull(s.Maps);
        Assert.AreEqual(0, s.Map("office").Marks.Count);
    }

    [Test]
    public void EachPlaceHasOneMap()
    {
        var s = SaveData.Fresh();
        s.Map("office").Mark(3, MapMarkKind.Fork);

        Assert.AreSame(s.Map("office"), s.Map("office"));
        Assert.AreEqual(1, s.Map("office").Marks.Count);
        Assert.AreEqual(0, s.Map("school").Marks.Count, "one place's map isn't another's");
        Assert.AreEqual(2, s.Maps.Count);
    }

    [Test]
    public void APlacesBestKeepsTheHigherScoreAndCountsEveryRun()
    {
        var s = SaveData.Fresh();

        Assert.IsTrue(s.PlaceBest("office").Record(300f));
        Assert.IsFalse(s.PlaceBest("office").Record(120f), "a lower score isn't a best");
        Assert.AreEqual(300f, s.PlaceBest("office").BestScore);
        Assert.AreEqual(2, s.PlaceBest("office").Runs);
        Assert.AreEqual(0, s.PlaceBest("school").Runs);
    }
}
