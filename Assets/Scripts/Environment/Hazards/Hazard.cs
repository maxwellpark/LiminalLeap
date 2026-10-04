using System.Collections.Generic;
using UnityEngine;

// Touch it and the run ends. Jumpable ones sit low enough to clear.
public class Hazard : MonoBehaviour, IRunResettable
{
    private static readonly List<Hazard> live = new();

    [SerializeField] private bool jumpable;

    private Vector3 home;
    private bool homed;

    public bool Jumpable => jumpable;

    // Kept as they enable, so the shifter can walk them without searching the scene.
    public static IReadOnlyList<Hazard> Live => live;

    private void Awake()
    {
        home = transform.localPosition;
        homed = true;
    }

    private void OnEnable()
    {
        live.Add(this);
    }

    private void OnDisable()
    {
        live.Remove(this);
    }

    // The shifter moves these, and pooling hands the piece back with them still moved.
    // Resets reach inactive objects too, and one never enabled has no home yet.
    public void ResetForNewRun()
    {
        if (homed)
        {
            transform.localPosition = home;
        }
    }
}
