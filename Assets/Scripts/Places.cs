using System;
using UnityEngine;

// A stretch of the story with its own look and rule, and an exit at the end.
public sealed class Place
{
    public string Id;           // save key, never shown
    public string Name;
    public string Short;        // "the office", for the summary
    public string Arrival;
    public int Length;          // pieces to the exit
    public Color Fog;
    public float FogDensity;
    public Color Ambient;
    public Color Light;
    public Feature[] Rules;
}

public static class Places
{
    public static readonly Place[] Story =
    {
        new()
        {
            Id = "office", Name = "Office after hours", Short = "the office",
            Arrival = "Everyone went home hours ago.", Length = 120,
            Fog = new Color(0.07f, 0.08f, 0.09f), FogDensity = 0.018f,
            Ambient = new Color(0.16f, 0.17f, 0.2f), Light = new Color(0.82f, 0.86f, 0.8f),
            Rules = new[] { Feature.LyingSigns },
        },
        new()
        {
            Id = "school", Name = "School at night", Short = "the school",
            Arrival = "The lights only come on as you reach them.", Length = 150,
            Fog = new Color(0.04f, 0.05f, 0.08f), FogDensity = 0.022f,
            Ambient = new Color(0.1f, 0.11f, 0.17f), Light = new Color(0.7f, 0.78f, 0.95f),
            Rules = new[] { Feature.LightAsResource },
        },
        new()
        {
            Id = "house", Name = "A house that's almost yours", Short = "the house",
            Arrival = "The hallway is longer than you remember.", Length = 180,
            Fog = new Color(0.09f, 0.06f, 0.05f), FogDensity = 0.02f,
            Ambient = new Color(0.18f, 0.13f, 0.11f), Light = new Color(0.95f, 0.78f, 0.6f),
            Rules = new[] { Feature.LyingSigns, Feature.ShiftWhenUnobserved, Feature.GhostPursuer },
        },
    };

    // Decided by the place. Outside one, as in a free run, they fall back to the flags.
    public static readonly Feature[] Scoped =
    {
        Feature.LyingSigns,
        Feature.LightAsResource,
        Feature.ShiftWhenUnobserved,
        Feature.GhostPursuer,
    };

    // The story's current place, the daily's by date, or none for a free run.
    public static Place ForRun()
    {
        if (RunMode.Story)
        {
            return Story[Mathf.Clamp(SaveStore.Data.StoryPlace, 0, Story.Length - 1)];
        }

        return RunMode.Daily ? ForDaily(RunMode.Seed) : null;
    }

    public static Place ForDaily(int seed)
    {
        return Story[(int)((uint)seed % (uint)Story.Length)];
    }

    public static Place Next(Place place)
    {
        var i = Array.IndexOf(Story, place);
        return i >= 0 && i + 1 < Story.Length ? Story[i + 1] : null;
    }

    // True when that was the last place, which ends the story and starts it over.
    public static bool Advance(SaveData data)
    {
        data.StoryPlace++;
        if (data.StoryPlace < Story.Length)
        {
            return false;
        }

        data.StoryPlace = 0;
        data.StoryFinishes++;
        return true;
    }

    public static void Apply(Place place)
    {
        Features.ClearPlace();
        if (place == null)
        {
            return;
        }

        foreach (var feature in Scoped)
        {
            Features.SetPlaceRule(feature, Array.IndexOf(place.Rules, feature) >= 0);
        }
    }
}
