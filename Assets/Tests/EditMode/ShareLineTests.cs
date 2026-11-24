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
    public void EachStretchShowsWhatHappenedThere()
    {
        var marks = new[] { StretchMark.Quiet, StretchMark.Dodged, StretchMark.NearMiss, StretchMark.LookedBack };

        Assert.AreEqual(
            ShareLine.Block + ShareLine.Dodged + ShareLine.NearMiss + ShareLine.LookedBack + ShareLine.Door,
            ShareLine.Strip(RunOutcome.Banked, 200f, marks));
    }

    [Test]
    public void ThePreviewColoursEachStretchToMatch()
    {
        var marks = new[] { StretchMark.Dodged, StretchMark.Quiet };
        var preview = ShareLine.Preview(80f, marks);

        Assert.AreEqual(
            "<color=" + ShareLine.Colour(StretchMark.Dodged) + ">\u25A1</color>"
            + "<color=" + ShareLine.Colour(StretchMark.Quiet) + ">\u25A1</color>",
            preview);
        Assert.AreNotEqual(ShareLine.Colour(StretchMark.Dodged), ShareLine.Colour(StretchMark.Quiet));
    }

    // Only the square the font has, or the summary shows boxes for missing glyphs.
    [Test]
    public void ThePreviewUsesNoEmoji()
    {
        var preview = ShareLine.Preview(5000f, new[] { StretchMark.Dodged, StretchMark.NearMiss, StretchMark.LookedBack });
        foreach (var c in preview)
        {
            Assert.IsFalse(char.IsSurrogate(c), "an emoji got into the preview");
        }

        StringAssert.EndsWith("+", preview);
    }

    [Test]
    public void APlaceLineShowsEachCallAndEndsOnTheDoor()
    {
        var lines = Lines(ShareLine.ForPlace("the office", new[] { true, false, true, true }));

        Assert.AreEqual("Liminal Leap \u00B7 the office", lines[0]);
        Assert.AreEqual(ShareLine.RightCall + ShareLine.WrongCall + ShareLine.RightCall + ShareLine.RightCall + ShareLine.Door, lines[1]);
        Assert.AreEqual("out after 4 laps \u00B7 3 right, 1 wrong", lines[2]);
        Assert.AreEqual(ShareLine.Link, lines[3]);
    }

    [Test]
    public void OneLapSaysLap()
    {
        StringAssert.Contains("out after 1 lap \u00B7", ShareLine.ForPlace("the school", new[] { true }));
    }

    [Test]
    public void ThePlacePreviewMatchesTheCalls()
    {
        var preview = ShareLine.PlacePreview(new[] { true, false });

        Assert.AreEqual(2, preview.Split('\u25A1').Length - 1);
        StringAssert.Contains(ShareLine.Colour(StretchMark.Dodged), preview, "a wrong call should be red like a dodge");
    }

    [Test]
    public void TheMarksReachTheCopiedLine()
    {
        var share = ShareLine.For("2026-11-24", null, RunOutcome.Died, 60f, 0f, false,
            new[] { StretchMark.NearMiss, StretchMark.Dodged });

        Assert.AreEqual(ShareLine.NearMiss + ShareLine.Dodged + ShareLine.Eye, share.Split('\n')[1]);
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
