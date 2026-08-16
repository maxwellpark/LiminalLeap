using NUnit.Framework;

public class PursuitModelTests
{
    private static PursuitModel.Settings Default()
    {
        return new PursuitModel.Settings
        {
            StartDistance = 45f,
            CloseRate = 3.5f,
            RecoverRate = 6f,
            MaxDistance = 45f,
            SpeedRelief = 0f,
        };
    }

    [Test]
    public void ItClosesWhenNotWatched()
    {
        var after = PursuitModel.Step(45f, 1f, false, 0f, Default());
        Assert.Less(after, 45f);
    }

    [Test]
    public void WatchingHoldsItOff()
    {
        var after = PursuitModel.Step(20f, 1f, true, 0f, Default());
        Assert.Greater(after, 20f);
    }

    [Test]
    public void SpeedBuysRoomEvenWhenUnwatched()
    {
        var s = Default();
        s.SpeedRelief = 10f; // outpaces the 3.5 close rate
        Assert.Greater(PursuitModel.Step(20f, 1f, false, 1f, s), 20f);
    }

    [Test]
    public void ItNeverGoesBelowZero()
    {
        Assert.AreEqual(0f, PursuitModel.Step(0.1f, 10f, false, 0f, Default()));
    }

    [Test]
    public void ItNeverExceedsTheCap()
    {
        Assert.AreEqual(45f, PursuitModel.Step(44f, 100f, true, 1f, Default()));
    }

    [Test]
    public void ZeroDeltaChangesNothing()
    {
        Assert.AreEqual(30f, PursuitModel.Step(30f, 0f, false, 0f, Default()));
    }

    [Test]
    public void ProximityIsZeroAtStartAndOneOnContact()
    {
        var s = Default();
        Assert.AreEqual(0f, PursuitModel.Proximity(45f, s), 1e-4f);
        Assert.AreEqual(1f, PursuitModel.Proximity(0f, s), 1e-4f);
    }

    [Test]
    public void ProximityRisesAsItCloses()
    {
        var s = Default();
        Assert.Greater(PursuitModel.Proximity(10f, s), PursuitModel.Proximity(30f, s));
    }

    [Test]
    public void ProximityStaysInRangeOutsideTheBand()
    {
        var s = Default();
        Assert.AreEqual(0f, PursuitModel.Proximity(9999f, s), 1e-4f);
        Assert.AreEqual(1f, PursuitModel.Proximity(-5f, s), 1e-4f);
    }

    [Test]
    public void CaughtOnlyAtZero()
    {
        Assert.IsFalse(PursuitModel.Caught(0.5f));
        Assert.IsTrue(PursuitModel.Caught(0f));
    }

    [Test]
    public void NegativeRatesDoNotReverseThePursuit()
    {
        var s = Default();
        s.CloseRate = -5f;
        Assert.AreEqual(20f, PursuitModel.Step(20f, 1f, false, 0f, s), 1e-4f);
    }

    // Watching forever should recover to the cap, not creep past it.
    [Test]
    public void SustainedWatchingSettlesAtTheCap()
    {
        var s = Default();
        var d = 5f;
        for (var i = 0; i < 200; i++)
        {
            d = PursuitModel.Step(d, 0.1f, true, 0f, s);
        }

        Assert.AreEqual(45f, d, 1e-3f);
    }
}
