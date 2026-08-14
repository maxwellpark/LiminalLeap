using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Opens scenes and reports unassigned serialized references. A generated scene
// looked fine until play, because the UIManager prefab ships its text fields empty.
public static class SceneValidator
{
    // Fields that are legitimately optional, so an empty one isn't a fault.
    private static readonly HashSet<string> Optional = new()
    {
        "marker", "generator", "authored", "endAnchor", "shakeSettings", "label",
        "distanceText", "speedText", "highScoreText",
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

        // Two MainCamera-tagged cameras makes Camera.main a coin flip, and the loser
        // is usually the one that actually follows the player.
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
