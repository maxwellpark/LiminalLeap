using System;

// Pure buffer DSP, no Unity audio types, so the greybox sounds can be unit tested.
public static class Synth
{
    public const int SampleRate = 44100;

    public static int Samples(float seconds)
    {
        return Math.Max(1, (int)(seconds * SampleRate));
    }

    // Phase is accumulated rather than recomputed per sample, or the sweep clicks.
    public static float[] Sweep(float startHz, float endHz, float seconds, float decay)
    {
        var buf = new float[Samples(seconds)];
        double phase = 0d;

        for (var i = 0; i < buf.Length; i++)
        {
            var t = (float)i / buf.Length;
            var hz = startHz + (endHz - startHz) * t;
            phase += 2d * Math.PI * hz / SampleRate;
            buf[i] = (float)Math.Sin(phase) * (float)Math.Exp(-decay * t);
        }

        return buf;
    }

    public static float[] Noise(float seconds, int seed, float cutoff)
    {
        var buf = new float[Samples(seconds)];
        var state = seed == 0 ? 1u : (uint)seed;
        var prev = 0f;

        for (var i = 0; i < buf.Length; i++)
        {
            var white = NextFloat(ref state);
            prev += cutoff * (white - prev); // one-pole lowpass, harsh white is unpleasant
            buf[i] = prev;
        }

        return buf;
    }

    // Slow amplitude drift. Static noise reads as tape hiss; movement reads as air.
    public static void Modulate(float[] buf, float rateHz, float depth, float phase)
    {
        for (var i = 0; i < buf.Length; i++)
        {
            var t = (float)i / SampleRate;
            var lfo = (float)Math.Sin(2d * Math.PI * rateHz * t + phase);
            buf[i] *= 1f - depth + depth * (lfo * 0.5f + 0.5f);
        }
    }

    public static void Decay(float[] buf, float amount)
    {
        for (var i = 0; i < buf.Length; i++)
        {
            buf[i] *= (float)Math.Exp(-amount * ((float)i / buf.Length));
        }
    }

    // Without this every one-shot starts and ends on a step, which is an audible click.
    public static void Fade(float[] buf, int fadeSamples)
    {
        var n = Math.Min(fadeSamples, buf.Length / 2);
        for (var i = 0; i < n; i++)
        {
            var g = (float)i / n;
            buf[i] *= g;
            buf[buf.Length - 1 - i] *= g;
        }
    }

    public static void Normalise(float[] buf, float peak)
    {
        var max = 0f;
        for (var i = 0; i < buf.Length; i++)
        {
            var abs = Math.Abs(buf[i]);
            if (abs > max)
            {
                max = abs;
            }
        }

        if (max <= float.Epsilon)
        {
            return;
        }

        var scale = peak / max;
        for (var i = 0; i < buf.Length; i++)
        {
            buf[i] *= scale;
        }
    }

    public static float[] Mix(float[] a, float gainA, float[] b, float gainB)
    {
        var buf = new float[Math.Max(a.Length, b.Length)];
        for (var i = 0; i < buf.Length; i++)
        {
            var av = i < a.Length ? a[i] * gainA : 0f;
            var bv = i < b.Length ? b[i] * gainB : 0f;
            buf[i] = av + bv;
        }

        return buf;
    }

    // Crossfades the dropped tail over the head and shortens the buffer by that much,
    // so the last sample flows into the first and a looping clip doesn't tick each cycle.
    public static float[] MakeSeamless(float[] buf, int crossfadeSamples)
    {
        var n = Math.Min(crossfadeSamples, buf.Length / 2);
        if (n <= 0)
        {
            return buf;
        }

        var length = buf.Length - n;
        var result = new float[length];
        Array.Copy(buf, result, length);

        for (var i = 0; i < n; i++)
        {
            var g = (float)i / n;
            result[i] = result[i] * g + buf[length + i] * (1f - g);
        }

        return result;
    }

    private static float NextFloat(ref uint state)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        return state / (float)uint.MaxValue * 2f - 1f;
    }
}
