using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TrackSceneryTests
{
    private readonly List<GameObject> made = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var go in made)
        {
            Object.DestroyImmediate(go);
        }

        made.Clear();
    }

    // Pillars in two lanes either side, as the prefab builder lays them.
    private TrackScenery Make()
    {
        var root = new GameObject("Piece");
        made.Add(root);
        var holder = new GameObject(TrackScenery.PillarHolder).transform;
        holder.SetParent(root.transform, false);
        foreach (var x in new[] { -3.5f, 3.5f })
        {
            for (var z = 0; z < 4; z++)
            {
                var pillar = new GameObject("Pillar").transform;
                pillar.SetParent(holder, false);
                pillar.localPosition = new Vector3(x, 4f, z * 2.5f);
                pillar.localScale = new Vector3(0.6f, 8f, 0.6f);
            }
        }

        return root.AddComponent<TrackScenery>();
    }

    private static Transform Pillar(TrackScenery scenery, int i)
    {
        return scenery.transform.Find(TrackScenery.PillarHolder).GetChild(i);
    }

    [Test]
    public void AnEchoCopiesOneSideAndLeavesTheOther()
    {
        var fork = Make();
        var before = Make();
        var echoed = Make();
        fork.Vary(11);
        before.Vary(11);
        echoed.Vary(500);

        fork.Echo(500, -1);

        var differ = false;
        for (var i = 0; i < 4; i++)
        {
            differ |= Pillar(before, i).gameObject.activeSelf != Pillar(echoed, i).gameObject.activeSelf
                || Pillar(before, i).localScale != Pillar(echoed, i).localScale;
        }

        Assert.IsTrue(differ, "the two seeds stand the left lane alike, so this proves nothing");

        for (var i = 0; i < 8; i++)
        {
            var left = Pillar(fork, i).localPosition.x < 0f;
            var expected = Pillar(left ? echoed : before, i);
            Assert.AreEqual(expected.gameObject.activeSelf, Pillar(fork, i).gameObject.activeSelf, "pillar " + i);
            if (expected.gameObject.activeSelf)
            {
                Assert.AreEqual(expected.localScale, Pillar(fork, i).localScale, "pillar " + i);
            }
        }
    }

    // Otherwise a pooled fork piece would keep its echo when laid as ordinary track.
    [Test]
    public void TheNextVaryClearsAnEcho()
    {
        var fork = Make();
        var plain = Make();
        fork.Echo(500, 1);

        fork.Vary(11);
        plain.Vary(11);

        for (var i = 0; i < 8; i++)
        {
            Assert.AreEqual(Pillar(plain, i).gameObject.activeSelf, Pillar(fork, i).gameObject.activeSelf, "pillar " + i);
            if (Pillar(plain, i).gameObject.activeSelf)
            {
                Assert.AreEqual(Pillar(plain, i).localScale, Pillar(fork, i).localScale, "pillar " + i);
            }
        }
    }
}
