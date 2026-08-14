using NUnit.Framework;

public class ProceduralTexturesTests
{
    private const int Size = 64;

    private static void AssertSane(float[] pixels, string what)
    {
        Assert.AreEqual(Size * Size, pixels.Length, what + " wrong length");
        foreach (var p in pixels)
        {
            Assert.IsFalse(float.IsNaN(p), what + " has NaN");
            Assert.GreaterOrEqual(p, 0f, what + " below range");
            Assert.LessOrEqual(p, 1f, what + " above range");
        }
    }

    [Test]
    public void NoiseStaysInRange()
    {
        AssertSane(ProceduralTextures.Noise(Size, 3, 4), "noise");
    }

    [Test]
    public void NoiseIsDeterministicForASeed()
    {
        CollectionAssert.AreEqual(
            ProceduralTextures.Noise(Size, 5, 4),
            ProceduralTextures.Noise(Size, 5, 4));
    }

    [Test]
    public void NoiseDiffersBetweenSeeds()
    {
        CollectionAssert.AreNotEqual(
            ProceduralTextures.Noise(Size, 5, 4),
            ProceduralTextures.Noise(Size, 6, 4));
    }

    [Test]
    public void NoiseActuallyVaries()
    {
        // a flat texture would pass a range check while looking like nothing
        var pixels = ProceduralTextures.Noise(Size, 9, 4);
        var min = 1f;
        var max = 0f;
        foreach (var p in pixels)
        {
            min = p < min ? p : min;
            max = p > max ? p : max;
        }

        Assert.Greater(max - min, 0.15f, "noise is nearly flat");
    }

    [Test]
    public void NoiseSurvivesDegenerateArguments()
    {
        Assert.DoesNotThrow(() => ProceduralTextures.Noise(0, 1, 0, 0));
        Assert.DoesNotThrow(() => ProceduralTextures.Noise(1, 1, 1, 1));
    }

    [Test]
    public void GridDrawsBothClearAndLinePixels()
    {
        var pixels = ProceduralTextures.Grid(Size, 4, 0.2f);
        var lines = 0;
        var clear = 0;
        foreach (var p in pixels)
        {
            if (p < 0.5f) lines++; else clear++;
        }

        Assert.Greater(lines, 0, "no grid lines drawn");
        Assert.Greater(clear, lines, "grid is mostly line, lineWidth is wrong");
    }

    [Test]
    public void GridWithNoWidthIsAllClear()
    {
        foreach (var p in ProceduralTextures.Grid(Size, 4, 0f))
        {
            Assert.AreEqual(1f, p, 1e-5f);
        }
    }

    [Test]
    public void MultiplyDarkensAndStaysInRange()
    {
        var a = ProceduralTextures.Noise(Size, 2, 4);
        var b = ProceduralTextures.Grid(Size, 4, 0.2f);
        var mixed = ProceduralTextures.Multiply(a, b, 1f);

        AssertSane(mixed, "multiply");
        for (var i = 0; i < mixed.Length; i++)
        {
            Assert.LessOrEqual(mixed[i], a[i] + 1e-5f, "multiply brightened a pixel");
        }
    }

    [Test]
    public void RemapLandsInsideTheRequestedBand()
    {
        foreach (var p in ProceduralTextures.Remap(ProceduralTextures.Noise(Size, 4, 4), 0.3f, 0.7f))
        {
            Assert.GreaterOrEqual(p, 0.3f - 1e-5f);
            Assert.LessOrEqual(p, 0.7f + 1e-5f);
        }
    }

    [Test]
    public void RemapHandlesAnInvertedBand()
    {
        Assert.DoesNotThrow(() => ProceduralTextures.Remap(ProceduralTextures.Noise(Size, 4, 4), 0.9f, 0.1f));
    }
}
