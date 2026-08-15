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
        go.AddComponent<TitleScreen>();

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
            slab.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(Surface.Track);
            Object.DestroyImmediate(slab.GetComponent<Collider>());
        }

        for (var i = 0; i < 10; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar.name = "Pillar" + i;
            pillar.transform.SetParent(root.transform);
            pillar.transform.localScale = new Vector3(1.2f, 9f, 1.2f);
            pillar.transform.position = new Vector3(i % 2 == 0 ? -6.5f : 6.5f, 4.5f, 12f + i * 22f);
            pillar.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(Surface.Track);
            Object.DestroyImmediate(pillar.GetComponent<Collider>());
        }
    }

    // The title can't load the game scene unless both are registered.
    private static void RegisterBuildScenes()
    {
        var wanted = new[] { TitlePath, GamePath };
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        foreach (var path in wanted)
        {
            if (File.Exists(path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            else
            {
                Debug.LogWarning("Not registering missing scene: " + path);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("BUILD SCENES: " + string.Join(", ", scenes.ConvertAll(s => s.path)));
    }
}
