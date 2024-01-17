using UnityEngine;

public class SpeedTriggerable : MonoBehaviour, ITriggerable
{
    [SerializeField] private float speedToAdd;
    [SerializeField] private bool shakeCamera;
    [SerializeField] private CameraShakeSettings shakeSettings;

    public float Trigger()
    {
        if (shakeCamera)
        {
            var cm = CameraManager.GetInstance();
            cm.Shake(shakeSettings);
        }

        var renderer = GetComponent<Renderer>();
        renderer.enabled = false;
        //var color = renderer.material.color;
        //renderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
        Debug.Log("Adding speed: " + speedToAdd);
        return speedToAdd;
    }
}
