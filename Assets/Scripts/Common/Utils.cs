using UnityEngine;

public static class Utils
{
    public static bool ApproximatelyEquals(this Vector3 self, Vector3 other, float tolerance = 0.001f)
    {
        return (self - other).sqrMagnitude <= tolerance * tolerance;
    }
}
