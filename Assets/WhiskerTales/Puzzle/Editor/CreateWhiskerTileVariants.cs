using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiskerTales.Puzzle.Editor
{
    /// <summary>
    /// One-shot Editor menu that creates six Prefab Variants of the Kit's base candy
    /// prefabs and swaps each Variant's SpriteRenderer sprite to a Whisker tile PNG.
    ///
    /// Wraps the Kit per [[feedback_no_greenfield]] — Variant, not fork.
    /// Sprite-only override keeps every Kit component (Tile, ColorTile, PooledObject,
    /// SpriteRenderer settings) inherited, so Kit code paths still see real Tile
    /// components and the GameBoard sorts/swaps the same way.
    /// </summary>
    public static class CreateWhiskerTileVariants
    {
        private const string OutputDir = "Assets/WhiskerTales/Puzzle/Skin/WhiskerTilePrefabs";

        private struct Mapping
        {
            public string BasePrefabPath;
            public string SpritePath;
            public string VariantName;

            public Mapping(string baseP, string sprite, string variant)
            {
                BasePrefabPath = baseP;
                SpritePath = sprite;
                VariantName = variant;
            }
        }

        // Edit this table to change color↔Whisker mapping.
        private static readonly Mapping[] Mappings =
        {
            new Mapping("Assets/CandyMatch3Kit/Prefabs/Candies/BlueCandy.prefab",   "Assets/WhiskerTales/Art/Tiles/tile_fish.png",     "FishTile"),
            new Mapping("Assets/CandyMatch3Kit/Prefabs/Candies/GreenCandy.prefab",  "Assets/WhiskerTales/Art/Tiles/tile_catnip.png",   "CatnipTile"),
            new Mapping("Assets/CandyMatch3Kit/Prefabs/Candies/OrangeCandy.prefab", "Assets/WhiskerTales/Art/Tiles/tile_fishbone.png", "FishboneTile"),
            new Mapping("Assets/CandyMatch3Kit/Prefabs/Candies/PurpleCandy.prefab", "Assets/WhiskerTales/Art/Tiles/tile_yarn.png",     "YarnTile"),
            new Mapping("Assets/CandyMatch3Kit/Prefabs/Candies/RedCandy.prefab",    "Assets/WhiskerTales/Art/Tiles/tile_pawprint.png", "PawprintTile"),
            new Mapping("Assets/CandyMatch3Kit/Prefabs/Candies/YellowCandy.prefab", "Assets/WhiskerTales/Art/Tiles/tile_milk.png",     "MilkTile"),
        };

        [MenuItem("WhiskerTales/Tiles/Create Whisker Tile Variants")]
        public static void CreateAll()
        {
            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
                AssetDatabase.Refresh();
            }

            int created = 0;
            int skipped = 0;

            foreach (var m in Mappings)
            {
                var basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(m.BasePrefabPath);
                if (basePrefab == null)
                {
                    Debug.LogError($"[WhiskerTileVariants] Base prefab missing: {m.BasePrefabPath}");
                    skipped++;
                    continue;
                }

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(m.SpritePath);
                if (sprite == null)
                {
                    Debug.LogError($"[WhiskerTileVariants] Sprite missing (Texture Type must be Sprite): {m.SpritePath}");
                    skipped++;
                    continue;
                }

                var variantPath = $"{OutputDir}/{m.VariantName}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(variantPath) != null)
                {
                    Debug.Log($"[WhiskerTileVariants] Variant already exists, skipping: {variantPath}");
                    skipped++;
                    continue;
                }

                // Instantiate as connected prefab → SaveAsPrefabAsset on the connected
                // instance produces a Variant (its m_CorrespondingSourceObject points at
                // the base prefab and only the overridden sprite is recorded).
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                try
                {
                    var sr = instance.GetComponent<SpriteRenderer>();
                    if (sr == null)
                    {
                        Debug.LogError($"[WhiskerTileVariants] {m.BasePrefabPath} has no SpriteRenderer at root");
                        skipped++;
                        continue;
                    }
                    sr.sprite = sprite;

                    PrefabUtility.SaveAsPrefabAsset(instance, variantPath, out bool success);
                    if (success)
                    {
                        Debug.Log($"[WhiskerTileVariants] created: {variantPath} (base: {basePrefab.name}, sprite: {sprite.name})");
                        created++;
                    }
                    else
                    {
                        Debug.LogError($"[WhiskerTileVariants] SaveAsPrefabAsset returned false for {variantPath}");
                        skipped++;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WhiskerTileVariants] done — created={created}, skipped={skipped}");
        }
    }
}
