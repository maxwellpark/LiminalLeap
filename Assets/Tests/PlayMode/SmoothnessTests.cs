using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SmoothnessTests
{
    private RunFixture fixture;

    [SetUp]
    public void SetUp()
    {
        RunFixture.IsolateFlags();
        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
    }

    // Boundary frames used to move short and read as jitter.
    [UnityTest]
    public IEnumerator SpeedMatchesCommandedEveryFrame()
    {
        // let the run settle past the spawn and the first piece
        var settle = Time.time + 1.5f;
        while (Time.time < settle)
        {
            fixture.Input.Tick();
            yield return null;
        }

        var worstRatio = 1f;
        var shortFrames = 0;
        var frames = 0;

        var until = Time.time + 4f;
        var previous = fixture.Player.transform.position;

        while (Time.time < until)
        {
            fixture.Input.Tick();
            yield return null;

            var now = fixture.Player.transform.position;
            var dt = Time.deltaTime;
            if (dt <= 0f)
            {
                previous = now;
                continue;
            }

            // horizontal only: bob moves y and would count as motion
            var moved = new Vector2(now.x - previous.x, now.z - previous.z).magnitude;
            var commanded = PlayerTrackMovement.CurrentSpeed * dt;
            previous = now;

            if (commanded <= 0.0001f)
            {
                continue;
            }

            var ratio = moved / commanded;
            frames++;

            if (ratio < 0.9f)
            {
                shortFrames++;
                worstRatio = Mathf.Min(worstRatio, ratio);
            }
        }

        Debug.Log($"SMOOTHNESS frames={frames} short={shortFrames} worst={worstRatio:F3}");
        Assert.Greater(frames, 30, "not enough samples");
        Assert.AreEqual(0, shortFrames,
            $"{shortFrames}/{frames} frames moved short of commanded speed, worst {worstRatio:P0}");
    }

    // Rotating per spill step turned twice on boundary frames.
    [UnityTest]
    public IEnumerator HeadingNeverTurnsFasterThanAllowed()
    {
        // Easing settles around 42 deg/s on a 7 degree seam. Past 120 is a snap.
        const float sane = 120f;

        var settle = Time.time + 1.5f;
        while (Time.time < settle)
        {
            fixture.Input.Tick();
            yield return null;
        }

        var worst = 0f;
        var spikes = 0;
        var frames = 0;
        var previous = fixture.Player.transform.rotation;

        var until = Time.time + 4f;
        while (Time.time < until)
        {
            fixture.Input.Tick();
            yield return null;

            var now = fixture.Player.transform.rotation;
            var dt = Time.deltaTime;
            if (dt <= 0f)
            {
                previous = now;
                continue;
            }

            var degrees = Quaternion.Angle(previous, now);
            previous = now;
            frames++;

            var rate = degrees / dt;
            if (rate > sane)
            {
                spikes++;
                worst = Mathf.Max(worst, rate);
            }
        }

        Debug.Log($"HEADING frames={frames} spikes={spikes} worst={worst:F0} deg/s");
        Assert.Greater(frames, 30, "not enough samples");
        Assert.AreEqual(0, spikes, $"{spikes}/{frames} frames turned faster than {sane} deg/s, worst {worst:F0}");
    }
}
