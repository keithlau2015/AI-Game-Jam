using Platformer.Core;
using Platformer.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    public class WorkerDragController : MonoBehaviour
    {
        public Camera worldCamera;
        public LayerMask pickMask = ~0;

        SessionModel session;
        WorkerUnit activeWorker;
        bool pointerHeld;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            if (worldCamera == null)
                worldCamera = Camera.main;
        }

        void Update()
        {
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

            var world = ScreenToWorld(screenPosition);
            var station = FindStationAt(world);

            if (station != null && station.TryAssign(activeWorker))
            {
                activeWorker = null;
                return;
            }

            activeWorker.CancelDrag();
            activeWorker = null;
        }

        WorkStation FindStationAt(Vector3 worldPosition)
        {
            if (RoundController.Instance == null || RoundController.Instance.stations == null)
                return null;

            var stations = RoundController.Instance.stations;
            for (var i = 0; i < stations.Length; i++)
            {
                var station = stations[i];
                if (station == null)
                    continue;
                var collider = station.GetComponent<Collider2D>();
                if (collider != null && collider.OverlapPoint(worldPosition))
                    return station;
            }

            return null;
        }

        Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            var depth = worldCamera != null ? Mathf.Abs(worldCamera.transform.position.z) : 10f;
            var world = worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depth));
            world.z = 0f;
            return world;
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
