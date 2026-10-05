using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RunTests
{
    private RunFixture fixture;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags();
        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
    }

    // Time, not frames: batchmode runs uncapped.
    private IEnumerator Seconds(float seconds)
    {
        var until = Time.time + seconds;
        while (Time.time < until)
        {
            fixture.Input.Tick();
            yield return null;
        }
    }

    private IEnumerator UntilResetOr(float timeoutSeconds)
    {
        var deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (PlayerTrackMovement.DistanceCovered > 1f && Time.realtimeSinceStartup < deadline)
        {
            // The run waits on the summary. RunSummaryTests covers dismissing it.
            if (RunSummary.Instance != null)
            {
                RunSummary.Instance.Dismiss();
            }

            fixture.Input.Tick();
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator PlayerRunsForwardOnItsOwn()
    {
        var start = fixture.Player.transform.position.z;
        yield return Seconds(1f);
        Assert.Greater(fixture.Player.transform.position.z, start + 4f, "player did not advance");
    }

    [UnityTest]
    public IEnumerator DistanceTracksActualTravel()
    {
        yield return Seconds(1f);
        var travelled = fixture.Player.transform.position.z;
        Assert.Greater(travelled, 4f, "nothing moved, so this would compare 0 to 0");
        Assert.AreEqual(travelled, PlayerTrackMovement.DistanceCovered, travelled * 0.25f + 1f);
    }

    // The ramp takes about 17s, so pin the invariants rather than wait for the plateau.
    [UnityTest]
    public IEnumerator SpeedRampsAndNeverPassesTheCap()
    {
        const float cap = 32f;

        yield return Seconds(0.5f);
        var early = PlayerTrackMovement.CurrentSpeed;
        var highest = early;

        var until = Time.time + 6f;
        var previous = early;
        while (Time.time < until)
        {
            var speed = PlayerTrackMovement.CurrentSpeed;
            Assert.GreaterOrEqual(speed, previous - 0.01f, "speed dropped while running clear track");
            highest = Mathf.Max(highest, speed);
            previous = speed;

            fixture.Input.Tick();
            yield return null;
        }

        Assert.Greater(highest, early, "speed never ramped");
        Assert.LessOrEqual(highest, cap + 0.01f, "speed passed the cap");
    }

    [UnityTest]
    public IEnumerator StrafeMovesAndClampsAtTheRail()
    {
        fixture.Input.Horizontal = 1f;
        yield return Seconds(2f);

        var x = fixture.Player.transform.position.x;
        Assert.Greater(x, 0.5f, "strafe did nothing");
        Assert.LessOrEqual(x, RunFixture.TrackHalfWidth + 0.01f, "strafe escaped the track");
    }

    [UnityTest]
    public IEnumerator JumpLeavesTheGroundAndComesBack()
    {
        yield return Seconds(0.2f);
        var grounded = fixture.Player.transform.position.y;

        fixture.Input.PressJump();
        yield return Seconds(0.2f);
        Assert.Greater(fixture.Player.transform.position.y, grounded + 0.3f, "jump did not lift the player");

        yield return Seconds(1.5f);
        Assert.AreEqual(grounded, fixture.Player.transform.position.y, 0.25f, "player never landed");
    }

    [UnityTest]
    public IEnumerator ShortHopIsLowerThanAHeldJump()
    {
        yield return Seconds(0.2f);

        fixture.Input.PressJump();
        yield return Seconds(0.05f);
        fixture.Input.ReleaseJump();

        var shortPeak = 0f;
        var until = Time.time + 1f;
        while (Time.time < until)
        {
            shortPeak = Mathf.Max(shortPeak, fixture.Player.transform.position.y);
            fixture.Input.Tick();
            yield return null;
        }

        yield return Seconds(0.5f);

        fixture.Input.PressJump();
        var heldPeak = 0f;
        until = Time.time + 1f;
        while (Time.time < until)
        {
            heldPeak = Mathf.Max(heldPeak, fixture.Player.transform.position.y);
            fixture.Input.Tick();
            yield return null;
        }

        Assert.Less(shortPeak, heldPeak, "releasing early should give a lower hop");
    }

    [UnityTest]
    public IEnumerator PickupAddsSpeedAndDisappears()
    {
        var pickup = fixture.AddPickup(0f, 3);
        var renderer = pickup.GetComponent<Renderer>();

        yield return Seconds(6f);

        Assert.IsFalse(renderer.enabled, "pickup was never collected");
    }

    [UnityTest]
    public IEnumerator HazardEndsTheRun()
    {
        fixture.AddHazard(0f, 3);

        yield return Seconds(1f);
        Assert.Greater(PlayerTrackMovement.DistanceCovered, 1f, "run never started, reset would pass for free");

        yield return Seconds(5f);
        yield return UntilResetOr(4f);

        Assert.Less(PlayerTrackMovement.DistanceCovered, 1f, "hitting a hazard did not reset the run");
    }

    [UnityTest]
    public IEnumerator TheLightsStayOnWhenTheFlagIsOff()
    {
        Features.Override(Feature.LightAsResource, false);
        MoodLighting.GetInstance().ResetForNewRun();

        yield return Seconds(3f);

        Assert.AreEqual(0f, MoodLighting.GetInstance().Darkness,
            "a flag that is off should leave the corridor alone");
    }

    // The head start should cover the opening.
    [UnityTest]
    public IEnumerator YouStartInsideTheLight()
    {
        Features.Override(Feature.LightAsResource, true);
        MoodLighting.GetInstance().ResetForNewRun();

        yield return Seconds(3f);

        Assert.AreEqual(0f, MoodLighting.GetInstance().Darkness,
            "went dark during the opening stretch");

        Features.Override(Feature.LightAsResource, false);
    }

    [UnityTest]
    public IEnumerator RestartResetsDistance()
    {
        yield return Seconds(1f);
        Assert.Greater(PlayerTrackMovement.DistanceCovered, 1f);

        fixture.Input.RestartHeld = true;
        yield return UntilResetOr(4f);
        fixture.Input.RestartHeld = false;

        Assert.Less(PlayerTrackMovement.DistanceCovered, 1f, "restart did not reset the run");
    }

    [UnityTest]
    public IEnumerator TappingRestartDoesNothing()
    {
        yield return Seconds(1f);

        fixture.Input.PressRestart();
        fixture.Input.RestartHeld = true;
        yield return Seconds(0.1f);
        fixture.Input.RestartHeld = false;

        yield return Seconds(1f);

        Assert.IsTrue(PlayerTrackMovement.Running, "a tap ended the run");
        Assert.Greater(PlayerTrackMovement.DistanceCovered, 10f, "a tap reset the run");
    }

    [UnityTest]
    public IEnumerator RestartingIsNotRecordedAsARun()
    {
        yield return Seconds(1f);
        var runs = SaveStore.Data.Runs;

        // Not UntilResetOr: that dismisses the summary, which is the thing being checked.
        fixture.Input.RestartHeld = true;
        var sawSummary = false;
        var deadline = Time.realtimeSinceStartup + 4f;
        while (PlayerTrackMovement.DistanceCovered > 1f && Time.realtimeSinceStartup < deadline)
        {
            sawSummary |= RunSummary.GetInstance().WaitingForInput;
            fixture.Input.Tick();
            yield return null;
        }

        fixture.Input.RestartHeld = false;

        Assert.IsFalse(sawSummary, "a restart should go straight back in, not stop on the summary");
        Assert.Less(PlayerTrackMovement.DistanceCovered, 1f, "never restarted");
        Assert.AreEqual(runs, SaveStore.Data.Runs, "the restart was recorded as a run");
    }

    [UnityTest]
    public IEnumerator AMovedHazardGoesBackWithTheRun()
    {
        var hazard = fixture.AddHazard(-2f, 20);
        var home = hazard.transform.localPosition;

        yield return Seconds(0.5f);
        hazard.transform.localPosition = home + Vector3.right * 4f;

        fixture.Input.RestartHeld = true;
        yield return UntilResetOr(4f);
        fixture.Input.RestartHeld = false;

        Assert.AreEqual(home, hazard.transform.localPosition, "the hazard kept where it was moved to");
    }

    [UnityTest]
    public IEnumerator PausingHoldsTheRunStill()
    {
        yield return Seconds(0.5f);

        var menu = PauseMenu.GetInstance();
        menu.Pause();
        Assert.IsFalse(PlayerTrackMovement.Running);

        var held = PlayerTrackMovement.DistanceCovered;

        // Frames, not Seconds: Time.time stands still while paused.
        for (var i = 0; i < 30; i++)
        {
            fixture.Input.PressJump();
            fixture.Input.Tick();
            yield return null;
        }

        Assert.AreEqual(held, PlayerTrackMovement.DistanceCovered, "the run moved while paused");

        menu.Resume();
        Assert.IsTrue(PlayerTrackMovement.Running);

        yield return Seconds(0.5f);
        Assert.Greater(PlayerTrackMovement.DistanceCovered, held, "the run never resumed");
    }

    // The tests above passed while the light never moved.
    [UnityTest]
    public IEnumerator RunningPastTheLightGoesDark()
    {
        Features.Override(Feature.LightAsResource, true);

        // No head start and a still front, so any distance is past it.
        var lighting = MoodLighting.GetInstance();
        var field = typeof(MoodLighting).GetField("lightFront",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(field, "no lightFront field on MoodLighting");

        var settings = (LightFront.Settings)field.GetValue(lighting);
        settings.HeadStart = 0f;
        settings.Speed = 0f;
        lighting.ResetForNewRun();

        yield return Seconds(2f);

        Assert.Greater(lighting.Darkness, 0.1f, "ran well past the front and the corridor stayed lit");

        Features.Override(Feature.LightAsResource, false);
    }

    [UnityTest]
    public IEnumerator ARestartDoesNotBecomeTheGhost()
    {
        var original = SaveStore.Data.Ghost;
        var weak = new GhostTrace { Times = new[] { 0f, 1f } };
        SaveStore.Data.Ghost = weak;

        try
        {
            yield return Seconds(1f);
            Assert.Greater(PlayerTrackMovement.DistanceCovered, weak.TotalDistance,
                "needs to have outrun the ghost for this to prove anything");

            fixture.Input.RestartHeld = true;
            yield return UntilResetOr(4f);
            fixture.Input.RestartHeld = false;

            Assert.Less(PlayerTrackMovement.DistanceCovered, 1f, "never restarted");
            Assert.AreSame(weak, SaveStore.Data.Ghost, "the restarted run replaced the ghost");
        }
        finally
        {
            SaveStore.Data.Ghost = original;
        }
    }

    // 0.4s before plus 0.3s after is past the 0.6s hold.
    [UnityTest]
    public IEnumerator RestartProgressDoesNotSurviveAPause()
    {
        yield return Seconds(0.5f);

        fixture.Input.RestartHeld = true;
        yield return Seconds(0.4f);

        var menu = PauseMenu.GetInstance();
        menu.Pause();

        fixture.Input.RestartHeld = false;
        yield return Frames(5);
        fixture.Input.RestartHeld = true;
        yield return Frames(5);

        menu.Resume();
        yield return Seconds(0.3f);
        fixture.Input.RestartHeld = false;

        Assert.IsTrue(PlayerTrackMovement.Running, "the hold from before the pause carried on and restarted the run");
        Assert.Greater(PlayerTrackMovement.DistanceCovered, 5f, "the run was reset");
    }

    // The mirror stays up while paused, so only the Running gate can stop it.
    [UnityTest]
    public IEnumerator TheShifterHoldsStillWhilePaused()
    {
        Features.Override(Feature.ShiftWhenUnobserved, true);
        var hazard = fixture.AddHazard(0f, 12);

        yield return Seconds(0.3f);

        fixture.Input.LookingBack = true;
        var guard = 0f;
        while (!RearView.GetInstance().IsRaised && guard < 2f)
        {
            guard += Time.deltaTime;
            fixture.Input.Tick();
            yield return null;
        }

        Assert.IsTrue(RearView.GetInstance().IsRaised, "the mirror needs to be up for this to prove anything");

        var menu = PauseMenu.GetInstance();
        menu.Pause();
        var held = hazard.transform.localPosition;
        NextShiftAt().SetValue(UnobservedShifter.GetInstance(), 0f);

        yield return Frames(10);

        Assert.AreEqual(held, hazard.transform.localPosition, "a hazard moved while paused");

        menu.Resume();
        fixture.Input.LookingBack = false;
        Features.Override(Feature.ShiftWhenUnobserved, false);
    }

    [UnityTest]
    public IEnumerator AResetDoesNotInheritTheShifterDeadline()
    {
        yield return Seconds(0.2f);

        var shifter = UnobservedShifter.GetInstance();
        NextShiftAt().SetValue(shifter, Time.time + 1000f);
        shifter.ResetForNewRun();

        Assert.LessOrEqual((float)NextShiftAt().GetValue(shifter), Time.time + 1f,
            "the next run waited on a deadline left by the last one");
    }

    // Frames, not Seconds, wherever time is stopped.
    private IEnumerator Frames(int count)
    {
        for (var i = 0; i < count; i++)
        {
            fixture.Input.Tick();
            yield return null;
        }
    }

    private static System.Reflection.FieldInfo NextShiftAt()
    {
        var field = typeof(UnobservedShifter).GetField("nextShiftAt",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(field, "no nextShiftAt on UnobservedShifter");
        return field;
    }
}
