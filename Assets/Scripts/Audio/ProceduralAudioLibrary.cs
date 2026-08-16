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
                // Long loop, heavy filter, two odd-rate LFOs so it drifts instead of sitting flat.
                const float length = 7.3f;

                var air = Synth.Noise(length, 29, 0.012f);
                Synth.Modulate(air, 0.07f, 0.55f, 0f);

                var gust = Synth.Noise(length, 71, 0.05f);
                Synth.Modulate(gust, 0.031f, 0.8f, 2.1f);

                var drone = Synth.Sweep(48f, 48f, length, 0f);
                var detune = Synth.Sweep(48.7f, 48.7f, length, 0f);

                var buf = Synth.Mix(air, 1f, gust, 0.35f);
                buf = Synth.Mix(buf, 1f, Synth.Mix(drone, 0.5f, detune, 0.5f), 0.22f);
                Synth.Normalise(buf, 0.5f);
                return Synth.MakeSeamless(buf, Synth.Samples(0.6f));
            }

            case Sound.Dread:
            {
                // Slow pulse, ambiguous between a breath and machinery. Loops under everything
                // so the pursuer can be heard before the mirror confirms it.
                const float length = 4.4f;

                var sub = Synth.Sweep(41f, 41f, length, 0f);
                Synth.Modulate(sub, 0.42f, 0.85f, 0f);

                var body = Synth.Sweep(82f, 82f, length, 0f);
                Synth.Modulate(body, 0.42f, 0.9f, 0.6f);

                var breath = Synth.Noise(length, 53, 0.03f);
                Synth.Modulate(breath, 0.42f, 0.95f, 0.3f);

                var buf = Synth.Mix(Synth.Mix(sub, 0.6f, body, 0.3f), 1f, breath, 0.5f);
                Synth.Normalise(buf, 0.6f);
                return Synth.MakeSeamless(buf, Synth.Samples(0.5f));
            }

            case Sound.Success:
            {
                // Rising major arpeggio, the opposite shape to Death's falling sweep.
                var buf = new float[Synth.Samples(1.6f)];
                float[] notes = { 261.6f, 329.6f, 392f, 523.3f };

                for (var i = 0; i < notes.Length; i++)
                {
                    var note = Synth.Sweep(notes[i], notes[i], 1.2f, 3.4f);
                    var shimmer = Synth.Sweep(notes[i] * 2f, notes[i] * 2f, 0.8f, 5f);
                    Synth.MixAt(buf, note, Synth.Samples(0.09f * i), 0.55f);
                    Synth.MixAt(buf, shimmer, Synth.Samples(0.09f * i), 0.18f);
                }

                // Final octave held under it so the phrase resolves rather than just stopping.
                Synth.MixAt(buf, Synth.Sweep(523.3f, 523.3f, 1.4f, 1.8f), Synth.Samples(0.36f), 0.3f);

                return Shaped(buf, 0.75f);
            }

            case Sound.TitleSting:
            {
                // Minor triad, slow decay, one voice detuned so it beats rather than sits flat.
                var root = Synth.Sweep(110f, 110f, 2.4f, 1.1f);
                var third = Synth.Sweep(130.8f, 130.8f, 2.4f, 1.2f);
                var fifth = Synth.Sweep(164.8f, 164.8f, 2.4f, 1.3f);
                var detune = Synth.Sweep(110.4f, 110.4f, 2.4f, 1.1f);

                var air = Synth.Noise(2.4f, 91, 0.02f);
                Synth.Decay(air, 2f);

                var chord = Synth.Mix(Synth.Mix(root, 0.5f, third, 0.4f), 1f, Synth.Mix(fifth, 0.35f, detune, 0.3f), 1f);
                return Shaped(Synth.Mix(chord, 1f, air, 0.25f), 0.7f);
            }

            case Sound.MirrorUp:
            {
                // Short filtered swish, no tone, so it reads as movement not a UI beep.
                var air = Synth.Noise(0.22f, 131, 0.09f);
                Synth.Decay(air, 7f);
                var body = Synth.Sweep(180f, 420f, 0.22f, 6f);
                return Shaped(Synth.Mix(air, 0.8f, body, 0.25f), 0.4f);
            }

            case Sound.MirrorDown:
            {
                var air = Synth.Noise(0.18f, 137, 0.07f);
                Synth.Decay(air, 9f);
                var body = Synth.Sweep(360f, 150f, 0.18f, 7f);
                return Shaped(Synth.Mix(air, 0.7f, body, 0.25f), 0.35f);
            }

            case Sound.Confirm:
                return Shaped(Synth.Sweep(520f, 1040f, 0.16f, 4f), 0.55f);

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
