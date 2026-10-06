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

    public IReadOnlyList<TrackPiece> ActivePieces => active;

    private void Start()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        // Applied here as well as by the player, since either may start first and signs read it.
        Places.Apply(Places.ForRun());

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

        nextEnd = startPosition;
        nextForward = startForward.sqrMagnitude > 0f ? startForward.normalized : Vector3.forward;
        for (var i = 0; i < piecesAhead; i++)
        {
            SpawnNext();
        }
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
            Recycle(active[0]);
            active.RemoveAt(0);
        }

        while (active.Count < piecesAhead)
        {
            SpawnNext();
        }
    }

    private void SpawnNext()
    {
        if (piecePrefabs == null || piecePrefabs.Length == 0)
        {
            return;
        }

        var index = PickIndex();
        var piece = Take(index);

        // Back to back hazards are unavoidable: a jump covers more ground than one piece.
        // Random, or every swap was the same prefab.
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

        if (IsExit(index))
        {
            exitsSpawned++;
            nextExitAt = ExitSchedule.Next(spawned, exitsSpawned, placeLength, firstExitAfter, exitGapGrowth, storyRetryGap);
        }

        piece.Calm = Calm(spawned);

        spawned++;
        piece.gameObject.SetActive(true);
        piece.Passed = false;

        // A recycled piece still carries the last sign it was given.
        var stale = piece.GetComponent<TrackSign>();
        if (stale != null)
        {
            stale.Hide();
        }

        // Pooled pieces come back with pickups still collected otherwise.
        var resettables = piece.Resettables;
        for (var i = 0; i < resettables.Length; i++)
        {
            resettables[i].ResetForNewRun();
        }

        piece.transform.SetPositionAndRotation(
            piece.HasEndAnchor ? nextEnd : TrackChainer.NextPiecePosition(nextEnd, nextForward, piece.Length()),
            Quaternion.LookRotation(nextForward));

        // Seeded off the ordinal, so the same seed lays out the same corridor.
        if (piece.Scenery != null)
        {
            piece.Scenery.Vary(runSeed + spawned);
        }

        active.Add(piece);
        Announce(piece);

        nextEnd = piece.GetEndPosition();
        nextForward = piece.GetEndForward();
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

        var hazard = piece.GetComponentInChildren<Hazard>(true);
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

        // A breath means nothing to survive, so nothing that can kill you goes in it.
        if (Calm(spawned))
        {
            var clean = PickCleanIndex();
            if (clean >= 0)
            {
                return clean;
            }
        }

        if (Features.On(Feature.ExitDoors) && spawned >= nextExitAt)
        {
            var exit = FindExitIndex();
            if (exit >= 0)
            {
                return exit;
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
