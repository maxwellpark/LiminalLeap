using UnityEngine;

// Touch it and the run ends. Jumpable ones sit low enough to clear.
public class Hazard : MonoBehaviour
{
    [SerializeField] private bool jumpable;

    public bool Jumpable => jumpable;
}
