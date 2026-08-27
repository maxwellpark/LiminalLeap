using System;
using NUnit.Framework;

public class DailySeedTests
{
    private static readonly DateTime Day = new(2026, 8, 27, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public void TheSameDayGivesTheSameSeed()
    {
        Assert.AreEqual(DailySeed.For(Day), DailySeed.For(Day.AddHours(6)),
            "everyone has to get the same corridor or comparing scores is meaningless");
    }

    [Test]
    public void DifferentDaysGiveDifferentSeeds()
    {
        Assert.AreNotEqual(DailySeed.For(Day), DailySeed.For(Day.AddDays(1)));
    }

    // Raw ordinals would put consecutive days next to each other in the sequence, and
    // today looking like yesterday defeats the point of a daily.
    [Test]
    public void ConsecutiveDaysAreNotAdjacentSeeds()
    {
        for (var i = 0; i < 30; i++)
        {
            var a = DailySeed.For(Day.AddDays(i));
            var b = DailySeed.For(Day.AddDays(i + 1));
            Assert.Greater(Math.Abs((long)a - b), 1000, $"day {i} and {i + 1} are too close");
        }
    }

    [Test]
    public void SeedsAreNeverNegative()
    {
        for (var i = -400; i < 400; i++)
        {
            Assert.GreaterOrEqual(DailySeed.For(Day.AddDays(i)), 0, $"day offset {i}");
        }
    }

    [Test]
    public void TheKeyIsTheUtcDate()
    {
        Assert.AreEqual("2026-08-27", DailySeed.KeyFor(Day));
    }

    // Two players either side of midnight in different zones must agree on the day.
    [Test]
    public void TheKeyIsTheSameWhateverTheLocalOffset()
    {
        var utc = new DateTime(2026, 8, 27, 23, 30, 0, DateTimeKind.Utc);
        var elsewhere = new DateTimeOffset(utc).ToOffset(TimeSpan.FromHours(9)).UtcDateTime;

        Assert.AreEqual(DailySeed.KeyFor(utc), DailySeed.KeyFor(elsewhere));
        Assert.AreEqual(DailySeed.For(utc), DailySeed.For(elsewhere));
    }

    [Test]
    public void ChoosingDailySetsTheSeedAndDay()
    {
        RunMode.ChooseDaily();

        Assert.IsTrue(RunMode.Daily);
        Assert.AreEqual(DailySeed.Today(), RunMode.Seed);
        Assert.AreEqual(DailySeed.TodayKey(), RunMode.Day);

        RunMode.ChooseFree();
        Assert.IsFalse(RunMode.Daily);
        Assert.IsEmpty(RunMode.Day);
    }
}
