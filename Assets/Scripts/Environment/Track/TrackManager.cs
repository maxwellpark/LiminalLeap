using System.Collections.Generic;
using UnityEngine;

public class TrackManager : Singleton<TrackManager>
{
    private static readonly TrackPiece[] NoPieces = System.Array.Empty<TrackPiece>();

    [SerializeField] private Track startingTrack;
    // When set, pieces come from the procedural generator instead of hand-placed Tracks.
    [SerializeField] private ProceduralTrackGenerator generator;
    private Track currentTrack;

    protected override void Awake()
    {
        base.Awake();
        if (startingTrack != null)
        {
            SwitchTrack(startingTrack);
        }
    }

    public void SwitchTrack(Track track)
    {
        if (currentTrack != null)
        {
            currentTrack.Active = false;
        }
        track.Active = true;
        currentTrack = track;
    }

    // Runs every frame from the player, so no LINQ and no sqrt.
    public TrackPiece GetClosestPiece(Vector3 position)
    {
        IReadOnlyList<TrackPiece> pieces = generator != null
            ? generator.ActivePieces
            : currentTrack != null ? currentTrack.Pieces : NoPieces;

        TrackPiece closest = null;
        var bestSqr = float.MaxValue;

        for (var i = 0; i < pieces.Count; i++)
        {
            var piece = pieces[i];
            if (piece == null || piece.Passed)
            {
                continue;
            }

            var sqr = (position - piece.GetEndPosition()).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                closest = piece;
            }
        }

        return closest;
    }
}
