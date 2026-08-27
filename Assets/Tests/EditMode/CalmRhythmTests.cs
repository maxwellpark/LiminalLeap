using NUnit.Framework;

public class CalmRhythmTests
{
    private const int Run = 10;
    private const int Calm = 4;

    [Test]
    public void TheRunStartsBusy()
    {
        for (var piece = 0; piece < Run; piece++)
        {
            Assert.IsFalse(CalmRhythm.IsCalm(piece, Run, Calm), "piece " + piece);
        }
    }

    [Test]
    public void CalmFollowsTheRun()
    {
        for (var piece = Run; piece < Run + Calm; piece++)
        {
            Assert.IsTrue(CalmRhythm.IsCalm(piece, Run, Calm), "piece " + piece);
        }
    }

    [Test]
    public void ItRepeats()
    {
        var cycle = Run + Calm;

        for (var piece = 0; piece < cycle * 4; piece++)
        {
            Assert.AreEqual(
                CalmRhythm.IsCalm(piece, Run, Calm),
                CalmRhythm.IsCalm(piece + cycle, Run, Calm),
                "piece " + piece);
        }
    }

    [Test]
    public void ZeroLengthCalmNeverFires()
    {
        for (var piece = 0; piece < 40; piece++)
        {
            Assert.IsFalse(CalmRhythm.IsCalm(piece, Run, 0));
        }
    }

    [Test]
    public void NegativePiecesAreNotCalm()
    {
        Assert.IsFalse(CalmRhythm.IsCalm(-1, Run, Calm));
    }

    [Test]
    public void ProgressRunsFromStartToEndOfTheStretch()
    {
        Assert.AreEqual(0f, CalmRhythm.Progress(Run, Run, Calm), 0.001f);
        Assert.AreEqual(1f, CalmRhythm.Progress(Run + Calm - 1, Run, Calm), 0.001f);
    }

    [Test]
    public void ProgressIsZeroWhileBusy()
    {
        Assert.AreEqual(0f, CalmRhythm.Progress(0, Run, Calm), 0.001f);
        Assert.AreEqual(0f, CalmRhythm.Progress(Run - 1, Run, Calm), 0.001f);
    }

    [Test]
    public void ProgressNeverLeavesTheUnitRange()
    {
        for (var piece = 0; piece < 200; piece++)
        {
            var p = CalmRhythm.Progress(piece, Run, Calm);
            Assert.GreaterOrEqual(p, 0f, "piece " + piece);
            Assert.LessOrEqual(p, 1f, "piece " + piece);
        }
    }

    [Test]
    public void CountdownReachesZeroExactlyWhenCalmStarts()
    {
        Assert.AreEqual(Run, CalmRhythm.PiecesUntilCalm(0, Run, Calm));
        Assert.AreEqual(1, CalmRhythm.PiecesUntilCalm(Run - 1, Run, Calm));
        Assert.AreEqual(0, CalmRhythm.PiecesUntilCalm(Run, Run, Calm));
    }

    [Test]
    public void ASingleCalmPieceIsFullyProgressed()
    {
        Assert.IsTrue(CalmRhythm.IsCalm(Run, Run, 1));
        Assert.AreEqual(1f, CalmRhythm.Progress(Run, Run, 1), 0.001f);
    }
}
