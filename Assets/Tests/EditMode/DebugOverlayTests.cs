using NUnit.Framework;

public class DebugOverlayTests
{
    // The overlay binds tuning fields by name through reflection, and a typo or a later
    // rename only warns at runtime behind an F1 press. This turns that into a red test.
    [Test]
    public void EveryKnobResolvesToAFloatField()
    {
        foreach (var spec in DebugOverlay.Specs)
        {
            Assert.IsNotNull(
                DebugOverlay.Resolve(spec),
                $"knob '{spec.Label}' does not resolve: no private float '{spec.Field}' on {spec.Owner.Name}");
        }
    }

    [Test]
    public void KnobRangesAreSane()
    {
        foreach (var spec in DebugOverlay.Specs)
        {
            Assert.Less(spec.Min, spec.Max, $"knob '{spec.Label}' has an inverted range");
        }
    }

    [Test]
    public void KnobLabelsAreUnique()
    {
        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (var spec in DebugOverlay.Specs)
        {
            Assert.IsTrue(seen.Add(spec.Label), $"duplicate knob label '{spec.Label}'");
        }
    }

    [Test]
    public void ThereAreKnobsAtAll()
    {
        Assert.Greater(DebugOverlay.Specs.Length, 0);
    }
}
