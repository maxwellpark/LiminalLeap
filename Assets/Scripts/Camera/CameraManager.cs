using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    // Unassigned on the prefab: the authored reference only exists in MovementTestScene,
    // so anywhere else has to find the one the Player prefab carries.
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
