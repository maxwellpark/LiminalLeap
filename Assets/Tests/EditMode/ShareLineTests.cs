using System.Globalization;
using System.Threading;
using NUnit.Framework;

public class ShareLineTests
{
    private static string[] Lines(string share)
    {
        return share.Split('\n');
    }

    [Test]
    public void ItOpensWithTheDay()
    {
        var share = ShareLine.For("2026-11-24", "the house", RunOutcome.Banked, 412f, 3120f, true);

        Assert.AreEqual("Liminal Leap daily 2026-11-24", Lines(share)[0]);
    }

    [Test]
    public void GettingOutSaysWhereAndKeepsTheScore()
    {
        var share = ShareLine.For("2026-11-24", "the house", RunOutcome.Banked, 412f, 3120f, true);

        Assert.AreEqual("the house · out at 412 m · 3,120", Lines(share)[2]);
    }

    // Dying with an exit available loses the score, so the line can't claim it.
    [Test]
    public void ALostScoreIsNotClaimed()
    {
        var share = ShareLine.For("2026-11-24", "the office", RunOutcome.Died, 230f, 1800f, false);

        Assert.AreEqual("the office · caught at 230 m", Lines(share)[2]);
        StringAssert.DoesNotContain("1,800", share);
    }

    [Test]
    public void NoPlaceLeavesItOut()
    {
        var share = ShareLine.For("2026-11-24", null, RunOutcome.Banked, 100f, 50f, true);

        Assert.AreEqual("out at 100 m · 50", Lines(share)[2]);
    }

    [Test]
    public void ItEndsWithTheLink()
    {
        var lines = Lines(ShareLine.For("2026-11-24", "the school", RunOutcome.Banked, 100f, 50f, true));

        Assert.AreEqual(ShareLine.Link, lines[lines.Length - 1]);
    }

    [Test]
    public void OneBlockPerStretchRoundedUp()
    {
        Assert.AreEqual(Repeat(9) + ShareLine.Door, ShareLine.Strip(RunOutcome.Banked, 412f));
        Assert.AreEqual(Repeat(1) + ShareLine.Eye, ShareLine.Strip(RunOutcome.Died, 3f));
        Assert.AreEqual(ShareLine.Eye, ShareLine.Strip(RunOutcome.Died, 0f));
    }

    [Test]
    public void ALongRunIsCappedRatherThanWrapping()
    {
        Assert.AreEqual(Repeat(ShareLine.MaxBlocks) + "+" + ShareLine.Door, ShareLine.Strip(RunOutcome.Banked, 5000f));
    }

    // Pasted into a chat where others read it, so it can't depend on the sender's locale.
    [Test]
    public void NumbersReadTheSameInAnyLocale()
    {
        var was = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            var share = ShareLine.For("2026-11-24", null, RunOutcome.Banked, 1412f, 3120f, true);

            StringAssert.Contains("out at 1,412 m · 3,120", share);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = was;
        }
    }

    private static string Repeat(int blocks)
    {
        var result = string.Empty;
        for (var i = 0; i < blocks; i++)
        {
            result += ShareLine.Block;
        }

        return result;
    }
}
