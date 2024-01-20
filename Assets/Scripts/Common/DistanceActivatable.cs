using UnityEngine;

public class DistanceActivatable : MonoBehaviour
{
    [SerializeField] private float activationDistance;
    protected bool InRange => Vector3.Distance(transform.position, PlayerMovement.Position) <= activationDistance;
}
