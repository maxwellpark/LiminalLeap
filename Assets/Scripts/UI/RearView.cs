using UnityEngine;
using UnityEngine.UI;

// Mirror you hold a key to raise. The camera is disabled unless it's up, so it costs
// nothing the rest of the time, unlike the always-on full-res RearViewMirror.
public class RearView : Singleton<RearView>
{
    [SerializeField] private int width = 480;
    [SerializeField] private int height = 270;

    [Header("Feel")]
    [SerializeField] private float raiseSpeed = 6f;
    [SerializeField] private float dropSpeed = 9f;
    [SerializeField] private float slideDistance = 190f;
    [SerializeField] private float restScale = 0.86f;
    [SerializeField] private float tiltDegrees = 5f;
    [SerializeField] private float overshoot = 1.7f;

    [Header("Dread")]
    [SerializeField] private float shakeAtContact = 9f;
    [SerializeField] private Color calmTint = new(0.82f, 0.86f, 0.9f);
    [SerializeField] private Color dreadTint = new(1f, 0.72f, 0.68f);

    private Camera mirrorCamera;
    private RenderTexture target;
    private RawImage panel;
    private Image frame;
    private RectTransform holder;
    private CanvasGroup group;
    private Pursuer pursuer;

    private float shown;
    private bool wasRaised;
    private float noiseSeed;

    public bool IsRaised => shown > 0.5f;
    public Camera MirrorCamera => mirrorCamera;

    public override void Init()
    {
        var main = Camera.main;
        if (main == null)
        {
            return;
        }

        target = new RenderTexture(width, height, 16) { name = "RearViewRT" };

        var go = new GameObject("RearViewCamera");
        go.transform.SetParent(main.transform, false);
        mirrorCamera = go.AddComponent<Camera>();

        // CopyFrom copies the transform too, so face it backwards afterwards or the
        // mirror shows what is in front of you.
        mirrorCamera.CopyFrom(main);
        FaceBackwards();

        mirrorCamera.targetTexture = target;
        mirrorCamera.fieldOfView = 75f;
        mirrorCamera.enabled = false;

        noiseSeed = 41.3f;
        BuildPanel();
    }

    private void OnDestroy()
    {
        // Editor domain reloads leak these otherwise.
        if (target != null)
        {
            target.Release();
        }
    }

    private void Update()
    {
        if (panel == null)
        {
            return;
        }

        var raised = InputRouter.Source.LookingBack;
        var speed = raised ? raiseSpeed : dropSpeed;
        shown = Mathf.MoveTowards(shown, raised ? 1f : 0f, speed * Time.deltaTime);

        if (raised != wasRaised)
        {
            AudioManager.GetInstance().Play(raised ? Sound.MirrorUp : Sound.MirrorDown);
            wasRaised = raised;
        }

        Present();

        mirrorCamera.enabled = shown > 0.01f;
        FaceBackwards();
    }

    private void Present()
    {
        // Back-eased so it settles rather than arriving linearly, which reads as UI.
        var t = Mathf.Clamp01(shown);
        var eased = 1f - Mathf.Pow(1f - t, 3f);
        var settle = eased + overshoot * t * (1f - t) * (1f - t);

        group.alpha = eased;

        var dread = Dread();
        var shake = dread * shakeAtContact;

        var wobbleX = (Mathf.PerlinNoise(noiseSeed, Time.time * 18f) - 0.5f) * shake;
        var wobbleY = (Mathf.PerlinNoise(noiseSeed + 7f, Time.time * 18f) - 0.5f) * shake;

        holder.anchoredPosition = new Vector2(wobbleX, -40f - slideDistance * (1f - settle) + wobbleY);
        holder.localScale = Vector3.one * Mathf.Lerp(restScale, 1f, settle);
        holder.localRotation = Quaternion.Euler(0f, 0f, tiltDegrees * (1f - settle));

        panel.color = Color.Lerp(calmTint, dreadTint, dread);

        if (frame != null)
        {
            frame.color = new Color(0.04f, 0.04f, 0.05f, Mathf.Lerp(0.85f, 1f, dread));
        }
    }

    private float Dread()
    {
        pursuer = pursuer != null ? pursuer : Pursuer.Instance;
        return pursuer != null ? pursuer.Proximity : 0f;
    }

    private void FaceBackwards()
    {
        if (mirrorCamera != null)
        {
            mirrorCamera.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void BuildPanel()
    {
        var canvas = RuntimeUi.CreateCanvas("RearViewCanvas", 80);

        var go = new GameObject("RearView");
        go.transform.SetParent(canvas.transform, false);
        group = go.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        holder = go.AddComponent<RectTransform>();
        holder.anchorMin = new Vector2(0.5f, 1f);
        holder.anchorMax = new Vector2(0.5f, 1f);
        holder.pivot = new Vector2(0.5f, 1f);
        holder.sizeDelta = new Vector2(660f, 380f);

        // Bezel behind the image so it reads as a mirror rather than a floating rectangle.
        frame = new GameObject("Frame").AddComponent<Image>();
        frame.transform.SetParent(holder, false);
        frame.color = new Color(0.04f, 0.04f, 0.05f, 0.85f);
        frame.raycastTarget = false;
        Stretch(frame.rectTransform, 0f);

        panel = new GameObject("Mirror").AddComponent<RawImage>();
        panel.transform.SetParent(holder, false);
        panel.texture = target;
        panel.color = calmTint;
        panel.raycastTarget = false;
        Stretch(panel.rectTransform, 10f);
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }
}
