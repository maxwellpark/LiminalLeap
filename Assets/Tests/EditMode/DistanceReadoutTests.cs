using NUnit.Framework;

public class DistanceReadoutTests
{
    [Test]
    public void WithoutALoopItTellsTheTruth()
    {
        var readout = new DistanceReadout();

        Assert.AreEqual(412f, readout.Shown(412f));
    }

    [Test]
    public void ThroughALoopTheNumbersRepeat()
    {
        var readout = new DistanceReadout();
        readout.Loop(resumeAt: 300f, replayFrom: 200f);

        readout.Advance(300f);
        Assert.AreEqual(200f, readout.Shown(300f));
        readout.Advance(350f);
        Assert.AreEqual(250f, readout.Shown(350f), "it should count on in step");
    }

    [Test]
    public void ItCountsOnUntilTheReplayStarts()
    {
        var readout = new DistanceReadout();
        readout.Loop(resumeAt: 300f, replayFrom: 200f);

        readout.Advance(295f);
        Assert.AreEqual(295f, readout.Shown(295f), "still on the door's own piece");
        readout.Advance(300f);
        Assert.AreEqual(200f, readout.Shown(300f));
    }

    [Test]
    public void LoopsAddUp()
    {
        var readout = new DistanceReadout();
        readout.Loop(300f, 200f);
        readout.Advance(300f);
        readout.Loop(500f, 420f);
        readout.Advance(500f);

        Assert.AreEqual(320f, readout.Shown(500f));
        Assert.AreEqual(180f, readout.Behind);
    }

    [Test]
    public void ItNeverReadsBelowZeroOrRunsAhead()
    {
        var readout = new DistanceReadout();
        readout.Loop(100f, 0f);
        readout.Advance(100f);
        Assert.AreEqual(0f, readout.Shown(100f));

        readout.Loop(150f, 180f);
        readout.Advance(200f);
        Assert.AreEqual(100f, readout.Shown(200f), "a replay from further on mustn't push the readout ahead");
    }

    [Test]
    public void ResetStartsTruthfulAgain()
    {
        var readout = new DistanceReadout();
        readout.Loop(300f, 200f);
        readout.Advance(300f);
        readout.Loop(500f, 400f);
        readout.Reset();
        readout.Advance(600f);

        Assert.AreEqual(600f, readout.Shown(600f), "neither the rewind taken nor the one waiting survives");
    }
}
