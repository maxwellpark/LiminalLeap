using System.Collections;
using Events;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PursuerAttackPlayTests
{
    private RunFixture fixture;
    private bool hit;
    private bool dodged;

    [SetUp]
    public void SetUp()
    {
        // Set before the fixture builds: Pursuer reads the flags in Init.
        Features.IsolateForTests();
        Features.Override(Feature.PursuerAttacks, true);
        Features.Override(Feature.GhostPursuer, false);

        hit = false;
        dodged = false;

        GameManager.EventService.Add<OnAttackHitEvent>(OnHit);
        GameManager.EventService.Add<OnAttackDodgedEvent>(OnDodged);

        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        GameManager.EventService.Remove<OnAttackHitEvent>(OnHit);
        GameManager.EventService.Remove<OnAttackDodgedEvent>(OnDodged);

        fixture.Teardown();
        Features.ClearOverrides();
    }

    private void OnHit(OnAttackHitEvent evt)
    {
        hit = true;
    }

    private void OnDodged(OnAttackDodgedEvent evt)
    {
        dodged = true;
    }

    private static IEnumerator Settle(int frames = 3)
    {
        for (var i = 0; i < frames; i++)
        {
            yield return null;
        }
    }

    // Strafes with real input rather than poking the field, so the tested path is the real one.
    private IEnumerator MoveTo(float lane)
    {
        var guard = 0f;

        while (Mathf.Abs(PlayerTrackMovement.Lane - lane) > 0.25f && guard < 3f)
        {
            fixture.Input.Horizontal = Mathf.Sign(lane - PlayerTrackMovement.Lane);
            guard += Time.deltaTime;
            yield return null;
        }

        fixture.Input.Horizontal = 0f;
        yield return null;
    }

    private IEnumerator ResolveAttack()
    {
        var guard = 0f;

        while (!hit && !dodged && guard < 6f)
        {
            guard += Time.deltaTime;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator StayingInTheTargetedLaneIsAHit()
    {
        yield return Settle();

        var pursuer = Pursuer.GetInstance();
        yield return MoveTo(0f);

        pursuer.ForceAttack(AttackLane.Centre);
        yield return ResolveAttack();

        Assert.IsTrue(hit, "the beam went through the lane the player was standing in");
        Assert.IsFalse(dodged);
    }

    [UnityTest]
    public IEnumerator MovingOutOfTheLaneSurvivesWithoutEverLookingBack()
    {
        yield return Settle();

        var pursuer = Pursuer.GetInstance();
        fixture.Input.LookingBack = false;

        pursuer.ForceAttack(AttackLane.Left);
        yield return MoveTo(2f);
        yield return ResolveAttack();

        Assert.IsTrue(dodged, "guessing right should work, the mirror is information not permission");
        Assert.IsFalse(hit);
    }

    // The regression the whole redesign rests on. Holding the mirror must not save you.
    [UnityTest]
    public IEnumerator TheMirrorDoesNotGrantImmunity()
    {
        yield return Settle();

        var pursuer = Pursuer.GetInstance();
        fixture.Input.LookingBack = true;

        yield return MoveTo(0f);
        Assert.IsTrue(RearView.GetInstance().IsRaised, "the mirror needs to actually be up for this to prove anything");

        pursuer.ForceAttack(AttackLane.Centre);
        yield return ResolveAttack();

        Assert.IsTrue(hit, "holding the mirror up must not stop the beam");
    }

    [UnityTest]
    public IEnumerator HoldingTheMirrorDoesNotPushThePursuerBack()
    {
        yield return Settle();

        var pursuer = Pursuer.GetInstance();
        pursuer.AttackFrozen = true; // distance only, no attack interfering
        fixture.Input.LookingBack = true;

        yield return Settle(5);
        var start = pursuer.Distance;

        var guard = 0f;
        while (guard < 1.5f)
        {
            guard += Time.deltaTime;
            yield return null;
        }

        Assert.LessOrEqual(pursuer.Distance, start + 0.01f,
            "watching it must not buy distance, or the mirror is a defence again");
    }

    [UnityTest]
    public IEnumerator TheLaneIsOnlyReadableOnceItIsChosen()
    {
        yield return Settle();

        var pursuer = Pursuer.GetInstance();
        pursuer.AttackFrozen = true;

        Assert.IsFalse(pursuer.Attack.TargetVisible, "nothing to read before an attack starts");

        pursuer.ForceAttack(AttackLane.Right);
        Assert.IsTrue(pursuer.Attack.TargetVisible);
        Assert.AreEqual(AttackLane.Right, pursuer.Attack.TargetLane);
    }
}
