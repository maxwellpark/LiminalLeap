using System.Collections.Generic;
using Events;
using UnityEngine;

// Closes on you while you ignore it. Under PursuerAttacks the mirror stops holding it off
// and starts being the only place the next attack is readable.
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

    [Header("Attacks")]
    [SerializeField] private PursuerAttackConfig attack = new();
    [SerializeField] private int attackSeed = 7;
    [SerializeField] private float lookahead = 30f;     // how far forward a lane must be clear
    [SerializeField] private float playerHalfWidth = 0.6f;

    private readonly Collider[] overlaps = new Collider[32];
    private readonly List<HazardLanes.Span> blocked = new();

    private PursuitModel.Settings settings;
    private PursuerAttackModel attackModel;
    private PursuerAttackPresenter presenter;
    private GhostTrace ghost;
    private Transform body;
    private Renderer bodyRenderer;
    private float distance;
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
            CloseRate = closeRate,
            RecoverRate = attacks ? 0f : recoverRate,
            MaxDistance = startDistance,
            SpeedRelief = summons ? 0f : speedRelief,
            SpeedDraw = summons ? speedDraw : 0f,
            LungeWithin = lungeWithin,
            LungeMultiplier = lungeMultiplier,
        };

        attackModel = new PursuerAttackModel(attack, attackSeed);
        ghost = SaveStore.Data.Ghost;

        distance = startDistance;
        BuildBody();

        presenter = gameObject.AddComponent<PursuerAttackPresenter>();
        presenter.Bind(attackModel, attack, body);
    }

    public void ResetForNewRun()
    {
        distance = startDistance;
        runTime = 0f;
        warningsGiven = 0;
        lunging = false;
        allowed = PursuerAttackModel.AllLanes;
        attackModel?.Reset();

        // The last run just became the ghost, so pick it up rather than replaying the old one.
        ghost = SaveStore.Data.Ghost;
    }

    private void Update()
    {
        if (!active || body == null)
        {
            return;
        }

        var dt = Time.deltaTime;
        runTime += dt;

        StepDistance(dt);

        if (Features.On(Feature.PursuerAttacks))
        {
            allowed = ScanLanes();

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
            // Distance becomes how far ahead of your last self you are, so beating your
            // old pace is literally what holds it off.
            var lead = PlayerTrackMovement.DistanceCovered - ghost.DistanceAt(runTime);
            distance = Mathf.Clamp(startDistance + lead, 0f, startDistance);
            return;
        }

        // Under attacks the mirror is never consulted, so holding it can't buy anything.
        var observed = !Features.On(Feature.PursuerAttacks) && RearView.GetInstance().IsRaised;
        distance = PursuitModel.Step(distance, dt, observed, PlayerTrackMovement.SpeedFraction, settings);
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
            distance = Mathf.Min(settings.MaxDistance, distance + attack.PursuerSetbackOnDodge);
            GameManager.EventService.Dispatch(new OnAttackDodgedEvent(attackModel.TargetLane, lane));
        }

        if (result.Hit)
        {
            LastAttackResult = "hit " + attackModel.TargetLane;
            GameManager.EventService.Dispatch(new OnAttackHitEvent(attackModel.TargetLane));
            PlayerTrackMovement.Caught();
        }
    }

    // Has to be current rather than cached: the whole fairness rule is not firing into a
    // lane the track has already closed.
    private int ScanLanes()
    {
        blocked.Clear();

        var origin = PlayerTrackMovement.Position;
        var forward = Forward();
        var right = Vector3.Cross(Vector3.up, forward).normalized;
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        var centre = origin + forward * (lookahead * 0.5f);
        var extents = new Vector3(6f, 2.5f, lookahead * 0.5f);

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
            var lateral = Vector3.Dot(bounds.center - origin, right);

            // Extent of an AABB along an arbitrary axis, so a turned piece still measures right.
            var half = Mathf.Abs(bounds.extents.x * right.x)
                + Mathf.Abs(bounds.extents.y * right.y)
                + Mathf.Abs(bounds.extents.z * right.z);

            blocked.Add(new HazardLanes.Span(lateral, half));
        }

        return PursuerSafety.AllowedLanes(blocked, attack.LaneSpacing, playerHalfWidth);
    }

    // Only on the way in. Announcing every recovery would make it chatty rather than tense.
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

        // Darker and larger as it closes, so the mirror reads as a threat not a decoration.
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
