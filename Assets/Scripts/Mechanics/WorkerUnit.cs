using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class WorkerUnit : MonoBehaviour
    {
        public WorkerRole role = WorkerRole.Builder;
        public WorkerState state = WorkerState.InRoster;

        Vector3 homePosition;
        WorkStation currentStation;
        SpriteRenderer spriteRenderer;
        Collider2D pickCollider;

        public WorkStation CurrentStation => currentStation;

        public void Initialize(WorkerRole workerRole, Vector3 rosterHome)
        {
            role = workerRole;
            homePosition = rosterHome;
            transform.position = rosterHome;
            ApplyVisual();
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
    }
}
