using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : Singleton<TrackManager>
{
    [SerializeField] private Track startingTrack;
    // When set, pieces come from the procedural generator instead of hand-placed Tracks.
    [SerializeField] private ProceduralTrackGenerator generator;
    private Track currentTrack;
    private Track[] tracks;

    protected override void Awake()
    {
        base.Awake();
        // sort mode None: tracks is never read in order
        tracks = FindObjectsByType<Track>(FindObjectsSortMode.None);
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

    public TrackPiece GetClosestPiece(Vector3 position)
    {
        IEnumerable<TrackPiece> pieces = generator != null
            ? generator.ActivePieces
            : currentTrack != null ? currentTrack.Pieces : System.Array.Empty<TrackPiece>();

        return pieces
            .Where(t => !t.Passed)
            .OrderBy(p => Vector3.Distance(position, p.GetEndPosition()))
            .FirstOrDefault();
    }
}
