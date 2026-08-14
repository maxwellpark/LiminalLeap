using System.Collections.Generic;
using UnityEngine;

// Greybox audio, synthesised at load. Swap for AuthoredAudioLibrary once there are wavs.
public class ProceduralAudioLibrary : IAudioLibrary
{
    private readonly Dictionary<Sound, AudioClip> cache = new();

    public AudioClip Get(Sound sound)
    {
        if (!cache.TryGetValue(sound, out var clip))
        {
            var samples = Recipe(sound);
            clip = AudioClip.Create(sound.ToString(), samples.Length, 1, Synth.SampleRate, false);
            clip.SetData(samples, 0);
            cache[sound] = clip;
        }

        return clip;
    }

    // Separate from clip creation so the samples can be tested without Unity audio.
    public static float[] Recipe(Sound sound)
    {
        switch (sound)
        {
            case Sound.Jump:
                return Shaped(Synth.Sweep(240f, 680f, 0.18f, 3.5f), 0.7f);

            case Sound.Land:
            {
                var thud = Synth.Sweep(140f, 60f, 0.16f, 6f);
                var dust = Synth.Noise(0.1f, 7, 0.25f);
                Synth.Decay(dust, 9f);
                return Shaped(Synth.Mix(thud, 0.85f, dust, 0.35f), 0.75f);
            }

            case Sound.Pickup:
            {
                var low = Synth.Sweep(880f, 880f, 0.06f, 4f);
                var high = Synth.Sweep(1320f, 1320f, 0.09f, 5f);
                var buf = new float[low.Length + high.Length];
                low.CopyTo(buf, 0);
                high.CopyTo(buf, low.Length);
                return Shaped(buf, 0.6f);
            }

            case Sound.Death:
            {
                var fall = Synth.Sweep(420f, 55f, 0.9f, 2.2f);
                var hiss = Synth.Noise(0.9f, 13, 0.12f);
                Synth.Decay(hiss, 2.5f);
                return Shaped(Synth.Mix(fall, 0.8f, hiss, 0.4f), 0.8f);
            }

            case Sound.Wind:
            {
                // low rumble under filtered noise, looped, so speed has something to ride
                var air = Synth.Noise(2.2f, 29, 0.06f);
                var rumble = Synth.Sweep(70f, 70f, 2.2f, 0f);
                var buf = Synth.Mix(air, 0.9f, rumble, 0.15f);
                Synth.Normalise(buf, 0.55f);
                return Synth.MakeSeamless(buf, Synth.Samples(0.25f));
            }

            default:
                return new float[Synth.Samples(0.01f)];
        }
    }

    private static float[] Shaped(float[] buf, float peak)
    {
        Synth.Normalise(buf, peak);
        Synth.Fade(buf, Synth.Samples(0.004f));
        return buf;
    }
}
