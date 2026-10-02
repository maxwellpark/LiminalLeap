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

    // Death only reset the player, so a second run inherited passed pieces and the old branch.
    public void ResetRun()
    {
        if (generator != null)
        {
            generator.ResetRun();
        }
        else
        {
            foreach (var track in FindObjectsByType<Track>(FindObjectsSortMode.None))
            {
                var pieces = track.Pieces;
                if (pieces == null)
                {
                    continue;
                }

                for (var i = 0; i < pieces.Length; i++)
                {
                    pieces[i].Passed = false;
                }
            }
        }

        // includes inactive, or a pickup that hid itself never comes back
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mb is IRunResettable resettable)
            {
                resettable.ResetForNewRun();
            }
        }

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

    // In running order, passed ones included. For anything that has to look down the track
    // rather than straight along the camera, which stops being the track a few turns out.
    public IReadOnlyList<TrackPiece> Pieces => generator != null
        ? generator.ActivePieces
        : currentTrack != null ? currentTrack.Pieces : NoPieces;

    // Runs every frame from the player, so no LINQ and no sqrt.
    public TrackPiece GetClosestPiece(Vector3 position)
    {
        var pieces = Pieces;

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
