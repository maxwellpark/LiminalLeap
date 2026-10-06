using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Renders a generated scene to a PNG in batchmode. Browser screenshots come back
// black, as runInBackground is off.
public static class SceneShot
{
    private const string OutputDir = "Build/Shots";

    [MenuItem("Liminal Leap/Capture Scene Shots")]
    public static void CaptureFromMenu()
    {
        Capture("Assets/Scenes/Generated", 1600, 900);
    }

    // -executeMethod entry. Args: -scenes <dir> -width N -height N
    public static void CaptureFromCommandLine()
    {
        var args = System.Environment.GetCommandLineArgs();
        Capture(ArgValue(args, "-scenes", "Assets/Scenes/Generated"),
            ArgInt(args, "-width", 1600), ArgInt(args, "-height", 900));
    }

    private static void Capture(string sceneDir, int width, int height)
    {
        Directory.CreateDirectory(OutputDir);

        if (!Directory.Exists(sceneDir))
        {
            Debug.LogError("SHOTS FAILED: no scenes at " + sceneDir);
            return;
        }

        var taken = 0;

        foreach (var path in Directory.GetFiles(sceneDir, "*.unity"))
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Drives the real components rather than copying them, so stills match the game.
            Populate();

            var camera = FindCamera();

            if (camera == null)
            {
                Debug.LogWarning("SHOT SKIPPED " + scene.name + ": no camera");
                continue;
            }

            // Back off the eye line, as the first person camera starts inside the geometry.
            camera.transform.position += Vector3.up * 1.6f - camera.transform.forward * 4f;

            // The forward view, so a sign's mirror reading doesn't print over it.
            if (Presentation.MirrorOnlyLayer >= 0)
            {
                camera.cullingMask &= ~(1 << Presentation.MirrorOnlyLayer);
            }

            var file = Path.Combine(OutputDir, scene.name + ".png");
            Render(camera, width, height, file);
            taken++;

            Debug.Log("SHOT " + file);
        }

        Debug.Log($"SHOTS OK {taken} written to {OutputDir}");
    }

    private static void Populate()
    {
        var generator = Object.FindFirstObjectByType<ProceduralTrackGenerator>();
        if (generator != null)
        {
            generator.ResetRun();
        }

        // Init is what the game calls, so fog, ambient and the key light match play mode.
        var lighting = Object.FindFirstObjectByType<MoodLighting>();
        if (lighting == null)
        {
            lighting = new GameObject("ShotLighting").AddComponent<MoodLighting>();
        }

        lighting.Init();
    }

    // Renders through a RenderTexture rather than ScreenCapture, which needs a real screen.
    private static void Render(Camera camera, int width, int height, string file)
    {
        var target = new RenderTexture(width, height, 24) { antiAliasing = 2 };
        var previous = camera.targetTexture;
        var active = RenderTexture.active;

        camera.targetTexture = target;
        camera.Render();

        RenderTexture.active = target;
        var image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        image.Apply();

        File.WriteAllBytes(file, image.EncodeToPNG());

        camera.targetTexture = previous;
        RenderTexture.active = active;

        Object.DestroyImmediate(image);
        target.Release();
        Object.DestroyImmediate(target);
    }

    private static Camera FindCamera()
    {
        var main = Camera.main;
        if (main != null)
        {
            return main;
        }

        var any = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        return any.Length > 0 ? any[0] : null;
    }

    private static string ArgValue(string[] args, string flag, string fallback)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == flag)
            {
                return args[i + 1];
            }
        }

        return fallback;
    }

    private static int ArgInt(string[] args, string flag, int fallback)
    {
        return int.TryParse(ArgValue(args, flag, null), out var value) ? value : fallback;
    }
}
