using System.Collections.Generic;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class RoundController : MonoBehaviour
    {
        public static RoundController Instance { get; private set; }

        public WorkStation[] stations;

        SessionModel session;
        float productionBuffer;
        TaskAreaManager taskAreaManager;
        PopupTaskSpawner popupTaskSpawner;
        readonly List<WorkStation> dynamicStations = new List<WorkStation>();

        void Awake()
        {
            Instance = this;
            session = Simulation.GetModel<SessionModel>();
            taskAreaManager = GetComponent<TaskAreaManager>();
            if (taskAreaManager == null)
                taskAreaManager = gameObject.AddComponent<TaskAreaManager>();
            popupTaskSpawner = GetComponent<PopupTaskSpawner>();
            if (stations == null || stations.Length == 0)
                stations = GetComponentsInChildren<WorkStation>(true);
            SyncTaskStations();
        }

        void SyncTaskStations()
        {
            if (taskAreaManager == null)
                return;

            if (stations == null || stations.Length == 0)
            {
                taskAreaManager.taskStations = dynamicStations.ToArray();
                return;
            }

            if (dynamicStations.Count == 0)
            {
                taskAreaManager.taskStations = stations;
                return;
            }

            var merged = new List<WorkStation>(stations.Length + dynamicStations.Count);
            merged.AddRange(stations);
            merged.AddRange(dynamicStations);
            taskAreaManager.taskStations = merged.ToArray();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (session == null)
                return;
            if (session.round.phase == RoundPhase.Won
                || session.round.phase == RoundPhase.Lost
                || session.round.phase == RoundPhase.Idle)
                return;

            SessionEndChecker.CheckAndSchedule(session);
            if (session.round.phase != RoundPhase.Playing)
                return;
            if (session.eventState.awaitingDecision)
                return;

            var deltaTime = Time.deltaTime;
            var elapsed = session.round.timeLimit - session.round.timeRemaining;
            if (taskAreaManager != null)
                taskAreaManager.Tick(deltaTime, elapsed);
            if (popupTaskSpawner != null)
                popupTaskSpawner.Tick(deltaTime, elapsed);

            var timeBonus = taskAreaManager != null ? taskAreaManager.GetRoundTimeBonus() : 0f;
            session.round.timeRemaining -= deltaTime * (1f + timeBonus);
            if (session.round.timeRemaining <= 0f)
            {
                session.round.timeRemaining = 0f;
                session.lastEndReason = RoundEndReason.DaySurvived;
                Schedule<RoundWon>();
                return;
            }

            TickProduction(deltaTime);
        }

        void TickProduction(float deltaTime)
        {
            if (stations == null)
                return;

            var rate = 0f;
            for (var i = 0; i < stations.Length; i++)
            {
                if (stations[i] != null)
                    rate += stations[i].GetOutputPerSecond();
            }

            rate *= session.round.globalProductionMultiplier;
            productionBuffer += rate * deltaTime;
            if (productionBuffer < 1f)
                return;

            var gained = Mathf.FloorToInt(productionBuffer);
            productionBuffer -= gained;
            session.round.currentOutput += gained;
        }

        public void StartRound()
        {
            session.round.timeRemaining = session.round.timeLimit;
            session.round.currentOutput = 0;
            session.round.phase = RoundPhase.Playing;
            session.round.dragEnabled = true;
            session.sessionStarted = true;
            productionBuffer = 0f;
            dynamicStations.Clear();
            if (popupTaskSpawner != null)
                popupTaskSpawner.PrepareForRound();
            if (taskAreaManager != null)
                taskAreaManager.PrepareForRound();
            else
            {
                for (var i = 0; i < stations.Length; i++)
                {
                    if (stations[i] != null)
                        stations[i].PrepareForRound();
                }
            }
        }

        public void AddOutput(int amount)
        {
            if (amount <= 0)
                return;
            session.round.currentOutput += amount;
        }

        public void SetPausedForEvent(bool paused)
        {
            if (session.round.phase == RoundPhase.Won || session.round.phase == RoundPhase.Lost)
                return;

            session.round.phase = paused ? RoundPhase.PausedForEvent : RoundPhase.Playing;
            session.round.dragEnabled = !paused;
        }

        public void AddTime(float seconds)
        {
            session.round.timeRemaining += seconds;
        }

        public void ModifyProductionMultiplier(float delta)
        {
            session.round.globalProductionMultiplier = Mathf.Max(0.1f, session.round.globalProductionMultiplier + delta);
        }

        public void DisableRandomStation()
        {
            if (stations == null || stations.Length == 0)
                return;

            var candidates = new List<WorkStation>();
            for (var i = 0; i < stations.Length; i++)
            {
                if (stations[i] != null && stations[i].IsActive && !stations[i].disabled)
                    candidates.Add(stations[i]);
            }

            if (candidates.Count == 0)
                return;

            candidates[Random.Range(0, candidates.Count)].SetDisabled(true);
        }

        public void RegisterStation(WorkStation station)
        {
            if (station == null || dynamicStations.Contains(station))
                return;
            dynamicStations.Add(station);
            SyncTaskStations();
        }

        public void UnregisterStation(WorkStation station)
        {
            if (station == null)
                return;
            dynamicStations.Remove(station);
            SyncTaskStations();
        }

        public WorkStation[] GetAllStations()
        {
            if (dynamicStations.Count == 0)
                return stations;

            var merged = new List<WorkStation>();
            if (stations != null)
                merged.AddRange(stations);
            merged.AddRange(dynamicStations);
            return merged.ToArray();
        }
    }
}
