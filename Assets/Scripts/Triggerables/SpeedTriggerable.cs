using UnityEngine;

public class SpeedTriggerable : MonoBehaviour, ITriggerable
{
    [SerializeField] private float speedToAdd;

    public float Trigger()
    {
        var renderer = GetComponent<Renderer>();
        renderer.enabled = false;
        //var color = renderer.material.color;
        //renderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
        Debug.Log("Adding speed: " + speedToAdd);
        return speedToAdd;
    }
}
