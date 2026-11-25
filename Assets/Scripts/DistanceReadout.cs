// What the HUD admits to: through a looping door it rewinds, so the numbers repeat.
public sealed class DistanceReadout
{
    private float rewindAt = float.PositiveInfinity;
    private float rewindBy;

    public float Behind { get; private set; }   // metres the readout is short of the truth

    // Waits until the player reaches the replay, so the door's own piece still counts on.
    public void Loop(float resumeAt, float replayFrom)
    {
        if (resumeAt > replayFrom)
        {
            rewindAt = resumeAt;
            rewindBy = resumeAt - replayFrom;
        }
    }

    public void Advance(float trueDistance)
    {
        if (trueDistance >= rewindAt)
        {
            Behind += rewindBy;
            rewindAt = float.PositiveInfinity;
        }
    }

    public float Shown(float trueDistance)
    {
        return System.Math.Max(0f, trueDistance - Behind);
    }

    public void Reset()
    {
        Behind = 0f;
        rewindAt = float.PositiveInfinity;
    }
}
