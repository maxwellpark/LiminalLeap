using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PickupBobTests
{
    // The player's trigger is a 1 unit box at its feet, so anything outside this band is
    // uncollectable. Pickups being a hair out of reach has already been a bug once.
    private const float ReachBottom = 0f;
    private const float ReachTop = 1f;

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
        Features.ClearOverrides();
    }

    [UnityTest]
    public IEnumerator BobbingNeverTakesAPickupOutOfReach()
    {
        var pickup = fixture.AddPickup(0f, 2);
        pickup.gameObject.AddComponent<PickupBob>();

        var collider = pickup.GetComponent<Collider>();
        var elapsed = 0f;

        // A few cycles, so the top and bottom of the travel are both sampled.
        while (elapsed < 3f)
        {
            var bounds = collider.bounds;

            Assert.Less(bounds.min.y, ReachTop, "pickup floated above the player's reach");
            Assert.Greater(bounds.max.y, ReachBottom, "pickup sank below the player's reach");

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator ItActuallyMoves()
    {
        var pickup = fixture.AddPickup(0f, 2);
        pickup.gameObject.AddComponent<PickupBob>();
        yield return null;

        var start = pickup.transform.position.y;
        var moved = false;
        var elapsed = 0f;

        while (elapsed < 2f && !moved)
        {
            moved = !Mathf.Approximately(pickup.transform.position.y, start);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(moved, "a pickup that does not move reads as scenery, which is the point of this");
    }

    [UnityTest]
    public IEnumerator RecyclingPutsItBackWhereItStarted()
    {
        var pickup = fixture.AddPickup(0f, 2);
        var bob = pickup.gameObject.AddComponent<PickupBob>();
        var home = pickup.transform.localPosition;

        yield return null;
        yield return null;

        bob.ResetForNewRun();

        Assert.AreEqual(home.y, pickup.transform.localPosition.y, 0.0001f,
            "pooling would otherwise drift them a little further each run");
    }
}
