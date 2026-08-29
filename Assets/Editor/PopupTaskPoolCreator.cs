using Platformer.Model;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    public static class PopupTaskPoolCreator
    {
        const string CsvPath = "Assets/Data/PopupTasks/FamilyPopupTasks.csv";
        const string PoolPath = "Assets/Resources/FamilyPopupTaskPool.asset";

        [MenuItem("Platformer/Create Family Popup Task Pool")]
        public static void CreatePool()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder("Assets/Data/PopupTasks");
            EnsureFolder("Assets/Resources");

            var csv = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvPath);
            if (csv == null)
            {
                Debug.LogError($"Missing CSV at {CsvPath}");
                return;
            }

            var pool = AssetDatabase.LoadAssetAtPath<PopupTaskPool>(PoolPath);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<PopupTaskPool>();
                AssetDatabase.CreateAsset(pool, PoolPath);
            }

            pool.csvSource = csv;
            pool.maxConcurrentTasks = 3;
            pool.minGapBetweenSpawns = 8f;
            pool.ReloadFromCsv();
            EditorUtility.SetDirty(pool);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = pool;
            Debug.Log($"Popup task pool ready with {pool.Tasks.Count} tasks.");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            var folderName = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
