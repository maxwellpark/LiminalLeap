using UnityEngine;

// The way out. Banking is the only way the score survives.
public class ExitDoor : MonoBehaviour, IRunResettable
{
    [SerializeField] private float chimeRange = 45f;

    private bool announced;

    private void Start()
    {
        // Off means no door, so the variant compares cleanly.
        if (!Features.On(Feature.ExitDoors))
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetForNewRun()
    {
        announced = false;
    }

    private void Update()
    {
        if (announced || chimeRange <= 0f)
        {
            return;
        }

        if (Vector3.Distance(PlayerTrackMovement.Position, transform.position) > chimeRange)
        {
            return;
        }

        announced = true;
        AudioManager.GetInstance().Play(Sound.ExitNear);
    }
}
