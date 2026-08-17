using NUnit.Framework;

public class GhostTraceTests
{
    [Test]
    public void AnEmptyTraceHasNothingToReplay()
    {
        var trace = new GhostTrace();

        Assert.IsFalse(trace.HasData);
        Assert.AreEqual(0f, trace.DistanceAt(0f));
        Assert.AreEqual(0f, trace.DistanceAt(100f));
    }

    [Test]
    public void ItReadsBackTheDistanceItRecorded()
    {
        var trace = new GhostTrace { Times = new[] { 0f, 1f, 2f } };

        Assert.AreEqual(0f, trace.DistanceAt(0f), 0.001f);
        Assert.AreEqual(GhostTrace.Spacing, trace.DistanceAt(1f), 0.001f);
        Assert.AreEqual(GhostTrace.Spacing * 2f, trace.DistanceAt(2f), 0.001f);
    }

    [Test]
    public void ItInterpolatesBetweenSamples()
    {
        var trace = new GhostTrace { Times = new[] { 0f, 1f, 2f } };
        Assert.AreEqual(GhostTrace.Spacing * 0.5f, trace.DistanceAt(0.5f), 0.001f);
        Assert.AreEqual(GhostTrace.Spacing * 1.5f, trace.DistanceAt(1.5f), 0.001f);
    }

    // Outliving your last attempt is how you shake it off, so it must not keep going.
    [Test]
    public void ItStopsWhereTheRecordingStopped()
    {
        var trace = new GhostTrace { Times = new[] { 0f, 1f, 2f } };

        Assert.AreEqual(trace.TotalDistance, trace.DistanceAt(2f), 0.001f);
        Assert.AreEqual(trace.TotalDistance, trace.DistanceAt(60f), 0.001f);
    }

    [Test]
    public void ItNeverGoesBackwards()
    {
        var trace = new GhostTrace { Times = new[] { 0f, 0.5f, 1.7f, 1.9f, 4f } };
        var last = -1f;

        for (var t = 0f; t < 5f; t += 0.01f)
        {
            var d = trace.DistanceAt(t);
            Assert.GreaterOrEqual(d, last, "went backwards at t=" + t);
            last = d;
        }
    }

    [Test]
    public void RecordingASteadyPaceGivesEvenSamples()
    {
        var recorder = new GhostRecorder();

        for (var step = 1; step <= 100; step++)
        {
            var time = step * 0.1f;
            recorder.Sample(time, time * 10f); // 10 units per second
        }

        var trace = recorder.Build();

        Assert.IsTrue(trace.HasData);
        Assert.AreEqual(100f, trace.TotalDistance, GhostTrace.Spacing);
        Assert.AreEqual(5f, trace.DistanceAt(0.5f), 0.5f);
    }

    [Test]
    public void ASlowerRunTakesLongerToCoverTheSameGround()
    {
        var fast = new GhostRecorder();
        var slow = new GhostRecorder();

        for (var step = 1; step <= 100; step++)
        {
            fast.Sample(step * 0.1f, step * 0.1f * 20f);
            slow.Sample(step * 0.1f, step * 0.1f * 5f);
        }

        var atFive = fast.Build().DistanceAt(5f);
        Assert.Greater(atFive, slow.Build().DistanceAt(5f), "the faster ghost should be further along");
    }

    [Test]
    public void JumpingSeveralMarksInOneFrameStillRecordsThemAll()
    {
        var recorder = new GhostRecorder();
        recorder.Sample(1f, GhostTrace.Spacing * 5f);

        var trace = recorder.Build();
        Assert.AreEqual(GhostTrace.Spacing * 5f, trace.TotalDistance, 0.001f);
    }

    [Test]
    public void ItStopsGrowingAtTheSampleCap()
    {
        var recorder = new GhostRecorder();
        recorder.Sample(1f, GhostTrace.Spacing * (GhostTrace.MaxSamples + 500));

        Assert.LessOrEqual(recorder.Count, GhostTrace.MaxSamples);
    }

    // Keeping the last run instead would mean dying early hands the next run a trivial
    // pursuer, which makes it easier to die early again.
    [Test]
    public void TheBetterRunBecomesTheGhost()
    {
        var far = new GhostTrace { Times = new[] { 0f, 1f, 2f, 3f, 4f } };
        var near = new GhostTrace { Times = new[] { 0f, 1f } };

        Assert.AreSame(far, GhostTrace.Best(far, near), "a worse run must not replace the ghost");
        Assert.AreSame(far, GhostTrace.Best(near, far), "a better run should replace it");
    }

    [Test]
    public void AnythingBeatsNothing()
    {
        var run = new GhostTrace { Times = new[] { 0f, 1f } };

        Assert.AreSame(run, GhostTrace.Best(null, run));
        Assert.AreSame(run, GhostTrace.Best(new GhostTrace(), run));
    }

    [Test]
    public void AnEmptyRunNeverReplacesARealOne()
    {
        var run = new GhostTrace { Times = new[] { 0f, 1f } };

        Assert.AreSame(run, GhostTrace.Best(run, null));
        Assert.AreSame(run, GhostTrace.Best(run, new GhostTrace()));
    }

    [Test]
    public void ResetClearsTheRecording()
    {
        var recorder = new GhostRecorder();
        recorder.Sample(1f, 50f);
        recorder.Reset();

        Assert.IsFalse(recorder.Build().HasData);
    }
}
