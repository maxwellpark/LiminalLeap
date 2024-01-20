using UnityEngine;

public class ZeroGravityZone : DistanceActivatable
{
    [SerializeField] private float strength = 10f;
    [SerializeField] private LayerMask layers;

    private void OnTriggerStay(Collider other)
    {
        if (!InRange)
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
