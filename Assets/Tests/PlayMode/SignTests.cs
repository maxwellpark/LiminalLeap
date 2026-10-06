using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public class SignTests
{
    private GameObject host;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(host);
    }

    private TextMeshPro OnLayer(int layer)
    {
        foreach (var text in host.GetComponentsInChildren<TextMeshPro>(true))
        {
            if (text.gameObject.layer == layer)
            {
                return text;
            }
        }

        return null;
    }

    // Ahead it can lie. The mirror's reading is always the truth.
    [UnityTest]
    public IEnumerator TheMirrorReadsTheTruth()
    {
        Assert.GreaterOrEqual(Presentation.ForwardOnlyLayer, 0, "no ForwardOnly layer");
        Assert.GreaterOrEqual(Presentation.MirrorOnlyLayer, 0, "no MirrorOnly layer");

        host = new GameObject("Piece");
        var sign = host.AddComponent<TrackSign>();
        sign.Paint(SignKind.ExitAhead, SignKind.Jump);
        yield return null;

        Assert.AreEqual(SignText.Label(SignKind.ExitAhead), OnLayer(Presentation.ForwardOnlyLayer).text);
        Assert.AreEqual(SignText.Label(SignKind.Jump), OnLayer(Presentation.MirrorOnlyLayer).text);

        sign.Paint(SignKind.Clear, SignKind.Strafe);
        Assert.AreEqual(SignText.Label(SignKind.Clear), OnLayer(Presentation.ForwardOnlyLayer).text);
        Assert.AreEqual(SignText.Label(SignKind.Strafe), OnLayer(Presentation.MirrorOnlyLayer).text);
    }

    // The panel flips the image, so the reflection is written backwards to read right.
    [UnityTest]
    public IEnumerator TheReflectionIsMirrorWriting()
    {
        host = new GameObject("Piece");
        host.AddComponent<TrackSign>().Paint(SignKind.ExitAhead, SignKind.Jump);
        yield return null;

        var label = OnLayer(Presentation.ForwardOnlyLayer).transform;
        var reflection = OnLayer(Presentation.MirrorOnlyLayer).transform;
        Assert.Less(reflection.localScale.x, 0f, "not pre-flipped for the mirror panel");
        Assert.Less(Vector3.Dot(reflection.up, label.up), -0.9f, "not turned to read from behind");
    }
}
