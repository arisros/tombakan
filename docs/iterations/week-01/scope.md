# Iteration Week 1 — Scope

**Source:** TESTER_REPORT_week1.md  
**Owner:** product-owner  

---

## Selected Tasks (5)

### TASK-01 · Score floor + dead-code cleanup
**Agent:** dev  
**Files:** `Assets/MobileARTemplateAssets/Scripts/GameManager.cs`  
**Rationale:** BUG-01 + BUG-02 + BUG-03. Score negativity harms learner motivation.
Dead `UpdateTimerUI` and oversized `collectedFishColors[1000]` are code quality risks.  
**Acceptance:** `score` never drops below 0; `UpdateTimerUI` removed; `collectedFishColors`
is `List<string>`; `EndGame` uses `collectedFishColors.Count`.

### TASK-02 · Combo streak multiplier
**Agent:** dev  
**Files:** `Assets/MobileARTemplateAssets/Scripts/GameManager.cs`  
**Rationale:** UX-02. Flat reward curve kills mid-session engagement.  
**Acceptance:** Tracking `comboStreak` int; 3-in-a-row = 2× points; 5-in-a-row = 3× points;
combo resets on wrong hit; happy feedback text shows multiplier (e.g. "x2 COMBO! +200").

### TASK-03 · Dynamic fish count (difficulty ramp)
**Agent:** dev  
**Files:** `Assets/MobileARTemplateAssets/Scripts/FishSpawner.cs`,
`Assets/MobileARTemplateAssets/Scripts/GameManager.cs`  
**Rationale:** UX-03. Always-5 fish removes challenge progression.  
**Acceptance:** Fish count scales with `correctHitCount`: 0–2 = 3 fish, 3–5 = 4 fish,
6–9 = 5 fish, 10–14 = 6 fish, 15+ = 7 fish. `fishCount` inspector default unchanged as
fallback (still serialised).

### TASK-04 · Numeric timer countdown text
**Agent:** dev + ui  
**Files:** `Assets/MobileARTemplateAssets/Scripts/GameManager.cs`  
**Rationale:** UX-01. Players need precise time awareness for strategic throws.  
**Acceptance:** `public TMP_Text timerCountdownText` field added; when assigned in scene,
displays `Mathf.CeilToInt(timeLeft)` each Update; null-safe (no NullReferenceException
if not wired in scene). Resets to full duration string on StartGame.

### TASK-05 · Raise TierHigh threshold + add TierLegend tier
**Agent:** dev  
**Files:** `Assets/MobileARTemplateAssets/Scripts/GameManager.cs`  
**Rationale:** UX-04. Current TierHigh (>4) is reachable in ~15 s — no lasting challenge.  
**Acceptance:** Revised thresholds: Empty=0, Low=1-4, Mid=5-9, High=10-14, Legend≥15.
`TierLegend` Image field added (optional — null-safe); if not wired in scene, falls back
gracefully to TierHigh.

---

## Out of Scope This Week
- Expanding colour vocabulary beyond Red/Green/Blue (needs artist + scene changes)
- Removing orphaned `FishHit.cs` (needs careful scene prefab audit)
- Hit animation on fish (requires animator state machine changes)
