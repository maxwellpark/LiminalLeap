using System;

// Pure alpha ramp, no Unity texture types, so it's testable. No post-processing here.
public static class VignetteTexture
{
    // Row-major alphas, 0 in the clear centre rising to 1 at the corners.
    public static float[] Build(int size, float inner, float outer)
    {
        if (size < 2)
        {
            size = 2;
        }

        if (outer <= inner)
        {
            outer = inner + 0.0001f;
        }

        var alphas = new float[size * size];
        var centre = (size - 1) * 0.5f;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = (x - centre) / centre;
                var dy = (y - centre) / centre;
                var d = (float)Math.Sqrt(dx * dx + dy * dy);

                var t = Clamp01((d - inner) / (outer - inner));
                alphas[y * size + x] = t * t * (3f - 2f * t); // smoothstep, a linear ramp bands
            }
        }

        return alphas;
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
