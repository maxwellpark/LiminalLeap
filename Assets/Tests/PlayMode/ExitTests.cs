using System.Collections;
using Events;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ExitTests
{
    private RunFixture fixture;
    private RunOutcome? outcome;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags();
        Features.Override(Feature.ExitDoors, true);

        outcome = null;
        GameManager.EventService.Add<OnDeathEvent>(OnEnded);

        fixture = new RunFixture();
        fixture.Build();
        fixture.AddExit(2.8f, 3);
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.EventService.Remove<OnDeathEvent>(OnEnded);
        fixture.Teardown();
        Features.ClearOverrides();
    }

    private void OnEnded(OnDeathEvent evt)
    {
        outcome = evt.Outcome;
    }

    // Runs right until it is level with the exit, so the door is genuinely being touched.
    private IEnumerator RunIntoTheExit(float seconds)
    {
        var guard = 0f;

        while (guard < seconds)
        {
            fixture.Input.Horizontal = 1f;
            fixture.Input.Tick();
            guard += Time.deltaTime;
            yield return null;
        }
    }

    // The bug this exists for: the exit sits in the right hand dodge lane, so touching it
    // must not end the run or dodging a left lane attack would bank it for you.
    [UnityTest]
    public IEnumerator StrafingThroughAnExitDoesNotBankTheRun()
    {
        yield return RunIntoTheExit(5f);

        Assert.IsNull(outcome, "the run ended without the player ever asking to leave");
        Assert.Greater(PlayerTrackMovement.DistanceCovered, 20f, "the player should still be running");
    }

    // Asking every frame rather than timing the press, so the test is about the rule and
    // not about whether it guessed the right moment.
    [UnityTest]
    public IEnumerator PressingInsideTheExitBanksTheRun()
    {
        var guard = 0f;

        while (outcome == null && guard < 8f)
        {
            fixture.Input.Horizontal = 1f;
            fixture.Input.PressBank();
            fixture.Input.Tick();
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(RunOutcome.Banked, outcome, "pressing inside the doorway should leave with the score");
    }

    [UnityTest]
    public IEnumerator PressingAwayFromAnExitDoesNothing()
    {
        var guard = 0f;

        while (guard < 2f)
        {
            fixture.Input.Horizontal = -1f; // hard left, nowhere near the door
            fixture.Input.PressBank();
            fixture.Input.Tick();
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.IsNull(outcome, "leaving should need a door, not just the key");
    }

    [UnityTest]
    public IEnumerator NoExitsExistWhenTheFlagIsOff()
    {
        Features.Override(Feature.ExitDoors, false);

        var zone = fixture.AddExit(2.8f, 5);
        yield return null;

        Assert.IsFalse(zone.activeSelf, "a flag that is off should leave nothing in the world");
    }
}
