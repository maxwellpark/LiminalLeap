using UnityEngine;

// Wider trigger around a hazard. Credit is given on exit, not entry, so dying inside
// one doesn't count as a near miss.
[RequireComponent(typeof(Collider))]
public class NearMissZone : MonoBehaviour, IRunResettable
{
    [SerializeField] private float reward = 1f;

    private bool inside;
    private bool credited;

    private void OnTriggerEnter(Collider other)
    {
        if (!credited && other.GetComponentInParent<PlayerTrackMovement>() != null)
        {
            inside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponentInParent<PlayerTrackMovement>();
        if (!inside || credited || player == null)
        {
            return;
        }

        inside = false;
        credited = true;
        player.RegisterNearMiss(reward);
    }

    public void ResetForNewRun()
    {
        inside = false;
        credited = false;
    }
}
