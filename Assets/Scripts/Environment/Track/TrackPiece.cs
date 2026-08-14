using UnityEngine;

// End is an explicit child transform where the next piece attaches. The old version
// derived it from Renderer.bounds, a world-space AABB, so it only held for pieces
// aligned to +Z and made turns impossible.
public class TrackPiece : MonoBehaviour
{
    public bool Passed { get; set; }

    // Which prefab this came from, so the pool hands back the same kind of piece.
    public int PrefabIndex { get; set; } = -1;

    [SerializeField] private Transform endAnchor;
    [SerializeField] private GameObject marker;

    private Renderer cachedRenderer;
    private bool rendererLookedUp;

    public bool HasEndAnchor => endAnchor != null;

    public Vector3 GetEndPosition()
    {
        return endAnchor != null ? endAnchor.position : FallbackEnd();
    }

    // Direction the next piece runs in, so a turn chains off the anchor's own rotation.
    public Vector3 GetEndForward()
    {
        return endAnchor != null ? endAnchor.forward : transform.forward;
    }

    // Only the anchorless path needs this; anchored pieces chain socket to socket.
    public float Length()
    {
        var r = CachedRenderer();
        if (r == null)
        {
            return 10f;
        }

        // localBounds, not bounds: the world AABB grows as the piece rotates.
        return r.localBounds.size.z * transform.lossyScale.z;
    }

    private void Start()
    {
        if (marker != null)
        {
            marker.transform.position = GetEndPosition();
        }
    }

    private Vector3 FallbackEnd()
    {
        return transform.position + 0.5f * Length() * transform.forward;
    }

    private Renderer CachedRenderer()
    {
        if (!rendererLookedUp)
        {
            cachedRenderer = GetComponent<Renderer>();
            rendererLookedUp = true;
        }

        return cachedRenderer;
    }
}
