# CI Setup Guide

This guide explains how to get the GitHub Actions CI pipeline fully working for Tombakan. Target time: under 10 minutes.

---

## Why CI needs a Unity license

The `test` and `build-android` jobs use [game-ci](https://game-ci.com/), which spins up a headless Unity Editor container to compile the project and run tests. Unity requires license activation even in a container. Without a valid license the jobs are automatically **skipped** (not failed) — you will see a yellow warning in the Actions tab.

The `lint` job does **not** need a license and runs on every push regardless.

---

## Step 1 — Get your Unity license file (.ulf)

You need a `.ulf` (Unity License File) from your Unity account. Choose one method:

### Option A — GitHub activation workflow (recommended)

1. In your repo, go to **Actions → All workflows**.
2. Look for the `Activation` workflow provided by game-ci. If it is not present, add it manually:

   ```yaml
   # .github/workflows/activation.yml
   name: Acquire activation file
   on: [workflow_dispatch]
   jobs:
     activation:
       runs-on: ubuntu-latest
       steps:
         - uses: game-ci/unity-activate@v2
           id: activation
           with:
             unityVersion: 2022.3.62f3
         - uses: actions/upload-artifact@v4
           with:
             name: Unity_v2022.3.62f3.alf
             path: ${{ steps.activation.outputs.filePath }}
   ```

3. Run the workflow manually (click **Run workflow**).
4. Download the `.alf` artifact it produces.
5. Go to <https://license.unity3d.com/manual>, upload the `.alf` file, and download the resulting `.ulf` file.

### Option B — Local activation

1. Install the Unity Editor locally (same version: `2022.3.62f3`).
2. Run the editor from the command line to generate an `.alf` request file:
   ```
   Unity -batchmode -createManualActivationFile -logfile
   ```
3. Upload the `.alf` to <https://license.unity3d.com/manual> and download the `.ulf`.

---

## Step 2 — Add GitHub Secrets

In your repository on GitHub:

1. Go to **Settings → Secrets and variables → Actions**.
2. Click **New repository secret** for each of the three secrets below:

| Secret name | Value |
|---|---|
| `UNITY_LICENSE` | Full contents of the `.ulf` file (copy-paste everything, including the XML tags) |
| `UNITY_EMAIL` | The email address of your Unity account |
| `UNITY_PASSWORD` | The password of your Unity account |

> **Tip:** When pasting the `.ulf` contents, make sure there are no leading/trailing blank lines — some editors add them.

---

## Step 3 — Verify

1. Push any commit (or re-run the workflow from the Actions tab).
2. Open the **Actions** tab and watch the run.
3. The `check-secrets` job should output: `Unity license secret found — Unity jobs will run.`
4. The `test` and `build-android` jobs should proceed instead of being skipped.
5. A passing run uploads two artifacts: `test-results` and `build-android`.

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| `test` / `build-android` jobs are skipped | `UNITY_LICENSE` secret is missing or empty — re-add it from Step 2. |
| `License activation failed` in Unity logs | The `.ulf` may be for a different Unity version or account. Re-generate using the exact version `2022.3.62f3`. |
| `lint` job fails with `No .csproj found` | Expected on a fresh clone without a `Library/` folder. Unity generates `.csproj` on first import. The job is `continue-on-error: true` so it will not block other jobs. |
| `dotnet format` reports formatting errors | Run `dotnet format <path-to.csproj>` locally and commit the changes. |
| Build fails on `Android NDK not found` | game-ci's Unity image includes the NDK; this usually means the `unityVersion` in `ci.yml` does not match your project. Update `unityVersion: 2022.3.62f3` if you upgraded Unity. |
| Secret shows as `***` but jobs still skip | GitHub masks secret values in logs. Double-check the secret **name** exactly matches `UNITY_LICENSE` (case-sensitive). |
