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
        public WorkerAttributes attributes;
        public string displayName = "Worker";
        public FamilyMemberId familyMember = FamilyMemberId.None;

        Vector3 homePosition;
        WorkStation currentStation;
        SpriteRenderer spriteRenderer;
        Collider2D pickCollider;

        public WorkStation CurrentStation => currentStation;

        public void Initialize(WorkerRole workerRole, Vector3 rosterHome, string workerName = null)
        {
            role = workerRole;
            displayName = string.IsNullOrEmpty(workerName) ? workerRole.ToString() : workerName;
            familyMember = FamilyMemberRules.FromDisplayName(displayName);
            homePosition = rosterHome;
            attributes = WorkerAttributes.CreateRandom(workerRole);
            transform.position = rosterHome;
            ApplyVisual();
            ApplyRosterVisibility();
        }

        public string GetAttributeSummary()
        {
            return $"Build {attributes.builderSkill}  Study {attributes.analystSkill}  Active {attributes.courierSkill}  Happiness {attributes.happiness}";
        }

        public WorkerRole GetJobRoleForStation(WorkStation station)
        {
            if (station == null)
                return role;

            if (station.acceptAnyMember)
                return role;

            var allowed = station.GetAllowedMemberColors();
            if (allowed == WorkerColor.All)
                return role;

            if (allowed == WorkerColor.Orange)
                return WorkerRole.Builder;
            if (allowed == WorkerColor.Blue)
                return WorkerRole.Analyst;
            if (allowed == WorkerColor.Green)
                return WorkerRole.Courier;

            return station.requiredRole == WorkerRole.Any ? role : station.requiredRole;
        }

        public WorkerColor GetMemberColor()
        {
            return WorkerColorRules.FromRole(role);
        }

        public float GetEfficiencyForStation(WorkStation station)
        {
            return 1f;
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

        public void SyncRosterPickArea(Vector3 center, Vector2 worldSize)
        {
            if (state != WorkerState.InRoster)
                return;

            homePosition = center;
            transform.position = center;

            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var spriteSize = spriteRenderer.sprite.bounds.size;
                if (spriteSize.x > 0f && spriteSize.y > 0f)
                {
                    transform.localScale = new Vector3(
                        worldSize.x / spriteSize.x,
                        worldSize.y / spriteSize.y,
                        1f);
                }
            }

            if (pickCollider is BoxCollider2D box)
            {
                box.size = Vector2.one;
                box.offset = Vector2.zero;
            }
        }

        public bool ContainsPickPoint(Vector3 worldPoint)
        {
            if (state == WorkerState.InRoster)
            {
                if (pickCollider != null && pickCollider.enabled)
                    return pickCollider.OverlapPoint(worldPoint);
                return false;
            }

            if (spriteRenderer != null && spriteRenderer.enabled)
                return spriteRenderer.bounds.Contains(worldPoint);

            if (pickCollider != null && pickCollider.enabled)
                return pickCollider.OverlapPoint(worldPoint);

            return false;
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
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = 20;
            }
            transform.localScale = Vector3.one * 0.7f;
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
            {
                spriteRenderer.enabled = true;
                spriteRenderer.sortingOrder = 5;
            }
            transform.localScale = Vector3.one * 0.7f;

            var slotIndex = station.AssignedWorkers.Count - 1;
            var offset = GetSlotOffset(slotIndex);
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
            ApplyRosterVisibility();
        }

        public void SetPickEnabled(bool enabled)
        {
            if (pickCollider != null)
                pickCollider.enabled = enabled;
        }

        void ApplyRosterVisibility()
        {
            if (spriteRenderer == null)
                return;
            spriteRenderer.enabled = state != WorkerState.InRoster;
        }

        static Vector3 GetSlotOffset(int slotIndex)
        {
            return slotIndex switch
            {
                0 => new Vector3(-0.45f, 0.3f, 0f),
                1 => new Vector3(0.45f, 0.3f, 0f),
                2 => new Vector3(-0.45f, -0.3f, 0f),
                3 => new Vector3(0.45f, -0.3f, 0f),
                _ => new Vector3((slotIndex % 2) * 0.6f - 0.3f, (slotIndex / 2) * 0.5f, 0f)
            };
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
