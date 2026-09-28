export const meta = {
  name: 'tombakan-iteration',
  description: 'Weekly autonomous game improvement: audit, scope, implement, validate, branch',
  phases: [
    { title: 'Audit', detail: 'Game tester simulates user sessions' },
    { title: 'Scope', detail: 'Product owner picks iteration tasks' },
    { title: 'Implement', detail: 'Dev + Artist + UI work in parallel' },
    { title: 'Validate', detail: 'QA tests + re-tester confirms fixes' },
    { title: 'Branch & Report', detail: 'New branch, commit, update logs' },
  ],
}

const REPO = '/home/user/tombakan'
const weekNum = args && args.week ? args.week : '?'
const WEEK_DIR = 'docs/iterations/week-' + String(weekNum).padStart(2, '0')
const TESTER_REPORT = WEEK_DIR + '/tester-report.md'
const SCOPE = WEEK_DIR + '/scope.md'
const BACKLOG = 'docs/product/BACKLOG.md'
const LOG = 'docs/iterations/LOG.md'

const TESTER_SCHEMA = {
  type: 'object',
  properties: {
    topIssues: { type: 'array', items: { type: 'object', properties: {
      rank: { type: 'number' }, issue: { type: 'string' },
      file: { type: 'string' }, impact: { type: 'string' }
    }, required: ['rank', 'issue', 'file', 'impact'] } },
    reportPath: { type: 'string' }
  },
  required: ['topIssues', 'reportPath']
}

const SCOPE_SCHEMA = {
  type: 'object',
  properties: {
    tasks: { type: 'array', items: { type: 'object', properties: {
      id: { type: 'string' },
      owner: { type: 'string', enum: ['dev', 'artist', 'ui'] },
      description: { type: 'string' },
      acceptanceCriteria: { type: 'string' }
    }, required: ['id', 'owner', 'description', 'acceptanceCriteria'] } }
  },
  required: ['tasks']
}

// ── Audit ─────────────────────────────────────────────────────────
phase('Audit')
log('Week ' + weekNum + ': game-tester running...')
const testerReport = await agent(
  'Game tester for Tombakan at ' + REPO + '. ' +
  'Read ' + BACKLOG + '. Read scripts: GameManager.cs, FishSwim.cs, FishSpawner.cs, SpearThrower.cs, PlaceWaterOnPlane.cs, FishHitBox.cs, Dict.cs (in Assets/MobileARTemplateAssets/Scripts/). ' +
  'Simulate 10 user sessions (first-timer, casual, frustrated, speed-runner). Trace full flow and find friction/bugs/gaps. ' +
  'Save report to ' + REPO + '/' + TESTER_REPORT + '. Return topIssues and reportPath.',
  { label: 'game-tester', phase: 'Audit', schema: TESTER_SCHEMA }
)
log('Found ' + (testerReport ? testerReport.topIssues.length : 0) + ' issues')

// ── Scope ─────────────────────────────────────────────────────────
phase('Scope')
const scope = await agent(
  'Product owner for Tombakan at ' + REPO + '. ' +
  'Read ' + TESTER_REPORT + ', ' + BACKLOG + ', ' + LOG + '. ' +
  'Pick 3-5 tasks: fix P0 first, max 2 dev/1 artist/1 ui, each doable in one agent turn. ' +
  'Save to ' + REPO + '/' + SCOPE + '. Return tasks with id/owner/description/acceptanceCriteria.',
  { label: 'product-owner', phase: 'Scope', schema: SCOPE_SCHEMA }
)
log('Scoped ' + (scope ? scope.tasks.length : 0) + ' tasks')

// ── Implement ─────────────────────────────────────────────────────
phase('Implement')
const devTasks = scope.tasks.filter(function(t) { return t.owner === 'dev' })
const artistTasks = scope.tasks.filter(function(t) { return t.owner === 'artist' })
const uiTasks = scope.tasks.filter(function(t) { return t.owner === 'ui' })
const fmt = function(tasks) { return tasks.map(function(t) { return t.id + ': ' + t.description + ' (done when: ' + t.acceptanceCriteria + ')' }).join(' | ') }

const workers = []
if (devTasks.length) workers.push(function() { return agent('Unity C# dev on Tombakan at ' + REPO + '. Read ' + SCOPE + '. Tasks: ' + fmt(devTasks) + '. Read files before editing. Use GameConstants.* for numbers.', { label: 'dev-agent', phase: 'Implement' }) })
if (artistTasks.length) workers.push(function() { return agent('URP artist on Tombakan at ' + REPO + '. Read ' + SCOPE + '. Tasks: ' + fmt(artistTasks) + '. Edit .mat/.prefab YAML only. No .cs files.', { label: 'artist-agent', phase: 'Implement' }) })
if (uiTasks.length) workers.push(function() { return agent('UI engineer on Tombakan at ' + REPO + '. Read ' + SCOPE + '. Tasks: ' + fmt(uiTasks) + '. Edit scene/prefab YAML only. Canvases stay Screen Space Overlay.', { label: 'ui-agent', phase: 'Implement' }) })
if (workers.length) { await parallel(workers) } else { log('No tasks this iteration') }

// ── Validate ──────────────────────────────────────────────────────
phase('Validate')
await parallel([
  function() { return agent('QA on Tombakan at ' + REPO + '. Read ' + SCOPE + '. Write tests in Assets/Tests/ for each acceptance criterion. Use [Test] for pure logic, [UnityTest] for scene tests.', { label: 'qa-agent', phase: 'Validate' }) },
  function() { return agent('Re-tester on Tombakan at ' + REPO + '. Read ' + TESTER_REPORT + ' and ' + SCOPE + '. For each task: find changed file, re-trace player session, confirm fix. Append "## Validation Week ' + weekNum + '" table (Task ID | Status | Evidence) to ' + TESTER_REPORT + '.', { label: 're-tester', phase: 'Validate' }) }
])

// ── Branch & Report ───────────────────────────────────────────────
phase('Branch & Report')
const branch = 'iteration/week-' + weekNum
await agent(
  'Release manager for Tombakan at ' + REPO + '. ' +
  'Create branch ' + branch + ', stage Assets/ ' + BACKLOG + ' ' + LOG + ' ' + TESTER_REPORT + ' ' + SCOPE + ', ' +
  'commit using Conventional Commits (type(scope): subject), no co-author or attribution trailers. ' +
  'Append a Week ' + weekNum + ' entry to ' + LOG + '. Update ' + BACKLOG + ' (mark done items, add new findings). Report files changed.',
  { label: 'release-manager', phase: 'Branch & Report' }
)
log('Done: branch: ' + branch)
