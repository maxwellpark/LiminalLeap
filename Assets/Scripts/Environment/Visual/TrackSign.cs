using TMPro;
using UnityEngine;

// Floor signage. Ahead it may lie; past it, the mirror shows what it should have said.
public class TrackSign : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new(0f, 0.02f, 4f);
    [SerializeField] private float size = 3.2f;
    [SerializeField] private Color colour = new(0.62f, 0.66f, 0.72f, 0.85f);

    private TextMeshPro label;
    private TextMeshPro reflection;

    public SignKind Shown { get; private set; }
    public SignKind Truth { get; private set; }

    public void Paint(SignKind shown, SignKind truth)
    {
        Shown = shown;
        Truth = truth;

        if (label == null)
        {
            label = WorldSign.Floor(transform, SignText.Label(shown), localOffset, size, colour);
            reflection = WorldSign.Floor(transform, SignText.Label(truth), localOffset, size, colour);

            // Turned to read from behind, and pre-flipped, since the mirror panel flips it back.
            reflection.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
            reflection.transform.localScale = new Vector3(-1f, 1f, 1f);

            SetLayer(label, Presentation.ForwardOnlyLayer);
            SetLayer(reflection, Presentation.MirrorOnlyLayer);
        }
        else
        {
            label.text = SignText.Label(shown);
            reflection.text = SignText.Label(truth);
        }

        label.gameObject.SetActive(true);
        reflection.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (label != null)
        {
            label.gameObject.SetActive(false);
            reflection.gameObject.SetActive(false);
        }
    }

    private static void SetLayer(Component part, int layer)
    {
        if (layer >= 0)
        {
            part.gameObject.layer = layer;
        }
    }
}
