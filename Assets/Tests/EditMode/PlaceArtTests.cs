using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlaceArtTests
{
    private readonly List<Object> made = new();

    [TearDown]
    public void TearDown()
    {
        foreach (var o in made)
        {
            Object.DestroyImmediate(o);
        }

        made.Clear();
    }

    private TrackPiece Piece()
    {
        var go = new GameObject("Piece");
        made.Add(go);
        return go.AddComponent<TrackPiece>();
    }

    private Material Material()
    {
        var m = new Material(Shader.Find("Standard"));
        made.Add(m);
        return m;
    }

    [Test]
    public void APlaceWithNoFilesHasNoArt()
    {
        var nowhere = new Place { Id = "no-such-place" };

        Assert.IsNull(PlaceArt.Wall(nowhere));
        Assert.IsNull(PlaceArt.Ceiling(nowhere));
        Assert.AreEqual(0, PlaceArt.Props(nowhere).Length);
        Assert.IsNull(PlaceArt.Wall(null), "a free run wears no place");
    }

    [Test]
    public void WithNoArtAPieceGainsNothing()
    {
        var piece = Piece();
        var before = piece.transform.childCount;

        piece.Dress(null, null, System.Array.Empty<GameObject>(), 7);

        Assert.AreEqual(before, piece.transform.childCount);
    }

    [Test]
    public void WallsAndCeilingWearTheirArtAndCanNotBeHit()
    {
        var piece = Piece();
        var wall = Material();
        var ceiling = Material();

        piece.Dress(wall, ceiling, null, 7);

        foreach (var name in new[] { "WallLeft", "WallRight" })
        {
            var part = piece.transform.Find(name);
            Assert.IsTrue(part.gameObject.activeSelf, name);
            Assert.AreSame(wall, part.GetComponent<Renderer>().sharedMaterial, name);
            Assert.IsNull(part.GetComponent<Collider>(), name + " would kill or snag the player");
        }

        Assert.AreSame(ceiling, piece.transform.Find("Ceiling").GetComponent<Renderer>().sharedMaterial);
    }

    // A pooled piece laid next in a place without art mustn't keep the last one's walls.
    [Test]
    public void ArtGoesWhenTheNextLayingHasNone()
    {
        var piece = Piece();
        piece.Dress(Material(), Material(), null, 7);

        piece.Dress(null, null, null, 7);

        Assert.IsFalse(piece.transform.Find("WallLeft").gameObject.activeSelf);
        Assert.IsFalse(piece.transform.Find("Ceiling").gameObject.activeSelf);
    }

    // The seed decides, so a loop's replay stands the same prop on the same side.
    [Test]
    public void TheSameSeedStandsTheSameProp()
    {
        var props = new List<GameObject>();
        for (var i = 0; i < 3; i++)
        {
            var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prop.name = "Prop" + i;
            made.Add(prop);
            props.Add(prop);
        }

        var stood = 0;
        for (var seed = 0; seed < 60; seed++)
        {
            var a = Piece();
            var b = Piece();
            a.Dress(null, null, props.ToArray(), seed);
            b.Dress(null, null, props.ToArray(), seed);

            var shownA = Shown(a);
            var shownB = Shown(b);
            Assert.AreEqual(shownA == null, shownB == null, "seed " + seed);
            if (shownA != null)
            {
                stood++;
                Assert.AreEqual(shownA.name, shownB.name, "seed " + seed);
                Assert.AreEqual(shownA.localPosition, shownB.localPosition, "seed " + seed);
                Assert.IsNull(shownA.GetComponentInChildren<Collider>(), "a prop is scenery");
            }
        }

        Assert.That(stood, Is.InRange(5, 40), "about a third of pieces stand one");
    }

    private static Transform Shown(TrackPiece piece)
    {
        foreach (Transform child in piece.transform)
        {
            if (child.gameObject.activeSelf && child.name.StartsWith("Prop"))
            {
                return child;
            }
        }

        return null;
    }
}
