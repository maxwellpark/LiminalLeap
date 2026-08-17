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
        RunFixture.IsolateFlags();

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
            MakePickupPiece("Pickup"),
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

        Features.ClearOverrides();
    }

    [UnityTest]
    public IEnumerator SignsGetPaintedWhenTheFlagIsOn()
    {
        Features.Override(Feature.LyingSigns, true);
        generator.ResetRun();
        yield return null;

        Assert.Greater(root.GetComponentsInChildren<TrackSign>(true).Length, 0, "no signage was painted");
    }

    [UnityTest]
    public IEnumerator NoSignsWhenTheFlagIsOff()
    {
        Features.Override(Feature.LyingSigns, false);
        generator.ResetRun();
        yield return null;

        Assert.AreEqual(0, root.GetComponentsInChildren<TrackSign>(true).Length, "a flag that is off should do nothing");
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

    private TrackPiece MakePickupPiece(string name)
    {
        var piece = MakePiece(name, 0f, false);

        var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "SpeedPickup";
        pickup.transform.SetParent(piece.transform, false);
        pickup.transform.localPosition = new Vector3(0f, 0.5f, 5f);
        pickup.GetComponent<Collider>().isTrigger = true;
        pickup.AddComponent<SpeedTriggerable>();

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

    // Pooling reuses objects, so a collected pickup came back still collected and the
    // track quietly ran out of them mid-run.
    [UnityTest]
    public IEnumerator RecycledPickupsComeBack()
    {
        // Past the forced-plain lead-in first, or there is nothing collectable yet.
        for (var step = 0; step < 25; step++)
        {
            player.position += Vector3.forward * 12f;
            yield return null;
        }

        var collected = 0;
        foreach (var trig in root.GetComponentsInChildren<SpeedTriggerable>(true))
        {
            trig.Trigger();
            collected++;
        }

        Assert.Greater(collected, 0, "no pickups spawned, so this proves nothing");

        // Run far enough that every spawned piece has been recycled at least once.
        for (var step = 0; step < 60; step++)
        {
            player.position += Vector3.forward * 12f;
            yield return null;
        }

        var live = 0;
        foreach (var trig in root.GetComponentsInChildren<SpeedTriggerable>(true))
        {
            var renderer = trig.GetComponent<Renderer>();
            if (renderer != null && renderer.enabled)
            {
                live++;
            }
        }

        Debug.Log($"ENDLESS pickups collected={collected} live after recycling={live}");
        Assert.Greater(live, 0, "every pooled pickup stayed collected, so the track ran dry");
    }
}
