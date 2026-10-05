using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PursuerTests
{
    private RunFixture fixture;

    // Covers the variant with attacks off, where watching it is still what holds it off.
    // The attacks variant deliberately breaks that rule and is covered separately.
    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags(attacks: false);
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
        var until = Time.time + seconds;
        while (Time.time < until)
        {
            fixture.Input.Tick();
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator ItClosesWhileYouIgnoreIt()
    {
        yield return Seconds(0.5f);
        var start = Pursuer.GetInstance().Distance;

        fixture.Input.LookingBack = false;
        yield return Seconds(2f);

        Assert.Less(Pursuer.GetInstance().Distance, start, "pursuer never closed");
    }

    // True only with attacks off. Under PursuerAttacks the mirror is information and buys
    // nothing, which PursuerAttackPlayTests asserts instead.
    [UnityTest]
    public IEnumerator LookingBackHoldsItOffWithoutAttacks()
    {
        yield return Seconds(0.5f);

        fixture.Input.LookingBack = false;
        yield return Seconds(2f);
        var afterIgnoring = Pursuer.GetInstance().Distance;

        fixture.Input.LookingBack = true;
        yield return Seconds(2f);
        var afterWatching = Pursuer.GetInstance().Distance;

        Assert.Greater(afterWatching, afterIgnoring, "watching did not hold it off");
    }

    // CopyFrom copies the transform, so setting the 180 before it silently showed the
    // forward view in the mirror.
    [UnityTest]
    public IEnumerator MirrorActuallyLooksBackwards()
    {
        yield return Seconds(0.3f);

        var mirror = RearView.GetInstance().MirrorCamera;
        Assert.IsNotNull(mirror, "no mirror camera");

        var main = Camera.main;
        Assert.IsNotNull(main, "no main camera");

        var dot = Vector3.Dot(mirror.transform.forward, main.transform.forward);
        Assert.Less(dot, -0.9f, $"mirror is not facing backwards, dot={dot:F2}");
    }

    [UnityTest]
    public IEnumerator DeathPutsItBack()
    {
        yield return Seconds(0.5f);
        var start = Pursuer.GetInstance().Distance;

        fixture.Input.LookingBack = false;
        yield return Seconds(2f);
        Assert.Less(Pursuer.GetInstance().Distance, start);

        fixture.Input.RestartHeld = true;
        var deadline = Time.realtimeSinceStartup + 4f;
        while (PlayerTrackMovement.DistanceCovered > 1f && Time.realtimeSinceStartup < deadline)
        {
            // The run waits on the summary before resetting; this test is about the pursuer.
            if (RunSummary.Instance != null)
            {
                RunSummary.Instance.Dismiss();
            }

            fixture.Input.Tick();
            yield return null;
        }

        fixture.Input.RestartHeld = false;
        yield return Seconds(0.2f);
        Assert.AreEqual(start, Pursuer.GetInstance().Distance, 2f, "pursuer did not reset with the run");
    }

    [UnityTest]
    public IEnumerator NothingMovesBehindTheSummary()
    {
        yield return Seconds(0.5f);
        Assert.IsTrue(PlayerTrackMovement.Running);

        fixture.Input.LookingBack = false;
        fixture.AddHazardAhead();

        var deadline = Time.realtimeSinceStartup + 4f;
        while (PlayerTrackMovement.Running && Time.realtimeSinceStartup < deadline)
        {
            fixture.Input.Tick();
            yield return null;
        }

        Assert.IsFalse(PlayerTrackMovement.Running, "the run should be over");
        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput, "needs to be sat on the summary");

        var held = Pursuer.GetInstance().Distance;
        fixture.Input.LookingBack = true;
        yield return Seconds(2f);

        Assert.AreEqual(held, Pursuer.GetInstance().Distance, 0.001f, "it kept closing behind the summary");
        Assert.IsFalse(RearView.GetInstance().IsRaised, "the mirror came up over the summary");
    }
}
