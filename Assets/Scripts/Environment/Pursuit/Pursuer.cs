using System.Collections.Generic;
using Events;
using UnityEngine;

// Closes on you while you ignore it. With attacks on, the mirror is where you read them.
public class Pursuer : Singleton<Pursuer>, IRunResettable
{
    [SerializeField] private bool active = true;
    [SerializeField] private float startDistance = 45f;
    [SerializeField] private float closeRate = 3.5f;
    [SerializeField] private float recoverRate = 6f;
    [SerializeField] private float speedRelief = 2.5f;
    [SerializeField] private float speedDraw = 3f;      // replaces relief under SpeedSummons
    [SerializeField] private float lungeWithin = 12f;
    [SerializeField] private float lungeMultiplier = 2.2f;
    [SerializeField] private float bodyHeight = 2.4f;

    [Header("Warnings")]
    [SerializeField] private float[] warnAt = { 0.3f, 0.55f, 0.75f };

    [SerializeField] private float darkCloseBoost = 1.4f;  // how much bolder it is unlit

    [Header("Attacks")]
    [SerializeField] private PursuerAttackConfig attack = new();
    [SerializeField] private int attackSeed = 7;        // used when no run mode has been chosen
    [SerializeField] private float resolveMargin = 15f; // track checked either side of where the beam lands
    [SerializeField] private float playerHalfWidth = 0.6f;

    private const int AttackSalt = 0x5A17;

    private readonly Collider[] overlaps = new Collider[32];
    private readonly List<HazardLanes.Span> blocked = new();

    private PursuitModel.Settings settings;
    private PursuerAttackModel attackModel;
    private PursuerAttackPresenter presenter;
    private GhostTrace ghost;
    private Transform body;
    private Renderer bodyRenderer;
    private float distance;
    private float dodgeBonus;
    private float runTime;
    private int warningsGiven;
    private bool lunging;
    private int allowed = PursuerAttackModel.AllLanes;

    public float Distance => distance;
    public float Proximity => PursuitModel.Proximity(distance, settings);

    // The mirror and the debug overlay read these. Neither is allowed to write them.
    public PursuerAttackModel Attack => attackModel;
    public PursuerAttackConfig AttackConfig => attack;
    public int AllowedLanes => allowed;

    // Dev only, driven by the F keys in DebugOverlay.
    public bool AttackFrozen { get; set; }
    public string LastAttackResult { get; private set; } = "none";

    public void ForceAttack(AttackLane lane)
    {
        attackModel?.ForceAttack(lane);
    }

    public override void Init()
    {
        var attacks = Features.On(Feature.PursuerAttacks);
        var summons = Features.On(Feature.SpeedSummons);

        settings = new PursuitModel.Settings
        {
            StartDistance = startDistance,
            CloseRate = attacks ? attack.CloseRateDuringAttacks : closeRate,
            RecoverRate = attacks ? 0f : recoverRate,
            IgnoreObservation = attacks,
            MaxDistance = startDistance,
            SpeedRelief = summons ? 0f : attacks ? attack.SpeedReliefDuringAttacks : speedRelief,
            SpeedDraw = summons ? speedDraw : 0f,
            LungeWithin = lungeWithin,
            LungeMultiplier = lungeMultiplier,
        };

        attackModel = new PursuerAttackModel(attack, RunMode.SeedFor(attackSeed, AttackSalt));
        ghost = SaveStore.Data.Ghost;

        distance = startDistance;
        BuildBody();

        presenter = gameObject.AddComponent<PursuerAttackPresenter>();
        presenter.Bind(attackModel, attack, body);
    }

    public void ResetForNewRun()
    {
        distance = startDistance;
        dodgeBonus = 0f;
        runTime = 0f;
        warningsGiven = 0;
        lunging = false;
        allowed = PursuerAttackModel.AllLanes;
        attackModel?.Reset(RunMode.SeedFor(attackSeed, AttackSalt));

        // The last run just became the ghost, so pick it up rather than replaying the old one.
        ghost = SaveStore.Data.Ghost;
    }

    private void Update()
    {
        // Frozen behind the summary.
        if (!active || body == null || !PlayerTrackMovement.Running)
        {
            return;
        }

        var dt = Time.deltaTime;
        runTime += dt;

        // Holds station in a calm stretch, but an attack in flight still resolves.
        var calm = PlayerTrackMovement.InCalm;

        if (!calm)
        {
            StepDistance(dt);
        }

        if (Features.On(Feature.PursuerAttacks))
        {
            // An empty mask is how the model already postpones.
            if (calm)
            {
                allowed = 0;
            }
            else if (attackModel.Phase is AttackPhase.Idle or AttackPhase.Warning)
            {
                // Only matters before the lane locks.
                allowed = ScanLanes();
            }

            if (!AttackFrozen)
            {
                TickAttack(dt);
            }
        }

        Follow();
        Warn();

        if (PursuitModel.Caught(distance))
        {
            PlayerTrackMovement.Caught();
        }
    }

    private void StepDistance(float dt)
    {
        if (Features.On(Feature.GhostPursuer) && ghost != null && ghost.HasData)
        {
            // Distance is your lead over your last self.
            var lead = PlayerTrackMovement.DistanceCovered - ghost.DistanceAt(runTime);

            // Recomputed every frame, so a dodge bonus has to be added here.
            distance = Mathf.Clamp(startDistance + lead + dodgeBonus, 0f, startDistance);
            return;
        }

        // IgnoreObservation decides whether this counts.
        var observed = RearView.GetInstance().IsRaised;

        // The dark costs more than visibility.
        var step = settings;
        var lighting = MoodLighting.Instance;

        if (lighting != null && lighting.Darkness > 0f)
        {
            step.CloseRate *= 1f + lighting.Darkness * Mathf.Max(0f, darkCloseBoost);
        }

        distance = PursuitModel.Step(distance, dt, observed, PlayerTrackMovement.SpeedFraction, step);

        if (dodgeBonus > 0f)
        {
            distance = Mathf.Min(settings.MaxDistance, distance + dodgeBonus);
            dodgeBonus = 0f;
        }
    }

    private void TickAttack(float dt)
    {
        var lane = PlayerTrackMovement.Lane;
        var result = attackModel.Tick(dt, lane, allowed);
        var audio = AudioManager.GetInstance();

        if (result.Started)
        {
            audio.Play(Sound.AttackWarning);
        }

        if (result.TelegraphReady)
        {
            audio.Play(Sound.AttackCharge);
        }

        if (result.Locked)
        {
            audio.Play(Sound.AttackImminent);
        }

        if (result.Fired)
        {
            audio.Play(Sound.AttackFire);
        }

        if (result.Aborted)
        {
            LastAttackResult = "aborted";
        }

        if (result.Dodged)
        {
            LastAttackResult = "dodged " + attackModel.TargetLane;
            audio.Play(Sound.AttackDodge);
            dodgeBonus += attack.PursuerSetbackOnDodge;
            GameManager.EventService.Dispatch(new OnAttackDodgedEvent(attackModel.TargetLane, lane));
        }

        if (result.Hit)
        {
            LastAttackResult = "hit " + attackModel.TargetLane;
            GameManager.EventService.Dispatch(new OnAttackHitEvent(attackModel.TargetLane));
            PlayerTrackMovement.Caught();
        }
    }

    // Current, not cached, so it never fires into a closed lane.
    // Measured from the centreline, as Threatens is.
    private int ScanLanes()
    {
        blocked.Clear();

        PursuerSafety.ResolveWindow(
            PlayerTrackMovement.CurrentSpeed, SecondsUntilResolve(), resolveMargin, out var near, out var far);

        var cursor = PlayerTrackMovement.TrackCentre;
        var heading = Forward();
        var along = 0f;

        var tracks = TrackManager.Instance;
        var pieces = tracks != null ? tracks.Pieces : null;

        if (pieces != null)
        {
            for (var i = 0; i < pieces.Count && along < far; i++)
            {
                var piece = pieces[i];
                if (piece == null || piece.Passed)
                {
                    continue;
                }

                var end = piece.GetEndPosition();
                var length = Vector3.Distance(cursor, end);

                if (length > 0.001f)
                {
                    ScanStretch(cursor, (end - cursor) / length, along, along + length, near, far);
                }

                cursor = end;
                heading = piece.GetEndForward();
                along += length;
            }
        }

        // Beyond spawned track, carry straight on.
        if (along < far)
        {
            ScanStretch(cursor, heading, along, far, near, far);
        }

        return PursuerSafety.AllowedLanes(blocked, attack.LaneSpacing, playerHalfWidth);
    }

    // Lanes are measured across each stretch, so they hold past turns.
    private void ScanStretch(Vector3 start, Vector3 direction, float from, float to, float near, float far)
    {
        var a = Mathf.Max(from, near);
        var b = Mathf.Min(to, far);

        if (b <= a)
        {
            return;
        }

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction.Normalize();

        var right = Vector3.Cross(Vector3.up, direction);
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        var centre = start + direction * ((a + b) * 0.5f - from);
        var extents = new Vector3(6f, 2.5f, (b - a) * 0.5f);

        var count = Physics.OverlapBoxNonAlloc(
            centre, extents, overlaps, rotation, ~0, QueryTriggerInteraction.Collide);

        for (var i = 0; i < count; i++)
        {
            var collider = overlaps[i];
            if (collider == null || collider.GetComponent<Hazard>() == null)
            {
                continue;
            }

            var bounds = collider.bounds;
            var lateral = Vector3.Dot(bounds.center - start, right);

            // Extent of an AABB along an arbitrary axis, so a turned piece still measures right.
            var half = Mathf.Abs(bounds.extents.x * right.x)
                + Mathf.Abs(bounds.extents.y * right.y)
                + Mathf.Abs(bounds.extents.z * right.z);

            blocked.Add(new HazardLanes.Span(lateral, half));
        }
    }

    // Before the warning, the whole lead is still to come.
    private float SecondsUntilResolve()
    {
        return attackModel != null && attackModel.Phase != AttackPhase.Idle
            ? attackModel.TimeUntilFire + attack.FireDuration
            : attack.LeadTime;
    }

    // Only on the way in, or it gets chatty.
    private void Warn()
    {
        var near = Proximity;

        if (warnAt != null && warningsGiven < warnAt.Length && near >= warnAt[warningsGiven])
        {
            warningsGiven++;
            AudioManager.GetInstance().Play(Sound.Approach);
        }

        var inLunge = distance < lungeWithin;
        if (inLunge && !lunging)
        {
            AudioManager.GetInstance().Play(Sound.Lunge);
        }

        lunging = inLunge;
    }

    private void Follow()
    {
        var player = PlayerTrackMovement.Position;
        var forward = Forward();
        var back = -forward;
        var right = Vector3.Cross(Vector3.up, forward).normalized;

        // Leaning into the lane it is about to take is the telegraph, so it has to move.
        var bias = 0f;
        if (attackModel != null && Features.On(Feature.PursuerAttacks) && attackModel.TargetVisible)
        {
            bias = attackModel.LaneCentre(attackModel.TargetLane);
        }

        body.position = player + back * distance + right * bias + Vector3.up * (bodyHeight * 0.5f - 0.5f);
        body.rotation = Quaternion.LookRotation(-back, Vector3.up);

        // Darker and larger as it closes.
        var t = Proximity;
        body.localScale = new Vector3(1.1f + t * 0.5f, bodyHeight, 1.1f + t * 0.5f);
    }

    private static Vector3 Forward()
    {
        var forward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }

    private void BuildBody()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Pursuer";
        Destroy(go.GetComponent<Collider>());

        bodyRenderer = go.GetComponent<Renderer>();
        bodyRenderer.material = new Material(Shader.Find("Standard")) { color = new Color(0.02f, 0.02f, 0.03f) };
        bodyRenderer.material.SetFloat("_Glossiness", 0f);

        body = go.transform;
        body.SetParent(transform, false);
    }
}
