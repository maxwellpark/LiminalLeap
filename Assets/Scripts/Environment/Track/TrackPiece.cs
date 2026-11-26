using System.Collections.Generic;
using UnityEngine;

// End anchor, not Renderer.bounds, which only held for pieces facing +Z.
public class TrackPiece : MonoBehaviour
{
    public bool Passed { get; set; }

    // Set at spawn, so calm is a property of the track.
    public bool Calm { get; set; }

    // Set at spawn in a place with laps: which lap, whether its exit ends it, what's wrong here.
    public int Lap { get; set; }
    public bool EndsLap { get; set; }
    public AnomalyKind Anomaly { get; set; }
    public int Ordinal { get; set; }

    // Built by the generator the first time this piece is a fork, then toggled.
    public GameObject ForkWall { get; set; }

    // Which prefab this came from, so the pool hands back the same kind of piece.
    public int PrefabIndex { get; set; } = -1;

    // Set at spawn. The player reads it for grip and bob.
    public Floor Floor { get; private set; }

    // Lets the generator space hazard pieces apart without inspecting their children.
    [SerializeField] private bool containsHazard;

    [SerializeField] private Transform endAnchor;
    [SerializeField] private GameObject marker;

    private Renderer cachedRenderer;
    private bool rendererLookedUp;

    // Built on first use, so a piece with no place art carries nothing extra.
    private GameObject leftWall;
    private GameObject rightWall;
    private GameObject ceilingPart;
    private GameObject leftFloor;
    private GameObject rightFloor;
    private static MaterialPropertyBlock sideTiling;
    private readonly Dictionary<GameObject, GameObject> propsStood = new();
    private const uint PropChance = 35;   // percent of pieces that stand a prop
    private const float JointReach = 1.1f;    // the next piece starts the full 7 degrees round: 8.6 m x tan 7 = 1.06 m

    // The slab under the player, and the linoleum the prefab was baked with.
    private Renderer floorRenderer;
    private Material bakedFloor;
    private bool floorLookedUp;
    private IRunResettable[] resettables;

    // Cached, as GetComponentsInChildren allocates.
    private TrackScenery scenery;
    private bool sceneryLookedUp;

    // Cached like the resettables: this is read on every spawn.
    public TrackScenery Scenery
    {
        get
        {
            if (!sceneryLookedUp)
            {
                sceneryLookedUp = true;
                scenery = GetComponent<TrackScenery>();
            }

            return scenery;
        }
    }

    public IRunResettable[] Resettables =>
        resettables ??= GetComponentsInChildren<IRunResettable>(true);

    // After children are added at runtime, such as a fork's wall.
    public void ForgetResettables()
    {
        resettables = null;
    }

    // The prefab's own track material, whatever floor it's wearing, for anything built on it.
    public Material BakedFloor
    {
        get
        {
            LookUpFloor();
            return bakedFloor;
        }
    }

    public void SetFloor(Floor floor)
    {
        Floor = floor;
        LookUpFloor();

        if (floorRenderer != null)
        {
            var generated = PlaceArt.ForFloor(floor);
            floorRenderer.sharedMaterial = generated != null ? generated
                : floor == Floor.Linoleum ? bakedFloor
                : MaterialLibrary.ForFloor(floor);
        }
    }

    // A place's walls, ceiling and props, where it has art for them. Null hides that part.
    public void Dress(Material wall, Material ceiling, GameObject[] props, int seed)
    {
        // Only turns reach past the end; overlapping straight walls would lie in one plane and flicker.
        var reach = TurnDegrees > 0.5f ? JointReach : 0f;
        var length = 10f + reach;
        var middle = 5f + reach * 0.5f;

        SetPart(ref leftWall, "WallLeft", new Vector3(-8.5f, 4.75f, middle), new Vector3(0.2f, 9.5f, length), wall);
        SetPart(ref rightWall, "WallRight", new Vector3(8.5f, 4.75f, middle), new Vector3(0.2f, 9.5f, length), wall);
        SetPart(ref ceilingPart, "Ceiling", new Vector3(0f, 9.5f, middle), new Vector3(17.2f, 0.2f, length), ceiling);

        // Floor out to the walls, under the pillars, or a dark trench runs along each side.
        var under = wall != null && floorRenderer != null ? floorRenderer.sharedMaterial : null;
        SetPart(ref leftFloor, "FloorLeft", new Vector3(-6.25f, -0.25f, middle), new Vector3(4.5f, 0.5f, length), under);
        SetPart(ref rightFloor, "FloorRight", new Vector3(6.25f, -0.25f, middle), new Vector3(4.5f, 0.5f, length), under);
        if (under != null)
        {
            // The slab's tiling is for 8 m; these are 4.5 m wide, so keep a tile its size.
            sideTiling ??= new MaterialPropertyBlock();
            var st = under.mainTextureScale;
            sideTiling.SetVector("_MainTex_ST", new Vector4(st.x * 4.5f / 8f, st.y, 0f, 0f));
            leftFloor.GetComponent<Renderer>().SetPropertyBlock(sideTiling);
            rightFloor.GetComponent<Renderer>().SetPropertyBlock(sideTiling);
        }

        StandProp(props, seed);
    }

    public bool HasEndAnchor => endAnchor != null;
    public bool ContainsHazard => containsHazard;

    // Nothing to hit, collect or leave through. Used for the stretch under the signage.
    public bool IsPlain =>
        !containsHazard
        && GetComponentInChildren<SpeedTriggerable>(true) == null
        && GetComponentInChildren<ExitDoor>(true) == null;

    public float TurnDegrees =>
        endAnchor != null ? Mathf.Abs(Mathf.DeltaAngle(0f, endAnchor.localEulerAngles.y)) : 0f;

    public Vector3 GetEndPosition()
    {
        return endAnchor != null ? endAnchor.position : FallbackEnd();
    }

    // Direction the next piece runs in, so a turn chains off the anchor's own rotation.
    public Vector3 GetEndForward()
    {
        return endAnchor != null ? endAnchor.forward : transform.forward;
    }

    // Only the anchorless path needs this; anchored pieces chain socket to socket.
    public float Length()
    {
        var r = CachedRenderer();
        if (r == null)
        {
            return 10f;
        }

        // localBounds, not bounds: the world AABB grows as the piece rotates.
        return r.localBounds.size.z * transform.lossyScale.z;
    }

    private void Start()
    {
        if (marker != null)
        {
            marker.transform.position = GetEndPosition();
        }
    }

    private Vector3 FallbackEnd()
    {
        return transform.position + 0.5f * Length() * transform.forward;
    }

    // Boxes rather than quads, so it doesn't matter which way a face points.
    private void SetPart(ref GameObject part, string name, Vector3 at, Vector3 size, Material material)
    {
        if (material == null)
        {
            if (part != null)
            {
                part.SetActive(false);
            }

            return;
        }

        if (part == null)
        {
            part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(transform, false);
            part.transform.localPosition = at;
            part.transform.localScale = size;
            ForgetResettables();
        }

        part.GetComponent<Renderer>().sharedMaterial = material;
        part.SetActive(true);
    }

    // Chosen from the piece's seed, so a replay stands the same prop.
    private void StandProp(GameObject[] props, int seed)
    {
        foreach (var stood in propsStood.Values)
        {
            stood.SetActive(false);
        }

        if (props == null || props.Length == 0)
        {
            return;
        }

        var roll = Hash(seed);
        if (roll % 100 >= PropChance)
        {
            return;
        }

        var prefab = props[(int)(roll / 100 % (uint)props.Length)];
        if (prefab == null)
        {
            return;
        }

        if (!propsStood.TryGetValue(prefab, out var prop))
        {
            prop = Instantiate(prefab, transform, false);
            foreach (var collider in prop.GetComponentsInChildren<Collider>(true))
            {
                DestroyImmediate(collider);
            }

            propsStood[prefab] = prop;
            ForgetResettables();
        }

        var left = (roll / 7919 & 1) == 0;
        prop.transform.localPosition = new Vector3(left ? -5.2f : 5.2f, 0f, 5f);
        prop.transform.localRotation = Quaternion.Euler(0f, left ? 90f : -90f, 0f);
        prop.SetActive(true);
    }


    private static uint Hash(int seed)
    {
        unchecked
        {
            var h = (uint)(seed * 73856093) ^ 0x9e3779b9u;
            h ^= h >> 13;
            h *= 0x85ebca6b;
            h ^= h >> 16;
            return h;
        }
    }

    // Before the first swap, so the baked material is the one kept.
    private void LookUpFloor()
    {
        if (floorLookedUp)
        {
            return;
        }

        floorLookedUp = true;
        var visual = transform.Find("Visual");
        floorRenderer = visual != null ? visual.GetComponent<Renderer>() : null;
        bakedFloor = floorRenderer != null ? floorRenderer.sharedMaterial : null;
    }

    private Renderer CachedRenderer()
    {
        if (!rendererLookedUp)
        {
            cachedRenderer = GetComponent<Renderer>();
            rendererLookedUp = true;
        }

        return cachedRenderer;
    }
}
