using UnityEngine;

public class TrackPiece : MonoBehaviour
{
    public bool Passed { get; set; }
    [SerializeField] private GameObject marker;

    private void Start()
    {
        if (marker != null)
        {
            marker.transform.position = GetEndPosition();
        }
    }

    public Vector3 GetEndPosition()
    {
        Bounds bounds = GetComponent<Renderer>().bounds;
        Vector3 endPosition = transform.position + 0.5f * bounds.size.z * transform.forward;
        endPosition.z -= 0.5f;
        return endPosition;
    }
}
