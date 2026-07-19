using UnityEditor;
using UnityEngine;

// Small editor convenience: drop a procedural track generator into the scene from the
// menu, then assign the piece prefabs + player in the inspector. Saves the manual
// hand-placing of a whole track.
public static class TrackGeneratorMenu
{
    [MenuItem("Liminal Leap/Add Procedural Track Generator")]
    public static void AddGenerator()
    {
        var go = new GameObject("ProceduralTrackGenerator");
        Undo.RegisterCreatedObjectUndo(go, "Add Procedural Track Generator");
        go.AddComponent<ProceduralTrackGenerator>();
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
    }
}
