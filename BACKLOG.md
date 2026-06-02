# Backlog

## Completed

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
- [ ] Expand colour vocabulary — use more of the 20 colours in `Dict.cs` (currently only Red/Green/Blue)
- [ ] Remove orphaned `FishHit.cs` (never called; `FishHitBox` handles all hit logic)
- [ ] Colour-blind accessibility — add shape/symbol overlays to fish in addition to colour

### Medium Priority
- [ ] Hit animation on fish — trigger a "caught" squish/flop animation in Animator on correct hit
- [ ] Adaptive hit delay — reduce `hitDelay` slightly as `correctHitCount` grows for faster late-game pacing
- [ ] High-score persistence — save best score with `PlayerPrefs` and show on main screen

### Low Priority
- [ ] Tutorial scene improvements — GoalManager onboarding does not account for players who already know AR placement
- [ ] Mute/volume button on gameplay HUD
- [ ] Haptic feedback on correct hit (mobile only)
