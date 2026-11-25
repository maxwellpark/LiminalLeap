using NUnit.Framework;

public class PlaceMapTests
{
    [Test]
    public void AMarkIsNewOnlyTheFirstTime()
    {
        var map = new PlaceMap { Id = "office" };

        Assert.IsTrue(map.Mark(12, MapMarkKind.Anomaly, (int)AnomalyKind.WrongLight));
        Assert.IsFalse(map.Mark(12, MapMarkKind.Anomaly, (int)AnomalyKind.WrongLight), "seen again isn't news");
        Assert.AreEqual(1, map.Marks.Count);
    }

    // Two anomalies at one spot are two things learned, not one.
    [Test]
    public void TheSameSpotCanHoldDifferentMarks()
    {
        var map = new PlaceMap();

        Assert.IsTrue(map.Mark(12, MapMarkKind.Anomaly, (int)AnomalyKind.WrongLight));
        Assert.IsTrue(map.Mark(12, MapMarkKind.Anomaly, (int)AnomalyKind.LowLights));
        Assert.IsTrue(map.Mark(12, MapMarkKind.Floor, (int)Floor.Carpet));
        Assert.AreEqual(2, map.Count(MapMarkKind.Anomaly));
        Assert.AreEqual(1, map.Count(MapMarkKind.Floor));
    }

    [Test]
    public void ANegativePlaceIsNotMarked()
    {
        var map = new PlaceMap();

        Assert.IsFalse(map.Mark(-1, MapMarkKind.Fork));
        Assert.AreEqual(0, map.Marks.Count);
    }

    [Test]
    public void ADoorIsKnownOnceTakenEitherWay()
    {
        var map = new PlaceMap();
        Assert.IsTrue(map.Mark(20, MapMarkKind.Fork));
        Assert.IsFalse(map.KnowsWhereDoorLeads(20, LoopRule.LeftDoor));

        map.Mark(20, MapMarkKind.LoopingDoor, LoopRule.LeftDoor);
        Assert.IsTrue(map.KnowsWhereDoorLeads(20, LoopRule.LeftDoor));
        Assert.IsFalse(map.KnowsWhereDoorLeads(20, LoopRule.RightDoor), "the other door is still a guess");

        map.Mark(20, MapMarkKind.OnwardDoor, LoopRule.RightDoor);
        Assert.IsTrue(map.KnowsWhereDoorLeads(20, LoopRule.RightDoor));
        Assert.IsFalse(map.KnowsWhereDoorLeads(21, LoopRule.RightDoor), "another fork's doors are their own");
    }
}
