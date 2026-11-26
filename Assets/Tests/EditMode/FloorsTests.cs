using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class FloorsTests
{
    private static List<(Floor Floor, int Length)> Stretches(FloorRule rule, int pieces)
    {
        var stretches = new List<(Floor, int)>();
        for (var i = 0; i < pieces; i++)
        {
            var floor = rule.At(i);
            if (stretches.Count > 0 && stretches[^1].Item1 == floor)
            {
                stretches[^1] = (floor, stretches[^1].Item2 + 1);
            }
            else
            {
                stretches.Add((floor, 1));
            }
        }

        return stretches;
    }

    [Test]
    public void ARunOpensOnLinoleum()
    {
        foreach (var seed in new[] { 0, 1, 1234, -77 })
        {
            Assert.AreEqual(Floor.Linoleum, new FloorRule(seed, 8, 14).At(0), "seed " + seed);
        }
    }

    // The last stretch is cut short by where the count stops, so it's left out.
    [Test]
    public void StretchesRunEightToFourteenPieces()
    {
        var stretches = Stretches(new FloorRule(1234, 8, 14), 2000);
        stretches.RemoveAt(stretches.Count - 1);

        foreach (var (_, length) in stretches)
        {
            Assert.That(length, Is.InRange(8, 14));
        }
    }

    [Test]
    public void EveryFloorTurnsUp()
    {
        var seen = new HashSet<Floor>();
        foreach (var (floor, _) in Stretches(new FloorRule(99, 8, 14), 500))
        {
            seen.Add(floor);
        }

        Assert.AreEqual(4, seen.Count);
    }

    [Test]
    public void TheSameSeedLaysTheSameFloors()
    {
        var a = new FloorRule(42, 8, 14);
        var b = new FloorRule(42, 8, 14);
        for (var i = 0; i < 300; i++)
        {
            Assert.AreEqual(a.At(i), b.At(i), "piece " + i);
        }
    }

    [Test]
    public void AnotherSeedLaysOthers()
    {
        var a = new FloorRule(1, 8, 14);
        var b = new FloorRule(2, 8, 14);
        var differ = false;
        for (var i = 0; i < 300 && !differ; i++)
        {
            differ = a.At(i) != b.At(i);
        }

        Assert.IsTrue(differ);
    }

    // A new run asks from piece 0 again, and must get what a fresh rule would.
    [Test]
    public void AskingBackwardsGivesTheSameAnswer()
    {
        var rule = new FloorRule(7, 8, 14);
        var first = new Floor[200];
        for (var i = 0; i < first.Length; i++)
        {
            first[i] = rule.At(i);
        }

        for (var i = 0; i < first.Length; i++)
        {
            Assert.AreEqual(first[i], rule.At(i), "piece " + i);
        }

        Assert.AreEqual(first[150], rule.At(150));
        Assert.AreEqual(first[3], rule.At(3));
    }

    [Test]
    public void OnlyKeepsOneFloor()
    {
        var rule = FloorRule.Only(Floor.Boards);
        Assert.AreEqual(Floor.Boards, rule.At(0));
        Assert.AreEqual(Floor.Boards, rule.At(5000));
    }

    [Test]
    public void BadLengthsAreClampedToOneAndUp()
    {
        var stretches = Stretches(new FloorRule(5, 0, -3), 20);

        Assert.AreEqual(20, stretches.Count, "stretches of one piece each");
    }

    [Test]
    public void LinoleumMovesAsTheGameWasTuned()
    {
        var feel = FloorFeel.For(Floor.Linoleum);

        Assert.AreEqual(1f, feel.Grip);
        Assert.AreEqual(1f, feel.Bob);
        Assert.AreEqual(1f, feel.LandPitch);
        Assert.AreEqual(1f, feel.LandVolume);
    }

    [Test]
    public void EachOtherFloorChangesHowYouMove()
    {
        Assert.Less(FloorFeel.For(Floor.Tile).Grip, 0.5f, "wet tile should slide");
        Assert.Less(FloorFeel.For(Floor.Carpet).Bob, 1f, "carpet should soften the bob");
        Assert.Less(FloorFeel.For(Floor.Carpet).LandVolume, 1f, "carpet should muffle a landing");
        Assert.Greater(FloorFeel.For(Floor.Boards).Bob, 1f, "boards should bob deeper");
        Assert.Less(FloorFeel.For(Floor.Boards).LandPitch, 1f, "boards should knock lower");
    }

    [Test]
    public void AFootfallLandsAtTheBobsLowPoint()
    {
        var low = Mathf.PI * 0.5f;
        Assert.AreEqual(0, Stride.Footfalls(0.1f, low - 0.05f));
        Assert.AreEqual(1, Stride.Footfalls(low - 0.05f, low + 0.05f));
        Assert.AreEqual(0, Stride.Footfalls(low + 0.05f, low + Mathf.PI - 0.05f));

        // The bob is -|sin(phase)|, deepest there.
        Assert.AreEqual(-1f, -Mathf.Abs(Mathf.Sin(low)), 1e-6f);
    }

    [Test]
    public void StandingStillTakesNoSteps()
    {
        Assert.AreEqual(0, Stride.Footfalls(3f, 3f));
        Assert.AreEqual(0, Stride.Footfalls(3f, 2f));
    }

    [Test]
    public void FramesCountTheSameStepsAsOneLongStride()
    {
        foreach (var frame in new[] { 0.013f, 0.05f, 0.31f, 1.7f })
        {
            var total = 0;
            var phase = 0f;
            while (phase + frame <= 40f)
            {
                total += Stride.Footfalls(phase, phase + frame);
                phase += frame;
            }

            Assert.AreEqual(Stride.Footfalls(0f, phase), total, "frame " + frame);
        }

        Assert.AreEqual(12, Stride.Footfalls(0f, 12f * Mathf.PI));
    }
}
