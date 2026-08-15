using NUnit.Framework;

public class DebugOverlayTests
{
    // Binding is by name, so a rename only warns behind an F1 press. This makes it red.
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
