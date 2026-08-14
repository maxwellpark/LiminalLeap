using System;
using NUnit.Framework;

public class SynthTests
{
    private static void AssertSane(float[] buf, string what)
    {
        Assert.Greater(buf.Length, 0, what + " is empty");
        for (var i = 0; i < buf.Length; i++)
        {
            Assert.IsFalse(float.IsNaN(buf[i]) || float.IsInfinity(buf[i]), what + " has a bad sample at " + i);
            Assert.LessOrEqual(Math.Abs(buf[i]), 1f, what + " clips at " + i);
        }
    }

    [Test]
    public void Samples_ScalesWithSampleRate()
    {
        Assert.AreEqual(Synth.SampleRate, Synth.Samples(1f));
        Assert.AreEqual(Synth.SampleRate / 2, Synth.Samples(0.5f));
    }

    [Test]
    public void Samples_NeverReturnsZero()
    {
        Assert.GreaterOrEqual(Synth.Samples(0f), 1);
    }

    [Test]
    public void Sweep_StaysInRangeAndDecays()
    {
        var buf = Synth.Sweep(200f, 800f, 0.2f, 4f);
        AssertSane(buf, "sweep");
        Assert.Less(Math.Abs(buf[buf.Length - 1]), Math.Abs(buf[buf.Length / 20]));
    }

    [Test]
    public void Sweep_IsContinuous()
    {
        // phase accumulation should mean no sample-to-sample jumps, which would click
        var buf = Synth.Sweep(100f, 4000f, 0.3f, 0f);
        for (var i = 1; i < buf.Length; i++)
        {
            Assert.Less(Math.Abs(buf[i] - buf[i - 1]), 0.75f, "discontinuity at " + i);
        }
    }

    [Test]
    public void Noise_IsDeterministicForASeed()
    {
        CollectionAssert.AreEqual(Synth.Noise(0.05f, 5, 0.3f), Synth.Noise(0.05f, 5, 0.3f));
    }

    [Test]
    public void Noise_DiffersBetweenSeeds()
    {
        CollectionAssert.AreNotEqual(Synth.Noise(0.05f, 5, 0.3f), Synth.Noise(0.05f, 6, 0.3f));
    }

    [Test]
    public void Noise_SurvivesAZeroSeed()
    {
        AssertSane(Synth.Noise(0.05f, 0, 0.3f), "zero-seed noise");
    }

    [Test]
    public void Fade_SilencesBothEnds()
    {
        var buf = Synth.Sweep(440f, 440f, 0.1f, 0f);
        Synth.Fade(buf, 64);
        Assert.AreEqual(0f, buf[0], 1e-6f);
        Assert.AreEqual(0f, buf[buf.Length - 1], 1e-6f);
    }

    [Test]
    public void Fade_HandlesAFadeLongerThanTheBuffer()
    {
        var buf = Synth.Sweep(440f, 440f, 0.001f, 0f);
        Assert.DoesNotThrow(() => Synth.Fade(buf, 100000));
        AssertSane(buf, "over-faded buffer");
    }

    [Test]
    public void Normalise_HitsTheRequestedPeak()
    {
        var buf = new[] { 0.1f, -0.2f, 0.05f };
        Synth.Normalise(buf, 0.8f);
        var max = 0f;
        foreach (var s in buf)
        {
            max = Math.Max(max, Math.Abs(s));
        }
        Assert.AreEqual(0.8f, max, 1e-5f);
    }

    [Test]
    public void Normalise_LeavesSilenceAlone()
    {
        var buf = new float[16];
        Assert.DoesNotThrow(() => Synth.Normalise(buf, 0.8f));
        CollectionAssert.AreEqual(new float[16], buf);
    }

    [Test]
    public void Mix_TakesTheLongerLength()
    {
        var mixed = Synth.Mix(new float[10], 1f, new float[25], 1f);
        Assert.AreEqual(25, mixed.Length);
    }

    [Test]
    public void MakeSeamless_ShortensByTheCrossfade()
    {
        var buf = Synth.Noise(0.5f, 3, 0.2f);
        var looped = Synth.MakeSeamless(buf, 1000);
        Assert.AreEqual(buf.Length - 1000, looped.Length);
    }

    [Test]
    public void MakeSeamless_JoinsEndToStart()
    {
        // the wrap point must not be a bigger step than the material either side of it
        var buf = Synth.Noise(0.5f, 11, 0.05f);
        var looped = Synth.MakeSeamless(buf, 2000);
        var seam = Math.Abs(looped[0] - looped[looped.Length - 1]);

        var typical = 0f;
        for (var i = 1; i < 200; i++)
        {
            typical = Math.Max(typical, Math.Abs(looped[i] - looped[i - 1]));
        }

        Assert.LessOrEqual(seam, Math.Max(typical * 4f, 0.05f), "loop point would tick");
    }

    [Test]
    public void EverySound_ProducesASaneBuffer()
    {
        foreach (Sound sound in Enum.GetValues(typeof(Sound)))
        {
            AssertSane(ProceduralAudioLibrary.Recipe(sound), sound.ToString());
        }
    }

    [Test]
    public void OneShots_AreShortEnoughToNotOverlapThemselves()
    {
        Assert.Less(ProceduralAudioLibrary.Recipe(Sound.Jump).Length, Synth.Samples(0.5f));
        Assert.Less(ProceduralAudioLibrary.Recipe(Sound.Pickup).Length, Synth.Samples(0.5f));
        Assert.Less(ProceduralAudioLibrary.Recipe(Sound.Land).Length, Synth.Samples(0.5f));
    }
}
