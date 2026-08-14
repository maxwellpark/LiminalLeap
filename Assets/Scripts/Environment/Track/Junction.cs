using UnityEngine;

// Steer to commit: you take whichever branch you're strafed into when you cross the split.
// The transform's position and forward define that split plane, its right defines the lanes.
public class Junction : DistanceActivatable, IRunResettable
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
        var lateral = Vector3.Dot(toPlayer, transform.right);
        var choice = ChooseBranch(lateral, tracks.Length, laneDeadzone);
        TrackManager.GetInstance().SwitchTrack(tracks[choice]);
        ToastManager.GetInstance().Show(BranchName(choice));
    }

    // Kept alive across runs so the fork can be taken again after a death.
    public void ResetForNewRun()
    {
        resolved = false;
    }

    private string BranchName(int choice)
    {
        if (choice == 0)
        {
            return "Left";
        }

        return choice == tracks.Length - 1 ? "Right" : "Straight";
    }

    public static int ChooseBranch(float lateral, int branchCount, float deadzone)
    {
        if (branchCount < 3)
        {
            return lateral < 0f ? 0 : branchCount - 1;
        }

        if (lateral < -deadzone)
        {
            return 0;
        }

        return lateral > deadzone ? branchCount - 1 : 1;
    }
}
