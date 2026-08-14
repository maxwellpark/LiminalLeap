using UnityEditor;
using UnityEngine;

// Drops a generator in. Piece prefabs and player still need assigning in the inspector.
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
