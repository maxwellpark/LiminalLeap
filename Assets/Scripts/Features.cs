using System;
using UnityEngine;

public enum Feature
{
    LookBackCost,
    ExitDoors,
    GhostPursuer,
    ShiftWhenUnobserved,
    LyingSigns,
    SpeedSummons,
    PursuerAttacks,
    CalmSections,
}

// Experiment toggles. Default is the shipped variant, prefs hold a manual override, and
// test overrides stay in memory so running the suite can't rewrite the real save.
//
// Resolved values are cached because this is read several times a frame from Update: the
// naive version built a prefs key string every call and allocated in the hot path.
public static class Features
{
    private const string Prefix = "liminalleap.feature.";
    private const sbyte Unset = -1;

    public static readonly Feature[] All = (Feature[])Enum.GetValues(typeof(Feature));

    private static readonly sbyte[] Overrides = Filled(All.Length);
    private static readonly sbyte[] Cache = Filled(All.Length);
    private static readonly string[] Keys = BuildKeys();

    private static bool useStorage = true;
    private static bool isolated;

    // Throws rather than falling through, so adding a feature without deciding fails loudly.
    public static bool DefaultFor(Feature feature)
    {
        return feature switch
        {
            Feature.LookBackCost => true,
            Feature.ExitDoors => true,
            Feature.LyingSigns => true,
            Feature.PursuerAttacks => true,
            Feature.CalmSections => true,
            Feature.GhostPursuer => false,
            Feature.ShiftWhenUnobserved => false,
            Feature.SpeedSummons => false,
            _ => throw new ArgumentOutOfRangeException(nameof(feature), feature, "no default set"),
        };
    }

    public static bool On(Feature feature)
    {
        var i = (int)feature;

        if (Overrides[i] != Unset)
        {
            return Overrides[i] == 1;
        }

        if (Cache[i] == Unset)
        {
            Cache[i] = (sbyte)(Resolve(feature) ? 1 : 0);
        }

        return Cache[i] == 1;
    }

    public static void Set(Feature feature, bool on)
    {
        Overrides[(int)feature] = Unset;
        PlayerPrefs.SetInt(Keys[(int)feature], on ? 1 : 0);
        PlayerPrefs.Save();
        Invalidate();
    }

    public static void ClearStored(Feature feature)
    {
        Overrides[(int)feature] = Unset;
        PlayerPrefs.DeleteKey(Keys[(int)feature]);
        PlayerPrefs.Save();
        Invalidate();
    }

    // In memory only, so nothing leaks into the player's prefs by accident.
    public static void Override(Feature feature, bool on)
    {
        Overrides[(int)feature] = (sbyte)(on ? 1 : 0);
    }

    public static void ClearOverrides()
    {
        for (var i = 0; i < Overrides.Length; i++)
        {
            Overrides[i] = Unset;
        }
    }

    // Tests run against defaults, not whatever was last toggled in the editor.
    public static void IsolateForTests()
    {
        ClearOverrides();
        isolated = true;
        useStorage = false;
        Invalidate();
    }

    // A latch, because PlayMode tests spawn a GameManager and its Awake calls this. Without
    // it, half the flags would quietly start reading the real prefs mid test.
    public static void UseStorage()
    {
        if (isolated)
        {
            return;
        }

        useStorage = true;
        Invalidate();
    }

    public static void EndIsolation()
    {
        isolated = false;
        useStorage = true;
        Invalidate();
    }

    // One char per feature in enum order, so a run's stats can be bucketed by variant.
    public static string VariantKey()
    {
        var chars = new char[All.Length];
        for (var i = 0; i < All.Length; i++)
        {
            chars[i] = On(All[i]) ? '1' : '0';
        }

        return new string(chars);
    }

    private static bool Resolve(Feature feature)
    {
        if (!useStorage)
        {
            return DefaultFor(feature);
        }

        var stored = PlayerPrefs.GetInt(Keys[(int)feature], -1);
        return stored < 0 ? DefaultFor(feature) : stored == 1;
    }

    private static void Invalidate()
    {
        for (var i = 0; i < Cache.Length; i++)
        {
            Cache[i] = Unset;
        }
    }

    private static sbyte[] Filled(int length)
    {
        var array = new sbyte[length];
        for (var i = 0; i < length; i++)
        {
            array[i] = Unset;
        }

        return array;
    }

    private static string[] BuildKeys()
    {
        var keys = new string[All.Length];
        for (var i = 0; i < All.Length; i++)
        {
            keys[i] = Prefix + All[i];
        }

        return keys;
    }
}
