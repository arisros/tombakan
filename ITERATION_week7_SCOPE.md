# Iteration Scope — Week 7

**Date:** 2026-09-28
**Input:** TESTER_REPORT_week7.md, BACKLOG.md, ITERATION_LOG.md
**Tasks:** 4 (2 dev · 1 artist · 1 ui)

---

## Selected Tasks

### TASK-01 — Fix SpearHit fishLayer silent miss (P0 / dev)

**Source:** BUG-B (Tester Report rank 3 — "Spear passes through all fish; core mechanic fails")

**Description:**
`SpearHit.cs:18` calls `Physics.OverlapSphere(transform.position, hitRadius, fishLayer)`.
When `fishLayer` is left at its default value (layer 0, "Default"), Unity's layer mask silently
excludes everything on the Default layer from the overlap check, so no fish are ever hit.
Fix: add a `[RuntimeInitializeOnLoadMethod]` or `Awake` warning when `fishLayer == 0`, and
add a fallback that runs an unmasked `Physics.OverlapSphere` (no layer filter) so fish are
always detectable regardless of Inspector assignment. Log a visible `Debug.LogError` in
development builds so the scene-setup gap surfaces immediately.

**Files:** `Assets/MobileARTemplateAssets/Scripts/SpearHit.cs`

**Acceptance Criteria:**
- In a fresh scene where `fishLayer` was never assigned (value == 0), throwing a spear
  still registers hits on fish colliders.
- A `Debug.LogError` fires once in the Editor/Development build when `fishLayer == 0` at
  start, pointing to the field that needs assigning.
- When `fishLayer` is correctly set, behaviour is identical to today (no regression).
- Existing tests pass; a new EditMode test verifies the fallback path returns a non-empty
  collider array when fish are on layer 0.

---

### TASK-02 — Fix FishSpawner.ApplyColor for multi-mesh fish (P0 / dev)

**Source:** BUG-C (Tester Report rank 4 — "Color-ID mechanic unreliable on multi-mesh prefabs")

**Description:**
`FishSpawner.cs:103` uses `fish.GetComponentInChildren<Renderer>()`, which returns only the
first Renderer in depth-first traversal order. A standard low-poly fish has separate Renderer
components for body, fins, and tail. Only the first mesh is recolored; the rest keep the
default material color, making color-based targeting ambiguous.
Fix: replace the single-component call with `GetComponentsInChildren<Renderer>()` and loop
over the array, applying `material.color` (or `SetColor("_BaseColor", …)` for URP/Lit) to
every Renderer. Preserve the null-catalog single-color fallback path.

**Files:** `Assets/MobileARTemplateAssets/Scripts/FishSpawner.cs`

**Acceptance Criteria:**
- All Renderer components on a spawned fish receive the assigned target color.
- A fish prefab with three child Renderers (body, fins, eye) shows a uniform color on all
  three meshes after `ApplyColor` runs.
- The null-catalog path (no FishCatalog assigned) is unaffected and continues to work.
- An EditMode test instantiates a multi-renderer fish stub and asserts that all Renderer
  materials carry the expected color after `ApplyColor`.

---

### TASK-03 — AR scanning status overlay (UX-1 / ui)

**Source:** UX-1 (Tester Report rank 1 — "First-timer can't start; silent failure")

**Description:**
`PlaceWaterOnPlane.Update()` returns silently when no AR planes are found, giving the
first-time player no feedback that anything is happening. Add two UI states to the placement
flow:
1. **Scanning state** — while `planeManager.trackables.count == 0`, show a text label:
   `"Arahkan kamera ke lantai"` (Point camera at the floor) with a simple scan-pulse
   animation or icon.
2. **Ready state** — once at least one ARPlane is tracked, swap the label to
   `"Ketuk untuk menempatkan air"` (Tap to place water).
3. Both labels are hidden immediately after successful water placement.

This requires a two-field addition to `PlaceWaterOnPlane.cs` (serialized `TMP_Text scanLabel`
reference + the two-state logic) and wiring the new UI text element in the scene.

**Files:**
- `Assets/MobileARTemplateAssets/Scripts/PlaceWaterOnPlane.cs` (script change)
- `Assets/Scenes/GamePlay.unity` (scene: add TMP_Text `ScanLabel` under the AR UI canvas,
  assign reference to PlaceWaterOnPlane component)

**Acceptance Criteria:**
- Before any ARPlane is detected, the scan label is visible with the "Arahkan kamera ke
  lantai" string.
- After the first ARPlane is tracked, the label text updates to "Ketuk untuk menempatkan
  air" without any code outside PlaceWaterOnPlane.cs being changed.
- After the player taps and water is placed, the label `gameObject` is set inactive.
- The script change is null-safe: if `scanLabel` is unassigned in the Inspector, no
  NullReferenceException is thrown.

---

### TASK-04 — Audit fish prefab renderer hierarchy for color-override compatibility (artist)

**Source:** BUG-C companion (Tester Report rank 4; TASK-02 prerequisite audit)

**Description:**
TASK-02 fixes `ApplyColor` to color all child Renderers. For that fix to work, every mesh on
every fish prefab must use a material with a `_BaseColor` (URP/Lit, URP/Unlit) or `_Color`
(Standard) property that responds to `material.color` at runtime. Prefabs that share a
material asset must use `material.color` (instance copy) not `sharedMaterial.color`.

Audit all fish prefabs under `Assets/` (including any in `FishCatalog`):
1. Confirm each has at least one `Renderer` per visible mesh (body, fins, tail).
2. Confirm each Renderer's material uses URP/Lit, URP/Unlit, or equivalent with `_BaseColor`.
3. Flag any prefab whose Renderers share a single material instance that would cause all fish
   to change color when one is targeted (shared-material contamination).
4. Produce a one-paragraph prefab health summary in the PR description.

**Files:** Fish prefab `.prefab` and `.mat` files under `Assets/`

**Acceptance Criteria:**
- All fish prefabs are confirmed to use per-instance materials (not shared) for color
  overrides, OR the ones that share materials are fixed to use `Instantiate` at spawn time.
- All Renderer materials on fish prefabs expose `_BaseColor` (URP) or `_Color` (Standard)
  so runtime tinting works.
- No prefab has a Renderer on a visible mesh that lacks a tintable material.
- The PR description includes a brief audit table (prefab name · renderer count · material
  type · action taken).

---

## Deferred This Week

| Issue | Reason |
|-------|--------|
| BUG-A — Daily bonus level-up reward skipped | 3rd dev slot; addressed in week 8 as standalone task to avoid exceeding dev budget |
| UX-2 — Throw-mechanic hint | Depends on UX-1 (TASK-03) landing first; queue for week 8 |
| UX-3 — Water re-positioning button | Requires scene UI + GameManager coordination; size exceeds one agent turn |
| BUG-D — Spear snaps to fish center | Visual polish; lower player impact than P0 items |
| POLISH-1 — "COMBO!" → Indonesian | One-liner; can be bundled into a polish pass week 8 |
