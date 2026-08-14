using System.Collections.Generic;
using NUnit.Framework;

public class HazardLanesTests
{
    private const float TrackHalf = 3f;
    private const float PlayerHalf = 0.6f;

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
    public void EmptyTrackHasAGap()
    {
        Assert.IsTrue(HazardLanes.HasGap(Spans(), TrackHalf, PlayerHalf));
    }

    [Test]
    public void OneBlockerLeavesRoomEitherSide()
    {
        Assert.IsTrue(HazardLanes.HasGap(Spans((0f, 0.9f)), TrackHalf, PlayerHalf));
    }

    [Test]
    public void ABlockerSpanningTheTrackSealsIt()
    {
        Assert.IsFalse(HazardLanes.HasGap(Spans((0f, 3f)), TrackHalf, PlayerHalf));
    }

    [Test]
    public void TwoBlockersCanStillLeaveAMiddleLane()
    {
        Assert.IsTrue(HazardLanes.HasGap(Spans((-2.2f, 0.8f), (2.2f, 0.8f)), TrackHalf, PlayerHalf));
    }

    [Test]
    public void ClosingTheMiddleStillLeavesTheEdges()
    {
        // the 0.8 gap between them is too narrow, but -3..-1.6 on the outside is not
        var spans = Spans((-1f, 0.6f), (1f, 0.6f));
        Assert.IsTrue(HazardLanes.HasGap(spans, TrackHalf, PlayerHalf));
        Assert.AreEqual(1.4f, HazardLanes.WidestGap(spans, TrackHalf), 1e-4f);
    }

    [Test]
    public void SealingTheEdgesAndMiddleLeavesNothing()
    {
        // middle gap 0.8, edges covered out to the rails
        var spans = Spans((-2.3f, 0.9f), (0f, 0.5f), (2.3f, 0.9f));
        Assert.IsFalse(HazardLanes.HasGap(spans, TrackHalf, PlayerHalf));
    }

    [Test]
    public void OverlappingBlockersAreTreatedAsOne()
    {
        Assert.IsFalse(HazardLanes.HasGap(Spans((-0.5f, 2.6f), (0.5f, 2.6f)), TrackHalf, PlayerHalf));
    }

    [Test]
    public void UnorderedInputGivesTheSameAnswer()
    {
        var forward = HazardLanes.HasGap(Spans((-2.2f, 0.8f), (2.2f, 0.8f)), TrackHalf, PlayerHalf);
        var reversed = HazardLanes.HasGap(Spans((2.2f, 0.8f), (-2.2f, 0.8f)), TrackHalf, PlayerHalf);
        Assert.AreEqual(forward, reversed);
    }

    [Test]
    public void AGapExactlyThePlayersWidthCounts()
    {
        // free lane from -3 to -1.8 is exactly 1.2 wide
        Assert.IsTrue(HazardLanes.HasGap(Spans((0f, 1.8f)), TrackHalf, PlayerHalf));
    }

    [Test]
    public void WidestGapFindsTheEdgeLane()
    {
        var widest = HazardLanes.WidestGap(Spans((-1f, 0.5f)), TrackHalf);
        Assert.AreEqual(3.5f, widest, 1e-4f); // -0.5 up to +3
    }

    [Test]
    public void WidestGapOnAnEmptyTrackIsTheWholeWidth()
    {
        Assert.AreEqual(6f, HazardLanes.WidestGap(Spans(), TrackHalf), 1e-4f);
    }

    [Test]
    public void WidestGapIsZeroWhenSealed()
    {
        Assert.AreEqual(0f, HazardLanes.WidestGap(Spans((0f, 5f)), TrackHalf), 1e-4f);
    }
}
