// Linoleum is the floor the movement was tuned on.
public enum Floor
{
    Linoleum,
    Carpet,
    Tile,
    Boards,
}

// How a floor moves you, each against linoleum's 1.
public readonly struct FloorFeel
{
    public readonly float Grip;        // strafe acceleration: below 1 slides in and out of a lane
    public readonly float Bob;         // height of the running bob
    public readonly float LandPitch;
    public readonly float LandVolume;

    public FloorFeel(float grip, float bob, float landPitch, float landVolume)
    {
        Grip = grip;
        Bob = bob;
        LandPitch = landPitch;
        LandVolume = landVolume;
    }

    public static FloorFeel For(Floor floor)
    {
        return floor switch
        {
            Floor.Carpet => new FloorFeel(1.25f, 0.6f, 0.8f, 0.55f),
            Floor.Tile => new FloorFeel(0.35f, 1f, 1.15f, 1f),
            Floor.Boards => new FloorFeel(1f, 1.35f, 0.7f, 1.1f),
            _ => new FloorFeel(1f, 1f, 1f, 1f),
        };
    }
}

// Which floor each piece gets: stretches from the run's seed, never the same floor twice running.
public sealed class FloorRule
{
    private const int Kinds = 4;

    private readonly int seed;
    private readonly int shortest;
    private readonly int longest;
    private readonly bool fixedFloor;
    private readonly Floor only;

    // The stretch `At` last landed in, so a run reads forward without starting over.
    private int stretch;
    private int stretchStart;
    private int stretchEnd;
    private Floor stretchFloor;

    public FloorRule(int seed, int shortest, int longest)
    {
        this.seed = seed;
        this.shortest = System.Math.Max(1, shortest);
        this.longest = System.Math.Max(this.shortest, longest);
        Restart();
    }

    private FloorRule(Floor floor)
    {
        fixedFloor = true;
        only = floor;
    }

    public static FloorRule Only(Floor floor)
    {
        return new FloorRule(floor);
    }

    public Floor At(int ordinal)
    {
        if (fixedFloor)
        {
            return only;
        }

        if (ordinal < stretchStart)
        {
            Restart();
        }

        while (ordinal >= stretchEnd)
        {
            stretch++;
            stretchStart = stretchEnd;
            stretchEnd = stretchStart + LengthOf(stretch);
            stretchFloor = (Floor)(((int)stretchFloor + 1 + (int)(Hash(stretch, 1) % (Kinds - 1))) % Kinds);
        }

        return stretchFloor;
    }

    // Opens on linoleum, so the controls are learned on the floor they were tuned on.
    private void Restart()
    {
        stretch = 0;
        stretchStart = 0;
        stretchEnd = LengthOf(0);
        stretchFloor = Floor.Linoleum;
    }

    private int LengthOf(int index)
    {
        return shortest + (int)(Hash(index, 0) % (uint)(longest - shortest + 1));
    }

    private uint Hash(int index, int salt)
    {
        unchecked
        {
            var h = (uint)(seed * 73856093) ^ (uint)(index * 19349663) ^ (uint)(salt * 83492791);
            h ^= h >> 13;
            h *= 0x85ebca6b;
            h ^= h >> 16;
            return h;
        }
    }
}
