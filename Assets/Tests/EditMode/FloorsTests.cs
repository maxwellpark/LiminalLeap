using System.Collections.Generic;
using System;
using NUnit.Framework;

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

    // As PlayerTrackMovement drives it: 3.4 radians of bob a metre, at 60 frames a second.
    private static (int Steps, bool AllOnLowPoints) Run(Stride stride, float speed, float seconds)
    {
        const float dt = 1f / 60f;
        var steps = 0;
        var onLow = true;
        var phase = 0f;
        for (var t = 0f; t < seconds; t += dt)
        {
            var before = phase;
            phase += speed * dt * 3.4f;
            if (stride.Step(before, phase, t))
            {
                steps++;
                onLow &= Stride.LowPoints(before, phase) > 0;
            }
        }

        return (steps, onLow);
    }

    [Test]
    public void LowPointsAreWhereTheBobIsDeepest()
    {
        // The bob is -|sin(phase)|.
        for (var k = 0; k < 6; k++)
        {
            Assert.AreEqual(-1f, -Math.Abs(Math.Sin(Stride.LowPoint + k * Math.PI)), 1e-5, "low point " + k);
        }

        Assert.AreEqual(0, Stride.LowPoints(0.1f, Stride.LowPoint - 0.05f));
        Assert.AreEqual(1, Stride.LowPoints(Stride.LowPoint - 0.05f, Stride.LowPoint + 0.05f));
        Assert.AreEqual(12, Stride.LowPoints(0f, 12f * (float)Math.PI));
    }

    [Test]
    public void StandingStillTakesNoSteps()
    {
        var stride = new Stride(4.5f);
        Assert.IsFalse(stride.Step(3f, 3f, 10f));
        Assert.IsFalse(stride.Step(3f, 2f, 10f));
    }

    [Test]
    public void AtSpeedStepsKeepARunnersCadence()
    {
        foreach (var speed in new[] { 8f, 15f, 22f })
        {
            var (steps, onLow) = Run(new Stride(4.5f), speed, 10f);
            Assert.That(steps / 10f, Is.InRange(3.4f, 4.5f), "speed " + speed);
            Assert.IsTrue(onLow, "a step off a low point at speed " + speed);
        }
    }

    [Test]
    public void SlowEnoughEveryLowPointIsAStep()
    {
        var (steps, _) = Run(new Stride(4.5f), 1f, 10f);
        Assert.AreEqual(Stride.LowPoints(0f, 34f), steps, 1);
    }

    [Test]
    public void ALandingHoldsOffTheNextStep()
    {
        var stride = new Stride(4f);
        stride.Landed(5f);
        Assert.IsFalse(stride.Step(Stride.LowPoint - 0.1f, Stride.LowPoint + 0.1f, 5.1f));
        Assert.IsTrue(stride.Step(Stride.LowPoint + (float)Math.PI - 0.1f, Stride.LowPoint + (float)Math.PI + 0.1f, 5.3f));
    }
}
