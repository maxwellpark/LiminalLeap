using UnityEngine;

// Kept free of scene and MonoBehaviour deps so it's unit-testable.
public static class TrackChainer
{
    // Piece origin is assumed to be its centre.
    public static Vector3 NextPiecePosition(Vector3 prevEnd, Vector3 forward, float pieceLength)
    {
        return prevEnd + forward.normalized * (pieceLength * 0.5f);
    }

    public static bool ShouldRecycle(Vector3 pieceEnd, Vector3 forward, Vector3 playerPos, float behindDistance)
    {
        var ahead = Vector3.Dot(playerPos - pieceEnd, forward.normalized);
        return ahead > behindDistance;
    }
}
