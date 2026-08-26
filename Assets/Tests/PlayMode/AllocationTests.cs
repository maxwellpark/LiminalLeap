using System.Collections;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

// Per frame garbage is what makes a WebGL build hitch, and it is invisible until measured.
//
// Everything here is measured against a control: the test runner allocates around 4 KB a
// frame all by itself, so an absolute number would have read as a serious problem when
// almost none of it was ours. Each test subtracts a baseline taken in the same run, which
// also keeps the budget meaningful across machines and Unity versions.
public class AllocationTests
{
    private const int WarmUpFrames = 40;
    private const int SampleFrames = 120;

    // Enough to catch an array or a closure per frame, loose enough not to flake.
    private const long BudgetOverBaseline = 2048;

    private RunFixture fixture;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags(attacks: true);
        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
        Features.ClearOverrides();
    }

    private IEnumerator Frames(int count)
    {
        for (var i = 0; i < count; i++)
        {
            fixture.Input.Tick();
            yield return null;
        }
    }

    // Result lands in the box so the caller can read it back out of a coroutine.
    private IEnumerator Sample(long[] into)
    {
        var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

        if (!recorder.Valid)
        {
            recorder.Dispose();
            into[0] = -1;
            yield break;
        }

        long total = 0;
        var counted = 0;

        for (var i = 0; i < SampleFrames; i++)
        {
            fixture.Input.Tick();
            yield return null;

            var value = recorder.LastValue;
            if (value < 0)
            {
                continue;
            }

            total += value;
            counted++;
        }

        recorder.Dispose();
        into[0] = counted > 0 ? total / counted : 0;
    }

    private IEnumerator CompareToBaseline(string label)
    {
        // Warm up first: lazy caches allocate once and would be blamed on the steady state.
        yield return Frames(WarmUpFrames);

        var measured = new long[1];
        yield return Sample(measured);

        if (measured[0] < 0)
        {
            Assert.Ignore("GC recorder unavailable on this platform");
            yield break;
        }

        // Same run, same machine, nothing of ours alive.
        fixture.Teardown();
        yield return Frames(10);

        var baseline = new long[1];
        yield return Sample(baseline);

        var ours = measured[0] - baseline[0];
        Debug.Log($"ALLOC {label}: {measured[0]} B/frame, baseline {baseline[0]}, ours {ours}");

        Assert.Less(ours, BudgetOverBaseline,
            $"{label} allocates {ours} B/frame above an empty scene, which is what makes a web build hitch");
    }

    [UnityTest]
    public IEnumerator TheRunLoopStaysCheapPerFrame()
    {
        yield return CompareToBaseline("run loop");
    }

    // The presenter drives eight parts a frame for the length of every telegraph, so it is
    // the likeliest place for a per frame allocation to reappear.
    [UnityTest]
    public IEnumerator AnAttackTelegraphStaysCheapPerFrame()
    {
        yield return Frames(10);

        var pursuer = Pursuer.GetInstance();
        pursuer.ForceAttack(AttackLane.Centre);
        pursuer.AttackFrozen = true; // hold it in telegraph for the whole sample

        yield return CompareToBaseline("attack telegraph");
    }

    // Spawning used to fetch a piece's resettables with GetComponentsInChildren, which
    // allocates an array every time, and spawns come faster the quicker you run.
    [UnityTest]
    public IEnumerator NoFrameSpikesWhilePiecesRecycle()
    {
        yield return Frames(WarmUpFrames);

        var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        if (!recorder.Valid)
        {
            recorder.Dispose();
            Assert.Ignore("GC recorder unavailable on this platform");
            yield break;
        }

        long worst = 0;
        for (var i = 0; i < SampleFrames; i++)
        {
            fixture.Input.Tick();
            yield return null;
            worst = System.Math.Max(worst, recorder.LastValue);
        }

        recorder.Dispose();
        Debug.Log($"ALLOC recycling: worst frame {worst} B");

        // A spike means a spawn allocated, which is the shape of the old bug.
        Assert.Less(worst, 32768, "a single frame spiked, which usually means a spawn allocated");
    }
}
