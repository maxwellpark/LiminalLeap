using System.Collections.Generic;
using UnityEngine;

// Built at runtime: TestSceneGenerator is editor-only and generated scenes are ignored.
public class RunFixture
{
    public const float PieceLength = 10f;
    public const float TrackHalfWidth = 3f;

    public GameObject Root { get; private set; }
    public PlayerTrackMovement Player { get; private set; }
    public ScriptedInput Input { get; private set; }

    private readonly List<GameObject> spawned = new();

    // Built inactive and activated in order: Awake fires the moment a component is added.
    public void Build(int pieces = 30)
    {
        Root = new GameObject("TestRoot");
        Root.SetActive(false);
        spawned.Add(Root);

        var camera = new GameObject("Main Camera", typeof(Camera));
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 2f, -5f);
        spawned.Add(camera);

        var trackGo = new GameObject("Track");
        trackGo.transform.SetParent(Root.transform);

        for (var i = 0; i < pieces; i++)
        {
            BuildPiece(trackGo.transform, i);
        }

        var track = trackGo.AddComponent<Track>();

        var managers = new GameObject("Managers");
        managers.SetActive(false);
        spawned.Add(managers);
        managers.AddComponent<GameManager>();

        var tm = managers.AddComponent<TrackManager>();
        SetField(tm, "startingTrack", track);

        Player = BuildPlayer();
        Input = new ScriptedInput();
        InputRouter.Source = Input;

        Root.SetActive(true);              // pieces exist before Track.Awake reads them
        managers.SetActive(true);          // TrackManager.Awake switches to startingTrack
        Player.gameObject.SetActive(true); // player Start finds a live TrackManager
    }

    public void Teardown()
    {
        InputRouter.Reset();
        Time.timeScale = 1f;

        foreach (var go in spawned)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
            }
        }

        spawned.Clear();

        // Managers spawn themselves, so sweep anything they left behind.
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(canvas.gameObject);
        }
    }

    public GameObject AddHazard(float lane, int pieceIndex)
    {
        var hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hazard.name = "TestHazard";
        hazard.transform.SetParent(Root.transform);
        hazard.transform.position = new Vector3(lane, 0.5f, pieceIndex * PieceLength + PieceLength * 0.5f);
        hazard.transform.localScale = new Vector3(1.8f, 2f, 0.8f);
        hazard.GetComponent<Collider>().isTrigger = true;
        hazard.AddComponent<Hazard>();
        spawned.Add(hazard);
        return hazard;
    }

    public SpeedTriggerable AddPickup(float lane, int pieceIndex)
    {
        var pickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pickup.name = "TestPickup";
        pickup.transform.SetParent(Root.transform);
        pickup.transform.position = new Vector3(lane, 0.5f, pieceIndex * PieceLength + PieceLength * 0.5f);
        pickup.GetComponent<Collider>().isTrigger = true;

        var trig = pickup.AddComponent<SpeedTriggerable>();
        SetField(trig, "speedToAdd", 2f);
        spawned.Add(pickup);
        return trig;
    }

    private static void BuildPiece(Transform parent, int index)
    {
        var piece = new GameObject("Piece" + index);
        piece.transform.SetParent(parent);
        piece.transform.SetPositionAndRotation(
            new Vector3(0f, 0f, index * PieceLength), Quaternion.identity);

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(piece.transform, false);
        visual.transform.localScale = new Vector3(8f, 0.5f, PieceLength);
        visual.transform.localPosition = new Vector3(0f, -0.25f, PieceLength * 0.5f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        var anchor = new GameObject("End").transform;
        anchor.SetParent(piece.transform, false);
        anchor.localPosition = new Vector3(0f, 0f, PieceLength);

        var trackPiece = piece.AddComponent<TrackPiece>();
        SetField(trackPiece, "endAnchor", anchor);
    }

    private PlayerTrackMovement BuildPlayer()
    {
        var go = new GameObject("Player");
        go.SetActive(false);
        go.transform.position = Vector3.zero;
        spawned.Add(go);

        var body = go.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;

        var trigger = go.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Vector3.one;

        return go.AddComponent<PlayerTrackMovement>();
    }

    // Reflection rather than SerializedObject: that's editor-only and this has to run in CI.
    private static void SetField(object target, string name, object value)
    {
        var field = target.GetType().GetField(name,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert(field != null, "no field " + name + " on " + target.GetType().Name);
        field.SetValue(target, value);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.InvalidOperationException(message);
        }
    }
}
