using TMPro;
using UnityEngine;

// Floor signage announcing what is coming. The generator tells it the truth and it decides
// whether to pass it on.
public class TrackSign : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new(0f, 0.02f, 4f);
    [SerializeField] private float size = 3.2f;
    [SerializeField] private Color colour = new(0.62f, 0.66f, 0.72f, 0.85f);

    private TextMeshPro label;

    public SignKind Shown { get; private set; }

    public void Paint(SignKind shown)
    {
        Shown = shown;

        if (label == null)
        {
            label = WorldSign.Floor(transform, SignText.Label(shown), localOffset, size, colour);
        }
        else
        {
            label.text = SignText.Label(shown);
        }

        label.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (label != null)
        {
            label.gameObject.SetActive(false);
        }
    }
}
