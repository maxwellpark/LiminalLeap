using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EndlessTests
{
    private GameObject root;
    private ProceduralTrackGenerator generator;
    private Transform player;
    private GameObject prefabHolder;

    [SetUp]
    public void SetUp()
    {
        // Built at runtime rather than loaded with AssetDatabase: PlayMode tests must not
        // reference the editor, and doing so silently emptied the whole suite.
        prefabHolder = new GameObject("Prefabs");
        prefabHolder.SetActive(false);

        var pieces = new[]
        {
            MakePiece("Straight", 0f, false),
            MakePiece("TurnLeft", -7f, false),
            MakePiece("TurnRight", 7f, false),
            MakePiece("Block", 0f, true),
        };

        root = new GameObject("EndlessRoot");
        root.SetActive(false);

        player = new GameObject("FakePlayer").transform;

        generator = root.AddComponent<ProceduralTrackGenerator>();
        SetField(generator, "piecePrefabs", pieces);
        SetField(generator, "player", player);

        root.SetActive(true);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
        Object.DestroyImmediate(prefabHolder);
        if (player != null)
        {
            Object.DestroyImmediate(player.gameObject);
        }
    }

    private TrackPiece MakePiece(string name, float yaw, bool hazard)
    {
        var go = new GameObject(name);
        go.transform.SetParent(prefabHolder.transform, false);

        var anchor = new GameObject("End").transform;
        anchor.SetParent(go.transform, false);
        anchor.localPosition = new Vector3(0f, 0f, 10f);
        anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);

        var piece = go.AddComponent<TrackPiece>();
        SetField(piece, "endAnchor", anchor);
        SetField(piece, "containsHazard", hazard);
        return piece;
    }

    private static void SetField(object target, string name, object value)
    {
        var f = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, "no field " + name + " on " + target.GetType().Name);
        f.SetValue(target, value);
    }

    // The old finite track was 40 pieces. Walking well past that must still find track.
    [UnityTest]
    public IEnumerator TrackKeepsComingIndefinitely()
    {
        for (var step = 0; step < 120; step++)
        {
            player.position += Vector3.forward * 12f;
            yield return null;
            Assert.Greater(generator.ActivePieces.Count, 0, $"ran out of track after {step} steps");
        }

        Debug.Log($"ENDLESS survived 120 steps with {generator.ActivePieces.Count} pieces live");
    }

    // Pooling is the point: it must not leak a GameObject per piece forever.
    [UnityTest]
    public IEnumerator ItRecyclesInsteadOfGrowing()
    {
        yield return null;
        var early = root.transform.childCount;

        for (var step = 0; step < 80; step++)
        {
            player.position += Vector3.forward * 12f;
            yield return null;
        }

        var late = root.transform.childCount;
        Debug.Log($"ENDLESS children early={early} late={late}");
        Assert.Less(late, early + 30, $"pieces are accumulating: {early} then {late}");
    }
}
