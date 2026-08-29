using System.Collections.Generic;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class WorkStation : MonoBehaviour
    {
        public string stationId;
        public WorkerRole requiredRole = WorkerRole.Any;
        public int capacity = 2;
        public float outputPerWorker = 3f;
        public bool disabled;

        readonly List<WorkerUnit> assignedWorkers = new List<WorkerUnit>();

        public IReadOnlyList<WorkerUnit> AssignedWorkers => assignedWorkers;

        public bool HasSpace => !disabled && assignedWorkers.Count < capacity;

        public bool CanAccept(WorkerUnit worker)
        {
            if (worker == null || !HasSpace)
                return false;
            if (requiredRole != WorkerRole.Any && worker.role != requiredRole)
                return false;
            return true;
        }

        public bool TryAssign(WorkerUnit worker)
        {
            if (!CanAccept(worker))
                return false;

            assignedWorkers.Add(worker);
            worker.AssignToStation(this);
            return true;
        }

        public void RemoveWorker(WorkerUnit worker)
        {
            if (worker == null)
                return;
            assignedWorkers.Remove(worker);
        }

        public float GetOutputPerSecond()
        {
            if (disabled || assignedWorkers.Count == 0)
                return 0f;
            return assignedWorkers.Count * outputPerWorker;
        }

        public void SetDisabled(bool value)
        {
            disabled = value;
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null)
                sprite.color = value ? new Color(0.35f, 0.35f, 0.35f, 1f) : Color.white;
        }
    }
}
