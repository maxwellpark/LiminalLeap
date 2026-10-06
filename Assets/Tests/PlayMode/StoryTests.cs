using System.Collections;
using Events;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class StoryTests
{
    private RunFixture fixture;
    private RunOutcome? outcome;
    private int savedPlace;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags();
        Features.Override(Feature.ExitDoors, true);

        savedPlace = SaveStore.Data.StoryPlace;
        SaveStore.Data.StoryPlace = 0;
        RunMode.ChooseStory();

        outcome = null;
        GameManager.EventService.Add<OnDeathEvent>(OnEnded);

        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.EventService.Remove<OnDeathEvent>(OnEnded);
        fixture.Teardown();

        SaveStore.Data.StoryPlace = savedPlace;
        SaveStore.Save();
        RunMode.ChooseFree();
        Features.ClearOverrides();
        Features.ClearPlace();
    }

    private void OnEnded(OnDeathEvent evt)
    {
        outcome = evt.Outcome;
    }

    [UnityTest]
    public IEnumerator ARunTakesOnItsPlace()
    {
        yield return null;
        yield return null;

        var office = Places.Story[0];
        Assert.AreEqual(office.Fog, RenderSettings.fogColor, "the office's fog was not applied");
        Assert.IsFalse(Features.On(Feature.LightAsResource), "the office has no light rule");
    }

    [UnityTest]
    public IEnumerator TakingTheExitMovesOnToTheNextPlace()
    {
        fixture.AddExit(2.8f, 3);

        var guard = 0f;
        while (outcome == null && guard < 8f)
        {
            fixture.Input.Horizontal = 1f;
            fixture.Input.PressBank();
            fixture.Input.Tick();
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(RunOutcome.Banked, outcome, "never got out of the office");
        fixture.Input.Horizontal = 0f;

        var deadline = Time.realtimeSinceStartup + 4f;
        while (PlayerTrackMovement.DistanceCovered > 1f && Time.realtimeSinceStartup < deadline)
        {
            RunSummary.GetInstance().Dismiss();
            fixture.Input.Tick();
            yield return null;
        }

        var school = Places.Story[1];
        Assert.AreEqual(1, SaveStore.Data.StoryPlace, "the story did not move on");
        Assert.IsTrue(Features.On(Feature.LightAsResource), "the school's rule did not apply");
        Assert.AreEqual(school.Fog, RenderSettings.fogColor, "the school's fog did not apply");
    }

    [UnityTest]
    public IEnumerator TheStoryHidesTheArcadeHud()
    {
        var hud = new GameObject("Hud").AddComponent<UIManager>();
        yield return null;
        yield return null;

        Assert.IsFalse(hud.HudVisible, "score and speed should be hidden in the story");
    }

    [UnityTest]
    public IEnumerator DyingInTheStoryGoesStraightBackIn()
    {
        yield return null;
        fixture.AddHazardAhead();

        var sawSummary = false;
        var died = false;
        var deadline = Time.realtimeSinceStartup + 6f;
        while (Time.realtimeSinceStartup < deadline)
        {
            died |= outcome == RunOutcome.Died;
            sawSummary |= RunSummary.GetInstance().WaitingForInput;
            if (died && PlayerTrackMovement.DistanceCovered < 1f)
            {
                break;
            }

            fixture.Input.Tick();
            yield return null;
        }

        Assert.IsTrue(died, "never hit the hazard");
        Assert.IsFalse(sawSummary, "the story showed the score summary");
        Assert.Less(PlayerTrackMovement.DistanceCovered, 1f, "the office did not restart");
        Assert.AreEqual(0, SaveStore.Data.StoryPlace, "dying should not move the story");
    }

    [UnityTest]
    public IEnumerator TheStoryExitDoesNotTalkAboutScore()
    {
        fixture.AddExit(2.8f, 3);

        var guard = 0f;
        var toasts = ToastManager.GetInstance();
        while (!AtTheDoor(toasts.LastMessage) && guard < 8f)
        {
            fixture.Input.Horizontal = 1f;
            fixture.Input.Tick();
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(AtTheDoor(toasts.LastMessage), "never reached the exit");
        StringAssert.Contains("open", toasts.LastMessage);
        StringAssert.DoesNotContain("leave with", toasts.LastMessage);
    }

    // Other toasts, like the mirror hint, can arrive first.
    private static bool AtTheDoor(string message)
    {
        return message != null && (message.Contains("open") || message.Contains("leave"));
    }

    [UnityTest]
    public IEnumerator ArcadeToastsStayQuietInTheStory()
    {
        yield return null;
        var toasts = ToastManager.GetInstance();

        toasts.ShowArcade("Near miss   x1.5");

        Assert.AreNotEqual("Near miss   x1.5", toasts.LastMessage);
    }
}
