// Pacing. A corridor at a fixed speed has no dynamics, and liminal spaces are about
// dwelling in them rather than sprinting through.
//
// Deliberately not a stop: the run keeps moving, the pace drops and the hazards clear, so
// there is a beat to look around without breaking the flow the junction work fought for.
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

    // 0 at the first calm piece, 1 at the last. Lets presentation ease in and out rather
    // than snapping, which would read as a bug rather than a breath.
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
