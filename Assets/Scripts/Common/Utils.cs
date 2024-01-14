using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static bool GetAnyKeyDown(IEnumerable<KeyCode> keys)
    {
        return keys.Any(k => Input.GetKeyDown(k));
    }
}
