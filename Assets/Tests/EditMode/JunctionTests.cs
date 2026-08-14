using NUnit.Framework;

public class JunctionTests
{
    private const float Deadzone = 1f;

    [Test]
    public void TwoBranches_LeftOfCentreTakesTheLeft()
    {
        Assert.AreEqual(0, Junction.ChooseBranch(-0.01f, 2, Deadzone));
    }

    [Test]
    public void TwoBranches_RightOfCentreTakesTheRight()
    {
        Assert.AreEqual(1, Junction.ChooseBranch(0.01f, 2, Deadzone));
    }

    [Test]
    public void TwoBranches_DeadCentreTakesTheRight()
    {
        // no middle lane to fall into, so centre has to resolve one way, document which
        Assert.AreEqual(1, Junction.ChooseBranch(0f, 2, Deadzone));
    }

    [Test]
    public void TwoBranches_IgnoresTheDeadzone()
    {
        Assert.AreEqual(0, Junction.ChooseBranch(-0.5f, 2, Deadzone));
    }

    [Test]
    public void ThreeBranches_CentreTakesTheMiddle()
    {
        Assert.AreEqual(1, Junction.ChooseBranch(0f, 3, Deadzone));
    }

    [Test]
    public void ThreeBranches_InsideTheDeadzoneStaysMiddle()
    {
        Assert.AreEqual(1, Junction.ChooseBranch(-0.9f, 3, Deadzone));
        Assert.AreEqual(1, Junction.ChooseBranch(0.9f, 3, Deadzone));
    }

    [Test]
    public void ThreeBranches_OutsideTheDeadzonePicksASide()
    {
        Assert.AreEqual(0, Junction.ChooseBranch(-1.1f, 3, Deadzone));
        Assert.AreEqual(2, Junction.ChooseBranch(1.1f, 3, Deadzone));
    }

    [Test]
    public void ThreeBranches_OnTheDeadzoneEdgeStaysMiddle()
    {
        Assert.AreEqual(1, Junction.ChooseBranch(-Deadzone, 3, Deadzone));
        Assert.AreEqual(1, Junction.ChooseBranch(Deadzone, 3, Deadzone));
    }

    [Test]
    public void FourBranches_OuterLanesAreTheEnds()
    {
        Assert.AreEqual(0, Junction.ChooseBranch(-5f, 4, Deadzone));
        Assert.AreEqual(3, Junction.ChooseBranch(5f, 4, Deadzone));
    }

    // Known limit: only left/middle/right exist, so a 4-way can never select index 2.
    // Fine while junctions are 2 or 3 wide; needs real lane bands if that changes.
    [Test]
    public void FourBranches_CannotReachTheSecondMiddleLane()
    {
        var reachable = new System.Collections.Generic.HashSet<int>();
        for (var lateral = -5f; lateral <= 5f; lateral += 0.1f)
        {
            reachable.Add(Junction.ChooseBranch(lateral, 4, Deadzone));
        }

        Assert.IsFalse(reachable.Contains(2));
        CollectionAssert.AreEquivalent(new[] { 0, 1, 3 }, reachable);
    }

    [Test]
    public void SingleBranch_AlwaysResolvesToTheOnlyOne()
    {
        Assert.AreEqual(0, Junction.ChooseBranch(-5f, 1, Deadzone));
        Assert.AreEqual(0, Junction.ChooseBranch(5f, 1, Deadzone));
    }
}
