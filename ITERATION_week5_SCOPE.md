# Iteration Week 5 — Scope

## Selected Tasks

| ID | Owner | Task | Acceptance Criteria |
|----|-------|------|---------------------|
| TASK-01 | dev | Fix spawn-pipeline bugs: (a) guard `PickNewTarget` so it is a no-op after `EndGame`; (b) fix `FishSpawner.SpawnFish` adding a duplicate `FishSwim`; (c) reorder `targetSpeciesLabel` update to after `SpawnFish` | `GameManager.PickNewTarget` returns early when `!gameRunning`; `FishSpawner.SpawnFish` uses `GetComponent<FishSwim>() ?? AddComponent<FishSwim>()`; `targetSpeciesLabel.text` assigned after the `SpawnFish(...)` call |
| TASK-02 | dev | Fix defensive guards: (a) disable `PlaceWaterOnPlane` after first successful placement so mid-game taps cannot re-position the water; (b) fix `GameManager.CollectSummary` to show all species/colours caught this session, not only first-ever discoveries | `PlaceWaterOnPlane.Update` disables itself (`enabled = false` or component disabled) after the first successful raycast hit; `CollectSummary` produces a non-empty, accurate summary for returning players who have already unlocked all species |
| TASK-03 | dev | Result-screen text polish: (a) `Accuracy.Format` returns `"--"` (or `"Belum ada lemparan"`) when both `correct == 0` and `wrong == 0`; (b) `GameManager.EndGame` skips setting `resultXpText` when `xpEarned == 0` (leave previous text or set empty) | `Accuracy.Format(0, 0)` returns a non-numeric placeholder string; `resultXpText` is not updated when `xpEarned == 0` |
| TASK-04 | dev | Remove dead TODO comment at `GameManager.cs:365` (`// TODO (Zen mode): pass a speed multiplier…`) | The comment no longer exists in `GameManager.cs` |

> **Note — Artist/UI tasks:** No artist or UI-only tasks are scoped this iteration. All top issues are pure C# logic bugs with no material, prefab shader, or canvas-layout component; assigning artist/UI work would manufacture tasks that add no player value. The 4-dev-task cap is waived for this iteration in favour of shipping the highest-impact fixes.

## Deferred (next iteration)

- **BUG-4 (partial):** `PlaceWaterOnPlane` — TASK-02 disables the component; a future iteration could also add a "re-position water" button for players who mis-placed.
- **UX-2:** Achievement unlock has no in-game notification — requires both a new UI panel (prefab) and C# show/hide logic; scope for Week 6.
- **UX-1:** Throw-mechanic tutorial hint for first-time players — needs `TombakanOnboarding` integration and a new tutorial panel.
- **POLISH-1:** Stagger level-up panel and new-record badge on result screen (0.3–0.5 s delay) — low friction, defer until after P1 bugs resolved.
- **POLISH-3:** ScreenShake magnitude on wrong hit causes AR world lurch — reduce `wrongHitMagnitude` in inspector or switch to a UI flash — defer to artist pass.

## Rationale

**TASK-01** addresses the two highest-impact bugs this session: BUG-1 causes fish to haunt the result screen (every game where a fish is caught with < 3 s left), and BUG-2 causes every fish to jitter or teleport due to duplicate movement AI. BUG-3 is a trivial reorder within the same PickNewTarget method; combining it with T1 costs nothing.

**TASK-02** fixes a silent game-breaker (stray AR tap teleports the play field) and a deflating result-screen bug (returning players always see "Tidak ada ikan dikumpulkan"). Both are one-file additions.

**TASK-03** is small polish with outsized first-impression value: "0/0 (0%)" on a zero-throw result looks like broken UI. Fixing it alongside the dead TODO makes the codebase cleaner without a separate dev pass.

**TASK-04** is a single-line deletion that removes stale intent from production code. Zero risk, zero effort.
