using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Reports unassigned refs. Generated scenes looked fine until play without this.
public static class SceneValidator
{
    // Fields that are legitimately optional, so an empty one isn't a fault.
    private static readonly HashSet<string> Optional = new()
    {
        "marker", "generator", "authored", "endAnchor", "shakeSettings", "label", "sheet",
        "distanceText", "speedText", "highScoreText", "scoreText", "multiplierText",
    };

    [MenuItem("Liminal Leap/Validate Generated Scenes")]
    public static void ValidateFromCommandLine()
    {
        var dir = "Assets/Scenes/Generated";
        if (!Directory.Exists(dir))
        {
            Debug.Log("VALIDATE: no generated scenes");
            return;
        }

        var problems = 0;
        foreach (var path in Directory.GetFiles(dir, "*.unity"))
        {
            problems += ValidateScene(path.Replace('\\', '/'));
        }

        Debug.Log(problems == 0 ? "VALIDATE OK" : "VALIDATE FOUND " + problems + " problems");
    }

    private static int ValidateScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        var problems = 0;
        var pieces = 0;

        // Two MainCamera tags makes Camera.main a coin flip.
        var mainCameras = 0;
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (cam.CompareTag("MainCamera"))
            {
                mainCameras++;
            }
        }

        if (mainCameras != 1)
        {
            Debug.LogWarning($"{path}: {mainCameras} cameras tagged MainCamera, expected exactly 1");
            problems++;
        }

        problems += CheckPickupsAreReachable(path);

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null)
                {
                    Debug.LogWarning(path + ": missing script on a GameObject");
                    problems++;
                    continue;
                }

                if (mb is TrackPiece)
                {
                    pieces++;
                }

                problems += CheckFields(path, mb);
            }
        }

        Debug.Log($"VALIDATE {Path.GetFileName(path)}: pieces={pieces} problems={problems}");
        return problems;
    }

    // Pickups sat 0.1 units out of reach and just quietly did nothing.
    private static int CheckPickupsAreReachable(string path)
    {
        var trigger = FindPlayerTrigger();
        var pieces = Object.FindObjectsByType<TrackPiece>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (trigger == null || pieces.Length == 0)
        {
            return 0;
        }

        // Measure against the track: basePos converges onto the anchors at runtime.
        var halfHeight = trigger.bounds.extents.y;
        var problems = 0;

        foreach (var pickup in Object.FindObjectsByType<SpeedTriggerable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var col = pickup.GetComponent<Collider>();
            if (col == null)
            {
                Debug.LogWarning($"{path}: {pickup.name} has no collider");
                problems++;
                continue;
            }

            var runY = NearestPieceHeight(pieces, pickup.transform.position);
            var low = runY - halfHeight;
            var high = runY + halfHeight;

            if (col.bounds.max.y < low || col.bounds.min.y > high)
            {
                Debug.LogWarning(
                    $"{path}: {pickup.name} spans y {col.bounds.min.y:F2}..{col.bounds.max.y:F2}, " +
                    $"outside the player's running reach of {low:F2}..{high:F2}");
                problems++;
            }
        }

        return problems;
    }

    private static float NearestPieceHeight(TrackPiece[] pieces, Vector3 position)
    {
        var best = pieces[0];
        var bestSqr = float.MaxValue;

        foreach (var piece in pieces)
        {
            var d = piece.transform.position - position;
            d.y = 0f;
            if (d.sqrMagnitude < bestSqr)
            {
                bestSqr = d.sqrMagnitude;
                best = piece;
            }
        }

        return best.transform.position.y;
    }

    private static Collider FindPlayerTrigger()
    {
        foreach (var movement in Object.FindObjectsByType<PlayerTrackMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            foreach (var col in movement.GetComponentsInChildren<Collider>(true))
            {
                if (col.isTrigger)
                {
                    return col;
                }
            }
        }

        return null;
    }

    private static int CheckFields(string path, MonoBehaviour mb)
    {
        var problems = 0;
        var so = new SerializedObject(mb);
        var prop = so.GetIterator();

        while (prop.NextVisible(true))
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference
                || prop.objectReferenceValue != null
                || Optional.Contains(prop.name))
            {
                continue;
            }

            Debug.LogWarning($"{path}: {mb.GetType().Name}.{prop.name} is unassigned");
            problems++;
        }

        return problems;
    }
}
