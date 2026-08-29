using Platformer.Core;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    public class WorkerDragController : MonoBehaviour
    {
        public Camera worldCamera;
        public LayerMask pickMask = ~0;
        public float stationSnapRadius = 2f;
        public float stationBoundsPadding = 0.75f;
        public WorkerPlacementConfirmUI confirmUI;

        SessionModel session;
        WorkerUnit activeWorker;
        WorkerUnit pendingWorker;
        WorkStation pendingStation;
        bool pointerHeld;
        bool awaitingConfirmation;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            if (worldCamera == null)
                worldCamera = Camera.main;
            if (confirmUI == null)
                confirmUI = FindAnyObjectByType<WorkerPlacementConfirmUI>();
            if (confirmUI == null)
            {
                var uiObject = new GameObject("WorkerPlacementConfirmUI");
                confirmUI = uiObject.AddComponent<WorkerPlacementConfirmUI>();
            }
        }

        void Update()
        {
            if (awaitingConfirmation)
                return;

            if (!CanInteract())
            {
                if (pointerHeld && activeWorker != null)
                    activeWorker.CancelDrag();
                pointerHeld = false;
                activeWorker = null;
                return;
            }

            if (TryGetPointerDown(out var screenPosition))
                BeginPick(screenPosition);

            if (pointerHeld && activeWorker != null)
            {
                if (TryGetPointerPosition(out screenPosition))
                    activeWorker.UpdateDragPosition(ScreenToWorld(screenPosition));
                if (TryGetPointerUp(out screenPosition))
                    EndPick(screenPosition);
            }
        }

        bool CanInteract()
        {
            return session != null
                && session.sessionStarted
                && session.round.phase == RoundPhase.Playing
                && session.round.dragEnabled
                && !session.eventState.awaitingDecision;
        }

        void BeginPick(Vector2 screenPosition)
        {
            var world = ScreenToWorld(screenPosition);
            var hit = Physics2D.OverlapPoint(world, pickMask);
            if (hit == null)
                return;

            var worker = hit.GetComponent<WorkerUnit>();
            if (worker == null || worker.state == WorkerState.Dragging)
                return;

            activeWorker = worker;
            pointerHeld = true;
            worker.BeginDrag();
        }

        void EndPick(Vector2 screenPosition)
        {
            pointerHeld = false;
            if (activeWorker == null)
                return;

            var pointerWorld = ScreenToWorld(screenPosition);
            var workerWorld = activeWorker.transform.position;
            var station = FindBestStation(activeWorker, pointerWorld)
                ?? FindBestStation(activeWorker, workerWorld);

            if (station != null)
            {
                pendingWorker = activeWorker;
                pendingStation = station;
                activeWorker = null;
                awaitingConfirmation = true;
                confirmUI.Show(pendingWorker, pendingStation, ConfirmPlacement, CancelPlacement);
                return;
            }

            activeWorker.CancelDrag();
            activeWorker = null;
        }

        void ConfirmPlacement()
        {
            if (pendingWorker != null && pendingStation != null)
            {
                if (!pendingStation.TryAssign(pendingWorker))
                    pendingWorker.CancelDrag();
            }

            ClearPending();
        }

        void CancelPlacement()
        {
            pendingWorker?.CancelDrag();
            ClearPending();
        }

        void ClearPending()
        {
            pendingWorker = null;
            pendingStation = null;
            awaitingConfirmation = false;
        }

        WorkStation FindBestStation(WorkerUnit worker, Vector3 worldPosition)
        {
            var stations = ResolveStations();
            if (stations == null || stations.Length == 0)
                return null;

            WorkStation nearest = null;
            var nearestDistance = float.MaxValue;

            for (var i = 0; i < stations.Length; i++)
            {
                var station = stations[i];
                if (station == null || !station.IsActive || !station.CanAccept(worker))
                    continue;

                if (StationContainsPoint(station, worldPosition))
                    return station;

                var distance = Vector2.Distance(worldPosition, station.transform.position);
                if (distance <= stationSnapRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = station;
                }
            }

            return nearest;
        }

        bool StationContainsPoint(WorkStation station, Vector3 worldPosition)
        {
            var collider = station.GetComponent<Collider2D>();
            if (collider == null)
                return false;

            var bounds = collider.bounds;
            bounds.Expand(stationBoundsPadding);
            return bounds.Contains(worldPosition);
        }

        WorkStation[] ResolveStations()
        {
            if (RoundController.Instance != null
                && RoundController.Instance.stations != null
                && RoundController.Instance.stations.Length > 0)
                return RoundController.Instance.stations;

            return FindObjectsByType<WorkStation>(FindObjectsSortMode.None);
        }

        Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            if (worldCamera == null)
                return Vector3.zero;

            var depth = Mathf.Abs(worldCamera.transform.position.z);
            var world = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            world.z = 0f;
            return world;
        }

        bool TryGetPointerPosition(out Vector2 screenPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        bool TryGetPointerDown(out Vector2 screenPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }

        bool TryGetPointerUp(out Vector2 screenPosition)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }
    }
}
