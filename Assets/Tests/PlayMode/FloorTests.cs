using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// The generator's side of floors: stretches in a free run, one floor in a place, none with the flag off.
public class FloorTests
{
    private GameObject root;
    private ProceduralTrackGenerator generator;
    private Transform player;
    private GameObject prefabHolder;
    private int savedPlace;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags();
        savedPlace = SaveStore.Data.StoryPlace;
        Features.Override(Feature.Surfaces, true);
        RunMode.ChooseFree();

        prefabHolder = new GameObject("Prefabs");
        prefabHolder.SetActive(false);

        root = new GameObject("FloorRoot");
        root.SetActive(false);
        player = new GameObject("FakePlayer").transform;

        generator = root.AddComponent<ProceduralTrackGenerator>();
        SetField(generator, "piecePrefabs", new[] { MakePiece("StraightA"), MakePiece("StraightB") });
        SetField(generator, "player", player);
        root.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prefabHolder);
        Object.DestroyImmediate(player.gameObject);
        SaveStore.Data.StoryPlace = savedPlace;
        RunMode.ChooseFree();
        StoryLaps.Clear();
        Features.ClearOverrides();
        Features.ClearPlace();
    }

    // Walks the player down the track and returns each piece's floor, in the order laid.
    // Keyed by ordinal, as the pool hands the same piece back for later ones.
    private IEnumerator Run(List<Floor> into, int pieces)
    {
        var byOrdinal = new Dictionary<int, Floor>();
        yield return null;
        for (var step = 0; step < pieces * 2 && byOrdinal.Count < pieces; step++)
        {
            foreach (var piece in generator.ActivePieces)
            {
                byOrdinal.TryAdd(piece.Ordinal, piece.Floor);
            }

            player.position += Vector3.forward * 10f;
            yield return null;
        }

        for (var i = 0; i < pieces; i++)
        {
            Assert.IsTrue(byOrdinal.TryGetValue(i, out var floor), "piece " + i + " was never seen");
            into.Add(floor);
        }
    }

    [UnityTest]
    public IEnumerator AFreeRunChangesFloorInStretches()
    {
        var floors = new List<Floor>();
        yield return Run(floors, 60);

        Assert.AreEqual(Floor.Linoleum, floors[0], "a run opens on linoleum");
        var changes = 0;
        for (var i = 1; i < floors.Count; i++)
        {
            if (floors[i] != floors[i - 1])
            {
                changes++;
            }
        }

        Assert.That(changes, Is.InRange(3, 7), "60 pieces in stretches of 8 to 14");
    }

    [UnityTest]
    public IEnumerator APlaceKeepsItsOwnFloor()
    {
        SaveStore.Data.StoryPlace = 0;
        RunMode.ChooseStory();
        generator.ResetRun();

        var floors = new List<Floor>();
        yield return Run(floors, 40);

        CollectionAssert.AreEqual(new HashSet<Floor> { Places.Story[0].Floor }, new HashSet<Floor>(floors));
    }

    [UnityTest]
    public IEnumerator WithTheFlagOffItIsAllLinoleum()
    {
        Features.Override(Feature.Surfaces, false);
        player.position = Vector3.zero;
        generator.ResetRun();

        var floors = new List<Floor>();
        yield return Run(floors, 40);

        CollectionAssert.AreEqual(new HashSet<Floor> { Floor.Linoleum }, new HashSet<Floor>(floors));
    }

    private TrackPiece MakePiece(string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(prefabHolder.transform, false);
        Object.DestroyImmediate(go.GetComponent<Collider>());

        var anchor = new GameObject("End").transform;
        anchor.SetParent(go.transform, false);
        anchor.localPosition = new Vector3(0f, 0f, 10f);

        var piece = go.AddComponent<TrackPiece>();
        SetField(piece, "endAnchor", anchor);
        SetField(piece, "containsHazard", false);
        return piece;
    }

    private static void SetField(object target, string name, object value)
    {
        var f = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, "no field " + name + " on " + target.GetType().Name);
        f.SetValue(target, value);
    }
}
