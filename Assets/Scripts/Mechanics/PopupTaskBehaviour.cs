using Platformer.Model;
using TMPro;
using UnityEngine;

namespace Platformer.Mechanics
{
    [RequireComponent(typeof(WorkStation))]
    public class PopupTaskBehaviour : MonoBehaviour
    {
        public PopupTaskDefinition definition;

        WorkStation station;
        float existenceRemaining;
        bool resolved;

        public bool IsPopupTask => definition != null;
        public string RequirementLabel => definition != null
            ? PopupTaskRequirementValidator.BuildRequirementLabel(definition.requirement, definition.roleRequirementRaw)
            : string.Empty;

        void Awake()
        {
            station = GetComponent<WorkStation>();
        }

        public void Configure(PopupTaskDefinition taskDefinition)
        {
            definition = taskDefinition;
            if (station == null)
                station = GetComponent<WorkStation>();

            if (definition == null || station == null)
                return;

            station.stationId = $"{definition.icon} {definition.title}";
            station.mode = WorkStationMode.TimedTask;
            station.acceptAnyMember = false;
            station.allowedMemberColors = WorkerColor.All;
            station.capacity = definition.maxParticipants;
            station.spawnWindowStart = definition.spawnTimeMin;
            station.spawnWindowEnd = definition.spawnTimeMax;
            station.taskDuration = definition.workDuration;
            station.taskProgressPerWorker = 1f;
            station.correctWorkerSpeedMultiplier = 1f;
            station.taskOutputReward = 0;
            existenceRemaining = definition.existenceDuration;
            resolved = false;
            EnsureIconLabel();
        }

        public void TickExistence(float deltaTime)
        {
            if (definition == null || resolved || station == null || !station.IsSpawned)
                return;

            existenceRemaining -= deltaTime;
            if (existenceRemaining > 0f)
                return;

            ResolveFailure();
        }

        public bool CanAccept(WorkerUnit worker)
        {
            if (definition == null || worker == null || station == null)
                return false;

            return PopupTaskRequirementValidator.CanAcceptMember(
                definition.requirement,
                worker,
                station.AssignedWorkers);
        }

        public bool HasValidAssignment()
        {
            if (definition == null || station == null)
                return false;

            return PopupTaskRequirementValidator.MeetsRequirement(
                definition.requirement,
                station.AssignedWorkers);
        }

        public void OnTaskCompleted()
        {
            if (resolved || definition == null)
                return;

            if (HasValidAssignment())
                ResolveSuccess();
            else
                ResolveFailure();
        }

        public void OnTaskExpired()
        {
            if (!resolved)
                ResolveFailure();
        }

        void ResolveSuccess()
        {
            if (resolved)
                return;
            resolved = true;
            var participants = GatherParticipants();
            PopupTaskOutcomeApplier.Apply(definition.successEffects, Core.Simulation.GetModel<SessionModel>(), participants);
            if (GameAudioController.Instance != null)
                GameAudioController.Instance.PlayTaskSuccess();
            if (station != null)
                station.ForceCompletePopup();
        }

        void ResolveFailure()
        {
            if (resolved)
                return;
            resolved = true;
            var participants = GatherParticipants();
            PopupTaskOutcomeApplier.Apply(definition.failureEffects, Core.Simulation.GetModel<SessionModel>(), participants);
            if (GameAudioController.Instance != null)
                GameAudioController.Instance.PlayTaskFail();
            HideStation();
        }

        WorkerUnit[] GatherParticipants()
        {
            if (station == null || station.AssignedWorkers.Count == 0)
                return System.Array.Empty<WorkerUnit>();

            var list = new WorkerUnit[station.AssignedWorkers.Count];
            for (var i = 0; i < station.AssignedWorkers.Count; i++)
                list[i] = station.AssignedWorkers[i];
            return list;
        }

        void HideStation()
        {
            if (station != null)
                station.ForceClosePopup();
        }

        void EnsureIconLabel()
        {
            if (definition == null)
                return;

            var labelObject = transform.Find("PopupIcon");
            TextMeshPro label;
            if (labelObject == null)
            {
                var created = new GameObject("PopupIcon", typeof(TextMeshPro));
                created.transform.SetParent(transform, false);
                created.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                label = created.GetComponent<TextMeshPro>();
                label.fontSize = 4f;
                label.alignment = TextAlignmentOptions.Center;
            }
            else
            {
                label = labelObject.GetComponent<TextMeshPro>();
            }

            if (label != null)
                label.text = definition.icon;
        }
    }
}
