using NUnit.Framework;
using UnityEngine;

public class UtilsTests
{
    [Test]
    public void ApproximatelyEquals_TrueForIdenticalVectors()
    {
        Assert.IsTrue(new Vector3(1f, 2f, 3f).ApproximatelyEquals(new Vector3(1f, 2f, 3f)));
    }

    [Test]
    public void ApproximatelyEquals_TrueJustInsideTolerance()
    {
        Assert.IsTrue(Vector3.zero.ApproximatelyEquals(new Vector3(0.0009f, 0f, 0f)));
    }

    [Test]
    public void ApproximatelyEquals_FalseJustOutsideTolerance()
    {
        Assert.IsFalse(Vector3.zero.ApproximatelyEquals(new Vector3(0.0011f, 0f, 0f)));
    }

    [Test]
    public void ApproximatelyEquals_RespectsACustomTolerance()
    {
        var a = Vector3.zero;
        var b = new Vector3(0f, 0f, 0.5f);
        Assert.IsTrue(a.ApproximatelyEquals(b, 1f));
        Assert.IsFalse(a.ApproximatelyEquals(b, 0.25f));
    }

    // squared comparison must order the same as the old Distance one, including diagonals
    [Test]
    public void ApproximatelyEquals_MatchesDistanceOnDiagonals()
    {
        var a = Vector3.zero;
        var b = new Vector3(0.5f, 0.5f, 0.5f);
        const float tolerance = 0.9f;
        Assert.AreEqual(Vector3.Distance(a, b) <= tolerance, a.ApproximatelyEquals(b, tolerance));
    }

    [Test]
    public void ApproximatelyEquals_IsSymmetric()
    {
        var a = new Vector3(3f, -1f, 7f);
        var b = new Vector3(3f, -1f, 7.0005f);
        Assert.AreEqual(a.ApproximatelyEquals(b), b.ApproximatelyEquals(a));
    }
}
