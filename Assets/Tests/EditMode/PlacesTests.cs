using System;
using NUnit.Framework;

public class PlacesTests
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
        Features.ClearPlace();
    }

    private static Place Get(string id)
    {
        return Array.Find(Places.Story, p => p.Id == id);
    }

    [Test]
    public void TheStoryRunsOfficeSchoolHouse()
    {
        Assert.AreEqual(new[] { "office", "school", "house" }, Array.ConvertAll(Places.Story, p => p.Id));
    }

    [Test]
    public void EachPlaceHasALengthAndWords()
    {
        foreach (var place in Places.Story)
        {
            Assert.Greater(place.Length, 0, place.Id);
            Assert.IsNotEmpty(place.Name, place.Id);
            Assert.IsNotEmpty(place.Short, place.Id);
            Assert.IsNotEmpty(place.Arrival, place.Id);
            Assert.IsNotNull(place.Rules, place.Id);
        }
    }

    // As the design doc assigns them.
    [Test]
    public void EachPlaceBringsItsOwnRule()
    {
        Places.Apply(Get("office"));
        Assert.IsFalse(Features.On(Feature.LightAsResource));
        Assert.IsFalse(Features.On(Feature.ShiftWhenUnobserved));
        Assert.IsFalse(Features.On(Feature.GhostPursuer));

        Places.Apply(Get("school"));
        Assert.IsTrue(Features.On(Feature.LightAsResource));
        Assert.IsFalse(Features.On(Feature.ShiftWhenUnobserved));
        Assert.IsFalse(Features.On(Feature.GhostPursuer));

        Places.Apply(Get("house"));
        Assert.IsFalse(Features.On(Feature.LightAsResource));
        Assert.IsTrue(Features.On(Feature.ShiftWhenUnobserved));
        Assert.IsTrue(Features.On(Feature.GhostPursuer));
    }

    [Test]
    public void APlaceOnlyDecidesItsScopedRules()
    {
        Places.Apply(Get("house"));

        foreach (var feature in Features.All)
        {
            if (Array.IndexOf(Places.Scoped, feature) < 0)
            {
                Assert.AreEqual(Features.DefaultFor(feature), Features.On(feature), feature.ToString());
            }
        }
    }

    [Test]
    public void NoPlaceFallsBackToTheFlags()
    {
        Places.Apply(Get("house"));
        Places.Apply(null);

        foreach (var feature in Places.Scoped)
        {
            Assert.AreEqual(Features.DefaultFor(feature), Features.On(feature), feature.ToString());
        }
    }

    [Test]
    public void AdvancingWalksThePlacesThenFinishes()
    {
        var save = SaveData.Fresh();

        Assert.IsFalse(Places.Advance(save));
        Assert.AreEqual(1, save.StoryPlace);

        Assert.IsFalse(Places.Advance(save));
        Assert.AreEqual(2, save.StoryPlace);

        Assert.IsTrue(Places.Advance(save), "out of the house should end the story");
        Assert.AreEqual(0, save.StoryPlace, "a finished story starts over");
        Assert.AreEqual(1, save.StoryFinishes);
    }

    [Test]
    public void TheLastPlaceHasNoNext()
    {
        Assert.AreSame(Get("school"), Places.Next(Get("office")));
        Assert.AreSame(Get("house"), Places.Next(Get("school")));
        Assert.IsNull(Places.Next(Get("house")));
    }

    [Test]
    public void TheDailyPlaceIsFixedByTheSeed()
    {
        foreach (var seed in new[] { 0, 1, 2, 7, int.MaxValue, -5 })
        {
            Assert.AreSame(Places.ForDaily(seed), Places.ForDaily(seed));
            Assert.Contains(Places.ForDaily(seed), Places.Story);
        }
    }

    [Test]
    public void PlaceBestsAreKeptPerPlace()
    {
        var save = SaveData.Fresh();

        Assert.IsTrue(save.PlaceBest("office").Record(100f));
        Assert.IsFalse(save.PlaceBest("office").Record(50f));
        Assert.IsTrue(save.PlaceBest("school").Record(30f));

        Assert.AreEqual(100f, save.PlaceBest("office").BestScore);
        Assert.AreEqual(30f, save.PlaceBest("school").BestScore);
        Assert.AreEqual(2, save.PlaceBest("office").Runs);
    }
}
