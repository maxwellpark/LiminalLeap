using System.Collections.Generic;
using UnityEngine;

// The track ahead only rearranges while you are looking the other way. Extends the pursuer's
// observed rule to the world, which is what makes looking back cost something real.
public class UnobservedShifter : Singleton<UnobservedShifter>
{
    [SerializeField] private float minDistanceAhead = 34f;  // never rearrange in your face
    [SerializeField] private float shiftInterval = 0.35f;
    [SerializeField] private float trackHalfWidth = 3f;
    [SerializeField] private float playerHalfWidth = 0.6f;
    [SerializeField] private float[] lanes = { -2f, 0f, 2f };

    private readonly List<HazardLanes.Span> row = new();
    private float nextShiftAt;

    private void Update()
    {
        if (!Features.On(Feature.ShiftWhenUnobserved))
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

        nextShiftAt = Time.time + shiftInterval;
        Shift();
    }

    private void Shift()
    {
        var player = PlayerTrackMovement.Position;
        var hazards = FindObjectsByType<Hazard>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var hazard in hazards)
        {
            var body = hazard.transform;
            if (Vector3.Distance(body.position, player) < minDistanceAhead)
            {
                continue;
            }

            TryMove(body);
        }
    }

    // Only ever moves it somewhere the row is still passable, or an unwatched moment could
    // hand back a run you could not have survived.
    private void TryMove(Transform body)
    {
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

        var start = Random.Range(0, lanes.Length);

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
