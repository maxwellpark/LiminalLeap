using UnityEngine;

// Everything you can see about an attack. Reads the model and never writes to it, so a
// zone can re-skin the telegraph without the rules moving.
public class PursuerAttackPresenter : MonoBehaviour
{
    private PursuerAttackModel model;
    private PursuerAttackConfig config;
    private Transform pursuer;

    private Transform marker;
    private Renderer markerRenderer;
    private Transform beam;
    private Renderer beamRenderer;

    public void Bind(PursuerAttackModel model, PursuerAttackConfig config, Transform pursuer)
    {
        this.model = model;
        this.config = config;
        this.pursuer = pursuer;

        marker = BuildBar("AttackTelegraph");
        markerRenderer = marker.GetComponent<Renderer>();

        beam = BuildBar("AttackBeam");
        beamRenderer = beam.GetComponent<Renderer>();
    }

    private void Update()
    {
        if (model == null || marker == null)
        {
            return;
        }

        var firing = model.Phase == AttackPhase.Fire;
        var telegraphing = model.TargetVisible && !firing;

        marker.gameObject.SetActive(telegraphing);
        beam.gameObject.SetActive(firing);

        if (telegraphing)
        {
            PlaceMarker();
        }
        else if (firing)
        {
            PlaceBeam();
        }
    }

    // Deliberately stops short of the player: the lane is only readable in the mirror,
    // otherwise there would be no reason to look back.
    private void PlaceMarker()
    {
        var player = PlayerTrackMovement.Position;
        var forward = Forward();
        var right = Vector3.Cross(Vector3.up, forward).normalized;
        var lane = model.LaneCentre(model.TargetLane);

        var behind = pursuer != null ? Mathf.Abs(Vector3.Dot(pursuer.position - player, forward)) : 20f;
        var length = Mathf.Max(4f, behind - 4f);
        var centre = player - forward * (length * 0.5f + 4f) + right * lane + Vector3.up * 0.03f;

        marker.SetPositionAndRotation(centre, Quaternion.LookRotation(forward, Vector3.up));
        marker.localScale = new Vector3(config.LaneHalfWidth * 2f, 0.05f, length);

        // Locked is the last chance to move, so the pulse stops and it goes solid.
        var locked = model.Phase == AttackPhase.Locked;
        var pulse = locked ? 1f : 0.6f + 0.2f * Mathf.Sin(Time.time * 7f);
        Tint(markerRenderer, new Color(0.85f, 0.9f, 1f), pulse);
    }

    private void PlaceBeam()
    {
        var player = PlayerTrackMovement.Position;
        var forward = Forward();
        var right = Vector3.Cross(Vector3.up, forward).normalized;
        var lane = model.LaneCentre(model.TargetLane);

        // Runs from behind you to a way ahead, rather than centred, so it reads as passing
        // through rather than appearing on top of you.
        const float length = 60f;
        var centre = player + forward * (length * 0.25f) + right * lane + Vector3.up * 0.9f;

        beam.SetPositionAndRotation(centre, Quaternion.LookRotation(forward, Vector3.up));
        beam.localScale = new Vector3(config.LaneHalfWidth * 2f, 1.8f, length);

        // Shallow and slow on purpose. A hard strobe filling the screen is the thing that
        // made the lighting unpleasant last time.
        var shimmer = 0.8f + 0.12f * Mathf.Sin(Time.time * 9f);
        Tint(beamRenderer, new Color(0.88f, 0.92f, 1f), shimmer);
    }

    private static Vector3 Forward()
    {
        var forward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }

    private static void Tint(Renderer target, Color colour, float strength)
    {
        if (target == null)
        {
            return;
        }

        var lit = colour * Mathf.Clamp01(strength);
        target.material.color = lit;
        target.material.SetColor("_EmissionColor", lit * 2.2f);
    }

    private Transform BuildBar(string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;

        // Resolution is the model's job, so this must never be something you can collide with.
        Destroy(go.GetComponent<Collider>());

        var material = new Material(Shader.Find("Standard"));
        material.EnableKeyword("_EMISSION");
        material.SetFloat("_Glossiness", 0f);
        go.GetComponent<Renderer>().material = material;

        go.transform.SetParent(transform, false);
        go.SetActive(false);
        return go.transform;
    }
}
