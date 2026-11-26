using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Stand-ins until generated models replace them: +Z front, pivot at the base, metres, no collider.
public static class PropPrefabs
{
    private const string Root = "Assets/Resources/Art/Places";
    private const string MaterialFolder = "Assets/Art/Props/Materials";

    private static readonly Dictionary<string, Material> Materials = new();

    [MenuItem("Liminal Leap/Generate Stand-in Props")]
    public static void GenerateFromCommandLine()
    {
        Directory.CreateDirectory(MaterialFolder);
        Materials.Clear();

        Save("office", "WaterCooler", WaterCooler());
        Save("office", "FilingCabinet", Cabinet("FilingCabinet", 0.5f, 1.3f, 0.6f, 4, "Steel", new Color(0.55f, 0.57f, 0.58f)));
        Save("office", "Plant", Plant());
        Save("office", "Photocopier", Photocopier());

        Save("school", "Lockers", Lockers());
        Save("school", "Extinguisher", Extinguisher());
        Save("school", "NoticeBoard", NoticeBoard());
        Save("school", "Bin", Bin());

        Save("house", "SideTable", SideTable());
        Save("house", "CoatStand", CoatStand());
        Save("house", "Armchair", Armchair());
        Save("house", "Radiator", Radiator());

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PROPS GENERATED in " + Root);
    }

    private static GameObject WaterCooler()
    {
        var root = new GameObject("WaterCooler");
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), new Vector3(0.32f, 1f, 0.32f), "Plastic", new Color(0.86f, 0.86f, 0.84f));
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 1.25f, 0f), new Vector3(0.28f, 0.25f, 0.28f), "Water", new Color(0.55f, 0.72f, 0.85f));
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.75f, 0.17f), new Vector3(0.12f, 0.06f, 0.04f), "Tap", new Color(0.2f, 0.35f, 0.7f));
        return root;
    }

    private static GameObject Cabinet(string name, float wide, float high, float deep, int drawers, string finish, Color colour)
    {
        var root = new GameObject(name);
        Part(root, PrimitiveType.Cube, new Vector3(0f, high * 0.5f, 0f), new Vector3(wide, high, deep), finish, colour);

        // A dark gap and a handle per drawer, on the front face.
        var drawer = high / drawers;
        for (var i = 0; i < drawers; i++)
        {
            Part(root, PrimitiveType.Cube, new Vector3(0f, (i + 1) * drawer, deep * 0.5f), new Vector3(wide * 0.96f, 0.015f, 0.01f), "Shadow", new Color(0.12f, 0.12f, 0.13f));
            Part(root, PrimitiveType.Cube, new Vector3(0f, (i + 0.6f) * drawer, deep * 0.5f + 0.01f), new Vector3(0.14f, 0.025f, 0.02f), "Handle", new Color(0.3f, 0.3f, 0.32f));
        }

        return root;
    }

    private static GameObject Plant()
    {
        var root = new GameObject("Plant");
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.2f, 0f), new Vector3(0.4f, 0.2f, 0.4f), "Pot", new Color(0.45f, 0.3f, 0.22f));
        Part(root, PrimitiveType.Sphere, new Vector3(0f, 0.75f, 0f), new Vector3(0.7f, 0.8f, 0.7f), "Leaves", new Color(0.22f, 0.38f, 0.2f));
        Part(root, PrimitiveType.Sphere, new Vector3(0.12f, 1.1f, 0.05f), new Vector3(0.45f, 0.5f, 0.45f), "Leaves", new Color(0.22f, 0.38f, 0.2f));
        return root;
    }

    private static GameObject Photocopier()
    {
        var root = new GameObject("Photocopier");
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.45f, 0f), new Vector3(0.95f, 0.9f, 0.65f), "Plastic", new Color(0.86f, 0.86f, 0.84f));
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.95f, -0.02f), new Vector3(0.9f, 0.1f, 0.6f), "Lid", new Color(0.32f, 0.33f, 0.35f));
        Part(root, PrimitiveType.Cube, new Vector3(0.25f, 0.86f, 0.33f), new Vector3(0.3f, 0.04f, 0.04f), "Panel", new Color(0.15f, 0.3f, 0.25f));
        return root;
    }

    private static GameObject Lockers()
    {
        var root = new GameObject("Lockers");
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.9f, 0f), new Vector3(1.2f, 1.8f, 0.45f), "Locker", new Color(0.36f, 0.5f, 0.45f));
        for (var i = 1; i < 3; i++)
        {
            Part(root, PrimitiveType.Cube, new Vector3(-0.6f + i * 0.4f, 0.9f, 0.23f), new Vector3(0.015f, 1.76f, 0.01f), "Shadow", new Color(0.12f, 0.12f, 0.13f));
        }

        for (var i = 0; i < 3; i++)
        {
            Part(root, PrimitiveType.Cube, new Vector3(-0.4f + i * 0.4f, 1.55f, 0.23f), new Vector3(0.22f, 0.08f, 0.01f), "Shadow", new Color(0.12f, 0.12f, 0.13f));
        }

        return root;
    }

    private static GameObject Extinguisher()
    {
        var root = new GameObject("Extinguisher");
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.05f, 0f), new Vector3(0.3f, 0.1f, 0.3f), "Stand", new Color(0.25f, 0.25f, 0.27f));
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(0.18f, 0.3f, 0.18f), "Red", new Color(0.72f, 0.1f, 0.08f));
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.75f, 0.04f), new Vector3(0.06f, 0.08f, 0.12f), "Shadow", new Color(0.12f, 0.12f, 0.13f));
        return root;
    }

    private static GameObject NoticeBoard()
    {
        var root = new GameObject("NoticeBoard");
        Part(root, PrimitiveType.Cube, new Vector3(-0.5f, 0.7f, 0f), new Vector3(0.05f, 1.4f, 0.05f), "Steel", new Color(0.55f, 0.57f, 0.58f));
        Part(root, PrimitiveType.Cube, new Vector3(0.5f, 0.7f, 0f), new Vector3(0.05f, 1.4f, 0.05f), "Steel", new Color(0.55f, 0.57f, 0.58f));
        Part(root, PrimitiveType.Cube, new Vector3(0f, 1.25f, 0f), new Vector3(1.1f, 0.75f, 0.04f), "Cork", new Color(0.6f, 0.45f, 0.3f));

        // A few pinned sheets, pale against the cork.
        var sheets = new[] { new Vector3(-0.3f, 1.35f), new Vector3(0.05f, 1.2f), new Vector3(0.3f, 1.38f) };
        foreach (var at in sheets)
        {
            Part(root, PrimitiveType.Cube, new Vector3(at.x, at.y, 0.025f), new Vector3(0.21f, 0.29f, 0.005f), "Paper", new Color(0.9f, 0.9f, 0.86f));
        }

        return root;
    }

    private static GameObject Bin()
    {
        var root = new GameObject("Bin");
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.3f, 0f), new Vector3(0.36f, 0.3f, 0.36f), "Steel", new Color(0.55f, 0.57f, 0.58f));
        return root;
    }

    private static GameObject SideTable()
    {
        var root = new GameObject("SideTable");
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.55f, 0f), new Vector3(0.5f, 0.05f, 0.45f), "Wood", new Color(0.42f, 0.28f, 0.18f));
        foreach (var x in new[] { -0.21f, 0.21f })
        {
            foreach (var z in new[] { -0.18f, 0.18f })
            {
                Part(root, PrimitiveType.Cube, new Vector3(x, 0.27f, z), new Vector3(0.04f, 0.55f, 0.04f), "Wood", new Color(0.42f, 0.28f, 0.18f));
            }
        }

        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.75f, 0f), new Vector3(0.05f, 0.17f, 0.05f), "Brass", new Color(0.62f, 0.5f, 0.28f));
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.98f, 0f), new Vector3(0.32f, 0.11f, 0.32f), "Shade", new Color(0.88f, 0.8f, 0.62f));
        return root;
    }

    private static GameObject CoatStand()
    {
        var root = new GameObject("CoatStand");
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.02f, 0f), new Vector3(0.45f, 0.02f, 0.45f), "Wood", new Color(0.42f, 0.28f, 0.18f));
        Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.9f, 0f), new Vector3(0.05f, 0.9f, 0.05f), "Wood", new Color(0.42f, 0.28f, 0.18f));
        Part(root, PrimitiveType.Cube, new Vector3(0f, 1.7f, 0f), new Vector3(0.4f, 0.03f, 0.03f), "Wood", new Color(0.42f, 0.28f, 0.18f));
        Part(root, PrimitiveType.Cube, new Vector3(0f, 1.7f, 0f), new Vector3(0.03f, 0.03f, 0.4f), "Wood", new Color(0.42f, 0.28f, 0.18f));

        // A coat left behind, which is the point of the thing.
        Part(root, PrimitiveType.Cube, new Vector3(0.12f, 1.25f, 0f), new Vector3(0.08f, 0.8f, 0.4f), "Coat", new Color(0.22f, 0.24f, 0.3f));
        return root;
    }

    private static GameObject Armchair()
    {
        var root = new GameObject("Armchair");
        var fabric = new Color(0.45f, 0.36f, 0.3f);
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.22f, 0f), new Vector3(0.85f, 0.44f, 0.8f), "Fabric", fabric);
        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.65f, -0.33f), new Vector3(0.85f, 0.65f, 0.15f), "Fabric", fabric);
        Part(root, PrimitiveType.Cube, new Vector3(-0.37f, 0.55f, 0f), new Vector3(0.12f, 0.25f, 0.8f), "Fabric", fabric);
        Part(root, PrimitiveType.Cube, new Vector3(0.37f, 0.55f, 0f), new Vector3(0.12f, 0.25f, 0.8f), "Fabric", fabric);
        return root;
    }

    private static GameObject Radiator()
    {
        var root = new GameObject("Radiator");
        for (var i = 0; i < 10; i++)
        {
            Part(root, PrimitiveType.Cube, new Vector3(-0.45f + i * 0.1f, 0.4f, 0f), new Vector3(0.06f, 0.6f, 0.1f), "Paint", new Color(0.86f, 0.85f, 0.8f));
        }

        Part(root, PrimitiveType.Cube, new Vector3(0f, 0.08f, 0f), new Vector3(1f, 0.04f, 0.04f), "Paint", new Color(0.86f, 0.85f, 0.8f));
        return root;
    }

    private static void Part(GameObject root, PrimitiveType shape, Vector3 at, Vector3 size, string material, Color colour)
    {
        var part = GameObject.CreatePrimitive(shape);
        part.name = material;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        part.transform.SetParent(root.transform, false);
        part.transform.localPosition = at;
        part.transform.localScale = size;
        part.GetComponent<Renderer>().sharedMaterial = MaterialFor(material, colour);
    }

    // Kept when it already exists, so its GUID doesn't move. Paths use "/", which AssetDatabase expects.
    private static Material MaterialFor(string name, Color colour)
    {
        if (Materials.TryGetValue(name, out var cached))
        {
            return cached;
        }

        var path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Standard")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = colour;
        material.SetFloat("_Glossiness", 0.15f);
        EditorUtility.SetDirty(material);
        Materials[name] = material;
        return material;
    }

    private static void Save(string place, string name, GameObject root)
    {
        var folder = Root + "/" + place + "/Props";
        Directory.CreateDirectory(folder);
        PrefabUtility.SaveAsPrefabAsset(root, folder + "/" + name + ".prefab");
        Object.DestroyImmediate(root);
    }
}
