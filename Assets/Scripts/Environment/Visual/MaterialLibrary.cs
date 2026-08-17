using System.Collections.Generic;
using UnityEngine;

public enum Surface
{
    Track,
    Hazard,
    JumpBar,
    Pickup,
    Exit,
}

// Generated materials cached per surface. Built-in pipeline, so Standard shader.
public static class MaterialLibrary
{
    private const int Size = 256;

    private static readonly Dictionary<Surface, Material> Cache = new();

    public static Material Get(Surface surface)
    {
        if (Cache.TryGetValue(surface, out var existing) && existing != null)
        {
            return existing;
        }

        var material = Build(surface);
        Cache[surface] = material;
        return material;
    }

    // Editor domain reloads leave stale destroyed materials behind.
    public static void Clear()
    {
        Cache.Clear();
    }

    private static Material Build(Surface surface)
    {
        switch (surface)
        {
            case Surface.Track:
            {
                var grime = ProceduralTextures.Noise(Size, 11, 6);
                var panels = ProceduralTextures.Grid(Size, 8, 0.06f);
                var mixed = ProceduralTextures.Remap(ProceduralTextures.Multiply(grime, panels, 0.7f), 0.22f, 0.62f);
                return Make("TrackSurface", mixed, new Color(0.75f, 0.74f, 0.70f), 0.15f);
            }

            case Surface.Hazard:
            {
                var grime = ProceduralTextures.Noise(Size, 29, 10);
                var stripes = ProceduralTextures.Grid(Size, 4, 0.35f);
                var mixed = ProceduralTextures.Remap(ProceduralTextures.Multiply(grime, stripes, 0.55f), 0.3f, 1f);
                return Make("HazardSurface", mixed, new Color(0.85f, 0.28f, 0.2f), 0.2f);
            }

            case Surface.JumpBar:
            {
                var grime = ProceduralTextures.Noise(Size, 47, 8);
                var stripes = ProceduralTextures.Grid(Size, 6, 0.4f);
                var mixed = ProceduralTextures.Remap(ProceduralTextures.Multiply(grime, stripes, 0.6f), 0.35f, 1f);
                return Make("JumpBarSurface", mixed, new Color(0.95f, 0.75f, 0.2f), 0.25f);
            }

            case Surface.Exit:
            {
                // Exit sign green, emissive so it carries down a dim corridor.
                var glow = ProceduralTextures.Remap(ProceduralTextures.Noise(Size, 97, 3), 0.7f, 1f);
                var material = Make("ExitSurface", glow, new Color(0.35f, 0.95f, 0.5f), 0.35f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.25f, 0.8f, 0.4f));
                return material;
            }

            default:
            {
                var glow = ProceduralTextures.Remap(ProceduralTextures.Noise(Size, 71, 4), 0.6f, 1f);
                var material = Make("PickupSurface", glow, new Color(0.4f, 0.85f, 0.95f), 0.5f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 0.75f));
                return material;
            }
        }
    }

    private static Material Make(string name, float[] greyscale, Color tint, float smoothness)
    {
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, true)
        {
            name = name + "Tex",
            wrapMode = TextureWrapMode.Repeat,
        };

        var pixels = new Color32[greyscale.Length];
        for (var i = 0; i < greyscale.Length; i++)
        {
            var v = (byte)(Mathf.Clamp01(greyscale[i]) * 255f);
            pixels[i] = new Color32(v, v, v, 255);
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        var shader = Shader.Find("Standard");
        var material = new Material(shader) { name = name };
        material.mainTexture = texture;
        material.color = tint;

        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", smoothness);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        return material;
    }
}
