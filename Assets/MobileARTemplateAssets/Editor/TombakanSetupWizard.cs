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

        // Starter species — 8 verified Indonesian marine/freshwater fish (see docs/species-data.md)
        // Scientific names, rarities, colors, and fun facts are cross-checked against
        // FishBase, LIPI, and KKP references. Do not change names/latin without re-verifying.
        var speciesData = new[]
        {
            // id                  displayName          englishName              latinName                        rarity                   baseColor                               funFact
            ("ikan_badut",         "Ikan Badut",         "Clownfish",             "Amphiprion ocellaris",          FishRarity.Common,    new Color(1.00f, 0.40f, 0.00f), "Ikan badut hidup bersama anemon laut dan tidak tersengat tentakelnya karena kulitnya dilapisi lendir khusus."),
            ("bandeng",            "Bandeng",            "Milkfish",              "Chanos chanos",                 FishRarity.Common,    new Color(0.78f, 0.85f, 0.91f), "Bandeng adalah ikan asli Indonesia yang sudah dibudidayakan di tambak selama ratusan tahun dan dikenal sebagai ikan banyak duri."),
            ("kakap_merah",        "Kakap Merah",        "Mangrove Red Snapper",  "Lutjanus argentimaculatus",     FishRarity.Uncommon,  new Color(0.88f, 0.19f, 0.13f), "Kakap merah bisa hidup di air laut maupun air payau, dan merupakan salah satu ikan favorit nelayan Indonesia karena dagingnya yang lezat."),
            ("kembung",            "Kembung",            "Indian Mackerel",       "Rastrelliger kanagurta",        FishRarity.Common,    new Color(0.23f, 0.43f, 0.65f), "Kembung adalah salah satu ikan yang paling banyak ditangkap di Indonesia dan sering dijual di pasar tradisional sebagai lauk sehari-hari."),
            ("tuna_sirip_kuning",  "Tuna Sirip Kuning",  "Yellowfin Tuna",        "Thunnus albacares",             FishRarity.Rare,      new Color(0.12f, 0.25f, 0.50f), "Tuna sirip kuning bisa berenang dengan kecepatan hingga 75 km/jam dan Indonesia adalah salah satu negara penghasil tuna terbesar di dunia."),
            ("kerapu_bebek",       "Kerapu Bebek",       "Humpback Grouper",      "Cromileptes altivelis",         FishRarity.Uncommon,  new Color(0.94f, 0.93f, 0.88f), "Kerapu bebek punya bintik-bintik hitam di seluruh tubuhnya dan merupakan ikan karang paling mahal di Indonesia karena rasanya sangat enak."),
            ("lele",               "Lele",               "Walking Catfish",       "Clarias batrachus",             FishRarity.Common,    new Color(0.29f, 0.25f, 0.25f), "Lele bisa berjalan di darat menggunakan siripnya untuk berpindah ke kolam lain ketika airnya kering!"),
            ("ikan_nila",          "Ikan Nila",          "Nile Tilapia",          "Oreochromis niloticus",         FishRarity.Common,    new Color(0.33f, 0.42f, 0.33f), "Ikan nila berasal dari Afrika tapi sekarang menjadi salah satu ikan air tawar yang paling banyak dibudidayakan di Indonesia."),
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
