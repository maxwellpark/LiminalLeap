using System;
using System.Collections.Generic;

public enum MapMarkKind
{
    Floor,        // Value: the Floor underfoot
    Landmark,     // Value: which landmark stands there
    Anomaly,      // Value: the AnomalyKind seen there
    Fork,         // Value: unused, 0
    LoopingDoor,  // Value: LoopRule.LeftDoor or RightDoor, taken and found to loop
    OnwardDoor,   // Value: likewise, taken and found to carry on
}

// One thing uncovered, at a piece's place along a lap of the corridor.
[Serializable]
public struct MapMark
{
    public int At;
    public MapMarkKind Kind;
    public int Value;

    public MapMark(int at, MapMarkKind kind, int value)
    {
        At = at;
        Kind = kind;
        Value = value;
    }
}

// A flat list, as JsonUtility can't save a dictionary.
[Serializable]
public class PlaceMap
{
    public string Id;
    public List<MapMark> Marks = new();

    // True the first time, so a run can tell what it uncovered.
    public bool Mark(int at, MapMarkKind kind, int value = 0)
    {
        if (at < 0 || Has(at, kind, value))
        {
            return false;
        }

        Marks.Add(new MapMark(at, kind, value));
        return true;
    }

    public bool Has(int at, MapMarkKind kind, int value = 0)
    {
        for (var i = 0; i < Marks.Count; i++)
        {
            var m = Marks[i];
            if (m.At == at && m.Kind == kind && m.Value == value)
            {
                return true;
            }
        }

        return false;
    }

    public int Count(MapMarkKind kind)
    {
        var n = 0;
        for (var i = 0; i < Marks.Count; i++)
        {
            if (Marks[i].Kind == kind)
            {
                n++;
            }
        }

        return n;
    }

    // Either mark settles it, since a fixed place's door always leads the same way.
    public bool KnowsWhereDoorLeads(int at, int door)
    {
        return Has(at, MapMarkKind.LoopingDoor, door) || Has(at, MapMarkKind.OnwardDoor, door);
    }
}
