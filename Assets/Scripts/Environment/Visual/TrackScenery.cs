using UnityEngine;

// Props built into the piece and varied per spawn, so pooling covers them.
public class TrackScenery : MonoBehaviour
{
    public const string PillarHolder = "Pillars";
    public const string LightHolder = "Lights";

    [SerializeField, Range(0f, 1f)] private float pillarChance = 0.55f;
    [SerializeField, Range(0f, 1f)] private float lightChance = 0.8f;
    [SerializeField] private float heightVariation = 2.5f;

    [Header("Anomalies")]
    [SerializeField] private Color wrongLight = new(1f, 0.32f, 0.18f);
    [SerializeField] private float hangGap = 2f;       // floor to the bottom of a hanging pillar
    [SerializeField] private float lowLightHeight = 3.2f;

    // Made on first use: a property block can't be built while the type loads.
    private static MaterialPropertyBlock tint;
    private static MaterialPropertyBlock plain;

    private Transform[] pillars;
    private Transform[] lights;
    private Renderer[] lightRenderers;
    private Vector3[] pillarRest;
    private Vector3[] pillarRestPosition;
    private Vector3[] lightRestPosition;
    private bool captured;
    private bool wrong;

    // Seeded by piece ordinal, so the same seed gives the same corridor.
    public void Vary(int seed)
    {
        Capture();
        Restore();

        for (var i = 0; i < pillars.Length; i++)
        {
            var pillar = pillars[i];
            if (pillar == null)
            {
                continue;
            }

            var on = Hash01(seed, i * 7 + 1) < pillarChance;
            pillar.gameObject.SetActive(on);

            if (!on)
            {
                continue;
            }

            // Varying height makes it read as a different building.
            var rest = pillarRest[i];
            var tall = rest.y + (Hash01(seed, i * 7 + 2) - 0.5f) * heightVariation;
            pillar.localScale = new Vector3(rest.x, Mathf.Max(2f, tall), rest.z);
        }

        for (var i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
            {
                lights[i].gameObject.SetActive(Hash01(seed, i * 11 + 3) < lightChance);
            }
        }
    }

    // Against a baseline Vary never produces, or it isn't wrong, just varied.
    public void MakeWrong(AnomalyKind kind)
    {
        Capture();

        switch (kind)
        {
            case AnomalyKind.WrongLight:
                tint ??= new MaterialPropertyBlock();
                tint.SetColor("_Color", wrongLight);
                tint.SetColor("_EmissionColor", wrongLight * 1.6f);
                for (var i = 0; i < lights.Length; i++)
                {
                    lights[i].gameObject.SetActive(true);
                    if (lightRenderers[i] != null)
                    {
                        lightRenderers[i].SetPropertyBlock(tint);
                    }
                }

                break;

            case AnomalyKind.HangingPillars:
                for (var i = 0; i < pillars.Length; i++)
                {
                    var pillar = pillars[i];
                    pillar.gameObject.SetActive(true);
                    var rest = pillarRestPosition[i];
                    pillar.localPosition = new Vector3(rest.x, hangGap + pillar.localScale.y * 0.5f, rest.z);
                }

                break;

            case AnomalyKind.LowLights:
                for (var i = 0; i < lights.Length; i++)
                {
                    lights[i].gameObject.SetActive(true);
                    var rest = lightRestPosition[i];
                    lights[i].localPosition = new Vector3(rest.x, lowLightHeight, rest.z);
                }

                break;

            default:
                return;
        }

        wrong = true;
    }

    // Pooled pieces come back carrying whatever was wrong with them last time.
    private void Restore()
    {
        if (!wrong)
        {
            return;
        }

        wrong = false;
        plain ??= new MaterialPropertyBlock();

        for (var i = 0; i < pillars.Length; i++)
        {
            pillars[i].localPosition = pillarRestPosition[i];
        }

        for (var i = 0; i < lights.Length; i++)
        {
            lights[i].localPosition = lightRestPosition[i];
            if (lightRenderers[i] != null)
            {
                lightRenderers[i].SetPropertyBlock(plain);
            }
        }
    }

    // Named holders, so the prefab builder doesn't wire arrays.
    private void Capture()
    {
        if (captured)
        {
            return;
        }

        captured = true;
        pillars = Collect(transform.Find(PillarHolder));
        lights = Collect(transform.Find(LightHolder));

        pillarRest = new Vector3[pillars.Length];
        pillarRestPosition = new Vector3[pillars.Length];
        for (var i = 0; i < pillars.Length; i++)
        {
            pillarRest[i] = pillars[i].localScale;
            pillarRestPosition[i] = pillars[i].localPosition;
        }

        lightRenderers = new Renderer[lights.Length];
        lightRestPosition = new Vector3[lights.Length];
        for (var i = 0; i < lights.Length; i++)
        {
            lightRenderers[i] = lights[i].GetComponentInChildren<Renderer>(true);
            lightRestPosition[i] = lights[i].localPosition;
        }
    }

    private static Transform[] Collect(Transform holder)
    {
        if (holder == null)
        {
            return System.Array.Empty<Transform>();
        }

        var found = new Transform[holder.childCount];
        for (var i = 0; i < holder.childCount; i++)
        {
            found[i] = holder.GetChild(i);
        }

        return found;
    }

    // Deterministic and allocation free, unlike a System.Random per spawn.
    private static float Hash01(int seed, int salt)
    {
        unchecked
        {
            var h = (uint)(seed * 73856093) ^ (uint)(salt * 19349663);
            h ^= h >> 13;
            h *= 0x85ebca6b;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
