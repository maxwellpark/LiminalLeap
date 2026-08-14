using NUnit.Framework;

public class ScriptedInputTests
{
    [TearDown]
    public void RestoreRouter()
    {
        InputRouter.Reset();
    }

    [Test]
    public void PressIsOnlyTrueForOneTick()
    {
        var input = new ScriptedInput();
        input.PressJump();

        input.Tick();
        Assert.IsTrue(input.JumpPressed, "press should land on the tick after it was queued");

        input.Tick();
        Assert.IsFalse(input.JumpPressed, "press stuck on, a held key would fire every frame");
    }

    [Test]
    public void ReleaseIsOnlyTrueForOneTick()
    {
        var input = new ScriptedInput();
        input.ReleaseJump();
        input.Tick();
        Assert.IsTrue(input.JumpReleased);
        input.Tick();
        Assert.IsFalse(input.JumpReleased);
    }

    [Test]
    public void RestartIsOnlyTrueForOneTick()
    {
        var input = new ScriptedInput();
        input.PressRestart();
        input.Tick();
        Assert.IsTrue(input.RestartPressed);
        input.Tick();
        Assert.IsFalse(input.RestartPressed);
    }

    [Test]
    public void NothingIsPressedBeforeATick()
    {
        var input = new ScriptedInput();
        Assert.IsFalse(input.JumpPressed);
        Assert.IsFalse(input.JumpReleased);
        Assert.IsFalse(input.RestartPressed);
    }

    [Test]
    public void HorizontalHoldsUntilChanged()
    {
        var input = new ScriptedInput { Horizontal = -1f };
        input.Tick();
        Assert.AreEqual(-1f, input.Horizontal);
        input.Tick();
        Assert.AreEqual(-1f, input.Horizontal, "axes are held, not edges");
    }

    [Test]
    public void RouterDefaultsToTheKeyboard()
    {
        InputRouter.Reset();
        Assert.IsInstanceOf<KeyboardInput>(InputRouter.Source);
    }

    [Test]
    public void RouterCanBeSwappedAndRestored()
    {
        var scripted = new ScriptedInput();
        InputRouter.Source = scripted;
        Assert.AreSame(scripted, InputRouter.Source);

        InputRouter.Reset();
        Assert.IsInstanceOf<KeyboardInput>(InputRouter.Source);
    }
}
