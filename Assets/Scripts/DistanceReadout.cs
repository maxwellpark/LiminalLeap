using System.Collections.Generic;

// What the HUD admits to: on track laid again it reads what it read the first time.
public sealed class DistanceReadout
{
    // Each change waits until the player reaches the piece it was laid for.
    private readonly Queue<(float At, float Behind)> pending = new();

    public float Behind { get; private set; }   // metres the readout is short of the truth

    // From `at` metres of true distance on, read `behind` metres short.
    public void From(float at, float behind)
    {
        pending.Enqueue((at, System.Math.Max(0f, behind)));
    }

    public void Advance(float trueDistance)
    {
        while (pending.Count > 0 && trueDistance >= pending.Peek().At)
        {
            Behind = pending.Dequeue().Behind;
        }
    }

    public float Shown(float trueDistance)
    {
        return System.Math.Max(0f, trueDistance - Behind);
    }

    public void Reset()
    {
        Behind = 0f;
        pending.Clear();
    }
}
