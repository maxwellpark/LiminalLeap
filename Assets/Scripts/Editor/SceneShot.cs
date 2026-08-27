using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Renders a generated scene to a PNG without opening the editor UI or a browser.
//
// Driving the web build through Chrome to look at it does not work: runInBackground is off,
// so Unity stops painting the moment focus lapses, and most "screenshots" came back black.
// This renders deterministically in batchmode, and doubles as a way to produce stills for
// an itch page.
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

            // The endless scenes hold a generator and a player and nothing else: the track
            // and the lighting only exist once the game runs. Drive the real components
            // rather than reproducing what they do, or this becomes a second source of
            // truth that drifts from the game.
            Populate();

            var camera = FindCamera();

            if (camera == null)
            {
                Debug.LogWarning("SHOT SKIPPED " + scene.name + ": no camera");
                continue;
            }

            // Nudged up and back off the player's eye line: the runtime camera is first
            // person and sits inside the geometry until the run moves it.
            camera.transform.position += Vector3.up * 1.6f - camera.transform.forward * 4f;

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
