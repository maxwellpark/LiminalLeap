// The pieces most recently laid, so a looping door can lay them again exactly.
public sealed class TrackMemory
{
    public readonly struct Piece
    {
        public readonly int Prefab;
        public readonly int Seed;   // what the scenery was varied with
        public readonly float Start; // what the HUD read where it began
        public readonly Floor Floor;

        public Piece(int prefab, int seed, float start = 0f, Floor floor = Floor.Linoleum)
        {
            Prefab = prefab;
            Seed = seed;
            Start = start;
            Floor = floor;
        }
    }

    private readonly Piece[] ring;
    private int next;

    public int Count { get; private set; }

    public TrackMemory(int capacity)
    {
        ring = new Piece[System.Math.Max(1, capacity)];
    }

    public void Add(Piece piece)
    {
        ring[next] = piece;
        next = (next + 1) % ring.Length;
        Count = System.Math.Min(Count + 1, ring.Length);
    }

    // Oldest first, as they were run. Returns how many were copied.
    public int CopyLast(int count, Piece[] into)
    {
        var n = System.Math.Max(0, System.Math.Min(System.Math.Min(count, Count), into.Length));
        for (var i = 0; i < n; i++)
        {
            into[i] = ring[(next - n + i + ring.Length * 2) % ring.Length];
        }

        return n;
    }

    // The first of the last `count`, which is where a looping door's replay begins.
    public bool TryOldestOfLast(int count, out Piece piece)
    {
        var n = System.Math.Min(count, Count);
        if (n <= 0)
        {
            piece = default;
            return false;
        }

        piece = ring[(next - n + ring.Length * 2) % ring.Length];
        return true;
    }

    public void Clear()
    {
        next = 0;
        Count = 0;
    }
}
