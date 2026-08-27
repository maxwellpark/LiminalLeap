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
        MaterialAssets.GenerateFromCommandLine();

        Build("Straight", 0f, Decoration.None);
        Build("TurnLeft", -7f, Decoration.None);
        Build("TurnRight", 7f, Decoration.None);
        Build("Pickup", 0f, Decoration.Pickup);
        Build("Block", 0f, Decoration.Block);
        Build("Bar", 0f, Decoration.Bar);
        Build("Exit", 0f, Decoration.Exit);

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
        Exit,
    }

    private static void Build(string name, float yaw, Decoration decoration)
    {
        var root = new GameObject(name);

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = new Vector3(Width, 0.5f, Length);
        visual.transform.localPosition = new Vector3(0f, -0.25f, Length * 0.5f);
        visual.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Track);
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
        AddScenery(root.transform);

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
                pickup.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Pickup);

                var trig = pickup.AddComponent<SpeedTriggerable>();
                var so = new SerializedObject(trig);
                so.FindProperty("speedToAdd").floatValue = 2f;
                so.FindProperty("shakeCamera").boolValue = true;
                so.FindProperty("shakeSettings").FindPropertyRelative("Amplitude").floatValue = 0.06f;
                so.FindProperty("shakeSettings").FindPropertyRelative("Duration").floatValue = 0.12f;
                so.ApplyModifiedPropertiesWithoutUndo();

                pickup.AddComponent<PickupBob>();
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

            case Decoration.Exit:
            {
                // The zone spans the whole piece so the offer lasts long enough to take.
                // It only offers: banking needs a press, so strafing through costs nothing.
                var zone = new GameObject("ExitZone");
                zone.transform.SetParent(piece, false);
                zone.transform.localPosition = new Vector3(2.8f, 1.1f, Length * 0.5f);

                var box = zone.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(2.2f, 2.4f, Length);
                zone.AddComponent<ExitDoor>();

                // Child of the zone, so turning the feature off hides the door with it.
                var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frame.name = "ExitFrame";
                frame.transform.SetParent(zone.transform, false);
                frame.transform.localPosition = new Vector3(0.6f, 0f, 0f);
                frame.transform.localScale = new Vector3(0.35f, 2.4f, 2.8f);
                frame.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Exit);
                Object.DestroyImmediate(frame.GetComponent<Collider>());

                WorldSign.Floor(piece, "EXIT", new Vector3(2.8f, 0.02f, Length * 0.5f - 2.6f), 2.4f,
                    new Color(0.4f, 0.95f, 0.55f, 0.9f));
                break;
            }
        }
    }

    // Built into every piece and toggled per spawn by TrackScenery, so the corridor varies
    // without the generator instantiating anything at runtime.
    private static void AddScenery(Transform piece)
    {
        var scenery = piece.gameObject.AddComponent<TrackScenery>();

        var pillars = new GameObject(TrackScenery.PillarHolder).transform;
        pillars.SetParent(piece, false);

        // Outside the deck, which is 8 wide, so they frame the run rather than block it.
        float[] lanes = { -6.5f, 6.5f };
        float[] along = { Length * 0.25f, Length * 0.75f };

        for (var l = 0; l < lanes.Length; l++)
        {
            for (var a = 0; a < along.Length; a++)
            {
                var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pillar.name = $"Pillar{l}{a}";
                pillar.transform.SetParent(pillars, false);
                pillar.transform.localPosition = new Vector3(lanes[l], 4f, along[a]);
                pillar.transform.localScale = new Vector3(1.2f, 8f, 1.2f);
                pillar.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Track);
                Object.DestroyImmediate(pillar.GetComponent<Collider>());
            }
        }

        var lights = new GameObject(TrackScenery.LightHolder).transform;
        lights.SetParent(piece, false);

        // Overhead strips. The corridor had no ceiling at all, so there was nothing above
        // the horizon and nothing to explain where the light was coming from.
        for (var a = 0; a < along.Length; a++)
        {
            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = $"Strip{a}";
            strip.transform.SetParent(lights, false);
            strip.transform.localPosition = new Vector3(0f, 5.6f, along[a]);
            strip.transform.localScale = new Vector3(2.6f, 0.14f, 2.4f);
            strip.GetComponent<Renderer>().sharedMaterial = MaterialAssets.Load(Surface.Light);
            Object.DestroyImmediate(strip.GetComponent<Collider>());
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
            MaterialAssets.Load(jumpable ? Surface.JumpBar : Surface.Hazard);

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
