using UnityEngine;

// A pickup that sits perfectly still reads as scenery. The travel is deliberately small:
// the player's trigger only reaches so far, and pickups being a hair out of reach has
// already been a bug once.
public class PickupBob : MonoBehaviour, IRunResettable
{
    [SerializeField] private float height = 0.12f;
    [SerializeField] private float cyclesPerSecond = 1.1f;
    [SerializeField] private float spinDegreesPerSecond = 55f;

    private Vector3 home;
    private Quaternion facing;
    private float phase;

    private void Awake()
    {
        home = transform.localPosition;
        facing = transform.localRotation;

        // Seeded off its own position, so a row of them doesn't pulse in lockstep.
        phase = Mathf.Repeat(home.x * 1.7f + home.z * 0.31f, Mathf.PI * 2f);
    }

    public void ResetForNewRun()
    {
        transform.localPosition = home;
        transform.localRotation = facing;
    }

    private void Update()
    {
        phase += cyclesPerSecond * Mathf.PI * 2f * Time.deltaTime;

        transform.localPosition = home + Vector3.up * (Mathf.Sin(phase) * height);
        transform.Rotate(Vector3.up, spinDegreesPerSecond * Time.deltaTime, Space.Self);
    }
}
