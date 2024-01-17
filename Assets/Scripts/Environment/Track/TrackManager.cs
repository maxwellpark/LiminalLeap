using System.Linq;
using UnityEngine;

public class TrackManager : Singleton<TrackManager>
{
    private TrackPiece[] trackPieces;

    protected override void Awake()
    {
        base.Awake();
        trackPieces = FindObjectsOfType<TrackPiece>();
    }

    public TrackPiece GetClosestPiece(Vector3 position)
    {
        return trackPieces
            .Where(t => !t.Passed)
            .OrderBy(p => Vector3.Distance(position, p.GetEndPosition()))
            .FirstOrDefault();
    }
}
