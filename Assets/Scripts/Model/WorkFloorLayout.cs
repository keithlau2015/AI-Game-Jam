using System;
using UnityEngine;

namespace Platformer.Model
{
    [CreateAssetMenu(fileName = "WorkFloorLayout", menuName = "Platformer/Work Floor Layout")]
    public class WorkFloorLayout : ScriptableObject
    {
        public Sprite backgroundSprite;
        public Vector2 backgroundOrigin = new Vector2(-8f, -4.5f);
        public Vector2 worldSize = new Vector2(16f, 9f);
        public float cameraOrthographicSize = 4.5f;
        public Vector2 cameraPosition = Vector2.zero;
        public bool showStationOverlay = true;
        public float stationOverlayAlpha = 0.35f;
        public WorkStationDefinition[] stations = Array.Empty<WorkStationDefinition>();
        public RosterSlotDefinition[] rosterSlots = Array.Empty<RosterSlotDefinition>();

        public Vector3 NormalizedToWorld(Vector2 normalized)
        {
            return new Vector3(
                backgroundOrigin.x + normalized.x * worldSize.x,
                backgroundOrigin.y + normalized.y * worldSize.y,
                0f);
        }

        public Vector2 WorldToNormalized(Vector3 world)
        {
            if (worldSize.x <= 0f || worldSize.y <= 0f)
                return Vector2.zero;

            return new Vector2(
                (world.x - backgroundOrigin.x) / worldSize.x,
                (world.y - backgroundOrigin.y) / worldSize.y);
        }
    }

    [Serializable]
    public class WorkStationDefinition
    {
        public string stationId = "Station";
        public string displayLabel = "Station";
        public Vector2 normalizedPosition = new Vector2(0.5f, 0.5f);
        public WorkStationMode mode = WorkStationMode.PermanentProduction;
        public WorkerRole requiredRole = WorkerRole.Any;
        public bool acceptAnyMember;
        public WorkerColor allowedMemberColors = WorkerColor.None;
        public int capacity = 1;
        public float outputPerWorker = 3f;
        public float spawnWindowStart = 10f;
        public float spawnWindowEnd = 60f;
        public float taskDuration = 12f;
        public float taskProgressPerWorker = 1.2f;
        public float correctWorkerSpeedMultiplier = 2f;
        public float activeTaskRoundTimeBonus = 0.35f;
        public int taskOutputReward = 10;
        public Vector2 colliderSize = new Vector2(2.4f, 2.2f);
        public Vector2 visualScale = new Vector2(2.4f, 1.6f);
        public Color tint = Color.white;
    }

    [Serializable]
    public class RosterSlotDefinition
    {
        public string displayName = "Worker";
        public Vector2 normalizedPosition = new Vector2(0.05f, 0.5f);
        public WorkerRole role = WorkerRole.Builder;
    }
}
