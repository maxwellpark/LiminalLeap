using NUnit.Framework;

public class VignetteTextureTests
{
    private const int Size = 64;

    private static float At(float[] a, int size, int x, int y)
    {
        return a[y * size + x];
    }

    [Test]
    public void Build_ReturnsASquareOfTheRequestedSize()
    {
        Assert.AreEqual(Size * Size, VignetteTexture.Build(Size, 0.35f, 1f).Length);
    }

    [Test]
    public void Build_CentreIsClear()
    {
        var a = VignetteTexture.Build(Size, 0.35f, 1f);
        Assert.AreEqual(0f, At(a, Size, Size / 2, Size / 2), 1e-4f);
    }

    [Test]
    public void Build_CornersAreOpaque()
    {
        var a = VignetteTexture.Build(Size, 0.35f, 1f);
        Assert.AreEqual(1f, At(a, Size, 0, 0), 1e-4f);
        Assert.AreEqual(1f, At(a, Size, Size - 1, Size - 1), 1e-4f);
    }

    [Test]
    public void Build_RisesMonotonicallyFromCentreToCorner()
    {
        var a = VignetteTexture.Build(Size, 0.2f, 1.2f);
        var prev = -1f;
        for (var i = 0; i < Size / 2; i++)
        {
            var v = At(a, Size, Size / 2 - i, Size / 2 - i);
            Assert.GreaterOrEqual(v, prev, "vignette dips at step " + i);
            prev = v;
        }
    }

    [Test]
    public void Build_StaysInRange()
    {
        foreach (var v in VignetteTexture.Build(Size, 0.35f, 1f))
        {
            Assert.IsFalse(float.IsNaN(v));
            Assert.GreaterOrEqual(v, 0f);
            Assert.LessOrEqual(v, 1f);
        }
    }

    [Test]
    public void Build_IsSymmetric()
    {
        var a = VignetteTexture.Build(Size, 0.35f, 1f);
        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                Assert.AreEqual(At(a, Size, x, y), At(a, Size, Size - 1 - x, y), 1e-5f);
            }
        }
    }

    [Test]
    public void Build_SurvivesAnInvertedRadius()
    {
        // outer <= inner would divide by zero and paint the screen black
        Assert.DoesNotThrow(() => VignetteTexture.Build(Size, 1f, 0.5f));
        foreach (var v in VignetteTexture.Build(Size, 1f, 0.5f))
        {
            Assert.IsFalse(float.IsNaN(v));
        }
    }

    [Test]
    public void Build_SurvivesATinySize()
    {
        Assert.DoesNotThrow(() => VignetteTexture.Build(0, 0.35f, 1f));
        Assert.DoesNotThrow(() => VignetteTexture.Build(1, 0.35f, 1f));
    }
}
