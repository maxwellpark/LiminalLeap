using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class GeneratedMaterialTests
{
    // MaterialAssets.Folder, which lives in the editor assembly this one can't see.
    private const string Folder = "Assets/Materials/Generated";

    // An emission colour with the keyword off renders nothing, and that is exactly how the
    // strip lights, exits and pickups shipped. Checks the baked assets, since that is what a
    // build actually uses.
    [Test]
    public void EverythingMeantToGlowHasEmissionOn()
    {
        var checkedAny = false;

        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { Folder }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || !material.HasProperty("_EmissionColor"))
            {
                continue;
            }

            var emission = material.GetColor("_EmissionColor");
            if (emission.maxColorComponent <= 0f)
            {
                continue;
            }

            checkedAny = true;
            Assert.IsTrue(material.IsKeywordEnabled("_EMISSION"), $"{path} has an emission colour but no _EMISSION");
            Assert.AreEqual(0, (int)(material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack),
                $"{path} is flagged EmissiveIsBlack, so saving it strips the keyword again");
        }

        Assert.IsTrue(checkedAny, "no emissive materials found, so this proves nothing");
    }
}
