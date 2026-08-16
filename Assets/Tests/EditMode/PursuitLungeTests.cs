using NUnit.Framework;

public class PursuitLungeTests
{
    private static PursuitModel.Settings WithLunge()
    {
        return new PursuitModel.Settings
        {
            StartDistance = 45f,
            CloseRate = 3.5f,
            RecoverRate = 6f,
            MaxDistance = 45f,
            SpeedRelief = 0f,
            LungeWithin = 12f,
            LungeMultiplier = 2.2f,
        };
    }

    [Test]
    public void FarAwayItClosesAtTheBaseRate()
    {
        Assert.AreEqual(3.5f, PursuitModel.CloseRateAt(40f, WithLunge()), 1e-4f);
    }

    [Test]
    public void InsideTheThresholdItCommits()
    {
        Assert.AreEqual(3.5f * 2.2f, PursuitModel.CloseRateAt(5f, WithLunge()), 1e-4f);
    }

    [Test]
    public void TheThresholdItselfIsStillTheBaseRate()
    {
        Assert.AreEqual(3.5f, PursuitModel.CloseRateAt(12f, WithLunge()), 1e-4f);
    }

    [Test]
    public void NoLungeConfiguredMeansNoChange()
    {
        var s = WithLunge();
        s.LungeWithin = 0f;
        Assert.AreEqual(3.5f, PursuitModel.CloseRateAt(1f, s), 1e-4f);
    }

    [Test]
    public void AMultiplierBelowOneCannotSlowIt()
    {
        var s = WithLunge();
        s.LungeMultiplier = 0.2f;
        Assert.AreEqual(3.5f, PursuitModel.CloseRateAt(5f, s), 1e-4f, "a lunge should never be a reprieve");
    }

    [Test]
    public void LungingClosesFasterOverTheSameSecond()
    {
        var s = WithLunge();
        var lungeLoss = 8f - PursuitModel.Step(8f, 1f, false, 0f, s);
        var cruiseLoss = 40f - PursuitModel.Step(40f, 1f, false, 0f, s);
        Assert.Greater(lungeLoss, cruiseLoss, "close range should lose more ground per second");
    }

    // Watching still has to work inside the lunge, or it becomes an unavoidable death.
    [Test]
    public void WatchingStillHoldsItOffDuringALunge()
    {
        Assert.Greater(PursuitModel.Step(5f, 1f, true, 0f, WithLunge()), 5f);
    }
}
