using NUnit.Framework;

public class ExitScheduleTests
{
    [Test]
    public void APlacesExitIsAtItsEnd()
    {
        Assert.AreEqual(120, ExitSchedule.First(120, 22));
    }

    [Test]
    public void AMissedPlaceExitComesRoundAgain()
    {
        Assert.AreEqual(128, ExitSchedule.Next(120, 1, 120, 22, 1.5f, 8));
        Assert.AreEqual(136, ExitSchedule.Next(128, 2, 120, 22, 1.5f, 8));
    }

    // Unchanged from before places: 22, then gaps of 33, 50, 74.
    [Test]
    public void EndlessExitsGetRarer()
    {
        Assert.AreEqual(22, ExitSchedule.First(0, 22));
        Assert.AreEqual(55, ExitSchedule.Next(22, 1, 0, 22, 1.5f, 8));
        Assert.AreEqual(105, ExitSchedule.Next(55, 2, 0, 22, 1.5f, 8));
        Assert.AreEqual(179, ExitSchedule.Next(105, 3, 0, 22, 1.5f, 8));
    }
}
