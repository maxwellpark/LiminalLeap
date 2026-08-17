using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class PursuerAttackModelTests
{
    private const int All = PursuerAttackModel.AllLanes;

    private static PursuerAttackConfig Config()
    {
        return new PursuerAttackConfig
        {
            MinAttackInterval = 1f,
            MaxAttackInterval = 1f,   // fixed, so the run up is exactly one second
            WarningDuration = 1f,
            TelegraphDuration = 0.8f,
            LockDuration = 0.5f,
            FireDuration = 0.2f,
            CooldownDuration = 1f,
            LaneSpacing = 2f,
            LaneHalfWidth = 1.1f,
            DodgeTolerance = 0.25f,
        };
    }

    // Ticks sized to each phase, so every transition lands exactly on its boundary.
    private static AttackTick Fire(PursuerAttackModel model, AttackLane lane, float playerLane)
    {
        model.ForceAttack(lane);
        model.Tick(0.8f, playerLane, All);
        model.Tick(0.5f, playerLane, All);
        return model.Tick(0.2f, playerLane, All);
    }

    [Test]
    public void ItStartsIdle()
    {
        Assert.AreEqual(AttackPhase.Idle, new PursuerAttackModel(Config(), 1).Phase);
    }

    [Test]
    public void ItWalksTheWholeSequenceInOrder()
    {
        var model = new PursuerAttackModel(Config(), 1);
        var seen = new List<AttackPhase>();

        model.Tick(1f, 0f, All);
        seen.Add(model.Phase);
        model.Tick(1f, 0f, All);
        seen.Add(model.Phase);
        model.Tick(0.8f, 0f, All);
        seen.Add(model.Phase);
        model.Tick(0.5f, 0f, All);
        seen.Add(model.Phase);
        model.Tick(0.2f, 3f, All);
        seen.Add(model.Phase);
        model.Tick(0.01f, 3f, All);
        seen.Add(model.Phase);
        model.Tick(1f, 3f, All);
        seen.Add(model.Phase);

        CollectionAssert.AreEqual(
            new[]
            {
                AttackPhase.Warning, AttackPhase.Telegraph, AttackPhase.Locked,
                AttackPhase.Fire, AttackPhase.Resolve, AttackPhase.Cooldown, AttackPhase.Idle,
            },
            seen);
    }

    [Test]
    public void ItHoldsIdleUntilTheIntervalElapses()
    {
        var model = new PursuerAttackModel(Config(), 1);
        model.Tick(0.9f, 0f, All);
        Assert.AreEqual(AttackPhase.Idle, model.Phase);
    }

    [Test]
    public void TheWarningComesBeforeTheLaneIsChosen()
    {
        var model = new PursuerAttackModel(Config(), 1);
        var started = model.Tick(1f, 0f, All);

        Assert.IsTrue(started.Started, "the player has to hear it coming");
        Assert.AreEqual(AttackPhase.Warning, model.Phase);
        Assert.IsFalse(model.TargetVisible, "the lane must not be readable during the warning");
    }

    [Test]
    public void TheLaneBecomesReadableAtTelegraph()
    {
        var model = new PursuerAttackModel(Config(), 1);
        model.Tick(1f, 0f, All);
        var ready = model.Tick(1f, 0f, All);

        Assert.IsTrue(ready.TelegraphReady);
        Assert.IsTrue(model.TargetVisible, "this is the whole reason to look back");
    }

    [Test]
    public void TimeUntilFireCountsDownAcrossPhases()
    {
        var config = Config();
        var model = new PursuerAttackModel(config, 1);

        model.Tick(1f, 0f, All);
        var atWarning = model.TimeUntilFire;

        model.Tick(1f, 0f, All);
        var atTelegraph = model.TimeUntilFire;

        model.Tick(0.8f, 0f, All);
        var atLock = model.TimeUntilFire;

        Assert.AreEqual(config.WarningDuration + config.TelegraphDuration + config.LockDuration, atWarning, 0.001f);
        Assert.Greater(atWarning, atTelegraph);
        Assert.Greater(atTelegraph, atLock);
        Assert.AreEqual(config.LockDuration, atLock, 0.001f);
    }

    [Test]
    public void StayingInTheTargetedLaneIsAHit()
    {
        var model = new PursuerAttackModel(Config(), 1);
        var result = Fire(model, AttackLane.Left, -2f);

        Assert.IsTrue(result.Hit);
        Assert.IsFalse(result.Dodged);
    }

    [Test]
    public void MovingOutOfTheTargetedLaneIsADodge()
    {
        var model = new PursuerAttackModel(Config(), 1);
        var result = Fire(model, AttackLane.Left, 2f);

        Assert.IsTrue(result.Dodged);
        Assert.IsFalse(result.Hit);
    }

    [Test]
    public void EveryLaneResolvesAgainstItsOwnCentre()
    {
        foreach (var lane in new[] { AttackLane.Left, AttackLane.Centre, AttackLane.Right })
        {
            var model = new PursuerAttackModel(Config(), 1);
            var centre = model.LaneCentre(lane);
            Assert.IsTrue(Fire(model, lane, centre).Hit, lane + " should have connected");
        }
    }

    [Test]
    public void ToleranceShavesTheEdgeOfTheHitBox()
    {
        var config = Config();
        var edge = config.LaneHalfWidth - config.DodgeTolerance; // 0.85

        var inside = new PursuerAttackModel(config, 1);
        Assert.IsTrue(Fire(inside, AttackLane.Centre, edge - 0.01f).Hit, "just inside should connect");

        var outside = new PursuerAttackModel(config, 1);
        Assert.IsTrue(Fire(outside, AttackLane.Centre, edge + 0.01f).Dodged, "just outside should clear");
    }

    [Test]
    public void ItNeverTargetsALaneThatIsNotAllowed()
    {
        for (var seed = 0; seed < 40; seed++)
        {
            var model = new PursuerAttackModel(Config(), seed);
            model.Tick(1f, 0f, 1 << (int)AttackLane.Right);
            model.Tick(1f, 0f, 1 << (int)AttackLane.Right);

            Assert.AreEqual(AttackPhase.Telegraph, model.Phase, "seed " + seed);
            Assert.AreEqual(AttackLane.Right, model.TargetLane, "seed " + seed);
        }
    }

    [Test]
    public void ZeroWeightLanesAreNeverPicked()
    {
        var config = Config();
        config.LaneWeights = new[] { 0f, 1f, 0f };

        for (var seed = 0; seed < 40; seed++)
        {
            var model = new PursuerAttackModel(config, seed);
            model.Tick(1f, 0f, All);
            model.Tick(1f, 0f, All);
            Assert.AreEqual(AttackLane.Centre, model.TargetLane, "seed " + seed);
        }
    }

    [Test]
    public void EqualWeightsReachEveryLaneEventually()
    {
        var seen = new HashSet<AttackLane>();

        for (var seed = 0; seed < 60; seed++)
        {
            var model = new PursuerAttackModel(Config(), seed);
            model.Tick(1f, 0f, All);
            model.Tick(1f, 0f, All);
            seen.Add(model.TargetLane);
        }

        Assert.AreEqual(3, seen.Count, "a lane that never comes up is a lane the player stops watching");
    }

    // The fairness rule: no fair lane means wait, it does not mean fire anyway.
    [Test]
    public void ItWaitsWhileNoLaneIsFair()
    {
        var model = new PursuerAttackModel(Config(), 1);

        for (var i = 0; i < 200; i++)
        {
            model.Tick(0.1f, 0f, 0);
        }

        Assert.AreEqual(AttackPhase.Idle, model.Phase);
    }

    [Test]
    public void ItFiresAsSoonAsALaneFreesUp()
    {
        var model = new PursuerAttackModel(Config(), 1);
        model.Tick(5f, 0f, 0);
        Assert.AreEqual(AttackPhase.Idle, model.Phase);

        model.Tick(0.016f, 0f, All);
        Assert.AreEqual(AttackPhase.Warning, model.Phase, "the wait was already served");
    }

    [Test]
    public void ItAbortsRatherThanFiringIntoTheOnlyOpenLane()
    {
        var model = new PursuerAttackModel(Config(), 1);
        model.Tick(1f, 0f, All);

        var aborted = model.Tick(1f, 0f, 0);

        Assert.IsTrue(aborted.Aborted);
        Assert.AreEqual(AttackPhase.Cooldown, model.Phase);
        Assert.IsFalse(model.TargetVisible);
    }

    [Test]
    public void AbortedAttacksNeverResolve()
    {
        var model = new PursuerAttackModel(Config(), 1);
        model.Tick(1f, 0f, All);
        model.Tick(1f, 0f, 0);

        for (var i = 0; i < 50; i++)
        {
            var tick = model.Tick(0.05f, 0f, 0);
            Assert.IsFalse(tick.Fired, "an aborted attack must not go off");
            Assert.IsFalse(tick.Hit);
        }
    }

    [Test]
    public void TheSameSeedGivesTheSameAttack()
    {
        var a = new PursuerAttackModel(Config(), 99);
        var b = new PursuerAttackModel(Config(), 99);

        a.Tick(1f, 0f, All);
        a.Tick(1f, 0f, All);
        b.Tick(1f, 0f, All);
        b.Tick(1f, 0f, All);

        Assert.AreEqual(a.TargetLane, b.TargetLane);
    }

    [Test]
    public void ResetPutsItBackToIdle()
    {
        var model = new PursuerAttackModel(Config(), 1);
        model.Tick(1f, 0f, All);
        model.Tick(1f, 0f, All);
        model.Reset();

        Assert.AreEqual(AttackPhase.Idle, model.Phase);
        Assert.IsFalse(model.TargetVisible);
    }

    [Test]
    public void ZeroDeltaChangesNothing()
    {
        var model = new PursuerAttackModel(Config(), 1);
        var before = model.Phase;
        model.Tick(0f, 0f, All);
        Assert.AreEqual(before, model.Phase);
    }

    // The structural half of mirror independence. The behavioural half is in PlayMode.
    [Test]
    public void TheModelIsNotAllowedToKnowAboutTheMirror()
    {
        var tick = typeof(PursuerAttackModel).GetMethod(nameof(PursuerAttackModel.Tick));
        var types = tick.GetParameters().Select(p => p.ParameterType).ToArray();

        CollectionAssert.AreEqual(
            new[] { typeof(float), typeof(float), typeof(int) },
            types,
            "the attack must not take mirror state; the mirror is information, not defence");
    }
}
