#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click setup: Tools > Tombakan > Create Starter Data
/// Creates the ScriptableObject assets the game systems need in
/// Assets/TombakanData/ so the designer can wire them up in the Inspector.
/// Run once per project; safe to re-run (skips existing assets).
/// </summary>
public static class TombakanSetupWizard
{
    const string DataDir = "Assets/TombakanData";

    [MenuItem("Tools/Tombakan/Create Starter Data")]
    public static void CreateStarterData()
    {
        EnsureDir(DataDir);
        EnsureDir($"{DataDir}/Fish");
        EnsureDir($"{DataDir}/Skins");

        CreateFishCatalog();
        CreateSpearShopCatalog();
        CreateLevelRewardTable();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Tombakan] Starter data created in Assets/TombakanData/. Wire them up in your GameManager, FishSpawner, and SpearThrower Inspectors.");
    }

    static void CreateFishCatalog()
    {
        string path = $"{DataDir}/FishCatalog.asset";
        if (AssetExists(path)) return;

        var catalog = ScriptableObject.CreateInstance<FishCatalog>();

        // Starter species (Indonesian waters — verified names)
        var speciesData = new[]
        {
            ("ikan_badut",  "Ikan Badut",   "Clownfish",     "Amphiprioninae", FishRarity.Common,   new Color(1f, 0.4f, 0f),   "Ikan badut tinggal di anemon laut dan tidak akan tersengat oleh tentakelnya."),
            ("tongkol",     "Tongkol",      "Mackerel Tuna", "Euthynnus",      FishRarity.Common,   new Color(0.2f, 0.4f, 0.8f),"Tongkol bisa berenang sangat cepat dan sering bergerak dalam kelompok besar."),
            ("kakap_merah", "Kakap Merah",  "Red Snapper",   "Lutjanus campechanus", FishRarity.Uncommon, new Color(0.9f, 0.2f, 0.2f),"Kakap merah adalah ikan yang disukai banyak nelayan karena rasanya yang enak."),
            ("kerapu",      "Kerapu",       "Grouper",       "Epinephelus",    FishRarity.Uncommon, new Color(0.5f, 0.3f, 0.2f),"Kerapu bisa mengubah warna kulit mereka untuk menyamarkan diri dari predator."),
            ("baronang",    "Baronang",     "Rabbitfish",    "Siganus",        FishRarity.Common,   new Color(0.7f, 0.8f, 0.3f),"Baronang memiliki duri beracun, jadi nelayan harus berhati-hati saat menangkapnya."),
            ("ikan_nila",   "Ikan Nila",    "Nile Tilapia",  "Oreochromis niloticus", FishRarity.Common, new Color(0.5f, 0.6f, 0.5f),"Ikan nila adalah salah satu ikan budidaya paling populer di Indonesia."),
            ("bandeng",     "Bandeng",      "Milkfish",      "Chanos chanos",  FishRarity.Common,   new Color(0.8f, 0.85f, 0.9f),"Bandeng adalah ikan nasional Indonesia dan dikenal dengan banyak durinya."),
            ("ikan_mas",    "Ikan Mas",     "Goldfish",      "Carassius auratus", FishRarity.Rare,  new Color(1f, 0.75f, 0f),   "Ikan mas adalah simbol keberuntungan dan banyak dipelihara sebagai ikan hias."),
        };

        foreach (var (id, name, eng, latin, rarity, color, fact) in speciesData)
        {
            string speciesPath = $"{DataDir}/Fish/{id}.asset";
            if (AssetExists(speciesPath)) continue;

            var species = ScriptableObject.CreateInstance<FishSpecies>();
            species.id          = id;
            species.displayName = name;
            species.englishName = eng;
            species.latinName   = latin;
            species.rarity      = rarity;
            species.baseColor   = color;
            species.funFact     = fact;
            AssetDatabase.CreateAsset(species, speciesPath);
            catalog.species.Add(species);
        }

        AssetDatabase.CreateAsset(catalog, path);
        Debug.Log($"[Tombakan] Created FishCatalog with {catalog.species.Count} species at {path}");
    }

    static void CreateSpearShopCatalog()
    {
        string path = $"{DataDir}/SpearShopCatalog.asset";
        if (AssetExists(path)) return;

        var catalog = ScriptableObject.CreateInstance<SpearShopCatalog>();

        var skinData = new[]
        {
            ("spear_default", "Tombak Standar", 0,   true),
            ("spear_bamboo",  "Tombak Bambu",   50,  false),
            ("spear_besi",    "Tombak Besi",    150, false),
        };

        foreach (var (id, name, price, free) in skinData)
        {
            string skinPath = $"{DataDir}/Skins/{id}.asset";
            if (AssetExists(skinPath)) continue;

            var skin = ScriptableObject.CreateInstance<SpearSkin>();
            skin.id                = id;
            skin.displayName       = name;
            skin.price             = price;
            skin.currency          = SpearCurrency.Coins;
            skin.unlockedByDefault = free;
            AssetDatabase.CreateAsset(skin, skinPath);
            catalog.skins.Add(skin);
        }

        AssetDatabase.CreateAsset(catalog, path);
        Debug.Log($"[Tombakan] Created SpearShopCatalog with {catalog.skins.Count} skins at {path}");
    }

    static void CreateLevelRewardTable()
    {
        string path = $"{DataDir}/LevelRewardTable.asset";
        if (AssetExists(path)) return;

        var table = ScriptableObject.CreateInstance<LevelRewardTable>();

        table.rewards.Add(new LevelReward { level = 2,  softCurrencyBonus = 20, celebrationText = "Level 2! Terus berlatih!" });
        table.rewards.Add(new LevelReward { level = 3,  softCurrencyBonus = 30, celebrationText = "Level 3! Hebat!" });
        table.rewards.Add(new LevelReward { level = 5,  unlockedSpearSkinId = "spear_bamboo", softCurrencyBonus = 50, celebrationText = "Level 5! Tombak Bambu terbuka!" });
        table.rewards.Add(new LevelReward { level = 8,  softCurrencyBonus = 80, celebrationText = "Level 8! Kamu semakin ahli!" });
        table.rewards.Add(new LevelReward { level = 10, unlockedSpearSkinId = "spear_besi", softCurrencyBonus = 100, celebrationText = "Level 10! Tombak Besi terbuka! Luar biasa!" });
        table.rewards.Add(new LevelReward { level = 15, softCurrencyBonus = 150, celebrationText = "Level 15! Penombak sejati!" });
        table.rewards.Add(new LevelReward { level = 20, softCurrencyBonus = 200, celebrationText = "Level 20! Master Tombakan!" });

        AssetDatabase.CreateAsset(table, path);
        Debug.Log($"[Tombakan] Created LevelRewardTable at {path}");
    }

    static bool AssetExists(string path) => File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), path));
    static void EnsureDir(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
