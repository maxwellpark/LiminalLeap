using UnityEngine;

public class SpeedTriggerable : MonoBehaviour, ITriggerable
{
    [SerializeField] private float speedToAdd;
    [SerializeField] private bool shakeCamera;
    [SerializeField] private CameraShakeSettings shakeSettings;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public float Trigger()
    {
        if (shakeCamera)
        {
            CameraManager.GetInstance().Shake(shakeSettings);
        }

        if (rend != null)
        {
            rend.enabled = false;
        }

        AudioManager.GetInstance().Play(Sound.Pickup);
        return speedToAdd;
    }
}
