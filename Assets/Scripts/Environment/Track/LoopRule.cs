// Where the track forks, and which door goes back over track already run.
public sealed class LoopRule
{
    public const int LeftDoor = 0;
    public const int RightDoor = 1;

    public int Seed { get; }
    public int Every { get; }       // pieces from one fork to the next
    public int FirstAfter { get; }  // clear of the lead-in and its signage

    public LoopRule(int seed, int every, int firstAfter)
    {
        Seed = seed;
        Every = System.Math.Max(1, every);
        FirstAfter = System.Math.Max(0, firstAfter);
    }

    // Due, not exact: a fork that couldn't go in its slot goes in the next one that's free.
    public bool Due(int spawned, int lastForkAt)
    {
        return spawned >= FirstAfter && spawned - lastForkAt >= Every;
    }

    public int LoopDoor(int fork)
    {
        unchecked
        {
            var h = (uint)(Seed * 73856093) ^ (uint)(fork * 19349663);
            h ^= h >> 13;
            h *= 0x85ebca6b;
            h ^= h >> 16;
            return (h & 1) == 0 ? LeftDoor : RightDoor;
        }
    }

    // Steered left of the wall's centre is the left door.
    public static int DoorFor(float lateral)
    {
        return lateral < 0f ? LeftDoor : RightDoor;
    }
}
