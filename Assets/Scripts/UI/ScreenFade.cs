using UnityEngine;
using UnityEngine.UI;

// Unscaled time, so the wipe still runs while the death sequence slows the game.
public class ScreenFade : Singleton<ScreenFade>
{
    [SerializeField] private Image sheet;
    [SerializeField] private Color colour = Color.black;

    private float alpha;
    private float target;
    private float speed = 4f;

    public override void Init()
    {
        if (sheet == null)
        {
            var canvas = RuntimeUi.CreateCanvas("FadeCanvas", 200);
            sheet = RuntimeUi.CreateFullScreenImage(canvas.transform, "Fade", colour);
        }

        alpha = 0f;
        target = 0f;
        Apply();
    }

    public void To(float value, float overSeconds)
    {
        target = Mathf.Clamp01(value);
        speed = overSeconds <= 0f ? 1000f : 1f / overSeconds;
    }

    private void Update()
    {
        if (sheet == null || Mathf.Approximately(alpha, target))
        {
            return;
        }

        alpha = Mathf.MoveTowards(alpha, target, speed * Time.unscaledDeltaTime);
        Apply();
    }

    private void Apply()
    {
        if (sheet != null)
        {
            sheet.color = new Color(colour.r, colour.g, colour.b, alpha);
        }
    }
}
