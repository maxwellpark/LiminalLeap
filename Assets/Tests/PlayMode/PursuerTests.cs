using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PursuerTests
{
    private RunFixture fixture;

    [SetUp]
    public void SetUp()
    {
        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
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

    // The whole mechanic: looking back has to actually hold it off, not just render a mirror.
    [UnityTest]
    public IEnumerator LookingBackHoldsItOff()
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

    [UnityTest]
    public IEnumerator DeathPutsItBack()
    {
        yield return Seconds(0.5f);
        var start = Pursuer.GetInstance().Distance;

        fixture.Input.LookingBack = false;
        yield return Seconds(2f);
        Assert.Less(Pursuer.GetInstance().Distance, start);

        fixture.Input.PressRestart();
        var deadline = Time.realtimeSinceStartup + 4f;
        while (PlayerTrackMovement.DistanceCovered > 1f && Time.realtimeSinceStartup < deadline)
        {
            fixture.Input.Tick();
            yield return null;
        }

        yield return Seconds(0.2f);
        Assert.AreEqual(start, Pursuer.GetInstance().Distance, 2f, "pursuer did not reset with the run");
    }
}
