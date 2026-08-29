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
                AnyPermanent("Family Lounge", "Family Lounge", new Vector2(0.58f, 0.52f), new Color(0.45f, 0.4f, 0.55f, 1f), 4, 2.5f),
                ColorPermanent("Study Desk", "Study Desk", new Vector2(0.74f, 0.66f), WorkerColor.Blue, new Color(0.2f, 0.35f, 0.6f, 1f), 2, 3f),
                ColorPermanent("Kitchen Stove", "Kitchen Stove", new Vector2(0.22f, 0.4f), WorkerColor.Orange, new Color(0.55f, 0.3f, 0.2f, 1f), 2, 3f),
                ColorTimed("Read With Child", "Read With Child", new Vector2(0.48f, 0.34f), WorkerColor.Blue, new Color(0.85f, 0.35f, 0.55f, 1f), 1, 12f, 55f, 14f, 10),
                ColorTimed("Homework Desk", "Homework Desk", new Vector2(0.14f, 0.66f), WorkerColor.Green, new Color(0.35f, 0.55f, 0.75f, 1f), 1, 8f, 50f, 12f, 8),
                ColorTimed("Prepare Dinner", "Prepare Dinner", new Vector2(0.28f, 0.28f), WorkerColor.Orange, new Color(0.75f, 0.45f, 0.2f, 1f), 1, 18f, 65f, 16f, 12),
                AnyTimed("Movie Night", "Movie Night", new Vector2(0.84f, 0.36f), new Color(0.25f, 0.2f, 0.45f, 1f), 4, 5f, 45f, 10f, 10)
            };
            layout.rosterSlots = new[]
            {
                Roster("Dad", new Vector2(0.18f, 0.06f), WorkerRole.Analyst),
                Roster("Mom", new Vector2(0.4f, 0.06f), WorkerRole.Builder),
                Roster("Mia", new Vector2(0.62f, 0.06f), WorkerRole.Courier),
                Roster("Leo", new Vector2(0.84f, 0.06f), WorkerRole.Builder)
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

        static WorkStationDefinition AnyPermanent(string stationId, string displayLabel, Vector2 normalizedPosition, Color tint, int capacity, float outputPerWorker)
        {
            return new WorkStationDefinition
            {
                stationId = stationId,
                displayLabel = displayLabel,
                normalizedPosition = normalizedPosition,
                mode = WorkStationMode.PermanentProduction,
                acceptAnyMember = true,
                allowedMemberColors = WorkerColor.All,
                capacity = capacity,
                outputPerWorker = outputPerWorker,
                tint = tint,
                colliderSize = new Vector2(2.8f, 2.2f),
                visualScale = new Vector2(2.8f, 1.8f)
            };
        }

        static WorkStationDefinition ColorPermanent(string stationId, string displayLabel, Vector2 normalizedPosition, WorkerColor allowedColor, Color tint, int capacity, float outputPerWorker)
        {
            return new WorkStationDefinition
            {
                stationId = stationId,
                displayLabel = displayLabel,
                normalizedPosition = normalizedPosition,
                mode = WorkStationMode.PermanentProduction,
                requiredRole = RoleFromColor(allowedColor),
                acceptAnyMember = false,
                allowedMemberColors = allowedColor,
                capacity = capacity,
                outputPerWorker = outputPerWorker,
                tint = tint,
                colliderSize = new Vector2(2.2f, 1.8f),
                visualScale = new Vector2(2.2f, 1.5f)
            };
        }

        static WorkStationDefinition ColorTimed(string stationId, string displayLabel, Vector2 normalizedPosition, WorkerColor allowedColor, Color tint, int capacity, float spawnStart, float spawnEnd, float duration, int reward)
        {
            return new WorkStationDefinition
            {
                stationId = stationId,
                displayLabel = displayLabel,
                normalizedPosition = normalizedPosition,
                mode = WorkStationMode.TimedTask,
                requiredRole = RoleFromColor(allowedColor),
                acceptAnyMember = false,
                allowedMemberColors = allowedColor,
                capacity = capacity,
                spawnWindowStart = spawnStart,
                spawnWindowEnd = spawnEnd,
                taskDuration = duration,
                taskProgressPerWorker = 1.2f,
                correctWorkerSpeedMultiplier = 2f,
                activeTaskRoundTimeBonus = 0.35f,
                taskOutputReward = reward,
                tint = tint,
                colliderSize = new Vector2(2.2f, 1.8f),
                visualScale = new Vector2(2.2f, 1.5f)
            };
        }

        static WorkStationDefinition AnyTimed(string stationId, string displayLabel, Vector2 normalizedPosition, Color tint, int capacity, float spawnStart, float spawnEnd, float duration, int reward)
        {
            return new WorkStationDefinition
            {
                stationId = stationId,
                displayLabel = displayLabel,
                normalizedPosition = normalizedPosition,
                mode = WorkStationMode.TimedTask,
                acceptAnyMember = true,
                allowedMemberColors = WorkerColor.All,
                capacity = capacity,
                spawnWindowStart = spawnStart,
                spawnWindowEnd = spawnEnd,
                taskDuration = duration,
                taskProgressPerWorker = 1.2f,
                correctWorkerSpeedMultiplier = 2f,
                activeTaskRoundTimeBonus = 0.35f,
                taskOutputReward = reward,
                tint = tint,
                colliderSize = new Vector2(2.8f, 2.2f),
                visualScale = new Vector2(2.8f, 1.8f)
            };
        }

        static WorkerRole RoleFromColor(WorkerColor color)
        {
            return color switch
            {
                WorkerColor.Orange => WorkerRole.Builder,
                WorkerColor.Blue => WorkerRole.Analyst,
                WorkerColor.Green => WorkerRole.Courier,
                _ => WorkerRole.Any
            };
        }

        static RosterSlotDefinition Roster(string displayName, Vector2 normalizedPosition, WorkerRole role)
        {
            return new RosterSlotDefinition
            {
                displayName = displayName,
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
