using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds a throwaway play-test scene from a seed: track, scattered pickups, junctions.
// Menu for hand use, GenerateFromCommandLine for headless/agentic runs.
public static class TestSceneGenerator
{
    private const string OutputDir = "Assets/Scenes/Generated";

    public class Settings
    {
        public int Seed = 1;
        public int Pieces = 40;
        public float PickupChance = 0.35f;
        public int JunctionEvery = 12;
        public string Name = "Generated";
    }

    [MenuItem("Liminal Leap/Generate Test Scene")]
    public static void GenerateDefault()
    {
        var path = Generate(new Settings { Seed = Environment.TickCount, Name = "Manual" });
        Debug.Log("GENERATED " + path);
    }

    // -executeMethod entry point. Args: -seed N -pieces N -count N
    public static void GenerateFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        var seed = ArgInt(args, "-seed", 1);
        var pieces = ArgInt(args, "-pieces", 40);
        var count = ArgInt(args, "-count", 1);

        for (var i = 0; i < count; i++)
        {
            var settings = new Settings { Seed = seed + i, Pieces = pieces, Name = "Seed" + (seed + i) };
            Debug.Log("GENERATED " + Generate(settings));
        }
    }

    public static string Generate(Settings settings)
    {
        Directory.CreateDirectory(OutputDir);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var rng = new System.Random(settings.Seed);

        var root = new GameObject("GeneratedTrack");
        var built = 0;
        var cursor = Vector3.zero;

        for (var i = 0; i < settings.Pieces; i++)
        {
            var length = 10f;
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "Piece" + i;
            piece.transform.SetParent(root.transform);
            piece.transform.localScale = new Vector3(8f, 0.5f, length);
            piece.transform.position = cursor + new Vector3(0f, -0.25f, length * 0.5f);

            var trackPiece = piece.AddComponent<TrackPiece>();
            var anchor = new GameObject("End").transform;
            anchor.SetParent(piece.transform, false);
            // local +Z is half the piece, and the cube's scale is already applied by the parent
            anchor.localPosition = new Vector3(0f, 0.5f, 0.5f);
            SetPrivate(trackPiece, "endAnchor", anchor);

            if (rng.NextDouble() < settings.PickupChance)
            {
                AddPickup(root.transform, cursor + new Vector3(LaneOffset(rng), 1f, length * 0.5f));
            }

            cursor += new Vector3(0f, 0f, length);
            built++;

            if (settings.JunctionEvery > 0 && built % settings.JunctionEvery == 0)
            {
                AddJunctionMarker(root.transform, cursor);
            }
        }

        var path = Path.Combine(OutputDir, settings.Name + ".unity");
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    private static void AddPickup(Transform parent, Vector3 position)
    {
        var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "SpeedPickup";
        pickup.transform.SetParent(parent);
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * 0.8f;

        var col = pickup.GetComponent<Collider>();
        col.isTrigger = true;

        var trig = pickup.AddComponent<SpeedTriggerable>();
        SetPrivate(trig, "speedToAdd", 2f);
    }

    // A visible marker only; wiring real branches needs authored Track objects.
    private static void AddJunctionMarker(Transform parent, Vector3 position)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "JunctionMarker";
        marker.transform.SetParent(parent);
        marker.transform.position = position + Vector3.up;
        marker.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
        UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
    }

    private static float LaneOffset(System.Random rng)
    {
        return (float)(rng.NextDouble() * 6d - 3d);
    }

    private static int ArgInt(string[] args, string flag, int fallback)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == flag && int.TryParse(args[i + 1], out var value))
            {
                return value;
            }
        }

        return fallback;
    }

    private static void SetPrivate(UnityEngine.Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(field);
        if (prop == null)
        {
            return;
        }

        if (value is Transform t)
        {
            prop.objectReferenceValue = t;
        }
        else if (value is float f)
        {
            prop.floatValue = f;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
