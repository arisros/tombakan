using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Acceptance tests for TASK-03: "Posisi ulang" reposition button before game start.
///
/// Covers:
///   - PlaceWaterOnPlane.Reposition() is a public method.
///   - Reposition() hides the water prefab.
///   - Reposition() re-enables the component (re-arms the next AR tap).
///   - Reposition() calls fishSpawner.ClearAll().
///   - Reposition() is null-safe for optional dependencies.
///
/// Scene-level criteria (button visible pre-game, hidden post-StartGame) are
/// covered by Week7_Task03_RepositionPlayModeTests in the PlayMode suite.
/// </summary>
public class Week7_Task03_RepositionTests
{
    PlaceWaterOnPlane placer;
    GameObject hostGO;
    GameObject waterPlaneGO;

    [SetUp]
    public void SetUp()
    {
        hostGO = new GameObject("PlaceWaterOnPlaneHost");
        placer = hostGO.AddComponent<PlaceWaterOnPlane>();

        waterPlaneGO = new GameObject("WaterPlane");
        waterPlaneGO.SetActive(true);
        placer.waterPlane = waterPlaneGO;

        // Leave optional references null by default; individual tests assign as needed.
        placer.planeManager       = null;
        placer.pointCloudManager  = null;
        placer.fishSpawner        = null;
    }

    [TearDown]
    public void TearDown()
    {
        if (hostGO       != null) Object.DestroyImmediate(hostGO);
        if (waterPlaneGO != null) Object.DestroyImmediate(waterPlaneGO);
    }

    // AC: PlaceWaterOnPlane.cs has a public Reposition() method.
    [Test]
    public void Reposition_IsPublicInstanceMethod()
    {
        var method = typeof(PlaceWaterOnPlane).GetMethod(
            "Reposition",
            BindingFlags.Public | BindingFlags.Instance);

        Assert.IsNotNull(method,
            "Reposition() must exist as a public instance method on PlaceWaterOnPlane.");
    }

    // AC: After Reposition(), water prefab is hidden.
    [Test]
    public void Reposition_DeactivatesWaterPlane()
    {
        waterPlaneGO.SetActive(true);

        placer.Reposition();

        Assert.IsFalse(waterPlaneGO.activeSelf,
            "Reposition() must call waterPlane.SetActive(false) to hide the water prefab.");
    }

    // AC: After Reposition(), component is re-armed to accept the next AR tap.
    [Test]
    public void Reposition_ReenablesComponent()
    {
        placer.enabled = false; // simulate state after first placement (one-shot disabled)

        placer.Reposition();

        Assert.IsTrue(placer.enabled,
            "Reposition() must set enabled = true so the component's Update() processes the next AR tap.");
    }

    // AC: Reposition() is null-safe when fishSpawner is not assigned.
    [Test]
    public void Reposition_NullFishSpawner_DoesNotThrow()
    {
        placer.fishSpawner = null;

        Assert.DoesNotThrow(() => placer.Reposition(),
            "Reposition() must not throw a NullReferenceException when fishSpawner is not wired.");
    }

    // AC: Reposition() is null-safe when planeManager is not assigned.
    [Test]
    public void Reposition_NullPlaneManager_DoesNotThrow()
    {
        placer.planeManager = null;

        Assert.DoesNotThrow(() => placer.Reposition(),
            "Reposition() must not throw a NullReferenceException when planeManager is not wired.");
    }

    // AC: After Reposition(), fish are cleared (fishSpawner.ClearAll() is called).
    //     Observable: CurrentTargetSpecies == null after ClearAll.
    [Test]
    public void Reposition_WithFishSpawner_ClearsAllFish()
    {
        var spawnerGO = new GameObject("FishSpawner");
        var spawner   = spawnerGO.AddComponent<FishSpawner>();
        placer.fishSpawner = spawner;

        Assert.DoesNotThrow(() => placer.Reposition(),
            "Reposition() must not throw when fishSpawner is assigned.");

        Assert.IsNull(spawner.CurrentTargetSpecies,
            "FishSpawner.CurrentTargetSpecies must be null after Reposition() calls ClearAll().");

        Object.DestroyImmediate(spawnerGO);
    }

    // AC: After Reposition(), subsequent AR taps re-place water correctly.
    //     Verifies the component's Update() is armed again by checking enabled = true.
    [Test]
    public void Reposition_AfterBeingDisabledTwice_StillReenablesComponent()
    {
        // First placement disables the component.
        placer.enabled = false;
        placer.Reposition(); // first reposition
        Assert.IsTrue(placer.enabled, "enabled after first Reposition().");

        // Simulate second placement disabling the component.
        placer.enabled = false;
        placer.Reposition(); // second reposition

        Assert.IsTrue(placer.enabled,
            "Reposition() must re-enable the component each time it is called, " +
            "allowing repeated re-placement cycles.");
    }
}
