using NUnit.Framework;

public class ControlsTests
{
    [Test]
    public void TheTitleNamesEveryKeyARunUses()
    {
        // Each key beside its action, so a key moved to the wrong action fails too.
        foreach (var pair in new[]
        {
            "A D  steer", "SPACE  jump", "SHIFT or Q  look back", "E  leave", "hold R  restart", "P  pause",
        })
        {
            StringAssert.Contains(pair, Controls.Summary, pair);
        }
    }

    [Test]
    public void TheTitleWarnsOfFlashingAndSaysHowToTurnItDown()
    {
        StringAssert.Contains("flicker", Controls.Flashing(false, false));
        StringAssert.Contains(Controls.ReducedFlashing + "  reduced flashing  off", Controls.Flashing(false, false));
        StringAssert.Contains(Controls.ReducedFlashing + "  reduced flashing  on", Controls.Flashing(true, false));
        StringAssert.Contains(Controls.Mute + "  sound  on", Controls.Flashing(false, false));
        StringAssert.Contains(Controls.Mute + "  sound  off", Controls.Flashing(false, true));
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
