using UnityEngine;

// Pure helpers for chaining track pieces end-to-end. No scene or MonoBehaviour
// dependencies, so this is the unit-testable core of the procedural generator.
public static class TrackChainer
{
    // Where the next piece's centre should sit so its start meets the previous piece's
    // end, given that end position, the run-forward direction there, and the next piece's
    // length (piece origin assumed at its centre).
    public static Vector3 NextPiecePosition(Vector3 prevEnd, Vector3 forward, float pieceLength)
    {
        return prevEnd + forward.normalized * (pieceLength * 0.5f);
    }

    // True once the player has run this far past a piece's end (safe to recycle it).
    public static bool ShouldRecycle(Vector3 pieceEnd, Vector3 forward, Vector3 playerPos, float behindDistance)
    {
        var ahead = Vector3.Dot(playerPos - pieceEnd, forward.normalized);
        return ahead > behindDistance;
    }
}
