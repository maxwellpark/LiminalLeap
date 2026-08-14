using UnityEngine;

// Fog, ambient and a flickering key light. Self-provisioning like the other managers,
// so generated scenes and MovementTestScene both get the mood without being edited.
public class MoodLighting : Singleton<MoodLighting>
{
    [SerializeField] private Color fogColour = new(0.08f, 0.09f, 0.11f);
    [SerializeField] private float fogDensity = 0.018f;
    [SerializeField] private Color ambient = new(0.16f, 0.16f, 0.2f);
    [SerializeField] private Color keyColour = new(0.85f, 0.82f, 0.7f);
    [SerializeField] private float keyIntensity = 0.9f;

    [Header("Flicker")]
    [SerializeField] private float flickerDepth = 0.18f;
    [SerializeField] private float flickerSpeed = 7f;
    [SerializeField] private float dropoutChance = 0.004f; // per frame, brief full dip
    [SerializeField] private float dropoutSeconds = 0.06f;

    private Light key;
    private float dropoutUntil;
    private float noiseSeed;

    public override void Init()
    {
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

        if (Time.time < dropoutUntil)
        {
            key.intensity = keyIntensity * 0.15f;
            return;
        }

        if (Random.value < dropoutChance)
        {
            dropoutUntil = Time.time + dropoutSeconds;
            return;
        }

        // Perlin rather than Random per frame: strobing reads as a bug, drift reads as dread.
        var wobble = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed) - 0.5f;
        key.intensity = keyIntensity * (1f + wobble * flickerDepth * 2f);
    }
}
