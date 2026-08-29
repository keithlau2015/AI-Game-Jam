using Platformer.Model;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    public static class WorkFloorLayoutCreator
    {
        const string Root = "Assets/Data/WorkFloorLayouts";
        const string BackgroundPath = "Assets/Art/Backgrounds/FamilyHomeScene.jpg";

        [MenuItem("Platformer/Create Family Home Work Floor Layout")]
        public static void CreateFamilyHomeLayout()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(Root);

            var background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            if (background == null)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
                if (texture != null)
                {
                    var importer = AssetImporter.GetAtPath(BackgroundPath) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        importer.spritePixelsPerUnit = 120f;
                        importer.filterMode = FilterMode.Bilinear;
                        importer.SaveAndReimport();
                        background = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
                    }
                }
            }

            var layout = ScriptableObject.CreateInstance<WorkFloorLayout>();
            layout.backgroundSprite = background;
            layout.backgroundOrigin = new Vector2(-8f, -4.5f);
            layout.worldSize = new Vector2(16f, 9f);
            layout.cameraOrthographicSize = 4.5f;
            layout.cameraPosition = Vector2.zero;
            layout.showStationOverlay = true;
            layout.stationOverlayAlpha = 0.35f;
            layout.stations = new[]
            {
                Station("Study Desk", "Study Desk", new Vector2(0.74f, 0.66f), WorkStationMode.PermanentProduction, WorkerRole.Analyst, new Color(0.2f, 0.35f, 0.6f, 1f), 2, 3f, 0f, 0f, 0f, 0f, 0f, 0),
                Station("Kitchen Stove", "Kitchen Stove", new Vector2(0.22f, 0.4f), WorkStationMode.PermanentProduction, WorkerRole.Builder, new Color(0.55f, 0.3f, 0.2f, 1f), 2, 3f, 0f, 0f, 0f, 0f, 0f, 0),
                Station("Read With Child", "Read With Child", new Vector2(0.48f, 0.34f), WorkStationMode.TimedTask, WorkerRole.Analyst, new Color(0.85f, 0.35f, 0.55f, 1f), 1, 0f, 12f, 55f, 14f, 1.2f, 2f, 10),
                Station("Homework Desk", "Homework Desk", new Vector2(0.14f, 0.66f), WorkStationMode.TimedTask, WorkerRole.Courier, new Color(0.35f, 0.55f, 0.75f, 1f), 1, 0f, 8f, 50f, 12f, 1.2f, 2f, 8),
                Station("Prepare Dinner", "Prepare Dinner", new Vector2(0.28f, 0.28f), WorkStationMode.TimedTask, WorkerRole.Builder, new Color(0.75f, 0.45f, 0.2f, 1f), 1, 0f, 18f, 65f, 16f, 1.2f, 2f, 12),
                Station("Water Garden", "Water Garden", new Vector2(0.84f, 0.36f), WorkStationMode.TimedTask, WorkerRole.Courier, new Color(0.2f, 0.5f, 0.25f, 1f), 1, 0f, 5f, 45f, 10f, 1.2f, 2f, 10)
            };
            layout.rosterSlots = new[]
            {
                Roster(new Vector2(0.04f, 0.72f), WorkerRole.Builder),
                Roster(new Vector2(0.04f, 0.58f), WorkerRole.Analyst),
                Roster(new Vector2(0.04f, 0.44f), WorkerRole.Courier),
                Roster(new Vector2(0.04f, 0.3f), WorkerRole.Builder)
            };

            var path = $"{Root}/FamilyHomeLayout.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WorkFloorLayout>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);

            AssetDatabase.CreateAsset(layout, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = layout;
            EditorGUIUtility.PingObject(layout);
        }

        static WorkStationDefinition Station(
            string stationId,
            string displayLabel,
            Vector2 normalizedPosition,
            WorkStationMode mode,
            WorkerRole role,
            Color tint,
            int capacity,
            float outputPerWorker,
            float spawnStart,
            float spawnEnd,
            float duration,
            float progressPerWorker,
            float speedMultiplier,
            int reward)
        {
            return new WorkStationDefinition
            {
                stationId = stationId,
                displayLabel = displayLabel,
                normalizedPosition = normalizedPosition,
                mode = mode,
                requiredRole = role,
                tint = tint,
                capacity = capacity,
                outputPerWorker = outputPerWorker,
                spawnWindowStart = spawnStart,
                spawnWindowEnd = spawnEnd,
                taskDuration = duration,
                taskProgressPerWorker = progressPerWorker,
                correctWorkerSpeedMultiplier = speedMultiplier,
                taskOutputReward = reward,
                colliderSize = new Vector2(2.2f, 1.8f),
                visualScale = new Vector2(2.2f, 1.5f)
            };
        }

        static RosterSlotDefinition Roster(Vector2 normalizedPosition, WorkerRole role)
        {
            return new RosterSlotDefinition
            {
                normalizedPosition = normalizedPosition,
                role = role
            };
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
