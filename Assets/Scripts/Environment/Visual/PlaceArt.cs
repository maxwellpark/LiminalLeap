using System.Collections.Generic;
using UnityEngine;

// Generated art from Resources, per docs/ART.md; null where there's no file.
public static class PlaceArt
{
    private static readonly Dictionary<string, Material> Materials = new();
    private static readonly Dictionary<string, GameObject[]> PropSets = new();

    // Every texture covers 2 m and repeats: the floor slab is 8 x 10 m, a wall 10 x 9.5 m and
    // the ceiling 17.2 x 10 m (TrackPiece.Dress).
    private static readonly Vector2 FloorTiling = new(4f, 5f);
    private static readonly Vector2 WallTiling = new(5f, 4.75f);
    private static readonly Vector2 CeilingTiling = new(8.6f, 5f);

    // Asked for every piece laid, so by index rather than a path string that would be garbage.
    private static readonly Material[] Floors = new Material[4];
    private static readonly bool[] FloorsLooked = new bool[4];

    public static Material ForFloor(Floor floor)
    {
        var i = (int)floor;
        if (i < 0 || i >= Floors.Length)
        {
            return null;
        }

        if (!FloorsLooked[i] || (!ReferenceEquals(Floors[i], null) && Floors[i] == null))
        {
            FloorsLooked[i] = true;
            Floors[i] = Load("Art/Floors/" + floor, FloorTiling);
        }

        return Floors[i];
    }

    public static Material Wall(Place place)
    {
        return place != null ? Load("Art/Places/" + place.Id + "/Wall", WallTiling) : null;
    }

    public static Material Ceiling(Place place)
    {
        return place != null ? Load("Art/Places/" + place.Id + "/Ceiling", CeilingTiling) : null;
    }

    public static GameObject[] Props(Place place)
    {
        if (place == null)
        {
            return System.Array.Empty<GameObject>();
        }

        var path = "Art/Places/" + place.Id + "/Props";
        if (!PropSets.TryGetValue(path, out var set))
        {
            set = Resources.LoadAll<GameObject>(path);
            PropSets[path] = set;
        }

        return set;
    }

    // A missing file is remembered too, so it costs one lookup a run rather than one a piece.
    private static Material Load(string path, Vector2 tiling)
    {
        // ReferenceEquals for "no file"; Unity's == null would also be true for a destroyed one.
        if (Materials.TryGetValue(path, out var cached) && (ReferenceEquals(cached, null) || cached != null))
        {
            return cached;
        }

        var texture = Resources.Load<Texture2D>(path);
        var material = texture != null ? MaterialLibrary.FromTexture(path.Replace('/', '_'), texture, tiling) : null;
        Materials[path] = material;
        return material;
    }
}
