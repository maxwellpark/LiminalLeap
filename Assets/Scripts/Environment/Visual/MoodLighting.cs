using UnityEngine;

// Fog, ambient and a flickering key light. Self-provisioning like the other managers.
public class MoodLighting : Singleton<MoodLighting>, IRunResettable
{
    [SerializeField] private Color fogColour = new(0.08f, 0.09f, 0.11f);
    [SerializeField] private float fogDensity = 0.018f;
    [SerializeField] private Color ambient = new(0.16f, 0.16f, 0.2f);
    [SerializeField] private Color keyColour = new(0.85f, 0.82f, 0.7f);
    [SerializeField] private float keyIntensity = 0.9f;

    [Header("Flicker")]
    [SerializeField] private bool reducedFlashing;          // accessibility: drift only, no dips
    [SerializeField] private float flickerDepth = 0.12f;
    [SerializeField] private float flickerSpeed = 3f;
    [SerializeField] private float responsiveness = 9f;     // how fast intensity chases its target

    [Header("Light as a resource")]
    [SerializeField] private LightFront.Settings lightFront = new();
    [SerializeField, Range(0f, 1f)] private float darkFloor = 0.12f;   // key light left in the dark
    [SerializeField] private float darkFog = 2.6f;                     // fog multiplier when fully dark

    [Header("Calm")]
    [SerializeField] private float calmLift = 1.5f;      // brighter through a breath
    [SerializeField] private float calmSettle = 1.2f;    // how fast it eases either way

    [Header("Dropouts")]
    [SerializeField] private float dropoutsPerSecond = 0.08f; // roughly one every 12s
    [SerializeField] private float dropoutSeconds = 0.12f;
    [SerializeField, Range(0f, 1f)] private float dropoutFloor = 0.45f;
    [SerializeField] private float dropoutCooldown = 4f;      // never two in quick succession

    private Light key;
    private float front;
    private float calm;
    private float dropoutRemaining;
    private float nextDropoutAllowed;
    private float noiseSeed;

    // Read by the vignette and the pursuer. The dark is meant to be felt in more than one
    // place, or it is just a dimmer switch.
    public float Darkness { get; private set; }

    public void ResetForNewRun()
    {
        front = LightFront.Start(lightFront);
        Darkness = 0f;
    }

    public override void Init()
    {
        ResetForNewRun();

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColour;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;

        key = FindFirstObjectByType<Light>();
        if (key == null)
        {
            var go = new GameObject("KeyLight");
            key = go.AddComponent<Light>();
        }

        key.type = LightType.Directional;
        key.color = keyColour;
        key.intensity = keyIntensity;
        key.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

        noiseSeed = 13.7f;

        // Camera has to clear to the fog colour or the horizon reads as a hard edge.
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = fogColour;
        }
    }

    private void Update()
    {
        if (key == null)
        {
            return;
        }

        var dt = Time.deltaTime;

        // Perlin rather than Random per frame: strobing reads as a bug, drift reads as dread.
        // Eased rather than switched, or the stretch would announce itself with a snap.
        var wanted = PlayerTrackMovement.InCalm ? 1f : 0f;
        calm = Mathf.MoveTowards(calm, wanted, calmSettle * dt);

        var wobble = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed) - 0.5f;

        // Steadier through a breath as well as brighter: the flicker is the dread, and the
        // point of the stretch is that there is nothing to dread for a moment.
        var depth = (reducedFlashing ? flickerDepth * 0.4f : flickerDepth) * Mathf.Lerp(1f, 0.25f, calm);
        var target = keyIntensity
            * Mathf.Lerp(1f, calmLift, calm)
            * Mathf.Lerp(1f, darkFloor, Darkness)
            * (1f + wobble * depth * 2f);

        // No dropouts mid breath, for the same reason.
        if (!reducedFlashing && calm < 0.5f)
        {
            target *= DropoutMultiplier(dt);
        }

        // Chased, not assigned: snapping to the floor was the strobe that felt epileptic.
        key.intensity = Mathf.Lerp(key.intensity, target, responsiveness * dt);
    }

    private void StepLight(float dt)
    {
        if (!Features.On(Feature.LightAsResource))
        {
            Darkness = 0f;
            RenderSettings.fogDensity = fogDensity;
            return;
        }

        var travelled = PlayerTrackMovement.DistanceCovered;

        if (front <= 0f)
        {
            front = LightFront.Start(lightFront);
        }

        front = LightFront.Advance(front, dt, travelled, lightFront);
        Darkness = LightFront.Darkness(travelled, front, lightFront);

        // Fog closes in as well as the light going: losing the corridor is the point, not
        // just losing brightness.
        RenderSettings.fogDensity = Mathf.Lerp(fogDensity, fogDensity * darkFog, Darkness);
    }

    private float DropoutMultiplier(float dt)
    {
        if (dropoutRemaining > 0f)
        {
            dropoutRemaining -= dt;
            return dropoutFloor;
        }

        if (Time.time < nextDropoutAllowed)
        {
            return 1f;
        }

        // Per second, not per frame, or the flicker rate depends on the framerate.
        if (Random.value < dropoutsPerSecond * dt)
        {
            dropoutRemaining = dropoutSeconds;
            nextDropoutAllowed = Time.time + dropoutCooldown;
            return dropoutFloor;
        }

        return 1f;
    }
}
