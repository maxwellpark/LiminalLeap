using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    // Unassigned on the prefab, so anywhere but MovementTestScene has to find it.
    [SerializeField] private CameraShake cameraShake;

    public override void Init()
    {
        if (cameraShake == null)
        {
            cameraShake = FindFirstObjectByType<CameraShake>();
        }
    }

    public void Shake(CameraShakeSettings settings)
    {
        if (cameraShake == null)
        {
            cameraShake = FindFirstObjectByType<CameraShake>();
        }

        if (cameraShake != null)
        {
            cameraShake.Shake(settings);
        }
    }
}
