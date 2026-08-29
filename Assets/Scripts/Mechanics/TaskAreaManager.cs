using Platformer.Model;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class TaskAreaManager : MonoBehaviour
    {
        public WorkStation[] taskStations;

        void Awake()
        {
            if (taskStations == null || taskStations.Length == 0)
                taskStations = GetComponentsInChildren<WorkStation>(true);
        }

        public void PrepareForRound()
        {
            if (taskStations == null)
                return;

            for (var i = 0; i < taskStations.Length; i++)
            {
                if (taskStations[i] != null)
                    taskStations[i].PrepareForRound();
            }
        }

        public void Tick(float deltaTime, float elapsedRoundTime)
        {
            if (taskStations == null)
                return;

            for (var i = 0; i < taskStations.Length; i++)
            {
                var station = taskStations[i];
                if (station == null || station.mode != WorkStationMode.TimedTask)
                    continue;

                station.TickSpawn(elapsedRoundTime);
                station.TickTask(deltaTime);
            }
        }

        public float GetRoundTimeBonus()
        {
            if (taskStations == null)
                return 0f;

            var bonus = 0f;
            for (var i = 0; i < taskStations.Length; i++)
            {
                if (taskStations[i] != null)
                    bonus += taskStations[i].ActiveTaskRoundTimeBonus;
            }
            return bonus;
        }
    }
}
