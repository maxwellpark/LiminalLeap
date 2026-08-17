using NUnit.Framework;

public class SignTextTests
{
    [Test]
    public void EveryKindHasSomethingToSay()
    {
        foreach (var kind in SignText.All)
        {
            Assert.IsNotEmpty(SignText.Label(kind), kind.ToString());
        }
    }

    [Test]
    public void AHighRollTellsTheTruth()
    {
        Assert.AreEqual(SignKind.Jump, SignText.Choose(SignKind.Jump, 0.9f, 0.25f));
    }

    [Test]
    public void ALowRollLies()
    {
        Assert.AreNotEqual(SignKind.Jump, SignText.Choose(SignKind.Jump, 0.05f, 0.25f));
    }

    [Test]
    public void NoLieChanceMeansAlwaysHonest()
    {
        for (var roll = 0f; roll < 1f; roll += 0.05f)
        {
            Assert.AreEqual(SignKind.ExitAhead, SignText.Choose(SignKind.ExitAhead, roll, 0f));
        }
    }

    // A lie that lands on the truth is just a sign, so it must never happen.
    [Test]
    public void ALieIsNeverAccidentallyTheTruth()
    {
        foreach (var truth in SignText.All)
        {
            for (var roll = 0f; roll < 0.3f; roll += 0.005f)
            {
                var shown = SignText.Choose(truth, roll, 0.3f);
                Assert.AreNotEqual(truth, shown, $"{truth} at roll {roll} did not actually lie");
            }
        }
    }

    [Test]
    public void ItAlwaysReturnsARealKind()
    {
        foreach (var truth in SignText.All)
        {
            for (var roll = 0f; roll < 1f; roll += 0.01f)
            {
                var shown = SignText.Choose(truth, roll, 0.5f);
                CollectionAssert.Contains(SignText.All, shown);
            }
        }
    }

    [Test]
    public void MostSignsAreHonestAtATypicalLieChance()
    {
        var lies = 0;
        var total = 0;

        for (var roll = 0f; roll < 1f; roll += 0.001f)
        {
            total++;
            if (SignText.IsLie(SignKind.Clear, SignText.Choose(SignKind.Clear, roll, 0.22f)))
            {
                lies++;
            }
        }

        // Too many and the signs become wallpaper, which is worse than not having them.
        Assert.Less(lies / (float)total, 0.3f);
    }
}
