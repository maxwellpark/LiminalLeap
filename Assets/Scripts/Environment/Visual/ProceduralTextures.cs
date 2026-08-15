using System;

// Greyscale maths, no Unity texture types, so it's unit testable like Synth.
public static class ProceduralTextures
{
    // Value noise: hashed lattice, smoothstepped between corners. Deterministic per seed.
    public static float[] Noise(int size, int seed, int cells, int octaves = 4)
    {
        size = Math.Max(2, size);
        cells = Math.Max(1, cells);
        octaves = Math.Max(1, octaves);

        var pixels = new float[size * size];
        var total = 0f;
        var amplitude = 1f;
        var frequency = cells;

        for (var o = 0; o < octaves; o++)
        {
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    pixels[y * size + x] += Sample(x / (float)size * frequency, y / (float)size * frequency, seed + o) * amplitude;
                }
            }

            total += amplitude;
            amplitude *= 0.5f;
            frequency *= 2;
        }

        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] /= total;
        }

        return pixels;
    }

    // Dark lines on cell boundaries. Reads as panelling and gives speed something to bite on.
    public static float[] Grid(int size, int cells, float lineWidth)
    {
        size = Math.Max(2, size);
        cells = Math.Max(1, cells);
        lineWidth = Clamp01(lineWidth);

        var pixels = new float[size * size];

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var u = x / (float)size * cells;
                var v = y / (float)size * cells;
                var edge = Math.Min(Fract(u), Fract(v));
                var isLine = edge < lineWidth * 0.5f;
                pixels[y * size + x] = isLine ? 0f : 1f;
            }
        }

        return pixels;
    }

    public static float[] Multiply(float[] a, float[] b, float bStrength)
    {
        var pixels = new float[a.Length];
        for (var i = 0; i < a.Length; i++)
        {
            var mask = 1f - bStrength + b[i % b.Length] * bStrength;
            pixels[i] = Clamp01(a[i] * mask);
        }

        return pixels;
    }

    public static float[] Remap(float[] source, float low, float high)
    {
        var pixels = new float[source.Length];
        for (var i = 0; i < source.Length; i++)
        {
            pixels[i] = low + Clamp01(source[i]) * (high - low);
        }

        return pixels;
    }

    private static float Sample(float x, float y, int seed)
    {
        var xi = (int)Math.Floor(x);
        var yi = (int)Math.Floor(y);
        var xf = Smooth(x - xi);
        var yf = Smooth(y - yi);

        var a = Hash(xi, yi, seed);
        var b = Hash(xi + 1, yi, seed);
        var c = Hash(xi, yi + 1, seed);
        var d = Hash(xi + 1, yi + 1, seed);

        return Lerp(Lerp(a, b, xf), Lerp(c, d, xf), yf);
    }

    private static float Hash(int x, int y, int seed)
    {
        var h = unchecked((uint)(x * 374761393 + y * 668265263 + seed * 2246822519));
        h = (h ^ (h >> 13)) * 1274126177u;
        return (h ^ (h >> 16)) / (float)uint.MaxValue;
    }

    private static float Smooth(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private static float Fract(float v)
    {
        return v - (float)Math.Floor(v);
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
