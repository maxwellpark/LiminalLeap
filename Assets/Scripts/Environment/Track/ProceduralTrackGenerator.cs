using System.Collections.Generic;
using UnityEngine;

// Chains piece prefabs ahead of the player and pools the ones behind.
// Anchored pieces chain socket to socket, so turns work; anchorless ones fall back
// to the old centre-origin maths.
public class ProceduralTrackGenerator : MonoBehaviour
{
    [SerializeField] private TrackPiece[] piecePrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private int piecesAhead = 8;       // pieces kept spawned in front
    [SerializeField] private float recycleBehind = 6f;  // recycle once the player is this far past a piece
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private Vector3 startForward = Vector3.forward;
    [SerializeField] private int seed = 12345;          // same seed, same track
    [SerializeField] private int maxRepeats = 2;        // stop one prefab running away with it

    private readonly List<TrackPiece> active = new();
    private readonly Dictionary<int, Queue<TrackPiece>> pools = new();
    private Vector3 nextEnd;
    private Vector3 nextForward;
    private System.Random rng;
    private int lastIndex = -1;
    private int repeats;

    public IReadOnlyList<TrackPiece> ActivePieces => active;

    private void Start()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        for (var i = 0; i < active.Count; i++)
        {
            Recycle(active[i]);
        }
        active.Clear();

        rng = new System.Random(seed);
        lastIndex = -1;
        repeats = 0;

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
        piece.gameObject.SetActive(true);
        piece.Passed = false;

        piece.transform.SetPositionAndRotation(
            piece.HasEndAnchor ? nextEnd : TrackChainer.NextPiecePosition(nextEnd, nextForward, piece.Length()),
            Quaternion.LookRotation(nextForward));

        active.Add(piece);
        nextEnd = piece.GetEndPosition();
        nextForward = piece.GetEndForward();
    }

    private int PickIndex()
    {
        if (piecePrefabs.Length == 1)
        {
            return 0;
        }

        int index;
        do
        {
            index = rng.Next(piecePrefabs.Length);
        }
        while (index == lastIndex && repeats >= maxRepeats);

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
