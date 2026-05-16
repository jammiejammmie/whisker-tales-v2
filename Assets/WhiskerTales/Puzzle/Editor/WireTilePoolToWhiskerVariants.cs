using System;
using System.Collections.Generic;
using System.Reflection;
using GameVanilla.Core;
using GameVanilla.Game.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhiskerTales.Puzzle.Editor
{
    /// <summary>
    /// One-shot Editor menu that wires WhiskerGameScene's TilePool so every
    /// child ObjectPool.prefab points at a Whisker tile variant. Permanent
    /// scene-level fix that replaces the runtime WhiskerGameBoardHotfix donor
    /// patch.
    ///
    /// Color mapping (applied to base + *HorizontalStriped + *VerticalStriped
    /// + *Wrapped pool slots):
    ///   Blue → FishTile      Green  → CatnipTile     Orange → FishboneTile
    ///   Purple → YarnTile    Red    → PawprintTile   Yellow → MilkTile
    ///
    /// Specials (colorBomb, honey/ice/syrup, marshmallow/chocolate/unbreakable,
    /// cherry/watermelon, lightBg/darkBg) have no per-slot Whisker art and
    /// fall back to FishTile so InitializeObjectPools no longer crashes on
    /// null prefabs. These pool types aren't requested by Level 1.
    /// </summary>
    public static class WireTilePoolToWhiskerVariants
    {
        private const string ScenePath = "Assets/WhiskerTales/Puzzle/Scenes/WhiskerGameScene.unity";
        private const string VariantDir = "Assets/WhiskerTales/Puzzle/Skin/WhiskerTilePrefabs";
        private const string FallbackVariantName = "FishTile";

        private static readonly Dictionary<string, string> ColorPrefixToVariantName =
            new Dictionary<string, string>
            {
                { "blue",   "FishTile" },
                { "green",  "CatnipTile" },
                { "orange", "FishboneTile" },
                { "purple", "YarnTile" },
                { "red",    "PawprintTile" },
                { "yellow", "MilkTile" },
            };

        [MenuItem("WhiskerTales/Tiles/Wire TilePool to Whisker Variants")]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var variants = new Dictionary<string, GameObject>();
            foreach (var name in new HashSet<string>(ColorPrefixToVariantName.Values))
            {
                var path = $"{VariantDir}/{name}.prefab";
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null)
                {
                    EditorUtility.DisplayDialog(
                        "WireTilePool",
                        $"Missing variant prefab: {path}\n\nRun 'Create Whisker Tile Variants' first.",
                        "OK");
                    return;
                }
                variants[name] = go;
            }

            TilePool tilePool = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                tilePool = root.GetComponentInChildren<TilePool>(true);
                if (tilePool != null) break;
            }
            if (tilePool == null)
            {
                EditorUtility.DisplayDialog(
                    "WireTilePool",
                    $"TilePool not found in scene {scene.name}.",
                    "OK");
                return;
            }

            int rewired = 0;
            int unchanged = 0;
            int skipped = 0;

            var fields = typeof(TilePool).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.FieldType != typeof(ObjectPool)) continue;

                var pool = field.GetValue(tilePool) as ObjectPool;
                if (pool == null)
                {
                    Debug.LogWarning($"[WireTilePool] TilePool.{field.Name} is null in Inspector — skipping");
                    skipped++;
                    continue;
                }

                var variantName = ResolveVariantName(field.Name);
                var variant = variants[variantName];

                var so = new SerializedObject(pool);
                var prop = so.FindProperty("prefab");
                if (prop == null)
                {
                    Debug.LogError($"[WireTilePool] {pool.gameObject.name}: no 'prefab' SerializedProperty");
                    skipped++;
                    continue;
                }

                var before = prop.objectReferenceValue;
                if (before == variant)
                {
                    unchanged++;
                    continue;
                }

                prop.objectReferenceValue = variant;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(pool);

                Debug.Log($"[WireTilePool] {field.Name} → {variant.name}   (was: {(before != null ? before.name : "null")})");
                rewired++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            var summary = $"rewired={rewired}, unchanged={unchanged}, skipped={skipped}";
            Debug.Log($"[WireTilePool] done — {summary}");
            EditorUtility.DisplayDialog(
                "WireTilePool",
                $"TilePool wired to Whisker variants.\n\n{summary}\n\nScene saved.",
                "OK");
        }

        private static string ResolveVariantName(string fieldName)
        {
            foreach (var kvp in ColorPrefixToVariantName)
            {
                if (fieldName.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }
            return FallbackVariantName;
        }
    }
}
