using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static bool GetAnyKeyDown(IEnumerable<KeyCode> keys)
    {
        return keys.Any(k => Input.GetKeyDown(k));
    }

    public static bool ApproximatelyEquals(this Vector3 self, Vector3 other, float tolerance = 0.001f)
    {
        return Vector3.Distance(self, other) <= tolerance;
    }
}
