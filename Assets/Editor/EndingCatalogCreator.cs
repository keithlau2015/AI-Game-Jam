using Platformer.Model;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    public static class EndingCatalogCreator
    {
        const string Root = "Assets/Data/Endings";
        const string BackgroundPath = "Assets/Art/Backgrounds/FamilyHomeScene.jpg";

        [MenuItem("Platformer/Create Family Ending Catalog")]
        public static void CreateCatalog()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(Root);

            var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            var catalog = ScriptableObject.CreateInstance<EndingCatalog>();
            catalog.defaultImage = background;
            catalog.defeatEnding = CreateEnding(Root, "Ending_Defeat", "A Rough Day", "The family ended the day tired and out of sync. Tomorrow can still feel warmer.");
            catalog.timeExpiredEnding = CreateEnding(Root, "Ending_TimeExpired", "Too Much at Once", "There was more to do than time allowed. The house grew quiet before anyone felt finished.");
            catalog.statLossEnding = CreateEnding(Root, "Ending_StatLoss", "Emotions Overflow", "Stress spread faster than anyone could comfort each other.");
            catalog.campaignCompleteEnding = CreateEnding(Root, "Ending_CampaignComplete", "Four Days Together", "After four days, the family found its own rhythm. Not perfect, but theirs.");
            catalog.builderFocusedEnding = CreateEnding(Root, "Ending_Builder", "Warm Kitchen Evenings", "Shared meals and small repairs made the home feel steady again.");
            catalog.analystFocusedEnding = CreateEnding(Root, "Ending_Analyst", "Quiet Understanding", "Books, plans, and patient talks helped everyone feel heard.");
            catalog.courierFocusedEnding = CreateEnding(Root, "Ending_Courier", "Busy Happy Home", "There was always someone moving, helping, and bringing life into every room.");
            catalog.balancedEnding = CreateEnding(Root, "Ending_Balanced", "Balanced Family Life", "Everyone contributed in their own way, and the day ended in gentle harmony.");

            AssignImage(catalog.defeatEnding, background);
            AssignImage(catalog.timeExpiredEnding, background);
            AssignImage(catalog.statLossEnding, background);
            AssignImage(catalog.campaignCompleteEnding, background);
            AssignImage(catalog.builderFocusedEnding, background);
            AssignImage(catalog.analystFocusedEnding, background);
            AssignImage(catalog.courierFocusedEnding, background);
            AssignImage(catalog.balancedEnding, background);

            var path = $"{Root}/FamilyEndingCatalog.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EndingCatalog>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);

            AssetDatabase.CreateAsset(catalog, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
        }

        static EndingDefinition CreateEnding(string folder, string fileName, string title, string description)
        {
            var ending = ScriptableObject.CreateInstance<EndingDefinition>();
            ending.title = title;
            ending.description = description;
            AssetDatabase.CreateAsset(ending, $"{folder}/{fileName}.asset");
            return ending;
        }

        static void AssignImage(EndingDefinition ending, Sprite image)
        {
            if (ending == null)
                return;

            ending.image = image;
            EditorUtility.SetDirty(ending);
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
