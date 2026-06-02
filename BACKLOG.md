# Backlog

## Completed

### Week 3
- [x] Target colour name label (accessibility + vocabulary)
- [x] Adaptive inter-round delay (pacing scales with skill)
- [x] Accuracy stat on result screen
- [x] WaterRipple null-guard (NRE fix)

### Week 2
- [x] Centralise + expand fish-colour palette (FishPalette, +Kuning) — fixes palette drift bug
- [x] Clear fish on game end (no shoal behind result screen)
- [x] High-score persistence (PlayerPrefs) + new-record badge
- [x] Aggregated, overflow-safe result colour summary

### Week 1
- [x] Score floor — prevent negative score display
- [x] Combo streak multiplier (2× / 3×)
- [x] Dynamic fish count difficulty ramp (3→7)
- [x] Numeric timer countdown text field
- [x] Raised tier thresholds + TierLegend
- [x] CI/CD — GitHub Actions: EditMode test runner + Android build (game-ci)
- [x] Assembly definitions (`Tombakan.Runtime` + `Tombakan.Tests`) so tests are discoverable

---

## Open

### High Priority
- [ ] **Add `UNITY_LICENSE` repo secret** (+ `UNITY_EMAIL`/`UNITY_PASSWORD`) so CI test/build jobs actually run instead of skipping
- [ ] C# lint in CI (`dotnet format --verify-no-changes`) — requires generating `.csproj` via Unity in-container first; follow-up to the test-runner job
- [ ] Expand colour vocabulary further (4 of 20 in use; Dict.cs supports more)
- [ ] Remove orphaned `FishHit.cs` / `FishIdentity.cs` (never called) — needs prefab/scene audit
- [ ] Colour-blind accessibility — add shape/symbol overlays to fish (Week 3 added a target colour *word* label; in-water fish still colour-only)

### Medium Priority
- [ ] Hit animation on fish — trigger a "caught" squish/flop animation in Animator on correct hit

### Low Priority
- [ ] Tutorial scene improvements — GoalManager onboarding does not account for players who already know AR placement
- [ ] Mute/volume button on gameplay HUD
- [ ] Haptic feedback on correct hit (mobile only)
