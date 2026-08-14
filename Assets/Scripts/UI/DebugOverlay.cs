using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;

// F1 in play mode. Live telemetry plus sliders bound straight to the serialized tuning
// fields, so a value that feels wrong gets dragged rather than described, changed in code
// and replayed.
//
// Dev tooling, so it reads the keyboard directly rather than through InputRouter: the
// gameplay seam is for gameplay, and a scripted test should never toggle this.
public class DebugOverlay : Singleton<DebugOverlay>
{
    private class Knob
    {
        public string Label;
        public Func<float> Get;
        public Action<float> Set;
        public float Min;
        public float Max;
    }

    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    [SerializeField] private int width = 380;

    private readonly List<Knob> knobs = new();
    private bool built;
    private bool visible;
    private float smoothedMs;
    private ProfilerRecorder gcRecorder;
    private Vector2 scroll;

    protected override void OnEnable()
    {
        base.OnEnable();
        gcRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (gcRecorder.Valid)
        {
            gcRecorder.Dispose();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            visible = !visible;
            if (visible && !built)
            {
                BuildKnobs();
            }
        }

        // Unscaled: the death sequence slows time and would otherwise fake a frame spike.
        smoothedMs = Mathf.Lerp(smoothedMs, Time.unscaledDeltaTime * 1000f, 0.1f);
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        GUILayout.BeginArea(new Rect(10f, 10f, width, Screen.height - 20f), GUI.skin.box);
        scroll = GUILayout.BeginScrollView(scroll);

        GUILayout.Label("<b>Run</b>", RichLabel());
        GUILayout.Label($"speed      {PlayerTrackMovement.CurrentSpeed:F2}  ({PlayerTrackMovement.SpeedFraction:P0})");
        GUILayout.Label($"distance   {PlayerTrackMovement.DistanceCovered:F1} m");
        GUILayout.Label($"score      {PlayerTrackMovement.Score:F0}  x{PlayerTrackMovement.Multiplier:F2}");

        GUILayout.Space(6f);
        GUILayout.Label("<b>Frame</b>", RichLabel());
        GUILayout.Label($"frame      {smoothedMs:F2} ms  ({(smoothedMs > 0f ? 1000f / smoothedMs : 0f):F0} fps)");
        GUILayout.Label($"gc/frame   {GcPerFrame()}");
        GUILayout.Label($"pieces     {CountPieces()}");

        GUILayout.Space(6f);
        GUILayout.Label("<b>Tuning</b>", RichLabel());

        foreach (var knob in knobs)
        {
            var value = knob.Get();
            GUILayout.Label($"{knob.Label}  {value:F3}");
            var next = GUILayout.HorizontalSlider(value, knob.Min, knob.Max);
            if (!Mathf.Approximately(next, value))
            {
                knob.Set(next);
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private string GcPerFrame()
    {
        if (!gcRecorder.Valid)
        {
            return "unavailable";
        }

        var bytes = gcRecorder.LastValue;
        return bytes <= 0 ? "0 B" : bytes < 1024 ? bytes + " B" : (bytes / 1024f).ToString("F1") + " KB";
    }

    private static int CountPieces()
    {
        return FindObjectsByType<TrackPiece>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
    }

    private static GUIStyle RichLabel()
    {
        return new GUIStyle(GUI.skin.label) { richText = true };
    }

    public readonly struct KnobSpec
    {
        public readonly Type Owner;
        public readonly string Field;
        public readonly string Label;
        public readonly float Min;
        public readonly float Max;

        public KnobSpec(Type owner, string field, string label, float min, float max)
        {
            Owner = owner;
            Field = field;
            Label = label;
            Min = min;
            Max = max;
        }
    }

    // Explicit list rather than reflecting over everything: these are the numbers that
    // were invented and are most likely wrong. Public so a test can check the field
    // names still resolve, since a typo here only warns at runtime behind an F1 press.
    public static readonly KnobSpec[] Specs =
    {
        new(typeof(PlayerTrackMovement), "maxSpeed", "max speed", 10f, 60f),
        new(typeof(PlayerTrackMovement), "acceleration", "accel", 0.2f, 8f),
        new(typeof(PlayerTrackMovement), "jumpHeight", "jump height", 0.5f, 6f),
        new(typeof(PlayerTrackMovement), "jumpUpTime", "jump rise", 0.1f, 0.8f),
        new(typeof(PlayerTrackMovement), "strafeSpeed", "strafe speed", 2f, 20f),
        new(typeof(PlayerTrackMovement), "strafeAccel", "strafe accel", 5f, 200f),
        new(typeof(PlayerTrackMovement), "bobHeight", "bob height", 0f, 0.3f),
        new(typeof(PlayerTrackMovement), "bobStridesPerSecond", "bob rate", 0.5f, 8f),
        new(typeof(PlayerTrackMovement), "maxFovBoost", "fov boost", 0f, 45f),
        new(typeof(PlayerTrackMovement), "deathTimeScale", "death slowmo", 0.05f, 1f),
        new(typeof(PlayerTrackMovement), "deathPause", "death pause", 0.1f, 2f),
        new(typeof(MoodLighting), "fogDensity", "fog density", 0f, 0.06f),
        new(typeof(MoodLighting), "flickerDepth", "flicker", 0f, 0.6f),
        new(typeof(SpeedVignette), "fullIntensity", "vignette", 0f, 1f),
        new(typeof(AudioManager), "sfxVolume", "sfx vol", 0f, 1f),
        new(typeof(AudioManager), "windVolume", "wind vol", 0f, 0.6f),
    };

    public static FieldInfo Resolve(KnobSpec spec)
    {
        var info = spec.Owner.GetField(spec.Field, BindingFlags.Instance | BindingFlags.NonPublic);
        return info != null && info.FieldType == typeof(float) ? info : null;
    }

    private void BuildKnobs()
    {
        built = true;
        knobs.Clear();

        foreach (var spec in Specs)
        {
            var target = FindFirstObjectByType(spec.Owner) as MonoBehaviour;
            if (target == null)
            {
                continue;
            }

            var info = Resolve(spec);
            if (info == null)
            {
                Debug.LogWarning($"DebugOverlay: no float field '{spec.Field}' on {spec.Owner.Name}");
                continue;
            }

            knobs.Add(new Knob
            {
                Label = spec.Label,
                Get = () => (float)info.GetValue(target),
                Set = v => info.SetValue(target, v),
                Min = spec.Min,
                Max = spec.Max,
            });
        }
    }
}
