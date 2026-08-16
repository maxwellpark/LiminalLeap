using UnityEngine;

// Closes on you while unwatched. It sits behind the player, so the only way to see it
// is the mirror, and raising the mirror is what holds it off.
public class Pursuer : Singleton<Pursuer>, IRunResettable
{
    [SerializeField] private bool active = true;
    [SerializeField] private float startDistance = 45f;
    [SerializeField] private float closeRate = 3.5f;
    [SerializeField] private float recoverRate = 6f;
    [SerializeField] private float speedRelief = 2.5f;
    [SerializeField] private float lungeWithin = 12f;
    [SerializeField] private float lungeMultiplier = 2.2f;
    [SerializeField] private float bodyHeight = 2.4f;

    [Header("Warnings")]
    [SerializeField] private float[] warnAt = { 0.3f, 0.55f, 0.75f };

    private PursuitModel.Settings settings;
    private Transform body;
    private Renderer bodyRenderer;
    private float distance;
    private int warningsGiven;
    private bool lunging;

    public float Distance => distance;
    public float Proximity => PursuitModel.Proximity(distance, settings);

    public override void Init()
    {
        settings = new PursuitModel.Settings
        {
            StartDistance = startDistance,
            CloseRate = closeRate,
            RecoverRate = recoverRate,
            MaxDistance = startDistance,
            SpeedRelief = speedRelief,
            LungeWithin = lungeWithin,
            LungeMultiplier = lungeMultiplier,
        };

        distance = startDistance;
        BuildBody();
    }

    public void ResetForNewRun()
    {
        distance = startDistance;
        warningsGiven = 0;
        lunging = false;
    }

    private void Update()
    {
        if (!active || body == null)
        {
            return;
        }

        var observed = RearView.GetInstance().IsRaised;
        distance = PursuitModel.Step(distance, Time.deltaTime, observed, PlayerTrackMovement.SpeedFraction, settings);

        Follow();
        Warn();

        if (PursuitModel.Caught(distance))
        {
            PlayerTrackMovement.Caught();
        }
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
        var back = Camera.main != null ? -Camera.main.transform.forward : Vector3.back;
        back.y = 0f;
        back = back.sqrMagnitude > 0.001f ? back.normalized : Vector3.back;

        body.position = player + back * distance + Vector3.up * (bodyHeight * 0.5f - 0.5f);
        body.rotation = Quaternion.LookRotation(-back, Vector3.up);

        // Darker and larger as it closes, so the mirror reads as a threat not a decoration.
        var t = Proximity;
        body.localScale = new Vector3(1.1f + t * 0.5f, bodyHeight, 1.1f + t * 0.5f);

        if (bodyRenderer != null)
        {
            bodyRenderer.material.color = new Color(0.02f, 0.02f, 0.03f, 1f);
        }
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
