using UnityEngine;

// Steer to commit: you take whichever branch you're strafed into when you cross the split.
// The transform's position and forward define that split plane, its right defines the lanes.
public class Junction : DistanceActivatable
{
    [SerializeField] private Track[] tracks;
    [SerializeField] private float laneDeadzone = 1f; // width of the middle lane, 3+ branches only

    private bool resolved;

    private void Update()
    {
        if (resolved || tracks == null || tracks.Length == 0 || !InRange)
        {
            return;
        }

        var toPlayer = PlayerTrackMovement.Position - transform.position;
        if (Vector3.Dot(toPlayer, transform.forward) < 0f)
        {
            return;
        }

        resolved = true;
        TrackManager.GetInstance().SwitchTrack(tracks[ChooseBranch(Vector3.Dot(toPlayer, transform.right))]);
        Destroy(gameObject);
    }

    private int ChooseBranch(float lateral)
    {
        if (tracks.Length < 3)
        {
            return lateral < 0f ? 0 : tracks.Length - 1;
        }

        if (lateral < -laneDeadzone)
        {
            return 0;
        }

        return lateral > laneDeadzone ? tracks.Length - 1 : 1;
    }
}
