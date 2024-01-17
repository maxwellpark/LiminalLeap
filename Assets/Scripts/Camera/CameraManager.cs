using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private CameraShake cameraShake;

    public void Shake(CameraShakeSettings settings)
    {
        cameraShake.Shake(settings);
    }
}
