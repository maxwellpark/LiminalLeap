using NUnit.Framework;

public class RunStripTests
{
    [Test]
    public void EachFiftyMetresIsItsOwnStretch()
    {
        var strip = new RunStrip();
        strip.Mark(10f, StretchMark.NearMiss);
        strip.Mark(60f, StretchMark.Dodged);

        Assert.AreEqual(StretchMark.NearMiss, strip.Marks[0]);
        Assert.AreEqual(StretchMark.Dodged, strip.Marks[1]);
        Assert.AreEqual(StretchMark.Quiet, strip.Marks[2]);
    }

    // Looking back is marked every frame, so it mustn't drown out a dodge in the same stretch.
    [Test]
    public void TheLouderMarkWins()
    {
        var strip = new RunStrip();
        strip.Mark(20f, StretchMark.Dodged);
        strip.Mark(21f, StretchMark.LookedBack);
        strip.Mark(22f, StretchMark.NearMiss);

        Assert.AreEqual(StretchMark.Dodged, strip.Marks[0]);
    }

    [Test]
    public void PastTheCapIsIgnoredNotWrapped()
    {
        var strip = new RunStrip();
        strip.Mark(RunStrip.MetresPerStretch * RunStrip.MaxStretches + 5f, StretchMark.Dodged);
        strip.Mark(-30f, StretchMark.NearMiss);

        Assert.AreEqual(StretchMark.NearMiss, strip.Marks[0]);
        for (var i = 1; i < strip.Marks.Count; i++)
        {
            Assert.AreEqual(StretchMark.Quiet, strip.Marks[i], "stretch " + i);
        }
    }

    [Test]
    public void ResetClearsTheRun()
    {
        var strip = new RunStrip();
        strip.Mark(10f, StretchMark.Dodged);
        strip.Reset();

        Assert.AreEqual(StretchMark.Quiet, strip.Marks[0]);
    }
}
