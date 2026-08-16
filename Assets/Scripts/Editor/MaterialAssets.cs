using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Bakes the generated materials to disk. Prefabs cannot reference a material created with
// new Material() at runtime, so saving piece prefabs dropped them and everything went purple.
public static class MaterialAssets
{
    public const string Folder = "Assets/Materials/Generated";

    [MenuItem("Liminal Leap/Generate Material Assets")]
    public static void GenerateFromCommandLine()
    {
        Directory.CreateDirectory(Folder);

        foreach (Surface surface in Enum.GetValues(typeof(Surface)))
        {
            Bake(surface);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("MATERIALS BAKED in " + Folder);
    }

    public static Material Load(Surface surface)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(PathFor(surface));
        return existing != null ? existing : Bake(surface);
    }

    private static Material Bake(Surface surface)
    {
        var path = PathFor(surface);
        var source = MaterialLibrary.Get(surface);

        var material = new Material(source);
        var texture = source.mainTexture as Texture2D;

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(material, path);

        // The texture has to live in the asset too, or the material points at nothing.
        if (texture != null)
        {
            var copy = new Texture2D(texture.width, texture.height, texture.format, true)
            {
                name = surface + "Tex",
                wrapMode = texture.wrapMode,
                filterMode = texture.filterMode,
            };
            Graphics.CopyTexture(texture, copy);
            AssetDatabase.AddObjectToAsset(copy, material);
            material.mainTexture = copy;
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static string PathFor(Surface surface)
    {
        return Path.Combine(Folder, surface + ".mat");
    }
}
