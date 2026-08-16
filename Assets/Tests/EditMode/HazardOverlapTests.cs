using System.Collections.Generic;
using NUnit.Framework;

public class HazardOverlapTests
{
    private static List<HazardLanes.Span> Spans(params (float centre, float half)[] items)
    {
        var list = new List<HazardLanes.Span>();
        foreach (var (centre, half) in items)
        {
            list.Add(new HazardLanes.Span(centre, half));
        }
        return list;
    }

    [Test]
    public void NothingToOverlapIsFine()
    {
        Assert.IsFalse(HazardLanes.Overlaps(new HazardLanes.Span(0f, 0.9f), Spans(), 0.6f));
    }

    [Test]
    public void IdenticalSpansOverlap()
    {
        Assert.IsTrue(HazardLanes.Overlaps(new HazardLanes.Span(0f, 0.9f), Spans((0f, 0.9f)), 0f));
    }

    [Test]
    public void PartialOverlapIsCaught()
    {
        // -0.9..0.9 against 0.1..1.9
        Assert.IsTrue(HazardLanes.Overlaps(new HazardLanes.Span(0f, 0.9f), Spans((1f, 0.9f)), 0f));
    }

    [Test]
    public void ClearlySeparatedSpansDoNot()
    {
        Assert.IsFalse(HazardLanes.Overlaps(new HazardLanes.Span(-2.5f, 0.5f), Spans((2.5f, 0.5f)), 0.6f));
    }

    [Test]
    public void TouchingSpansCountAsOverlappingOnceAGapIsRequired()
    {
        // 0.9 apart edge to edge, but we want 0.6 of clear air between them
        var candidate = new HazardLanes.Span(1.9f, 0.9f); // 1.0..2.8
        Assert.IsFalse(HazardLanes.Overlaps(candidate, Spans((0f, 0.9f)), 0f), "edges only just clear");
        Assert.IsTrue(HazardLanes.Overlaps(candidate, Spans((0f, 0.9f)), 0.6f), "0.1 of air is less than the 0.6 wanted");
    }

    [Test]
    public void ItChecksEveryExistingSpan()
    {
        var existing = Spans((-2.5f, 0.5f), (0f, 0.5f), (2.5f, 0.5f));
        Assert.IsTrue(HazardLanes.Overlaps(new HazardLanes.Span(0.2f, 0.5f), existing, 0f), "middle collision missed");
    }

    [Test]
    public void NegativeGapIsTreatedAsZero()
    {
        Assert.IsFalse(HazardLanes.Overlaps(new HazardLanes.Span(2f, 0.5f), Spans((0f, 0.5f)), -5f));
    }

    [Test]
    public void NullListIsSafe()
    {
        Assert.IsFalse(HazardLanes.Overlaps(new HazardLanes.Span(0f, 1f), null, 0.5f));
    }

    // The two rules are independent: a row can be passable and still self-intersect.
    [Test]
    public void PassableRowsCanStillOverlapEachOther()
    {
        var stacked = Spans((0f, 0.9f), (0.3f, 0.9f));
        Assert.IsTrue(HazardLanes.HasGap(stacked, 3f, 0.6f), "edges are open, so it is passable");
        Assert.IsTrue(HazardLanes.Overlaps(stacked[1], Spans((0f, 0.9f)), 0f), "but the two pieces intersect");
    }
}
