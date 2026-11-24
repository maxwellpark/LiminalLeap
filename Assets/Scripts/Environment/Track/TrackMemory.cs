// The pieces most recently laid, so a looping door can lay them again exactly.
public sealed class TrackMemory
{
    public readonly struct Piece
    {
        public readonly int Prefab;
        public readonly int Seed;   // what the scenery was varied with

        public Piece(int prefab, int seed)
        {
            Prefab = prefab;
            Seed = seed;
        }
    }

    private readonly Piece[] ring;
    private int next;

    public int Count { get; private set; }
    public int Capacity => ring.Length;

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

    public void Clear()
    {
        next = 0;
        Count = 0;
    }
}
