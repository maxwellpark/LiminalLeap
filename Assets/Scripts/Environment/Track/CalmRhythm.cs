// Pacing: the run keeps moving, but the pace drops and the hazards clear for a while.
public static class CalmRhythm
{
    public static bool IsCalm(int piece, int runLength, int calmLength)
    {
        if (piece < 0 || runLength <= 0 || calmLength <= 0)
        {
            return false;
        }

        return piece % (runLength + calmLength) >= runLength;
    }

    // 0 at the first calm piece, 1 at the last, so presentation can ease.
    public static float Progress(int piece, int runLength, int calmLength)
    {
        if (!IsCalm(piece, runLength, calmLength))
        {
            return 0f;
        }

        var into = piece % (runLength + calmLength) - runLength;
        return calmLength <= 1 ? 1f : into / (float)(calmLength - 1);
    }

    // How far until the next breath, for anything that wants to telegraph it.
    public static int PiecesUntilCalm(int piece, int runLength, int calmLength)
    {
        if (piece < 0 || runLength <= 0 || calmLength <= 0)
        {
            return int.MaxValue;
        }

        if (IsCalm(piece, runLength, calmLength))
        {
            return 0;
        }

        return runLength - piece % (runLength + calmLength);
    }
}
