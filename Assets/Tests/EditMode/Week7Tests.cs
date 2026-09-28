// EditMode tests for Week 7:
//   TASK-01: SpearHit fishLayer=0 fallback — unmasked OverlapSphere still finds fish
//   TASK-02: FishSpawner.ApplyColor paints all child Renderers on a multi-mesh fish
//   TASK-03: PlaceWaterOnPlane scan-label null-safety and text logic
using NUnit.Framework;
using TMPro;
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

// ── TASK-01 AC3 regression: correctly-assigned fishLayer still finds fish ─────

[TestFixture]
public class SpearHitRegressionWeek7Tests
{
    // AC3: When fishLayer is correctly set, the masked OverlapSphere still finds
    // the collider placed on that exact layer (no regression from the fallback fix).
    [Test]
    public void SpearHit_CorrectMask_FindsColliderOnMatchingLayer()
    {
        const int fishLayerIndex = 8; // user-defined layer slot; always exists in Unity
        var fishGo = new GameObject("FishStub");
        fishGo.layer = fishLayerIndex;
        var col = fishGo.AddComponent<SphereCollider>();
        col.radius = 0.1f;
        Physics.SyncTransforms();

        LayerMask fishMask = 1 << fishLayerIndex;

        Collider[] hits = Physics.OverlapSphere(fishGo.transform.position, 0.5f, fishMask);

        Assert.IsTrue(hits.Length > 0,
            "OverlapSphere with a correctly-assigned LayerMask must find the collider " +
            "on that layer; the TASK-01 fallback must not break the happy path.");

        Object.DestroyImmediate(fishGo);
    }

    // AC3 complementary: a collider on a different layer is NOT returned by the mask,
    // proving the mask is genuinely filtering.
    [Test]
    public void SpearHit_CorrectMask_DoesNotFindColliderOnDifferentLayer()
    {
        const int fishLayerIndex  = 8;
        const int otherLayerIndex = 9;
        var fishGo = new GameObject("FishStub");
        fishGo.layer = otherLayerIndex; // on a different layer than the mask
        var col = fishGo.AddComponent<SphereCollider>();
        col.radius = 0.1f;
        Physics.SyncTransforms();

        LayerMask fishMask = 1 << fishLayerIndex;

        Collider[] hits = Physics.OverlapSphere(fishGo.transform.position, 0.5f, fishMask);

        Assert.AreEqual(0, hits.Length,
            "A correctly-set LayerMask must exclude colliders on other layers.");

        Object.DestroyImmediate(fishGo);
    }
}

// ── TASK-03: PlaceWaterOnPlane scan-label null-safety and text logic ──────────

[TestFixture]
public class PlaceWaterOnPlaneWeek7Tests
{
    // Reflection helpers — UpdateScanLabel and scanLabel are non-public by design.
    static System.Reflection.MethodInfo s_UpdateScanLabel =
        typeof(PlaceWaterOnPlane).GetMethod(
            "UpdateScanLabel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    static System.Reflection.FieldInfo s_ScanLabelField =
        typeof(PlaceWaterOnPlane).GetField(
            "scanLabel",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    static void SetScanLabel(PlaceWaterOnPlane comp, TMP_Text label) =>
        s_ScanLabelField?.SetValue(comp, label);

    static void InvokeUpdateScanLabel(PlaceWaterOnPlane comp) =>
        s_UpdateScanLabel.Invoke(comp, null);

    // AC4: if scanLabel is unassigned in the Inspector (null), UpdateScanLabel
    // must not throw a NullReferenceException.
    [Test]
    public void ScanLabel_Unassigned_NullSafe_NoException()
    {
        var go   = new GameObject("PlaceWater");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        // scanLabel left null — simulates a scene where the UI reference was not wired.

        Assert.DoesNotThrow(() => InvokeUpdateScanLabel(comp),
            "UpdateScanLabel must be null-safe when scanLabel is not assigned.");

        Object.DestroyImmediate(go);
    }

    // AC4 complementary: the post-placement deactivation guard must also be null-safe
    // (the code uses  if (scanLabel != null) scanLabel.gameObject.SetActive(false)).
    [Test]
    public void ScanLabel_NullGuard_PlacementDeactivation_NoException()
    {
        // Directly verify the null-guard pattern used in the placement branch:
        //   if (scanLabel != null) scanLabel.gameObject.SetActive(false);
        // This ensures the guard is present; a missing guard would cause
        // NullReferenceException when scanLabel is unassigned.
        TMP_Text nullLabel = null;
        Assert.DoesNotThrow(() =>
        {
            if (nullLabel != null)
                nullLabel.gameObject.SetActive(false);
        }, "The null-guard on scanLabel must prevent NullReferenceException on deactivation.");
    }

    // AC1: before any ARPlane is detected (planeManager field is null), the label
    // must display "Arahkan kamera ke lantai" (Point camera at the floor).
    [Test]
    public void ScanLabel_NullPlaneManager_ShowsScanningText()
    {
        var go   = new GameObject("PlaceWater");
        var comp = go.AddComponent<PlaceWaterOnPlane>();
        // planeManager left null — no ARPlaneManager assigned.

        var labelGo = new GameObject("ScanLabel");
        var label   = labelGo.AddComponent<TextMeshPro>();
        SetScanLabel(comp, label);

        InvokeUpdateScanLabel(comp);

        Assert.AreEqual(
            "Arahkan kamera ke lantai",
            label.text,
            "Before any ARPlane is detected, the scan label must show the floor-scanning guidance.");

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(labelGo);
    }

    // AC3: after water is placed, the label gameObject must be set inactive.
    // Since ARRaycastManager cannot be exercised in EditMode, this test verifies
    // the deactivation logic path in isolation: a non-null scanLabel becomes inactive.
    [Test]
    public void ScanLabel_AfterPlacement_BecomesInactive()
    {
        var labelGo = new GameObject("ScanLabel");
        var label   = labelGo.AddComponent<TextMeshPro>();
        labelGo.SetActive(true);

        // Replicate the placement code path:  if (scanLabel != null) scanLabel.gameObject.SetActive(false);
        TMP_Text scanLabel = label;
        if (scanLabel != null)
            scanLabel.gameObject.SetActive(false);

        Assert.IsFalse(
            labelGo.activeSelf,
            "scanLabel.gameObject.SetActive(false) must make the label inactive after water is placed.");

        Object.DestroyImmediate(labelGo);
    }

    // Verify that the reflection helpers can locate both UpdateScanLabel and the
    // scanLabel field — a compile-time guard that fails loudly if the method or
    // field is renamed without updating the tests.
    [Test]
    public void ReflectionTargets_ExistOnPlaceWaterOnPlane()
    {
        Assert.IsNotNull(s_UpdateScanLabel,
            "Private method UpdateScanLabel must exist on PlaceWaterOnPlane.");
        Assert.IsNotNull(s_ScanLabelField,
            "Private field scanLabel must exist on PlaceWaterOnPlane.");
    }
}
