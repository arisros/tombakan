# Iteration Week 3 — Scope

**Source:** TESTER_REPORT_week3.md  
**Owner:** product-owner  
**Theme:** Teach and pace — surface the colour word, adapt the tempo to skill, give
end-of-game accuracy feedback, and silence a known NRE.

---

## Selected Tasks (4)

### TASK-01 · Target colour name label (accessibility + vocabulary)
**Agent:** dev  
**Files:** `Scripts/GameManager.cs`  
**Rationale:** UX-10. Communicating the target by colour alone fails in glare, for
colour-blind players, and for the vocabulary goal. The Indonesian name already exists in
`ColorHexLocalization`.  
**Acceptance:**
- Add optional, null-safe `TMP_Text targetColorLabel`.
- `PickNewTarget` sets it to `ColorHexLocalization.ToIndonesian(targetColor)` each round.
- No behaviour change when the field is unassigned in the scene.

### TASK-02 · Adaptive inter-round delay
**Agent:** dev  
**Files:** `Scripts/GameManager.cs`, `Scripts/PacingRules.cs` (new)  
**Rationale:** UX-08. Constant 2.2 s lock drags the late game for skilled players.  
**Acceptance:**
- New pure static `PacingRules.HitDelayForProgress(float baseDelay, int correctHitCount)`:
  shrinks the delay by 0.1 s per correct hit, clamped to a 1.0 s floor and never above
  `baseDelay`.
- `OnFishHit` computes the delay from this rule and uses it for both `LockThrow` and the
  `PickNewTarget` invoke (preserving the existing `+0.8 s` retarget offset).
- `hitDelay` field becomes the base/maximum (default unchanged at 2.2).
- Pure/static and unit-testable.

### TASK-03 · Accuracy stat on the result screen
**Agent:** dev  
**Files:** `Scripts/GameManager.cs`, `Scripts/Accuracy.cs` (new)  
**Rationale:** UX-09. No miss tracking, so no accuracy feedback for the learning loop.  
**Acceptance:**
- Track `wrongHitCount`; reset in `StartGame`, increment on a wrong hit in `OnFishHit`.
- New pure static `Accuracy.Percent(int correct, int wrong)` → rounded integer percent;
  returns 0 when no attempts.
- Optional, null-safe `TMP_Text resultAccuracyText` shows e.g. "9/12 (75%)" in `EndGame`.
- Pure/static and unit-testable.

### TASK-04 · `WaterRipple` null-guard
**Agent:** dev  
**Files:** `Scripts/WaterRipple.cs`  
**Rationale:** BUG-07. NRE spam when the water plane lacks a Renderer.  
**Acceptance:**
- Resolve the Renderer safely; if absent, disable the component (or early-return) instead
  of dereferencing null every frame. No `NullReferenceException` under any configuration.

---

## Out of Scope This Week
- Removing dead `FishHit.cs` / `FishIdentity.cs` — still needs a prefab/scene audit
- Expanding the palette beyond 4 colours — defer until the name label proves the UX
- C# linter in CI / `UNITY_LICENSE` secret — unchanged external blockers (backlog)
