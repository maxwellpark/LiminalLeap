using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the title scene and registers it plus the game scene in Build Settings.
public static class TitleSceneGenerator
{
    private const string OutputDir = "Assets/Scenes";
    private const string TitlePath = OutputDir + "/TitleScreen.unity";
    private const string GamePath = OutputDir + "/MovementTestScene.unity";

    [MenuItem("Liminal Leap/Generate Title Scene")]
    public static void GenerateFromCommandLine()
    {
        Directory.CreateDirectory(OutputDir);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var camera = Camera.main;
        if (camera != null)
        {
            camera.transform.SetPositionAndRotation(new Vector3(0f, 2.2f, -14f), Quaternion.Euler(4f, 0f, 0f));
            camera.fieldOfView = 55f;
        }

        BuildBackdrop();

        var go = new GameObject("TitleScreen");
        var title = go.AddComponent<TitleScreen>();
        PointAtGameScenes(title);

        EditorSceneManager.SaveScene(scene, TitlePath);
        RegisterBuildScenes();

        Debug.Log("GENERATED " + TitlePath);
    }

    // Something to drift past, using the same materials so it reads as the same game.
    private static void BuildBackdrop()
    {
        var root = new GameObject("Backdrop");

        for (var i = 0; i < 24; i++)
        {
            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "Slab" + i;
            slab.transform.SetParent(root.transform);
            slab.transform.localScale = new Vector3(8f, 0.5f, 10f);
            slab.transform.position = new Vector3(0f, -0.25f, i * 10f);
            slab.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Track);
            Object.DestroyImmediate(slab.GetComponent<Collider>());
        }

        for (var i = 0; i < 10; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Pillar" + i;
            pillar.transform.SetParent(root.transform);
            pillar.transform.localScale = new Vector3(1.2f, 9f, 1.2f);
            pillar.transform.position = new Vector3(i % 2 == 0 ? -6.5f : 6.5f, 4.5f, 12f + i * 22f);
            pillar.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Track);
            Object.DestroyImmediate(pillar.GetComponent<Collider>());
        }
    }

    // The title can't load anything that isn't registered, generated scenes included.
    [MenuItem("Liminal Leap/Refresh Build Scenes")]
    public static void RegisterBuildScenes()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        if (File.Exists(TitlePath))
        {
            scenes.Add(new EditorBuildSettingsScene(TitlePath, true));
        }

        foreach (var path in GeneratedScenePaths())
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        if (File.Exists(GamePath))
        {
            scenes.Add(new EditorBuildSettingsScene(GamePath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("BUILD SCENES: " + string.Join(", ", scenes.ConvertAll(s => s.path)));
    }

    // Only the fallback. Generated scenes are resolved from Build Settings at runtime,
    // because a baked list of names goes stale as soon as scenes are regenerated.
    private static void PointAtGameScenes(TitleScreen title)
    {
        var so = new SerializedObject(title);
        var array = so.FindProperty("gameScenes");
        array.arraySize = 1;
        array.GetArrayElementAtIndex(0).stringValue = Path.GetFileNameWithoutExtension(GamePath);
        so.ApplyModifiedPropertiesWithoutUndo();

        Debug.Log("TITLE FALLBACK: " + Path.GetFileNameWithoutExtension(GamePath));
    }

    public static string[] GeneratedScenePaths()
    {
        var dir = "Assets/Scenes/Generated";
        if (!Directory.Exists(dir))
        {
            return System.Array.Empty<string>();
        }

        var found = Directory.GetFiles(dir, "*.unity");
        System.Array.Sort(found);
        for (var i = 0; i < found.Length; i++)
        {
            found[i] = found[i].Replace('\\', '/');
        }

        return found;
    }
}
