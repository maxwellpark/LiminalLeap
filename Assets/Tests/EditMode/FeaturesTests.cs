using System;
using NUnit.Framework;

public class FeaturesTests
{
    [SetUp]
    public void SetUp()
    {
        Features.IsolateForTests();
    }

    [TearDown]
    public void TearDown()
    {
        Features.ClearOverrides();
    }

    // A new feature with no default would otherwise silently read as off everywhere.
    [Test]
    public void EveryFeatureHasADefault()
    {
        foreach (var feature in Features.All)
        {
            Assert.DoesNotThrow(() => Features.DefaultFor(feature), "no default for " + feature);
        }
    }

    [Test]
    public void IsolatedFlagsReadTheirDefaults()
    {
        foreach (var feature in Features.All)
        {
            Assert.AreEqual(Features.DefaultFor(feature), Features.On(feature), feature.ToString());
        }
    }

    [Test]
    public void AnOverrideWins()
    {
        Features.Override(Feature.SpeedSummons, true);
        Assert.IsTrue(Features.On(Feature.SpeedSummons));

        Features.Override(Feature.SpeedSummons, false);
        Assert.IsFalse(Features.On(Feature.SpeedSummons));
    }

    [Test]
    public void ClearingAnOverrideGoesBackToTheDefault()
    {
        Features.Override(Feature.ExitDoors, !Features.DefaultFor(Feature.ExitDoors));
        Features.ClearOverrides();
        Assert.AreEqual(Features.DefaultFor(Feature.ExitDoors), Features.On(Feature.ExitDoors));
    }

    // GameManager.Awake calls UseStorage, and PlayMode tests spawn one. Without the latch
    // half the flags quietly start reading the real prefs part way through a test.
    [Test]
    public void IsolationSurvivesAGameManagerWakingUp()
    {
        Features.UseStorage();

        foreach (var feature in Features.All)
        {
            Assert.AreEqual(Features.DefaultFor(feature), Features.On(feature), feature.ToString());
        }
    }

    [Test]
    public void OverridesSurviveAGameManagerWakingUp()
    {
        Features.Override(Feature.SpeedSummons, true);
        Features.UseStorage();

        Assert.IsTrue(Features.On(Feature.SpeedSummons));
    }

    [Test]
    public void VariantKeyHasOneCharacterPerFeature()
    {
        Assert.AreEqual(Features.All.Length, Features.VariantKey().Length);
    }

    [Test]
    public void VariantKeyTracksTheFlags()
    {
        foreach (var feature in Features.All)
        {
            Features.Override(feature, false);
        }

        var allOff = Features.VariantKey();
        Assert.AreEqual(new string('0', Features.All.Length), allOff);

        Features.Override(Feature.ExitDoors, true);
        Assert.AreNotEqual(allOff, Features.VariantKey(), "flipping a flag must change the bucket");
    }

    [Test]
    public void EveryFeatureGetsItsOwnSlotInTheKey()
    {
        foreach (var feature in Features.All)
        {
            Features.Override(feature, false);
        }

        var seen = new System.Collections.Generic.HashSet<string>();

        foreach (var feature in Features.All)
        {
            Features.Override(feature, true);
            Assert.IsTrue(seen.Add(Features.VariantKey()), "two features share a slot in the key");
            Features.Override(feature, false);
        }
    }
}
