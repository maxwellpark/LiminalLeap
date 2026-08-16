using System.IO;
using UnityEditor;
using UnityEngine;

// Generates the piece prefabs the runtime generator chains. Hazards and pickups are baked
// into variants rather than spawned separately, so pooling keeps working unchanged.
public static class TrackPiecePrefabs
{
    public const string Folder = "Assets/Prefabs/TrackPieces";

    private const float Length = 10f;
    private const float Width = 8f;
    private const float HalfWidth = 3f;

    [MenuItem("Liminal Leap/Generate Track Piece Prefabs")]
    public static void GenerateFromCommandLine()
    {
        Directory.CreateDirectory(Folder);

        Build("Straight", 0f, Decoration.None);
        Build("TurnLeft", -7f, Decoration.None);
        Build("TurnRight", 7f, Decoration.None);
        Build("Pickup", 0f, Decoration.Pickup);
        Build("Block", 0f, Decoration.Block);
        Build("Bar", 0f, Decoration.Bar);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PIECES GENERATED in " + Folder);
    }

    private enum Decoration
    {
        None,
        Pickup,
        Block,
        Bar,
    }

    private static void Build(string name, float yaw, Decoration decoration)
    {
        var root = new GameObject(name);

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(Width, 0.5f, Length);
        visual.transform.localPosition = new Vector3(0f, -0.25f, Length * 0.5f);
        visual.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(Surface.Track);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        var anchor = new GameObject("End").transform;
        anchor.SetParent(root.transform, false);
        anchor.localPosition = new Vector3(0f, 0f, Length);
        anchor.localRotation = Quaternion.Euler(0f, yaw, 0f);

        var piece = root.AddComponent<TrackPiece>();
        var so = new SerializedObject(piece);
        so.FindProperty("endAnchor").objectReferenceValue = anchor;
        so.FindProperty("containsHazard").boolValue = decoration is Decoration.Block or Decoration.Bar;
        so.ApplyModifiedPropertiesWithoutUndo();

        Decorate(root.transform, decoration);

        var path = Path.Combine(Folder, name + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
    }

    private static void Decorate(Transform piece, Decoration decoration)
    {
        switch (decoration)
        {
            case Decoration.Pickup:
            {
                var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pickup.name = "SpeedPickup";
                pickup.transform.SetParent(piece, false);
                pickup.transform.localPosition = new Vector3(0f, 0.5f, Length * 0.5f);
                pickup.GetComponent<Collider>().isTrigger = true;
                pickup.GetComponent<Renderer>().sharedMaterial = MaterialLibrary.Get(Surface.Pickup);

                var trig = pickup.AddComponent<SpeedTriggerable>();
                var so = new SerializedObject(trig);
                so.FindProperty("speedToAdd").floatValue = 2f;
                so.FindProperty("shakeCamera").boolValue = true;
                so.FindProperty("shakeSettings").FindPropertyRelative("Amplitude").floatValue = 0.06f;
                so.FindProperty("shakeSettings").FindPropertyRelative("Duration").floatValue = 0.12f;
                so.ApplyModifiedPropertiesWithoutUndo();
                break;
            }

            case Decoration.Block:
                // Offset to one side, so there is always a way past without jumping.
                AddHazard(piece, -1.5f, 2.2f, false);
                break;

            case Decoration.Bar:
                // Spans the track, so this one has to be jumped.
                AddHazard(piece, 0f, 0.6f, true);
                break;
        }
    }

    private static void AddHazard(Transform piece, float lane, float height, bool jumpable)
    {
        var width = jumpable ? HalfWidth * 2f : 1.8f;

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = jumpable ? "HazardBar" : "HazardBlock";
        body.transform.SetParent(piece, false);
        body.transform.localPosition = new Vector3(lane, height * 0.5f, Length * 0.5f);
        body.transform.localScale = new Vector3(width, height, 0.8f);
        body.GetComponent<Collider>().isTrigger = true;
        body.GetComponent<Renderer>().sharedMaterial =
            MaterialLibrary.Get(jumpable ? Surface.JumpBar : Surface.Hazard);

        var hazard = body.AddComponent<Hazard>();
        var so = new SerializedObject(hazard);
        so.FindProperty("jumpable").boolValue = jumpable;
        so.ApplyModifiedPropertiesWithoutUndo();

        var zone = new GameObject("NearMiss");
        zone.transform.SetParent(body.transform, false);
        zone.transform.localScale = new Vector3(2.4f, 1f, 3.5f);
        zone.AddComponent<BoxCollider>().isTrigger = true;
        zone.AddComponent<NearMissZone>();
    }
}
