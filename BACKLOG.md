# Backlog

## Completed

### Week 1
- [x] Score floor — prevent negative score display
- [x] Combo streak multiplier (2× / 3×)
- [x] Dynamic fish count difficulty ramp (3→7)
- [x] Numeric timer countdown text field
- [x] Raised tier thresholds + TierLegend

---

## Open

### High Priority
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
