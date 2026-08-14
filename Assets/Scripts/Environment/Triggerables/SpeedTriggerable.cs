using UnityEngine;

public class SpeedTriggerable : MonoBehaviour, ITriggerable, IRunResettable
{
    [SerializeField] private float speedToAdd;
    [SerializeField] private bool shakeCamera;
    [SerializeField] private CameraShakeSettings shakeSettings;

    private Renderer rend;
    private Collider trigger;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        trigger = GetComponent<Collider>();
    }

    public float Trigger()
    {
        if (shakeCamera)
        {
            CameraManager.GetInstance().Shake(shakeSettings);
        }

        SetCollected(true);
        AudioManager.GetInstance().Play(Sound.Pickup);
        ToastManager.GetInstance().Show($"+{speedToAdd:F0} speed");
        return speedToAdd;
    }

    public void ResetForNewRun()
    {
        SetCollected(false);
    }

    // Collider goes with the renderer, or a collected pickup keeps boosting you invisibly.
    private void SetCollected(bool collected)
    {
        if (rend != null)
        {
            rend.enabled = !collected;
        }

        if (trigger != null)
        {
            trigger.enabled = !collected;
        }
    }
}
