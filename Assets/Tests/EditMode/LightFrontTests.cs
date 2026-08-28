using NUnit.Framework;

public class LightFrontTests
{
    private static LightFront.Settings Config()
    {
        return new LightFront.Settings { Speed = 14f, HeadStart = 60f, FadeRange = 45f, Recovery = 1.6f };
    }

    [Test]
    public void YouStartInsideTheLight()
    {
        var s = Config();
        Assert.AreEqual(0f, LightFront.Darkness(0f, LightFront.Start(s), s));
    }

    [Test]
    public void CruisingUnderTheLightSpeedNeverGoesDark()
    {
        var s = Config();
        var front = LightFront.Start(s);
        var travelled = 0f;

        for (var t = 0f; t < 120f; t += 1f / 60f)
        {
            travelled += 10f / 60f;  // comfortably under the front's 14
            front = LightFront.Advance(front, 1f / 60f, travelled, s);
            Assert.AreEqual(0f, LightFront.Darkness(travelled, front, s), "went dark while cruising");
        }
    }

    // The inversion the whole idea rests on: speed has to cost something.
    [Test]
    public void SprintingOutrunsIt()
    {
        var s = Config();
        var front = LightFront.Start(s);
        var travelled = 0f;

        for (var t = 0f; t < 20f; t += 1f / 60f)
        {
            travelled += 32f / 60f;  // flat out
            front = LightFront.Advance(front, 1f / 60f, travelled, s);
        }

        Assert.AreEqual(1f, LightFront.Darkness(travelled, front, s), 0.001f, "flat out should reach full dark");
    }

    [Test]
    public void SlowingDownLetsItCatchUp()
    {
        var s = Config();
        var front = LightFront.Start(s);
        var travelled = 0f;

        for (var t = 0f; t < 20f; t += 1f / 60f)
        {
            travelled += 32f / 60f;
            front = LightFront.Advance(front, 1f / 60f, travelled, s);
        }

        Assert.Greater(LightFront.Darkness(travelled, front, s), 0.5f, "should be dark before recovering");

        for (var t = 0f; t < 60f; t += 1f / 60f)
        {
            travelled += 4f / 60f;  // crawl
            front = LightFront.Advance(front, 1f / 60f, travelled, s);
        }

        Assert.AreEqual(0f, LightFront.Darkness(travelled, front, s), "slowing down should get you back into the light");
    }

    [Test]
    public void DarknessRampsRatherThanSnapping()
    {
        var s = Config();

        Assert.AreEqual(0f, LightFront.Darkness(100f, 100f, s), 0.001f);
        Assert.AreEqual(0.5f, LightFront.Darkness(100f + s.FadeRange * 0.5f, 100f, s), 0.001f);
        Assert.AreEqual(1f, LightFront.Darkness(100f + s.FadeRange, 100f, s), 0.001f);
    }

    [Test]
    public void DarknessNeverLeavesTheUnitRange()
    {
        var s = Config();

        for (var over = -200f; over < 400f; over += 3f)
        {
            var d = LightFront.Darkness(100f + over, 100f, s);
            Assert.GreaterOrEqual(d, 0f, "over " + over);
            Assert.LessOrEqual(d, 1f, "over " + over);
        }
    }

    [Test]
    public void TheFrontOnlyEverMovesForward()
    {
        var s = Config();
        var front = LightFront.Start(s);

        for (var t = 0f; t < 30f; t += 1f / 60f)
        {
            var next = LightFront.Advance(front, 1f / 60f, 0f, s);
            Assert.GreaterOrEqual(next, front);
            front = next;
        }
    }

    [Test]
    public void ZeroDeltaAndNullSettingsAreSafe()
    {
        var s = Config();

        Assert.AreEqual(50f, LightFront.Advance(50f, 0f, 0f, s));
        Assert.AreEqual(50f, LightFront.Advance(50f, 1f, 0f, null));
        Assert.AreEqual(0f, LightFront.Darkness(10f, 100f, null));
    }
}
