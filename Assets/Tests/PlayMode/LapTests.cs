using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// A stand-in player, so running past an exit is the only call made here.
public class LapTests
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
        Features.Override(Feature.ExitDoors, true);
        Features.Override(Feature.CalmSections, false);

        savedPlace = SaveStore.Data.StoryPlace;
        SaveStore.Data.StoryPlace = 0;
        RunMode.ChooseStory();

        prefabHolder = new GameObject("Prefabs");
        prefabHolder.SetActive(false);

        // Several straights, or the repeat cap runs out of options and falls back on the exit.
        var pieces = new[]
        {
            MakePiece("StraightA"), MakePiece("StraightB"), MakePiece("StraightC"), MakePiece("StraightD"),
            MakeExitPiece("Exit"),
        };

        root = new GameObject("LapRoot");
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

    // One piece a step, noting every piece that comes into view.
    private IEnumerator Run(int steps, List<TrackPiece> seen)
    {
        var known = new HashSet<(TrackPiece, int)>();
        for (var i = 0; i < steps; i++)
        {
            foreach (var piece in generator.ActivePieces)
            {
                if (known.Add((piece, piece.Lap)) && piece.EndsLap)
                {
                    seen.Add(piece);
                }
            }

            player.position += Vector3.forward * 10f;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator TheGeneratorLaysLapsInTheStory()
    {
        yield return null;

        Assert.IsTrue(StoryLaps.Laying, "the office has laps, so the generator should be laying them");
        Assert.AreEqual(Places.Story[0].Calls, StoryLaps.Rule.Required);
    }

    [UnityTest]
    public IEnumerator ThereIsOneExitALap()
    {
        var exits = new List<TrackPiece>();
        yield return Run(1300, exits);

        var length = Places.Story[0].Length;
        Assert.Greater(exits.Count, 1300 / length - 4, "exits are coming less than once a lap");
        Assert.Less(exits.Count, 1300 / length + 2, "exits are coming more than once a lap");

        for (var i = 0; i < exits.Count; i++)
        {
            Assert.AreEqual(i, exits[i].Lap, "a lap had no exit, or two");
        }
    }

    // The first lap is clean, so running past its exit is the wrong call.
    [UnityTest]
    public IEnumerator RunningPastTheFirstExitIsWrong()
    {
        var exits = new List<TrackPiece>();
        var deadline = 200;
        var rule = StoryLaps.Rule;

        while (rule.Lap == 0 && deadline-- > 0)
        {
            yield return Run(1, exits);
        }

        Assert.AreEqual(1, rule.Lap, "never got past the first exit");
        Assert.AreEqual(0, rule.Streak);
        Assert.AreEqual(1, rule.CleanLap, "a wrong call should start again on a clean lap");
    }

    [UnityTest]
    public IEnumerator NothingIsWrongOnTheFirstLap()
    {
        yield return null;

        for (var step = 0; step < 40; step++)
        {
            foreach (var piece in generator.ActivePieces)
            {
                if (piece.Lap == 0)
                {
                    Assert.AreEqual(AnomalyKind.None, piece.Anomaly, "the baseline lap had something wrong");
                }
            }

            player.position += Vector3.forward * 10f;
            yield return null;
        }
    }

    // About 25 laps at even odds, so a run with none at all would be a broken rule, not luck.
    [UnityTest]
    public IEnumerator WrongLapsCarryTheirStretch()
    {
        yield return null;
        var wrongPieces = 0;
        var length = Places.Story[0].Length;

        for (var step = 0; step < 1300; step++)
        {
            foreach (var piece in generator.ActivePieces)
            {
                if (piece.Anomaly == AnomalyKind.None)
                {
                    continue;
                }

                wrongPieces++;
                Assert.AreNotEqual(AnomalyKind.WrongCount, piece.Anomaly, "a count anomaly has no stretch");
                Assert.Greater(piece.Lap, 0, "the baseline lap had something wrong");
                Assert.IsFalse(piece.EndsLap, "the exit itself was part of a stretch");
            }

            player.position += Vector3.forward * 10f;
            yield return null;
        }

        Assert.Greater(wrongPieces, 0, $"no anomaly in {1300 / length} laps");
    }

    [UnityTest]
    public IEnumerator EachLapGetsACount()
    {
        var exits = new List<TrackPiece>();
        yield return Run(200, exits);

        Assert.Greater(root.GetComponentsInChildren<LapSign>(true).Length, 1, "no lap counts were painted");
    }

    [UnityTest]
    public IEnumerator TheDailyHasNoLaps()
    {
        RunMode.ChooseDaily();
        generator.ResetRun();
        yield return null;

        Assert.IsFalse(StoryLaps.Laying, "the daily keeps its banking exits");
    }

    private TrackPiece MakePiece(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(prefabHolder.transform, false);

        var anchor = new GameObject("End").transform;
        anchor.SetParent(go.transform, false);
        anchor.localPosition = new Vector3(0f, 0f, 10f);

        var piece = go.AddComponent<TrackPiece>();
        SetField(piece, "endAnchor", anchor);
        SetField(piece, "containsHazard", false);
        return piece;
    }

    private TrackPiece MakeExitPiece(string name)
    {
        var piece = MakePiece(name);

        var door = new GameObject("Door");
        door.transform.SetParent(piece.transform, false);
        var exit = door.AddComponent<ExitDoor>();

        // No chime, which would reach for the audio manager.
        SetField(exit, "chimeRange", 0f);
        return piece;
    }

    private static void SetField(object target, string name, object value)
    {
        var f = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(f, "no field " + name + " on " + target.GetType().Name);
        f.SetValue(target, value);
    }
}
