using NUnit.Framework;

public class HazardSpacingTests
{
    private const float PieceLength = 10f;

    [Test]
    public void GapCoversTheWholeJumpArcAtFullSpeed()
    {
        // 32 * 0.64 = 20.5 units airborne, plus 6 margin, over 10-unit pieces
        var gap = HazardLanes.RequiredPieceGap(32f, 0.64f, PieceLength, 6f);
        Assert.GreaterOrEqual(gap * PieceLength, 32f * 0.64f, "a jump would land on the next hazard");
        Assert.AreEqual(3, gap);
    }

    [Test]
    public void AdjacentRowsAreNeverAllowedAtRealisticSpeeds()
    {
        Assert.Greater(HazardLanes.RequiredPieceGap(32f, 0.64f, PieceLength, 6f), 1);
    }

    [Test]
    public void FasterPlayersNeedMoreRoom()
    {
        var slow = HazardLanes.RequiredPieceGap(12f, 0.64f, PieceLength, 6f);
        var fast = HazardLanes.RequiredPieceGap(48f, 0.64f, PieceLength, 6f);
        Assert.Greater(fast, slow);
    }

    [Test]
    public void LongerAirtimeNeedsMoreRoom()
    {
        var brief = HazardLanes.RequiredPieceGap(32f, 0.3f, PieceLength, 6f);
        var floaty = HazardLanes.RequiredPieceGap(32f, 1.2f, PieceLength, 6f);
        Assert.Greater(floaty, brief);
    }

    [Test]
    public void LongerPiecesNeedFewerOfThem()
    {
        var shortPieces = HazardLanes.RequiredPieceGap(32f, 0.64f, 5f, 6f);
        var longPieces = HazardLanes.RequiredPieceGap(32f, 0.64f, 40f, 6f);
        Assert.Greater(shortPieces, longPieces);
    }

    [Test]
    public void GapIsNeverLessThanOne()
    {
        Assert.GreaterOrEqual(HazardLanes.RequiredPieceGap(0f, 0f, PieceLength, 0f), 1);
        Assert.GreaterOrEqual(HazardLanes.RequiredPieceGap(-5f, -1f, PieceLength, -3f), 1);
    }

    [Test]
    public void ZeroPieceLengthDoesNotDivideByZero()
    {
        Assert.DoesNotThrow(() => HazardLanes.RequiredPieceGap(32f, 0.64f, 0f, 6f));
        Assert.GreaterOrEqual(HazardLanes.RequiredPieceGap(32f, 0.64f, 0f, 6f), 1);
    }
}
