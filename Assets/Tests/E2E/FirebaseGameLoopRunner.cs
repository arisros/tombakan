// ═══════════════════════════════════════════════════════════════════
// Firebase Test Lab — Game Loop runner for automated device E2E.
//
// HOW TO USE:
//   1. Add this component to the GameManager GameObject in a Firebase
//      build variant of GamePlay.unity.
//   2. Build a release APK: Build Settings → Android → Build.
//   3. Upload the APK to Firebase Test Lab → Game Loop test.
//   4. Firebase launches the APK with intent extra "scenario" = 1.
//      This component detects that intent and runs the scripted loop.
//   5. Results are written to /sdcard/…/files/results.json which
//      Firebase Test Lab reads for pass/fail.
//
// DO NOT add this component in the Play Store release build.
// ═══════════════════════════════════════════════════════════════════

using System.Collections;
using System.IO;
using UnityEngine;

public class FirebaseGameLoopRunner : MonoBehaviour
{
#if UNITY_ANDROID
    void Start()
    {
        if (IsGameLoopLaunch())
            StartCoroutine(RunScenario1());
    }

    static bool IsGameLoopLaunch()
    {
        // Firebase Test Lab passes --gameloop and --scenario N as command-line args
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string a in args)
        {
            if (a == "--gameloop" || a.StartsWith("--scenario"))
                return true;
        }

        // Also check Android intent extra (Firebase injects "scenario" extra)
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent      = activity.Call<AndroidJavaObject>("getIntent");
            int scenario          = intent.Call<int>("getIntExtra", "scenario", -1);
            return scenario >= 0;
        }
        catch
        {
            return false;
        }
    }

    // Scenario 1: start → hit correct fish → hit wrong fish → let timer expire
    IEnumerator RunScenario1()
    {
        Debug.Log("[GameLoopRunner] Scenario 1 start");
        var result = new ScenarioResult { scenario = 1 };

        // Wait for AR scene to settle (no AR plane needed — we call StartGame directly)
        yield return new WaitForSeconds(1f);

        if (GameManager.I == null)
        {
            result.outcome = "FAIL";
            result.detail  = "GameManager.I is null";
            WriteResult(result);
            yield break;
        }

        GameManager.I.StartGame();
        yield return null;

        // ── Correct hit ───────────────────────────────────────────────
        Color target = GameManager.I.targetColor;
        int scoreBefore = GameManager.I.score;
        GameManager.I.OnFishHit(target);
        yield return new WaitForSeconds(0.1f);

        int afterCorrect = GameManager.I.score;
        if (afterCorrect != scoreBefore + GameConstants.PointPerCorrectHit)
        {
            result.outcome = "FAIL";
            result.detail  = $"Correct hit: expected score {scoreBefore + GameConstants.PointPerCorrectHit}, got {afterCorrect}";
            WriteResult(result);
            yield break;
        }

        // ── Wrong hit ─────────────────────────────────────────────────
        Color wrong = GameManager.I.targetColor == Color.red ? Color.green : Color.red;
        int beforeWrong = GameManager.I.score;
        GameManager.I.OnFishHit(wrong);
        yield return new WaitForSeconds(0.1f);

        int afterWrong = GameManager.I.score;
        if (afterWrong != beforeWrong - GameConstants.PenaltyPerWrongHit)
        {
            result.outcome = "FAIL";
            result.detail  = $"Wrong hit: expected score {beforeWrong - GameConstants.PenaltyPerWrongHit}, got {afterWrong}";
            WriteResult(result);
            yield break;
        }

        // ── Timer expiry ──────────────────────────────────────────────
        GameManager.I.timeLeft = 0.05f;
        yield return new WaitForSeconds(0.3f);

        if (GameManager.I.gameRunning)
        {
            result.outcome = "FAIL";
            result.detail  = "Timer did not expire — gameRunning still true";
            WriteResult(result);
            yield break;
        }

        // ── Result screen visible ─────────────────────────────────────
        bool resultVisible = GameManager.I.resultContainer != null &&
                             GameManager.I.resultContainer.activeSelf;
        if (!resultVisible)
        {
            result.outcome = "FAIL";
            result.detail  = "Result screen not visible after timer expiry";
            WriteResult(result);
            yield break;
        }

        result.outcome     = "PASS";
        result.finalScore  = GameManager.I.score;
        result.correctHits = GameManager.I.correctHitCount;
        result.detail      = "All assertions passed";

        Debug.Log($"[GameLoopRunner] Scenario 1 PASS — score={result.finalScore}");
        WriteResult(result);

        yield return new WaitForSeconds(0.5f);
        Application.Quit(0);
    }

    static void WriteResult(ScenarioResult r)
    {
        string dir = $"/sdcard/Android/data/{Application.identifier}/files";
        try { Directory.CreateDirectory(dir); } catch { /* permissions vary */ }

        string json = JsonUtility.ToJson(r, prettyPrint: true);
        string path = Path.Combine(dir, "results.json");
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"[GameLoopRunner] Results written to {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameLoopRunner] Could not write results: {ex.Message}");
        }
    }

    [System.Serializable]
    class ScenarioResult
    {
        public int    scenario;
        public string outcome;      // "PASS" or "FAIL"
        public string detail;
        public int    finalScore;
        public int    correctHits;
    }
#endif
}
