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

    // Elapsed time, not frames: batchmode runs uncapped so 60 frames was about 0.12s.
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
    public IEnumerator RestartResetsDistance()
    {
        yield return Seconds(1f);
        Assert.Greater(PlayerTrackMovement.DistanceCovered, 1f);

        fixture.Input.PressRestart();
        yield return UntilResetOr(4f);

        Assert.Less(PlayerTrackMovement.DistanceCovered, 1f, "restart did not reset the run");
    }
}
