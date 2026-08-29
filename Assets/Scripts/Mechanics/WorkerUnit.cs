using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using TMPro;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class WorkerUnit : MonoBehaviour
    {
        public WorkerRole role = WorkerRole.Builder;
        public WorkerState state = WorkerState.InRoster;
        public WorkerAttributes attributes;

        Vector3 homePosition;
        WorkStation currentStation;
        SpriteRenderer spriteRenderer;
        Collider2D pickCollider;
        TextMeshPro rosterLabel;

        public WorkStation CurrentStation => currentStation;

        public void Initialize(WorkerRole workerRole, Vector3 rosterHome)
        {
            role = workerRole;
            homePosition = rosterHome;
            attributes = WorkerAttributes.CreateRandom(workerRole);
            transform.position = rosterHome;
            ApplyVisual();
            UpdateRosterLabel();
        }

        public WorkerRole GetJobRoleForStation(WorkStation station)
        {
            if (station == null)
                return role;

            return station.requiredRole == WorkerRole.Any ? role : station.requiredRole;
        }

        public float GetEfficiencyForStation(WorkStation station)
        {
            return attributes.GetEfficiency(GetJobRoleForStation(station));
        }

        public int GetSkillForStation(WorkStation station)
        {
            return attributes.GetSkill(GetJobRoleForStation(station));
        }

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            pickCollider = GetComponent<Collider2D>();
        }

        public void BeginDrag()
        {
            state = WorkerState.Dragging;
            if (currentStation != null)
            {
                currentStation.RemoveWorker(this);
                currentStation = null;
            }
            if (pickCollider != null)
                pickCollider.enabled = false;
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 20;
        }

        public void UpdateDragPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public void CancelDrag()
        {
            ReturnHome();
        }

        public void AssignToStation(WorkStation station)
        {
            currentStation = station;
            state = WorkerState.Working;
            if (pickCollider != null)
                pickCollider.enabled = true;
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 5;

            var slotIndex = station.AssignedWorkers.Count - 1;
            var offset = new Vector3((slotIndex % 2) * 0.6f - 0.3f, (slotIndex / 2) * 0.5f, 0f);
            transform.position = station.transform.position + offset;

            var ev = Schedule<WorkerAssigned>();
            ev.worker = this;
            ev.station = station;
        }

        public void ReturnHome()
        {
            if (currentStation != null)
            {
                currentStation.RemoveWorker(this);
                currentStation = null;
            }
            state = WorkerState.InRoster;
            transform.position = homePosition;
            if (pickCollider != null)
                pickCollider.enabled = true;
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 10;
        }

        public void SetPickEnabled(bool enabled)
        {
            if (pickCollider != null)
                pickCollider.enabled = enabled;
        }

        void ApplyVisual()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.color = role switch
            {
                WorkerRole.Builder => new Color(0.9f, 0.45f, 0.2f, 1f),
                WorkerRole.Analyst => new Color(0.3f, 0.65f, 0.95f, 1f),
                WorkerRole.Courier => new Color(0.45f, 0.85f, 0.4f, 1f),
                _ => new Color(0.8f, 0.8f, 0.8f, 1f)
            };
        }

        void UpdateRosterLabel()
        {
            if (rosterLabel == null)
            {
                var labelObject = new GameObject("SkillLabel", typeof(TextMeshPro));
                labelObject.transform.SetParent(transform, false);
                labelObject.transform.localPosition = new Vector3(0f, -0.75f, 0f);
                rosterLabel = labelObject.GetComponent<TextMeshPro>();
                rosterLabel.fontSize = 1.6f;
                rosterLabel.alignment = TextAlignmentOptions.Center;
                rosterLabel.color = Color.white;
            }

            rosterLabel.text = $"B{attributes.builderSkill} A{attributes.analystSkill} C{attributes.courierSkill}";
        }
    }
}
