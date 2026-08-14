using NUnit.Framework;
using UnityEngine;

public class TrackChainerTests
{
    private const float Tolerance = 0.0001f;

    [Test]
    public void NextPiecePosition_SitsHalfALengthPastTheEnd()
    {
        var pos = TrackChainer.NextPiecePosition(new Vector3(0f, 0f, 10f), Vector3.forward, 4f);
        Assert.That(pos.z, Is.EqualTo(12f).Within(Tolerance));
    }

    [Test]
    public void NextPiecePosition_NormalisesForward()
    {
        var unit = TrackChainer.NextPiecePosition(Vector3.zero, Vector3.forward, 4f);
        var scaled = TrackChainer.NextPiecePosition(Vector3.zero, Vector3.forward * 37f, 4f);
        Assert.That(Vector3.Distance(unit, scaled), Is.LessThan(Tolerance));
    }

    [Test]
    public void NextPiecePosition_FollowsForwardOnOtherAxes()
    {
        var pos = TrackChainer.NextPiecePosition(Vector3.zero, Vector3.right, 6f);
        Assert.That(pos.x, Is.EqualTo(3f).Within(Tolerance));
        Assert.That(pos.z, Is.EqualTo(0f).Within(Tolerance));
    }

    [Test]
    public void ShouldRecycle_FalseWhilePlayerIsStillShortOfTheThreshold()
    {
        var recycle = TrackChainer.ShouldRecycle(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 5f), 6f);
        Assert.IsFalse(recycle);
    }

    [Test]
    public void ShouldRecycle_TrueOncePlayerIsPastTheThreshold()
    {
        var recycle = TrackChainer.ShouldRecycle(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 7f), 6f);
        Assert.IsTrue(recycle);
    }

    [Test]
    public void ShouldRecycle_FalseWhenPlayerIsBehindThePiece()
    {
        var recycle = TrackChainer.ShouldRecycle(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, -20f), 6f);
        Assert.IsFalse(recycle);
    }

    [Test]
    public void ShouldRecycle_IgnoresSidewaysDistance()
    {
        // only travel along forward should count, or a wide strafe would recycle the piece under you
        var recycle = TrackChainer.ShouldRecycle(Vector3.zero, Vector3.forward, new Vector3(500f, 0f, 1f), 6f);
        Assert.IsFalse(recycle);
    }

    [Test]
    public void ShouldRecycle_ExactlyAtThresholdDoesNotRecycle()
    {
        var recycle = TrackChainer.ShouldRecycle(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 6f), 6f);
        Assert.IsFalse(recycle);
    }
}
