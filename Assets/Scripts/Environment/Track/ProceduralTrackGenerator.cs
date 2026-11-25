using System.Collections.Generic;
using UnityEngine;

// Chains pieces ahead and pools those behind. Anchored ones socket together.
public class ProceduralTrackGenerator : MonoBehaviour
{
    [SerializeField] private TrackPiece[] piecePrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private int piecesAhead = 8;       // pieces kept spawned in front
    [SerializeField] private float recycleBehind = 6f;  // recycle once the player is this far past a piece
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private Vector3 startForward = Vector3.forward;
    [SerializeField] private int seed = 12345;          // used when no run mode has been chosen
    [SerializeField] private int maxRepeats = 2;        // stop one prefab running away with it
    [SerializeField] private float playerMaxSpeed = 32f; // matches PlayerTrackMovement
    [SerializeField] private float jumpAirtime = 0.64f;
    [SerializeField] private float hazardMargin = 6f;
    [SerializeField] private int straightLeadIn = 6;    // opening stretch for the floor signage

    [Header("Exits")]
    [SerializeField] private int firstExitAfter = 22;
    [SerializeField] private float exitGapGrowth = 1.5f; // the way out gets rarer the deeper you go
    [SerializeField] private int storyRetryGap = 8;      // a missed story exit comes round again

    [Header("Calm")]
    [SerializeField] private int busyPieces = 22;   // stretch of ordinary track
    [SerializeField] private int calmPieces = 6;    // breath between them

    [Header("Loops")]
    [SerializeField] private int forkEvery = 24;
    [SerializeField] private int firstForkAfter = 20;
    [SerializeField] private int replayPieces = 10;     // how much track a looping door lays again

    [Header("Floors")]
    [SerializeField] private int shortestFloor = 8;     // pieces in an arcade run's stretch of one floor
    [SerializeField] private int longestFloor = 14;

    [Header("Signage")]
    [SerializeField, Range(0f, 1f)] private float lieChance = 0.22f;
    [SerializeField] private int signLeadPieces = 3;     // reading room before the thing arrives

    private readonly List<TrackPiece> active = new();
    private readonly Dictionary<int, Queue<TrackPiece>> pools = new();
    private Vector3 nextEnd;
    private Vector3 nextForward;
    private System.Random rng;
    private int lastIndex = -1;
    private int repeats;
    private int spawned;
    private int lastHazardAt = int.MinValue / 2;
    private int nextExitAt;
    private int exitsSpawned;
    private int placeLength;
    private int runSeed;

    // Set in a place with laps: the lap being laid, and how far into it.
    private LapRule laps;
    private int lap;
    private int firstLap;
    private int pieceInLap;

    // Set in an arcade run with loops: spawning holds at a fork until the player picks a door.
    private LoopRule loops;
    private readonly TrackMemory memory = new(32);

    // A place keeps its own floor; an arcade run changes floor in stretches.
    private FloorRule floors;
    private TrackMemory.Piece[] replay;
    private int replayCount;
    private int replayAt;
    private int lastForkAt;
    private TrackPiece pendingFork;

    // End to end, as the player counts distance, and how far the HUD will read short of it there.
    private float laidMetres;
    private float laidBehind;

    public bool HoldingAtFork => pendingFork != null;
    public int ForksTaken { get; private set; }
    public int LoopsTaken { get; private set; }
    public int LoopDoorAhead => loops != null ? loops.LoopDoor(ForksTaken) : -1;
    public TrackMemory Memory => memory;
    public int Laid => spawned;

    public IReadOnlyList<TrackPiece> ActivePieces => active;

    private void Start()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        // Applied here as well as by the player, since either may start first and signs read it.
        Places.Apply(Places.RulesForRun());

        for (var i = 0; i < active.Count; i++)
        {
            Recycle(active[i]);
        }
        active.Clear();

        // Per run, not per scene.
        runSeed = RunMode.SeedFor(seed, 0);
        rng = new System.Random(runSeed);
        lastIndex = -1;
        repeats = 0;
        spawned = 0;
        lastHazardAt = int.MinValue / 2;
        exitsSpawned = 0;

        var place = RunMode.Story ? Places.ForRun() : null;
        placeLength = place != null ? place.Length : 0;
        nextExitAt = ExitSchedule.First(placeLength, firstExitAfter);

        // From the lap the player is on, so dying replays it and a call moves on from it.
        laps = StoryLaps.For(place);
        StoryLaps.Laying = laps != null;
        lap = laps != null ? laps.Lap : 0;
        firstLap = lap;
        pieceInLap = 0;

        // The story's exits are already calls, so forks are for the arcade runs.
        loops = !RunMode.Story && Features.On(Feature.Loops) ? new LoopRule(runSeed, forkEvery, firstForkAfter) : null;
        floors = !Features.On(Feature.Surfaces) ? FloorRule.Only(Floor.Linoleum)
            : place != null ? FloorRule.Only(place.Floor)
            : new FloorRule(runSeed, shortestFloor, longestFloor);

        // Built now, or the first carpet or tile generates its texture mid-run and drops a frame.
        if (Features.On(Feature.Surfaces))
        {
            MaterialLibrary.ForFloor(Floor.Carpet);
            MaterialLibrary.ForFloor(Floor.Tile);
            MaterialLibrary.ForFloor(Floor.Boards);
        }
        replay ??= new TrackMemory.Piece[Mathf.Max(1, replayPieces)];
        memory.Clear();
        replayCount = 0;
        replayAt = 0;
        lastForkAt = int.MinValue / 2;
        pendingFork = null;
        laidMetres = 0f;
        laidBehind = 0f;
        ForksTaken = 0;
        LoopsTaken = 0;

        nextEnd = startPosition;
        nextForward = startForward.sqrMagnitude > 0f ? startForward.normalized : Vector3.forward;
        for (var i = 0; i < piecesAhead && pendingFork == null; i++)
        {
            SpawnNext();
        }
    }

    // Or the player would keep calling laps on a track that's gone.
    private void OnDestroy()
    {
        StoryLaps.Laying = false;
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        while (active.Count > 0
            && TrackChainer.ShouldRecycle(active[0].GetEndPosition(), active[0].GetEndForward(), player.position, recycleBehind))
        {
            var gone = active[0];

            // Behind you and never taken, which is a call too. ResetRun recycles without asking.
            if (laps != null && gone.EndsLap && gone.Lap == laps.Lap)
            {
                RanPast();
            }

            Recycle(gone);
            active.RemoveAt(0);
        }

        if (pendingFork != null)
        {
            TryCommit();
        }

        while (active.Count < piecesAhead && pendingFork == null)
        {
            SpawnNext();
        }
    }

    // Past the wall, the door the player went through decides what gets laid next.
    private void TryCommit()
    {
        var wall = pendingFork.ForkWall.transform;
        var toPlayer = player.position - wall.position;
        if (Vector3.Dot(toPlayer, wall.forward) < 0f)
        {
            return;
        }

        if (LoopRule.DoorFor(Vector3.Dot(toPlayer, wall.right)) == loops.LoopDoor(ForksTaken))
        {
            replayCount = memory.CopyLast(replayPieces, replay);
            replayAt = 0;
            LoopsTaken++;
        }

        ForksTaken++;
        pendingFork = null;
    }

    // What the next piece will be, decided before any of it is laid.
    private struct Laying
    {
        public int Index;
        public int Remembered;     // the prefab a replay was asked for, or -1 for fresh track
        public bool Replaying;
        public bool Fork;
        public int Seed;           // what the scenery is varied with
        public float ShownBefore;  // what the HUD read here the first time, on a replay
        public Floor Floor;
    }

    // In this order, as each step reads `spawned` and the rng where the one before left them.
    private void SpawnNext()
    {
        if (piecePrefabs == null || piecePrefabs.Length == 0)
        {
            return;
        }

        var laying = Choose();
        var piece = TakeSpaced(ref laying.Index);

        var exit = IsExit(laying.Index);
        if (exit)
        {
            ScheduleNextExit();
        }

        Ready(piece);
        Place(piece, ref laying);
        Remember(piece, laying, exit);

        LayLap(piece, exit);

        active.Add(piece);
        Announce(piece);

        laidMetres += Vector3.Distance(nextEnd, piece.GetEndPosition());
        nextEnd = piece.GetEndPosition();
        nextForward = piece.GetEndForward();
    }

    private Laying Choose()
    {
        var laying = new Laying { Remembered = -1, Replaying = replayAt < replayCount };

        if (laying.Replaying)
        {
            var from = replay[replayAt++];
            laying.Seed = from.Seed;
            laying.ShownBefore = from.Start;
            laying.Floor = from.Floor;
            laying.Index = laying.Remembered = from.Prefab;

            // A breath still means nothing to survive, even on track already run.
            if (Calm(spawned) && piecePrefabs[laying.Index] != null && piecePrefabs[laying.Index].ContainsHazard)
            {
                var clean = PickCleanIndex();
                laying.Index = clean >= 0 ? clean : laying.Index;
            }

            return laying;
        }

        laying.Floor = floors.At(spawned);
        laying.Fork = ForkDue();
        laying.Index = laying.Fork ? FindFlatIndex() : PickIndex();
        if (laying.Index < 0)
        {
            laying.Fork = false;
            laying.Index = PickIndex();
        }

        return laying;
    }

    // Back to back hazards are unavoidable: a jump covers more ground than one piece.
    // Random, or every swap was the same prefab.
    private TrackPiece TakeSpaced(ref int index)
    {
        var piece = Take(index);

        if (piece.ContainsHazard && spawned - lastHazardAt < HazardGap())
        {
            var clean = PickCleanIndex();
            if (clean >= 0)
            {
                Recycle(piece);
                index = clean;
                piece = Take(index);
            }
        }

        if (piece.ContainsHazard)
        {
            lastHazardAt = spawned;
        }

        return piece;
    }

    private void ScheduleNextExit()
    {
        exitsSpawned++;

        // With laps, every exit is a call, so they come once a lap rather than as retries.
        var gap = laps != null ? placeLength : storyRetryGap;
        nextExitAt = ExitSchedule.Next(spawned, exitsSpawned, placeLength, firstExitAfter, exitGapGrowth, gap);
    }

    // Counts the piece as laid and clears what it carried from its last time out of the pool.
    private void Ready(TrackPiece piece)
    {
        piece.Calm = Calm(spawned);
        piece.Ordinal = spawned;

        spawned++;
        piece.gameObject.SetActive(true);
        piece.Passed = false;

        // A recycled piece still carries the last sign it was given.
        var stale = piece.GetComponent<TrackSign>();
        if (stale != null)
        {
            stale.Hide();
        }

        var staleCount = piece.GetComponent<LapSign>();
        if (staleCount != null)
        {
            staleCount.Hide();
        }

        // Pooled pieces come back with pickups still collected otherwise.
        var resettables = piece.Resettables;
        for (var i = 0; i < resettables.Length; i++)
        {
            resettables[i].ResetForNewRun();
        }
    }

    private void Place(TrackPiece piece, ref Laying laying)
    {
        piece.transform.SetPositionAndRotation(
            piece.HasEndAnchor ? nextEnd : TrackChainer.NextPiecePosition(nextEnd, nextForward, piece.Length()),
            Quaternion.LookRotation(nextForward));

        // Seeded off the ordinal, so the same seed lays out the same corridor. A replay keeps its own.
        if (!laying.Replaying)
        {
            laying.Seed = runSeed + spawned;
        }

        if (piece.Scenery != null)
        {
            piece.Scenery.Vary(laying.Seed);
        }

        piece.SetFloor(laying.Floor);

        if (piece.ForkWall != null)
        {
            piece.ForkWall.SetActive(false);
        }
    }

    // What a looping door will need to lay this again, or the fork that holds the track here.
    private void Remember(TrackPiece piece, Laying laying, bool exit)
    {
        // Laid again, so the HUD reads here what it read the first time.
        if (laying.Replaying && laidMetres - laying.ShownBefore != laidBehind)
        {
            laidBehind = laidMetres - laying.ShownBefore;
            PlayerTrackMovement.Readout.From(laidMetres, laidBehind);
        }

        if (laying.Fork)
        {
            MakeFork(piece);
            EchoReplay(piece);
            pendingFork = piece;
            lastForkAt = spawned - 1;
            lastHazardAt = spawned - 1;
        }
        else if (!exit)
        {
            // The original, not a swap made for spacing, or each loop drifts further from it.
            var prefab = laying.Remembered >= 0 ? laying.Remembered : laying.Index;
            memory.Add(new TrackMemory.Piece(prefab, laying.Seed, laidMetres - laidBehind, laying.Floor));
        }
    }

    // The tell: the looping side's pillars stand as they did where its replay begins.
    private void EchoReplay(TrackPiece fork)
    {
        if (fork.Scenery == null || !memory.TryOldestOfLast(replayPieces, out var first))
        {
            return;
        }

        fork.Scenery.Echo(first.Seed, LoopDoorAhead == LoopRule.LeftDoor ? -1 : 1);
    }

    // Never in a breath or on top of another hazard, since the wall is one.
    private bool ForkDue()
    {
        return loops != null
            && loops.Due(spawned, lastForkAt)
            && !Calm(spawned)
            && spawned - lastHazardAt >= HazardGap();
    }

    // A wall across the middle lane with a doorway either side. Built once per pooled piece.
    private static void MakeFork(TrackPiece piece)
    {
        if (piece.ForkWall == null)
        {
            // The baked track material, not the floor the piece happens to be wearing now.
            var deck = piece.GetComponentInChildren<Renderer>();
            var material = piece.BakedFloor != null ? piece.BakedFloor : deck != null ? deck.sharedMaterial : null;
            var wall = new GameObject("ForkWall");
            wall.transform.SetParent(piece.transform, false);

            // Halfway between start and end, whether the piece is placed by its start or its centre.
            var end = piece.transform.InverseTransformPoint(piece.GetEndPosition());
            wall.transform.localPosition = piece.HasEndAnchor ? new Vector3(0f, 0f, end.z * 0.5f) : Vector3.zero;

            // Only the middle can kill: the side posts sit past where strafing reaches.
            var middle = ForkPart(wall.transform, "Middle", new Vector3(0f, 1.5f, 0f), new Vector3(2.6f, 3f, 0.4f), material);
            middle.GetComponent<Collider>().isTrigger = true;
            middle.AddComponent<Hazard>().Fixed = true;

            Object.Destroy(ForkPart(wall.transform, "LeftPost", new Vector3(-3.4f, 1.6f, 0f), new Vector3(1.2f, 3.2f, 0.4f), material).GetComponent<Collider>());
            Object.Destroy(ForkPart(wall.transform, "RightPost", new Vector3(3.4f, 1.6f, 0f), new Vector3(1.2f, 3.2f, 0.4f), material).GetComponent<Collider>());
            Object.Destroy(ForkPart(wall.transform, "Lintel", new Vector3(0f, 3.5f, 0f), new Vector3(8f, 0.6f, 0.4f), material).GetComponent<Collider>());

            piece.ForkWall = wall;
            piece.ForgetResettables();
        }

        piece.ForkWall.SetActive(true);
    }

    private static GameObject ForkPart(Transform parent, string name, Vector3 at, Vector3 size, Material material)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = at;
        part.transform.localScale = size;
        if (material != null)
        {
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        return part;
    }

    private void LayLap(TrackPiece piece, bool exit)
    {
        piece.Lap = lap;
        piece.EndsLap = false;
        piece.Anomaly = AnomalyKind.None;

        if (laps == null)
        {
            return;
        }

        if (laps.InStretch(lap, pieceInLap, placeLength))
        {
            piece.Anomaly = laps.KindFor(lap);
            if (piece.Scenery != null)
            {
                piece.Scenery.MakeWrong(piece.Anomaly);
            }
        }

        // Clear of the control signage on the first lap laid.
        if (pieceInLap == (lap == firstLap ? straightLeadIn : 1))
        {
            var sign = piece.GetComponent<LapSign>();
            if (sign == null)
            {
                sign = piece.gameObject.AddComponent<LapSign>();
            }

            sign.Paint(lap, lap == laps.CleanLap);
        }

        if (exit)
        {
            piece.EndsLap = true;
            lap++;
            pieceInLap = 0;
        }
        else
        {
            pieceInLap++;
        }
    }

    private void RanPast()
    {
        if (laps.Call(tookExit: false) != LapCall.Wrong)
        {
            return;
        }

        // Back to the start. The pieces ahead were laid for a lap that isn't coming.
        var mover = player.GetComponent<PlayerTrackMovement>();
        if (mover != null)
        {
            mover.StartLap();
        }
    }

    // Painted a few pieces back so it can be read before the thing it describes turns up.
    private void Announce(TrackPiece piece)
    {
        if (!Features.On(Feature.LyingSigns) || active.Count <= signLeadPieces)
        {
            return;
        }

        // Only what is worth announcing, or the signs become wallpaper.
        var truth = TruthFor(piece);
        if (truth == SignKind.Clear)
        {
            return;
        }

        var host = active[active.Count - 1 - signLeadPieces];
        var sign = host.GetComponent<TrackSign>();
        if (sign == null)
        {
            sign = host.gameObject.AddComponent<TrackSign>();
        }

        sign.Paint(SignText.Choose(truth, (float)rng.NextDouble(), lieChance), truth);
    }

    private static SignKind TruthFor(TrackPiece piece)
    {
        if (piece.GetComponentInChildren<ExitDoor>(true) != null)
        {
            return SignKind.ExitAhead;
        }

        // Active only, which also skips an old fork's wall a pooled piece keeps switched off.
        var hazard = piece.GetComponentInChildren<Hazard>();
        if (hazard != null)
        {
            return hazard.Jumpable ? SignKind.Jump : SignKind.Strafe;
        }

        // Flagged but nothing found. A vague warning beats a missed one.
        return piece.ContainsHazard ? SignKind.Strafe : SignKind.Clear;
    }

    private bool IsExit(int index)
    {
        return index >= 0
            && index < piecePrefabs.Length
            && piecePrefabs[index] != null
            && piecePrefabs[index].GetComponentInChildren<ExitDoor>(true) != null;
    }

    private int FindExitIndex()
    {
        for (var i = 0; i < piecePrefabs.Length; i++)
        {
            if (IsExit(i))
            {
                return i;
            }
        }

        return -1;
    }

    private int HazardGap()
    {
        var pieceLength = piecePrefabs.Length > 0 && piecePrefabs[0] != null ? piecePrefabs[0].Length() : 10f;
        return HazardLanes.RequiredPieceGap(playerMaxSpeed, jumpAirtime, pieceLength, hazardMargin);
    }

    private int FindFlatIndex()
    {
        for (var i = 0; i < piecePrefabs.Length; i++)
        {
            var p = piecePrefabs[i];
            if (p != null && p.IsPlain && p.TurnDegrees < 1f)
            {
                return i;
            }
        }

        return -1;
    }

    private bool Calm(int piece)
    {
        return Features.On(Feature.CalmSections) && CalmRhythm.IsCalm(piece, busyPieces, calmPieces);
    }

    // Random, or every calm stretch is the same prefab.
    private int PickCleanIndex()
    {
        var options = 0;
        for (var i = 0; i < piecePrefabs.Length; i++)
        {
            if (Safe(i))
            {
                options++;
            }
        }

        if (options == 0)
        {
            return -1;
        }

        var wanted = rng.Next(options);
        for (var i = 0; i < piecePrefabs.Length; i++)
        {
            if (Safe(i) && wanted-- == 0)
            {
                return i;
            }
        }

        return -1;
    }

    private bool Safe(int index)
    {
        var piece = piecePrefabs[index];
        return piece != null && !piece.ContainsHazard && !IsExit(index);
    }

    private int PickIndex()
    {
        if (piecePrefabs.Length == 1)
        {
            return 0;
        }

        if (spawned < straightLeadIn)
        {
            var flat = FindFlatIndex();
            if (flat >= 0)
            {
                return flat;
            }
        }

        // Before the breath, which only rules out hazards: a lap's length is its exit's schedule.
        if (Features.On(Feature.ExitDoors) && spawned >= nextExitAt)
        {
            var exit = FindExitIndex();
            if (exit >= 0)
            {
                return exit;
            }
        }

        // A breath means nothing to survive, so nothing that can kill you goes in it.
        if (Calm(spawned))
        {
            var clean = PickCleanIndex();
            if (clean >= 0)
            {
                return clean;
            }
        }

        // Exits are scheduled, never rolled, or the way out would turn up at random.
        int index;
        var guard = 0;
        do
        {
            index = rng.Next(piecePrefabs.Length);
        }
        while (((index == lastIndex && repeats >= maxRepeats) || IsExit(index)) && guard++ < 16);

        // Out of rerolls, a repeat beats an exit nobody scheduled, which would end a lap early.
        if (IsExit(index))
        {
            var flat = FindFlatIndex();
            index = flat >= 0 ? flat : index;
        }

        repeats = index == lastIndex ? repeats + 1 : 0;
        lastIndex = index;
        return index;
    }

    private TrackPiece Take(int index)
    {
        if (pools.TryGetValue(index, out var queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }

        var piece = Instantiate(piecePrefabs[index], transform);
        piece.PrefabIndex = index;
        return piece;
    }

    private void Recycle(TrackPiece piece)
    {
        piece.gameObject.SetActive(false);
        if (!pools.TryGetValue(piece.PrefabIndex, out var queue))
        {
            queue = new Queue<TrackPiece>();
            pools[piece.PrefabIndex] = queue;
        }
        queue.Enqueue(piece);
    }
}
