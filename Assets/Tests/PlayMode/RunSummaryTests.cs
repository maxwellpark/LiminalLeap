using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class RunSummaryTests
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
        Features.ClearOverrides();
    }

    private IEnumerator Seconds(float seconds)
    {
        var until = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < until)
        {
            fixture.Input.Tick();
            yield return null;
        }
    }

    private IEnumerator UntilSummaryOr(float timeout)
    {
        var deadline = Time.realtimeSinceStartup + timeout;
        while (Time.realtimeSinceStartup < deadline)
        {
            if (RunSummary.GetInstance().WaitingForInput)
            {
                yield break;
            }

            fixture.Input.Tick();
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator DyingShowsTheSummary()
    {
        yield return Seconds(0.6f);

        fixture.Input.PressRestart(); // ends the run
        yield return UntilSummaryOr(3f);

        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput, "no summary after the run ended");
    }

    // The whole point: a runner that restarts itself never gives you the moment where you
    // decide to go again.
    [UnityTest]
    public IEnumerator TheRunDoesNotRestartOnItsOwn()
    {
        yield return Seconds(0.6f);
        var reached = PlayerTrackMovement.DistanceCovered;
        Assert.Greater(reached, 1f, "the player needs to have actually run for this to prove anything");

        fixture.Input.PressRestart();
        yield return UntilSummaryOr(3f);
        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput);

        yield return Seconds(2f);

        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput, "the summary let go without being asked");
        Assert.Less(PlayerTrackMovement.DistanceCovered, reached + 1f, "the run carried on behind the summary");
    }

    [UnityTest]
    public IEnumerator PressingGoesAgain()
    {
        yield return Seconds(0.6f);

        fixture.Input.PressRestart();
        yield return UntilSummaryOr(3f);
        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput);

        // Past the minimum display time, or a held key would skip the summary entirely.
        yield return Seconds(0.7f);

        var deadline = Time.realtimeSinceStartup + 3f;
        while (RunSummary.GetInstance().WaitingForInput && Time.realtimeSinceStartup < deadline)
        {
            fixture.Input.PressJump();
            fixture.Input.Tick();
            yield return null;
        }

        Assert.IsFalse(RunSummary.GetInstance().WaitingForInput, "pressing did not dismiss the summary");

        yield return Seconds(1.5f);
        Assert.Less(PlayerTrackMovement.DistanceCovered, 20f, "the run did not actually reset");
    }

    [UnityTest]
    public IEnumerator AHeldKeyCannotSkipItInstantly()
    {
        yield return Seconds(0.6f);

        fixture.Input.PressRestart();
        yield return UntilSummaryOr(3f);

        // Mashing from the frame it appears should still leave it up briefly.
        fixture.Input.PressJump();
        fixture.Input.Tick();
        yield return null;

        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput, "the summary was skipped before it could be read");
    }
}
