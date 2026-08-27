using UnityEngine;

// Props built into the piece prefab and varied per spawn, rather than instantiated at
// runtime. Pooling then handles them for free and a spawn allocates nothing.
//
// The corridor was a bare plane with no ceiling and nothing either side, which undersells
// the one thing this game is actually about.
public class TrackScenery : MonoBehaviour
{
    public const string PillarHolder = "Pillars";
    public const string LightHolder = "Lights";

    [SerializeField, Range(0f, 1f)] private float pillarChance = 0.55f;
    [SerializeField, Range(0f, 1f)] private float lightChance = 0.8f;
    [SerializeField] private float heightVariation = 2.5f;

    private Transform[] pillars;
    private Transform[] lights;
    private Vector3[] pillarRest;
    private bool captured;

    // Called by the generator on every spawn with the piece's ordinal, so the same seed
    // lays out the same corridor.
    public void Vary(int seed)
    {
        Capture();

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

            // Varying height alone reads as a different building rather than the same prop
            // moved along, and costs nothing.
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

    // Gathered from named holders rather than serialised arrays, so the prefab builder does
    // not have to wire object arrays through SerializedObject.
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
        for (var i = 0; i < pillars.Length; i++)
        {
            pillarRest[i] = pillars[i] != null ? pillars[i].localScale : Vector3.one;
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

    // Deterministic and allocation free. A System.Random per spawn would have been neither.
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
