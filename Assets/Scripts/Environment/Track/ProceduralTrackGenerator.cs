using System.Collections.Generic;
using UnityEngine;

// Endless track by chaining piece prefabs ahead of the player and pooling the ones left
// behind, so you author a handful of piece prefabs instead of hand-placing a level.
// Assign a few TrackPiece prefabs and the player transform, tune the window. Straight
// pieces chain cleanly; turn/curve pieces want an end anchor (see follow-up notes).
//
// Unverified: written outside the editor. TrackManager reads ActivePieces when assigned.
public class ProceduralTrackGenerator : MonoBehaviour
{
    [SerializeField] private TrackPiece[] piecePrefabs;
    [SerializeField] private Transform player;
    [SerializeField] private int piecesAhead = 8;       // pieces kept spawned in front
    [SerializeField] private float recycleBehind = 6f;  // recycle once the player is this far past a piece
    [SerializeField] private Vector3 startPosition = Vector3.zero;
    [SerializeField] private Vector3 startForward = Vector3.forward;

    private readonly List<TrackPiece> active = new();
    private readonly Queue<TrackPiece> pool = new();
    private Vector3 nextEnd;
    private Vector3 nextForward;

    public IReadOnlyList<TrackPiece> ActivePieces => active;

    private void Start()
    {
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
            && TrackChainer.ShouldRecycle(active[0].GetEndPosition(), active[0].transform.forward, player.position, recycleBehind))
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

        var piece = pool.Count > 0 ? pool.Dequeue() : Instantiate(piecePrefabs[Random.Range(0, piecePrefabs.Length)], transform);
        piece.gameObject.SetActive(true);
        piece.Passed = false;

        piece.transform.SetPositionAndRotation(
            TrackChainer.NextPiecePosition(nextEnd, nextForward, PieceLength(piece)),
            Quaternion.LookRotation(nextForward));

        active.Add(piece);
        nextEnd = piece.GetEndPosition();
        nextForward = piece.transform.forward;
    }

    private void Recycle(TrackPiece piece)
    {
        piece.gameObject.SetActive(false);
        pool.Enqueue(piece);
    }

    private static float PieceLength(TrackPiece piece)
    {
        var r = piece.GetComponent<Renderer>();
        return r != null ? r.bounds.size.z : 10f;
    }
}
