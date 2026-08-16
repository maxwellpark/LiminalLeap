using UnityEngine;
using UnityEngine.UI;

// Mirror you hold a key to raise. The camera is disabled unless it's up, so it costs
// nothing the rest of the time, unlike the always-on full-res RearViewMirror.
public class RearView : Singleton<RearView>
{
    [SerializeField] private int width = 480;
    [SerializeField] private int height = 270;
    [SerializeField] private float fadeSpeed = 10f;

    private Camera mirrorCamera;
    private RenderTexture target;
    private RawImage panel;
    private CanvasGroup group;
    private float shown;

    public bool IsRaised => shown > 0.5f;

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
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        mirrorCamera = go.AddComponent<Camera>();
        mirrorCamera.CopyFrom(main);
        mirrorCamera.targetTexture = target;
        mirrorCamera.fieldOfView = 75f;
        mirrorCamera.enabled = false;

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

        var want = InputRouter.Source.LookingBack ? 1f : 0f;
        shown = Mathf.MoveTowards(shown, want, fadeSpeed * Time.deltaTime);

        group.alpha = shown;
        mirrorCamera.enabled = shown > 0.01f;
    }

    private void BuildPanel()
    {
        var canvas = RuntimeUi.CreateCanvas("RearViewCanvas", 80);

        var go = new GameObject("RearView");
        go.transform.SetParent(canvas.transform, false);
        group = go.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        panel = new GameObject("Mirror").AddComponent<RawImage>();
        panel.transform.SetParent(go.transform, false);
        panel.texture = target;
        panel.raycastTarget = false;

        // Top centre, like a rear-view mirror sits.
        var rect = panel.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(640f, 360f);
        rect.anchoredPosition = new Vector2(0f, -40f);
    }
}
