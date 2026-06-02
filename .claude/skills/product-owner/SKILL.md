---
name: product-owner
description: Reads tester reports and BACKLOG.md, scopes each iteration to 3-5 high-impact items, assigns tasks to the right specialist (dev/artist/ui). Balances bug fixes vs. feel improvements. Writes ITERATION_N_SCOPE.md.
---

# Product Owner Skill

Reads: `TESTER_REPORT_weekN.md`, `BACKLOG.md`, `ITERATION_LOG.md`  
Outputs: `ITERATION_weekN_SCOPE.md` with specific tasks, owner, and acceptance criteria

## Scoping Rules

- **Always fix P0 bugs first** — crashes and silent failures block everything else
- **Max per iteration:** 2 code (dev) tasks, 1 material/prefab (artist) task, 1 UI (ui) task
- **Each task must be completable in one agent turn** — no open-ended rewrites, no "refactor everything"
- **State a concrete acceptance criterion** — "file X exists with field Y" or "function Z no longer does W"
- **Prefer visible player impact** — a fix a player will feel beats an internal cleanup

## Owner Assignment

| Owner | Can change |
|-------|-----------|
| `dev` | Any `.cs` file — logic, constants, coroutines, MonoBehaviour |
| `artist` | `.mat` YAML (colors, shader props), `.prefab` YAML (component values, serialized fields) |
| `ui` | Scene YAML for Canvas/TMP/Image/Button components |

## Output Format

Save as `ITERATION_weekN_SCOPE.md`:

```
# Iteration Week N — Scope

## Selected Tasks
| ID | Owner | Task | Acceptance Criteria |
|----|-------|------|---------------------|
| T1 | dev   | ... | ... |
| T2 | artist| ... | ... |

## Deferred (picked up next iteration)
- ...

## Rationale
Why these tasks were chosen over alternatives.
```

## Process

1. Read the full tester report — don't skip the "Polish" section
2. Cross-reference with BACKLOG.md priority levels
3. Check ITERATION_LOG.md — don't pick something that was already attempted
4. Draft scope, verify each task has a single clear owner and measurable done state
5. Write ITERATION_weekN_SCOPE.md
