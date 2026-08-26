using UnityEngine;

// Everything you can see about an attack. Reads the model and never writes to it, so a
// zone can re-skin the telegraph without the rules moving.
//
// Readability is the whole job here: the lane is only ever shown behind you, in a panel a
// few hundred pixels wide, for about a second. A thin floor strip at 30m was a couple of
// degrees of arc and easy to miss entirely.
public class PursuerAttackPresenter : MonoBehaviour
{
    private const int Chevrons = 6;

    private static readonly Color Charging = new(0.45f, 0.72f, 1f);
    private static readonly Color Locked = new(1f, 0.72f, 0.32f);
    private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

    // Renderer and material held from build time. Looking them up in the tint ran eight
    // GetComponent calls a frame for the length of every telegraph.
    private class Part
    {
        public GameObject Go;
        public Transform Transform;
        public Material Material;
        public bool Visible;
    }

    private PursuerAttackModel model;
    private PursuerAttackConfig config;
    private Transform pursuer;

    private Part floor;
    private Part curtain;
    private Part beam;
    private readonly Part[] chevrons = new Part[Chevrons];

    public void Bind(PursuerAttackModel model, PursuerAttackConfig config, Transform pursuer)
    {
        this.model = model;
        this.config = config;
        this.pursuer = pursuer;

        floor = BuildPart("AttackFloor", false);
        curtain = BuildPart("AttackCurtain", true);
        beam = BuildPart("AttackBeam", false);

        for (var i = 0; i < Chevrons; i++)
        {
            chevrons[i] = BuildPart("AttackChevron" + i, true);
        }
    }

    private void Update()
    {
        if (model == null || floor == null)
        {
            return;
        }

        var firing = model.Phase == AttackPhase.Fire;
        var telegraphing = model.TargetVisible && !firing;

        Show(floor, telegraphing);
        Show(curtain, telegraphing);
        Show(beam, firing);

        for (var i = 0; i < Chevrons; i++)
        {
            Show(chevrons[i], telegraphing);
        }

        if (telegraphing)
        {
            Telegraph();
        }
        else if (firing)
        {
            Fire();
        }
    }

    // Only on change. SetActive is a native call, and nine of them a frame for a whole run
    // is a lot to pay for a thing that is off almost all of the time.
    private static void Show(Part part, bool visible)
    {
        if (part.Visible == visible)
        {
            return;
        }

        part.Visible = visible;
        part.Go.SetActive(visible);
    }

    // Deliberately stops short of the player: the lane is only readable in the mirror,
    // otherwise there would be no reason to look back.
    private void Telegraph()
    {
        var player = PlayerTrackMovement.Position;
        var forward = Forward();
        var right = Vector3.Cross(Vector3.up, forward).normalized;
        var lane = model.LaneCentre(model.TargetLane);
        var width = config.LaneHalfWidth * 2f;

        var behind = pursuer != null ? Mathf.Abs(Vector3.Dot(pursuer.position - player, forward)) : 20f;
        var length = Mathf.Max(6f, behind - 4f);
        var rotation = Quaternion.LookRotation(forward, Vector3.up);
        var origin = player - forward * 4f + right * lane;
        var centre = origin - forward * (length * 0.5f);

        // Locked is the last chance to move, so the colour changes and the pulse stops.
        var locked = model.Phase == AttackPhase.Locked;
        var colour = locked ? Locked : Charging;
        var pulse = locked ? 1f : 0.65f + 0.2f * Mathf.Sin(Time.time * 6f);

        floor.Transform.SetPositionAndRotation(centre + Vector3.up * 0.03f, rotation);
        floor.Transform.localScale = new Vector3(width, 0.06f, length);
        Tint(floor, colour, pulse, 1f);

        // The part that actually reads at a glance: a wall of light down the lane.
        curtain.Transform.SetPositionAndRotation(centre + Vector3.up * 1.7f, rotation);
        curtain.Transform.localScale = new Vector3(width, 3.4f, length);
        Tint(curtain, colour, pulse * 0.8f, 0.28f);

        // Sweep toward the player so it reads as incoming rather than just present.
        var sweep = Time.time * (locked ? 3.5f : 1.6f);
        for (var i = 0; i < Chevrons; i++)
        {
            var t = (i + 0.5f) / Chevrons;
            var bar = chevrons[i];

            bar.Transform.SetPositionAndRotation(origin - forward * (length * t) + Vector3.up * 0.5f, rotation);
            bar.Transform.localScale = new Vector3(width * 1.25f, 0.9f, 0.5f);

            var wave = Mathf.Repeat(sweep + t, 1f);
            Tint(bar, colour, 0.35f + 0.65f * wave, 0.5f * wave);
        }
    }

    private void Fire()
    {
        var player = PlayerTrackMovement.Position;
        var forward = Forward();
        var right = Vector3.Cross(Vector3.up, forward).normalized;
        var lane = model.LaneCentre(model.TargetLane);

        // Runs from behind you to well ahead, rather than centred, so it reads as passing
        // through rather than appearing on top of you.
        const float length = 60f;
        var centre = player + forward * (length * 0.25f) + right * lane + Vector3.up * 0.9f;

        beam.Transform.SetPositionAndRotation(centre, Quaternion.LookRotation(forward, Vector3.up));
        beam.Transform.localScale = new Vector3(config.LaneHalfWidth * 2f, 1.8f, length);

        // Shallow and slow on purpose. A hard strobe filling the screen is the thing that
        // made the lighting unpleasant last time.
        var shimmer = 0.8f + 0.12f * Mathf.Sin(Time.time * 9f);
        Tint(beam, new Color(1f, 0.93f, 0.82f), shimmer, 1f);
    }

    private static Vector3 Forward()
    {
        var forward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }

    private static void Tint(Part part, Color colour, float strength, float alpha)
    {
        if (part.Material == null)
        {
            return;
        }

        var lit = colour * Mathf.Clamp01(strength);
        lit.a = Mathf.Clamp01(alpha);

        part.Material.color = lit;
        part.Material.SetColor(Emission, lit * 2.4f);
    }

    private Part BuildPart(string name, bool transparent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;

        // Resolution is the model's job, so this must never be something you can collide with.
        Destroy(go.GetComponent<Collider>());

        var material = new Material(Shader.Find("Standard"));
        material.EnableKeyword("_EMISSION");
        material.SetFloat("_Glossiness", 0f);

        if (transparent)
        {
            // Standard needs all of this set by hand to blend rather than write depth.
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }

        var renderer = go.GetComponent<Renderer>();
        renderer.material = material;

        go.transform.SetParent(transform, false);
        go.SetActive(false);

        return new Part
        {
            Go = go,
            Transform = go.transform,
            Material = renderer.material,
            Visible = false,
        };
    }
}
