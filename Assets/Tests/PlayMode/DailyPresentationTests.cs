using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// The daily keeps everything the story hides.
public class DailyPresentationTests
{
    private RunFixture fixture;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags();
        RunMode.ChooseDaily();
        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
        RunMode.ChooseFree();
        Features.ClearOverrides();
        Features.ClearPlace();
    }

    [UnityTest]
    public IEnumerator TheDailyShowsTheHud()
    {
        var hud = new GameObject("Hud").AddComponent<UIManager>();
        yield return null;
        yield return null;

        Assert.IsTrue(hud.HudVisible);
    }

    [UnityTest]
    public IEnumerator DyingInTheDailyShowsTheSummary()
    {
        yield return null;
        fixture.AddHazardAhead();

        var deadline = Time.realtimeSinceStartup + 5f;
        while (!RunSummary.GetInstance().WaitingForInput && Time.realtimeSinceStartup < deadline)
        {
            fixture.Input.Tick();
            yield return null;
        }

        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput, "the daily should end on its score");
    }

    [UnityTest]
    public IEnumerator ArcadeToastsShowInTheDaily()
    {
        yield return null;
        var toasts = ToastManager.GetInstance();

        toasts.ShowArcade("Near miss   x1.5");

        Assert.AreEqual("Near miss   x1.5", toasts.LastMessage);
    }
}
