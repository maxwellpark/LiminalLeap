using NUnit.Framework;

public class ControlsTests
{
    [Test]
    public void TheTitleNamesEveryKeyARunUses()
    {
        foreach (var key in new[] { "A D", "SPACE", "SHIFT", "Q", "E", "hold R", "P" })
        {
            StringAssert.Contains(key, Controls.Summary, key);
        }
    }

    [Test]
    public void TheTitleWarnsOfFlashingAndSaysHowToTurnItDown()
    {
        StringAssert.Contains("flicker", Controls.Flashing);
        StringAssert.Contains(Controls.Pause + " then " + Controls.ReducedFlashing, Controls.Flashing);
    }

    [Test]
    public void EveryKeyIsDistinct()
    {
        var keys = new[]
        {
            Controls.Jump, Controls.LookBack, Controls.LookBackAlso, Controls.Leave, Controls.Restart,
            Controls.Pause, Controls.Daily, Controls.ReducedFlashing, Controls.ToTitle, Controls.Mute,
        };
        CollectionAssert.AllItemsAreUnique(keys);
    }
}
