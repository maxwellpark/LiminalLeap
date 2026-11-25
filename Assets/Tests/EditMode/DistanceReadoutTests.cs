using NUnit.Framework;

public class DistanceReadoutTests
{
    [Test]
    public void WithoutALoopItTellsTheTruth()
    {
        var readout = new DistanceReadout();
        readout.Advance(412f);

        Assert.AreEqual(412f, readout.Shown(412f));
    }

    [Test]
    public void ThroughALoopTheNumbersRepeat()
    {
        var readout = new DistanceReadout();
        readout.From(300f, 100f);

        readout.Advance(300f);
        Assert.AreEqual(200f, readout.Shown(300f));
        readout.Advance(350f);
        Assert.AreEqual(250f, readout.Shown(350f), "it should count on in step");
    }

    [Test]
    public void ItCountsOnUntilTheReplayStarts()
    {
        var readout = new DistanceReadout();
        readout.From(300f, 100f);

        readout.Advance(295f);
        Assert.AreEqual(295f, readout.Shown(295f), "still on the door's own piece");
        Assert.AreEqual(0f, readout.Behind);
        readout.Advance(300f);
        Assert.AreEqual(200f, readout.Shown(300f));
    }

    // A piece left out of the replay, as an exit is: the numbers skip where it stood, as they did.
    [Test]
    public void AGapInTheReplaySkipsAheadAsTheFirstRunDid()
    {
        var readout = new DistanceReadout();
        readout.From(300f, 110f);
        readout.From(340f, 100f);

        readout.Advance(330f);
        Assert.AreEqual(220f, readout.Shown(330f));
        readout.Advance(340f);
        Assert.AreEqual(240f, readout.Shown(340f));
    }

    [Test]
    public void SeveralChangesPassedInOneFrameLandOnTheLast()
    {
        var readout = new DistanceReadout();
        readout.From(100f, 50f);
        readout.From(110f, 40f);
        readout.From(500f, 0f);

        readout.Advance(120f);
        Assert.AreEqual(40f, readout.Behind);
    }

    [Test]
    public void ItNeverReadsBelowZeroOrRunsAhead()
    {
        var readout = new DistanceReadout();
        readout.From(100f, 100f);
        readout.Advance(100f);
        Assert.AreEqual(0f, readout.Shown(100f));

        readout.From(150f, -30f);
        readout.Advance(200f);
        Assert.AreEqual(200f, readout.Shown(200f), "a negative lag mustn't push the readout ahead");
    }

    [Test]
    public void ResetStartsTruthfulAgain()
    {
        var readout = new DistanceReadout();
        readout.From(300f, 100f);
        readout.Advance(300f);
        readout.From(500f, 120f);
        readout.Reset();
        readout.Advance(600f);

        Assert.AreEqual(600f, readout.Shown(600f), "neither the change taken nor the one waiting survives");
    }
}
