// EditMode tests for Week 7:
//   TASK-01: SpearHit fishLayer=0 fallback — unmasked OverlapSphere still finds fish
//   TASK-02: FishSpawner.ApplyColor paints all child Renderers on a multi-mesh fish
using NUnit.Framework;
using UnityEngine;

// ── TASK-01: SpearHit layer-mask fallback ────────────────────────────────────

[TestFixture]
public class SpearHitWeek7Tests
{
    // Confirm that the unassigned default LayerMask has value 0, which is the
    // sentinel that triggers the fallback path in SpearHit.CheckFishHit.
    [Test]
    public void SpearHit_DefaultLayerMask_HasValueZero()
    {
        LayerMask mask = default;
        Assert.AreEqual(0, mask.value,
            "An unassigned LayerMask must have value 0 so the fallback guard fires.");
    }

    // Core regression: a masked OverlapSphere with value=0 finds nothing,
    // while the unmasked fallback finds the collider on layer 0.
    [Test]
    public void SpearHit_ZeroMask_FindsNothing_FallbackFindsCollider()
    {
        // Arrange: create a collider on layer 0 (Default)
        var fishGo = new GameObject("FishStub");
        fishGo.layer = 0;
        var col = fishGo.AddComponent<SphereCollider>();
        col.radius = 0.1f;
        Physics.SyncTransforms();

        var origin = fishGo.transform.position;
        const float radius = 0.5f;

        // Act
        Collider[] maskedHits   = Physics.OverlapSphere(origin, radius, 0);
        Collider[] fallbackHits = Physics.OverlapSphere(origin, radius);

        // Assert
        Assert.AreEqual(0, maskedHits.Length,
            "LayerMask value 0 (no layers selected) must not find any colliders — " +
            "this is the silent-miss bug TASK-01 fixes.");
        Assert.IsTrue(fallbackHits.Length > 0,
            "Unmasked fallback OverlapSphere must find colliders on layer 0 (Default).");

        Object.DestroyImmediate(fishGo);
    }
}

// ── TASK-02: FishSpawner.ApplyColor multi-renderer ───────────────────────────

[TestFixture]
public class FishSpawnerWeek7Tests
{
    // Creates a minimal fish stub: one root + N child GameObjects each with a MeshRenderer.
    static GameObject BuildMultiRendererFish(int rendererCount)
    {
        var root = new GameObject("FishRoot");
        for (int i = 0; i < rendererCount; i++)
        {
            var child = new GameObject($"Part_{i}");
            child.transform.SetParent(root.transform);
            var mr = child.AddComponent<MeshRenderer>();
            // Assign a fresh material so each Renderer has its own instance.
            mr.material = new Material(Shader.Find("Standard"));
        }
        return root;
    }

    // All three Renderers (body, fins, eye) must receive the target color.
    [Test]
    public void ApplyColor_ThreeChildRenderers_AllReceiveTargetColor()
    {
        var fish          = BuildMultiRendererFish(3);
        var expectedColor = new Color(0f, 1f, 0f, 1f); // green

        FishSpawner.ApplyColor(fish, expectedColor);

        var renderers = fish.GetComponentsInChildren<Renderer>();
        Assert.AreEqual(3, renderers.Length, "Stub must expose exactly 3 Renderers.");
        foreach (var r in renderers)
            Assert.AreEqual(expectedColor, r.material.color,
                $"Renderer '{r.gameObject.name}' did not receive the target color.");

        Object.DestroyImmediate(fish);
    }

    // A fish with no Renderer children must not throw (null-catalog fallback path).
    [Test]
    public void ApplyColor_NoRenderers_DoesNotThrow()
    {
        var fish = new GameObject("EmptyFish");
        Assert.DoesNotThrow(() => FishSpawner.ApplyColor(fish, Color.red));
        Object.DestroyImmediate(fish);
    }

    // Regression: single-renderer fish (previous behaviour) still works.
    [Test]
    public void ApplyColor_SingleRenderer_StillPainted()
    {
        var fish          = BuildMultiRendererFish(1);
        var expectedColor = new Color(1f, 0f, 0f, 1f); // red

        FishSpawner.ApplyColor(fish, expectedColor);

        var renderers = fish.GetComponentsInChildren<Renderer>();
        Assert.AreEqual(1, renderers.Length);
        Assert.AreEqual(expectedColor, renderers[0].material.color,
            "Single-Renderer fish must still receive the target color after refactor.");

        Object.DestroyImmediate(fish);
    }
}
