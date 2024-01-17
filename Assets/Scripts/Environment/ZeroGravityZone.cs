using UnityEngine;

public class ZeroGravityZone : MonoBehaviour
{
    [SerializeField] private float strength = 10f;
    [SerializeField] private LayerMask layers;
    [SerializeField] private float activationDistance;

    private void OnTriggerStay(Collider other)
    {
        var distanceFromPlayer = Vector3.Distance(transform.position, PlayerMovement.Position);
        //Debug.Log("distanceFromPlayer = " + distanceFromPlayer);

        if (distanceFromPlayer > activationDistance)
        {
            return;
        }

        if ((layers.value & (1 << other.gameObject.layer)) != 0)
        {
            var distanceToCenter = Vector3.Distance(transform.position, other.transform.position);
            var zeroGravityForce = strength / distanceToCenter;

            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                var forceDirection = (transform.position - other.transform.position).normalized;
                rb.AddForce(forceDirection * zeroGravityForce);
            }
        }
    }
}
