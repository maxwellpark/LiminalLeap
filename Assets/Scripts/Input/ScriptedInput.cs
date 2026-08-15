// Presses are one-frame edges like the real thing, so callers must Tick each frame.
public class ScriptedInput : IInputSource
{
    private bool jumpQueued;
    private bool releaseQueued;
    private bool restartQueued;

    public float Horizontal { get; set; }
    public bool JumpPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool RestartPressed { get; private set; }

    public void PressJump()
    {
        jumpQueued = true;
    }

    public void ReleaseJump()
    {
        releaseQueued = true;
    }

    public void PressRestart()
    {
        restartQueued = true;
    }

    // Call once per frame, before whatever reads the input.
    public void Tick()
    {
        JumpPressed = jumpQueued;
        JumpReleased = releaseQueued;
        RestartPressed = restartQueued;

        jumpQueued = false;
        releaseQueued = false;
        restartQueued = false;
    }
}
