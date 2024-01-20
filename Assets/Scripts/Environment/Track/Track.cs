using UnityEngine;

public class Track : MonoBehaviour
{
    public TrackPiece[] Pieces { get; private set; }
    public bool Active { get; set; }

    private void Awake()
    {
        Pieces = GetComponentsInChildren<TrackPiece>();
    }
}
