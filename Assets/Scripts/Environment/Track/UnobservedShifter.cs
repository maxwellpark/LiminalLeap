using System.Collections.Generic;
using UnityEngine;

// Rearranges the track ahead only while you're looking back.
public class UnobservedShifter : Singleton<UnobservedShifter>, IRunResettable
{
    private const int ShiftSalt = 0x5417;

    [SerializeField] private float minDistanceAhead = 34f;  // never rearrange in your face
    [SerializeField] private float shiftInterval = 0.35f;
    [SerializeField] private float trackHalfWidth = 3f;
    [SerializeField] private float playerHalfWidth = 0.6f;
    [SerializeField] private float[] lanes = { -2f, 0f, 2f };
    [SerializeField] private int shiftSeed = 3;     // used when no run mode has been chosen

    private readonly List<HazardLanes.Span> row = new();
    private System.Random rng;
    private float nextShiftAt;

    public override void Init()
    {
        ResetForNewRun();
    }

    // Reseeded and retimed per run, so a daily stays the same.
    public void ResetForNewRun()
    {
        rng = new System.Random(RunMode.SeedFor(shiftSeed, ShiftSalt));
        nextShiftAt = Time.time + shiftInterval;
    }

    private void Update()
    {
        // Not paused or after a death, when the mirror can still be up.
        if (!Features.On(Feature.ShiftWhenUnobserved) || !PlayerTrackMovement.Running)
        {
            return;
        }

        var mirror = RearView.Instance;
        if (mirror == null || !mirror.IsRaised)
        {
            return;
        }

        if (Time.time < nextShiftAt)
        {
            return;
        }

        // Not mid attack, or a hazard could move into the escape lane.
        var pursuer = Pursuer.Instance;
        if (pursuer != null && pursuer.Attack != null && pursuer.Attack.InFlight)
        {
            return;
        }

        nextShiftAt = Time.time + shiftInterval;
        Shift();
    }

    private void Shift()
    {
        var player = PlayerTrackMovement.Position;
        var hazards = Hazard.Live;

        for (var i = 0; i < hazards.Count; i++)
        {
            var body = hazards[i].transform;
            if (Vector3.Distance(body.position, player) < minDistanceAhead)
            {
                continue;
            }

            TryMove(body);
        }
    }

    // Only where the row stays passable.
    private void TryMove(Transform body)
    {
        if (body.parent == null)
        {
            return;
        }

        var local = body.localPosition;
        var half = body.localScale.x * 0.5f;

        row.Clear();
        foreach (Transform sibling in body.parent)
        {
            if (sibling == body || sibling.GetComponent<Hazard>() == null)
            {
                continue;
            }

            row.Add(new HazardLanes.Span(sibling.localPosition.x, sibling.localScale.x * 0.5f));
        }

        var start = rng.Next(lanes.Length);

        for (var i = 0; i < lanes.Length; i++)
        {
            var lane = lanes[(start + i) % lanes.Length];
            if (Mathf.Approximately(lane, local.x))
            {
                continue;
            }

            var candidate = new HazardLanes.Span(lane, half);
            if (HazardLanes.Overlaps(candidate, row, 0.6f))
            {
                continue;
            }

            row.Add(candidate);
            if (!HazardLanes.HasGap(row, trackHalfWidth, playerHalfWidth))
            {
                row.RemoveAt(row.Count - 1);
                continue;
            }

            body.localPosition = new Vector3(lane, local.y, local.z);
            return;
        }
    }
}
