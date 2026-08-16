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
        fixture = new RunFixture();
        fixture.Build();
    }

    [TearDown]
    public void TearDown()
    {
        fixture.Teardown();
    }

    // Actual movement should match commanded speed every frame. MoveTowards clamping at a
    // piece boundary drops the surplus, so boundary frames move short and it reads as jitter.
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
}
