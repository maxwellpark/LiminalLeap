using System;
using NUnit.Framework;

// Apart from DailySeedTests, which test RunMode too, so these run under plain .NET.
public class DailyPickTests
{
    [Test]
    public void AKeyAndItsSeedAlwaysComeFromTheSameDay()
    {
        var midnight = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        foreach (var at in new[] { midnight.AddMilliseconds(-1), midnight, midnight.AddMilliseconds(1) })
        {
            var (key, seed) = DailySeed.Pick(at);
            var day = DateTime.SpecifyKind(DateTime.ParseExact(key, "yyyy-MM-dd", null), DateTimeKind.Utc);
            Assert.AreEqual(DailySeed.For(day), seed, at.ToString("o"));
        }

        Assert.AreEqual("2026-08-27", DailySeed.Pick(midnight.AddMilliseconds(-1)).Key);
        Assert.AreEqual("2026-08-28", DailySeed.Pick(midnight).Key);
    }
}
