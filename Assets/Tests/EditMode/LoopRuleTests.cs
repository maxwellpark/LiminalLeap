using NUnit.Framework;

public class LoopRuleTests
{
    [Test]
    public void NoForkInTheLeadIn()
    {
        var rule = new LoopRule(1, 24, 20);

        Assert.IsFalse(rule.Due(10, int.MinValue / 2));
        Assert.IsTrue(rule.Due(20, int.MinValue / 2));
    }

    [Test]
    public void ForksComeEveryStretch()
    {
        var rule = new LoopRule(1, 24, 20);

        Assert.IsFalse(rule.Due(30, 20));
        Assert.IsFalse(rule.Due(43, 20), "one piece early");
        Assert.IsTrue(rule.Due(44, 20));
        Assert.IsTrue(rule.Due(47, 20), "a fork that missed its slot is still due");
    }

    [Test]
    public void TheSameSeedLoopsTheSameDoors()
    {
        var a = new LoopRule(42, 24, 20);
        var b = new LoopRule(42, 24, 20);

        for (var fork = 0; fork < 50; fork++)
        {
            Assert.AreEqual(a.LoopDoor(fork), b.LoopDoor(fork), "fork " + fork);
        }
    }

    // Always the same door would make the fork a rule to learn once, not a choice.
    [Test]
    public void BothDoorsLoopSometimes()
    {
        var rule = new LoopRule(7, 24, 20);
        var left = 0;
        for (var fork = 0; fork < 200; fork++)
        {
            left += rule.LoopDoor(fork) == LoopRule.LeftDoor ? 1 : 0;
        }

        Assert.That(left, Is.InRange(60, 140));
    }

    [Test]
    public void SteeringPicksTheDoor()
    {
        Assert.AreEqual(LoopRule.LeftDoor, LoopRule.DoorFor(-2f));
        Assert.AreEqual(LoopRule.RightDoor, LoopRule.DoorFor(2f));
    }

    [Test]
    public void MemoryGivesBackTheLastPiecesInOrder()
    {
        var memory = new TrackMemory(4);
        for (var i = 0; i < 6; i++)
        {
            memory.Add(new TrackMemory.Piece(i, 100 + i));
        }

        var into = new TrackMemory.Piece[3];
        Assert.AreEqual(3, memory.CopyLast(3, into));
        Assert.AreEqual(new[] { 3, 4, 5 }, new[] { into[0].Prefab, into[1].Prefab, into[2].Prefab });
        Assert.AreEqual(105, into[2].Seed);
    }

    // Full, the oldest piece sits where the next one goes, which is where an index slips.
    [Test]
    public void AFullMemoryGivesBackEverythingInOrder()
    {
        var memory = new TrackMemory(4);
        for (var i = 0; i < 6; i++)
        {
            memory.Add(new TrackMemory.Piece(i, 0));
        }

        var into = new TrackMemory.Piece[4];
        Assert.AreEqual(4, memory.CopyLast(4, into));
        Assert.AreEqual(new[] { 2, 3, 4, 5 }, new[] { into[0].Prefab, into[1].Prefab, into[2].Prefab, into[3].Prefab });

        var short2 = new TrackMemory.Piece[2];
        Assert.AreEqual(2, memory.CopyLast(4, short2), "limited by where it's copied to");
        Assert.AreEqual(new[] { 4, 5 }, new[] { short2[0].Prefab, short2[1].Prefab });
    }

    [Test]
    public void MemoryGivesNoMoreThanItHas()
    {
        var memory = new TrackMemory(8);
        memory.Add(new TrackMemory.Piece(1, 1));
        memory.Add(new TrackMemory.Piece(2, 2));

        var into = new TrackMemory.Piece[5];
        Assert.AreEqual(2, memory.CopyLast(5, into));
        Assert.AreEqual(1, into[0].Prefab);
        Assert.AreEqual(2, into[1].Prefab);
        Assert.AreEqual(0, memory.CopyLast(-3, into), "a negative count is nothing, not a negative one");

        memory.Clear();
        Assert.AreEqual(0, memory.CopyLast(5, into));
    }
}
