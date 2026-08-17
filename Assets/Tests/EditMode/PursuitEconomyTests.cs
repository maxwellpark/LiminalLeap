using NUnit.Framework;

// The pursuit has to be something the player can act on. Once the mirror stopped holding it
// off, dodging became the only counterplay, so a dodge has to actually pay for itself.
public class PursuitEconomyTests
{
    private const float Step = 1f / 60f;

    private static PursuitModel.Settings Settings(PursuerAttackConfig attack, float start)
    {
        return new PursuitModel.Settings
        {
            StartDistance = start,
            MaxDistance = start,
            CloseRate = attack.CloseRateDuringAttacks,
            RecoverRate = 0f,       // the mirror buys nothing under attacks
            SpeedRelief = attack.SpeedReliefDuringAttacks,
            LungeWithin = 12f,
            LungeMultiplier = 2.2f,
        };
    }

    private static float GroundLostOverACycle(PursuerAttackConfig attack, float speedFraction, float start)
    {
        var settings = Settings(attack, start);
        var distance = start;

        for (var t = 0f; t < attack.CycleTime; t += Step)
        {
            distance = PursuitModel.Step(distance, Step, false, speedFraction, settings);
        }

        return start - distance;
    }

    [Test]
    public void ADodgePaysForTheGroundLostWaitingForIt()
    {
        var attack = new PursuerAttackConfig();

        foreach (var pace in new[] { 0f, 0.3f, 0.6f, 1f })
        {
            var lost = GroundLostOverACycle(attack, pace, 45f);
            Assert.LessOrEqual(lost, attack.PursuerSetbackOnDodge,
                $"at {pace:P0} pace a perfect dodge still loses ground, so there is no counterplay");
        }
    }

    // The other half: it still has to be a threat if you ignore it.
    [Test]
    public void IgnoringItStillGetsYouCaught()
    {
        var attack = new PursuerAttackConfig();
        var settings = Settings(attack, 45f);
        var distance = 45f;

        for (var t = 0f; t < 120f; t += Step)
        {
            distance = PursuitModel.Step(distance, Step, false, 0f, settings);
        }

        Assert.IsTrue(PursuitModel.Caught(distance), "standing still for two minutes should be fatal");
    }

    // Uncatchable at speed would pin Proximity at zero, and Proximity is what drives the
    // dread audio and the vignette, so the pursuer would go quiet for most of a run.
    [Test]
    public void SpeedNeverMakesItHarmless()
    {
        var attack = new PursuerAttackConfig();
        Assert.Greater(GroundLostOverACycle(attack, 1f, 45f), 0f, "flat out should still cost you ground");
    }

    [Test]
    public void RunningFastBuysRoom()
    {
        var attack = new PursuerAttackConfig();

        var slow = GroundLostOverACycle(attack, 0.1f, 45f);
        var fast = GroundLostOverACycle(attack, 1f, 45f);

        Assert.Less(fast, slow, "speed should still be worth something");
    }

    // Belt and braces on the rule itself, in case a variant reintroduces recovery.
    [Test]
    public void WatchingItBuysNothingWhenRecoveryIsOff()
    {
        var attack = new PursuerAttackConfig();
        var settings = Settings(attack, 45f);

        var watched = PursuitModel.Step(45f, 1f, true, 0.5f, settings);
        var ignored = PursuitModel.Step(45f, 1f, false, 0.5f, settings);

        Assert.AreEqual(ignored, watched, 0.0001f, "holding the mirror must not change the distance");
    }
}
