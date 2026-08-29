using Platformer.Model;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    [CustomEditor(typeof(WorkFloorLayout))]
    public class WorkFloorLayoutInspector : UnityEditor.Editor
    {
        void OnEnable()
        {
            SceneView.duringSceneGui += DrawSceneHandles;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= DrawSceneHandles;
        }

        void DrawSceneHandles(SceneView view)
        {
            if (target == null)
                return;

            var layout = (WorkFloorLayout)target;
            DrawBackgroundBounds(layout);
            DrawStationHandles(layout);
            DrawRosterHandles(layout);
        }

        static void DrawBackgroundBounds(WorkFloorLayout layout)
        {
            var min = layout.backgroundOrigin;
            var max = min + layout.worldSize;
            var corners = new[]
            {
                new Vector3(min.x, min.y, 0f),
                new Vector3(max.x, min.y, 0f),
                new Vector3(max.x, max.y, 0f),
                new Vector3(min.x, max.y, 0f),
                new Vector3(min.x, min.y, 0f)
            };

            Handles.color = new Color(0.3f, 0.8f, 1f, 0.8f);
            for (var i = 0; i < corners.Length - 1; i++)
                Handles.DrawLine(corners[i], corners[i + 1]);
        }

        void DrawStationHandles(WorkFloorLayout layout)
        {
            if (layout.stations == null)
                return;

            Undo.RecordObject(layout, "Move Work Station");

            for (var i = 0; i < layout.stations.Length; i++)
            {
                var station = layout.stations[i];
                var world = layout.NormalizedToWorld(station.normalizedPosition);
                var label = string.IsNullOrEmpty(station.displayLabel) ? station.stationId : station.displayLabel;

                EditorGUI.BeginChangeCheck();
                var moved = Handles.FreeMoveHandle(
                    world,
                    HandleUtility.GetHandleSize(world) * 0.08f,
                    Vector3.zero,
                    Handles.SphereHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    station.normalizedPosition = layout.WorldToNormalized(moved);
                    EditorUtility.SetDirty(layout);
                }

                Handles.Label(world + Vector3.up * 0.25f, label);
            }
        }

        void DrawRosterHandles(WorkFloorLayout layout)
        {
            if (layout.rosterSlots == null)
                return;

            Undo.RecordObject(layout, "Move Roster Slot");

            for (var i = 0; i < layout.rosterSlots.Length; i++)
            {
                var slot = layout.rosterSlots[i];
                var world = layout.NormalizedToWorld(slot.normalizedPosition);

                EditorGUI.BeginChangeCheck();
                var moved = Handles.FreeMoveHandle(
                    world,
                    HandleUtility.GetHandleSize(world) * 0.06f,
                    Vector3.zero,
                    Handles.CubeHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    slot.normalizedPosition = layout.WorldToNormalized(moved);
                    EditorUtility.SetDirty(layout);
                }

                Handles.Label(world + Vector3.up * 0.2f, slot.role.ToString());
            }
        }
    }
}
