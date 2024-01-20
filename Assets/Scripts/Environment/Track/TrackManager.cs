using System.Linq;
using UnityEngine;

public class TrackManager : Singleton<TrackManager>
{
    [SerializeField] private Track startingTrack;
    private Track currentTrack;
    private Track[] tracks;

    protected override void Awake()
    {
        base.Awake();
        tracks = FindObjectsOfType<Track>();
        SwitchTrack(startingTrack);
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
        return currentTrack.Pieces
            .Where(t => !t.Passed)
            .OrderBy(p => Vector3.Distance(position, p.GetEndPosition()))
            .FirstOrDefault();
    }
}
