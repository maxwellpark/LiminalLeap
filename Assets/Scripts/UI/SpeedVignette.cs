using UnityEngine;
using UnityEngine.UI;

// Darkens the edges with speed. Sorts under the HUD so it never dims the text.
public class SpeedVignette : Singleton<SpeedVignette>
{
    [SerializeField] private Image sheet;
    [SerializeField] private Color colour = Color.black;
    [SerializeField, Range(0f, 1f)] private float restIntensity = 0.12f;
    [SerializeField, Range(0f, 1f)] private float fullIntensity = 0.55f;
    [SerializeField, Range(0f, 1f)] private float dreadIntensity = 0.35f;
    [SerializeField, Range(0f, 1f)] private float blindIntensity = 0.5f;
    [SerializeField, Range(0f, 1f)] private float darkIntensity = 0.35f;
    [SerializeField] private float responsiveness = 3f;
    [SerializeField] private int resolution = 256;
    [SerializeField, Range(0f, 1f)] private float clearRadius = 0.35f;
    [SerializeField, Range(0f, 2f)] private float edgeRadius = 1f;

    private float intensity;

    public override void Init()
    {
        if (sheet == null)
        {
            var canvas = RuntimeUi.CreateCanvas("VignetteCanvas", 50);
            sheet = RuntimeUi.CreateFullScreenImage(canvas.transform, "Vignette", colour);
            sheet.sprite = BuildSprite();
            sheet.type = Image.Type.Simple;
        }

        intensity = restIntensity;
        Apply();
    }

    private void Update()
    {
        if (sheet == null)
        {
            return;
        }

        var speed = Mathf.Lerp(restIntensity, fullIntensity, PlayerTrackMovement.SpeedFraction);
        var dread = Pursuer.GetInstance().Proximity * dreadIntensity;

        // Instance, not GetInstance: the vignette must never be what spawns the mirror.
        var mirror = RearView.Instance;
        var blind = mirror != null ? mirror.Blindness * blindIntensity : 0f;

        var lighting = MoodLighting.Instance;
        var dark = lighting != null ? lighting.Darkness * darkIntensity : 0f;

        var target = Mathf.Clamp01(speed + dread + blind + dark);
        intensity = Mathf.Lerp(intensity, target, responsiveness * Time.deltaTime);
        Apply();
    }

    private void Apply()
    {
        sheet.color = new Color(colour.r, colour.g, colour.b, intensity);
    }

    private Sprite BuildSprite()
    {
        var size = Mathf.Max(2, resolution);
        var alphas = VignetteTexture.Build(size, clearRadius, edgeRadius);

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        var pixels = new Color32[alphas.Length];
        for (var i = 0; i < alphas.Length; i++)
        {
            pixels[i] = new Color32(255, 255, 255, (byte)(alphas[i] * 255f));
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f));
    }
}
