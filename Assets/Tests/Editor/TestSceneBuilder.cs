using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Run once: Tombakan → Create Test Scene
// Saves Assets/Tests/PlayMode/TombakanTestScene.unity and adds it to Build Settings.
// Commit the generated scene — CI will then find it automatically.
public static class TestSceneBuilder
{
    const string SCENE_PATH = "Assets/Tests/PlayMode/TombakanTestScene.unity";
    const string STUB_PREFAB_PATH = "Assets/Tests/Resources/StubFish.prefab";

    [MenuItem("Tombakan/Create Test Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── AudioManager ────────────────────────────────────────────────
        var audioGO = new GameObject("AudioManager");
        var am = audioGO.AddComponent<AudioManager>();
        am.bgmSource = audioGO.AddComponent<AudioSource>();
        am.sfxSource = audioGO.AddComponent<AudioSource>();
        // Clips left null: PlayOneShot(null) is a no-op; BGM guard handles null clip.

        // ── Canvas ───────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject Panel(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGO.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        TMP_Text Txt(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGO.transform, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<TextMeshProUGUI>();
        }

        Image Img(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(canvasGO.transform, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<Image>();
        }

        // ── UI stubs ─────────────────────────────────────────────────────
        var mainScreenUI        = Panel("MainScreenUI");
        var gamePlayUI          = Panel("GamePlayUI");
        var scoreText           = Txt("ScoreText");
        var timerBarFill        = Img("TimerBarFill");
        var targetColorImage    = Img("TargetColorImage");
        var happyFeedback       = Panel("HappyFeedback");
        var happyFeedbackText   = Txt("HappyFeedbackText");
        var sadFeedback         = Panel("SadFeedback");
        var sadFeedbackText     = Txt("SadFeedbackText");
        var resultContainer     = Panel("ResultContainer");
        var resultScoreText     = Txt("ResultScoreText");
        var resultCorrectText   = Txt("ResultCorrectFishText");
        var tierEmpty           = Img("TierEmpty");
        var tierLow             = Img("TierLow");
        var tierMid             = Img("TierMid");
        var tierHigh            = Img("TierHigh");

        // ── Water plane stub (FishSpawner anchor) ────────────────────────
        var waterPlane = new GameObject("WaterPlane");
        waterPlane.transform.position = Vector3.zero;

        // ── Stub fish prefab ─────────────────────────────────────────────
        Directory.CreateDirectory("Assets/Tests/Resources");
        var stubPrefab = BuildStubFishPrefab();

        // ── FishSpawner ───────────────────────────────────────────────────
        var spawnerGO = new GameObject("FishSpawner");
        var spawner = spawnerGO.AddComponent<FishSpawner>();
        spawner.waterPlane = waterPlane.transform;
        spawner.fishPrefab = stubPrefab;
        spawner.fishCount = 5;
        spawner.spawnRadius = 1.5f;

        // ── GameManager ───────────────────────────────────────────────────
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();

        gm.mainScreenUI         = mainScreenUI;
        gm.gamePlayUI           = gamePlayUI;
        gm.scoreText            = scoreText;
        gm.timerBarFill         = timerBarFill;
        gm.targetColorImage     = targetColorImage;
        gm.happyFeedback        = happyFeedback;
        gm.happyFeedbackText    = happyFeedbackText;
        gm.sadFeedback          = sadFeedback;
        gm.sadFeedbackText      = sadFeedbackText;
        gm.resultContainer      = resultContainer;
        gm.resultScoreText      = resultScoreText;
        gm.resultCorrectFishText = resultCorrectText;
        gm.TierEmpty            = tierEmpty;
        gm.TierLow              = tierLow;
        gm.TierMid              = tierMid;
        gm.TierHigh             = tierHigh;
        gm.fishSpawner          = spawner;
        gm.spearThrower         = null; // not needed for logic/E2E tests

        // ── Save ──────────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AddToBuildSettings(SCENE_PATH);
        AssetDatabase.Refresh();

        Debug.Log($"[TestSceneBuilder] Saved {SCENE_PATH} — commit this file so CI finds it.");
    }

    static GameObject BuildStubFishPrefab()
    {
        // Reuse existing prefab if already saved
        if (File.Exists(STUB_PREFAB_PATH))
            return AssetDatabase.LoadAssetAtPath<GameObject>(STUB_PREFAB_PATH);

        var fish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fish.name = "StubFish";
        fish.transform.localScale = Vector3.one * 0.2f;
        fish.AddComponent<FishTarget>();
        fish.AddComponent<FishHitBox>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(fish, STUB_PREFAB_PATH);
        Object.DestroyImmediate(fish);
        return prefab;
    }

    static void AddToBuildSettings(string path)
    {
        var existing = EditorBuildSettings.scenes;
        foreach (var s in existing)
            if (s.path == path) return;

        var next = new EditorBuildSettingsScene[existing.Length + 1];
        System.Array.Copy(existing, next, existing.Length);
        next[existing.Length] = new EditorBuildSettingsScene(path, true);
        EditorBuildSettings.scenes = next;
    }
}
