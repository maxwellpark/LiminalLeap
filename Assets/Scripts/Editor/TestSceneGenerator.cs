using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds a playable test scene from a seed: track, pickups, managers, player.
// Menu for hand use, GenerateFromCommandLine for headless/agentic runs.
public static class TestSceneGenerator
{
    private const string OutputDir = "Assets/Scenes/Generated";

    // The player's trigger box is 1 unit centred on the track surface, so it only reaches
    // y +0.5. Pickups at y=1 with a 0.4 radius missed by 0.1 and were catchable only mid-jump.
    private const float PickupHeight = 0.5f;
    private const float PickupDiameter = 1f;

    public class Settings
    {
        public int Seed = 1;
        public int Pieces = 40;
        public float PieceLength = 10f;
        public float PickupChance = 0.35f;
        public float MaxYawDegrees = 7f;  // per seam; strings of these make the bends
        public float StraightChance = 0.4f;
        public float HazardChance = 0.3f;
        public float TrackHalfWidth = 3f;   // matches the player's strafe limit
        public float PlayerHalfWidth = 0.6f;
        public string Name = "Generated";
    }

    [MenuItem("Liminal Leap/Generate Test Scene")]
    public static void GenerateDefault()
    {
        Debug.Log("GENERATED " + Generate(new Settings { Seed = Environment.TickCount, Name = "Manual" }));
    }

    // -executeMethod entry. Args: -seed N -pieces N -count N -yaw N
    public static void GenerateFromCommandLine()
    {
        var args = Environment.GetCommandLineArgs();
        var seed = ArgInt(args, "-seed", 1);
        var pieces = ArgInt(args, "-pieces", 40);
        var count = ArgInt(args, "-count", 1);
        var yaw = ArgInt(args, "-yaw", 7);

        for (var i = 0; i < count; i++)
        {
            Debug.Log("GENERATED " + Generate(new Settings
            {
                Seed = seed + i,
                Pieces = pieces,
                MaxYawDegrees = yaw,
                Name = "Seed" + (seed + i),
            }));
        }
    }

    public static string Generate(Settings settings)
    {
        Directory.CreateDirectory(OutputDir);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var rng = new System.Random(settings.Seed);

        var root = new GameObject("GeneratedTrack");
        var track = root.AddComponent<Track>();

        var position = Vector3.zero;
        var forward = Vector3.forward;

        for (var i = 0; i < settings.Pieces; i++)
        {
            // Origin sits at the piece's start, so it chains socket to socket like the runtime generator.
            var piece = new GameObject("Piece" + i);
            piece.transform.SetParent(root.transform);
            piece.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward));

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(piece.transform, false);
            visual.transform.localScale = new Vector3(8f, 0.5f, settings.PieceLength);
            visual.transform.localPosition = new Vector3(0f, -0.25f, settings.PieceLength * 0.5f);

            // First piece stays straight so the run doesn't open on a bend.
            var yaw = i == 0 || rng.NextDouble() < settings.StraightChance
                ? 0f
                : (float)(rng.NextDouble() * 2d - 1d) * settings.MaxYawDegrees;

            var anchor = new GameObject("End").transform;
            anchor.SetParent(piece.transform, false);
            anchor.localPosition = new Vector3(0f, 0f, settings.PieceLength);
            anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var trackPiece = piece.AddComponent<TrackPiece>();
            SetPrivate(trackPiece, "endAnchor", anchor);

            // Hazards first: pickups then avoid the lanes they occupy.
            var blocked = new List<HazardLanes.Span>();
            if (i > 2 && rng.NextDouble() < settings.HazardChance)
            {
                blocked = AddHazardRow(piece.transform, settings, rng);
            }

            if (rng.NextDouble() < settings.PickupChance)
            {
                var lane = PickFreeLane(blocked, settings, rng);
                AddPickup(root.transform, piece.transform.TransformPoint(new Vector3(lane, PickupHeight, settings.PieceLength * 0.5f)));
            }

            position = anchor.position;
            forward = anchor.forward;
        }

        AddKillFloor(root.transform, settings.Pieces * settings.PieceLength);
        AddSupportingCast(track);

        var path = Path.Combine(OutputDir, settings.Name + ".unity");
        EditorSceneManager.SaveScene(scene, path);
        return path;
    }

    // Without these the scene builds a track nobody can run on.
    private static void AddSupportingCast(Track track)
    {
        InstantiatePrefab("Assets/Prefabs/Singletons/GameManager.prefab");
        InstantiatePrefab("Assets/Prefabs/Singletons/UIManager.prefab");
        var cameraManager = InstantiatePrefab("Assets/Prefabs/Singletons/CameraManager.prefab");

        var trackManager = InstantiatePrefab("Assets/Prefabs/Singletons/TrackManager.prefab");
        if (trackManager != null)
        {
            var tm = trackManager.GetComponent<TrackManager>();
            if (tm != null)
            {
                SetPrivate(tm, "startingTrack", track);
            }
        }

        var player = InstantiatePrefab("Assets/Prefabs/Player.prefab");
        if (player != null)
        {
            // On the track line: basePos starts here and converges onto the anchors,
            // so spawning higher just makes the player sink for the first piece.
            player.transform.position = Vector3.zero;
        }

        RemoveStrayCameras(player);

        // CameraShake lives on the Player prefab, so it can only be wired once both exist.
        if (cameraManager != null && player != null)
        {
            var shake = player.GetComponentInChildren<CameraShake>(true);
            if (shake != null)
            {
                SetPrivate(cameraManager.GetComponent<CameraManager>(), "cameraShake", shake);
            }
        }
    }

    // The Player prefab brings its own MainCamera-tagged camera, so the one the default
    // scene template drops at the origin just fights it for Camera.main and never moves.
    private static void RemoveStrayCameras(GameObject player)
    {
        var playerCameras = player != null
            ? player.GetComponentsInChildren<Camera>(true)
            : Array.Empty<Camera>();

        foreach (var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (Array.IndexOf(playerCameras, cam) >= 0)
            {
                continue;
            }

            Debug.Log("Removing stray camera: " + cam.gameObject.name);
            UnityEngine.Object.DestroyImmediate(cam.gameObject);
        }
    }

    private static GameObject InstantiatePrefab(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogWarning("Missing prefab, skipping: " + path);
            return null;
        }

        return (GameObject)PrefabUtility.InstantiatePrefab(asset);
    }

    // Places blockers one at a time, refusing any that would leave no way through.
    private static List<HazardLanes.Span> AddHazardRow(Transform piece, Settings settings, System.Random rng)
    {
        var blocked = new List<HazardLanes.Span>();
        var wanted = rng.NextDouble() < 0.3d ? 2 : 1;
        const float halfWidth = 0.9f;

        for (var attempt = 0; attempt < 12 && blocked.Count < wanted; attempt++)
        {
            var centre = (float)(rng.NextDouble() * 2d - 1d) * (settings.TrackHalfWidth - halfWidth);
            var candidate = new HazardLanes.Span(centre, halfWidth);

            var trial = new List<HazardLanes.Span>(blocked) { candidate };
            if (!HazardLanes.HasGap(trial, settings.TrackHalfWidth, settings.PlayerHalfWidth))
            {
                continue; // would seal the track
            }

            var jumpable = rng.NextDouble() < 0.4d;
            AddHazard(piece, settings, centre, halfWidth, jumpable);
            blocked.Add(candidate);
        }

        return blocked;
    }

    private static void AddHazard(Transform piece, Settings settings, float lane, float halfWidth, bool jumpable)
    {
        var height = jumpable ? 0.6f : 2.2f;
        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = jumpable ? "HazardBar" : "HazardBlock";
        body.transform.SetParent(piece, false);
        body.transform.localPosition = new Vector3(lane, height * 0.5f, settings.PieceLength * 0.5f);
        body.transform.localScale = new Vector3(halfWidth * 2f, height, 0.8f);
        body.GetComponent<Collider>().isTrigger = true;

        var hazard = body.AddComponent<Hazard>();
        var so = new SerializedObject(hazard);
        so.FindProperty("jumpable").boolValue = jumpable;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Wider child trigger for the near-miss credit.
        var zone = new GameObject("NearMiss");
        zone.transform.SetParent(body.transform, false);
        zone.transform.localScale = new Vector3(2.4f, 1f, 3.5f);
        var zoneCol = zone.AddComponent<BoxCollider>();
        zoneCol.isTrigger = true;
        zone.AddComponent<NearMissZone>();
    }

    private static float PickFreeLane(List<HazardLanes.Span> blocked, Settings settings, System.Random rng)
    {
        for (var attempt = 0; attempt < 12; attempt++)
        {
            var lane = (float)(rng.NextDouble() * 2d - 1d) * settings.TrackHalfWidth;
            var clear = true;

            foreach (var span in blocked)
            {
                if (lane > span.Min - 0.5f && lane < span.Max + 0.5f)
                {
                    clear = false;
                    break;
                }
            }

            if (clear)
            {
                return lane;
            }
        }

        return 0f;
    }

    // Nothing to fall onto otherwise, so a mistake just leaves you drifting.
    private static void AddKillFloor(Transform parent, float length)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "KillFloor";
        floor.tag = "KillFloor";
        floor.transform.SetParent(parent, false);
        floor.transform.position = new Vector3(0f, -8f, length * 0.5f);
        floor.transform.localScale = new Vector3(400f, 1f, length + 200f);
        floor.GetComponent<Collider>().isTrigger = true;
        UnityEngine.Object.DestroyImmediate(floor.GetComponent<MeshRenderer>());
    }

    private static void AddPickup(Transform parent, Vector3 position)
    {
        var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "SpeedPickup";
        pickup.transform.SetParent(parent);
        pickup.transform.position = position;
        pickup.transform.localScale = Vector3.one * PickupDiameter;
        pickup.GetComponent<Collider>().isTrigger = true;

        var trig = pickup.AddComponent<SpeedTriggerable>();
        var so = new SerializedObject(trig);
        so.FindProperty("speedToAdd").floatValue = 2f;
        so.FindProperty("shakeCamera").boolValue = true;
        so.FindProperty("shakeSettings").FindPropertyRelative("Amplitude").floatValue = 0.06f;
        so.FindProperty("shakeSettings").FindPropertyRelative("Duration").floatValue = 0.12f;
        so.ApplyModifiedPropertiesWithoutUndo();
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
            Debug.LogWarning("No serialized field '" + field + "' on " + target.GetType().Name);
            return;
        }

        switch (value)
        {
            case float f:
                prop.floatValue = f;
                break;
            case UnityEngine.Object o:
                prop.objectReferenceValue = o;
                break;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
