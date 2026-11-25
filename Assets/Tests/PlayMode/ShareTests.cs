using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// A daily ends on a line you can paste. Nothing else does.
public class ShareTests
{
    private RunFixture fixture;

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
        RunMode.ChooseFree();
        Features.ClearOverrides();
        Features.ClearPlace();
    }

    private void Build(bool daily)
    {
        RunFixture.IsolateFlags();
        if (daily)
        {
            RunMode.ChooseDaily();
        }

        fixture = new RunFixture();
        fixture.Build();
    }

    private IEnumerator DieAndWaitForSummary()
    {
        yield return null;
        fixture.AddHazardAhead();

        var deadline = Time.realtimeSinceStartup + 5f;
        while (!RunSummary.GetInstance().WaitingForInput && Time.realtimeSinceStartup < deadline)
        {
            fixture.Input.Tick();
            yield return null;
        }

        Assert.IsTrue(RunSummary.GetInstance().WaitingForInput, "no summary after the run ended");
    }

    // Past the summary's minimum, so a press counts.
    private IEnumerator Settle()
    {
        var until = Time.realtimeSinceStartup + 1.2f;
        while (Time.realtimeSinceStartup < until)
        {
            fixture.Input.Tick();
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator ADailyEndsOnTodaysLine()
    {
        Build(daily: true);
        yield return DieAndWaitForSummary();

        var share = RunSummary.GetInstance().Share;
        Assert.IsNotNull(share);
        StringAssert.StartsWith("Liminal Leap daily " + RunMode.Day, share);
        StringAssert.Contains(Places.ForRun().Short, share);
        Assert.IsTrue(ShareClipboard.Armed, "the page needs the line before the key arrives");
    }

    [UnityTest]
    public IEnumerator AFreeRunHasNothingToShare()
    {
        Build(daily: false);
        yield return DieAndWaitForSummary();

        Assert.IsNull(RunSummary.GetInstance().Share);
        Assert.IsFalse(ShareClipboard.Armed);
    }

    [UnityTest]
    public IEnumerator ClickingCopySaysSoAndStaysOnTheSummary()
    {
        Build(daily: true);
        yield return DieAndWaitForSummary();
        yield return Settle();

        var summary = RunSummary.GetInstance();
        Assert.Greater(summary.CopyButtonRect.width, 0f, "the button never laid out, so this proves nothing");

        fixture.Input.Pointer = summary.CopyButtonRect.center;
        fixture.Input.PressPointer();
        fixture.Input.Tick();
        yield return null;

        Assert.IsTrue(summary.Copied);
        Assert.IsTrue(summary.WaitingForInput, "copying shouldn't count as going again");
    }

    [UnityTest]
    public IEnumerator ClickingElsewhereDoesNotCopy()
    {
        Build(daily: true);
        yield return DieAndWaitForSummary();
        yield return Settle();

        var summary = RunSummary.GetInstance();
        fixture.Input.Pointer = summary.CopyButtonRect.center + new Vector2(0f, summary.CopyButtonRect.height * 3f);
        fixture.Input.PressPointer();
        fixture.Input.Tick();
        yield return null;

        Assert.IsFalse(summary.Copied);
    }

    [UnityTest]
    public IEnumerator GoingAgainDisarmsIt()
    {
        Build(daily: true);
        yield return DieAndWaitForSummary();
        yield return Settle();

        fixture.Input.PressJump();
        fixture.Input.Tick();
        yield return null;

        Assert.IsNull(RunSummary.GetInstance().Share);
        Assert.IsFalse(ShareClipboard.Armed, "C would still copy a finished run's line");
    }
}
