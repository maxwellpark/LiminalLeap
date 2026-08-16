using UnityEngine;

// Scale punch on a RectTransform. No allocation and it idles out entirely once settled.
public class UiPop : MonoBehaviour
{
    [SerializeField] private float punchScale = 1.22f;
    [SerializeField] private float recovery = 9f;

    private RectTransform rect;
    private float scale = 1f;
    private bool settled = true;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Punch()
    {
        scale = punchScale;
        settled = false;
        Apply();
    }

    private void Update()
    {
        if (settled)
        {
            return;
        }

        scale = Mathf.Lerp(scale, 1f, recovery * Time.unscaledDeltaTime);

        if (Mathf.Abs(scale - 1f) < 0.002f)
        {
            scale = 1f;
            settled = true;
        }

        Apply();
    }

    private void Apply()
    {
        if (rect != null)
        {
            rect.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
