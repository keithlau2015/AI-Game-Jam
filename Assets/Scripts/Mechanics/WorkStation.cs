using System.Collections.Generic;
using Platformer.Model;
using TMPro;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class WorkStation : MonoBehaviour
    {
        public string stationId;
        public WorkStationMode mode = WorkStationMode.PermanentProduction;
        public WorkerRole requiredRole = WorkerRole.Any;
        public bool acceptAnyMember;
        public WorkerColor allowedMemberColors = WorkerColor.All;
        public int capacity = 1;
        public float outputPerWorker = 3f;
        public bool disabled;

        public float spawnWindowStart = 10f;
        public float spawnWindowEnd = 60f;
        public float taskDuration = 12f;
        public float taskProgressPerWorker = 1f;
        public float correctWorkerSpeedMultiplier = 2f;
        public float activeTaskRoundTimeBonus = 0.35f;
        public int taskOutputReward = 8;

        readonly List<WorkerUnit> assignedWorkers = new List<WorkerUnit>();

        float scheduledSpawnTime;
        float taskProgress;
        TaskAreaPhase taskPhase = TaskAreaPhase.WaitingToSpawn;
        TextMeshPro statusLabel;
        Color baseColor = Color.white;
        PopupTaskBehaviour popupBehaviour;

        public IReadOnlyList<WorkerUnit> AssignedWorkers => assignedWorkers;
        public bool IsSpawned => mode == WorkStationMode.PermanentProduction || taskPhase == TaskAreaPhase.Active;
        public bool IsActive => IsSpawned && !disabled && taskPhase != TaskAreaPhase.Completed && taskPhase != TaskAreaPhase.MissedSpawn && taskPhase != TaskAreaPhase.Failed;
        public bool HasSpace => IsActive && assignedWorkers.Count < capacity;
        public float ActiveTaskRoundTimeBonus => IsActive && HasCorrectWorker() ? activeTaskRoundTimeBonus : 0f;

        public bool CanAccept(WorkerUnit worker)
        {
            if (worker == null || !HasSpace)
                return false;

            EnsurePopupBehaviour();
            if (popupBehaviour != null && popupBehaviour.IsPopupTask)
                return popupBehaviour.CanAccept(worker);

            return MatchesMemberColor(worker);
        }

        public bool MatchesMemberColor(WorkerUnit worker)
        {
            if (worker == null)
                return false;

            if (acceptAnyMember)
                return true;

            var allowed = ResolveAllowedMemberColors();
            if (allowed == WorkerColor.All)
                return true;

            return (allowed & worker.GetMemberColor()) != 0;
        }

        public bool TryAssign(WorkerUnit worker)
        {
            if (!CanAccept(worker))
                return false;

            assignedWorkers.Add(worker);
            worker.AssignToStation(this);
            UpdateStatusLabel();
            return true;
        }

        public void RemoveWorker(WorkerUnit worker)
        {
            if (worker == null)
                return;
            assignedWorkers.Remove(worker);
            UpdateStatusLabel();
        }

        public float GetOutputPerSecond()
        {
            if (!IsActive || disabled || mode != WorkStationMode.PermanentProduction || assignedWorkers.Count == 0)
                return 0f;

            var total = 0f;
            for (var i = 0; i < assignedWorkers.Count; i++)
                total += outputPerWorker * assignedWorkers[i].GetEfficiencyForStation(this);
            return total;
        }

        public void PrepareForRound()
        {
            taskProgress = 0f;
            disabled = false;
            ReturnAllWorkersHome();

            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
                baseColor = sprite.color;

            if (mode == WorkStationMode.TimedTask)
            {
                taskPhase = TaskAreaPhase.WaitingToSpawn;
                scheduledSpawnTime = Random.Range(spawnWindowStart, spawnWindowEnd);
                SetVisible(false);
                return;
            }

            taskPhase = TaskAreaPhase.Active;
            SetVisible(true);
            UpdateStatusLabel();
        }

        public void TickSpawn(float elapsedRoundTime)
        {
            if (mode != WorkStationMode.TimedTask || taskPhase != TaskAreaPhase.WaitingToSpawn)
                return;

            if (elapsedRoundTime > spawnWindowEnd)
            {
                taskPhase = TaskAreaPhase.MissedSpawn;
                SetVisible(false);
                return;
            }

            if (elapsedRoundTime < scheduledSpawnTime)
                return;

            taskPhase = TaskAreaPhase.Active;
            SetVisible(true);
            UpdateStatusLabel();
        }

        public void TickTask(float deltaTime)
        {
            if (mode != WorkStationMode.TimedTask || taskPhase != TaskAreaPhase.Active)
                return;

            EnsurePopupBehaviour();
            if (popupBehaviour != null && popupBehaviour.IsPopupTask)
            {
                if (!popupBehaviour.HasValidAssignment())
                {
                    UpdateStatusLabel();
                    return;
                }

                taskProgress += deltaTime;
                UpdateStatusLabel();
                if (taskProgress < taskDuration)
                    return;

                CompleteTask();
                return;
            }

            if (!HasCorrectWorker())
            {
                UpdateStatusLabel();
                return;
            }

            var rate = 0f;
            for (var i = 0; i < assignedWorkers.Count; i++)
            {
                var worker = assignedWorkers[i];
                if (!IsCorrectWorker(worker))
                    continue;
                rate += taskProgressPerWorker * worker.GetEfficiencyForStation(this) * correctWorkerSpeedMultiplier;
            }

            taskProgress += rate * deltaTime;
            UpdateStatusLabel();

            if (taskProgress < taskDuration)
                return;

            CompleteTask();
        }

        public void CompleteTask()
        {
            if (taskPhase == TaskAreaPhase.Completed)
                return;

            EnsurePopupBehaviour();
            if (popupBehaviour != null && popupBehaviour.IsPopupTask)
            {
                popupBehaviour.OnTaskCompleted();
                return;
            }

            taskPhase = TaskAreaPhase.Completed;
            ReturnAllWorkersHome();

            if (RoundController.Instance != null)
                RoundController.Instance.AddOutput(taskOutputReward);

            SetVisible(false);
        }

        public void ForceActivatePopup()
        {
            taskPhase = TaskAreaPhase.Active;
            taskProgress = 0f;
            SetVisible(true);
            UpdateStatusLabel();
        }

        public void ForceClosePopup()
        {
            taskPhase = TaskAreaPhase.Failed;
            ReturnAllWorkersHome();
            SetVisible(false);
            gameObject.SetActive(false);
        }

        public void ForceCompletePopup()
        {
            taskPhase = TaskAreaPhase.Completed;
            ReturnAllWorkersHome();
            SetVisible(false);
            gameObject.SetActive(false);
        }

        public void SetDisabled(bool value)
        {
            if (!IsActive)
                return;

            disabled = value;
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.color = value ? new Color(0.35f, 0.35f, 0.35f, 1f) : baseColor;
        }

        void ReturnAllWorkersHome()
        {
            for (var i = assignedWorkers.Count - 1; i >= 0; i--)
                assignedWorkers[i].ReturnHome();
            assignedWorkers.Clear();
        }

        bool HasCorrectWorker()
        {
            for (var i = 0; i < assignedWorkers.Count; i++)
            {
                if (IsCorrectWorker(assignedWorkers[i]))
                    return true;
            }
            return false;
        }

        bool IsCorrectWorker(WorkerUnit worker)
        {
            return MatchesMemberColor(worker);
        }

        public WorkerColor GetAllowedMemberColors()
        {
            return ResolveAllowedMemberColors();
        }

        WorkerColor ResolveAllowedMemberColors()
        {
            if (allowedMemberColors != WorkerColor.None)
                return allowedMemberColors;

            if (requiredRole == WorkerRole.Any)
                return WorkerColor.All;

            return WorkerColorRules.FromRole(requiredRole);
        }

        string GetMemberRestrictionLabel()
        {
            return WorkerColorRules.BuildAllowedColorsLabel(acceptAnyMember, ResolveAllowedMemberColors());
        }

        void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = visible;
        }

        void UpdateStatusLabel()
        {
            EnsureStatusLabel();
            if (statusLabel == null)
                return;

            if (mode == WorkStationMode.TimedTask)
            {
                if (taskPhase == TaskAreaPhase.WaitingToSpawn)
                {
                    statusLabel.gameObject.SetActive(false);
                    return;
                }

                statusLabel.gameObject.SetActive(true);
                if (taskPhase == TaskAreaPhase.Active)
                {
                    var memberText = popupBehaviour != null && popupBehaviour.IsPopupTask
                        ? popupBehaviour.RequirementLabel
                        : GetMemberRestrictionLabel();
                    var progress = taskDuration > 0f ? Mathf.Clamp01(taskProgress / taskDuration) : 0f;
                    statusLabel.text = $"{memberText} {assignedWorkers.Count}/{capacity} {Mathf.RoundToInt(progress * 100f)}%";
                    return;
                }

                statusLabel.text = string.Empty;
                return;
            }

            statusLabel.gameObject.SetActive(true);
            statusLabel.text = $"{GetMemberRestrictionLabel()} {assignedWorkers.Count}/{capacity}";
        }

        void EnsureStatusLabel()
        {
            if (statusLabel != null)
                return;

            var labelObject = transform.Find("StatusLabel");
            if (labelObject != null)
            {
                statusLabel = labelObject.GetComponent<TextMeshPro>();
                return;
            }

            var created = new GameObject("StatusLabel", typeof(TextMeshPro));
            created.transform.SetParent(transform, false);
            created.transform.localPosition = new Vector3(0f, -0.85f, 0f);
            statusLabel = created.GetComponent<TextMeshPro>();
            statusLabel.fontSize = 2f;
            statusLabel.alignment = TextAlignmentOptions.Center;
            statusLabel.color = Color.white;
        }

        void EnsurePopupBehaviour()
        {
            if (popupBehaviour == null)
                popupBehaviour = GetComponent<PopupTaskBehaviour>();
        }
    }
}
