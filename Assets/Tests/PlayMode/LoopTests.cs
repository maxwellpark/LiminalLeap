using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Straight pieces only, so a replayed stretch can be read back prefab for prefab.
public class LoopTests
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
        PlayerTrackMovement.Readout.Reset();
        savedPlace = SaveStore.Data.StoryPlace;
        Features.Override(Feature.Loops, true);
        Features.Override(Feature.CalmSections, false);
        RunMode.ChooseFree();

        prefabHolder = new GameObject("Prefabs");
        prefabHolder.SetActive(false);

        var pieces = new[] { MakePiece("StraightA"), MakePiece("StraightB"), MakePiece("StraightC"), MakePiece("StraightD") };

        root = new GameObject("LoopRoot");
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
        Object.DestroyImmediate(player.gameObject);
        SaveStore.Data.StoryPlace = savedPlace;
        RunMode.ChooseFree();
        StoryLaps.Clear();
        Features.ClearOverrides();
        Features.ClearPlace();
    }

    // Down the middle, so no door is picked before the fork is laid.
    private IEnumerator UntilFork(int maxSteps = 80)
    {
        yield return null;
        for (var i = 0; i < maxSteps && !generator.HoldingAtFork; i++)
        {
            player.position += Vector3.forward * 10f;
            yield return null;
        }

        Assert.IsTrue(generator.HoldingAtFork, "no fork came, so this proves nothing");
    }

    private TrackPiece Fork()
    {
        var pieces = generator.ActivePieces;
        return pieces[pieces.Count - 1];
    }

    // Two to the side and one past the wall is all the generator needs to read the door.
    private IEnumerator Through(int door)
    {
        var wall = Fork().ForkWall.transform;
        player.position = wall.position + wall.right * (door == LoopRule.LeftDoor ? -2f : 2f) + wall.forward * 1f;
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator TheTrackHoldsAtAForkUntilADoorIsTaken()
    {
        yield return UntilFork();
        var fork = Fork();
        var laid = generator.Laid;

        Assert.IsTrue(fork.ForkWall.activeSelf, "the last piece laid should be the fork");
        yield return null;
        yield return null;
        Assert.AreEqual(laid, generator.Laid, "track was laid past a fork nobody had chosen at");

        yield return Through(LoopRule.RightDoor);
        Assert.IsFalse(generator.HoldingAtFork);
        Assert.AreEqual(1, generator.ForksTaken);
        Assert.Greater(generator.Laid, laid, "taking a door should lay the track beyond it");
    }

    [UnityTest]
    public IEnumerator TheLoopingDoorLaysTheTrackAlreadyRun()
    {
        yield return UntilFork();

        var remembered = new TrackMemory.Piece[10];
        var count = generator.Memory.CopyLast(remembered.Length, remembered);
        Assert.AreEqual(remembered.Length, count, "not enough track behind the fork to loop");

        var laidBefore = generator.Laid;
        yield return Through(generator.LoopDoorAhead);
        Assert.AreEqual(1, generator.LoopsTaken);

        // New pieces are read off the end, since pieces from before the fork are still listed.
        var after = new List<int>();
        for (var step = 0; step < 40 && after.Count < count; step++)
        {
            var pieces = generator.ActivePieces;
            var fresh = generator.Laid - laidBefore - after.Count;
            for (var i = pieces.Count - fresh; i < pieces.Count; i++)
            {
                after.Add(pieces[i].PrefabIndex);
            }

            player.position += Vector3.forward * 10f;
            yield return null;
        }

        var expected = new List<int>();
        for (var i = 0; i < count; i++)
        {
            expected.Add(remembered[i].Prefab);
        }

        CollectionAssert.AreEqual(expected, after.GetRange(0, count), "the looping door should lay what came before the fork");
    }

    // Ten remembered pieces of 10 m plus the fork: past it, the readout is back where the stretch began.
    [UnityTest]
    public IEnumerator TheLoopingDoorRewindsTheReadout()
    {
        yield return UntilFork();
        yield return Through(generator.LoopDoorAhead);

        PlayerTrackMovement.Readout.Advance(1000f);
        Assert.AreEqual(890f, PlayerTrackMovement.Readout.Shown(1000f), 0.5f);
    }

    [UnityTest]
    public IEnumerator TheOtherDoorLeavesTheReadoutTrue()
    {
        yield return UntilFork();
        var loopDoor = generator.LoopDoorAhead;
        yield return Through(loopDoor == LoopRule.LeftDoor ? LoopRule.RightDoor : LoopRule.LeftDoor);

        PlayerTrackMovement.Readout.Advance(1000f);
        Assert.AreEqual(1000f, PlayerTrackMovement.Readout.Shown(1000f));
    }

    [UnityTest]
    public IEnumerator TheOtherDoorGoesOn()
    {
        yield return UntilFork();
        var loopDoor = generator.LoopDoorAhead;

        yield return Through(loopDoor == LoopRule.LeftDoor ? LoopRule.RightDoor : LoopRule.LeftDoor);

        Assert.AreEqual(1, generator.ForksTaken);
        Assert.AreEqual(0, generator.LoopsTaken);
    }

    [UnityTest]
    public IEnumerator OnlyTheMiddleOfTheWallKills()
    {
        yield return UntilFork();

        // Destroy lands at the end of the frame the wall was built in.
        yield return null;
        var hazards = Fork().ForkWall.GetComponentsInChildren<Hazard>();

        Assert.AreEqual(1, hazards.Length);
        Assert.AreEqual(0f, hazards[0].transform.localPosition.x, 0.01f);
        foreach (var collider in Fork().ForkWall.GetComponentsInChildren<Collider>())
        {
            Assert.IsNotNull(collider.GetComponent<Hazard>(), collider.name + " can be hit but isn't the hazard");
        }
    }

    [UnityTest]
    public IEnumerator NoForksInTheStory()
    {
        SaveStore.Data.StoryPlace = 0;
        RunMode.ChooseStory();
        generator.ResetRun();
        yield return null;

        for (var i = 0; i < 80; i++)
        {
            player.position += Vector3.forward * 10f;
            yield return null;
            Assert.IsFalse(generator.HoldingAtFork, "a fork turned up in a story place");
        }
    }

    [UnityTest]
    public IEnumerator NoForksWithTheFlagOff()
    {
        Features.Override(Feature.Loops, false);
        player.position = Vector3.zero;
        generator.ResetRun();
        yield return null;

        for (var i = 0; i < 80; i++)
        {
            player.position += Vector3.forward * 10f;
            yield return null;
            Assert.IsFalse(generator.HoldingAtFork, "a flag that is off should do nothing");
        }
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
