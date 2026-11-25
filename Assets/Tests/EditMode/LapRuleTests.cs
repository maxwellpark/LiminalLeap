using NUnit.Framework;

public class LapRuleTests
{
    private static LapRule Rule(int required = 3, float chance = 0.5f, int seed = 7)
    {
        return new LapRule(seed, required, chance);
    }

    private static int FirstWrongLap(LapRule rule, int from)
    {
        for (var lap = from; lap < from + 200; lap++)
        {
            if (rule.HasAnomaly(lap))
            {
                return lap;
            }
        }

        Assert.Fail("no anomaly in 200 laps, so this proves nothing");
        return -1;
    }

    private static int FirstCleanLap(LapRule rule, int from)
    {
        for (var lap = from; lap < from + 200; lap++)
        {
            if (!rule.HasAnomaly(lap))
            {
                return lap;
            }
        }

        Assert.Fail("no clean lap in 200 laps, so this proves nothing");
        return -1;
    }

    // Walks the rule to a lap by calling every lap right.
    private static void PlayRightUntil(LapRule rule, int lap)
    {
        while (rule.Lap < lap)
        {
            var call = rule.Call(!rule.HasAnomaly(rule.Lap));
            if (call == LapCall.Leave)
            {
                Assert.Fail("left before reaching the lap under test");
            }
        }
    }

    [Test]
    public void TheFirstLapIsAlwaysClean()
    {
        for (var seed = 0; seed < 50; seed++)
        {
            Assert.IsFalse(new LapRule(seed, 3, 1f).HasAnomaly(0), "seed " + seed);
        }
    }

    [Test]
    public void TakingACleanExitCounts()
    {
        var rule = Rule();

        Assert.AreEqual(LapCall.Through, rule.Call(tookExit: true));
        Assert.AreEqual(1, rule.Streak);
        Assert.AreEqual(1, rule.Lap);
    }

    [Test]
    public void RunningPastACleanExitIsWrong()
    {
        var rule = Rule();

        Assert.AreEqual(LapCall.Wrong, rule.Call(tookExit: false));
        Assert.AreEqual(0, rule.Streak);
    }

    [Test]
    public void RunningPastAnAnomalyCounts()
    {
        var rule = Rule();
        PlayRightUntil(rule, FirstWrongLap(rule, 1));

        var before = rule.Streak;
        Assert.AreEqual(LapCall.Onward, rule.Call(tookExit: false));
        Assert.AreEqual(before + 1, rule.Streak);
    }

    [Test]
    public void TakingTheExitPastAnAnomalyLosesTheCount()
    {
        var rule = Rule(required: 50);
        PlayRightUntil(rule, FirstWrongLap(rule, 1));
        Assert.Greater(rule.Streak, 0, "needs a count to lose, or this proves nothing");

        Assert.AreEqual(LapCall.Wrong, rule.Call(tookExit: true));
        Assert.AreEqual(0, rule.Streak);
    }

    // Otherwise a wrong call could land you straight on another anomaly with no baseline.
    [Test]
    public void AWrongCallStartsAgainOnACleanLap()
    {
        var rule = Rule(chance: 1f);
        rule.Call(tookExit: false);

        Assert.AreEqual(rule.Lap, rule.CleanLap);
        Assert.IsFalse(rule.HasAnomaly(rule.Lap));
        Assert.IsTrue(rule.HasAnomaly(rule.Lap + 1), "with chance 1 only the clean lap should be clean");
    }

    [Test]
    public void EnoughRightCallsEndingOnATakeLeaves()
    {
        var rule = Rule(required: 2, chance: 0f);

        Assert.AreEqual(LapCall.Through, rule.Call(tookExit: true));
        Assert.AreEqual(LapCall.Leave, rule.Call(tookExit: true));
    }

    // Leaving is always a take: a count reached by running past waits for the next clean exit.
    [Test]
    public void ACountReachedByRunningPastWaitsForACleanExit()
    {
        LapRule rule = null;
        for (var seed = 0; seed < 200 && rule == null; seed++)
        {
            var candidate = new LapRule(seed, 2, 0.5f);
            if (candidate.HasAnomaly(1))
            {
                rule = candidate;
            }
        }

        Assert.IsNotNull(rule, "no seed with an anomaly on lap 1, so this proves nothing");

        Assert.AreEqual(LapCall.Through, rule.Call(tookExit: true));
        Assert.AreEqual(LapCall.Onward, rule.Call(tookExit: false));
        Assert.AreEqual(2, rule.Streak);

        while (rule.HasAnomaly(rule.Lap))
        {
            Assert.AreEqual(LapCall.Onward, rule.Call(tookExit: false));
        }

        Assert.AreEqual(LapCall.Leave, rule.Call(tookExit: true));
    }

    [Test]
    public void TheSameSeedPicksTheSameLaps()
    {
        var a = Rule(seed: 99);
        var b = Rule(seed: 99);

        for (var lap = 1; lap < 40; lap++)
        {
            Assert.AreEqual(a.KindFor(lap), b.KindFor(lap), "lap " + lap);
            Assert.AreEqual(a.StretchStart(lap, 50), b.StretchStart(lap, 50), "lap " + lap);
        }
    }

    // Mostly honest, or the clean exit is a lottery. Close to the chance over many laps.
    [Test]
    public void TheChanceHoldsOverManyLaps()
    {
        var rule = Rule(chance: 0.5f, seed: 3);
        var wrong = 0;
        for (var lap = 1; lap <= 2000; lap++)
        {
            wrong += rule.HasAnomaly(lap) ? 1 : 0;
        }

        Assert.That(wrong, Is.InRange(850, 1150));
    }

    [Test]
    public void EveryKindTurnsUp()
    {
        var rule = Rule(chance: 1f, seed: 11);
        var seen = new System.Collections.Generic.HashSet<AnomalyKind>();
        for (var lap = 1; lap < 400; lap++)
        {
            seen.Add(rule.KindFor(lap));
        }

        Assert.AreEqual(4, seen.Count, string.Join(",", seen));
        Assert.IsFalse(seen.Contains(AnomalyKind.None));
    }

    [Test]
    public void TheStretchStaysClearOfBothEnds()
    {
        var rule = Rule(chance: 1f);
        for (var lap = 1; lap < 300; lap++)
        {
            var start = rule.StretchStart(lap, 50);
            Assert.GreaterOrEqual(start, 50 / 4, "lap " + lap);
            Assert.LessOrEqual(start + LapRule.StretchPieces, 50 * 3 / 4, "lap " + lap);
        }
    }

    [Test]
    public void ACountAnomalyHasNoStretch()
    {
        var rule = Rule(chance: 1f, seed: 11);
        for (var lap = 1; lap < 200; lap++)
        {
            if (rule.KindFor(lap) != AnomalyKind.WrongCount)
            {
                continue;
            }

            for (var piece = 0; piece < 50; piece++)
            {
                Assert.IsFalse(rule.InStretch(lap, piece, 50));
            }

            return;
        }

        Assert.Fail("no wrong count in 200 laps, so this proves nothing");
    }

    [Test]
    public void TheCountIsHonestExceptOnAWrongCountLap()
    {
        var rule = Rule(chance: 1f, seed: 11);
        rule.Call(tookExit: true);
        var sawWrong = false;

        for (var lap = 1; lap < 200; lap++)
        {
            var wrong = rule.KindFor(lap) == AnomalyKind.WrongCount;
            sawWrong |= wrong;
            var expected = rule.Streak + (lap - rule.Lap);
            Assert.AreEqual(wrong ? expected + 2 : expected, rule.CountShown(lap), "lap " + lap);
        }

        Assert.IsTrue(sawWrong, "no wrong count lap, so this proves nothing");
        Assert.AreEqual(rule.Streak, rule.CountShown(rule.CleanLap), "the clean lap has to tell the truth");
    }

    // Laid ahead of the call, the next lap's number has to be right already, or it ticks in view.
    [Test]
    public void TheNextLapAlreadyShowsTheCountItWillHave()
    {
        var rule = Rule(required: 9, chance: 0f);
        var ahead = rule.CountShown(rule.Lap + 1);

        rule.Call(tookExit: true);
        Assert.AreEqual(ahead, rule.CountShown(rule.Lap), "the number changed once the call was made");
    }

    [Test]
    public void EveryCallIsKept()
    {
        var rule = Rule(required: 2, chance: 0f);
        rule.Call(tookExit: false);
        rule.Call(tookExit: true);
        rule.Call(tookExit: true);

        CollectionAssert.AreEqual(new[] { false, true, true }, rule.History);
    }

    // The pieces that look wrong are exactly the four from the stretch's start.
    [Test]
    public void AWrongLapIsWrongOnlyInsideItsStretch()
    {
        var rule = Rule(chance: 1f, seed: 11);
        for (var lap = 1; lap < 200; lap++)
        {
            var kind = rule.KindFor(lap);
            if (kind == AnomalyKind.WrongCount)
            {
                continue;
            }

            var start = rule.StretchStart(lap, 40);
            for (var piece = 0; piece < 40; piece++)
            {
                var inside = piece >= start && piece < start + LapRule.StretchPieces;
                Assert.AreEqual(inside, rule.InStretch(lap, piece, 40), $"lap {lap} piece {piece}");
            }

            return;
        }

        Assert.Fail("no scenery anomaly in 200 laps, so this proves nothing");
    }

    [Test]
    public void ACleanLapHasNoStretch()
    {
        var rule = Rule();
        for (var piece = 0; piece < 50; piece++)
        {
            Assert.IsFalse(rule.InStretch(0, piece, 50));
        }
    }
}
